using System;
using System.Collections.Generic;
using System.Linq;

namespace MinesweeperMVC.Models
{
    public class MinesweeperModel
    {
        public int Rows { get; set; }

        public int Columns { get; set; }

        public int BombCount { get; set; }

        public int BombsNotFound { get; set; }

        public int MoveCount { get; set; }

        public bool GameStarted { get; set; }

        public bool FlagClick { get; set;}

        public int FlagCount { get; set; }

        public List<List<Dictionary<string, int>>> Board { get; set; }

        public MinesweeperModel(int rows, int columns, int bombCount)
        {
            Rows = rows;
            Columns = columns;
            BombCount = bombCount;
            Board = new List<List<Dictionary<string, int>>>();
            for (int r = 0; r < Rows; r++)
            {
                var rowList = new List<Dictionary<string, int>>();
                for (int c = 0; c < Columns; c++)
                {
                    var individualMineDetails = new Dictionary<string, int>
                    {
                        {"Row", r},
                        {"Column", c},
                        {"Bomb", 0},
                        {"SurroundingBombs", 0},
                        {"Active", 0},
                        {"Flag", 0},
                        {"Symbol", 0}
                    };
                    rowList.Add(individualMineDetails);
                }
                Board.Add(rowList);
            }
        }

        public IEnumerable<Dictionary<string, int>> FindAll(string property)
        {
            return from x in Board from dictionary in x where dictionary[property] == 1 select dictionary;
        }

        public bool IsBoardFull()
        {
            var boardFull = Board.Where(x =>
            {
                return x.Any(y => y["Active"] == 0 && y["Flag"] == 0);
            });
            var flagQuery = FindAll("Flag");
            if (flagQuery.Select(flags => flags).All(dict => dict["Bomb"] == 0))
            {
                return false;
            }
            return !boardFull.Any();
        }

        public bool GameOver()
        {
            if (!IsBoardFull()) return false;
            var activeBombs = Board.Where(x =>
            {
                return x.Any(dict => dict["Bomb"] == 1 && dict["Active"] == 1);
            });
            return !activeBombs.Any() && BombCount == FlagCount;
        }

        public void CheckSurroundings(IDictionary<string, int> dict)
        {
            int row = dict["Row"];
            int col = dict["Column"];
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (j == 0 && i == 0)
                    {
                        continue;
                    }
                    int newRow = row + i;
                    int newCol = col + j;
                    if (IsInRange(newRow, newCol))
                    {
                        var tag = Board[newRow][newCol];
                        if (tag["Active"] == 0)
                        {
                            UncoverMine(tag);
                        }
                    }
                }
            }
        }

        public void UncoverMine(IDictionary<string, int> dict)
        {
            if (dict["Bomb"] == 1)
            {
                dict["Active"] = 1;
                GameOver();
            }
            else if (dict["SurroundingBombs"] == 0 && dict["Active"] == 0)
            {
                dict["Active"] = 1;
                dict["Symbol"] = 1;
                CheckSurroundings(dict);
            }
            else
            {
                dict["Active"] = 1;
                dict["Symbol"] = 2;
            }
        }

        public bool IsInRange(int i, int j)
        {
            return i >= 0
                   && i < Rows
                   && j >= 0
                   && j < Columns;
        }

        public void MineSelected(int row, int col)
        {
            MoveCount++;
            var currentMine = Board[row][col];
            if (currentMine["Active"] == 0)
            {
                if (FlagClick)
                {
                    if (currentMine["Flag"] == 0)
                    {
                        currentMine["Flag"] = 1;
                        BombsNotFound--;
                        FlagCount++;
                    }
                    else
                    {
                        currentMine["Flag"] = 0;
                        BombsNotFound++;
                        FlagCount--;
                    }
                }
                else if (!FlagClick && currentMine["Flag"] == 0)
                {
                    CreateBombs(currentMine);
                    UncoverMine(currentMine);
                }
            }
        }

        public void CreateBombs(IDictionary<string, int> senderTag)
        {
            if (GameStarted) return;
            while (BombsNotFound < BombCount)
            {
                int row = new Random().Next(0, Rows);
                int col = new Random().Next(0, Columns);
                var mine = Board[row][col];

                if (Board[row][col] != senderTag
                    && (Math.Abs(senderTag["Column"] - mine["Column"]) > 2 ||
                        Math.Abs(senderTag["Row"] - mine["Row"]) > 2)
                    && (Math.Abs(senderTag["Column"] - mine["Column"]) > 2 ||
                        Math.Abs(senderTag["Row"] - mine["Row"]) != 0)
                    && (Math.Abs(senderTag["Column"] - mine["Column"]) != 0 ||
                        Math.Abs(senderTag["Row"] - mine["Row"]) > 2)
                    && mine["Bomb"] == 0)
                {
                    mine["Bomb"] = 1;
                    BombsNotFound++;
                }
            }
            SetSurroundingBombs();
            GameStarted = true;
        }

        public void SetSurroundingBombs()
        {
            var directions = new List<(int, int)> { (-1, -1), (-1, 0), (-1, 1), (0, 1), (1, 1), (1, 0), (1, -1), (0, -1) };
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    var mine = Board[row][col];
                    int bombs = (from direction in directions
                                 where IsInRange(row + direction.Item1, col + direction.Item2)
                                 select Board[row + direction.Item1][col + direction.Item2]
                                 into surroundingTag
                                 where
                 Math.Abs(mine["Column"] - surroundingTag["Column"]) == 1 &&
                 Math.Abs(mine["Row"] - surroundingTag["Row"]) == 1 ||
                 Math.Abs(mine["Column"] - surroundingTag["Column"]) == 1 &&
                 Math.Abs(mine["Row"] - surroundingTag["Row"]) == 0 ||
                 Math.Abs(mine["Column"] - surroundingTag["Column"]) == 0 &&
                 Math.Abs(mine["Row"] - surroundingTag["Row"]) == 1
                                 select surroundingTag).Count(surroundingMines => surroundingMines["Bomb"] == 1);

                    mine["SurroundingBombs"] = bombs;
                }
            }
        }

        public string Display(in int row, in int col)
        {
            if (Board[row][col]["Bomb"] == 1 && Board[row][col]["Active"] == 1)
            {
                return "B";
            }

            if (Board[row][col]["Flag"] == 1)
            {
                return "F";
            }

            if (Board[row][col]["SurroundingBombs"] != 0 && Board[row][col]["Symbol"] == 2)
            {
                return Board[row][col]["SurroundingBombs"].ToString();
            }

            return Board[row][col]["Symbol"] == 1 ? "_" : "[]";
        }
    }
}
