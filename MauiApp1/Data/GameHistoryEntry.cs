using SQLite;

namespace MauiApp1.Data;

public class GameHistoryEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Result { get; set; } = string.Empty;

    public DateTime PlayedAtUtc { get; set; }
}
