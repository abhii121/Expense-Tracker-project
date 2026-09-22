using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/budgets")]
public class BudgetsController(BudgetService budgetService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<BudgetDto>>> GetForMonth([FromQuery] int year, [FromQuery] int month)
        => Ok(await budgetService.GetForMonthAsync(User.GetUserId(), year, month));

    [HttpPost]
    public async Task<ActionResult<BudgetDto>> Upsert(BudgetRequest request)
    {
        try
        {
            return Ok(await budgetService.UpsertAsync(User.GetUserId(), request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await budgetService.DeleteAsync(User.GetUserId(), id);
        return deleted ? NoContent() : NotFound();
    }
}
