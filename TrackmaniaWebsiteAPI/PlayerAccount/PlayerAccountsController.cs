using Microsoft.AspNetCore.Mvc;

namespace TrackmaniaWebsiteAPI.PlayerAccount
{
    [Route("[controller]")]
    [ApiController]
    public class PlayerAccountsController(PlayerAccountService playerAccountService)
        : ControllerBase
    {
        [HttpPost("GetAndUpdatePlayerAccounts")]
        public async Task<ActionResult<List<PlayerProfileDto>>> GetAndUpdatePlayerAccounts(
            string ubisoftUsernamesCommaSeperated
        )
        {
            var players = await playerAccountService.GetAndUpdatePlayerAccountsAsync(
                ubisoftUsernamesCommaSeperated
            );
            if (players.Count == 0)
            {
                return BadRequest("Error validating Ubisoft name");
            }
            return Ok(players);
        }
    }
}
