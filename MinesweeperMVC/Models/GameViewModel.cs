namespace MinesweeperMVC.Models;

/// <summary>
/// Everything the Game view needs: the domain model plus UI-only state
/// (whether a left click places a flag instead of revealing).
/// </summary>
public sealed record GameViewModel(MinesweeperGame Game, bool FlagMode);
