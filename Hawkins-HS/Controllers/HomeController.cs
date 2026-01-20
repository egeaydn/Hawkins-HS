using System.Diagnostics;
using Hawkins_HS.Models;
using Hawkins_HS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hawkins_HS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return View("Welcome");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View("Welcome");
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains(RoleConstants.Admin))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else if (roles.Contains(RoleConstants.Teacher))
            {
                return RedirectToAction("Dashboard", "Teachers");
            }
            else if (roles.Contains(RoleConstants.Student))
            {
                return RedirectToAction("Dashboard", "Students");
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
