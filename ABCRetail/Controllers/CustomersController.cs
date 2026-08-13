using Microsoft.AspNetCore.Mvc;
using ABCRetail.Models;
using ABCRetail.Services;

namespace ABCRetail.Controllers
{
    public class CustomersController : Controller
    {
        private readonly TableStorageService tableStorageService;

        public CustomersController(TableStorageService tableStorageService)
        {
            this.tableStorageService = tableStorageService;
        }

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            var customers = await tableStorageService.GetAllCustomersAsync();
            return View(customers);
        }

        // GET: Customers/Details
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
                return NotFound();

            var customer = await tableStorageService.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerProfile customer)
        {
            // Fix validation error for Azure keys
            ModelState.Remove("PartitionKey");
            ModelState.Remove("RowKey");
            ModelState.Remove("ETag");
            ModelState.Remove("Timestamp");

            if (ModelState.IsValid)
            {
                try
                {
                    await tableStorageService.AddCustomerAsync(customer);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error saving customer: " + ex.Message);
                }
            }

            return View(customer);
        }

        // GET: Customers/Edit
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
                return NotFound();

            var customer = await tableStorageService.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        // POST: Customers/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerProfile customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await tableStorageService.UpdateCustomerAsync(customer);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating customer: " + ex.Message);
                }
            }

            return View(customer);
        }

        // GET: Customers/Delete
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
                return NotFound();

            var customer = await tableStorageService.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        // POST: Customers/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            if (!string.IsNullOrEmpty(partitionKey) && !string.IsNullOrEmpty(rowKey))
            {
                await tableStorageService.DeleteCustomerAsync(partitionKey, rowKey);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}