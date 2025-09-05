using Microsoft.AspNetCore.Mvc;
using TrackmaniaWebsiteAPI.Models;
using TrackmaniaWebsiteAPI.RequestQueue;
using TrackmaniaWebsiteAPI.Tokens;

namespace TrackmaniaWebsiteAPI.UserAuth
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserRegisterDto request)
        {
            var user = await authService.RegisterAsync(request);
            if (user is null)
            {
                return BadRequest("Username already exists");
            }
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserLoginDto request)
        {
            var status = await authService.LoginAsync(request);
            if (status is null)
            {
                return BadRequest("Username or password is wrong");
            }
            return Ok(status);
        }

        [HttpPost("loginJwt")]
        public async Task<ActionResult<string>> LoginJwt(UserLoginDto request)
        {
            var token = await authService.LoginJwtAsync(request);
            if (token.Length < 30)
            {
                return BadRequest(token);
            }

            return Ok(token);
        }
    }
}
