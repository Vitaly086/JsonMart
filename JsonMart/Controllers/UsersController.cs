using JsonMart.Dtos;
using Microsoft.AspNetCore.Mvc;
using JsonMart.Services.Interfaces;

namespace JsonMart.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
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
            
            if (string.IsNullOrWhiteSpace(userCreateDto.Name))
            {
                return BadRequest("User name cannot be empty.");
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
    }
}