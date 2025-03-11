using Microsoft.AspNetCore.Mvc;

namespace pos.Controllers
{
    public class ItemExpiredController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
