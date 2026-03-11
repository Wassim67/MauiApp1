namespace MauiApp1.Services;

public class CurrentGameState
{
    public string Board { get; init; } = "---------";
    public int MovesCount { get; init; }
    public bool IsGameOver { get; init; }
    public string StatusMessage { get; init; } = "Ton tour (X)";
}
