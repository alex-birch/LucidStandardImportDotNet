namespace LucidStandardImport.model
{
    public enum UnitType
    {
        CM,
        Inches,
        PT,
        PX,
    }

    public enum LineType
    {
        straight,
        elbow,
        curved,
    }

    public enum EndpointType
    {
        lineEndpoint,
        shapeEndpoint,
        positionEndpoint,
    }

    public enum EndpointStyle
    {
        none,
        aggregation,
        arrow,
        hollowArrow,
        openArrow,
        async1,
        async2,
        closedSquare,
        openSquare,
        bpmnConditional,
        bpmnDefault,
        closedCircle,
        openCircle,
        composition,
        exactlyOne,
        generalization,
        many,
        nesting,
        one,
        oneOrMore,
        zeroOrMore,
        zeroOrOne,
    }

    public enum StrokeStyle
    {
        solid,
        dashed,
        dotted,
    }

    public enum ImageScale
    {
        Fit,
        Fill,
        Stretch,
        Original,
        Tile,
    }
}
