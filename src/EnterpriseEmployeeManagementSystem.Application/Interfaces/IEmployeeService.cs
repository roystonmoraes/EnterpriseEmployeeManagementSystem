using EnterpriseEmployeeManagementSystem.Application.DTOs.Employees;

namespace EnterpriseEmployeeManagementSystem.Application.Interfaces;

public interface IEmployeeService
{
    Task<EmployeeDto> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmployeeDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<EmployeeDto> GetByIdAsync(
    int id,
    CancellationToken cancellationToken = default);

    Task<EmployeeDto> UpdateAsync(
        int id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(
    int id,
    CancellationToken cancellationToken = default);
}