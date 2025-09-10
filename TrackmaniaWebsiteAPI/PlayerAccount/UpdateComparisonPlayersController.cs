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
        public async Task<ActionResult> UpdateComparisonPlayers(
            string playerIdsCommaSeperated,
            int userId
        )
        {
            var players = await comparisonPlayersService.UpdateUserComparisonPlayers(
                playerIdsCommaSeperated
            );
            await comparisonPlayersService.GetCurrentUserValues(userId, players);
            return Ok(players);
        }
    }
}
