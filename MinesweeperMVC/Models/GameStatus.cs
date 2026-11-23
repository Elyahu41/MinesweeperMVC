namespace MinesweeperMVC.Models;

public enum GameStatus
{
    /// <summary>Board is empty; mines are placed on the first reveal so the first click is always safe.</summary>
    NotStarted,
    InProgress,
    Won,
    Lost
}
