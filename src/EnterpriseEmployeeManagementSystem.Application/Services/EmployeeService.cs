using EnterpriseEmployeeManagementSystem.Application.DTOs.Employees;
using EnterpriseEmployeeManagementSystem.Application.Exceptions;
using EnterpriseEmployeeManagementSystem.Application.Interfaces;
using EnterpriseEmployeeManagementSystem.Domain.Entities;

namespace EnterpriseEmployeeManagementSystem.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<EmployeeDto> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        var emailExists = await _employeeRepository.ExistsByEmailAsync(
            request.Email,
            cancellationToken);

        if (emailExists)
        {
            throw new ConflictException(
    $"An employee with email '{request.Email}' already exists.");
        }

        var employee = new Employee
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            HireDate = request.HireDate,
            DepartmentId = request.DepartmentId,
            IsActive = true
        };

        await _employeeRepository.AddAsync(
            employee,
            cancellationToken);

        await _employeeRepository.SaveChangesAsync(
            cancellationToken);

        return new EmployeeDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            DateOfBirth = employee.DateOfBirth,
            HireDate = employee.HireDate,
            IsActive = employee.IsActive,
            DepartmentId = employee.DepartmentId
        };
    }

    public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync(
    CancellationToken cancellationToken = default)
    {
        var employees = await _employeeRepository.GetAllAsync(
            cancellationToken);

        return employees
            .Select(employee => new EmployeeDto
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                PhoneNumber = employee.PhoneNumber,
                DateOfBirth = employee.DateOfBirth,
                HireDate = employee.HireDate,
                IsActive = employee.IsActive,
                DepartmentId = employee.DepartmentId,
                DepartmentName = employee.Department?.Name
            })
            .ToList();
    }

    public async Task<EmployeeDto> GetByIdAsync(
    int id,
    CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (employee is null)
        {
            throw new NotFoundException(
                $"Employee with ID {id} was not found.");
        }

        return new EmployeeDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            DateOfBirth = employee.DateOfBirth,
            HireDate = employee.HireDate,
            IsActive = employee.IsActive,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department?.Name
        };
    }

    public async Task<EmployeeDto> UpdateAsync(
    int id,
    UpdateEmployeeRequest request,
    CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (employee is null)
        {
            throw new NotFoundException(
                $"Employee with ID {id} was not found.");
        }

        var emailExists = await _employeeRepository
            .ExistsByEmailAsync(request.Email, cancellationToken);

        if (emailExists && !string.Equals(
                employee.Email,
                request.Email,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException(
                $"An employee with email '{request.Email}' already exists.");
        }

        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.Email = request.Email;
        employee.PhoneNumber = request.PhoneNumber;
        employee.DateOfBirth = request.DateOfBirth;
        employee.HireDate = request.HireDate;
        employee.DepartmentId = request.DepartmentId;
        employee.IsActive = request.IsActive;

        _employeeRepository.Update(employee);

        await _employeeRepository.SaveChangesAsync(
            cancellationToken);

        return new EmployeeDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            DateOfBirth = employee.DateOfBirth,
            HireDate = employee.HireDate,
            IsActive = employee.IsActive,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department?.Name
        };
    }
}