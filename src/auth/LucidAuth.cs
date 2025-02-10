namespace LucidStandardImport.Auth
{
    /// <summary>
    /// Basic config needed for Lucid OAuth flows.
    /// </summary>
    public class LucidOAuthConfig
    {
        public string ClientId { get; set; } 
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
        // public string Scope { get; set; }
    }

    /// <summary>
    /// Represents the OAuth token data returned by Lucid. 
    /// </summary>
    public class LucidToken
    {
        public required string? AccessToken { get; set; }
        public required string TokenType { get; set; }
        public string? RefreshToken { get; set; }
        public long ExpiresAt { get; set; }
        public string? Scope { get; set; }
    }

    /// <summary>
    /// Represents a "session" in your application, wrapping token data or anything else you need.
    /// </summary>
    public class LucidSession
    {
        public LucidToken Token { get; set; }

        public bool IsTokenValid(int maxSecondsBeforeExpiration = 1)
        {
            // Check if current time < ExpiresAt
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return Token != null && now + maxSecondsBeforeExpiration < Token.ExpiresAt;
        }
    }
}
