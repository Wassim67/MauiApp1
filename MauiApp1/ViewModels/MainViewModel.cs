using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const string HumanPlayer = "X";
    private const string BotPlayer = "O";

    private readonly IGameHistoryService _gameHistoryService;
    private readonly Random _random = new();

    public ObservableCollection<GameCell> Cells { get; } = [];

    [ObservableProperty]
    private bool isGameOver;

    [ObservableProperty]
    private int movesCount;

    [ObservableProperty]
    private string statusMessage = "Ton tour (X)";

    [ObservableProperty]
    private string historyMessage = "Historique V/D/N : 0/0/0";

    public MainViewModel(IGameHistoryService gameHistoryService)
    {
        _gameHistoryService = gameHistoryService;

        for (var i = 0; i < 9; i++)
        {
            Cells.Add(new GameCell());
        }

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
            return;
        }

        PlayBotTurn();
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
    }

    private void PlayBotTurn()
    {
        var emptyCells = Cells.Where(c => string.IsNullOrEmpty(c.Value)).ToList();
        if (emptyCells.Count == 0)
        {
            return;
        }

        var botCell = emptyCells[_random.Next(emptyCells.Count)];
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
