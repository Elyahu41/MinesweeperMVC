using MinesweeperMVC.Models;

namespace MinesweeperMVC.Test.Models;

[TestClass]
public sealed class MinesweeperGameTests
{
    private static MinesweeperGame NewBeginner(int seed = 42) => new(Difficulty.Beginner, new Random(seed));

    private static MinesweeperGame StartedBeginner(int seed = 42)
    {
        var game = NewBeginner(seed);
        game.Reveal(4, 4);
        return game;
    }

    [TestMethod]
    public void NewGame_HasRequestedDimensions_AndNoMinesYet()
    {
        var game = NewBeginner();

        Assert.AreEqual(9, game.Rows);
        Assert.AreEqual(9, game.Columns);
        Assert.AreEqual(10, game.MineCount);
        Assert.AreEqual(GameStatus.NotStarted, game.Status);
        Assert.AreEqual(0, game.AllCells.Count(c => c.IsMine));
    }

    [TestMethod]
    public void Constructor_RejectsBoardWithNoSafeCell()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new MinesweeperGame(3, 3, 9));
    }

    [TestMethod]
    public void FirstReveal_PlacesExactMineCount_AndStartsGame()
    {
        var game = StartedBeginner();

        Assert.AreEqual(GameStatus.InProgress, game.Status);
        Assert.AreEqual(10, game.AllCells.Count(c => c.IsMine));
        Assert.IsNotNull(game.StartedAt);
    }

    [TestMethod]
    public void FirstReveal_IsAlwaysSafe_IncludingNeighbours()
    {
        for (var seed = 0; seed < 50; seed++)
        {
            var game = StartedBeginner(seed);
            var first = game[4, 4];

            Assert.IsFalse(first.IsMine, $"seed {seed}");
            Assert.IsTrue(game.Neighbours(first).All(n => !n.IsMine), $"seed {seed}");
            Assert.AreEqual(0, first.AdjacentMines, $"seed {seed}");
        }
    }

    [TestMethod]
    public void AdjacentMineCounts_AreCorrect()
    {
        var game = StartedBeginner();

        foreach (var cell in game.AllCells)
            Assert.AreEqual(game.Neighbours(cell).Count(n => n.IsMine), cell.AdjacentMines);
    }

    [TestMethod]
    public void RevealingAZero_FloodFillsToNumberedBorder()
    {
        var game = StartedBeginner();

        // Every revealed zero must have all of its neighbours revealed.
        foreach (var zero in game.AllCells.Where(c => c.IsRevealed && c.AdjacentMines == 0))
            Assert.IsTrue(game.Neighbours(zero).All(n => n.IsRevealed));

        Assert.IsTrue(game.AllCells.Count(c => c.IsRevealed) >= 9);
        Assert.IsFalse(game.AllCells.Any(c => c.IsRevealed && c.IsMine));
    }

    [TestMethod]
    public void RevealingAMine_LosesTheGame()
    {
        var game = StartedBeginner();
        var mine = game.AllCells.First(c => c.IsMine);

        game.Reveal(mine.Row, mine.Column);

        Assert.AreEqual(GameStatus.Lost, game.Status);
        Assert.IsTrue(mine.IsExploded);
        Assert.IsTrue(game.IsFinished);
    }

    [TestMethod]
    public void NoMovesAreAccepted_AfterGameIsOver()
    {
        var game = StartedBeginner();
        var mine = game.AllCells.First(c => c.IsMine);
        game.Reveal(mine.Row, mine.Column);
        var safe = game.AllCells.First(c => !c.IsMine && !c.IsRevealed);
        var moves = game.MoveCount;

        game.Reveal(safe.Row, safe.Column);
        game.ToggleFlag(safe.Row, safe.Column);

        Assert.IsFalse(safe.IsRevealed);
        Assert.IsFalse(safe.IsFlagged);
        Assert.AreEqual(moves, game.MoveCount);
    }

    [TestMethod]
    public void RevealingEverySafeCell_WinsTheGame_AndFlagsAllMines()
    {
        var game = StartedBeginner();

        foreach (var cell in game.AllCells.Where(c => !c.IsMine).ToList())
            game.Reveal(cell.Row, cell.Column);

        Assert.AreEqual(GameStatus.Won, game.Status);
        Assert.AreEqual(0, game.MinesRemaining);
        Assert.IsNotNull(game.FinishedAt);
    }

    [TestMethod]
    public void TinyBoard_WinsOnFirstClick_WhenOnlyOneSafeCell()
    {
        var game = new MinesweeperGame(2, 2, 3, new Random(1));

        game.Reveal(0, 0);

        Assert.AreEqual(GameStatus.Won, game.Status);
        Assert.IsFalse(game[0, 0].IsMine);
    }

    [TestMethod]
    public void ToggleFlag_FlagsAndUnflags_AndUpdatesCounter()
    {
        var game = StartedBeginner();
        var hidden = game.AllCells.First(c => !c.IsRevealed);

        game.ToggleFlag(hidden.Row, hidden.Column);
        Assert.IsTrue(hidden.IsFlagged);
        Assert.AreEqual(1, game.FlagCount);
        Assert.AreEqual(9, game.MinesRemaining);

        game.ToggleFlag(hidden.Row, hidden.Column);
        Assert.IsFalse(hidden.IsFlagged);
        Assert.AreEqual(10, game.MinesRemaining);
    }

    [TestMethod]
    public void FlaggedCell_CannotBeRevealed()
    {
        var game = StartedBeginner();
        var mine = game.AllCells.First(c => c.IsMine);

        game.ToggleFlag(mine.Row, mine.Column);
        game.Reveal(mine.Row, mine.Column);

        Assert.AreEqual(GameStatus.InProgress, game.Status);
        Assert.IsFalse(mine.IsRevealed);
    }

    [TestMethod]
    public void RevealedCell_CannotBeFlagged()
    {
        var game = StartedBeginner();

        game.ToggleFlag(4, 4);

        Assert.IsFalse(game[4, 4].IsFlagged);
    }

    [TestMethod]
    public void ChordingANumber_WithCorrectFlags_RevealsItsNeighbours()
    {
        var game = StartedBeginner();
        var number = game.AllCells.First(c =>
            c.IsRevealed && c.AdjacentMines > 0 && game.Neighbours(c).Any(n => !n.IsRevealed && !n.IsMine));

        foreach (var mine in game.Neighbours(number).Where(n => n.IsMine))
            game.ToggleFlag(mine.Row, mine.Column);
        game.Reveal(number.Row, number.Column);

        Assert.AreNotEqual(GameStatus.Lost, game.Status);
        Assert.IsTrue(game.Neighbours(number).All(n => n.IsRevealed || n.IsFlagged));
    }

    [TestMethod]
    public void OutOfRangeMoves_AreIgnored()
    {
        var game = NewBeginner();

        game.Reveal(-1, 0);
        game.Reveal(0, 99);
        game.ToggleFlag(9, 9);

        Assert.AreEqual(GameStatus.NotStarted, game.Status);
        Assert.AreEqual(0, game.MoveCount);
    }

    [TestMethod]
    [DataRow("beginner", 9, 9, 10)]
    [DataRow("Intermediate", 16, 16, 40)]
    [DataRow("EXPERT", 16, 30, 99)]
    [DataRow("nonsense", 9, 9, 10)]
    [DataRow(null, 9, 9, 10)]
    public void Difficulty_FromName_ResolvesPresets(string? name, int rows, int columns, int mines)
    {
        var d = Difficulty.FromName(name);

        Assert.AreEqual(rows, d.Rows);
        Assert.AreEqual(columns, d.Columns);
        Assert.AreEqual(mines, d.Mines);
    }
}
