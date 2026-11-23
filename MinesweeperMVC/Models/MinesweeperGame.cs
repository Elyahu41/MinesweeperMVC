namespace MinesweeperMVC.Models;

/// <summary>
/// The <b>Model</b> in MVC: all Minesweeper rules live here, with no knowledge of HTTP or HTML.
/// Controllers call <see cref="Reveal"/> and <see cref="ToggleFlag"/>; views only read state.
/// </summary>
public sealed class MinesweeperGame
{
    private readonly Cell[,] _board;
    private readonly Random _random;

    public MinesweeperGame(Difficulty difficulty, Random? random = null)
        : this(difficulty.Rows, difficulty.Columns, difficulty.Mines, random)
    {
        Difficulty = difficulty;
    }

    public MinesweeperGame(int rows, int columns, int mines, Random? random = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(rows, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(columns, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(mines);
        // At least one cell (the first click) must be safe.
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(mines, rows * columns);

        Rows = rows;
        Columns = columns;
        MineCount = mines;
        Difficulty = new Difficulty("Custom", rows, columns, mines);
        _random = random ?? Random.Shared;

        _board = new Cell[rows, columns];
        for (var r = 0; r < rows; r++)
        for (var c = 0; c < columns; c++)
            _board[r, c] = new Cell(r, c);
    }

    public Difficulty Difficulty { get; }
    public int Rows { get; }
    public int Columns { get; }
    public int MineCount { get; }
    public int MoveCount { get; private set; }
    public GameStatus Status { get; private set; } = GameStatus.NotStarted;
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? FinishedAt { get; private set; }

    public bool IsFinished => Status is GameStatus.Won or GameStatus.Lost;
    public int FlagCount => AllCells.Count(c => c.IsFlagged);

    /// <summary>The "mine counter" shown to the player. Can go negative if they over-flag.</summary>
    public int MinesRemaining => MineCount - FlagCount;

    public Cell this[int row, int column] => _board[row, column];

    public IEnumerable<Cell> AllCells
    {
        get
        {
            foreach (var cell in _board) yield return cell;
        }
    }

    public bool IsInRange(int row, int column) =>
        row >= 0 && row < Rows && column >= 0 && column < Columns;

    public IEnumerable<Cell> Neighbours(Cell cell)
    {
        for (var dr = -1; dr <= 1; dr++)
        for (var dc = -1; dc <= 1; dc++)
        {
            if (dr == 0 && dc == 0) continue;
            int r = cell.Row + dr, c = cell.Column + dc;
            if (IsInRange(r, c)) yield return _board[r, c];
        }
    }

    /// <summary>
    /// Left click. Reveals a hidden cell, or "chords" a revealed number whose
    /// flags are all placed (reveals every unflagged neighbour at once).
    /// </summary>
    public void Reveal(int row, int column)
    {
        if (IsFinished || !IsInRange(row, column)) return;

        var cell = _board[row, column];
        if (cell.IsFlagged) return;

        if (Status == GameStatus.NotStarted)
        {
            PlaceMines(cell);
            Status = GameStatus.InProgress;
            StartedAt = DateTimeOffset.UtcNow;
        }

        if (cell.IsRevealed)
        {
            Chord(cell);
            return;
        }

        MoveCount++;
        RevealCell(cell);
        CheckForWin();
    }

    /// <summary>Right click / flag mode. Toggles a flag on a hidden cell.</summary>
    public void ToggleFlag(int row, int column)
    {
        if (IsFinished || !IsInRange(row, column)) return;

        var cell = _board[row, column];
        if (cell.IsRevealed) return;

        cell.IsFlagged = !cell.IsFlagged;
        MoveCount++;
    }

    private void Chord(Cell cell)
    {
        if (cell.AdjacentMines == 0) return;

        var neighbours = Neighbours(cell).ToList();
        if (neighbours.Count(n => n.IsFlagged) != cell.AdjacentMines) return;

        MoveCount++;
        foreach (var n in neighbours.Where(n => !n.IsFlagged && !n.IsRevealed))
        {
            RevealCell(n);
            if (IsFinished) return;
        }
        CheckForWin();
    }

    private void RevealCell(Cell start)
    {
        if (start.IsMine)
        {
            start.IsExploded = true;
            Lose();
            return;
        }

        // Iterative flood fill: opening a 0 opens all of its neighbours.
        var pending = new Stack<Cell>();
        pending.Push(start);
        while (pending.Count > 0)
        {
            var cell = pending.Pop();
            if (cell.IsRevealed || cell.IsFlagged || cell.IsMine) continue;

            cell.IsRevealed = true;
            if (cell.AdjacentMines != 0) continue;

            foreach (var n in Neighbours(cell))
                if (!n.IsRevealed) pending.Push(n);
        }
    }

    /// <summary>
    /// Mines are placed after the first click, avoiding the clicked cell and (when the
    /// board has room) its 3×3 neighbourhood, so the opening move always clears an area.
    /// </summary>
    private void PlaceMines(Cell firstClick)
    {
        var safeZone = Neighbours(firstClick).Append(firstClick).ToHashSet();
        var candidates = AllCells.Where(c => !safeZone.Contains(c)).ToList();
        if (candidates.Count < MineCount)
            candidates = AllCells.Where(c => c != firstClick).ToList();

        // Partial Fisher–Yates shuffle: pick MineCount distinct cells.
        for (var i = 0; i < MineCount; i++)
        {
            var j = _random.Next(i, candidates.Count);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            candidates[i].IsMine = true;
        }

        foreach (var cell in AllCells)
            cell.AdjacentMines = Neighbours(cell).Count(n => n.IsMine);
    }

    private void CheckForWin()
    {
        if (Status != GameStatus.InProgress) return;
        if (AllCells.Any(c => !c.IsMine && !c.IsRevealed)) return;

        Status = GameStatus.Won;
        FinishedAt = DateTimeOffset.UtcNow;
        foreach (var mine in AllCells.Where(c => c.IsMine))
            mine.IsFlagged = true;
    }

    private void Lose()
    {
        Status = GameStatus.Lost;
        FinishedAt = DateTimeOffset.UtcNow;
    }
}
