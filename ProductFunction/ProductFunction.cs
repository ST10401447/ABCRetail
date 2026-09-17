using ABCRetail_ClassLibrary;
using ProductFunction.Service;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace ProductFunction
{
    public class ProductHttpFunction
    {
        private readonly ProductTableService _productTableService;
        private readonly ILogger<ProductHttpFunction> _logger;

        public ProductHttpFunction(
            ProductTableService productTableService,
            ILogger<ProductHttpFunction> logger)
        {
            _productTableService = productTableService;
            _logger = logger;
        }

        // POST: /api/products
        // Creates a new product
        [Function("AddProduct")]
        public async Task<HttpResponseData> AddProduct(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "post",
                Route = "products")]
            HttpRequestData req)
        {
            try
            {
                string requestBody =
                    await new StreamReader(req.Body).ReadToEndAsync();

                ProductEntity? product =
                    JsonSerializer.Deserialize<ProductEntity>(
                        requestBody,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (product == null)
                {
                    var badRequest =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await badRequest.WriteStringAsync(
                        "Invalid product data.");

                    return badRequest;
                }

                await _productTableService.AddProductAsync(product);

                var response =
                    req.CreateResponse(HttpStatusCode.Created);

                await response.WriteAsJsonAsync(new
                {
                    message = "Product created successfully.",
                    product
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error adding product.");

                var errorResponse =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await errorResponse.WriteStringAsync(
                    "An error occurred while creating the product.");

                return errorResponse;
            }
        }


        // GET: /api/products
        // Gets all products
        [Function("GetAllProducts")]
        public async Task<HttpResponseData> GetAllProducts(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "get",
                Route = "products")]
            HttpRequestData req)
        {
            try
            {
                List<ProductEntity> products =
                    await _productTableService.GetAllProductsAsync();

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(products);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving products.");

                var errorResponse =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await errorResponse.WriteStringAsync(
                    "An error occurred while retrieving products.");

                return errorResponse;
            }
        }


        // GET: /api/products/{partitionKey}/{rowKey}
        // Gets a single product
        [Function("GetProduct")]
        public async Task<HttpResponseData> GetProduct(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "get",
                Route = "products/{partitionKey}/{rowKey}")]
            HttpRequestData req,
            string partitionKey,
            string rowKey)
        {
            try
            {
                ProductEntity? product =
                    await _productTableService.GetProductAsync(
                        partitionKey,
                        rowKey);

                if (product == null)
                {
                    var notFound =
                        req.CreateResponse(
                            HttpStatusCode.NotFound);

                    await notFound.WriteStringAsync(
                        "Product not found.");

                    return notFound;
                }

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(product);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving product.");

                var errorResponse =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await errorResponse.WriteStringAsync(
                    "An error occurred while retrieving the product.");

                return errorResponse;
            }
        }


        // PUT: /api/products/{partitionKey}/{rowKey}
        // Updates an existing product
        [Function("UpdateProduct")]
        public async Task<HttpResponseData> UpdateProduct(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "put",
                Route = "products/{partitionKey}/{rowKey}")]
            HttpRequestData req,
            string partitionKey,
            string rowKey)
        {
            try
            {
                string requestBody =
                    await new StreamReader(req.Body).ReadToEndAsync();

                ProductEntity? product =
                    JsonSerializer.Deserialize<ProductEntity>(
                        requestBody,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (product == null)
                {
                    var badRequest =
                        req.CreateResponse(
                            HttpStatusCode.BadRequest);

                    await badRequest.WriteStringAsync(
                        "Invalid product data.");

                    return badRequest;
                }

                // Make sure the URL keys are used
                product.PartitionKey = partitionKey;
                product.RowKey = rowKey;

                // Check if product exists
                ProductEntity? existingProduct =
                    await _productTableService.GetProductAsync(
                        partitionKey,
                        rowKey);

                if (existingProduct == null)
                {
                    var notFound =
                        req.CreateResponse(
                            HttpStatusCode.NotFound);

                    await notFound.WriteStringAsync(
                        "Product not found.");

                    return notFound;
                }

                await _productTableService.UpdateProductAsync(product);

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(new
                {
                    message = "Product updated successfully.",
                    product
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating product.");

                var errorResponse =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await errorResponse.WriteStringAsync(
                    "An error occurred while updating the product.");

                return errorResponse;
            }
        }


        // DELETE: /api/products/{partitionKey}/{rowKey}
        // Deletes a product
        [Function("DeleteProduct")]
        public async Task<HttpResponseData> DeleteProduct(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "delete",
                Route = "products/{partitionKey}/{rowKey}")]
            HttpRequestData req,
            string partitionKey,
            string rowKey)
        {
            try
            {
                ProductEntity? existingProduct =
                    await _productTableService.GetProductAsync(
                        partitionKey,
                        rowKey);

                if (existingProduct == null)
                {
                    var notFound =
                        req.CreateResponse(
                            HttpStatusCode.NotFound);

                    await notFound.WriteStringAsync(
                        "Product not found.");

                    return notFound;
                }

                await _productTableService.DeleteProductAsync(
                    partitionKey,
                    rowKey);

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(new
                {
                    message = "Product deleted successfully.",
                    partitionKey,
                    rowKey
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting product.");

                var errorResponse =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await errorResponse.WriteStringAsync(
                    "An error occurred while deleting the product.");

                return errorResponse;
            }
        }
    }
}