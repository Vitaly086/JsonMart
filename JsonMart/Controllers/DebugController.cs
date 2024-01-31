using JsonMart.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JsonMart.Controllers;

[ApiController]
[Route("api/debug")]
public class DebugController : ControllerBase
{
    private readonly IUserService _userService;

    
    public DebugController(IUserService userService)
    {
        _userService = userService;
    }

    
    // Тестовый метод для возможности начисления денег пользователю
    [HttpPost("user/{id}/increase_balance")]
    public async Task<ActionResult> IncreaseBalance(int id, [FromBody] decimal amount, CancellationToken token)
    {
        if (amount <= 0)
        {
            return BadRequest("Amount must be greater than zero.");
        }

        var result = await _userService.TryIncreaseBalanceAsync(id, amount, token);
        if (!result)
        {
            return NotFound($"User with ID {id} not found.");
        }

        return Ok("Balance updated successfully.");
    }
}