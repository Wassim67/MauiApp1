using System.Linq;
using MauiApp1.Models;

namespace MauiApp1.Services;

public class FakeGameHistoryService : IGameHistoryService
{
    private CurrentGameState? _currentGame;

    public int Wins { get; private set; }
    public int Losses { get; private set; }
    public int Draws { get; private set; }

    public void AddWin() => Wins++;

    public void AddLoss() => Losses++;

    public void AddDraw() => Draws++;

    public CurrentGameState? LoadCurrentGame() => _currentGame;

    public void SaveCurrentGame(IReadOnlyList<GameCell> cells, int movesCount, bool isGameOver, string statusMessage)
    {
        var boardChars = cells.Select(c => string.IsNullOrEmpty(c.Value) ? '-' : c.Value[0]).ToArray();
        _currentGame = new CurrentGameState
        {
            Board = new string(boardChars),
            MovesCount = movesCount,
            IsGameOver = isGameOver,
            StatusMessage = statusMessage
        };
    }

    public void ClearCurrentGame()
    {
        _currentGame = null;
    }
}
