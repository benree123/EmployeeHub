using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // EMPLOYEE LIST
        // =========================================

        // GET: Employees
        public async Task<IActionResult> Index(string? searchString)
        {
            IQueryable<Employee> employees = _context.Employees;

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                string search = searchString.Trim();

                employees = employees.Where(e =>
                    e.FirstName.Contains(search) ||
                    e.LastName.Contains(search) ||
                    e.Email.Contains(search) ||
                    e.Department.Contains(search) ||
                    e.Position.Contains(search));
            }

            ViewData["CurrentFilter"] = searchString;

            // Newest employees appear first
            employees = employees.OrderByDescending(e => e.Id);

            return View(await employees.ToListAsync());
        }


        // =========================================
        // DETAILS
        // =========================================

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }


        // =========================================
        // CREATE
        // =========================================

        // GET: Employees/Create
        public IActionResult Create()
        {
            return View();
        }


        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("FirstName,LastName,Email,Department,Position,Salary,HireDate")]
            Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Employees.Add(employee);

                await _context.SaveChangesAsync();

                // Go back to employee list after saving
                return RedirectToAction("Index", "Employees");
            }

            return View(employee);
        }


        // =========================================
        // EDIT
        // =========================================

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }


        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,FirstName,LastName,Email,Department,Position,Salary,HireDate")]
            Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction("Index", "Employees");
            }

            return View(employee);
        }


        // =========================================
        // DELETE
        // =========================================

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }


        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee != null)
            {
                _context.Employees.Remove(employee);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Employees");
        }


        // =========================================
        // CHECK EMPLOYEE
        // =========================================

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}