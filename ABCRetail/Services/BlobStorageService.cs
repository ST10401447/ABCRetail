using ABCRetail.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ABCRetail.Services
{
    public class BlobStorageService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public BlobStorageService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            _httpClient.BaseAddress =
                new Uri("http://localhost:7275/api/");
        }

        // CREATE - Upload image
        // POST: http://localhost:7275/api/images?fileName=image.jpg
        public async Task UploadImageAsync(
            Stream fileStream,
            string fileName)
        {
            using var content =
                new StreamContent(fileStream);

            content.Headers.ContentType =
                new MediaTypeHeaderValue(
                    "application/octet-stream");

            HttpResponseMessage response =
                await _httpClient.PostAsync(
                    $"images?fileName={Uri.EscapeDataString(fileName)}",
                    content);

            response.EnsureSuccessStatusCode();
        }

        // CREATE - Upload image and return URL
        public async Task<string> UploadAndReturnUrl(
            IFormFile file,
            string? folder = null)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException(
                    "No file provided");
            }

            var allowedExtensions =
                new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".gif",
                    ".webp"
                };

            var extension =
                Path.GetExtension(
                    file.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException(
                    "Invalid file type");
            }

            var fileName =
                $"{Guid.NewGuid():N}{extension}";

            var blobName =
                string.IsNullOrEmpty(folder)
                    ? fileName
                    : $"{folder.TrimEnd('/')}/{fileName}";

            await using var stream =
                file.OpenReadStream();

            using var content =
                new StreamContent(stream);

            content.Headers.ContentType =
                new MediaTypeHeaderValue(
                    string.IsNullOrWhiteSpace(file.ContentType)
                        ? "application/octet-stream"
                        : file.ContentType);

            HttpResponseMessage response =
                await _httpClient.PostAsync(
                    $"images?fileName={Uri.EscapeDataString(blobName)}",
                    content);

            response.EnsureSuccessStatusCode();

            // If the Function returns a URL, use it.
            string json =
                await response.Content.ReadAsStringAsync();

            try
            {
                using JsonDocument document =
                    JsonDocument.Parse(json);

                if (document.RootElement.TryGetProperty(
                    "url",
                    out JsonElement urlElement))
                {
                    string? url =
                        urlElement.GetString();

                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        return url;
                    }
                }
            }
            catch
            {
                // Continue and build the URL below.
            }

            // Build the public Blob URL if the Function
            // does not return one.
            string? blobServiceUrl =
                _configuration[
                    "AzureStorage:BlobServiceUrl"];

            string? containerName =
                _configuration[
                    "AzureStorage:BlobContainerName"];

            if (string.IsNullOrWhiteSpace(blobServiceUrl))
            {
                throw new InvalidOperationException(
                    "AzureStorage:BlobServiceUrl is missing from appsettings.json.");
            }

            if (string.IsNullOrWhiteSpace(containerName))
            {
                throw new InvalidOperationException(
                    "AzureStorage:BlobContainerName is missing from appsettings.json.");
            }

            return
                $"{blobServiceUrl.TrimEnd('/')}/" +
                $"{containerName}/" +
                $"{blobName}";
        }

        // READ - Get all images
        // GET: http://localhost:7275/api/images
        public async Task<List<ProductImage>> GetAllImagesAsync()
        {
            HttpResponseMessage response =
                await _httpClient.GetAsync("images");

            response.EnsureSuccessStatusCode();

            string json =
                await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<
                List<ProductImage>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    })
                ?? new List<ProductImage>();
        }

        // DELETE - Delete image
        // DELETE: http://localhost:7275/api/images/{fileName}
        public async Task DeleteImageAsync(
            string fileName)
        {
            HttpResponseMessage response =
                await _httpClient.DeleteAsync(
                    $"images/{Uri.EscapeDataString(fileName)}");

            response.EnsureSuccessStatusCode();
        }
    }
}