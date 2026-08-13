using Microsoft.AspNetCore.Mvc;
using ABCRetail.Models;
using ABCRetail.Services;

namespace ABCRetail.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductTableService _productTableService;
        private readonly BlobStorageService _blobStorageService;

        public ProductsController(
            ProductTableService productTableService,
            BlobStorageService blobStorageService)
        {
            _productTableService = productTableService;
            _blobStorageService = blobStorageService;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = await _productTableService.GetAllProductsAsync();
            return View(products);
        }

        // GET: Products/Details
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var product = await _productTableService.GetProductAsync(partitionKey, rowKey);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductEntity product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Generate keys
                    product.PartitionKey = "Product";
                    product.RowKey = Guid.NewGuid().ToString();

                    // Upload image 
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Create a unique file name
                        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";

                        using (var stream = imageFile.OpenReadStream())
                        {
                            product.ProductImageUrl = await _blobStorageService.UploadAndReturnUrl(imageFile,"Product Images");
                        }

                       
                    }

                    await _productTableService.AddProductAsync(product);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error saving product: " + ex.Message);
                }
            }

            return View(product);
        }

        // GET: Products/Edit
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var product = await _productTableService.GetProductAsync(partitionKey, rowKey);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductEntity product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Handle new image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Delete old image if it exists
                        if (!string.IsNullOrEmpty(product.ProductImageUrl))
                        {
                            // ProductImageUrl currently stores the file name 
                            string oldFileName = ExtractFileName(product.ProductImageUrl);
                            await _blobStorageService.DeleteImageAsync(oldFileName);
                        }

                        // Upload new image
                        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";

                        using (var stream = imageFile.OpenReadStream())
                        {
                           product.ProductImageUrl = await _blobStorageService.UploadAndReturnUrl(imageFile, "Product Images");
                        }

                    }

                    await _productTableService.UpdateProductAsync(product);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating product: " + ex.Message);
                }
            }

            return View(product);
        }

        // GET: Products/Delete
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var product = await _productTableService.GetProductAsync(partitionKey, rowKey);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return RedirectToAction(nameof(Index));
            }

            var product = await _productTableService.GetProductAsync(partitionKey, rowKey);
            if (product != null)
            {
                // Delete associated image from blob storage
                if (!string.IsNullOrEmpty(product.ProductImageUrl))
                {
                    string fileName = ExtractFileName(product.ProductImageUrl);
                    await _blobStorageService.DeleteImageAsync(fileName);
                }

                await _productTableService.DeleteProductAsync(partitionKey, rowKey);
            }

            return RedirectToAction(nameof(Index));
        }

        // extracts the blob file name from a full URL or returns the value as-is
        private string ExtractFileName(string imageUrlOrFileName)
        {
            if (string.IsNullOrEmpty(imageUrlOrFileName))
                return string.Empty;

            // If it's already just a file name
            if (!imageUrlOrFileName.Contains('/'))
                return imageUrlOrFileName;

            // If it's a full URL, take the last segment
            return Path.GetFileName(new Uri(imageUrlOrFileName).LocalPath);
        }
    }
}