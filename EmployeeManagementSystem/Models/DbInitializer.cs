using System;
using System.Linq;

namespace EmployeeManagementSystem.Models
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Do not add demo employees if employees already exist.
            if (context.Employees.Any())
            {
                return;
            }

            var employees = new Employee[]
            {
                new Employee
                {
                    FirstName = "John",
                    LastName = "Carter",
                    Email = "john.carter@example.com",
                    Department = "Information Technology",
                    Position = "Senior Software Developer",
                    Salary = 85000,
                    HireDate = new DateTime(2024, 3, 18)
                },

                new Employee
                {
                    FirstName = "Sarah",
                    LastName = "Johnson",
                    Email = "sarah.johnson@example.com",
                    Department = "Human Resources",
                    Position = "HR Specialist",
                    Salary = 62000,
                    HireDate = new DateTime(2025, 1, 13)
                },

                new Employee
                {
                    FirstName = "Alex",
                    LastName = "Morgan",
                    Email = "alex.morgan@example.com",
                    Department = "Product",
                    Position = "Product Manager",
                    Salary = 92000,
                    HireDate = new DateTime(2023, 8, 7)
                },

                new Employee
                {
                    FirstName = "Emily",
                    LastName = "Davis",
                    Email = "emily.davis@example.com",
                    Department = "Finance",
                    Position = "Financial Analyst",
                    Salary = 68000,
                    HireDate = new DateTime(2025, 5, 20)
                },

                new Employee
                {
                    FirstName = "Daniel",
                    LastName = "Wilson",
                    Email = "daniel.wilson@example.com",
                    Department = "Marketing",
                    Position = "Marketing Coordinator",
                    Salary = 58000,
                    HireDate = new DateTime(2024, 10, 2)
                },

                new Employee
                {
                    FirstName = "Olivia",
                    LastName = "Brown",
                    Email = "olivia.brown@example.com",
                    Department = "Operations",
                    Position = "Operations Manager",
                    Salary = 88000,
                    HireDate = new DateTime(2022, 11, 14)
                },

                new Employee
                {
                    FirstName = "Michael",
                    LastName = "Thompson",
                    Email = "michael.thompson@example.com",
                    Department = "Sales",
                    Position = "Account Executive",
                    Salary = 72000,
                    HireDate = new DateTime(2025, 7, 8)
                },

                new Employee
                {
                    FirstName = "Sophia",
                    LastName = "Martinez",
                    Email = "sophia.martinez@example.com",
                    Department = "Information Technology",
                    Position = "Systems Analyst",
                    Salary = 74000,
                    HireDate = new DateTime(2024, 6, 24)
                },

                new Employee
                {
                    FirstName = "James",
                    LastName = "Anderson",
                    Email = "james.anderson@example.com",
                    Department = "Finance",
                    Position = "Senior Accountant",
                    Salary = 79000,
                    HireDate = new DateTime(2023, 4, 10)
                },

                new Employee
                {
                    FirstName = "Rachel",
                    LastName = "Lee",
                    Email = "rachel.lee@example.com",
                    Department = "Human Resources",
                    Position = "Recruitment Coordinator",
                    Salary = 60000,
                    HireDate = new DateTime(2026, 2, 16)
                },

                new Employee
                {
                    FirstName = "David",
                    LastName = "Clark",
                    Email = "david.clark@example.com",
                    Department = "Operations",
                    Position = "Business Operations Analyst",
                    Salary = 70000,
                    HireDate = new DateTime(2025, 9, 22)
                },

                new Employee
                {
                    FirstName = "Emma",
                    LastName = "Walker",
                    Email = "emma.walker@example.com",
                    Department = "Marketing",
                    Position = "Digital Marketing Specialist",
                    Salary = 64000,
                    HireDate = new DateTime(2026, 4, 6)
                }
            };

            context.Employees.AddRange(employees);
            context.SaveChanges();
        }
    }
}