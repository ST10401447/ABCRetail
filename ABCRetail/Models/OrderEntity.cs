using Azure;
using Azure.Data.Tables;

namespace ABCRetail.Models
{
    public class OrderEntity:ITableEntity
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string? ProductName { get; set; }
        public int? Price { get; set; }
        public int Quantity { get; set; } = 0;
    }
}
