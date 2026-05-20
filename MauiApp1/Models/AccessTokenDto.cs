namespace MauiApp1.Models;

public class AccessTokenDto
{
    public required string AccessToken { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
}
