using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;
using LucidStandardImport.Auth;

public class ConsolePromptingOAuthFlow : IOAuthFlow
{
    public async Task<string> GetOAuthCodeAsync(LucidOAuthConfig config, string authorizationUrl)
    {
        // Instruct the user to open the URL in their browser
        Console.WriteLine("Please open the following URL in your browser to authorize:");
        Console.WriteLine(authorizationUrl);
        Console.WriteLine();
        Console.WriteLine(
            "After authorizing, you will be redirected to your configured redirect URI."
        );
        Console.WriteLine(
            "Look for the 'code' parameter in the browser's address bar, copy it, then paste it here."
        );
        Console.WriteLine();

        // Prompt the user to paste the code
        Console.Write("Paste the authorization code here: ");
        var code = await Task.Run(() => Console.ReadLine() ?? "");

        return code;
    }
}

public class LocalWebServerOAuthFlow : IOAuthFlow
{
    public async Task<string> GetOAuthCodeAsync(LucidOAuthConfig config, string authorizationUrl)
    {
        var redirectUri = config.RedirectUri.EndsWith("/") ? config.RedirectUri : config.RedirectUri + "/";

        // Spin up HttpListener on the ephemeral port
        using var listener = new HttpListener();
        listener.Prefixes.Add(redirectUri);
        listener.Start();

        // Attempt to open the user's browser
        try
        {
            var psi = new ProcessStartInfo { FileName = authorizationUrl, UseShellExecute = true };
            Process.Start(psi);
        }
        catch
        {
            Console.WriteLine("Unable to open the browser automatically. Please open:");
            Console.WriteLine(authorizationUrl);
        }

        // Wait for the OAuth callback
        var tcs = new TaskCompletionSource<string>();
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5)); // Timeout after 5 minutes

        _ = Task.Run(
            async () =>
            {
                try
                {
                    while (!cts.IsCancellationRequested)
                    {
                        var context = await listener.GetContextAsync(); // blocks until request
                        var request = context.Request;
                        var response = context.Response;

                        if (request.Url != null && request.Url.AbsolutePath == "/")
                        {
                            // Check if we got 'code'
                            var codeParam = HttpUtility
                                .ParseQueryString(request.Url.Query)
                                .Get("code");
                            if (!string.IsNullOrEmpty(codeParam))
                            {
                                // We have the OAuth authorization code
                                tcs.TrySetResult(codeParam);

                                // Return a response to the browser
                                var responseString =
                                    "<html><body>Authentication successful. You can close this window.</body></html>";
                                var buffer = Encoding.UTF8.GetBytes(responseString);
                                response.ContentLength64 = buffer.Length;
                                using var output = response.OutputStream;
                                output.Write(buffer, 0, buffer.Length);

                                response.Close();
                                break;
                            }
                        }

                        // If no code, respond with a basic message
                        response.StatusCode = 200;
                        var defaultResponse =
                            "<html><body>Awaiting OAuth callback...</body></html>";
                        var defaultBuffer = Encoding.UTF8.GetBytes(defaultResponse);
                        response.ContentLength64 = defaultBuffer.Length;
                        using (var output = response.OutputStream)
                        {
                            output.Write(defaultBuffer, 0, defaultBuffer.Length);
                        }
                        response.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Listener exception: {ex.Message}");
                    tcs.TrySetResult(null);
                }
                finally
                {
                    listener.Stop();
                }
            },
            cts.Token
        );

        // Wait for either the code or a timeout
        var completed = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, cts.Token));
        var authCode = (completed == tcs.Task) ? tcs.Task.Result : null;

        return authCode;
    }
}

public class LocalAuthProvider : LucidOAuthProvider
{
    public LocalAuthProvider(LucidOAuthConfig oAuthConfig, string tokenPath)
        : base(oAuthConfig, new LocalFileTokenStorage(tokenPath), new LocalWebServerOAuthFlow()) { }
}

public class ConsoleAuthProvider : LucidOAuthProvider
{
    public ConsoleAuthProvider(LucidOAuthConfig oAuthConfig)
        : base(oAuthConfig, new LocalFileTokenStorage("./"), new ConsolePromptingOAuthFlow()) { }
}

public class LocalFileTokenStorage : ILucidTokenStorage
{
    private string _localTokenPath;

    public LocalFileTokenStorage(string localTokenPath)
    {
        _localTokenPath = localTokenPath;
    }

    public async Task<LucidToken?> LoadTokenAsync()
    {
        try
        {
            if (File.Exists(_localTokenPath))
            {
                var json = await File.ReadAllTextAsync(_localTokenPath);
                var token = JsonSerializer.Deserialize<LucidToken>(json);
                return token;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading token: " + ex.Message);
        }
        return null;
    }

    public async Task SaveTokenAsync(LucidToken token)
    {
        try
        {
            var json = JsonSerializer.Serialize(token);
            await File.WriteAllTextAsync(Path.Combine(_localTokenPath, "lucidToken.json"), json);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error saving token: " + ex.Message);
        }
    }
}
