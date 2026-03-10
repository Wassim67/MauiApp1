using CommunityToolkit.Mvvm.ComponentModel;

namespace MauiApp1.Models;

public partial class GameCell : ObservableObject
{
    [ObservableProperty]
    private string value = string.Empty;
}
