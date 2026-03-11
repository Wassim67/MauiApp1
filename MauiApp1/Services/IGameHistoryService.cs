using MauiApp1.Models;

namespace MauiApp1.Services;

public interface IGameHistoryService
{
    int Wins { get; }
    int Losses { get; }
    int Draws { get; }

    void AddWin();
    void AddLoss();
    void AddDraw();

    CurrentGameState? LoadCurrentGame();
    void SaveCurrentGame(IReadOnlyList<GameCell> cells, int movesCount, bool isGameOver, string statusMessage);
    void ClearCurrentGame();
}
