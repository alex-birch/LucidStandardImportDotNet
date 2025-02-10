using System.Threading.Tasks;

namespace LucidStandardImport.Auth
{
    public interface ILucidOAuthProvider
    {
        /// <summary>
        /// Creates or retrieves a valid Lucid session, handling OAuth if necessary.
        /// </summary>
        /// <param name="tokenPath">File path where OAuth tokens are stored/read.</param>
        /// <returns>A LucidSession containing token info</returns>
        Task<LucidSession> CreateLucidSessionAsync();
    }
}
