using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Configuration;
using ABCRetail.Models;

namespace ABCRetail.Services
{
    public class QueueStorageService
    {
        private readonly QueueClient queueClient;

        public QueueStorageService(QueueServiceClient queueServiceClient, IConfiguration configuration)
        {
            string queueName = configuration["AzureStorage:QueueName"];
            queueClient = queueServiceClient.GetQueueClient(queueName);
            queueClient.CreateIfNotExists();
        }

        // Sends a message to the queue
        public async Task SendMessageAsync(string messageText)
        {
            await queueClient.SendMessageAsync(messageText);
        }

        // Shows messages without removing them
        public async Task<List<QueueMessageViewModel>> GetMessagesAsync()
        {
            var messages = new List<QueueMessageViewModel>();

            PeekedMessage[] peekedMessages = await queueClient.PeekMessagesAsync(maxMessages: 32);

            foreach (var msg in peekedMessages)
            {
                messages.Add(new QueueMessageViewModel
                {
                    MessageId = msg.MessageId,
                    MessageText = msg.MessageText,
                    InsertedOn = msg.InsertedOn,
                    PopReceipt = ""
                });
            }

            return messages;
        }

        // Finds the real message and puts all other messages back immediately
        private async Task<QueueMessage> FindMessageAsync(string messageId)
        {
            var response = await queueClient.ReceiveMessagesAsync(
                maxMessages: 32,
                visibilityTimeout: TimeSpan.FromSeconds(30));

            QueueMessage target = null;

            foreach (var msg in response.Value)
            {
                if (msg.MessageId == messageId)
                {
                    target = msg;
                }
                else
                {
                  // Put this message back so it does not disappear
                    await queueClient.UpdateMessageAsync(
                        msg.MessageId,
                        msg.PopReceipt,
                        msg.MessageText,
                        visibilityTimeout: TimeSpan.FromSeconds(0));
                }
            }

            return target;
        }

        // Update a message
        public async Task UpdateMessageAsync(string messageId, string newMessageText)
        {
            var message = await FindMessageAsync(messageId);

            if (message != null)
            {
                await queueClient.UpdateMessageAsync(
                    message.MessageId,
                    message.PopReceipt,
                    newMessageText,
                    visibilityTimeout: TimeSpan.FromSeconds(0));
            }
        }

        // Delete a message
        public async Task DeleteMessageAsync(string messageId)
        {
            var message = await FindMessageAsync(messageId);

            if (message != null)
            {
                await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
            }
        }
    }
}