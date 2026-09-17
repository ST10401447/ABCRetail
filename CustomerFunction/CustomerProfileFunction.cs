using CustomerFunction.Models;
using CustomerFunction.Service;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace CustomerFunction
{
    public class CustomerProfileFunction
    {
        private readonly TableStorageService _tableStorageService;
        private readonly ILogger<CustomerProfileFunction> _logger;
    public CustomerProfileFunction(
        TableStorageService tableStorageService,
        ILogger<CustomerProfileFunction> logger)
        {
            _tableStorageService = tableStorageService;
            _logger = logger;
        }

        // POST: /api/customers
        [Function("AddCustomer")]
        public async Task<HttpResponseData> AddCustomer(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "customers")]
        HttpRequestData req)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                CustomerProfile? customer =
                    JsonSerializer.Deserialize<CustomerProfile>(
                        requestBody,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (customer == null)
                {
                    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequest.WriteStringAsync("Invalid customer data.");
                    return badRequest;
                }

                await _tableStorageService.AddCustomerAsync(customer);

                var response = req.CreateResponse(HttpStatusCode.Created);

                await response.WriteAsJsonAsync(new
                {
                    message = "Customer created successfully.",
                    customer
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding customer.");

                var errorResponse =
                    req.CreateResponse(HttpStatusCode.InternalServerError);

                await errorResponse.WriteStringAsync(
                    "An error occurred while creating the customer.");

                return errorResponse;
            }
        }


        // GET: /api/customers
        [Function("GetAllCustomers")]
        public async Task<HttpResponseData> GetAllCustomers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers")]
        HttpRequestData req)
        {
            try
            {
                var customers =
                    await _tableStorageService.GetAllCustomersAsync();

                var response = req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(customers);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers.");

                var errorResponse =
                    req.CreateResponse(HttpStatusCode.InternalServerError);

                await errorResponse.WriteStringAsync(
                    "An error occurred while retrieving customers.");

                return errorResponse;
            }
        }


        // GET: /api/customers/{partitionKey}/{rowKey}
        [Function("GetCustomer")]
        public async Task<HttpResponseData> GetCustomer(
            [HttpTrigger(
            AuthorizationLevel.Function,
            "get",
            Route = "customers/{partitionKey}/{rowKey}")]
        HttpRequestData req,
            string partitionKey,
            string rowKey)
        {
            try
            {
                var customer =
                    await _tableStorageService.GetCustomerAsync(
                        partitionKey,
                        rowKey);

                if (customer == null)
                {
                    var notFound =
                        req.CreateResponse(HttpStatusCode.NotFound);

                    await notFound.WriteStringAsync(
                        "Customer not found.");

                    return notFound;
                }

                var response = req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(customer);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer.");

                var errorResponse =
                    req.CreateResponse(HttpStatusCode.InternalServerError);

                await errorResponse.WriteStringAsync(
                    "An error occurred while retrieving the customer.");

                return errorResponse;
            }
        }


        // PUT: /api/customers/{partitionKey}/{rowKey}
        [Function("UpdateCustomer")]
        public async Task<HttpResponseData> UpdateCustomer(
            [HttpTrigger(
            AuthorizationLevel.Function,
            "put",
            Route = "customers/{partitionKey}/{rowKey}")]
        HttpRequestData req,
            string partitionKey,
            string rowKey)
        {
            try
            {
                string requestBody =
                    await new StreamReader(req.Body).ReadToEndAsync();

                CustomerProfile? customer =
                    JsonSerializer.Deserialize<CustomerProfile>(
                        requestBody,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (customer == null)
                {
                    var badRequest =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await badRequest.WriteStringAsync(
                        "Invalid customer data.");

                    return badRequest;
                }

                // Make sure the URL keys are used
                customer.PartitionKey = partitionKey;
                customer.RowKey = rowKey;

                // Check if customer exists first
                var existingCustomer =
                    await _tableStorageService.GetCustomerAsync(
                        partitionKey,
                        rowKey);

                if (existingCustomer == null)
                {
                    var notFound =
                        req.CreateResponse(HttpStatusCode.NotFound);

                    await notFound.WriteStringAsync(
                        "Customer not found.");

                    return notFound;
                }

                await _tableStorageService.UpdateCustomerAsync(customer);

                var response = req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(new
                {
                    message = "Customer updated successfully.",
                    customer
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer.");

                var errorResponse =
                    req.CreateResponse(HttpStatusCode.InternalServerError);

                await errorResponse.WriteStringAsync(
                    "An error occurred while updating the customer.");

                return errorResponse;
            }
        }


        // DELETE: /api/customers/{partitionKey}/{rowKey}
        [Function("DeleteCustomer")]
        public async Task<HttpResponseData> DeleteCustomer(
            [HttpTrigger(
            AuthorizationLevel.Function,
            "delete",
            Route = "customers/{partitionKey}/{rowKey}")]
        HttpRequestData req,
            string partitionKey,
            string rowKey)
        {
            try
            {
                var existingCustomer =
                    await _tableStorageService.GetCustomerAsync(
                        partitionKey,
                        rowKey);

                if (existingCustomer == null)
                {
                    var notFound =
                        req.CreateResponse(HttpStatusCode.NotFound);

                    await notFound.WriteStringAsync(
                        "Customer not found.");

                    return notFound;
                }

                await _tableStorageService.DeleteCustomerAsync(
                    partitionKey,
                    rowKey);

                var response = req.CreateResponse(HttpStatusCode.OK);

                await response.WriteStringAsync(
                    "Customer deleted successfully.");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer.");

                var errorResponse =
                    req.CreateResponse(HttpStatusCode.InternalServerError);

                await errorResponse.WriteStringAsync(
                    "An error occurred while deleting the customer.");

                return errorResponse;
            }
        }
    }
}
