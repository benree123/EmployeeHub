namespace EmployeeManagementSystem.Models
{
    public static class DepartmentStyle
    {
        private const int Colors = 8;

        // The same department always gets the same color, on every page.
        public static string ClassFor(string department)
        {
            var total = 0;
            foreach (var character in department.Trim().ToLowerInvariant()) total = (total * 31 + character) % 9973;
            return $"dept-{total % Colors}";
        }

        public static string Initials(Employee employee) =>
            $"{employee.FirstName.FirstOrDefault()}{employee.LastName.FirstOrDefault()}".ToUpperInvariant();
    }
}
