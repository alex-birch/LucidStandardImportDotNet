namespace LucidStandardImport.model
{
    public class Layer : IIdentifiableLucidObject
    {
        private List<Shape> ShapeReferences { get; } = new List<Shape>();
        public string Id { get; set; }
        public string Title { get; set; } // Name of the layer
        public IEnumerable<string> Items { get {
            return ShapeReferences.Select(s => s.Id);
        } } // References IDs of shapes, lines, or groups in this layer
        public string Note { get; set; }
        public List<CustomData> CustomData { get; set; }
        public List<LinkedData> LinkedData { get; set; }

        public void AddShape(Shape shape)
        {
            ShapeReferences.Add(shape);
        }
        public void RemoveShape(Shape shape)
        {
            ShapeReferences.Remove(shape);
        }
    }
}