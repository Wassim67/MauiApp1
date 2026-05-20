using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(2);

    private readonly MorpionApiClient _apiClient;

    public ObservableCollection<GameCell> Cells { get; } = [];

    [ObservableProperty]
    private bool isGameOver;

    [ObservableProperty]
    private int movesCount;

    [ObservableProperty]
    private string statusMessage = "Connexion a l'API...";

    public MainViewModel(MorpionApiClient apiClient)
    {
        _apiClient = apiClient;

        for (var i = 0; i < 9; i++)
        {
            Cells.Add(new GameCell());
        }

        _ = LoadCurrentGameAsync();
        _ = StartPollingAsync();
    }

    [RelayCommand]
    private async Task Play(GameCell? cell)
    {
        if (cell is null || IsGameOver || !string.IsNullOrEmpty(cell.Value))
        {
            return;
        }

        var index = Cells.IndexOf(cell);
        await RunApiActionAsync(async () =>
        {
            var game = await _apiClient.PlayMoveAsync(index);
            ApplyGame(game);
        });
    }

    [RelayCommand]
    private async Task Restart()
    {
        await RunApiActionAsync(async () =>
        {
            var game = await _apiClient.CreateGameAsync();
            ApplyGame(game);
        });
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await RunApiActionAsync(async () =>
        {
            var game = await _apiClient.GetCurrentGameAsync();
            if (game is null)
            {
                StatusMessage = "Aucune partie en cours.";
                return;
            }

            ApplyGame(game);
        });
    }

    private async Task LoadCurrentGameAsync()
    {
        try
        {
            var game = await _apiClient.GetCurrentGameAsync() ??
                       await _apiClient.CreateGameAsync();
            ApplyGame(game);
        }
        catch
        {
            StatusMessage = "API indisponible.";
        }
    }

    private async Task StartPollingAsync()
    {
        using var timer = new PeriodicTimer(PollingInterval);

        while (await timer.WaitForNextTickAsync())
        {
            await RunApiActionAsync(async () =>
            {
                var game = await _apiClient.GetCurrentGameAsync();
                if (game is not null)
                {
                    ApplyGame(game);
                }
            });
        }
    }

    private async Task RunApiActionAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (MorpionApiException exception)
        {
            await RefreshBoardAsync();
            StatusMessage = exception.Message;
        }
        catch
        {
            StatusMessage = "API indisponible.";
        }
    }

    private async Task RefreshBoardAsync()
    {
        var game = await _apiClient.GetCurrentGameAsync();
        if (game is not null)
        {
            ApplyGame(game);
        }
    }

    private void ApplyGame(GameDto game)
    {
        for (var i = 0; i < Cells.Count; i++)
        {
            Cells[i].Value = i < game.Board.Length ? game.Board[i] : string.Empty;
        }

        IsGameOver = game.IsGameOver;
        MovesCount = game.MovesCount;
        StatusMessage = game.StatusMessage;
    }
}
