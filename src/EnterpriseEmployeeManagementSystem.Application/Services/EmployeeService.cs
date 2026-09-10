using EnterpriseEmployeeManagementSystem.Application.DTOs;
using EnterpriseEmployeeManagementSystem.Application.DTOs.Employees;
using EnterpriseEmployeeManagementSystem.Application.Exceptions;
using EnterpriseEmployeeManagementSystem.Application.Interfaces;
using EnterpriseEmployeeManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EnterpriseEmployeeManagementSystem.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(IEmployeeRepository employeeRepository, ILogger<EmployeeService> logger)
    {
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    public async Task<EmployeeDto> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var emailExists = await _employeeRepository.ExistsByEmailAsync(
            request.Email,
            cancellationToken
        );

        if (emailExists)
        {
            throw new ConflictException(
                $"An employee with email '{request.Email}' already exists."
            );
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
            IsActive = true,
        };

        await _employeeRepository.AddAsync(employee, cancellationToken);

        await _employeeRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Employee {EmployeeId} created with email {Email}.",
            employee.Id,
            employee.Email
        );

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
        };
    }

    public async Task<PagedResult<EmployeeDto>> GetAllAsync(
        EmployeeQueryRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (request.PageNumber < 1)
        {
            request.PageNumber = 1;
        }

        if (request.PageSize < 1)
        {
            request.PageSize = 10;
        }

        if (request.PageSize > 100)
        {
            request.PageSize = 100;
        }

        var result = await _employeeRepository.GetPagedAsync(
            request.Search,
            request.ActiveOnly,
            request.PageNumber,
            request.PageSize,
            cancellationToken
        );

        var employees = result
            .Items.Select(employee => new EmployeeDto
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
                DepartmentName = employee.Department?.Name,
            })
            .ToList();

        return new PagedResult<EmployeeDto>
        {
            Items = employees,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = result.TotalCount,
        };
    }

    public async Task<EmployeeDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);

        if (employee is null)
        {
            throw new NotFoundException($"Employee with ID {id} was not found.");
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
            DepartmentName = employee.Department?.Name,
        };
    }

    public async Task<EmployeeDto> UpdateAsync(
        int id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);

        if (employee is null)
        {
            throw new NotFoundException($"Employee with ID {id} was not found.");
        }

        var emailExists = await _employeeRepository.ExistsByEmailAsync(
            request.Email,
            cancellationToken
        );

        if (
            emailExists
            && !string.Equals(employee.Email, request.Email, StringComparison.OrdinalIgnoreCase)
        )
        {
            throw new ConflictException(
                $"An employee with email '{request.Email}' already exists."
            );
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

        await _employeeRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Employee {EmployeeId} updated.", employee.Id);

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
            DepartmentName = employee.Department?.Name,
        };
    }

    public async Task DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);

        if (employee is null)
        {
            throw new NotFoundException($"Employee with ID {id} was not found.");
        }

        if (!employee.IsActive)
        {
            return;
        }

        employee.IsActive = false;

        _employeeRepository.Update(employee);

        await _employeeRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Employee {EmployeeId} deactivated.", employee.Id);
    }
}
