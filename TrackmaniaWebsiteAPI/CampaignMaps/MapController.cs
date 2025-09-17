using Microsoft.AspNetCore.Mvc;
using TrackmaniaWebsiteAPI.DatabaseQuery;

namespace TrackmaniaWebsiteAPI.CampaignMaps
{
    [Route("[controller]")]
    [ApiController]
    public class MapController(IMapInfoService mapInfoService) : ControllerBase
    {
        [HttpGet("GetMapsByYearAndSeason")]
        public async Task<ActionResult<List<object>>> GetMapsByYearAndSeason(
            int year,
            string season
        )
        {
            var maps = await mapInfoService.FindMapByYearAndSeason(year, season);
            if (maps.Count == 0)
            {
                return BadRequest("Year/season is invalid");
            }
            return Ok(maps);
        }

        [HttpGet("GetAllMapsAndWriteToJsonFile")]
        public async Task GetAllMapsFromDatabaseAndWriteToJsonFileAsync()
        {
            await mapInfoService.GetAllMaps();
        }
    }
}
