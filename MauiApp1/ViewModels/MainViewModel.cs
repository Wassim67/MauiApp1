using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Models;
using MauiApp1.Players;
using MauiApp1.Services;

namespace MauiApp1.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const string HumanPlayer = "X";
    private const string BotPlayer = "O";

    private readonly IGameHistoryService _gameHistoryService;
    private readonly IBotPlayer _botPlayer;

    public ObservableCollection<GameCell> Cells { get; } = [];

    [ObservableProperty]
    private bool isGameOver;

    [ObservableProperty]
    private int movesCount;

    [ObservableProperty]
    private string statusMessage = "Ton tour (X)";

    [ObservableProperty]
    private string historyMessage = "Historique V/D/N : 0/0/0";

    public MainViewModel(IGameHistoryService gameHistoryService, IBotPlayer botPlayer)
    {
        _gameHistoryService = gameHistoryService;
        _botPlayer = botPlayer;

        for (var i = 0; i < 9; i++)
        {
            Cells.Add(new GameCell());
        }

        RestoreCurrentGameIfAny();
        UpdateHistoryMessage();
    }

    [RelayCommand]
    private void Play(GameCell? cell)
    {
        if (cell is null || IsGameOver || !string.IsNullOrEmpty(cell.Value))
        {
            return;
        }

        ApplyMove(cell, HumanPlayer);
        if (TryEndGame(HumanPlayer))
        {
            PersistCurrentGame();
            return;
        }

        PlayBotTurn();
        PersistCurrentGame();
    }

    [RelayCommand]
    private void Restart()
    {
        foreach (var cell in Cells)
        {
            cell.Value = string.Empty;
        }

        IsGameOver = false;
        MovesCount = 0;
        StatusMessage = "Ton tour (X)";
        _gameHistoryService.ClearCurrentGame();
    }

    private void PlayBotTurn()
    {
        var botMoveIndex = _botPlayer.GetNextMoveIndex(Cells);
        if (botMoveIndex is null ||
            botMoveIndex < 0 ||
            botMoveIndex >= Cells.Count ||
            !string.IsNullOrEmpty(Cells[botMoveIndex.Value].Value))
        {
            botMoveIndex = Cells
                .Select((cell, index) => new { cell, index })
                .FirstOrDefault(x => string.IsNullOrEmpty(x.cell.Value))
                ?.index;

            if (botMoveIndex is null)
            {
                return;
            }
        }

        var botCell = Cells[botMoveIndex.Value];
        ApplyMove(botCell, BotPlayer);

        if (!TryEndGame(BotPlayer))
        {
            StatusMessage = "Ton tour (X)";
        }
    }

    private void ApplyMove(GameCell cell, string player)
    {
        cell.Value = player;
        MovesCount++;
    }

    private bool TryEndGame(string player)
    {
        if (HasWinner(player))
        {
            IsGameOver = true;

            if (player == HumanPlayer)
            {
                _gameHistoryService.AddWin();
                StatusMessage = "Tu as gagne !";
            }
            else
            {
                _gameHistoryService.AddLoss();
                StatusMessage = "Le bot a gagne.";
            }

            UpdateHistoryMessage();
            return true;
        }

        if (MovesCount == 9)
        {
            IsGameOver = true;
            _gameHistoryService.AddDraw();
            StatusMessage = "Match nul.";
            UpdateHistoryMessage();
            return true;
        }

        return false;
    }

    private void RestoreCurrentGameIfAny()
    {
        var state = _gameHistoryService.LoadCurrentGame();
        if (state is null)
        {
            return;
        }

        var board = state.Board ?? string.Empty;
        for (var i = 0; i < Cells.Count; i++)
        {
            var ch = i < board.Length ? board[i] : '-';
            Cells[i].Value = ch is 'X' or 'O' ? ch.ToString() : string.Empty;
        }

        MovesCount = state.MovesCount;
        IsGameOver = state.IsGameOver;
        StatusMessage = state.StatusMessage;
    }

    private void PersistCurrentGame()
    {
        _gameHistoryService.SaveCurrentGame(Cells, MovesCount, IsGameOver, StatusMessage);
    }

    private bool HasWinner(string player)
    {
        int[][] winningCombinations =
        [
            [0, 1, 2],
            [3, 4, 5],
            [6, 7, 8],
            [0, 3, 6],
            [1, 4, 7],
            [2, 5, 8],
            [0, 4, 8],
            [2, 4, 6]
        ];

        foreach (var combo in winningCombinations)
        {
            if (Cells[combo[0]].Value == player &&
                Cells[combo[1]].Value == player &&
                Cells[combo[2]].Value == player)
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateHistoryMessage()
    {
        HistoryMessage = $"Historique V/D/N : {_gameHistoryService.Wins}/{_gameHistoryService.Losses}/{_gameHistoryService.Draws}";
    }
}
