namespace MauiApp1.Services;

public class FakeGameHistoryService : IGameHistoryService
{
    public int Wins { get; private set; }
    public int Losses { get; private set; }
    public int Draws { get; private set; }

    public void AddWin() => Wins++;

    public void AddLoss() => Losses++;

    public void AddDraw() => Draws++;
}
