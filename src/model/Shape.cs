using Newtonsoft.Json;
using SixLabors.ImageSharp;

namespace LucidStandardImport.model
{
    public abstract class Shape : IIdentifiableLucidObject
    {
        public string Id { get; set; }
        public ShapeType Type { get; protected set; }
        public BoundingBox BoundingBox { get; set; }
        public Style Style { get; set; }
        public string Text { get; set; }
        private int? _opacity;

        /// <summary>
        /// Must be between 0 and 100 if set
        /// </summary>
        public int? Opacity
        {
            get => _opacity;
            set
            {
                if (value is < 0 or > 100)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(Opacity),
                        "Opacity must be between 0 and 100."
                    );
                }
                _opacity = value;
            }
        }
        public string Note { get; set; }
        public List<Action> Actions { get; set; } = null;
        public List<CustomData> CustomData { get; set; }
        public List<LinkedData> LinkedData { get; set; }
    }

    public class ImageShape : Shape
    {
        public ImageShape()
        {
            Type = ShapeType.Image;
        }

        public required ImageFill Image { get; set; }
        public required Stroke Stroke { get; set; }
    }

    public class RectangleShape : Shape
    {
        public RectangleShape()
        {
            Type = ShapeType.Rectangle;
            Text = ""; // Set to empty string or lucid will show 'text'
        }
    }

    public class CircleShape : Shape
    {
        public CircleShape()
        {
            Type = ShapeType.Circle;
        }
    }

    public class LineShape : Shape
    {
        public LineShape()
        {
            Type = ShapeType.Line;
        }
    }

    public class TextShape : Shape
    {
        public TextShape()
        {
            Type = ShapeType.Text;
        }
    }

    public class ImageFill : IIdentifiableLucidObject
    {
        [JsonIgnore]
        public string Id { get; set; }

        [JsonIgnore]
        public string? LocalPath { get; }
        public string? Ref { get; internal set; }
        public Uri? Url { get; }

        [JsonIgnore]
        public Image? InMemoryImage { get; private set; }
        public ImageScale ImageScale { get; set; }

        public ImageFill(string localPath, ImageScale imageScale)
        {
            LocalPath = localPath;
            ImageScale = imageScale;
        }

        public ImageFill(Uri url, ImageScale imageScale)
        {
            Url = url;
            ImageScale = imageScale;
        }

        public ImageFill(byte[] imageBytes, ImageScale imageScale)
        {
            InMemoryImage = Image.Load(imageBytes);
            ImageScale = imageScale;
        }
    }

    public enum ShapeType
    {
        Image,
        Rectangle,
        Circle,
        Text,
        Line,
    }
}
