using Microsoft.AspNetCore.Mvc;

namespace VehicleServiceManagemnt.Controllers
{
    public class ServiceRepresentativeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
