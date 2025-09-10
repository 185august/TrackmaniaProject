using Microsoft.AspNetCore.Mvc;
using TrackmaniaWebsiteAPI.DatabaseQuery;

namespace TrackmaniaWebsiteAPI.MapTimes
{
    [Route("[controller]")]
    [ApiController]
    public class MapTimeController(
        ITimeCalculationService calculationService,
        MapRecordsService mapRecordsService
    ) : ControllerBase
    {
        [HttpGet("GetAllMapTimes")]
        public async Task<ActionResult<List<MapPersonalBestInfo>>> GetAllMapTimes(
            string mapId,
            string mapUid,
            PlayerProfiles[] players
        )
        {
            var wr = await mapRecordsService.GetMapWr(mapUid);
            if (wr is null)
            {
                return BadRequest("Could not get current wr");
            }
            var otherRecords = await mapRecordsService.GetMapPersonalBestInfo(mapId, players);

            var personalBestInfos = new List<MapPersonalBestInfo> { wr };
            personalBestInfos.AddRange(otherRecords);
            foreach (var person in personalBestInfos)
            {
                person.RecordScore.TimeVsWr = calculationService.CalculateTimeDifferenceWr(
                    wr.RecordScore.Time,
                    person.RecordScore.Time
                );
            }
            return Ok(personalBestInfos);
        }
    }
}
