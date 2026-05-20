namespace MauiApp1.Models;

public class ApiErrorDto
{
    public string? Title { get; init; }
    public string? Detail { get; init; }
    public string? Message { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; }
}
