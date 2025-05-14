using System.Collections.ObjectModel;

namespace LucidStandardImport.model
{
    public class Page(ILucidIdFactory lucidIdFactory, string title = null, PageSettings pageSettings = null) : IIdentifiableLucidObject
    {
        // LucidIdFactory needs to be owned at the top level LucidDocument and passed to pages, to ensure uniqueness.
        public ILucidIdFactory LucidIdFactory { get; } = lucidIdFactory ?? throw new ArgumentNullException(nameof(lucidIdFactory));
        public string Id { get; set; }
        public string Title { get; set; } = title ?? "";
        public PageSettings Settings { get; set; } = pageSettings;
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

        // public Page()
        //     : this(new LucidIdFactory())
        // {
        //     lucidIdFactory.AssignId(this);
        // }

        public Page AddShape(Shape shape)
        {
            LucidIdFactory.AssignId(shape);
            _shapes.Add(shape);
            if (shape is ImageShape imageShape)
                LucidIdFactory.AssignId(imageShape.Image);
            return this;
        }
        public Page AddShapes(IEnumerable<Shape> shape)
        {
            foreach (var s in shape)
                AddShape(s);
            return this;
        }

        public Page AddLayer(Layer layer)
        {
            LucidIdFactory.AssignId(layer);
            _layers.Add(layer);
            return this;
        }

        public Page AddLayers(IEnumerable<Layer> layers)
        {
            foreach (var layer in layers)
                AddLayer(layer);
            return this;
        }

        public Layer AddLayer(string title)
        {
            AddLayer(title, out Layer layer);
            return layer;
        }

        public Page AddLayer(string title, out Layer layer)
        {
            layer = new Layer(this) { Title = title };
            LucidIdFactory.AssignId(layer);
            _layers.Add(layer);
            return this;
        }

        public Page AddGroup(Group group)
        {
            LucidIdFactory.AssignId(group);
            _groups.Add(group);
            return this;
        }
        public Page AddGroup(IEnumerable<Shape> shapes, out Group group)
        {
            AddShapes(shapes);
            group = new Group(shapes);
            LucidIdFactory.AssignId(group);
            _groups.Add(group);
            return this;
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
        a4,
        a3,
        a2,
        a1,
        a0,
        letter,
        legal,
        tabloid
    }

    public enum PaperOrientation
    {
        portrait,
        landscape
    }
}
