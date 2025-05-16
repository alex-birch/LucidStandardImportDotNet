using System.Text.Json.Serialization;

namespace LucidStandardImport.model
{
    public class Layer(Page page, string title = null) : IIdentifiableLucidObject
    {
        public string Title { get; set; } = title ?? ""; // Name of the layer
        private readonly Page _page = page ?? throw new ArgumentNullException(nameof(page));

        private List<Shape> ShapeReferences { get; } = [];
        private List<Line> LineReferences { get; } = [];
        private List<Group> GroupReferences { get; } = [];
        public string Id { get; set; }
[JsonIgnore]
        public string ExternalId { get; set; }

        public IEnumerable<string> Items
        {
            get { return [..ShapeReferences.Select(s => s.Id), ..GroupReferences.Select(g => g.Id)]; }
        } // References IDs of shapes, lines, or groups in this layer
        public string Note { get; set; }
        public List<CustomData> CustomData { get; set; }
        public List<LinkedData> LinkedData { get; set; }


        /// <summary>
        /// Add to this layer & page
        /// </summary>
        public Layer AddShape(Shape shape)
        {
_page.AddShape(shape);
            ShapeReferences.Add(shape);
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
         public Layer AddGroupedShape(IEnumerable<Shape> groupedShapes)
        {
            // AddShapes(shapeGroup); // Must add shapes before adding group, as this creates their Id
            // _page.AddShapes(shapeGroup);
            // _page.AddGroup(new Group(shapeGroup));
            _page.AddGroup(groupedShapes, out var group);
            GroupReferences.Add(group);
            return this;
        }

        public Layer AddGroupedShapes(IEnumerable<IEnumerable<Shape>> groupedShapes)
        {
            foreach (var shapeGroup in groupedShapes)
                AddGroupedShape(shapeGroup);
            return this;
        }

        public void RemoveShape(Shape shape)
        {
            ShapeReferences.Remove(shape);
        }
    }
}
