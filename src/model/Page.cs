using System.Collections.ObjectModel;

namespace LucidStandardImport.model
{
    public class Page(ILucidIdFactory lucidIdFactory) : IIdentifiableLucidObject
    {
        private ILucidIdFactory lucidIdFactory = lucidIdFactory;
        public string Id { get; set; }
        public string Title { get; set; } = "";
        public PageSettings Settings { get; set; }
        public IReadOnlyList<Shape> Shapes
        {
            get { return _shapes; }
        }
        public IReadOnlyList<Line> Lines
        {
            get { return _lines; }
        }
        public IReadOnlyList<Group> Groups
        {
            get { return _groups; }
        }
        public IReadOnlyList<Layer> Layers
        {
            get { return _layers; }
        }

        private List<Line> _lines = new List<Line>();
        private List<Shape> _shapes = new List<Shape>();
        private List<Layer> _layers = new List<Layer>();
        private List<Group> _groups = new List<Group>();

        public ReadOnlyCollection<CustomData> CustomData { get; set; }

        public Page()
            : this(new LucidIdFactory()) { }

        public void AddShape(Shape shape)
        {
            lucidIdFactory.AssignId(shape);
            _shapes.Add(shape);
        }

        public void AddLayer(Layer layer)
        {
            lucidIdFactory.AssignId(layer);
            _layers.Add(layer);
        }

        public void AddGroup(Group group)
        {
            lucidIdFactory.AssignId(group);
            _groups.Add(group);
        }
    }

    public class PageSize
    {
        public PaperSize Type { get; set; } // e.g., "letter", "a4"
        public PaperOrientation Format { get; set; } // e.g., "portrait", "landscape"
        public int Width { get; set; } // For custom sizes
        public int Height { get; set; } // For custom sizes
    }

    public enum PaperSize 
    {
        a4, a3, a2, a1, a0, letter, legal, tabloid
    }
    public enum PaperOrientation
    {
        portrait, landscape
    }
}
