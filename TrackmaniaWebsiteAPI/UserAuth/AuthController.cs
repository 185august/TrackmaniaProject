using Microsoft.AspNetCore.Mvc;

namespace TrackmaniaWebsiteAPI.UserAuth
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<string>> Register(UserRegisterDto request)
        {
            string? result = await authService.RegisterAsync(request);
            if (result is null)
            {
                return BadRequest(
                    "Registration failed. Username may already exists or invalid data provided"
                );
            }
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDetailsDto>> Login(UserLoginDto request)
        {
            var user = await authService.LoginAsync(request);
            if (user is null)
            {
                return BadRequest("Login failed. Username or password is wrong");
            }
            return Ok(user);
        }
    }
}
