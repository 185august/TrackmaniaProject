// using System.Security.Claims;
// using System.Text;
// using Microsoft.IdentityModel.JsonWebTokens;
// using Microsoft.IdentityModel.Tokens;
// using TrackmaniaWebsiteAPI.DatabaseQuery;
//
// namespace TrackmaniaWebsiteAPI.UserAuth;
//
// public class JwtHelperService(IConfiguration configuration)
// {
//     public string CreateToken(User user)
//     {
//         string secretKey = configuration["Jwt:Secret"]!;
//         var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
//
//         var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
//
//         var tokenDescriptor = new SecurityTokenDescriptor
//         {
//             Subject = new ClaimsIdentity(
//                 [
//                     new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
//                     new Claim(JwtRegisteredClaimNames.Name, user.Username),
//                 ]
//             ),
//             Expires = DateTime.Now.AddMinutes(
//                 configuration.GetValue<int>("Jwt:ExpirationInMinutes")
//             ),
//             SigningCredentials = credentials,
//             Issuer = configuration["Jwt:Issuer"],
//             Audience = configuration["Jwt:Audience"],
//         };
//
//         var handler = new JsonWebTokenHandler();
//         string token = handler.CreateToken(tokenDescriptor);
//
//         return token;
//     }
// }

//Program configuration
//builder.Services.AddAuthorization();

// builder
//     .Services.AddAuthentication(options =>
//     {
//         options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//         options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//         options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
//     })
//     .AddJwtBearer(options =>
//     {
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidIssuer = config["Jwt:Issuer"],
//             ValidAudience = config["Jwt:Audience"],
//             IssuerSigningKey = new SymmetricSecurityKey(
//                 Encoding.UTF8.GetBytes(config["Jwt:Secret"]!)
//             ),
//         };
//     });

// For jwt AUTH public async Task<string> LoginJwtAsync(UserLoginDto request)
// {
//     var user = await DoesUserExist(request.Username);
//     if (
//             user is null
//             || new PasswordHasher<User>().VerifyHashedPassword(
//                     user,
//                     user.PasswordHash,
//                     request.Password
//             ) == PasswordVerificationResult.Failed
//     )
//     {
//         throw new Exception("User login failed");
//     }
//
//     string token = jwtHelper.CreateToken(user);
//     return token;
// }

//FOR AUTHCONTROLLEr
// [HttpPost("loginJwt")]
// public async Task<ActionResult<string>> LoginJwt(UserLoginDto request)
// {
//     var token = await authService.LoginJwtAsync(request);
//     if (token.Length < 30)
//     {
//         return BadRequest(token);
//     }
//
//     return Ok(token);
// }
