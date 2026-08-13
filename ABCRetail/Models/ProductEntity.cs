using Azure;
using Azure.Data.Tables;

namespace ABCRetail.Models
{
    public class ProductEntity:ITableEntity
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public int? Price { get; set; }
        public string? ProductImageUrl { get; set; }
        public int? StockQuantity { get; set; }
    
    }
}
