using System.Text.Json.Serialization;
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
        /// <summary>
        /// Shape endpoint
        /// </summary>
        public Endpoint(string externalId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(externalId);
            ExternalId = externalId;
            Type = EndpointType.shapeEndpoint;
            Position = new(0.5, 0.5);
        }

        /// <summary>
        /// Absolute positioned endpoint
        /// </summary>
        public Endpoint(double positionX, double positionY)
        {
            Type = EndpointType.positionEndpoint;
            Position = new(positionX, positionY);
        }

        public string ShapeId { get; set; }

        [JsonIgnore]
        public string ExternalId { get; set; }

        /// <summary>
        /// ShapeEndpoint, RelativePosition -- A relative position specifying where on the target shape this endpoint should attach.
        /// PositionEndpoint, Position Endpoint -- An endpoint that is positioned somewhere on the canvas independent of a shape or line.
        /// </summary>
        public Position Position { get; set; }
        public EndpointType Type { get; set; }
        public string Style { get; set; } = "none";
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

    public record Position(double X, double Y);

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
