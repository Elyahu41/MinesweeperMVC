using Microsoft.AspNetCore.Mvc;
using MinesweeperMVC.Models;
using MinesweeperMVC.Services;

namespace MinesweeperMVC.Controllers;

/// <summary>
/// The <b>Controller</b> in MVC: turns HTTP requests into calls on the model, then
/// picks what to render. Every state change is a POST followed by a redirect back to
/// <see cref="Index"/> (Post/Redirect/Get), so refreshing the page never replays a move.
/// </summary>
public class GameController(IGameStore store) : Controller
{
    private const string GameIdKey = "GameId";
    private const string FlagModeKey = "FlagMode";

    [HttpGet]
    public IActionResult Index()
    {
        var game = LoadOrCreateGame();
        return View(new GameViewModel(game, FlagMode));
    }

    [HttpPost]
    public IActionResult New(string? difficulty)
    {
        store.Save(GameId, new MinesweeperGame(Difficulty.FromName(difficulty)));
        FlagMode = false;
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Left click on a cell. In flag mode this places a flag instead.</summary>
    [HttpPost]
    public IActionResult Reveal(int row, int col)
    {
        var game = LoadOrCreateGame();
        if (FlagMode) game.ToggleFlag(row, col);
        else game.Reveal(row, col);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Right click on a cell (sent by site.js).</summary>
    [HttpPost]
    public IActionResult Flag(int row, int col)
    {
        LoadOrCreateGame().ToggleFlag(row, col);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult ToggleFlagMode()
    {
        FlagMode = !FlagMode;
        return RedirectToAction(nameof(Index));
    }

    private MinesweeperGame LoadOrCreateGame()
    {
        var game = store.Load(GameId);
        if (game is null)
        {
            game = new MinesweeperGame(Difficulty.Beginner);
            store.Save(GameId, game);
        }
        return game;
    }

    /// <summary>Per-browser id kept in the session cookie.</summary>
    private string GameId
    {
        get
        {
            var id = HttpContext.Session.GetString(GameIdKey);
            if (id is null)
            {
                id = Guid.NewGuid().ToString("N");
                HttpContext.Session.SetString(GameIdKey, id);
            }
            return id;
        }
    }

    private bool FlagMode
    {
        get => HttpContext.Session.GetString(FlagModeKey) == "1";
        set => HttpContext.Session.SetString(FlagModeKey, value ? "1" : "0");
    }
}
