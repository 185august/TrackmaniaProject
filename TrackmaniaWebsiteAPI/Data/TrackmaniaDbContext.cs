using Microsoft.EntityFrameworkCore;
using TrackmaniaWebsiteAPI.Models;

namespace TrackmaniaWebsiteAPI.Data;

public class TrackmaniaDbContext(DbContextOptions<TrackmaniaDbContext> options)
    : Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<CampaignMapInfo> Maps { get; set; }
}
