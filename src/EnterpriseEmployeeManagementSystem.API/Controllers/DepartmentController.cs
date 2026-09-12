using EnterpriseEmployeeManagementSystem.Application.DTOs.Departments;
using EnterpriseEmployeeManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseEmployeeManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DepartmentResponse>>> GetAll(
        CancellationToken cancellationToken
    )
    {
        var departments = await _departmentService.GetAllAsync(cancellationToken);

        return Ok(departments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DepartmentResponse>> GetById(
        int id,
        CancellationToken cancellationToken
    )
    {
        var department = await _departmentService.GetByIdAsync(id, cancellationToken);

        return Ok(department);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HRManager")]
    public async Task<ActionResult<DepartmentResponse>> Create(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken
    )
    {
        var department = await _departmentService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = department.Id }, department);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,HRManager")]
    public async Task<ActionResult<DepartmentResponse>> Update(
        int id,
        UpdateDepartmentRequest request,
        CancellationToken cancellationToken
    )
    {
        var department = await _departmentService.UpdateAsync(id, request, cancellationToken);

        return Ok(department);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _departmentService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
