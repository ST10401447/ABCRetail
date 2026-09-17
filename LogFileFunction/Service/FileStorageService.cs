
using ABCRetail_ClassLibrary;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace LogFileFunction.Service
{
    public class FileStorageService
    {
        private readonly ShareClient shareClient;

        public FileStorageService(IConfiguration configuration)
        {
            string connectionString =
                Environment.GetEnvironmentVariable("AzureStorage");

            string shareName =
                Environment.GetEnvironmentVariable("FileSharedName");

            shareClient = new ShareClient(
                connectionString,
                shareName);

            shareClient.CreateIfNotExists();
        }

        // CREATE / UPLOAD FILE
        public async Task UploadFileAsync(
            Stream fileStream,
            string fileName)
        {
            // HTTP request streams do not support Length.
            // Copy the request stream into memory first.
            using MemoryStream memoryStream = new MemoryStream();

            await fileStream.CopyToAsync(memoryStream);

            memoryStream.Position = 0;

            ShareDirectoryClient rootDirectory =
                shareClient.GetRootDirectoryClient();

            ShareFileClient fileClient =
                rootDirectory.GetFileClient(fileName);

            // Delete the existing file first.
            // This allows the same method to also replace/update files.
            await fileClient.DeleteIfExistsAsync();

            // Create the new file.
            await fileClient.CreateAsync(memoryStream.Length);

            // Reset stream position before uploading.
            memoryStream.Position = 0;

            // Upload the file.
            await fileClient.UploadAsync(memoryStream);
        }

        // CREATE / UPLOAD TEXT LOG
        public async Task UploadLogAsync(
            string fileName,
            string content)
        {
            byte[] bytes =
                Encoding.UTF8.GetBytes(content);

            using MemoryStream memoryStream =
                new MemoryStream(bytes);

            await UploadFileAsync(
                memoryStream,
                fileName);
        }

        // READ ALL FILES
        public async Task<List<LogFile>> GetAllLogsAsync()
        {
            List<LogFile> logs =
                new List<LogFile>();

            ShareDirectoryClient rootDirectory =
                shareClient.GetRootDirectoryClient();

            await foreach (
                ShareFileItem item
                in rootDirectory.GetFilesAndDirectoriesAsync())
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

        // READ / DOWNLOAD FILE
        public async Task<Stream> DownloadFileAsync(
            string fileName)
        {
            ShareDirectoryClient rootDirectory =
                shareClient.GetRootDirectoryClient();

            ShareFileClient fileClient =
                rootDirectory.GetFileClient(fileName);

            ShareFileDownloadInfo download =
                await fileClient.DownloadAsync();

            return download.Content;
        }

        // DELETE FILE
        public async Task DeleteLogAsync(
            string fileName)
        {
            ShareDirectoryClient rootDirectory =
                shareClient.GetRootDirectoryClient();

            ShareFileClient fileClient =
                rootDirectory.GetFileClient(fileName);

            await fileClient.DeleteIfExistsAsync();
        }
    }
}