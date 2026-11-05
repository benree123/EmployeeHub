namespace EmployeeManagementSystem.Models
{
    public class EmployeeListViewModel
    {
        public IReadOnlyList<Employee> Items { get; init; } = [];
        public string? Search { get; init; }
        public string? Sort { get; init; }
        public bool Descending { get; init; }
        public int Page { get; init; }
        public int TotalPages { get; init; }
        public int Total { get; init; }
    }

    public class HomeViewModel
    {
        public int EmployeeCount { get; init; }
        public int DepartmentCount { get; init; }
        public IReadOnlyList<Employee> Recent { get; init; } = [];
    }
}
