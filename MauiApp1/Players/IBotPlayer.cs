using MauiApp1.Models;

namespace MauiApp1.Players;

public interface IBotPlayer
{
    int? GetNextMoveIndex(IReadOnlyList<GameCell> cells);
}
