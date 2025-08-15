using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TrackmaniaWebsiteProject.Models;

public class Maps
{
    public int MapId { get; set; }
    public string MapName { get; set; }

    public Maps() { }

    public Maps(int mapId, string mapName)
    {
        MapId = mapId;
        MapName = mapName;
    }
}
