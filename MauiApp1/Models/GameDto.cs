namespace MauiApp1.Models;

public class GameDto
{
    public string[] Board { get; init; } = [];
    public string CurrentPlayer { get; init; } = string.Empty;
    public bool IsGameOver { get; init; }
    public int MovesCount { get; init; }
    public string StatusMessage { get; init; } = string.Empty;
}
