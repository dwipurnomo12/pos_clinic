using Microsoft.AspNetCore.Mvc;

namespace pos.Controllers
{
    public class PurchaseReportController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
