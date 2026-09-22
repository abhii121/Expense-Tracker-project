using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController(DashboardService dashboardService) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummary>> GetSummary([FromQuery] int? year, [FromQuery] int? month)
    {
        var now = DateTime.UtcNow;
        return Ok(await dashboardService.GetSummaryAsync(User.GetUserId(), year ?? now.Year, month ?? now.Month));
    }
}
