using Microsoft.AspNetCore.Mvc;

namespace ABCRetail.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
