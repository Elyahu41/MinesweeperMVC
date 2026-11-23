namespace MinesweeperMVC.Models;

/// <summary>A single square on the board.</summary>
public sealed class Cell(int row, int column)
{
    public int Row { get; } = row;
    public int Column { get; } = column;

    public bool IsMine { get; internal set; }
    public bool IsRevealed { get; internal set; }
    public bool IsFlagged { get; internal set; }

    /// <summary>True only for the mine the player clicked on to lose the game.</summary>
    public bool IsExploded { get; internal set; }

    /// <summary>Number of mines in the eight neighbouring cells.</summary>
    public int AdjacentMines { get; internal set; }
}
