

using Newtonsoft.Json;

namespace LucidStandardImport.model
{
    public class LucidDocument
    {
        public int Version { get; set; } = 1;
        public required string Title { get; set; }
        public IReadOnlyList<Page> Pages
        {
            get { return _pages; }
        }
        public IReadOnlyList<Collection> Collections
        {
            get { return  _collections; }
        }
        public DocumentSettings DocumentSettings { get; set; }
        public BootstrapData BootstrapData { get; set; }

        private readonly List<Page> _pages = new List<Page>();
        private readonly List<Collection> _collections;

        [JsonIgnore]
        public ILucidIdFactory LucidIdFactory { get; }

        public LucidDocument(ILucidIdFactory lucidIdFactory)
        {
            LucidIdFactory = lucidIdFactory;
        }

        public LucidDocument()
            : this(new LucidIdFactory()) { }

        public void AddPage(Page page)
        {
            LucidIdFactory.AssignId(page);
            _pages.Add(page);
        }

        public void AddPages(IEnumerable<Page> pages)
        {
            _pages.AddRange(pages);
        }
    }
}
