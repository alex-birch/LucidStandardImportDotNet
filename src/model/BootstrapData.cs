namespace LucidStandardImport.model
{
        public class BootstrapData
    {
        public string PackageId { get; set; }
        public string ExtensionName { get; set; }
        public string MinimumVersion { get; set; }
        public Dictionary<string, string> Data { get; set; }
    }
}