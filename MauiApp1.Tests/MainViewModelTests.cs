using MauiApp1.Services;
using MauiApp1.ViewModels;

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
        var vm = CreateViewModel();
        var firstCell = vm.Cells[0];

        vm.PlayCommand.Execute(firstCell);

        Assert.Equal("X", firstCell.Value);
        Assert.Equal(2, vm.MovesCount);
        Assert.Equal(1, vm.Cells.Count(c => c.Value == "X"));
        Assert.Equal(1, vm.Cells.Count(c => c.Value == "O"));
        Assert.False(vm.IsGameOver);
        Assert.Equal("Ton tour (X)", vm.StatusMessage);
    }

    [Fact]
    public void HumanWinningMove_ShouldEndGame_AndIncreaseWins()
    {
        var history = new TestGameHistoryService();
        var vm = CreateViewModel(history);

        vm.Cells[0].Value = "X";
        vm.Cells[1].Value = "X";
        vm.MovesCount = 2;

        vm.PlayCommand.Execute(vm.Cells[2]);

        Assert.True(vm.IsGameOver);
        Assert.Equal("Tu as gagne !", vm.StatusMessage);
        Assert.Equal(1, history.Wins);
        Assert.Equal(0, history.Losses);
        Assert.Equal(0, history.Draws);
        Assert.Equal("Historique V/D/N : 1/0/0", vm.HistoryMessage);
    }

    [Fact]
    public void DrawMove_ShouldEndGame_AndIncreaseDraws()
    {
        var history = new TestGameHistoryService();
        var vm = CreateViewModel(history);

        vm.Cells[0].Value = "X";
        vm.Cells[1].Value = "O";
        vm.Cells[2].Value = "X";
        vm.Cells[3].Value = "X";
        vm.Cells[4].Value = "O";
        vm.Cells[5].Value = "O";
        vm.Cells[6].Value = "O";
        vm.Cells[7].Value = "X";
        vm.MovesCount = 8;

        vm.PlayCommand.Execute(vm.Cells[8]);

        Assert.True(vm.IsGameOver);
        Assert.Equal("Match nul.", vm.StatusMessage);
        Assert.Equal(0, history.Wins);
        Assert.Equal(0, history.Losses);
        Assert.Equal(1, history.Draws);
        Assert.Equal("Historique V/D/N : 0/0/1", vm.HistoryMessage);
    }

    [Fact]
    public void BotWinningMove_ShouldEndGame_AndIncreaseLosses()
    {
        var history = new TestGameHistoryService();
        var vm = CreateViewModel(history);

        vm.Cells[1].Value = "X";
        vm.Cells[2].Value = "O";
        vm.Cells[3].Value = "O";
        vm.Cells[4].Value = "X";
        vm.Cells[5].Value = "X";
        vm.Cells[6].Value = "O";
        vm.Cells[7].Value = "O";
        vm.MovesCount = 7;

        vm.PlayCommand.Execute(vm.Cells[0]);

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
        var vm = CreateViewModel(history);

        vm.Cells[0].Value = "X";
        vm.Cells[1].Value = "X";
        vm.MovesCount = 2;
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
        var vm = CreateViewModel();
        vm.Cells[0].Value = "X";
        vm.MovesCount = 1;

        vm.PlayCommand.Execute(vm.Cells[0]);

        Assert.Equal(1, vm.MovesCount);
        Assert.Equal(1, vm.Cells.Count(c => c.Value == "X"));
        Assert.Equal(0, vm.Cells.Count(c => c.Value == "O"));
        Assert.False(vm.IsGameOver);
    }

    private static MainViewModel CreateViewModel(TestGameHistoryService? history = null)
    {
        return new MainViewModel(history ?? new TestGameHistoryService());
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
}
