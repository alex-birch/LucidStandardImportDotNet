namespace LucidStandardImport.model
{
    public class Stroke
    {
        public string Color { get; set; } // Hexadecimal color representation
        public int Width { get; set; } // Width in pixels
        public StrokeStyle Style { get; set; }
    }
}