using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Position { get; set; } = string.Empty;

        [Range(0, 1000000)]
        [Precision(18, 2)]
        public decimal Salary { get; set; }

        [Display(Name = "Hire Date")]
        [DataType(DataType.Date)]
        [NotInFuture]
        public DateTime HireDate { get; set; }
    }
}