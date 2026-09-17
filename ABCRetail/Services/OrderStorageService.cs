using ABCRetail.Models;
using System.Net;
using System.Net.Http.Json;

namespace ABCRetail.Services
{
    public class OrderTableService
    {
        private readonly HttpClient _httpClient;

        public OrderTableService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _httpClient.BaseAddress =
                new Uri("http://localhost:7223/api/");
        }

        // CREATE
        // POST: http://localhost:7223/api/orders
        public async Task AddOrderAsync(OrderEntity order)
        {
            HttpResponseMessage response =
                await _httpClient.PostAsJsonAsync(
                    "orders",
                    order);

            response.EnsureSuccessStatusCode();
        }

        // READ ALL
        // GET: http://localhost:7223/api/orders
        public async Task<List<OrderEntity>> GetAllOrdersAsync()
        {
            HttpResponseMessage response =
                await _httpClient.GetAsync("orders");

            response.EnsureSuccessStatusCode();

            List<OrderEntity>? orders =
                await response.Content.ReadFromJsonAsync<List<OrderEntity>>();

            return orders ?? new List<OrderEntity>();
        }

        // READ ONE
        // GET: http://localhost:7223/api/orders/{partitionKey}/{rowKey}
        public async Task<OrderEntity?> GetOrderAsync(
            string partitionKey,
            string rowKey)
        {
            HttpResponseMessage response =
                await _httpClient.GetAsync(
                    $"orders/{Uri.EscapeDataString(partitionKey)}/{Uri.EscapeDataString(rowKey)}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<OrderEntity>();
        }

        // UPDATE
        // PUT: http://localhost:7223/api/orders/{partitionKey}/{rowKey}
        public async Task UpdateOrderAsync(OrderEntity order)
        {
            HttpResponseMessage response =
                await _httpClient.PutAsJsonAsync(
                    $"orders/{Uri.EscapeDataString(order.PartitionKey ?? "")}/{Uri.EscapeDataString(order.RowKey ?? "")}",
                    order);

            response.EnsureSuccessStatusCode();
        }

        // DELETE
        // DELETE: http://localhost:7223/api/orders/{partitionKey}/{rowKey}
        public async Task DeleteOrderAsync(
            string partitionKey,
            string rowKey)
        {
            HttpResponseMessage response =
                await _httpClient.DeleteAsync(
                    $"orders/{Uri.EscapeDataString(partitionKey)}/{Uri.EscapeDataString(rowKey)}");

            response.EnsureSuccessStatusCode();
        }
    }
}