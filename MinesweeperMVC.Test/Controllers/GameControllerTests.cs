using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MinesweeperMVC.Controllers;
using MinesweeperMVC.Models;
using MinesweeperMVC.Services;

namespace MinesweeperMVC.Test.Controllers;

[TestClass]
public sealed class GameControllerTests
{
    private MemoryGameStore _store = null!;
    private FakeSession _session = null!;
    private GameController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _store = new MemoryGameStore(new MemoryCache(new MemoryCacheOptions()));
        _session = new FakeSession();
        _controller = NewController(_session);
    }

    private GameController NewController(ISession session) => new(_store)
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { Session = session }
        }
    };

    private GameViewModel CurrentModel()
    {
        var view = (ViewResult)_controller.Index();
        return (GameViewModel)view.Model!;
    }

    [TestMethod]
    public void Index_CreatesBeginnerGame_ForNewVisitor()
    {
        var model = CurrentModel();

        Assert.AreEqual(Difficulty.Beginner, model.Game.Difficulty);
        Assert.AreEqual(GameStatus.NotStarted, model.Game.Status);
        Assert.IsFalse(model.FlagMode);
    }

    [TestMethod]
    public void Index_ReturnsSameGame_OnEveryRequestInSession()
    {
        var first = CurrentModel().Game;
        var second = CurrentModel().Game;

        Assert.AreSame(first, second);
    }

    [TestMethod]
    public void DifferentSessions_GetDifferentGames()
    {
        var mine = CurrentModel().Game;
        var other = (GameViewModel)((ViewResult)NewController(new FakeSession()).Index()).Model!;

        Assert.AreNotSame(mine, other.Game);
    }

    [TestMethod]
    public void New_StartsGameWithChosenDifficulty_AndRedirects()
    {
        var result = _controller.New("Expert");

        Assert.AreEqual(nameof(GameController.Index), ((RedirectToActionResult)result).ActionName);
        Assert.AreEqual(Difficulty.Expert, CurrentModel().Game.Difficulty);
    }

    [TestMethod]
    public void Reveal_UncoversCell_AndRedirects()
    {
        var result = _controller.Reveal(4, 4);

        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var game = CurrentModel().Game;
        Assert.AreEqual(GameStatus.InProgress, game.Status);
        Assert.IsTrue(game[4, 4].IsRevealed);
    }

    [TestMethod]
    public void Flag_TogglesFlag()
    {
        _controller.Flag(0, 0);

        Assert.IsTrue(CurrentModel().Game[0, 0].IsFlagged);
    }

    [TestMethod]
    public void FlagMode_MakesRevealPlaceAFlag()
    {
        _controller.ToggleFlagMode();
        _controller.Reveal(2, 3);

        var model = CurrentModel();
        Assert.IsTrue(model.FlagMode);
        Assert.IsTrue(model.Game[2, 3].IsFlagged);
        Assert.IsFalse(model.Game[2, 3].IsRevealed);
    }

    [TestMethod]
    public void New_ResetsFlagMode()
    {
        _controller.ToggleFlagMode();
        _controller.New("Beginner");

        Assert.IsFalse(CurrentModel().FlagMode);
    }

    /// <summary>Minimal in-memory ISession for controller tests.</summary>
    private sealed class FakeSession : ISession
    {
        private readonly Dictionary<string, byte[]> _data = [];

        public bool IsAvailable => true;
        public string Id { get; } = Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _data.Keys;

        public void Clear() => _data.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _data.Remove(key);
        public void Set(string key, byte[] value) => _data[key] = value;
        public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value) => _data.TryGetValue(key, out value);
    }
}
