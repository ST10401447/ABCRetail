using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using QueueFunction.Service;
using System.Net;

namespace QueueFunction
{
    public class QueueFunction
    {
        private readonly QueueStorageService _queueStorageService;
        private readonly ILogger<QueueFunction> _logger;

        public QueueFunction(
            QueueStorageService queueStorageService,
            ILogger<QueueFunction> logger)
        {
            _queueStorageService = queueStorageService;
            _logger = logger;
        }

        // POST /api/queue
        [Function("SendMessage")]
        public async Task<HttpResponseData> SendMessage(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "post",
                Route = "queue")]
            HttpRequestData req)
        {
            try
            {
                string messageText =
                    await new StreamReader(req.Body).ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(messageText))
                {
                    var badResponse =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await badResponse.WriteStringAsync(
                        "Please provide a message.");

                    return badResponse;
                }

                await _queueStorageService.SendMessageAsync(messageText);

                var response =
                    req.CreateResponse(HttpStatusCode.Created);

                await response.WriteAsJsonAsync(new
                {
                    message = "Message sent successfully.",
                    messageText = messageText
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error sending queue message.");

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "Error sending queue message.");

                return response;
            }
        }


        // GET /api/queue
        [Function("GetMessages")]
        public async Task<HttpResponseData> GetMessages(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "get",
                Route = "queue")]
            HttpRequestData req)
        {
            try
            {
                var messages =
                    await _queueStorageService.PeekMessagesAsync();

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(messages);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving queue messages.");

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "Error retrieving queue messages.");

                return response;
            }
        }


        // PUT /api/queue/{messageId}
        [Function("UpdateMessage")]
        public async Task<HttpResponseData> UpdateMessage(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "put",
                Route = "queue/{messageId}")]
            HttpRequestData req,
            string messageId)
        {
            try
            {
                string newMessageText =
                    await new StreamReader(req.Body).ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(newMessageText))
                {
                    var badResponse =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await badResponse.WriteStringAsync(
                        "Please provide a new message.");

                    return badResponse;
                }

                await _queueStorageService.UpdateMessageAsync(
                    messageId,
                    newMessageText);

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(new
                {
                    message = "Message updated successfully.",
                    messageId = messageId,
                    messageText = newMessageText
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating queue message.");

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "Error updating queue message.");

                return response;
            }
        }


        // DELETE /api/queue/{messageId}
        [Function("DeleteMessage")]
        public async Task<HttpResponseData> DeleteMessage(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "delete",
                Route = "queue/{messageId}")]
            HttpRequestData req,
            string messageId)
        {
            try
            {
                await _queueStorageService.DeleteMessageAsync(
                    messageId);

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteStringAsync(
                    "Message deleted successfully.");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting queue message.");

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "Error deleting queue message.");

                return response;
            }
        }
    }
}