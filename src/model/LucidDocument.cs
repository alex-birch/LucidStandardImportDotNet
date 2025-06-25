using Newtonsoft.Json;

namespace LucidStandardImport.model
{
    public class LucidDocument(string title)
    {
        // LucidIdFactory needs to be owned at the top level LucidDocument and passed to pages, to ensure uniqueness.
        [JsonIgnore]
        public LucidIdFactory LucidIdFactory { get; } = new();
        public string Title { get; set; } = title;
        public int Version { get; set; } = 1;
        public IReadOnlyList<Page> Pages
        {
            get { return [.._pages.Where(p => p != null)]; }
        }
        public IReadOnlyList<Collection> Collections
        {
            get { return _collections; }
        }
        public DocumentSettings DocumentSettings { get; set; }
        public BootstrapData BootstrapData { get; set; }

        private readonly List<Page> _pages = new List<Page>();
        private readonly List<Collection> _collections;

        public Page AddPage(string title = null, PageSettings pageSettings = null, int? index = null)
        {
            var page = new Page(LucidIdFactory, title, pageSettings);
            LucidIdFactory.AssignId(page);
            if (!index.HasValue)
                _pages.Add(page);
            else
            {
                while (_pages.Count < index.Value)
                    _pages.Add(null);
                if (_pages.Count == index.Value)
                    _pages.Add(page);
                else
                    _pages[index.Value] = page;
            }
            return page;
        }

        public void AddPage(Page page)
        {
            ArgumentNullException.ThrowIfNull(page.LucidIdFactory);
            // LucidIdFactory.AssignId(page);
            _pages.Add(page);
        }

        public void AddPages(IEnumerable<Page> pages)
        {
            foreach (var page in pages)
                AddPage(page);
        }
    }
}
