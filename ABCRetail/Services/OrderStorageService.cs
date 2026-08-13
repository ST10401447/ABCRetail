using ABCRetail.Models;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;

namespace ABCRetail.Services
{
    public class OrderTableService
    {
        private readonly TableClient tableClient;

        // Constructor 
        public OrderTableService(TableServiceClient tableServiceClient, IConfiguration configuration)
        {
            string tableName = configuration["AzureStorage:OrderTableName"];
            tableClient = tableServiceClient.GetTableClient(tableName);
            tableClient.CreateIfNotExists();
        }

        // Adds a new order to Azure Table Storage
        public async Task AddOrderAsync(OrderEntity order)
        {
            order.PartitionKey = "Order";
            order.RowKey = Guid.NewGuid().ToString();
            await tableClient.AddEntityAsync(order);
        }

        // Gets all orders from the table
        public async Task<List<OrderEntity>> GetAllOrdersAsync()
        {
            List<OrderEntity> orders = new List<OrderEntity>();

            await foreach (OrderEntity entity in tableClient.QueryAsync<OrderEntity>())
            {
                orders.Add(entity);
            }

            return orders;
        }

        // Gets a single order by PartitionKey and RowKey
        public async Task<OrderEntity?> GetOrderAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await tableClient.GetEntityAsync<OrderEntity>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        // Updates an order in the table
        public async Task UpdateOrderAsync(OrderEntity order)
        {
            await tableClient.UpdateEntityAsync(order, ETag.All, TableUpdateMode.Replace);
        }

        // Deletes an order using PartitionKey and RowKey
        public async Task DeleteOrderAsync(string partitionKey, string rowKey)
        {
            await tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }
    }
}