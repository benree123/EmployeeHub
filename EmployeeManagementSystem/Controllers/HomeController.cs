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
            var departments = await _context.Employees
                .GroupBy(e => e.Department)
                .Select(g => new { Name = g.Key, Employees = g.Count() })
                .ToListAsync();

            return View(new HomeViewModel
            {
                EmployeeCount = departments.Sum(d => d.Employees),
                DepartmentCount = departments.Count,
                Departments = departments
                    .OrderByDescending(d => d.Employees).ThenBy(d => d.Name)
                    .Select(d => new DepartmentCount(d.Name, d.Employees))
                    .ToList(),
                Recent = User.Identity?.IsAuthenticated == true
                    ? await _context.Employees.OrderByDescending(e => e.Id).Take(3).ToListAsync()
                    : []
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
