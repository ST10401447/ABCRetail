using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using ABCRetail_ClassLibrary;

namespace QueueFunction.Service
{
    public class QueueStorageService
    {
        private readonly QueueClient _queueClient;

        public QueueStorageService()
        {
            string? connectionString =
                Environment.GetEnvironmentVariable("AzureStorage");

            string? queueName =
                Environment.GetEnvironmentVariable("QueueName");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "AzureStorage is missing.");
            }

            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new InvalidOperationException(
                    "QueueName is missing.");
            }

            _queueClient = new QueueClient(
                connectionString,
                queueName);

            _queueClient.CreateIfNotExists();
        }


        // SEND MESSAGE
        public async Task SendMessageAsync(string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText))
            {
                throw new ArgumentException(
                    "Message cannot be empty.",
                    nameof(messageText));
            }

            await _queueClient.SendMessageAsync(messageText);
        }


        // GET / PEEK MESSAGES
        public async Task<List<QueueMessageResult>> PeekMessagesAsync()
        {
            List<QueueMessageResult> messages =
                new List<QueueMessageResult>();

            PeekedMessage[] peekedMessages =
                await _queueClient.PeekMessagesAsync(
                    maxMessages: 32);

            foreach (PeekedMessage msg in peekedMessages)
            {
                messages.Add(
                    new QueueMessageResult
                    {
                        MessageId = msg.MessageId,
                        MessageText = msg.MessageText,
                        InsertedOn = msg.InsertedOn
                    });
            }

            return messages;
        }


        // FIND A REAL MESSAGE
        private async Task<QueueMessage?> FindMessageAsync(
            string messageId)
        {
            QueueMessage[] receivedMessages =
                (await _queueClient.ReceiveMessagesAsync(
                    maxMessages: 32,
                    visibilityTimeout: TimeSpan.FromSeconds(30))).Value;

            QueueMessage? target = null;

            foreach (QueueMessage msg in receivedMessages)
            {
                if (msg.MessageId == messageId)
                {
                    target = msg;
                }
                else
                {
                    // Make other messages visible again
                    await _queueClient.UpdateMessageAsync(
                        msg.MessageId,
                        msg.PopReceipt,
                        msg.MessageText,
                        visibilityTimeout:
                            TimeSpan.FromSeconds(0));
                }
            }

            return target;
        }


        // UPDATE MESSAGE
        public async Task UpdateMessageAsync(
            string messageId,
            string newMessageText)
        {
            if (string.IsNullOrWhiteSpace(messageId))
            {
                throw new ArgumentException(
                    "Message ID cannot be empty.",
                    nameof(messageId));
            }

            if (string.IsNullOrWhiteSpace(newMessageText))
            {
                throw new ArgumentException(
                    "Message text cannot be empty.",
                    nameof(newMessageText));
            }

            QueueMessage? message =
                await FindMessageAsync(messageId);

            if (message == null)
            {
                throw new KeyNotFoundException(
                    "Message not found.");
            }

            await _queueClient.UpdateMessageAsync(
                message.MessageId,
                message.PopReceipt,
                newMessageText,
                visibilityTimeout:
                    TimeSpan.FromSeconds(0));
        }


        // DELETE MESSAGE
        public async Task DeleteMessageAsync(
            string messageId)
        {
            if (string.IsNullOrWhiteSpace(messageId))
            {
                throw new ArgumentException(
                    "Message ID cannot be empty.",
                    nameof(messageId));
            }

            QueueMessage? message =
                await FindMessageAsync(messageId);

            if (message == null)
            {
                throw new KeyNotFoundException(
                    "Message not found.");
            }

            await _queueClient.DeleteMessageAsync(
                message.MessageId,
                message.PopReceipt);
        }
    }
}