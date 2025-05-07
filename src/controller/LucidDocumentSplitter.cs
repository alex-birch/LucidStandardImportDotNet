using System.Text.Json; // or Newtonsoft.Json
using System.Text.Json.Serialization;
using LucidStandardImport.model;

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
            var baseDocJson = JsonSerializer.Serialize(new LucidDocument(originalDoc.Title));

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
                var testDocJson = JsonSerializer.Serialize(testDoc);
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
                    currentSize = JsonSerializer.Serialize(currentDocument).Length;
                    // Add the page to the new doc
                    currentSize += JsonSerializer.Serialize(page).Length;
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
    }

    public class LucidDocumentTitleCombo
    {
        public required LucidDocument LucidDocument { get; set; }
        public required string Title { get; set; }
    }
}
