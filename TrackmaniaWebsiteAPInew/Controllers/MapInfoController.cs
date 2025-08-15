using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackmaniaWebsiteAPInew.Data;
using TrackmaniaWebsiteAPInew.Models;

namespace TrackmaniaWebsiteAPInew.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MapInfoController : ControllerBase
    {
        private readonly MapInfoDbContext _context;

        public MapInfoController(MapInfoDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<MapInfo>>> GetMapInfos()
        {
            return Ok(await _context.AllMapsInfo.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MapInfo>> GetMapInfoById(int id)
        {
            var map = await _context.AllMapsInfo.FindAsync(id);
            if (map is null)
            {
                return NotFound();
            }
            return Ok(map);
        }

        [HttpPost]
        public async Task<ActionResult<MapInfo>> AddMapInfo(MapInfo mapInfo)
        {
            if (mapInfo is null)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(mapInfo.Title))
            {
                return BadRequest("Title is required");
            }

            _context.AllMapsInfo.Add(mapInfo);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMapInfoById), new { id = mapInfo.Id }, mapInfo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMapInfo(int id, MapInfo updatedMapInfo)
        {
            var map = await _context.AllMapsInfo.FindAsync(id);
            if (map is null)
            {
                return NotFound();
            }

            map.Title = updatedMapInfo.Title;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMapInfo(int id)
        {
            var map = await _context.AllMapsInfo.FindAsync(id);
            if (map is null)
            {
                return NotFound();
            }

            _context.AllMapsInfo.Remove(map);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
