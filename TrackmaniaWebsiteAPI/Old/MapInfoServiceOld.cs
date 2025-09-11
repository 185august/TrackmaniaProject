// namespace TrackmaniaWebsiteAPI.Old;
//
// public class MapInfoServiceOld
// {
//     public async Task<List<CampaignMapsInfo>> GetAllMapsAsync()
//     {
//         return await _context.CampaignMaps.ToListAsync();
//     }
//
//     public async Task<List<CampaignMapsInfo>> NormalizationOfNameColumn()
//     {
//         var maps = await GetAllMapsAsync();
//         foreach (var map in maps)
//         {
//             string[] parts = map.Name.Split(" - ");
//             if (parts.Length == 2)
//             {
//                 var seasonYear = parts[0].Split(' ');
//                 if (seasonYear.Length == 2)
//                 {
//                     map.Season = seasonYear[0];
//                     map.Year = int.Parse(seasonYear[1]);
//                     map.Position = int.Parse(parts[1]);
//                 }
//             }
//         }
//
//         await _context.SaveChangesAsync();
//         return maps;
//     }
//
//     public class MapUids()
//     {
//         public string Name { get; set; }
//         public int Position { get; set; }
//
//         [Key]
//         public string MapUid { get; set; }
//     }
// }
