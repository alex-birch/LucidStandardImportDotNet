using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LucidStandardImport.Auth;
using LucidStandardImport.model;
using LucidStandardImport.util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using SixLabors.ImageSharp;

namespace LucidStandardImport.Api
{
    public class LucidStandardImporter
    {
        // private ILucidOAuthProvider _lucidOAuthProvider;
        public string? DebugOutputFileLocation { get; set; }
        public string? DebugInputZipFileLocation { get; set; }

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

        public async Task<string[]> ImportDocumentAsync(
            LucidSession session,
            LucidDocument lucidDocument,
            string documentTitle,
            bool validateFileSize = false
        )
        {
            var multiFiles = LucidDocumentSplitter.SplitPagesAsYouGo(lucidDocument);
            var lucidUrls = new string[multiFiles.Count];

            var errors = new List<Exception>();

            var tasks = multiFiles.Select(
                async (splitFile, idx) =>
                {
                    try
                    {
                        FileInfo zipFilePath;

                        // Step 1: Check if DebugInputZipFileLocation is set
                        if (!string.IsNullOrEmpty(DebugInputZipFileLocation))
                        {
                            // Use the zip file from DebugInputZipFileLocation
                            zipFilePath = new FileInfo(DebugInputZipFileLocation);
                            if (!zipFilePath.Exists)
                            {
                                throw new FileNotFoundException(
                                    $"The specified debug zip file does not exist: {DebugInputZipFileLocation}"
                                );
                            }
                        }
                        else
                        {
                            // Serialize and zip the document
                            var pathToFolder = await SerializeAndZipToFolder(splitFile);
                            zipFilePath = ZipHelper.ZipFolderContents(
                                pathToFolder,
                                $"data_{Guid.NewGuid()}.lucid.zip"
                            );

                            // Optionally copy the zip file to DebugOutputFileLocation
                            if (DebugOutputFileLocation != null)
                            {
                                File.Copy(
                                    zipFilePath.FullName,
                                    Path.Combine(DebugOutputFileLocation, zipFilePath.Name)
                                );
                            }
                        }

                        if (validateFileSize)
                            CheckLucidFileLimitations(zipFilePath);

                        // Step 2: Upload the file and store the URL
                        lucidUrls[idx] = await UploadLucidFile(
                            zipFilePath,
                            session,
                            multiFiles.Count > 1
                                ? $"{splitFile.Title} - Part {idx + 1}"
                                : splitFile.Title
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

        public async Task<string> UploadLucidFile(
            string zipFilePath,
            LucidSession lucidSession,
            string title
        )
        {
            var lucidFile = new FileInfo(zipFilePath);
            if (!lucidFile.Exists)
                throw new FileNotFoundException(
                    $"The specified file does not exist: {zipFilePath}"
                );
            return await UploadLucidFile(lucidFile, lucidSession, title);
        }

        public async Task<string> UploadLucidFile(
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

        // public string SerializeDocument(LucidDocument lucidDocument)
        // {
        //     CopyAndNameLocalImages(lucidDocument, null, true);
        //     return SerializeToJsonString(lucidDocument);
        // }

        private async Task<DirectoryInfo> SerializeAndZipToFolder(LucidDocument lucidDocument)
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
            await CopyAndNameLocalImagesAsync(lucidDocument, imagesDirPath, false);

            var json = lucidDocument.SerializeToJsonString();
            var documentJsonPath = Path.Combine(lucidFileDir.FullName, "document.json");
            File.WriteAllText(documentJsonPath, json, new UTF8Encoding(false)); // UTF-8 without BOM

            return lucidFileDir;
        }

        private static async Task CopyAndNameLocalImagesAsync(
            LucidDocument lucidDocument,
            string? imagesDirPath,
            bool skipCopyingImages = false
        )
        {
            var imageTasks = new List<Task>();

            foreach (var page in lucidDocument.Pages)
            {
                foreach (var shape in page.Shapes)
                {
                    if (shape is ImageShape imageShape)
                    {
                        if (!skipCopyingImages)
                            Directory.CreateDirectory(
                                imagesDirPath
                                    ?? throw new ArgumentNullException(
                                        nameof(imagesDirPath),
                                        $"The images directory path must be provided when {nameof(skipCopyingImages)} is set to false."
                                    )
                            );

                        if (!string.IsNullOrEmpty(imageShape.ImageFill.LocalPath))
                        {
                            // Handle local file path
                            if (!File.Exists(imageShape.ImageFill.LocalPath))
                                throw new ArgumentException(
                                    $"Cannot find image file {imageShape.ImageFill.LocalPath}"
                                );

                            var localFileInfo = new FileInfo(imageShape.ImageFill.LocalPath);
                            var uploadedFileName = $"{shape.Id}{localFileInfo.Extension}";
                            if (!skipCopyingImages)
                            {
                                var destPath = Path.Combine(imagesDirPath, uploadedFileName);
                                imageTasks.Add(
                                    Task.Run(
                                        () =>
                                            File.Copy(
                                                imageShape.ImageFill.LocalPath,
                                                destPath,
                                                overwrite: true
                                            )
                                    )
                                );
                            }

                            imageShape.ImageFill.Ref = uploadedFileName;
                        }
                        else if (imageShape.ImageFill.InMemoryImage != null)
                        {
                            // Handle in-memory image
                            var uploadedFileName = $"{imageShape.ImageFill.Id}.png";
                            if (!skipCopyingImages)
                            {
                                var destPath = Path.Combine(imagesDirPath, uploadedFileName);
                                imageTasks.Add(
                                    Task.Run(() =>
                                    {
                                        using var fileStream = File.OpenWrite(destPath);
                                        imageShape.ImageFill.InMemoryImage.SaveAsPng(fileStream);
                                    })
                                );
                            }

                            imageShape.ImageFill.Ref = uploadedFileName;
                        }
                    }
                }
            }

            await Task.WhenAll(imageTasks);
        }

        public void LaunchUrlInBrowser(string url)
        {
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
                LaunchUrlInBrowser(url);
            }
        }

        public static void CheckLucidFileLimitations(FileInfo zipFile)
        {
            const long ZIP_LIMIT = 50 * 1024 * 1024;
            const long DATA_LIMIT = 1 * 1024 * 1024;
            const long IMAGES_LIMIT = 50 * 1024 * 1024;
            const long DOC_JSON_LIMIT = 2 * 1024 * 1024;

            long dataFolderSize = 0;
            long imagesFolderSize = 0;
            long documentJsonSize = 0;
            long totalUnzippedSize = 0;

            using (ZipArchive archive = ZipFile.OpenRead(zipFile.FullName))
            {
                foreach (var entry in archive.Entries)
                {
                    totalUnzippedSize += entry.Length;

                    if (entry.FullName.StartsWith("data/"))
                        dataFolderSize += entry.Length;
                    else if (entry.FullName.StartsWith("images/"))
                        imagesFolderSize += entry.Length;
                    else if (entry.FullName == "document.json")
                        documentJsonSize = entry.Length;
                }
            }

            StringBuilder message = new StringBuilder();

            if (zipFile.Length > ZIP_LIMIT)
                message.AppendLine(
                    $"ZIP file size {zipFile.Length / (1024 * 1024.0):F2}MB exceeds limit of 50MB."
                );
            if (dataFolderSize > DATA_LIMIT)
                message.AppendLine(
                    $"/data folder size {dataFolderSize / (1024 * 1024.0):F2}MB exceeds limit of 1MB."
                );
            if (imagesFolderSize > IMAGES_LIMIT)
                message.AppendLine(
                    $"/images folder size {imagesFolderSize / (1024 * 1024.0):F2}MB exceeds limit of 50MB."
                );
            if (documentJsonSize > DOC_JSON_LIMIT)
                message.AppendLine(
                    $"document.json size {documentJsonSize / (1024 * 1024.0):F2}MB exceeds limit of 2MB."
                );

            if (message.Length > 0)
                throw new InvalidOperationException(
                    "Lucid file size limitations exceeded:\n" + message.ToString()
                );
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
