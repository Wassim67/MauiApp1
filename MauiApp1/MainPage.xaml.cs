namespace MauiApp1;

public partial class MainPage : ContentPage
{
    private readonly Button[] _cells;
    private bool _isXTurn = true;
    private bool _isGameOver;
    private int _movesCount;

    public MainPage()
    {
        InitializeComponent();
        _cells = [Cell0, Cell1, Cell2, Cell3, Cell4, Cell5, Cell6, Cell7, Cell8];
        ResetGame();
    }

    private void OnCellClicked(object? sender, EventArgs e)
    {
        if (_isGameOver || sender is not Button cell || !string.IsNullOrEmpty(cell.Text))
        {
            return;
        }

        var currentPlayer = _isXTurn ? "X" : "O";
        cell.Text = currentPlayer;
        _movesCount++;

        if (HasWinner(currentPlayer))
        {
            StatusLabel.Text = $"{currentPlayer} gagne !";
            _isGameOver = true;
            return;
        }

        if (_movesCount == 9)
        {
            StatusLabel.Text = "Match nul.";
            _isGameOver = true;
            return;
        }

        _isXTurn = !_isXTurn;
        StatusLabel.Text = $"Tour de {(_isXTurn ? "X" : "O")}";
    }

    private void OnRestartClicked(object? sender, EventArgs e)
    {
        ResetGame();
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

        foreach (var combination in winningCombinations)
        {
            if (_cells[combination[0]].Text == player &&
                _cells[combination[1]].Text == player &&
                _cells[combination[2]].Text == player)
            {
                return true;
            }
        }

        return false;
    }

    private void ResetGame()
    {
        foreach (var cell in _cells)
        {
            cell.Text = string.Empty;
        }

        _isXTurn = true;
        _isGameOver = false;
        _movesCount = 0;
        StatusLabel.Text = "Tour de X";
    }
}
