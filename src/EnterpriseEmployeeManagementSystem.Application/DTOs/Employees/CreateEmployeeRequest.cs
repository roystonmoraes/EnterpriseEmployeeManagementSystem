namespace EnterpriseEmployeeManagementSystem.Application.DTOs.Employees
{
    public class CreateEmployeeRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public DateOnly HireDate { get; set; }
        public int DepartmentId { get; set; }
    }
}
