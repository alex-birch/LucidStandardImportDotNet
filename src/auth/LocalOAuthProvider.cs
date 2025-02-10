// using System.Diagnostics;
// using System.Net;
// using System.Net.Sockets;
// using System.Text;
// using System.Text.Json;
// using System.Web;

// namespace LucidStandardImport.Auth
// {
//     public class LocalOAuthProvider : ILucidOAuthProvider
//     {
//         private readonly LucidOAuthConfig _config;
//         private readonly string _localTokenPath;

//         public LocalOAuthProvider(LucidOAuthConfig config, string localTokenPath)
//         {
//             _config = config;
//             _localTokenPath = localTokenPath;
//         }

//         /// <summary>
//         /// Main entry point: checks for existing token, if invalid or missing starts OAuth flow.
//         /// </summary>
//         public async Task<LucidSession> CreateLucidSessionAsync()
//         {
//             // 1) Try loading an existing token
//             var existingToken = LoadTokenLocally();
//             if (existingToken != null)
//             {
//                 var existingSession = new LucidSession { Token = existingToken };
//                 if (existingSession.IsTokenValid())
//                 {
//                     // Token is valid - return as-is
//                     return existingSession;
//                 }
//             }

//             // 2) No valid token. Start OAuth flow with ephemeral port
//             var code = await StartOAuthFlowAsync();
//             if (string.IsNullOrEmpty(code))
//             {
//                 // If user canceled or some error occurred, return null
//                 return null;
//             }

//             // 3) Exchange the code for a token, using the same ephemeral redirect URI
//             var newToken = await ExchangeCodeForTokenAsync(code);
//             if (newToken == null)
//             {
//                 return null;
//             }

//             // 4) Save token to disk
//             SaveTokenLocally(newToken);

//             // 5) Return the new session
//             return new LucidSession { Token = newToken };
//         }

//         /// <summary>
//         /// Determine a free ephemeral port. The system picks an available port in ephemeral range.
//         /// </summary>
//         private int GetEphemeralPort()
//         {
//             // We create a TcpListener on port 0, then retrieve the port that the OS assigned.
//             var listener = new TcpListener(IPAddress.Loopback, 0);
//             listener.Start();
//             int port = ((IPEndPoint)listener.LocalEndpoint).Port;
//             listener.Stop();
//             return port;
//         }

//         private const string tokenUrl = "https://api.lucid.co/oauth2/token";

//         /// <summary>
//         /// Spins up a local HTTP listener on an ephemeral port, constructs the redirect URI,
//         /// opens the user’s browser, and waits for the "code" callback.
//         /// </summary>
//         /// <returns>A tuple of (authCode, ephemeralRedirectUri) or (null, ephemeralRedirectUri) on failure.</returns>
//         private async Task<string> StartOAuthFlowAsync()
//         {
//             string authorizationUrl = LucidAuthorizationTools.BuildAuthorizationUrl(_config);

//             // Spin up HttpListener on the ephemeral port
//             using var listener = new HttpListener();
//             listener.Prefixes.Add(_config.RedirectUri);
//             listener.Start();

//             // Attempt to open the user's browser
//             try
//             {
//                 var psi = new ProcessStartInfo
//                 {
//                     FileName = authorizationUrl,
//                     UseShellExecute = true,
//                 };
//                 Process.Start(psi);
//             }
//             catch
//             {
//                 Console.WriteLine("Unable to open the browser automatically. Please open:");
//                 Console.WriteLine(authorizationUrl);
//             }

//             // Wait for the OAuth callback
//             var tcs = new TaskCompletionSource<string>();
//             var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5)); // Timeout after 5 minutes

//             _ = Task.Run(
//                 async () =>
//                 {
//                     try
//                     {
//                         while (!cts.IsCancellationRequested)
//                         {
//                             var context = await listener.GetContextAsync(); // blocks until request
//                             var request = context.Request;
//                             var response = context.Response;

//                             if (request.Url != null && request.Url.AbsolutePath == "/")
//                             {
//                                 // Check if we got 'code'
//                                 var codeParam = HttpUtility
//                                     .ParseQueryString(request.Url.Query)
//                                     .Get("code");
//                                 if (!string.IsNullOrEmpty(codeParam))
//                                 {
//                                     // We have the OAuth authorization code
//                                     tcs.TrySetResult(codeParam);

//                                     // Return a response to the browser
//                                     var responseString =
//                                         "<html><body>Authentication successful. You can close this window.</body></html>";
//                                     var buffer = Encoding.UTF8.GetBytes(responseString);
//                                     response.ContentLength64 = buffer.Length;
//                                     using var output = response.OutputStream;
//                                     output.Write(buffer, 0, buffer.Length);

//                                     response.Close();
//                                     break;
//                                 }
//                             }

//                             // If no code, respond with a basic message
//                             response.StatusCode = 200;
//                             var defaultResponse =
//                                 "<html><body>Awaiting OAuth callback...</body></html>";
//                             var defaultBuffer = Encoding.UTF8.GetBytes(defaultResponse);
//                             response.ContentLength64 = defaultBuffer.Length;
//                             using (var output = response.OutputStream)
//                             {
//                                 output.Write(defaultBuffer, 0, defaultBuffer.Length);
//                             }
//                             response.Close();
//                         }
//                     }
//                     catch (Exception ex)
//                     {
//                         Console.WriteLine($"Listener exception: {ex.Message}");
//                         tcs.TrySetResult(null);
//                     }
//                     finally
//                     {
//                         listener.Stop();
//                     }
//                 },
//                 cts.Token
//             );

//             // Wait for either the code or a timeout
//             var completed = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, cts.Token));
//             var authCode = (completed == tcs.Task) ? tcs.Task.Result : null;

//             return authCode;
//         }

//         /// <summary>
//         /// Exchanges the authorization code for an OAuth token using the config's TokenUrl endpoint.
//         /// Lucid requires that redirect_uri match exactly, so we include ephemeralRedirectUri.
//         /// </summary>
//         private async Task<LucidToken> ExchangeCodeForTokenAsync(string code)
//         {
//             try
//             {
//                 using var client = new HttpClient();

//                 // Build form parameters
//                 var parameters = new Dictionary<string, string>
//                 {
//                     { "grant_type", "authorization_code" },
//                     { "code", code },
//                     { "client_id", _config.ClientId },
//                     { "client_secret", _config.ClientSecret },
//                     // IMPORTANT: Lucid requires an exact match for the redirect_uri:
//                     { "redirect_uri", _config.RedirectUri },
//                 };

//                 var content = new FormUrlEncodedContent(parameters);
//                 var response = await client.PostAsync(tokenUrl, content);

//                 if (!response.IsSuccessStatusCode)
//                 {
//                     Console.WriteLine($"Token request failed with status: {response.StatusCode}");
//                     return null;
//                 }

//                 var json = await response.Content.ReadAsStringAsync();
//                 var rawData = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

//                 // Convert the result to LucidToken
//                 var token = new LucidToken
//                 {
//                     AccessToken = rawData.TryGetValue("access_token", out var at)
//                         ? at.ToString()
//                         : null,
//                     TokenType = rawData.TryGetValue("token_type", out var tt)
//                         ? tt.ToString()
//                         : null,
//                     RefreshToken = rawData.TryGetValue("refresh_token", out var rt)
//                         ? rt.ToString()
//                         : null,
//                     Scope = rawData.TryGetValue("scope", out var sc) ? sc.ToString() : null,
//                 };

//                 // Often the response has "expires_in" (seconds); convert to an absolute Unix time
//                 if (
//                     rawData.TryGetValue("expires_in", out var ei)
//                     && long.TryParse(ei.ToString(), out var expiresIn)
//                 )
//                 {
//                     var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
//                     token.ExpiresAt = now + expiresIn;
//                 }
//                 else if (
//                     rawData.TryGetValue("expires_at", out var ea)
//                     && long.TryParse(ea.ToString(), out var eaVal)
//                 )
//                 {
//                     token.ExpiresAt = eaVal;
//                 }

//                 return token;
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error exchanging code for token: {ex.Message}");
//                 return null;
//             }
//         }

//         /// <summary>
//         /// Loads the token from disk if it exists.
//         /// </summary>
//         private LucidToken LoadTokenLocally()
//         {
//             try
//             {
//                 if (File.Exists(_localTokenPath))
//                 {
//                     var json = File.ReadAllText(_localTokenPath);
//                     var token = JsonSerializer.Deserialize<LucidToken>(json);
//                     return token;
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("Error loading token: " + ex.Message);
//             }
//             return null;
//         }

//         /// <summary>
//         /// Saves the token to disk as JSON.
//         /// </summary>
//         private void SaveTokenLocally(LucidToken token)
//         {
//             try
//             {
//                 var json = JsonSerializer.Serialize(token);
//                 File.WriteAllText(_localTokenPath, json);
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("Error saving token: " + ex.Message);
//             }
//         }
//     }
// }
