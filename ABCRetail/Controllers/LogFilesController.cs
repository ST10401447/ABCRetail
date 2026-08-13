using Microsoft.AspNetCore.Mvc;
using ABCRetail.Models;
using ABCRetail.Services;

namespace ABCRetail.Controllers
{
    public class LogFilesController : Controller
    {
        private readonly FileStorageService fileStorageService;

        public LogFilesController(FileStorageService fileStorageService)
        {
            this.fileStorageService = fileStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var logs = await fileStorageService.GetAllLogsAsync();
            return View(logs);
        }

        // Upload file
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                using (Stream stream = file.OpenReadStream())
                {
                    await fileStorageService.UploadFileAsync(stream, file.FileName);
                }
            }
            return RedirectToAction("Index");
        }

        // Download a file
        public async Task<IActionResult> Download(string fileName)
        {
            Stream fileStream = await fileStorageService.DownloadFileAsync(fileName);
            return File(fileStream, "application/octet-stream", fileName);
        }

        // GET: Shows the delete confirmation page
        public async Task<IActionResult> Delete(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return NotFound();
            }

            var logs = await fileStorageService.GetAllLogsAsync();
            LogFile logFile = null;

            foreach (var log in logs)
            {
                if (log.FileName == fileName)
                {
                    logFile = log;
                    break;
                }
            }

            if (logFile == null)
            {
                return NotFound();
            }

            return View(logFile);
        }

        // POST: Actually deletes the file
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                await fileStorageService.DeleteLogAsync(fileName);
            }
            return RedirectToAction("Index");
        }
    }
}