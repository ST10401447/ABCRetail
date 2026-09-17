using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCRetail_ClassLibrary
{
    public class OrderMessageQueue
    {
        public string MessageId { get; set; }
        public string PopReceipt { get; set; }
        public string MessageText { get; set; }
        public DateTimeOffset? InsertedOn { get; set; }
        public DateTimeOffset? ExpiresOn { get; set; }
    }
}
