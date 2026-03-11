using SQLite;

namespace MauiApp1.Data;

public class CurrentGameEntry
{
    [PrimaryKey]
    public int Id { get; set; } = 1;

    [NotNull]
    public string Board { get; set; } = "---------";

    public int MovesCount { get; set; }

    public bool IsGameOver { get; set; }

    [NotNull]
    public string StatusMessage { get; set; } = "Ton tour (X)";
}
