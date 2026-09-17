
using BlobFunction.Service;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace BlobFunction
{
    public class ProductImageFunction
    {
        private readonly BlobStorageService _blobStorageService;
        private readonly ILogger<ProductImageFunction> _logger;

        public ProductImageFunction(
            BlobStorageService blobStorageService,
            ILogger<ProductImageFunction> logger)
        {
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        // CREATE - Upload product image
        // POST: /api/images?fileName=product.jpg
        [Function("UploadProductImage")]
        public async Task<HttpResponseData> UploadProductImage(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "post",
                Route = "images")]
            HttpRequestData req)
        {
            try
            {
                string? fileName =
                    System.Web.HttpUtility.ParseQueryString(
                        req.Url.Query)["fileName"];

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    var response =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await response.WriteStringAsync(
                        "Please provide a fileName.");

                    return response;
                }

                if (req.Body == null)
                {
                    var response =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await response.WriteStringAsync(
                        "No image was provided.");

                    return response;
                }

                await _blobStorageService.UploadImageAsync(
                    req.Body,
                    fileName);

                var success =
                    req.CreateResponse(HttpStatusCode.Created);

                await success.WriteAsJsonAsync(new
                {
                    message = "Product image uploaded successfully.",
                    fileName = fileName
                });

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error uploading product image.");

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "Error uploading product image.");

                return response;
            }
        }

        // READ - Get all product images
        // GET: /api/images
        [Function("GetAllProductImages")]
        public async Task<HttpResponseData> GetAllProductImages(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "get",
                Route = "images")]
            HttpRequestData req)
        {
            try
            {
                List<ABCRetail_ClassLibrary.ProductImage> images =
                    await _blobStorageService.GetAllImagesAsync();

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(images);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving product images.");

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "Error retrieving product images.");

                return response;
            }
        }

        // DELETE - Delete product image
        // DELETE: /api/images/{fileName}
        [Function("DeleteProductImage")]
        public async Task<HttpResponseData> DeleteProductImage(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "delete",
                Route = "images/{fileName}")]
            HttpRequestData req,
            string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    var response =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await response.WriteStringAsync(
                        "Please provide a fileName.");

                    return response;
                }

                await _blobStorageService.DeleteImageAsync(
                    fileName);

                var success =
                    req.CreateResponse(HttpStatusCode.OK);

                await success.WriteAsJsonAsync(new
                {
                    message = "Product image deleted successfully.",
                    fileName = fileName
                });

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting product image {FileName}.",
                    fileName);

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "Error deleting product image.");

                return response;
            }
        }
    }
}

