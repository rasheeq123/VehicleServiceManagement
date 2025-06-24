using Microsoft.AspNetCore.Mvc;

namespace VehicleServiceManagemnt.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
