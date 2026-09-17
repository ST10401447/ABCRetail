using ABCRetail_ClassLibrary;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductFunction.Service
{
    public class ProductTableService
    {
        private readonly TableClient tableClient;

        // Constructor 
        public ProductTableService( IConfiguration configuration)
        {
            string connectionString =Environment.GetEnvironmentVariable("AzureStorage");
            string tableName = Environment.GetEnvironmentVariable("ProductTableName");
            tableClient = new TableClient(connectionString, tableName);
            tableClient.CreateIfNotExists();
        }

        // Adds a new product to Azure Table Storage
        public async Task AddProductAsync(ProductEntity product)
        {
            product.PartitionKey = "Product";
            product.RowKey = Guid.NewGuid().ToString();

            await tableClient.AddEntityAsync(product);
        }

        // Gets all products from the table
        public async Task<List<ProductEntity>> GetAllProductsAsync()
        {
            List<ProductEntity> products = new List<ProductEntity>();

            await foreach (ProductEntity entity in tableClient.QueryAsync<ProductEntity>())
            {
                products.Add(entity);
            }

            return products;
        }

        // Updates a product in the table
        public async Task UpdateProductAsync(ProductEntity product)
        {
            // Use ETag.All so it doesn't fail when ETag is empty
            await tableClient.UpdateEntityAsync(product, ETag.All, TableUpdateMode.Replace);
        }
        public async Task<ProductEntity?> GetProductAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await tableClient.GetEntityAsync<ProductEntity>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }
        // Deletes a product using PartitionKey and RowKey
        public async Task DeleteProductAsync(string partitionKey, string rowKey)
        {
            await tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }
    }
}


