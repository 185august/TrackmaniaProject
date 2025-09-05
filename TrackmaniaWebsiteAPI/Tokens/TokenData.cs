namespace TrackmaniaWebsiteAPI.Tokens;

public record TokenData(
    string AccessToken,
    string? RefreshToken,
    DateTime AccessExpiresAt,
    DateTime? RefreshExpiresAt
);
