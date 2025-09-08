namespace TrackmaniaWebsiteAPI.Tokens;

public record IndividualTokenData(
    string AccessToken,
    string? RefreshToken,
    DateTime AccessExpiresAt,
    DateTime? RefreshExpiresAt
);
