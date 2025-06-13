namespace LucidStandardImport.Auth;

public class LucidOAuthProvider : ILucidOAuthProvider
{
    private readonly LucidOAuthConfig _config;
    private ILucidTokenStorage _tokenStorage;
    private IOAuthFlow _oAuthFlow;

    public LucidOAuthProvider(
        LucidOAuthConfig config,
        ILucidTokenStorage tokenStorage,
        IOAuthFlow oAuthFlow
    )
    {
        _config = config;
        _tokenStorage = tokenStorage;
        _oAuthFlow = oAuthFlow;
    }

    /// <summary>
    /// Main entry point: checks for existing token, if invalid or missing, starts OAuth flow.
    /// </summary>
    public async Task<LucidSession> CreateLucidSessionAsync()
    {
        // 1) Try loading an existing token
        var existingToken = await _tokenStorage.LoadTokenAsync(null);
        if (existingToken != null)
        {
            var existingSession = new LucidSession { Token = existingToken };
            if (existingSession.IsTokenValid())
            {
                // Token is valid - return as-is
                return existingSession;
            }
        }

        // 2) No valid token. Prompt the user to complete the OAuth flow manually.
        var code = await StartOAuthFlowAsync();
        // If user canceled or some error occurred, return null
        if (string.IsNullOrEmpty(code))
            throw new InvalidOperationException(
                "User canceled or an error occurred during the oauth flow"
            );

        // 3) Exchange the code for a token
        var newToken = await LucidAuthorizationTools.ExchangeCodeForTokenAsync(_config, code);
        if (newToken == null)
            throw new InvalidOperationException(
                "An error  occurred while exchanging the code for a token"
            );

        // 4) Save token to disk
        await _tokenStorage.SaveTokenAsync(newToken, null);

        // 5) Return the new session
        return new LucidSession { Token = newToken };
    }

    /// <summary>
    /// Builds the authorization URL, asks the user to open it manually in a browser,
    /// complete the OAuth flow, and paste the resulting authorization code.
    /// </summary>
    private async Task<string> StartOAuthFlowAsync()
    {
        string authorizationUrl = LucidAuthorizationTools.BuildAuthorizationUrl(_config);

        return await _oAuthFlow.GetOAuthCodeAsync(_config, authorizationUrl);
    }
}

public class UserContext
{
    public required string UserId { get; set; }
}

public interface ILucidTokenStorage
{
    Task<LucidToken> LoadTokenAsync(string key);
    Task SaveTokenAsync(LucidToken token, string key);
}

public interface IOAuthFlow
{
    Task<string> GetOAuthCodeAsync(LucidOAuthConfig config, string authorizationUrl);
}
