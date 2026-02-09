using System.IO.Compression;

namespace DotNetBlueprint.Services
{
    public class ZipService
    {
        public byte[] CreateZipAsBytes(string sourceFolder, string rootFolderName = null!)
        {
            using var memoryStream = new MemoryStream();


            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: false))
            {
                foreach (var file in Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(sourceFolder, file)
                        .Replace("\\", "/"); // Forward slashes for compatibility

                    if (!string.IsNullOrEmpty(rootFolderName))
                        relativePath = rootFolderName + "/" + relativePath;

                    archive.CreateEntryFromFile(file, relativePath);
                }
            }

            return memoryStream.ToArray();
        }
    }
}
