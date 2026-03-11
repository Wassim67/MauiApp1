using MauiApp1.Data;
using MauiApp1.Models;

namespace MauiApp1.Services;

public class SqliteGameHistoryService : IGameHistoryService
{
    private readonly GameDatabase _database;
    private readonly object _syncLock = new();

    public SqliteGameHistoryService(GameDatabase database)
    {
        _database = database;
    }

    public int Wins
    {
        get
        {
            lock (_syncLock)
            {
                return _database.Connection.Table<GameHistoryEntry>().Count(x => x.Result == "W");
            }
        }
    }

    public int Losses
    {
        get
        {
            lock (_syncLock)
            {
                return _database.Connection.Table<GameHistoryEntry>().Count(x => x.Result == "L");
            }
        }
    }

    public int Draws
    {
        get
        {
            lock (_syncLock)
            {
                return _database.Connection.Table<GameHistoryEntry>().Count(x => x.Result == "D");
            }
        }
    }

    public void AddWin() => InsertHistory("W");

    public void AddLoss() => InsertHistory("L");

    public void AddDraw() => InsertHistory("D");

    public CurrentGameState? LoadCurrentGame()
    {
        lock (_syncLock)
        {
            var row = _database.Connection.Table<CurrentGameEntry>().FirstOrDefault(x => x.Id == 1);
            if (row is null)
            {
                return null;
            }

            return new CurrentGameState
            {
                Board = row.Board,
                MovesCount = row.MovesCount,
                IsGameOver = row.IsGameOver,
                StatusMessage = row.StatusMessage
            };
        }
    }

    public void SaveCurrentGame(IReadOnlyList<GameCell> cells, int movesCount, bool isGameOver, string statusMessage)
    {
        var board = new string(cells.Select(c => string.IsNullOrEmpty(c.Value) ? '-' : c.Value[0]).ToArray());
        var row = new CurrentGameEntry
        {
            Id = 1,
            Board = board,
            MovesCount = movesCount,
            IsGameOver = isGameOver,
            StatusMessage = statusMessage
        };

        lock (_syncLock)
        {
            _database.Connection.InsertOrReplace(row);
        }
    }

    public void ClearCurrentGame()
    {
        lock (_syncLock)
        {
            _database.Connection.Delete<CurrentGameEntry>(1);
        }
    }

    private void InsertHistory(string result)
    {
        lock (_syncLock)
        {
            _database.Connection.Insert(new GameHistoryEntry
            {
                Result = result,
                PlayedAtUtc = DateTime.UtcNow
            });
        }
    }
}
