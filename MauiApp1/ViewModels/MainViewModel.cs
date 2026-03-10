using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Models;

namespace MauiApp1.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<GameCell> Cells { get; } = [];

    [ObservableProperty]
    private bool isXTurn = true;

    [ObservableProperty]
    private bool isGameOver;

    [ObservableProperty]
    private int movesCount;

    [ObservableProperty]
    private string statusMessage = "Tour de X";

    public MainViewModel()
    {
        for (var i = 0; i < 9; i++)
        {
            Cells.Add(new GameCell());
        }
    }

    [RelayCommand]
    private void Play(GameCell? cell)
    {
        if (cell is null || IsGameOver || !string.IsNullOrEmpty(cell.Value))
        {
            return;
        }

        var currentPlayer = IsXTurn ? "X" : "O";
        cell.Value = currentPlayer;
        MovesCount++;

        if (HasWinner(currentPlayer))
        {
            StatusMessage = $"{currentPlayer} gagne !";
            IsGameOver = true;
            return;
        }

        if (MovesCount == 9)
        {
            StatusMessage = "Match nul.";
            IsGameOver = true;
            return;
        }

        IsXTurn = !IsXTurn;
        StatusMessage = $"Tour de {(IsXTurn ? "X" : "O")}";
    }

    [RelayCommand]
    private void Restart()
    {
        foreach (var cell in Cells)
        {
            cell.Value = string.Empty;
        }

        IsXTurn = true;
        IsGameOver = false;
        MovesCount = 0;
        StatusMessage = "Tour de X";
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
}
