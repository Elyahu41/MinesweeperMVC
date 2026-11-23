using Microsoft.Extensions.Caching.Memory;
using MinesweeperMVC.Models;

namespace MinesweeperMVC.Services;

/// <summary>In-memory store; abandoned games expire after an hour of inactivity.</summary>
public sealed class MemoryGameStore(IMemoryCache cache) : IGameStore
{
    private static readonly MemoryCacheEntryOptions Options = new() { SlidingExpiration = TimeSpan.FromHours(1) };

    public MinesweeperGame? Load(string gameId) =>
        cache.TryGetValue(Key(gameId), out MinesweeperGame? game) ? game : null;

    public void Save(string gameId, MinesweeperGame game) => cache.Set(Key(gameId), game, Options);

    private static string Key(string gameId) => $"minesweeper:{gameId}";
}
