namespace LucidStandardImport.model
{
    public class Stroke
    {
        public string Color { get; set; } // Hexadecimal color representation
        public int Width { get; set; } = 0; // Width in pixels
        public StrokeStyle Style { get; set; } = StrokeStyle.solid;
    }
}
