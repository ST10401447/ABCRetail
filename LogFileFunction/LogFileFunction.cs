
using Azure;
using LogFileFunction.Service;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace LogFileFunction
{
    public class LogFileFunction
    {
        private readonly FileStorageService _fileStorageService;
        private readonly ILogger<LogFileFunction> _logger;

        public LogFileFunction(
            FileStorageService fileStorageService,
            ILogger<LogFileFunction> logger)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        // CREATE
        // POST: /api/files?fileName=test.txt
        [Function("UploadFile")]
        public async Task<HttpResponseData> UploadFile(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "post",
                Route = "files")]
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

                await _fileStorageService.UploadFileAsync(
                    req.Body,
                    fileName);

                var success =
                    req.CreateResponse(HttpStatusCode.Created);

                await success.WriteAsJsonAsync(new
                {
                    message = "File uploaded successfully.",
                    fileName = fileName
                });

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file.");

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "Error uploading file.");

                return response;
            }
        }


        // READ
        // GET: /api/files
        [Function("GetAllFiles")]
        public async Task<HttpResponseData> GetAllFiles(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "files")]
            HttpRequestData req)
        {
            try
            {
                var files =
                    await _fileStorageService.GetAllLogsAsync();

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(files);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving files.");

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "Error retrieving files.");

                return response;
            }
        }


        // READ
        // GET: /api/files/{fileName}
        [Function("DownloadFile")]
        public async Task<HttpResponseData> DownloadFile(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "get",
                Route = "files/{fileName}")]
            HttpRequestData req,
            string fileName)
        {
            try
            {
                Stream fileStream =
                    await _fileStorageService.DownloadFileAsync(
                        fileName);

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                response.Headers.Add(
                    "Content-Type",
                    "application/octet-stream");

                response.Headers.Add(
                    "Content-Disposition",
                    $"attachment; filename=\"{fileName}\"");

                await fileStream.CopyToAsync(response.Body);

                return response;
            }
            catch (RequestFailedException ex)
                when (ex.Status == 404)
            {
                var response =
                    req.CreateResponse(HttpStatusCode.NotFound);

                await response.WriteStringAsync(
                    "File not found.");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error downloading file {FileName}.",
                    fileName);

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "Error downloading file.");

                return response;
            }
        }


        // UPDATE
        // PUT: /api/files/{fileName}
        [Function("UpdateFile")]
        public async Task<HttpResponseData> UpdateFile(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "put",
                Route = "files/{fileName}")]
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

                await _fileStorageService.UploadFileAsync(
                    req.Body,
                    fileName);

                var success =
                    req.CreateResponse(HttpStatusCode.OK);

                await success.WriteAsJsonAsync(new
                {
                    message = "File updated successfully.",
                    fileName = fileName
                });

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating file {FileName}.",
                    fileName);

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "Error updating file.");

                return response;
            }
        }


        // DELETE
        // DELETE: /api/files/{fileName}
        [Function("DeleteFile")]
        public async Task<HttpResponseData> DeleteFile(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "delete",
                Route = "files/{fileName}")]
            HttpRequestData req,
            string fileName)
        {
            try
            {
                await _fileStorageService.DeleteLogAsync(
                    fileName);

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(new
                {
                    message = "File deleted successfully.",
                    fileName = fileName
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting file {FileName}.",
                    fileName);

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "Error deleting file.");

                return response;
            }
        }
    }
}

