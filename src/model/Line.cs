namespace LucidStandardImport.model
{
    public class Line : IIdentifiableLucidObject
    {
        public string Id { get; set; }
        public LineType LineType { get; set; }
        public Endpoint Endpoint1 { get; set; }
        public Endpoint Endpoint2 { get; set; }
        public Stroke Stroke { get; set; }
        public List<LineText> Text { get; set; }
        public List<CustomData> CustomData { get; set; }
        public List<LinkedData> LinkedData { get; set; }
        public List<Point<double>> Joints { get; set; }
        public List<Point<double>> ElbowControlPoints { get; set; }
    }

    public class Point<T>
    {
        public T X { get; set; }
        public T Y { get; set; }
    }
}
