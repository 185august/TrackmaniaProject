using Microsoft.IdentityModel.JsonWebTokens;

namespace TrackmaniaWebsiteAPI.Services;


public static class JwtUtils
{
    public static async Task<string?> GetAccessTokenFromIdentityServer()
    {
        var tokenHandler = new JsonWebTokenHandler();
        using (var client = new HttpClient())
        {
            token.
        }
    }
}