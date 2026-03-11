using SQLite;

namespace MauiApp1.Data;

public class GameDatabase
{
    private readonly SQLiteConnection _connection;

    public GameDatabase()
    {
        _connection = new SQLiteConnection(DbConstants.DatabasePath, DbConstants.Flags);
        _connection.CreateTable<GameHistoryEntry>();
        _connection.CreateTable<CurrentGameEntry>();
    }

    public SQLiteConnection Connection => _connection;
}
