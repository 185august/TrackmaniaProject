using Microsoft.EntityFrameworkCore;
using TrackmaniaWebsiteAPI.Models;
using TrackmaniaWebsiteAPI.Services;

namespace TrackmaniaWebsiteAPI.Data;

public class TrackmaniaDbContext(DbContextOptions<TrackmaniaDbContext> options)
    : Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<CampaignMapsInfo> CampaignMaps { get; set; }
}
