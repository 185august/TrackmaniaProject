using Microsoft.AspNetCore.Mvc;

namespace TrackmaniaWebsiteAPI.PlayerAccount
{
    [Route("[controller]")]
    [ApiController]
    public class PlayerAccountsController(PlayerAccountService playerAccountService)
        : ControllerBase
    {
        [HttpPost("GetAndUpdatePlayerAccounts")]
        public async Task<ActionResult> GetAndUpdatePlayerAccounts(string playerIdsCommaSeperated)
        {
            var players = await playerAccountService.GetAndUpdatePlayerAccountsAsync(
                playerIdsCommaSeperated
            );
            if (players.Count == 0)
            {
                return BadRequest();
            }
            return Ok(players);
        }
    }
}
