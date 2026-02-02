using System.IO.Compression;

namespace DotNetBlueprint.Services
{
    public class ZipService
    {
        public byte[] CreateZipAsBytes(string sourceFolder, string rootFolderName = null!)
        {
            using var memoryStream = new MemoryStream();

            // Use leaveOpen: false so archive is finalized
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: false))
            {
                foreach (var file in Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories))
                {
                    // Relative path from source folder
                    var relativePath = Path.GetRelativePath(sourceFolder, file)
                        .Replace("\\", "/"); // Forward slashes for compatibility

                    // If a root folder name is provided, prepend it
                    if (!string.IsNullOrEmpty(rootFolderName))
                        relativePath = rootFolderName + "/" + relativePath;

                    archive.CreateEntryFromFile(file, relativePath);
                }
            }

            return memoryStream.ToArray();
        }
    }
}
