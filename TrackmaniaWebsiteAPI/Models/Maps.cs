using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TrackmaniaWebsiteProject.Models;

public class Maps
{
    [Key]
    public int MapId { get; set; }
    public string MapName { get; set; }
}
