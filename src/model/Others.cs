using LucidStandardImport.model;

namespace LucidStandardImport
{
    public class Collection
    {
        public string Id { get; set; }
        public string DataSource { get; set; } // Typically references a CSV file
    }

    public class Endpoint
    {
        public EndpointType Type { get; set; }
        public string ShapeId { get; set; }
        public RelativePosition Position { get; set; }
    }

    public class LinkedData
    {
        public string CollectionId { get; set; }
        public string Key { get; set; }
    }

    public class LineText
    {
        public string Text { get; set; }
        public double Position { get; set; } // Relative position on the line (0.0 to 1.0)
        public LineSide Side { get; set; } // Top, middle, or bottom
    }

    public class RelativePosition
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class Fill
    {
        public string Type { get; set; } // "color" or "image"
        public string Color { get; set; } // For color fills
        public string Ref { get; set; } // For image fills, references the image file
        public string ImageScale { get; set; } // For image fills, e.g., "fit", "stretch"
    }

    public enum LineSide
    {
        Top,
        Middle,
        Bottom,
    }

    public class Color
    {
        public string HexCode { get; set; } // E.g., "#FFFFFF"
    }
}
