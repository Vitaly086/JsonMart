using JsonMart.Dtos;
using Microsoft.AspNetCore.Mvc;
using JsonMart.Services.Interfaces;

namespace JsonMart.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    public class UsersControllerV1 : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersControllerV1(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserByIdAsync(int id, CancellationToken token)
        {
            var user = await _userService.GetUserByIdAsync(id, token);

            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            return Ok(user);
        }

        [HttpGet]
        public async Task<ActionResult<List<UserDto>>> GetAllUsersAsync(CancellationToken token)
        {
            var users = await _userService.GetAllUsersAsync(token);
            
            if (users == null || !users.Any())
            {
                return NotFound("Users not found.");
            }

            return Ok(users);
        }

        [HttpPost]
        public async Task<ActionResult<UserCreateResponseDto>> CreateUserAsync([FromBody] UserCreateDto? userCreateDto,
            CancellationToken token)
        {
            if (userCreateDto == null)
            {
                return BadRequest("User data is required.");
            }

            var userResponse = await _userService.CreateUserAsync(userCreateDto, token);

            if (userResponse == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while creating the user.");
            }

            if (!string.IsNullOrEmpty(userResponse.ErrorMessage))
            {
                return BadRequest(userResponse.ErrorMessage);
            }

            return Ok(userResponse.Id);
        }

        [HttpPost("{id}/increase_balance")]
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
}