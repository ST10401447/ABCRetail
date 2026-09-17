using ABCRetail_ClassLibrary;
using OrderFunction.Service;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace OrderFunction
{
    public class OrderHttpFunction
    {
        private readonly OrderStorageService _orderStorageService;
        private readonly ILogger<OrderHttpFunction> _logger;

        public OrderHttpFunction(
            OrderStorageService orderStorageService,
            ILogger<OrderHttpFunction> logger)
        {
            _orderStorageService = orderStorageService;
            _logger = logger;
        }

        // POST: /api/orders
        // Creates a new order
        [Function("AddOrder")]
        public async Task<HttpResponseData> AddOrder(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "post",
                Route = "orders")]
            HttpRequestData req)
        {
            try
            {
                string requestBody =
                    await new StreamReader(req.Body).ReadToEndAsync();

                OrderEntity? order =
                    JsonSerializer.Deserialize<OrderEntity>(
                        requestBody,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (order == null)
                {
                    var badRequest =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await badRequest.WriteStringAsync(
                        "Invalid order data.");

                    return badRequest;
                }

                await _orderStorageService.AddOrderAsync(order);

                var response =
                    req.CreateResponse(HttpStatusCode.Created);

                await response.WriteAsJsonAsync(new
                {
                    message = "Order created successfully.",
                    order
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error adding order.");

                var errorResponse =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await errorResponse.WriteStringAsync(
                    "An error occurred while creating the order.");

                return errorResponse;
            }
        }

        // GET: /api/orders
        // Gets all orders
        [Function("GetAllOrders")]
        public async Task<HttpResponseData> GetAllOrders(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "get",
                Route = "orders")]
            HttpRequestData req)
        {
            try
            {
                List<OrderEntity> orders =
                    await _orderStorageService.GetAllOrdersAsync();

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(orders);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving orders.");

                var errorResponse =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await errorResponse.WriteStringAsync(
                    "An error occurred while retrieving orders.");

                return errorResponse;
            }
        }

        // GET: /api/orders/{partitionKey}/{rowKey}
        // Gets a single order
        [Function("GetOrder")]
        public async Task<HttpResponseData> GetOrder(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "get",
                Route = "orders/{partitionKey}/{rowKey}")]
            HttpRequestData req,
            string partitionKey,
            string rowKey)
        {
            try
            {
                OrderEntity? order =
                    await _orderStorageService.GetOrderAsync(
                        partitionKey,
                        rowKey);

                if (order == null)
                {
                    var notFound =
                        req.CreateResponse(
                            HttpStatusCode.NotFound);

                    await notFound.WriteStringAsync(
                        "Order not found.");

                    return notFound;
                }

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(order);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving order.");

                var errorResponse =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await errorResponse.WriteStringAsync(
                    "An error occurred while retrieving the order.");

                return errorResponse;
            }
        }

        // PUT: /api/orders/{partitionKey}/{rowKey}
        // Updates an existing order
        [Function("UpdateOrder")]
        public async Task<HttpResponseData> UpdateOrder(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "put",
                Route = "orders/{partitionKey}/{rowKey}")]
            HttpRequestData req,
            string partitionKey,
            string rowKey)
        {
            try
            {
                string requestBody =
                    await new StreamReader(req.Body).ReadToEndAsync();

                OrderEntity? order =
                    JsonSerializer.Deserialize<OrderEntity>(
                        requestBody,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (order == null)
                {
                    var badRequest =
                        req.CreateResponse(
                            HttpStatusCode.BadRequest);

                    await badRequest.WriteStringAsync(
                        "Invalid order data.");

                    return badRequest;
                }

                // Make sure the URL keys are used
                order.PartitionKey = partitionKey;
                order.RowKey = rowKey;

                // Check if order exists
                OrderEntity? existingOrder =
                    await _orderStorageService.GetOrderAsync(
                        partitionKey,
                        rowKey);

                if (existingOrder == null)
                {
                    var notFound =
                        req.CreateResponse(
                            HttpStatusCode.NotFound);

                    await notFound.WriteStringAsync(
                        "Order not found.");

                    return notFound;
                }

                await _orderStorageService.UpdateOrderAsync(order);

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(new
                {
                    message = "Order updated successfully.",
                    order
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating order.");

                var errorResponse =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await errorResponse.WriteStringAsync(
                    "An error occurred while updating the order.");

                return errorResponse;
            }
        }

        // DELETE: /api/orders/{partitionKey}/{rowKey}
        // Deletes an order
        [Function("DeleteOrder")]
        public async Task<HttpResponseData> DeleteOrder(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "delete",
                Route = "orders/{partitionKey}/{rowKey}")]
            HttpRequestData req,
            string partitionKey,
            string rowKey)
        {
            try
            {
                // Check if order exists
                OrderEntity? existingOrder =
                    await _orderStorageService.GetOrderAsync(
                        partitionKey,
                        rowKey);

                if (existingOrder == null)
                {
                    var notFound =
                        req.CreateResponse(
                            HttpStatusCode.NotFound);

                    await notFound.WriteStringAsync(
                        "Order not found.");

                    return notFound;
                }

                await _orderStorageService.DeleteOrderAsync(
                    partitionKey,
                    rowKey);

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(new
                {
                    message = "Order deleted successfully.",
                    partitionKey,
                    rowKey
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting order.");

                var errorResponse =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await errorResponse.WriteStringAsync(
                    "An error occurred while deleting the order.");

                return errorResponse;
            }
        }
    }
}