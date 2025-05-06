namespace LucidStandardImport.model
{
    public class Layer(Page page, string title = null) : IIdentifiableLucidObject
    {
        public string Title { get; set; } = title ?? ""; // Name of the layer
        private Page _page = page ?? throw new ArgumentNullException(nameof(page));

        private List<Shape> ShapeReferences { get; } = new List<Shape>();
        public string Id { get; set; }
        public IEnumerable<string> Items
        {
            get { return ShapeReferences.Select(s => s.Id); }
        } // References IDs of shapes, lines, or groups in this layer
        public string Note { get; set; }
        public List<CustomData> CustomData { get; set; }
        public List<LinkedData> LinkedData { get; set; }


        /// <summary>
        /// Add to this layer & page
        /// </summary>
        public void AddShape(Shape shape)
        {
            ShapeReferences.Add(shape);
            _page.AddShape(shape);
        }

        /// <summary>
        /// Add shapes to this layer & page
        /// </summary>
        public void AddShapes(IEnumerable<Shape> shapes)
        {
            foreach (var shape in shapes)
                AddShape(shape);
        }

        public void RemoveShape(Shape shape)
        {
            ShapeReferences.Remove(shape);
        }
    }
}
