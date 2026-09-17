using ABCRetail.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace ABCRetail.Services
{
    public class ProductTableService
    {
        private readonly HttpClient _httpClient;

        public ProductTableService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _httpClient.BaseAddress =
                new Uri("http://localhost:7020/api/");
        }

        // CREATE - Add a new product
        // POST: http://localhost:7020/api/products
        public async Task AddProductAsync(ProductEntity product)
        {
            HttpResponseMessage response =
                await _httpClient.PostAsJsonAsync(
                    "products",
                    product);

            response.EnsureSuccessStatusCode();
        }

        // READ - Get all products
        // GET: http://localhost:7020/api/products
        public async Task<List<ProductEntity>> GetAllProductsAsync()
        {
            HttpResponseMessage response =
                await _httpClient.GetAsync("products");

            response.EnsureSuccessStatusCode();

            string json =
                await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<ProductEntity>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
                ?? new List<ProductEntity>();
        }

        // READ - Get one product
        // GET: http://localhost:7020/api/products/{partitionKey}/{rowKey}
        public async Task<ProductEntity?> GetProductAsync(
            string partitionKey,
            string rowKey)
        {
            HttpResponseMessage response =
                await _httpClient.GetAsync(
                    $"products/{Uri.EscapeDataString(partitionKey)}/{Uri.EscapeDataString(rowKey)}");

            if (response.StatusCode ==
                System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            string json =
                await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ProductEntity>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        // UPDATE - Update a product
        // PUT: http://localhost:7020/api/products/{partitionKey}/{rowKey}
        public async Task UpdateProductAsync(ProductEntity product)
        {
            if (string.IsNullOrWhiteSpace(product.PartitionKey))
            {
                throw new ArgumentException(
                    "PartitionKey is required.");
            }

            if (string.IsNullOrWhiteSpace(product.RowKey))
            {
                throw new ArgumentException(
                    "RowKey is required.");
            }

            HttpResponseMessage response =
                await _httpClient.PutAsJsonAsync(
                    $"products/{Uri.EscapeDataString(product.PartitionKey)}/{Uri.EscapeDataString(product.RowKey)}",
                    product);

            response.EnsureSuccessStatusCode();
        }

        // DELETE - Delete a product
        // DELETE: http://localhost:7020/api/products/{partitionKey}/{rowKey}
        public async Task DeleteProductAsync(
            string partitionKey,
            string rowKey)
        {
            HttpResponseMessage response =
                await _httpClient.DeleteAsync(
                    $"products/{Uri.EscapeDataString(partitionKey)}/{Uri.EscapeDataString(rowKey)}");

            if (response.StatusCode ==
                System.Net.HttpStatusCode.NotFound)
            {
                return;
            }

            response.EnsureSuccessStatusCode();
        }
    }
}