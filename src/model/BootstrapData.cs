namespace LucidStandardImport.model
{
    public class BootstrapData
    {
        public string PackageId { get; set; } = null!;
        public string ExtensionName { get; set; } = null!;
        public string MinimumVersion { get; set; } = null!;
        public Dictionary<string, string> Data { get; set; } = null!;
    }
}
