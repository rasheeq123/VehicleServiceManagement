using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using VehicleServiceManagemnt.Models;

namespace VehicleServiceManagement.Controllers
{
    public class AccountController : Controller

    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager)

        {
            _userManager = userManager;

            _signInManager = signInManager;

            _roleManager = roleManager;

        }


        [HttpGet]



        public IActionResult Register()

        {

            return View();

        }

        [HttpPost]

        public async Task<IActionResult> Register(User model)

        {

            if (ModelState.IsValid)

            {

                // Check if the email ID already exists

                var existingUser = await _userManager.FindByEmailAsync(model.EmailId);

                if (existingUser != null)

                {

                    ViewBag.ErrorMessage = "Email ID already exists.";

                    return View(model);

                }

                // if user type is Admin

                if (model.UserType == "Admin")

                {

                    var adminCount = (await _userManager.GetUsersInRoleAsync("Admin")).Count;

                    // If there are already two admins, return an error message

                    if (adminCount >= 2)

                    {

                        ViewBag.ErrorMessage = "Number of admin accounts have been limited to 2";

                        return View(model);

                    }

                }

                var user = new User

                {

                    UserName = model.EmailId,

                    Email = model.EmailId,

                    EmailConfirmed = true,

                    Password = model.Password

                };

                var res = await _userManager.CreateAsync(user, model.Password);

                if (res.Succeeded)

                {

                    if (model.UserType == "Admin")

                    {

                        // Add the user to the Admin role

                        await _userManager.AddToRoleAsync(user, "Admin");

                        HttpContext.Session.SetString("EmailId", model.EmailId);

                        return RedirectToAction("Login", "Account");

                    }

                    else if (model.UserType == "Advisor")

                    {

                        await _userManager.AddToRoleAsync(user, "Advisor");

                        HttpContext.Session.SetString("EmailId", model.EmailId);

                        return RedirectToAction("Login", "Account");

                    }

                    return RedirectToAction("Register", "User");

                }
                // reg fails
                var err = string.Join(",", res.Errors.Select(e => e.Description));

                return Content($"{err}");

            }

            return View(model);

        }

        //Login Page

        public IActionResult Login()

        {

            return View();

        }

        [HttpPost]

        public async Task<IActionResult> Login(User model)

        {

            if (ModelState.IsValid)

            {

                var res = await _signInManager.PasswordSignInAsync(model.EmailId, model.Password, isPersistent: false, lockoutOnFailure: false);

                if (res.Succeeded)

                {

                    var user = await _userManager.FindByEmailAsync(model.EmailId);

                    if (!User.Identity.IsAuthenticated)

                    {

                        return Unauthorized("User is not authenticated");

                    }

                    if (user != null)

                    {


                        var roles = await _userManager.GetRolesAsync(user);

                        var claims = new List<Claim>

                {

                    new Claim(ClaimTypes.Name, user.UserName),

                    new Claim(ClaimTypes.Role, model.UserType)

                };
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                        if (roles.Contains("Admin"))

                        {

                            return RedirectToAction("Index", "Admin");

                        }

                        else if (roles.Contains("Advisor"))

                        {

                            return RedirectToAction("Index", "Advisor");

                        }

                    }

                    else

                    {

                        ViewBag.ErrorMessage = "Incorrect email or password";

                        return View(model);

                    }

                }

                else

                {
                    ViewBag.ErrorMessage = "Incorrect email or password";

                    return View(model);

                }

            }

            else

            {

                ViewBag.ErrorMessage = "Incorrect email or password";

                return View(model);

            }

            return View();

        }
        public async Task<IActionResult> Logout()

        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Login");

        }

    }

}

Admin Controller

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.Data;
using VehicleServiceManagement.Models;

namespace VehicleServiceManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly VehicleDbContext db;

        public AdminController(VehicleDbContext context)
        {
            db = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public ActionResult ServiceScheduling()
        {

            var scheduledServices = db.ServiceRecords
                .Include(s => s.Vehicle)
                .Include(s => s.Vehicle.Customer)
                .Include(s => s.ServiceRepresentative)
                .Where(s => s.Status == ServiceStatus.Scheduled || s.Status == ServiceStatus.InProgress)
                .ToList();

            return View(scheduledServices);

        }

        // GET:  Service Records Page

        public IActionResult ManageServiceRecords()
        {

            var today = DateTime.Now;
            var serviceRecords = db.ServiceRecords
                .Include(s => s.Vehicle)
                .Include(s => s.ServiceRepresentative).ToList();

            foreach (var record in serviceRecords)
            {
                if (record.ServiceDate.Date <= today.Date && record.Status == ServiceStatus.Scheduled)
                {
                    record.Status = ServiceStatus.InProgress;
                }
            }

            db.SaveChanges();

            return View(serviceRecords);
        }

        public IActionResult CreateServiceRecord()
        {
            var combinedList = db.Vehicles.Select(v => new
            {
                v.VehicleID,
                DisplayText = v.Make + " - " + v.VIN
            }).ToList();

            ViewBag.Vehicles = new SelectList(combinedList, "VehicleID", "DisplayText");
            ViewBag.ServiceReps = new SelectList(db.ServiceRepresentatives, "ServiceRepID", "FirstName");
            return View();
        }

        // POST: Create New Service Record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateServiceRecord(ServiceRecordModel serviceRecord)
        {

            if (!ModelState.IsValid)
            {
                serviceRecord.Status = ServiceStatus.Scheduled;
                db.ServiceRecords.Add(serviceRecord);
                db.SaveChanges();
                return RedirectToAction("ManageServiceRecords");
            }
            return View(serviceRecord);
        }

        // GET: Edit Service Record
        public IActionResult EditServiceRecord(int id)
        {
            var serviceRecord = db.ServiceRecords.FirstOrDefault(s => s.ServiceRecordID == id);
            if (serviceRecord == null)
            {
                return NotFound();
            }
            ViewBag.Vehicles = new SelectList(db.Vehicles, "VehicleID", "Make", serviceRecord.VehicleID);
            ViewBag.ServiceReps = new SelectList(db.ServiceRepresentatives, "ServiceRepID", "FirstName", serviceRecord.ServiceRepID);
            return View(serviceRecord);
        }

        // POST: Edit Service Record
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult EditServiceRecord(int id, ServiceRecordModel serviceRecord)
        {
            if (id != serviceRecord.ServiceRecordID)
            {
                return BadRequest();
            }

            var existingRecord = db.ServiceRecords.FirstOrDefault(s => s.ServiceRecordID == id);

            if (existingRecord == null)
            {
                return NotFound();
            }


            existingRecord.ServiceDate = serviceRecord.ServiceDate;
            existingRecord.Status = serviceRecord.Status;
            existingRecord.ServiceRepID = serviceRecord.ServiceRepID;

            try
            {
                db.SaveChanges();
                return RedirectToAction("ManageServiceRecords");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error updating service record: " + ex.Message);
            }

            return View(serviceRecord);
        }



        // GET: Delete Service Record
        public IActionResult DeleteServiceRecord(int id)
        {
            var serviceRecord = db.ServiceRecords.FirstOrDefault(s => s.ServiceRecordID == id);
            if (serviceRecord == null)
            {
                return NotFound();
            }
            return View(serviceRecord);
        }

        // POST: Delete Service Record
        [HttpPost, ActionName("DeleteServiceRecord")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var serviceRecord = db.ServiceRecords.FirstOrDefault(s => s.ServiceRecordID == id);
            if (serviceRecord != null)
            {
                db.ServiceRecords.Remove(serviceRecord);
                db.SaveChanges();
            }
            return RedirectToAction("ManageServiceRecords");
        }

        // Assign a service advisor to a vehicle
        // GET: Assign Service Advisor Page
        public IActionResult AssignServiceAdvisor()
        {
            var serviceRecords = db.ServiceRecords
                .Include(s => s.Vehicle)
                .Where(s => s.Status == ServiceStatus.InProgress)
                .ToList();

            var serviceAdvisors = db.ServiceRepresentatives.ToList();

            ViewBag.ServiceRecords = new SelectList(serviceRecords, "ServiceRecordID", "Vehicle.Make");
            ViewBag.ServiceAdvisors = new SelectList(serviceAdvisors, "ServiceRepID", "FirstName");

            return View();
        }

        // POST: Assign Service Advisor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignServiceAdvisor(int serviceRecordId, int serviceAdvisorId)
        {
            var serviceRecord = db.ServiceRecords.Find(serviceRecordId);
            if (serviceRecord != null)
            {
                serviceRecord.ServiceRepID = serviceAdvisorId;
                serviceRecord.Status = ServiceStatus.Scheduled;

                db.SaveChanges();
                TempData["Success"] = "Service Advisor Assigned Successfully!";
            }
            else
            {
                TempData["Error"] = "Service Record Not Found!";
            }

            return RedirectToAction("AssignServiceAdvisor");
        }

        // Display completed service records
        public ActionResult ServiceCompletion()
        {
            var completedServices = db.ServiceRecords.Include(s => s.Vehicle).Where(s => s.Status == ServiceStatus.Completed).ToList();
            return View(completedServices);
        }

        // Manage vehicle details
        public ActionResult ManageVehicles()
        {
            var vehicles = db.Vehicles.ToList();
            return View(vehicles);
        }

        [HttpPost]


        public ActionResult AddVehicle(VehicleModel vehicle)
        {
            if (ModelState.IsValid)
            {
                db.Vehicles.Add(vehicle);
                db.SaveChanges();
            }
            return RedirectToAction("ManageVehicles");
        }

        public ActionResult DeleteVehicle(int id)
        {
            var vehicle = db.Vehicles.Find(id);
            if (vehicle != null)
            {
                db.Vehicles.Remove(vehicle);
                db.SaveChanges();
            }
            return RedirectToAction("ManageVehicles");
        }

        // Manage customer details
        public ActionResult ManageCustomers()
        {
            var customers = db.Customers.ToList();
            return View(customers);
        }

        [HttpPost]
        public ActionResult AddCustomer(CustomerModel customer)
        {
            if (ModelState.IsValid)
            {
                db.Customers.Add(customer);
                db.SaveChanges();
            }
            return RedirectToAction("ManageCustomers");
        }

        public ActionResult DeleteCustomer(int id)
        {
            var customer = db.Customers.Find(id);
            if (customer != null)
            {
                db.Customers.Remove(customer);
                db.SaveChanges();
            }
            return RedirectToAction("ManageCustomers");
        }

        // Manage service representatives
        public ActionResult ManageServiceRepresentatives()
        {
            var serviceReps = db.ServiceRepresentatives.ToList();
            return View(serviceReps);
        }

        [HttpPost]
        public ActionResult AddServiceRepresentative(ServiceRepresentativeModel rep)
        {
            if (ModelState.IsValid)
            {
                db.ServiceRepresentatives.Add(rep);
                db.SaveChanges();
            }
            return RedirectToAction("ManageServiceRepresentatives");
        }

        public ActionResult DeleteServiceRepresentative(int id)
        {
            var rep = db.ServiceRepresentatives.Find(id);
            if (rep != null)
            {
                db.ServiceRepresentatives.Remove(rep);
                db.SaveChanges();
            }
            return RedirectToAction("ManageServiceRepresentatives");
        }

        // Manage service items
        public ActionResult ManageServiceItems()
        {
            var serviceItems = db.ServiceItems.ToList();
            return View(serviceItems);
        }

        [HttpPost]
        public ActionResult AddServiceItem(ServiceItemModel item)
        {
            if (ModelState.IsValid)
            {
                db.ServiceItems.Add(item);
                db.SaveChanges();
            }
            return RedirectToAction("ManageServiceItems");
        }

        public ActionResult DeleteServiceItem(int id)
        {
            var item = db.ServiceItems.Find(id);
            if (item != null)
            {
                db.ServiceItems.Remove(item);
                db.SaveChanges();
            }
            return RedirectToAction("ManageServiceItems");
        }
    }
}