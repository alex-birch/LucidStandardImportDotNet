using System.Text;
using System.Text.Json; // or Newtonsoft.Json
using System.Text.Json.Serialization;
using LucidStandardImport.model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace LucidStandardImport.Api
{
    public static class LucidDocumentSplitter
    {
        /// <summary>
        /// Splits a <see cref="LucidDocument"/> into multiple documents if the serialized JSON
        /// would exceed <paramref name="maxSizeBytes"/>. Returns a list of new documents,
        /// each containing a subset of pages.
        /// </summary>
        /// <param name="originalDoc">The original <see cref="LucidDocument"/>.</param>
        /// <param name="documentTitle">Base title for documents (you can append page info if needed).</param>
        /// <param name="maxSizeBytes">Maximum size in bytes. Defaults to 2MB.</param>
        /// <returns>List of (LucidDocument + Title) combos.</returns>
        public static List<LucidDocumentTitleCombo> SplitPagesIntoMultiFiles(
            LucidDocument originalDoc,
            string documentTitle,
            int maxSizeBytes = 2 * 1048576 // 2 MB
        )
        {
            // If there are no pages, just return the original document in a list
            if (originalDoc.Pages == null || originalDoc.Pages.Count < 2)
            {
                return new List<LucidDocumentTitleCombo>
                {
                    new LucidDocumentTitleCombo
                    {
                        LucidDocument = originalDoc,
                        Title = documentTitle,
                    },
                };
            }

            // Prepare a base copy with an empty list of pages
            // so we can fill pages as we go.
            var baseDocJson = System.Text.Json.JsonSerializer.Serialize(
                new LucidDocument(originalDoc.Title)
            );

            // currentDocument will hold the "in-progress" subset of pages
            var currentDocument = CloneDocumentWithoutPages(originalDoc);
            var currentPages = new List<Page>();
            long currentSize = baseDocJson.Length; // approximate size of the empty doc

            var result = new List<LucidDocumentTitleCombo>();

            foreach (var page in originalDoc.Pages)
            {
                // Attempt adding this page to the current doc
                var testDoc = CloneDocumentWithoutPages(originalDoc);
                testDoc.AddPages(currentPages);
                testDoc.AddPage(page);

                // Get an approximate size after adding this page
                var testDocJson = System.Text.Json.JsonSerializer.Serialize(testDoc);
                long testDocSize = testDocJson.Length;

                if (currentSize + (testDocSize - currentSize) > maxSizeBytes)
                {
                    // That means adding this page would exceed maxSizeBytes
                    // So we finalize the currentDocument with what it has
                    if (currentPages.Count > 0)
                    {
                        var finalizedDoc = CloneDocumentWithoutPages(originalDoc);
                        finalizedDoc.AddPages(currentPages);

                        result.Add(
                            new LucidDocumentTitleCombo
                            {
                                LucidDocument = finalizedDoc,
                                Title = documentTitle + $" (Part {result.Count + 1})",
                            }
                        );
                    }

                    // Start a new doc
                    currentDocument = CloneDocumentWithoutPages(originalDoc);
                    currentPages = new List<Page> { page };
                    currentSize = System.Text.Json.JsonSerializer.Serialize(currentDocument).Length;
                    // Add the page to the new doc
                    currentSize += System.Text.Json.JsonSerializer.Serialize(page).Length;
                }
                else
                {
                    // We can add this page safely
                    currentPages.Add(page);
                    currentSize = testDocSize;
                }
            }

            // Add whatever remains
            if (currentPages.Count > 0)
            {
                var lastDoc = CloneDocumentWithoutPages(originalDoc);
                lastDoc.AddPages(currentPages);

                result.Add(
                    new LucidDocumentTitleCombo
                    {
                        LucidDocument = lastDoc,
                        Title = documentTitle + $" (Part {result.Count + 1})",
                    }
                );
            }

            return result;
        }

        /// <summary>
        /// Helper to create a shallow clone of the document without pages.
        /// You can customize what fields you want to copy.
        /// </summary>
        private static LucidDocument CloneDocumentWithoutPages(LucidDocument original)
        {
            return new LucidDocument(original.Title);
        }

        public static List<LucidDocument> SplitPagesAsYouGo(
            LucidDocument originalDoc,
            int maxSizeBytes = 2 * 1048576 // 2 MB
        )
        {
            var result = new List<LucidDocument>();
            var allPages = originalDoc.Pages ?? new List<Page>();

            LucidDocument currentDoc = new LucidDocument(originalDoc.Title)
            {
                Version = originalDoc.Version,
                DocumentSettings = originalDoc.DocumentSettings,
                BootstrapData = originalDoc.BootstrapData
            };

            foreach (var page in allPages)
            {
                currentDoc.AddPage(page);

                var json = currentDoc.SerializeToJsonString();
                var byteCount = Encoding.UTF8.GetByteCount(json);

                if (byteCount > maxSizeBytes)
                {
                    // Remove the last page, finalize current doc
                    currentDoc = RemoveLastPageAndAddToResult(currentDoc, result);

                    // Start a new doc with the current page
                    currentDoc = new LucidDocument(originalDoc.Title)
                    {
                        Version = originalDoc.Version,
                        DocumentSettings = originalDoc.DocumentSettings,
                        BootstrapData = originalDoc.BootstrapData
                    };
                    currentDoc.AddPage(page);

                    var singlePageJson = currentDoc.SerializeToJsonString();
                    var singlePageByteCount = Encoding.UTF8.GetByteCount(singlePageJson);
                    if (singlePageByteCount > maxSizeBytes)
                    {
                        throw new InvalidOperationException(
                            $"A single page ({page.Title}) exceeds the maximum allowed size of {maxSizeBytes / (1024 * 1024.0):F2}MB."
                        );
                    }
                }
            }

            // Add the last doc if it has pages
            if (currentDoc.Pages != null && currentDoc.Pages.Count > 0)
            {
                result.Add(currentDoc);
            }

            return result;
        }

        private static LucidDocument RemoveLastPageAndAddToResult(
            LucidDocument doc,
            List<LucidDocument> result
        )
        {
            // Remove the last page
            var pagesField = typeof(LucidDocument).GetField(
                "_pages",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            if (pagesField != null)
            {
                var pagesList = pagesField.GetValue(doc) as List<Page>;
                if (pagesList != null && pagesList.Count > 0)
                {
                    pagesList.RemoveAt(pagesList.Count - 1);
                }
            }
            result.Add(doc);
            return doc;
        }
    }

    public class LucidDocumentTitleCombo
    {
        public required LucidDocument LucidDocument { get; set; }
        public required string Title { get; set; }
    }
}
