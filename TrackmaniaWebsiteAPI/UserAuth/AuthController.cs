using Microsoft.AspNetCore.Mvc;
using TrackmaniaWebsiteAPI.DatabaseQuery;
using TrackmaniaWebsiteAPI.RequestQueue;
using TrackmaniaWebsiteAPI.Tokens;

namespace TrackmaniaWebsiteAPI.UserAuth
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<UserDetailsDto>> Register(UserRegisterDto request)
        {
            var createUser = await authService.RegisterAsync(request);
            if (createUser is null)
            {
                return BadRequest("Username already exists");
            }
            return Ok(createUser);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserLoginDto request)
        {
            var user = await authService.LoginAsync(request);
            if (user is null)
            {
                //return BadRequest("Username or password is wrong");
                throw new ApplicationException("User or password is wrong");
            }
            return Ok(user);
        }
    }
}
