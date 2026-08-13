using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.Extensions.Configuration;
using ABCRetail.Models;
using System.Text;

namespace ABCRetail.Services
{
    public class FileStorageService
    {
        private readonly ShareClient shareClient;

        public FileStorageService(ShareServiceClient shareServiceClient, IConfiguration configuration)
        {
            string shareName = configuration["AzureStorage:FileSharedName"];
            shareClient = shareServiceClient.GetShareClient(shareName);
            shareClient.CreateIfNotExists();
        }

        // Upload a file 
        public async Task UploadFileAsync(Stream fileStream, string fileName)
        {
            ShareDirectoryClient rootDirectory = shareClient.GetRootDirectoryClient();
            ShareFileClient fileClient = rootDirectory.GetFileClient(fileName);

            await fileClient.CreateAsync(fileStream.Length);
            await fileClient.UploadAsync(fileStream);
        }

        // Keep the old text log method if you still want it
        public async Task UploadLogAsync(string fileName, string content)
        {
            ShareDirectoryClient rootDirectory = shareClient.GetRootDirectoryClient();
            ShareFileClient fileClient = rootDirectory.GetFileClient(fileName);

            byte[] bytes = Encoding.UTF8.GetBytes(content);
            await fileClient.CreateAsync(bytes.Length);

            using (MemoryStream stream = new MemoryStream(bytes))
            {
                await fileClient.UploadAsync(stream);
            }
        }

        public async Task<List<LogFile>> GetAllLogsAsync()
        {
            List<LogFile> logs = new List<LogFile>();
            ShareDirectoryClient rootDirectory = shareClient.GetRootDirectoryClient();

            await foreach (ShareFileItem item in rootDirectory.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    logs.Add(new LogFile
                    {
                        FileName = item.Name,
                        FileSize = item.FileSize
                    });
                }
            }

            return logs;
        }

        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            ShareDirectoryClient rootDirectory = shareClient.GetRootDirectoryClient();
            ShareFileClient fileClient = rootDirectory.GetFileClient(fileName);

            ShareFileDownloadInfo download = await fileClient.DownloadAsync();
            return download.Content;
        }

        public async Task DeleteLogAsync(string fileName)
        {
            ShareDirectoryClient rootDirectory = shareClient.GetRootDirectoryClient();
            ShareFileClient fileClient = rootDirectory.GetFileClient(fileName);
            await fileClient.DeleteIfExistsAsync();
        }
    }
}