namespace LucidStandardImport.model
{
    public class Action
    {
        public string Type { get; set; } = null!;
        public string DocumentId { get; set; } = null!;
        public string PageId { get; set; } = null!;
        public bool NewWindow { get; set; }
    }
}
