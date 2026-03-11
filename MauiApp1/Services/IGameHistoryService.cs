namespace MauiApp1.Services;

public interface IGameHistoryService
{
    int Wins { get; }
    int Losses { get; }
    int Draws { get; }

    void AddWin();
    void AddLoss();
    void AddDraw();
}
