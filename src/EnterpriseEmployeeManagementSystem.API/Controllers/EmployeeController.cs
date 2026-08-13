using EnterpriseEmployeeManagementSystem.Application.DTOs.Employees;
using EnterpriseEmployeeManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseEmployeeManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var employee = await _employeeService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(Create),
            new { id = employee.Id },
            employee);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmployeeDto>>> GetAll(
    CancellationToken cancellationToken)
    {
        var employees = await _employeeService.GetAllAsync(
            cancellationToken);

        return Ok(employees);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetById(
    int id,
    CancellationToken cancellationToken)
    {
        var employee = await _employeeService.GetByIdAsync(
            id,
            cancellationToken);

        return Ok(employee);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> Update(
    int id,
    UpdateEmployeeRequest request,
    CancellationToken cancellationToken)
    {
        var employee = await _employeeService.UpdateAsync(
            id,
            request,
            cancellationToken);

        return Ok(employee);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(
    int id,
    CancellationToken cancellationToken)
    {
        await _employeeService.DeactivateAsync(
            id,
            cancellationToken);

        return NoContent();
    }
}