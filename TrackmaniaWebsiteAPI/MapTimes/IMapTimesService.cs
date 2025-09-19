using TrackmaniaWebsiteAPI.PlayerAccount;

namespace TrackmaniaWebsiteAPI.MapTimes;

public interface IMapTimesService
{
    Task<MapPersonalBestDto?> GetMapWr(string mapUid);
    Task<List<MapPersonalBestDto>> GetMapPersonalBestData(string mapId, PlayerProfileDto[] players);
    List<MapPersonalBestDto> GetTimeDifferenceToWrAndSort(
        List<MapPersonalBestDto> otherRecords,
        MapPersonalBestDto wr
    );
}
