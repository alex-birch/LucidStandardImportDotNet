namespace LucidStandardImport.model
{
    public class Group : IIdentifiableLucidObject
    {
        public string Id { get; set; }
        public List<string> Items { get; set; } // References IDs of shapes, lines, or other groups
        public string Note { get; set; }
        public List<CustomData> CustomData { get; set; }
        public List<LinkedData> LinkedData { get; set; }
    }
}