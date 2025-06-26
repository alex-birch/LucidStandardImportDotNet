using LucidStandardImport.util;
using Newtonsoft.Json;
using SixLabors.ImageSharp;


namespace LucidStandardImport.model;

public abstract class Shape(BoundingBox boundingBox = null) : IIdentifiableLucidObject
{
    public string Id { get; set; }
    [JsonIgnore]
    public string ExternalId { get; set; }
    public ShapeType Type { get; protected set; }
    public BoundingBox BoundingBox { get; set; } = boundingBox;
    public Style Style { get; set; }
    public string Text { get; set; } = ""; // Set to empty string or some shapes will show default 'text'
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
    [JsonProperty("image")]
    public required ImageFill ImageFill { get; set; }
    public required Stroke Stroke { get; set; }

    public async Task<List<ImageShape>> ProcessImageAsync(bool? greyscale = true, int? tileSize = 1000)
    {
        if (ImageFill.InMemoryImage == null && ImageFill.LocalPath == null)
            return [this];

        var image = ImageFill.InMemoryImage ?? Image.Load(ImageFill.LocalPath);
        image = await ImageSharpHelper.ProcessPngAsync(image, BoundingBox, greyscale.Value);
        
        if (tileSize.HasValue && tileSize > 0)
            return this.TileImage(image, tileSize);

        ImageFill.InMemoryImage = image;
        return [this];
    }
}

public class RectangleShape : Shape
{
    public RectangleShape()
    {
        Type = ShapeType.Rectangle;
    }
}

public class CircleShape : Shape
{
    public CircleShape()
    {
        Type = ShapeType.Circle;
    }
}
public class HexagonShape : Shape
{
    public HexagonShape()
    {
        Type = ShapeType.Hexagon;
    }
}
public class PentagonShape : Shape
{
    public PentagonShape()
    {
        Type = ShapeType.Pentagon;
    }
}



public class TextShape : Shape
{
    public TextShape()
    {
        Type = ShapeType.Text;
    }
}

public class TableShape : Shape
{
    public TableShape()
    {
        Type = ShapeType.Table;
        Text = null;
    }

    public TableShape(BoundingBox boundingBox,
    TableCell[,] cells,
    List<double?> rowHeights = null,
    List<double?> colWidths = null)
    {
        Type = ShapeType.Table;
        Text = null;
        BoundingBox = boundingBox;

        RowCount = cells.GetLength(0);
        ColCount = cells.GetLength(1);
        for (int row = 0; row < RowCount; row++)
            for (int col = 0; col < ColCount; col++)
            {
                var cell = cells[row, col];
                if (cell == null) continue;
                cell.YPosition = row;
                cell.XPosition = col;
                Cells.Add(cell);
            }

        UserSpecifiedRows = rowHeights != null
            ? [.. rowHeights
                .Select((h, i) => h != null ? new FieldSize { Index = i, Size = h.Value } : null)
                .Where(fs => fs != null)]
            : [];
        UserSpecifiedCols = colWidths != null
            ? [.. colWidths
                .Select((w, i) => w != null ? new FieldSize { Index = i, Size = w.Value
                 } : null)
                .Where(fs => fs != null)]
            : [];
    }

    /// <summary>
    /// Number of rows in the table (required).
    /// </summary>
    public int RowCount { get; set; }

    /// <summary>
    /// Number of columns in the table (required).
    /// </summary>
    public int ColCount { get; set; }

    /// <summary>
    /// Array of cell definitions (required).
    /// </summary>
    public List<TableCell> Cells { get; set; } = [];

    /// <summary>
    /// User-specified row heights (optional).
    /// </summary>
    public List<FieldSize> UserSpecifiedRows { get; set; }

    /// <summary>
    /// User-specified column widths (optional).
    /// </summary>
    public List<FieldSize> UserSpecifiedCols { get; set; }

    /// <summary>
    /// Show vertical borders between cells (optional, default true).
    /// </summary>
    public bool VerticalBorder { get; set; } = true;

    /// <summary>
    /// Show horizontal borders between cells (optional, default true).
    /// </summary>
    public bool HorizontalBorder { get; set; } = true;
}

public class TableCell(string text)
{
    public int XPosition { get; set; }
    public int YPosition { get; set; }
    public int MergeCellsRight { get; set; } = 0;
    public int MergeCellsDown { get; set; } = 0;
    public Style Style { get; set; }
    public string Text { get; set; } = text;
}

public class ImageFill : IIdentifiableLucidObject
{
    [JsonIgnore]
    public string Id { get; set; }

    [JsonIgnore]
    public string ExternalId { get; set; }

    [JsonIgnore]
    public string? LocalPath { get; }
    public string? Ref { get; internal set; }
    public Uri? Url { get; }

    [JsonIgnore]
    public Image? InMemoryImage { get; set; }
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
    public ImageFill(Image image, ImageScale imageScale)
    {
        InMemoryImage = image;
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
    Table,
    Hexagon,
    Pentagon
}
