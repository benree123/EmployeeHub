using System.Diagnostics;
using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Records appear only after sign-in. Visitors see totals only.
            return View(new HomeViewModel
            {
                EmployeeCount = await _context.Employees.CountAsync(),
                DepartmentCount = await _context.Employees.Select(e => e.Department).Distinct().CountAsync(),
                Recent = User.Identity?.IsAuthenticated == true
                    ? await _context.Employees.OrderByDescending(e => e.Id).Take(3).ToListAsync()
                    : []
            });
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
