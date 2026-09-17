using ABCRetail.Models;
using System.Net;
using System.Net.Http.Json;

namespace ABCRetail.Services
{
    public class TableStorageService
    {
        private readonly HttpClient _httpClient;

        public TableStorageService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress =
                new Uri("http://localhost:7191/api/");
        }

        // POST /api/customers
        // Adds a new customer
        public async Task AddCustomerAsync(CustomerProfile customer)
        {
            HttpResponseMessage response =
                await _httpClient.PostAsJsonAsync(
                    "customers",
                    customer);

            response.EnsureSuccessStatusCode();
        }

        // GET /api/customers
        // Gets all customers
        public async Task<List<CustomerProfile>> GetAllCustomersAsync()
        {
            HttpResponseMessage response =
                await _httpClient.GetAsync("customers");

            response.EnsureSuccessStatusCode();

            var customers =
                await response.Content
                    .ReadFromJsonAsync<List<CustomerProfile>>();

            return customers ?? new List<CustomerProfile>();
        }

        // GET /api/customers/{partitionKey}/{rowKey}
        // Gets a single customer
        public async Task<CustomerProfile?> GetCustomerAsync(
            string partitionKey,
            string rowKey)
        {
            string url =
                $"customers/{Uri.EscapeDataString(partitionKey)}/{Uri.EscapeDataString(rowKey)}";

            HttpResponseMessage response =
                await _httpClient.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<CustomerProfile>();
        }

        // PUT /api/customers/{partitionKey}/{rowKey}
        // Updates a customer
        public async Task UpdateCustomerAsync(
            CustomerProfile customer)
        {
            string url =
                $"customers/{Uri.EscapeDataString(customer.PartitionKey)}/{Uri.EscapeDataString(customer.RowKey)}";

            HttpResponseMessage response =
                await _httpClient.PutAsJsonAsync(
                    url,
                    customer);

            response.EnsureSuccessStatusCode();
        }

        // DELETE /api/customers/{partitionKey}/{rowKey}
        // Deletes a customer
        public async Task DeleteCustomerAsync(
            string partitionKey,
            string rowKey)
        {
            string url =
                $"customers/{Uri.EscapeDataString(partitionKey)}/{Uri.EscapeDataString(rowKey)}";

            HttpResponseMessage response =
                await _httpClient.DeleteAsync(url);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }

            response.EnsureSuccessStatusCode();
        }
    }
}