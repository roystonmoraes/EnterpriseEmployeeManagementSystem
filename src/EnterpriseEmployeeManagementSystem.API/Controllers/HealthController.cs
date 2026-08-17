using Microsoft.AspNetCore.Mvc;

namespace EnterpriseEmployeeManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(
            new
            {
                Status = "Healthy",
                Version = "1.0.0",
                Timestamp = DateTime.UtcNow,
            }
        );
    }
}
