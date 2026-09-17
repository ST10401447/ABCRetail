using ABCRetail.Models;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace ABCRetail.Services
{
    public class QueueStorageService
    {
        private readonly HttpClient _httpClient;

        public QueueStorageService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _httpClient.BaseAddress =
                new Uri("http://localhost:7027/api/");
        }

        // POST /api/queue
        // Sends a message
        public async Task SendMessageAsync(string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText))
            {
                throw new ArgumentException(
                    "Message cannot be empty.",
                    nameof(messageText));
            }

            using var content =
                new StringContent(
                    messageText,
                    Encoding.UTF8,
                    "text/plain");

            HttpResponseMessage response =
                await _httpClient.PostAsync(
                    "queue",
                    content);

            response.EnsureSuccessStatusCode();
        }


        // GET /api/queue
        // Gets all messages
        public async Task<List<QueueMessageViewModel>> GetMessagesAsync()
        {
            HttpResponseMessage response =
                await _httpClient.GetAsync("queue");

            response.EnsureSuccessStatusCode();

            var messages =
                await response.Content
                    .ReadFromJsonAsync<List<QueueMessageViewModel>>();

            return messages ??
                   new List<QueueMessageViewModel>();
        }


        // PUT /api/queue/{messageId}
        // Updates a message
        public async Task UpdateMessageAsync(
            string messageId,
            string newMessageText)
        {
            if (string.IsNullOrWhiteSpace(messageId))
            {
                throw new ArgumentException(
                    "Message ID cannot be empty.",
                    nameof(messageId));
            }

            if (string.IsNullOrWhiteSpace(newMessageText))
            {
                throw new ArgumentException(
                    "Message cannot be empty.",
                    nameof(newMessageText));
            }

            using var content =
                new StringContent(
                    newMessageText,
                    Encoding.UTF8,
                    "text/plain");

            string url =
                $"queue/{Uri.EscapeDataString(messageId)}";

            HttpResponseMessage response =
                await _httpClient.PutAsync(
                    url,
                    content);

            response.EnsureSuccessStatusCode();
        }


        // DELETE /api/queue/{messageId}
        // Deletes a message
        public async Task DeleteMessageAsync(
            string messageId)
        {
            if (string.IsNullOrWhiteSpace(messageId))
            {
                throw new ArgumentException(
                    "Message ID cannot be empty.",
                    nameof(messageId));
            }

            string url =
                $"queue/{Uri.EscapeDataString(messageId)}";

            HttpResponseMessage response =
                await _httpClient.DeleteAsync(url);

            if (response.StatusCode ==
                HttpStatusCode.NotFound)
            {
                return;
            }

            response.EnsureSuccessStatusCode();
        }
    }
}