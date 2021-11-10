using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinesweeperMVC.Controllers;

namespace MinesweeperMVC.Test.Controllers
{
    [TestClass()]
    public class HomeControllerTests
    {
        private readonly HomeController _homeController = new();

        [TestMethod()]
        public void HomeControllerMoveTest()
        {
            _homeController.Game();
            _homeController.Move(1, 2);
            Assert.AreEqual(1, _homeController.GetMinesweeperModel().Board[4][5]["Active"]);
        }

        [TestMethod()]
        public void GameStartedTest()
        {
            _homeController.Game();
            _homeController.Move(1, 2);
            Assert.AreEqual(true, _homeController.GetMinesweeperModel().GameStarted);
        }

        [TestMethod()]
        public void GameOverTest()
        {
            _homeController.Game();
            _homeController.Move(1, 2);
            Assert.AreEqual(false, _homeController.GetMinesweeperModel().GameOver());
        }

        [TestMethod()]
        public void BombCountTest()
        {
            _homeController.Game();
            Assert.AreEqual(150, _homeController.GetMinesweeperModel().BombCount);
        }

        [TestMethod()]
        public void FlagCountTest()
        {
            _homeController.Game();
            Assert.AreEqual(0, _homeController.GetMinesweeperModel().FlagCount);

            _homeController.Flag();
            _homeController.Move(1, 2);
            Assert.AreEqual(1, _homeController.GetMinesweeperModel().FlagCount);
        }

        [TestMethod()]
        public void FlagClickTest()
        {
            _homeController.Game();
            Assert.AreEqual(false, _homeController.GetMinesweeperModel().FlagClick);

            _homeController.Flag();
            Assert.AreEqual(true, _homeController.GetMinesweeperModel().FlagClick);
        }

        [TestMethod()]
        public void BombsNotFoundTest()
        {
            _homeController.Game();
            Assert.AreEqual(0, _homeController.GetMinesweeperModel().BombsNotFound);

            _homeController.Move(1, 2);
            Assert.AreEqual(150, _homeController.GetMinesweeperModel().BombsNotFound);
        }

        [TestMethod()]
        public void RowsTest()
        {
            _homeController.Game();
            Assert.AreEqual(30, _homeController.GetMinesweeperModel().Rows);
        }

        [TestMethod()]
        public void ColumnsTest()
        {
            _homeController.Game();
            Assert.AreEqual(100, _homeController.GetMinesweeperModel().Columns);
        }

        [TestMethod()]
        public void DisplayTest()
        {
            _homeController.Game();
            _homeController.Move(1, 2);
            Assert.AreEqual("_", _homeController.GetMinesweeperModel().Display(1,2));

            _homeController.Game();
            _homeController.Flag();
            _homeController.Move(1, 2);
            Assert.AreEqual("F", _homeController.GetMinesweeperModel().Display(1, 2));

            _homeController.Game();
            _homeController.Move(1, 2);

            int bombRow = 0, bombCol = 0;
            for (int i = 0; i < _homeController.GetMinesweeperModel().Rows; i++)
            {
                bombRow = i;
                for (int j = 0; j < _homeController.GetMinesweeperModel().Columns; j++)
                {
                    bombCol = j;
                    if (_homeController.GetMinesweeperModel().Board[i][j]["Bomb"] == 1)
                    {
                        break;
                    }
                }
            }

            Assert.AreEqual("[]", _homeController.GetMinesweeperModel().Display(bombRow, bombCol));

            _homeController.Move(bombRow, bombCol);
            Assert.AreEqual("B", _homeController.GetMinesweeperModel().Display(bombRow, bombCol));
        }
    }
}