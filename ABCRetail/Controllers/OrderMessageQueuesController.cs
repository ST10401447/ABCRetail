using Microsoft.AspNetCore.Mvc;
using ABCRetail.Models;
using ABCRetail.Services;

namespace ABCRetail.Controllers
{
    public class OrderMessageQueuesController : Controller
    {
        private readonly QueueStorageService queueStorageService;

        public OrderMessageQueuesController(QueueStorageService queueStorageService)
        {
            this.queueStorageService = queueStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var messages = await queueStorageService.GetMessagesAsync();
            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string messageText)
        {
            if (!string.IsNullOrWhiteSpace(messageText))
            {
                await queueStorageService.SendMessageAsync(messageText);
            }
            return RedirectToAction("Index");
        }

        // GET: Details
        public async Task<IActionResult> Details(string messageId)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                return NotFound();
            }

            var messages = await queueStorageService.GetMessagesAsync();
            QueueMessageViewModel message = null;

            foreach (var m in messages)
            {
                if (m.MessageId == messageId)
                {
                    message = m;
                    break;
                }
            }

            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // GET: Edit form
        public async Task<IActionResult> Edit(string messageId)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                return NotFound();
            }

            var messages = await queueStorageService.GetMessagesAsync();
            QueueMessageViewModel message = null;

            foreach (var m in messages)
            {
                if (m.MessageId == messageId)
                {
                    message = m;
                    break;
                }
            }

            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // POST: Save the edited message
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QueueMessageViewModel model)
        {
            if (string.IsNullOrEmpty(model.MessageId))
            {
                ModelState.AddModelError("", "MessageId is missing. Please go back and try again.");
                return View(model);
            }

            try
            {
                await queueStorageService.UpdateMessageAsync(model.MessageId, model.MessageText);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error updating message: " + ex.Message);
                return View(model);
            }
        }

        // GET: Delete confirmation page
        public async Task<IActionResult> Delete(string messageId)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                return NotFound();
            }

            var messages = await queueStorageService.GetMessagesAsync();
            QueueMessageViewModel message = null;

            foreach (var m in messages)
            {
                if (m.MessageId == messageId)
                {
                    message = m;
                    break;
                }
            }

            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // POST: Actually delete the message
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string messageId)
        {
            if (!string.IsNullOrEmpty(messageId))
            {
                await queueStorageService.DeleteMessageAsync(messageId);
            }
            return RedirectToAction("Index");
        }
    }
}