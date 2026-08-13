using Microsoft.AspNetCore.Mvc;
using ABCRetail.Models;
using ABCRetail.Services;

namespace ABCRetail.Controllers
{
    public class OrdersController : Controller
    {
        private readonly OrderTableService orderTableService;
        private readonly QueueStorageService queueStorageService;
        public OrdersController(OrderTableService orderTableService, QueueStorageService queueStorageService)
        {
            this.orderTableService = orderTableService;
            this.queueStorageService = queueStorageService;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var orders = await orderTableService.GetAllOrdersAsync();
            return View(orders);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderEntity order)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (order.Quantity < 1)
                    {
                        order.Quantity = 1;
                    }

                    await orderTableService.AddOrderAsync(order);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error saving order: " + ex.Message);
                }
            }
            return View(order);
        }

        // POST: Create order directly from a product card
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromProduct(string productName, int? price, int quantity)
        {
            if (string.IsNullOrEmpty(productName) || quantity < 1)
            {
                return RedirectToAction("Index", "Products");
            }

            var order = new OrderEntity
            {
                ProductName = productName,
                Price = price,
                Quantity = quantity
            };

            try
            {
                await queueStorageService.SendMessageAsync($"New order created: {productName}, Quantity: {quantity}, Price: {price}");
                await orderTableService.AddOrderAsync(order);
            }
            catch
            {
                
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Orders/Edit
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var order = await orderTableService.GetOrderAsync(partitionKey, rowKey);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OrderEntity order)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (order.Quantity < 1)
                    {
                        order.Quantity = 1;
                    }

                    await orderTableService.UpdateOrderAsync(order);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating order: " + ex.Message);
                }
            }
            return View(order);
        }

        // GET: Orders/Details
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var order = await orderTableService.GetOrderAsync(partitionKey, rowKey);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Delete
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            var order = await orderTableService.GetOrderAsync(partitionKey, rowKey);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            if (!string.IsNullOrEmpty(partitionKey) && !string.IsNullOrEmpty(rowKey))
            {
                await orderTableService.DeleteOrderAsync(partitionKey, rowKey);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}