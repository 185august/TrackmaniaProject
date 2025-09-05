using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TrackmaniaWebsiteAPI.Tokens;

namespace TrackmaniaWebsiteAPI.PlayerAccount
{
    [Route("[controller]")]
    [ApiController]
    public class OAuth2AccountController(PlayerAccountService playerAccountService) : ControllerBase
    {
        [HttpGet("GetAccountId")]
        public async Task<ActionResult> GetUbisoftAccountId(string accountNames)
        {
            try
            {
                string[] names = accountNames.Split(',');
                var accountIds = await playerAccountService.GetUbisoftAccountIdAsync(names);
                return Ok(accountIds);
            }
            catch (HttpRequestException ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
