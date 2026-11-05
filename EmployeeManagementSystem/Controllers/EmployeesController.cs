using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace EmployeeManagementSystem.Controllers
{
    public class EmployeesController : Controller
    {
        private const int PageSize = 10;
        private const string DuplicateEmail = "Another employee already uses this email address.";

        private readonly ApplicationDbContext _context;

        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // EMPLOYEE LIST
        // =========================================

        // GET: Employees
        public async Task<IActionResult> Index(string? searchString, string? sort, bool desc = false, int page = 1)
        {
            IQueryable<Employee> employees = _context.Employees;
            var search = searchString?.Trim();

            if (!string.IsNullOrEmpty(search))
            {
                var term = search.ToLower();

                employees = employees.Where(e =>
                    e.FirstName.ToLower().Contains(term) ||
                    e.LastName.ToLower().Contains(term) ||
                    e.Email.ToLower().Contains(term) ||
                    e.Department.ToLower().Contains(term) ||
                    e.Position.ToLower().Contains(term));
            }

            // Viewers cannot see salaries, so they cannot sort by them either.
            if (sort == "salary" && !User.CanEdit()) sort = null;

            employees = (sort, desc) switch
            {
                ("id", false) => employees.OrderBy(e => e.Id),
                ("id", true) => employees.OrderByDescending(e => e.Id),
                ("name", false) => employees.OrderBy(e => e.LastName).ThenBy(e => e.FirstName),
                ("name", true) => employees.OrderByDescending(e => e.LastName).ThenByDescending(e => e.FirstName),
                ("department", false) => employees.OrderBy(e => e.Department).ThenBy(e => e.LastName),
                ("department", true) => employees.OrderByDescending(e => e.Department).ThenBy(e => e.LastName),
                ("position", false) => employees.OrderBy(e => e.Position).ThenBy(e => e.LastName),
                ("position", true) => employees.OrderByDescending(e => e.Position).ThenBy(e => e.LastName),
                ("salary", false) => employees.OrderBy(e => e.Salary).ThenBy(e => e.Id),
                ("salary", true) => employees.OrderByDescending(e => e.Salary).ThenBy(e => e.Id),
                ("hired", false) => employees.OrderBy(e => e.HireDate).ThenBy(e => e.Id),
                ("hired", true) => employees.OrderByDescending(e => e.HireDate).ThenBy(e => e.Id),
                _ => employees.OrderByDescending(e => e.Id)
            };

            var total = await employees.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            page = Math.Clamp(page, 1, totalPages);

            return View(new EmployeeListViewModel
            {
                Items = await employees.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync(),
                Search = string.IsNullOrEmpty(search) ? null : search,
                Sort = sort,
                Descending = desc,
                Page = page,
                TotalPages = totalPages,
                Total = total
            });
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
        [Authorize(Policy = Roles.CanEditPolicy)]
        public IActionResult Create()
        {
            return View();
        }


        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Roles.CanEditPolicy)]
        [EnableRateLimiting("writes")]
        public async Task<IActionResult> Create(
            [Bind("FirstName,LastName,Email,Department,Position,Salary,HireDate")]
            Employee employee)
        {
            employee.Email = employee.Email.Trim().ToLowerInvariant();

            if (await EmailInUse(employee.Email, null))
            {
                ModelState.AddModelError(nameof(Employee.Email), DuplicateEmail);
            }

            if (ModelState.IsValid)
            {
                _context.Employees.Add(employee);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException) when (EmailTaken(employee.Email, null))
                {
                    // Another request saved the same address first.
                    _context.Entry(employee).State = EntityState.Detached;
                    ModelState.AddModelError(nameof(Employee.Email), DuplicateEmail);
                    return View(employee);
                }

                // Go back to employee list after saving
                return RedirectToAction("Index", "Employees");
            }

            return View(employee);
        }


        // =========================================
        // EDIT
        // =========================================

        // GET: Employees/Edit/5
        [Authorize(Policy = Roles.CanEditPolicy)]
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
        [Authorize(Policy = Roles.CanEditPolicy)]
        [EnableRateLimiting("writes")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,FirstName,LastName,Email,Department,Position,Salary,HireDate")]
            Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            employee.Email = employee.Email.Trim().ToLowerInvariant();

            if (await EmailInUse(employee.Email, employee.Id))
            {
                ModelState.AddModelError(nameof(Employee.Email), DuplicateEmail);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException) when (EmailTaken(employee.Email, employee.Id))
                {
                    _context.Entry(employee).State = EntityState.Detached;
                    ModelState.AddModelError(nameof(Employee.Email), DuplicateEmail);
                    return View(employee);
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
        [Authorize(Policy = Roles.CanEditPolicy)]
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
        [Authorize(Policy = Roles.CanEditPolicy)]
        [EnableRateLimiting("writes")]
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

        private Task<bool> EmailInUse(string email, int? exceptId)
        {
            return _context.Employees.AsNoTracking().AnyAsync(e => e.Email == email && e.Id != exceptId);
        }

        private bool EmailTaken(string email, int? exceptId)
        {
            return _context.Employees.AsNoTracking().Any(e => e.Email == email && e.Id != exceptId);
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}