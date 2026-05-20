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
    private bool _isPolling;

    public ObservableCollection<GameCell> Cells { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAuthPanelVisible))]
    [NotifyPropertyChangedFor(nameof(IsGameVisible))]
    private bool isAuthenticated;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isGameOver;

    [ObservableProperty]
    private int movesCount;

    [ObservableProperty]
    private string email = "player1@test.local";

    [ObservableProperty]
    private string password = "Password123";

    [ObservableProperty]
    private string statusMessage = "Connecte-toi pour jouer.";

    public bool IsAuthPanelVisible => !IsAuthenticated;

    public bool IsGameVisible => IsAuthenticated;

    public MainViewModel(MorpionApiClient apiClient)
    {
        _apiClient = apiClient;

        for (var i = 0; i < 9; i++)
        {
            Cells.Add(new GameCell());
        }

        Logout();
        _ = StartPollingAsync();
    }

    [RelayCommand]
    private async Task Login()
    {
        if (!ValidateCredentials())
        {
            return;
        }

        await RunAuthActionAsync(async () => await _apiClient.LoginAsync(Email, Password));
    }

    [RelayCommand]
    private async Task Register()
    {
        if (!ValidateCredentials())
        {
            return;
        }

        await RunAuthActionAsync(async () => await _apiClient.RegisterAsync(Email, Password));
    }

    [RelayCommand]
    private void Logout()
    {
        _apiClient.Logout();
        IsAuthenticated = false;
        IsGameOver = false;
        MovesCount = 0;
        ClearBoard();
        StatusMessage = "Connecte-toi pour jouer.";
    }

    [RelayCommand]
    private async Task Play(GameCell? cell)
    {
        if (!IsAuthenticated || cell is null || IsBusy || IsGameOver || !string.IsNullOrEmpty(cell.Value))
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
        if (!IsAuthenticated)
        {
            return;
        }

        await RunApiActionAsync(async () =>
        {
            var game = await _apiClient.CreateGameAsync();
            ApplyGame(game);
        });
    }

    [RelayCommand]
    private async Task Refresh()
    {
        if (!IsAuthenticated)
        {
            return;
        }

        await RunApiActionAsync(RefreshCurrentGameAsync);
    }

    private async Task RunAuthActionAsync(Func<Task> action)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;

        try
        {
            await action();
            IsAuthenticated = true;
            await LoadCurrentGameAsync();
        }
        catch (MorpionApiException exception)
        {
            StatusMessage = exception.Message;
        }
        catch
        {
            StatusMessage = "API indisponible.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadCurrentGameAsync()
    {
        var game = await _apiClient.GetCurrentGameAsync() ??
                   await _apiClient.CreateGameAsync();
        ApplyGame(game);
    }

    private async Task StartPollingAsync()
    {
        using var timer = new PeriodicTimer(PollingInterval);

        while (await timer.WaitForNextTickAsync())
        {
            if (!IsAuthenticated || IsBusy || _isPolling)
            {
                continue;
            }

            _isPolling = true;

            try
            {
                var game = await _apiClient.GetCurrentGameAsync();
                if (game is not null)
                {
                    ApplyGame(game);
                }
            }
            catch (MorpionApiException exception) when (exception.IsUnauthorized)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Logout();
                    StatusMessage = exception.Message;
                });
            }
            catch
            {
                // Le polling est silencieux pour ne pas bloquer l'utilisateur pendant un clic.
            }
            finally
            {
                _isPolling = false;
            }
        }
    }

    private async Task RunApiActionAsync(Func<Task> action)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;

        try
        {
            await action();
        }
        catch (MorpionApiException exception) when (exception.IsUnauthorized)
        {
            Logout();
            StatusMessage = exception.Message;
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
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshCurrentGameAsync()
    {
        var game = await _apiClient.GetCurrentGameAsync();
        if (game is null)
        {
            StatusMessage = "Aucune partie en cours.";
            ClearBoard();
            return;
        }

        ApplyGame(game);
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
        if (!MainThread.IsMainThread)
        {
            MainThread.BeginInvokeOnMainThread(() => ApplyGame(game));
            return;
        }

        for (var i = 0; i < Cells.Count; i++)
        {
            Cells[i].Value = i < game.Board.Length ? game.Board[i] : string.Empty;
        }

        IsGameOver = game.IsGameOver;
        MovesCount = game.MovesCount;
        StatusMessage = game.StatusMessage;
    }

    private void ClearBoard()
    {
        foreach (var cell in Cells)
        {
            cell.Value = string.Empty;
        }
    }

    private bool ValidateCredentials()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            StatusMessage = "Renseigne ton email.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "Renseigne ton mot de passe.";
            return false;
        }

        return true;
    }
}
