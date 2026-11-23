namespace MinesweeperMVC.Models;

/// <summary>The three classic Minesweeper presets.</summary>
public sealed record Difficulty(string Name, int Rows, int Columns, int Mines)
{
    public static readonly Difficulty Beginner = new("Beginner", 9, 9, 10);
    public static readonly Difficulty Intermediate = new("Intermediate", 16, 16, 40);
    public static readonly Difficulty Expert = new("Expert", 16, 30, 99);

    public static IReadOnlyList<Difficulty> All { get; } = [Beginner, Intermediate, Expert];

    /// <summary>Looks a preset up by name (case-insensitive), falling back to Beginner.</summary>
    public static Difficulty FromName(string? name) =>
        All.FirstOrDefault(d => string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase)) ?? Beginner;
}
