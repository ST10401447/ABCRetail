using ABCRetail_ClassLibrary;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlobFunction.Service
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient containerClient;

        // Constructor 
        public BlobStorageService(IConfiguration configuration)
        {
            string connectionString = Environment.GetEnvironmentVariable("AzureStorage");
            string blobContainerName = Environment.GetEnvironmentVariable("BlobContainerName");

            containerClient = new BlobContainerClient(connectionString, blobContainerName);

            // PublicAccessType.Blob allows images to be viewed via URL
           // containerClient.CreateIfNotExists(PublicAccessType.Blob);
            //containerClient.SetAccessPolicy(PublicAccessType.Blob);
        }

        // Uploads an image to Azure Blob Storage
        public async Task UploadImageAsync(Stream fileStream, string fileName)
        {
            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream, overwrite: true);
        }

        public async Task<string> UploadAndReturnUrl(IFormFile file, string? folder = null)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file provided");

            //  validation
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type");

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var blobName = string.IsNullOrEmpty(folder)
                ? fileName
                : $"{folder.TrimEnd('/')}/{fileName}";

            var blobClient = containerClient.GetBlobClient(blobName);

            var httpHeaders = new BlobHttpHeaders
            {
                ContentType = file.ContentType
            };

            await using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, httpHeaders);

            return blobClient.Uri.ToString();
        }

        // Gets a list of all images in the container
        public async Task<List<ProductImage>> GetAllImagesAsync()
        {
            List<ProductImage> images = new List<ProductImage>();

            await foreach (BlobItem blob in containerClient.GetBlobsAsync())
            {
                BlobClient blobClient = containerClient.GetBlobClient(blob.Name);

                images.Add(new ProductImage
                {
                    FileName = blob.Name,
                    Url = blobClient.Uri.ToString()
                });
            }

            return images;
        }

        // Deletes an image from blob storage
        public async Task DeleteImageAsync(string fileName)
        {
            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}

