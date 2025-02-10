using System.Web;
using LucidStandardImport.Auth;
using Newtonsoft.Json;

public class LucidAuthorizationTools
{
    private const string lucidAuthUrl = "https://lucid.app/oauth2/authorize";
    private const string tokenUrl = "https://api.lucid.co/oauth2/token";

    public static string BuildAuthorizationUrl(LucidOAuthConfig config)
    {
        // Build the OAuth authorization URL
        var uriBuilder = new UriBuilder(lucidAuthUrl);

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["response_type"] = "code";
        query["client_id"] = config.ClientId;
        query["scope"] = "lucidchart.document.app"; // This is the scope needed for Lucid standard import
        query["redirect_uri"] = config.RedirectUri;
        // Optionally add PKCE or 'state' if needed

        uriBuilder.Query = query.ToString();
        var authorizationUrl = uriBuilder.ToString();
        return authorizationUrl;
    }

    /// <summary>
    /// Exchanges the authorization code for an OAuth token using Lucid's token endpoint.
    /// </summary>
    public static async Task<LucidToken> ExchangeCodeForTokenAsync(
        LucidOAuthConfig config,
        string code
    )
    {
        try
        {
            using var client = new HttpClient();

            // Build form parameters
            var parameters = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "client_id", config.ClientId },
                { "client_secret", config.ClientSecret },
                // Must match the exact redirect URI used in the OAuth flow:
                { "redirect_uri", config.RedirectUri },
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync(tokenUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Token request failed with status: {response.StatusCode}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var rawData = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            if (rawData == null)
            {
                throw new InvalidOperationException(
                    "Failed to deserialize JSON into a dictionary."
                );
            }

            var accessToken2 = rawData["access_token"];

            var token = new LucidToken
            {
                AccessToken =
                    rawData.TryGetValue("access_token", out var accessToken)
                    && accessToken is string tokenValue
                        ? tokenValue
                        : throw new InvalidOperationException(
                            "Missing or invalid 'access_token' in response."
                        ),

                TokenType =
                    rawData.TryGetValue("token_type", out var tt) && tt is string ttStr
                        ? ttStr
                        : throw new InvalidOperationException(
                            "Missing or invalid 'token_type' in response."
                        ),

                RefreshToken =
                    rawData.TryGetValue("refresh_token", out var rt) && rt is string rtStr
                        ? rtStr
                        : null,

                Scope =
                    rawData.TryGetValue("scope", out var sc) && sc is string scStr ? scStr : null,
            };

            // If the response includes "expires_in" (seconds), convert to an absolute Unix time.
            if (
                rawData.TryGetValue("expires_in", out var ei)
                && long.TryParse(ei.ToString(), out var expiresIn)
            )
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                token.ExpiresAt = now + expiresIn;
            }
            else if (
                rawData.TryGetValue("expires_at", out var ea)
                && long.TryParse(ea.ToString(), out var eaVal)
            )
            {
                token.ExpiresAt = eaVal;
            }

            return token;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error exchanging code for token: {ex.Message}");
            return null;
        }
    }
}
