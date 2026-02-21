namespace LucidStandardImport.model
{
    public class PageSettings
    {
        public Color FillColor { get; set; } = null!;
        public bool InfiniteCanvas { get; set; }
        public bool AutoTiling { get; set; }
        public PageSize Size { get; set; } = null!;
    }
}
