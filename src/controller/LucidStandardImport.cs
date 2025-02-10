using System.Diagnostics;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Text.Json;
using LucidStandardImport.Auth;
using LucidStandardImport.model;
using LucidStandardImport.util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace LucidStandardImport.Api
{
    public class LucidStandardImporter
    {
        // private ILucidOAuthProvider _lucidOAuthProvider;
        public string? DebugOutputFileLocation { get; set; }

        public LucidStandardImporter()
        {
            // _lucidOAuthProvider = lucidAuthProvider;
        }

        // public async Task<string[]> ImportDocument(
        //     LucidDocument lucidDocument,
        //     string documentTitle
        // )
        // {
        //     var session = await _lucidOAuthProvider.CreateLucidSessionAsync();
        //     return await ImportDocument(session, lucidDocument, documentTitle);
        // }

        public async Task<string[]> ImportDocument(
            LucidSession session,
            LucidDocument lucidDocument,
            string documentTitle
        )
        {
            var multiFiles = SplitPagesIntoMultiFiles(lucidDocument, documentTitle);
            var lucidUrls = new string[multiFiles.Count];

            var errors = new List<Exception>();

            var tasks = multiFiles.Select(
                async (splitFile, idx) =>
                {
                    try
                    {
                        // Step 1: Serialize and zip the document
                        var pathToFolder = SerializeAndZipToFolder(splitFile.LucidDocument);
                        var zipFilePath = ZipHelper.ZipFolderContents(
                            pathToFolder,
                            $"data_{Guid.NewGuid()}.lucid.zip"
                        );

                        if (DebugOutputFileLocation != null)
                            File.Copy(
                                zipFilePath.FullName,
                                Path.Combine(DebugOutputFileLocation, zipFilePath.Name)
                            );

                        // Step 2: Upload the file and store the URL
                        lucidUrls[idx] = await UploadLucidFile(
                            zipFilePath,
                            session,
                            splitFile.Title
                        );
                    }
                    catch (Exception ex)
                    {
                        // Log and collect the error with context
                        var errorMessage =
                            $"Error processing file {splitFile.Title} (index {idx}): {ex.Message}";
                        errors.Add(new Exception(errorMessage, ex));
                    }
                }
            );

            await Task.WhenAll(tasks);

            // Log or handle collected exceptions
            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    Console.WriteLine(error.Message); // Replace with your logging mechanism
                }

                // Optionally, throw an aggregated exception
                throw new AggregateException(
                    "One or more errors occurred during file processing.",
                    errors
                );
            }
            return lucidUrls;
        }

        private List<LucidDocumentTitleCombo> SplitPagesIntoMultiFiles(
            LucidDocument lucidDocument,
            string documentTitle
        )
        {
            return LucidDocumentSplitter.SplitPagesIntoMultiFiles(lucidDocument, documentTitle);
        }

        private async Task<string> UploadLucidFile(
            FileInfo lucidFile,
            LucidSession lucidSession,
            string title
        )
        {
            const string fileType = "x-application/vnd.lucid.standardImport";
            const string url = "https://api.lucid.co/documents";

            using var httpClient = new HttpClient();

            // 1) Add headers
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                lucidSession.Token.AccessToken
            );
            httpClient.DefaultRequestHeaders.Add("Lucid-Api-Version", "1");

            // 2) Build a multipart/form-data request
            using var formContent = new MultipartFormDataContent();

            // Add the file
            var fileStreamContent = new StreamContent(lucidFile.OpenRead());
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(fileType);
            formContent.Add(fileStreamContent, "file", "data.lucid");

            // Add form fields
            formContent.Add(new StringContent(fileType), "type");
            formContent.Add(new StringContent(title), "title");
            formContent.Add(new StringContent("lucidchart"), "product");

            // 3) Execute the POST request
            var response = await httpClient.PostAsync(url, formContent);

            // 4) Check the response
            var responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                // Example: parse JSON to retrieve "editUrl"
                // Depending on how Lucid returns data, adjust the code accordingly
                // For instance (using System.Text.Json):
                using var doc = JsonDocument.Parse(responseBody);
                if (doc.RootElement.TryGetProperty("editUrl", out var editUrlElement))
                {
                    return editUrlElement.GetString() ?? string.Empty;
                }
                else
                {
                    // If no "editUrl" found, handle accordingly
                    throw new Exception("Upload success but 'editUrl' not found in response.");
                }
            }
            else
            {
                // Attempt to parse error details if present
                try
                {
                    using var doc = JsonDocument.Parse(responseBody);
                    if (
                        doc.RootElement.TryGetProperty("details", out var details)
                        && details.TryGetProperty("error", out var errorElement)
                    )
                    {
                        var errorMsg = errorElement.GetString();
                        throw new Exception("Error producing Lucid Export - " + errorMsg);
                    }
                }
                catch
                {
                    // fallback if JSON parsing fails
                }

                throw new Exception(
                    $"Error uploading Lucid file. StatusCode: {response.StatusCode}, Body: {responseBody}"
                );
            }
        }

        public string SerializeDocument(LucidDocument lucidDocument)
        {
            CopyAndNameLocalImages(lucidDocument, null, true);
            return SerializeToJsonString(lucidDocument);
        }

        private DirectoryInfo SerializeAndZipToFolder(LucidDocument lucidDocument)
        {
            // 1. Create a unique temp folder (e.g. "C:\Temp\LucidTemp_<GUID>")
            var baseDirPath = Path.Combine(
                Path.GetTempPath(),
                "LucidTemp_" + Guid.NewGuid().ToString("N")
            );
            var baseDir = Directory.CreateDirectory(baseDirPath);

            // Create the "lucid_file" subfolder
            var lucidFileDirPath = Path.Combine(baseDir.FullName, "data.lucid");
            var lucidFileDir = Directory.CreateDirectory(lucidFileDirPath);

            // Prepare the "images" subfolder (we'll create it only if needed below)
            var imagesDirPath = Path.Combine(lucidFileDir.FullName, "images");
            var dataDirPath = Path.Combine(lucidFileDir.FullName, "data");

            if (!Directory.Exists(imagesDirPath))
                Directory.CreateDirectory(imagesDirPath);
            if (!Directory.Exists(dataDirPath))
                Directory.CreateDirectory(dataDirPath);

            // 2. Find local images in the document and copy them into the "images" subfolder.
            //    We'll also update the `Ref` property to a relative path.
            CopyAndNameLocalImages(lucidDocument, imagesDirPath, false);

            var json = SerializeToJsonString(lucidDocument);
            var documentJsonPath = Path.Combine(lucidFileDir.FullName, "document.json");
            File.WriteAllText(documentJsonPath, json);

            return lucidFileDir;
        }

        private static string SerializeToJsonString(LucidDocument lucidDocument)
        {
            var opts = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = { new StringEnumConverter(new CamelCaseNamingStrategy()) },
            };
            return JsonConvert.SerializeObject(lucidDocument, Formatting.Indented, opts);
        }

        private static void CopyAndNameLocalImages(
            LucidDocument lucidDocument,
            string? imagesDirPath,
            bool skipCopyingImages = false
        )
        {
            foreach (var page in lucidDocument.Pages)
            {
                foreach (var shape in page.Shapes)
                {
                    // Is this shape an ImageShape with a local file reference?
                    if (
                        shape is ImageShape imageShape
                        && !string.IsNullOrEmpty(imageShape.Image.LocalPath)
                    )
                    {
                        if (!File.Exists(imageShape.Image.LocalPath))
                            throw new ArgumentException(
                                $"Cannot find image file {imageShape.Image.LocalPath}"
                            );

                        var localFileInfo = new FileInfo(imageShape.Image.LocalPath);
                        var uploadedFileName = $"{shape.Id}_{localFileInfo.Extension}";
                        if (!skipCopyingImages)
                        {
                            if (imagesDirPath == null)
                            {
                                throw new ArgumentNullException(
                                    nameof(imagesDirPath),
                                    $"The images directory path must be provided when {nameof(skipCopyingImages)} is set to false."
                                );
                            }
                            // Ensure the images directory exists
                            Directory.CreateDirectory(imagesDirPath);

                            var destPath = Path.Combine(imagesDirPath, uploadedFileName);

                            File.Copy(imageShape.Image.LocalPath, destPath, overwrite: true);
                        }

                        imageShape.Image.Ref = uploadedFileName;
                    }
                }
            }
        }

        public void LaunchUrlsInBrowser(string[] urls)
        {
            if (urls == null || urls.Length == 0)
            {
                throw new ArgumentException("The URL list cannot be null or empty.", nameof(urls));
            }

            foreach (var url in urls)
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    Console.WriteLine("Skipping an empty or invalid URL.");
                    continue;
                }

                try
                {
                    Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = url,
                            UseShellExecute =
                                true // Ensures the URL is opened with the default browser
                            ,
                        }
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to launch URL: {url}. Error: {ex.Message}");
                }
            }
        }
    }

    public class LowercaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return name.ToLowerInvariant(); // Convert enum names to lowercase
        }
    }
}
