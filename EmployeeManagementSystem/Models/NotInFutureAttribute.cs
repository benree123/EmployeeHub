using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models
{
    public sealed class NotInFutureAttribute : ValidationAttribute
    {
        // One extra day allows for time zones ahead of the server.
        public override bool IsValid(object? value) =>
            value is not DateTime date || date.Date <= DateTime.UtcNow.Date.AddDays(1);

        public override string FormatErrorMessage(string name) => $"{name} cannot be in the future.";
    }
}
