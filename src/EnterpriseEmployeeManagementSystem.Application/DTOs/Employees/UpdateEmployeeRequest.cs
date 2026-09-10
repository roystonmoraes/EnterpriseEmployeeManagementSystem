using System.ComponentModel.DataAnnotations;

namespace EnterpriseEmployeeManagementSystem.Application.DTOs.Employees
{
    public class UpdateEmployeeRequest
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public DateOnly DateOfBirth { get; set; }

        public DateOnly HireDate { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        public bool IsActive { get; set; }
    }
}
