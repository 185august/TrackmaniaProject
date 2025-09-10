using Microsoft.AspNetCore.Mvc;

namespace TrackmaniaWebsiteAPI.PlayerAccount
{
    [Route("[controller]")]
    [ApiController]
    public class UpdateComparisonPlayersController(
        IComparisonPlayerService comparisonPlayersService
    ) : ControllerBase
    {
        [HttpPost("UpdateComparisonPlayers")]
        public async Task<ActionResult> UpdateComparisonPlayers(string playerIdsCommaSeperated)
        {
            var players = await comparisonPlayersService.UpdateUserComparisonPlayers(
                playerIdsCommaSeperated
            );
            return Ok(players);
        }
    }
}
