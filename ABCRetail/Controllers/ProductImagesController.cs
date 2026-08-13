using Microsoft.AspNetCore.Mvc;
using ABCRetail.Services;
using ABCRetail.Models;

namespace ABCRetail.Controllers
{
    public class ProductImagesController : Controller
    {
        private readonly BlobStorageService blobStorageService;
        private readonly QueueStorageService queueStorageService;

        public ProductImagesController(BlobStorageService blobStorageService, QueueStorageService queueStorageService)
        {
            this.blobStorageService = blobStorageService;
            this.queueStorageService = queueStorageService;
        }

        // Shows all product images
        public async Task<IActionResult> Index()
        {
            var images = await blobStorageService.GetAllImagesAsync();
            return View(images);
        }

        // Uploads a new image
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                using (Stream stream = file.OpenReadStream())
                {
                    await blobStorageService.UploadImageAsync(stream, file.FileName);
                }
                await queueStorageService.SendMessageAsync("Processing order: uploaded image " + file.FileName);
            }
            return RedirectToAction("Index");
        }

        // Replaces an existing image
        [HttpPost]
        public async Task<IActionResult> Replace(string oldFileName, IFormFile newFile)
        {
            if (newFile != null && newFile.Length > 0)
            {
                await blobStorageService.DeleteImageAsync(oldFileName);

                using (Stream stream = newFile.OpenReadStream())
                {
                    await blobStorageService.UploadImageAsync(stream, newFile.FileName);
                }
                await queueStorageService.SendMessageAsync("Processing order: replaced image " + oldFileName);
            }
            return RedirectToAction("Index");
        }

        // GET: Shows the delete confirmation page
        public async Task<IActionResult> Delete(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return NotFound();

            var images = await blobStorageService.GetAllImagesAsync();
            var image = images.FirstOrDefault(i => i.FileName == fileName);

            if (image == null)
                return NotFound();

            return View(image);
        }

        // POST: Actually deletes the image
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                await blobStorageService.DeleteImageAsync(fileName);
                await queueStorageService.SendMessageAsync("Processing order: deleted image " + fileName);
            }
            return RedirectToAction("Index");
        }
    }
}