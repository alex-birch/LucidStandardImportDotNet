namespace LucidStandardImport.model
{
    public enum UnitType
    {
        CM,
        Inches,
        PT,
        PX
    }

    public enum LineType
    {
        straight,
        elbow,
        curved
    }

    public enum EndpointType
    {
        lineEndpoint,
        shapeEndpoint,
        positionEndpoint
    }

    public enum StrokeStyle
    {
        solid,
        dashed,
        dotted
    }

    public enum ImageScale
    {
        Fit,
        Fill,
        Stretch,
        Original,
        Tile
    }
}