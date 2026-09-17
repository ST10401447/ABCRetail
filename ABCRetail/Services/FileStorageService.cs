
using ABCRetail.Models;
using ABCRetail.Services;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ABCRetail.Services
{
    public class FileStorageService
    {
        private readonly HttpClient _httpClient;

        public FileStorageService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress =
                new Uri("http://localhost:7015/api/");
        }

        // CREATE - Upload a file
        public async Task<bool> UploadFileAsync(
            Stream fileStream,
            string fileName)
        {
            using var content = new StreamContent(fileStream);

            content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");

            HttpResponseMessage response =
                await _httpClient.PostAsync(
                    $"files?fileName={Uri.EscapeDataString(fileName)}",
                    content);

            return response.IsSuccessStatusCode;
        }

        // CREATE - Upload text log
        public async Task<bool> UploadLogAsync(
            string fileName,
            string content)
        {
            using var stream =
                new MemoryStream(Encoding.UTF8.GetBytes(content));

            return await UploadFileAsync(stream, fileName);
        }

        // READ - Get all files
        public async Task<List<LogFile>> GetAllLogsAsync()
        {
            HttpResponseMessage response =
                await _httpClient.GetAsync("files");

            if (!response.IsSuccessStatusCode)
            {
                return new List<LogFile>();
            }

            string json =
                await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<LogFile>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
                ?? new List<LogFile>();
        }

        // READ - Download a file
        public async Task<Stream> DownloadFileAsync(
            string fileName)
        {
            HttpResponseMessage response =
                await _httpClient.GetAsync(
                    $"files/{Uri.EscapeDataString(fileName)}");

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }

        // UPDATE - Replace an existing file
        public async Task<bool> UpdateFileAsync(
            Stream fileStream,
            string fileName)
        {
            using var content =
                new StreamContent(fileStream);

            content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");

            HttpResponseMessage response =
                await _httpClient.PutAsync(
                    $"files/{Uri.EscapeDataString(fileName)}",
                    content);

            return response.IsSuccessStatusCode;
        }

        // DELETE - Delete a file
        public async Task<bool> DeleteLogAsync(
            string fileName)
        {
            HttpResponseMessage response =
                await _httpClient.DeleteAsync(
                    $"files/{Uri.EscapeDataString(fileName)}");

            return response.IsSuccessStatusCode;
        }
    }
}
