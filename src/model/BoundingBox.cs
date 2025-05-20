namespace LucidStandardImport.model;

public class BoundingBox
{
    public double X { get; set; }
    public double Y { get; set; }
    public double W { get; set; }
    public double H { get; set; }
    public double? Rotation { get; set; }

    public BoundingBox(double x, double y, double width, double height, double? rotation = null)
    {
        X = x;
        Y = y;
        W = width;
        H = height;
        Rotation = rotation;
    }
}
