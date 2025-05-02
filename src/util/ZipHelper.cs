using System.IO.Compression;

namespace LucidStandardImport.util
{
    public class ZipHelper
    {
        public static FileInfo ZipFolderContents(DirectoryInfo directoryInfo, string zipFileName)
        {
            if (directoryInfo == null)
            {
                throw new ArgumentNullException(
                    nameof(directoryInfo),
                    "The directory cannot be null."
                );
            }

            if (!directoryInfo.Exists)
            {
                throw new DirectoryNotFoundException(
                    $"The directory '{directoryInfo.FullName}' does not exist."
                );
            }

            if (string.IsNullOrWhiteSpace(zipFileName))
            {
                throw new ArgumentException(
                    "The zip file name cannot be null or empty.",
                    nameof(zipFileName)
                );
            }

            // Ensure the zip file name has the .zip extension
            if (!zipFileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                zipFileName += ".zip";
            }

            string zipFilePath = Path.Combine(
                directoryInfo.Parent?.FullName
                    ?? throw new InvalidOperationException("Parent directory is null"),
                zipFileName
            );

            // Delete the ZIP file if it already exists
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }

            // Create the ZIP file with the contents of the directory
            using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                foreach (var file in directoryInfo.GetFiles())
                {
                    zipArchive.CreateEntryFromFile(file.FullName, file.Name);
                }

                foreach (var subDir in directoryInfo.GetDirectories())
                {
                    AddDirectoryToZip(zipArchive, subDir, subDir.Name);
                }
            }

            return new FileInfo(zipFilePath);
        }

        // Helper method to add directories recursively
        private static void AddDirectoryToZip(
            ZipArchive zipArchive,
            DirectoryInfo directory,
            string entryPath
        )
        {
            foreach (var file in directory.GetFiles())
            {
                string entryFilePath = $"{entryPath}/{file.Name}".Replace("\\", "/");
                zipArchive.CreateEntryFromFile(file.FullName, entryFilePath);
            }

            foreach (var subDir in directory.GetDirectories())
            {
                string subDirPath = $"{entryPath}/{subDir.Name}".Replace("\\", "/");
                AddDirectoryToZip(zipArchive, subDir, subDirPath);
            }
        }

    }
}
