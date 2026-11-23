using MinesweeperMVC.Models;

namespace MinesweeperMVC.Services;

/// <summary>Keeps each player's game between requests.</summary>
public interface IGameStore
{
    MinesweeperGame? Load(string gameId);
    void Save(string gameId, MinesweeperGame game);
}
