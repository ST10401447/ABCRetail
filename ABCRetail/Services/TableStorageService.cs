using ABCRetail.Models;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;

namespace ABCRetail.Services
{
    public class TableStorageService
    {
        private readonly TableClient tableClient;

        public TableStorageService(TableServiceClient tableServiceClient, IConfiguration configuration)
        {
            string tableName = configuration["AzureStorage:TableName"];
            tableClient = tableServiceClient.GetTableClient(tableName);
            tableClient.CreateIfNotExists();
        }

        // Adds a new customer
        public async Task AddCustomerAsync(CustomerProfile customer)
        {
            customer.PartitionKey = "Customer";
            customer.RowKey = Guid.NewGuid().ToString();
            await tableClient.AddEntityAsync(customer);
        }

        // Gets all customers
        public async Task<List<CustomerProfile>> GetAllCustomersAsync()
        {
            var customers = new List<CustomerProfile>();

            await foreach (CustomerProfile entity in tableClient.QueryAsync<CustomerProfile>())
            {
                customers.Add(entity);
            }

            return customers;
        }

        // Gets a single customer
        public async Task<CustomerProfile?> GetCustomerAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await tableClient.GetEntityAsync<CustomerProfile>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        // Updates a customer
        public async Task UpdateCustomerAsync(CustomerProfile customer)
        {
            await tableClient.UpdateEntityAsync(customer, ETag.All, TableUpdateMode.Replace);
        }

        // Deletes a customer
        public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
        {
            await tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }
    }
}