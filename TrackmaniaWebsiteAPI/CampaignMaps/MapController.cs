using Microsoft.AspNetCore.Mvc;
using TrackmaniaWebsiteAPI.DatabaseQuery;

namespace TrackmaniaWebsiteAPI.CampaignMaps
{
    [Route("[controller]")]
    [ApiController]
    public class MapController(IMapInfoService mapInfoService) : ControllerBase
    {
        [HttpGet("GetMapsByYearAndSeason")]
        public async Task<ActionResult<List<CampaignMapsInfo>>> GetMapsByYearAndSeason(
            int year,
            string season
        )
        {
            var maps = await mapInfoService.FindMapByYearAndSeason(year, season);
            if (maps.Count == 0)
            {
                return BadRequest();
            }
            return Ok(maps);
        }
    }
}
