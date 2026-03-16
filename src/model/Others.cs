using System.Text.Json.Serialization;
using LucidStandardImport.model;

namespace LucidStandardImport
{
    public class Collection
    {
        public string Id { get; set; } = null!;
        public string DataSource { get; set; } = null!; // Typically references a CSV file
    }

    public class Endpoint
    {
        /// <summary>
        /// Shape endpoint
        /// </summary>
        public Endpoint(string externalId, EndpointStyle style = EndpointStyle.none)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(externalId);
            ExternalId = externalId;
            Type = EndpointType.shapeEndpoint;
            Position = new(0.5, 0.5);
            Style = style;
        }

        /// <summary>
        /// Shape endpoint with custom relative anchor point.
        /// </summary>
        public Endpoint(string externalId, double relativeX, double relativeY, EndpointStyle style = EndpointStyle.none)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(externalId);
            ExternalId = externalId;
            Type = EndpointType.shapeEndpoint;
            Position = new(relativeX, relativeY);
            Style = style;
        }

        /// <summary>
        /// Absolute positioned endpoint
        /// </summary>
        public Endpoint(double positionX, double positionY, EndpointStyle style = EndpointStyle.none)
        {
            Type = EndpointType.positionEndpoint;
            Position = new(positionX, positionY);
            Style = style;
        }

        public string ShapeId { get; set; } = null!;

        [JsonIgnore]
        public string ExternalId { get; set; } = null!;

        /// <summary>
        /// ShapeEndpoint, RelativePosition -- A relative position specifying where on the target shape this endpoint should attach.
        /// PositionEndpoint, Position Endpoint -- An endpoint that is positioned somewhere on the canvas independent of a shape or line.
        /// </summary>
        public Position Position { get; set; }
        public EndpointType Type { get; set; }
        public EndpointStyle Style { get; set; } = EndpointStyle.none;
    }

    public class LinkedData
    {
        public string CollectionId { get; set; } = null!;
        public string Key { get; set; } = null!;
    }

    public class LineText
    {
        public string Text { get; set; } = null!;
        public double Position { get; set; } // Relative position on the line (0.0 to 1.0)
        public LineSide Side { get; set; } // Top, middle, or bottom
    }

    public record Position(double X, double Y);

    public class Fill
    {
        public string Type { get; set; } = null!; // "color" or "image"
        public string Color { get; set; } = null!; // For color fills
        public string Ref { get; set; } = null!; // For image fills, references the image file
        public string ImageScale { get; set; } = null!; // For image fills, e.g., "fit", "stretch"
    }

    public enum LineSide
    {
        Top,
        Middle,
        Bottom,
    }

    public class Color
    {
        public string HexCode { get; set; } = null!; // E.g., "#FFFFFF"
    }

    public class FieldSize
    {
        public int Index { get; set; }
        public double Size { get; set; }
    }
}
