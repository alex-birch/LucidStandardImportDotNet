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
        public Layer AddShape(Shape shape)
        {
            ShapeReferences.Add(shape);
            _page.AddShape(shape);
            return this;
        }

        /// <summary>
        /// Add shapes to this layer & page
        /// </summary>
        public Layer AddShapes(IEnumerable<Shape> shapes)
        {
            foreach (var shape in shapes)
                AddShape(shape);
            return this;
        }

        /// <summary>
        /// Add shapes to this layer & page, grouping by outer list
        /// </summary>
        public Layer AddGroupedShapes(IEnumerable<IEnumerable<Shape>> groupedShapes)
        {
            foreach (var shapeGroup in groupedShapes)
            {
                AddShapes(shapeGroup); // Must add shapes before adding group, as this creates their Id
                _page.AddGroup(new Group(shapeGroup));
            }
            return this;
        }

        public void RemoveShape(Shape shape)
        {
            ShapeReferences.Remove(shape);
        }
    }
}
