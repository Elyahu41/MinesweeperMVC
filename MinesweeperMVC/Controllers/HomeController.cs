using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MinesweeperMVC.Models;

namespace MinesweeperMVC.Controllers
{
    public class HomeController : Controller
    {
        private static MinesweeperModel _mm = new(30, 100, 150);// store as a session variable

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult GameOverLosePage()
        {
            _mm = new MinesweeperModel(30, 100, 150);
            return View();
        }

        public IActionResult GameOverWinPage()
        {
            _mm = new MinesweeperModel(30, 100, 150);
            return View();
        }

        public IActionResult Game()
        {
            _mm = new MinesweeperModel(30, 100, 150);
            return View(model: _mm);
        }

        public IActionResult Move(int row, int col)
        {
            _mm.MineSelected(row,col);
            return View("Game", _mm);
        }
        
        public IActionResult Flag()
        {
            _mm.FlagClick = !_mm.FlagClick;
            return View("Game", _mm);
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public MinesweeperModel GetMinesweeperModel()
        {
            return _mm;
        }
    }
}
