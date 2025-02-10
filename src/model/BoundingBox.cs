namespace LucidStandardImport.model
{
    public class BoundingBox
    {
        public decimal X { get; set; }
        public decimal Y { get; set; }
        public decimal W { get; set; }
        public decimal H { get; set; }
        public decimal? Rotation { get; set; }

        public BoundingBox(
            decimal x,
            decimal y,
            decimal width,
            decimal height,
            decimal? rotation = null
        )
        {
            X = x;
            Y = y;
            W = width;
            H = height;
            Rotation = rotation;
        }
    }
}
