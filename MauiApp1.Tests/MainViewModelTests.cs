using System.Collections.Generic;
using System.Linq;
using MauiApp1.Models;
using MauiApp1.Players;
using MauiApp1.Services;
using MauiApp1.ViewModels;
using Xunit;

namespace MauiApp1.Tests;

public class MainViewModelTests
{
    [Fact]
    public void NewGame_ShouldStartWithEmptyBoard_AndZeroHistory()
    {
        var vm = CreateViewModel();

        Assert.Equal(9, vm.Cells.Count);
        Assert.All(vm.Cells, cell => Assert.Equal(string.Empty, cell.Value));
        Assert.False(vm.IsGameOver);
        Assert.Equal(0, vm.MovesCount);
        Assert.Equal("Ton tour (X)", vm.StatusMessage);
        Assert.Equal("Historique V/D/N : 0/0/0", vm.HistoryMessage);
    }

    [Fact]
    public void HumanMove_ShouldPlaceX_AndThenBotShouldPlayO()
    {
        var vm = CreateViewModel(new FakeBotPlayer(1));

        vm.PlayCommand.Execute(vm.Cells[0]);

        Assert.Equal("X", vm.Cells[0].Value);
        Assert.Equal("O", vm.Cells[1].Value);
        Assert.Equal(2, vm.MovesCount);
        Assert.False(vm.IsGameOver);
        Assert.Equal("Ton tour (X)", vm.StatusMessage);
    }

    [Fact]
    public void HumanWinningSequence_ShouldEndGame_AndIncreaseWins()
    {
        var history = new TestGameHistoryService();
        var vm = CreateViewModel(new FakeBotPlayer(3, 4, 5), history);

        vm.PlayCommand.Execute(vm.Cells[0]);
        vm.PlayCommand.Execute(vm.Cells[1]);
        vm.PlayCommand.Execute(vm.Cells[2]);

        Assert.True(vm.IsGameOver);
        Assert.Equal("Tu as gagne !", vm.StatusMessage);
        Assert.Equal(1, history.Wins);
        Assert.Equal(0, history.Losses);
        Assert.Equal(0, history.Draws);
        Assert.Equal("Historique V/D/N : 1/0/0", vm.HistoryMessage);
    }

    [Fact]
    public void FullGameEndingInDraw_ShouldIncreaseDraws()
    {
        var history = new TestGameHistoryService();
        var vm = CreateViewModel(new FakeBotPlayer(1, 4, 5, 6), history);

        vm.PlayCommand.Execute(vm.Cells[0]);
        vm.PlayCommand.Execute(vm.Cells[2]);
        vm.PlayCommand.Execute(vm.Cells[3]);
        vm.PlayCommand.Execute(vm.Cells[7]);
        vm.PlayCommand.Execute(vm.Cells[8]);

        Assert.True(vm.IsGameOver);
        Assert.Equal("Match nul.", vm.StatusMessage);
        Assert.Equal(0, history.Wins);
        Assert.Equal(0, history.Losses);
        Assert.Equal(1, history.Draws);
        Assert.Equal("Historique V/D/N : 0/0/1", vm.HistoryMessage);
    }

    [Fact]
    public void BotWinningSequence_ShouldIncreaseLosses()
    {
        var history = new TestGameHistoryService();
        var vm = CreateViewModel(new FakeBotPlayer(3, 4, 5), history);

        vm.PlayCommand.Execute(vm.Cells[0]);
        vm.PlayCommand.Execute(vm.Cells[1]);
        vm.PlayCommand.Execute(vm.Cells[8]);

        Assert.True(vm.IsGameOver);
        Assert.Equal("Le bot a gagne.", vm.StatusMessage);
        Assert.Equal(0, history.Wins);
        Assert.Equal(1, history.Losses);
        Assert.Equal(0, history.Draws);
        Assert.Equal("Historique V/D/N : 0/1/0", vm.HistoryMessage);
    }

    [Fact]
    public void Restart_ShouldClearBoard_AndKeepHistory()
    {
        var history = new TestGameHistoryService();
        var vm = CreateViewModel(new FakeBotPlayer(3, 4), history);

        vm.PlayCommand.Execute(vm.Cells[0]);
        vm.PlayCommand.Execute(vm.Cells[1]);
        vm.PlayCommand.Execute(vm.Cells[2]);

        vm.RestartCommand.Execute(null);

        Assert.False(vm.IsGameOver);
        Assert.Equal(0, vm.MovesCount);
        Assert.All(vm.Cells, cell => Assert.Equal(string.Empty, cell.Value));
        Assert.Equal("Ton tour (X)", vm.StatusMessage);
        Assert.Equal("Historique V/D/N : 1/0/0", vm.HistoryMessage);
    }

    [Fact]
    public void PlayingOnNonEmptyCell_ShouldDoNothing()
    {
        var vm = CreateViewModel(new FakeBotPlayer(1, 2, 3));

        vm.PlayCommand.Execute(vm.Cells[0]);
        var movesAfterValidTurn = vm.MovesCount;

        vm.PlayCommand.Execute(vm.Cells[0]);

        Assert.Equal(movesAfterValidTurn, vm.MovesCount);
        Assert.Equal(1, vm.Cells.Count(c => c.Value == "X"));
        Assert.Equal(1, vm.Cells.Count(c => c.Value == "O"));
        Assert.False(vm.IsGameOver);
    }

    private static MainViewModel CreateViewModel(
        IBotPlayer? botPlayer = null,
        TestGameHistoryService? history = null)
    {
        return new MainViewModel(
            history ?? new TestGameHistoryService(),
            botPlayer ?? new FakeBotPlayer());
    }

    private sealed class TestGameHistoryService : IGameHistoryService
    {
        public int Wins { get; private set; }
        public int Losses { get; private set; }
        public int Draws { get; private set; }

        public void AddWin() => Wins++;

        public void AddLoss() => Losses++;

        public void AddDraw() => Draws++;
    }

    private sealed class FakeBotPlayer : IBotPlayer
    {
        private readonly Queue<int> _plannedMoves;

        public FakeBotPlayer(params int[] plannedMoves)
        {
            _plannedMoves = new Queue<int>(plannedMoves);
        }

        public int? GetNextMoveIndex(IReadOnlyList<GameCell> cells)
        {
            while (_plannedMoves.Count > 0)
            {
                var move = _plannedMoves.Dequeue();
                if (move >= 0 && move < cells.Count && string.IsNullOrEmpty(cells[move].Value))
                {
                    return move;
                }
            }

            for (var i = 0; i < cells.Count; i++)
            {
                if (string.IsNullOrEmpty(cells[i].Value))
                {
                    return i;
                }
            }

            return null;
        }
    }
}
