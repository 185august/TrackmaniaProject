using Microsoft.AspNetCore.Mvc;
using TrackmaniaWebsiteAPI.Data;
using TrackmaniaWebsiteAPI.Services;

namespace TrackmaniaWebsiteAPI.MapTimes
{
    [Route("[controller]")]
    [ApiController]
    public class MapTimeController(
        IApiTokensService apiTokensService,
        IMapInfoService mapInfoService,
        ITimeCalculationService calculationService,
        TrackmaniaDbContext context,
        ApiRequestQueue queue,
        MapRecordsService mapRecordsService
    ) : ControllerBase
    {
        [HttpGet("GetAllMapTimes")]
        public async Task<ActionResult<List<MapPersonalBestInfo>>> GetAllMapTimes(
            string mapId,
            string accountIdList,
            string mapUid
        )
        {
            try
            {
                var wr = await mapRecordsService.GetMapWr(mapUid);
                if (wr is null)
                {
                    return BadRequest("Could not get current wr");
                }
                var otherRecords = await mapRecordsService.GetMapPersonalBestInfo(
                    mapId,
                    accountIdList
                );
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
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest("Could not get info");
            }
        }
    }
}
