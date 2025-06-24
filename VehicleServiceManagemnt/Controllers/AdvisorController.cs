using Microsoft.AspNetCore.Mvc;

namespace VehicleServiceManagemnt.Controllers
{
    public class AdvisorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
