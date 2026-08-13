namespace ABCRetail.Models
{
    public class QueueMessageViewModel
    {
        public string MessageId { get; set; }
        public string PopReceipt { get; set; }
        public string MessageText { get; set; }
        public DateTimeOffset? InsertedOn { get; set; }
        public DateTimeOffset? ExpiresOn { get; set; }
    }
}