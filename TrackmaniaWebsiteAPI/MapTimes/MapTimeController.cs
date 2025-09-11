using Microsoft.AspNetCore.Mvc;
using TrackmaniaWebsiteAPI.DatabaseQuery;

namespace TrackmaniaWebsiteAPI.MapTimes
{
    [Route("[controller]")]
    [ApiController]
    public class MapTimeController(MapTimesService mapTimesService) : ControllerBase
    {
        [HttpPost("GetAllMapTimes")]
        public async Task<ActionResult<List<MapPersonalBestData>>> GetAllMapTimes(
            string mapId,
            string mapUid,
            PlayerProfiles[] players
        )
        {
            var wr = await mapTimesService.GetMapWr(mapUid);
            if (wr is null)
            {
                return BadRequest("Could not get current wr");
            }
            var otherRecords = await mapTimesService.GetMapPersonalBestData(mapId, players);

            return Ok(mapTimesService.GetTimeDifferenceToWrAndSort(otherRecords, wr));
        }
    }
}
