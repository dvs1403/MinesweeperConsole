using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace MinesweeperConsole
{
    public interface IGame
    {
        string GetGame();
        void Play();
    }
    public struct Data
    {
        public int TotalBombs;
        public int TotalTiles;
        public char[,] DisplayedMap;
        public List<(int, int)> ZeroOpened;
    }
    public interface IMemento
    {
        Data GetState();

        DateTime GetDate();
    }
    class SaveGame : IMemento
    {
        private Data _data;

        private DateTime _date;

        public SaveGame(Data data)
        {
            this._data = data;
            this._date = DateTime.Now;
        }
        public Data GetState()
        {
            return this._data;
        }
        public DateTime GetDate()
        {
            return this._date;
        }
    }
    public class Caretaker
    {
        private List<IMemento> _mementos = new List<IMemento>();

        private SingleGame _game = null;

        public Caretaker(SingleGame game)
        {
            this._game = game;
        }
        public void Backup()
        {
            this._mementos.Add(this._game.Save());
        }
        public void Undo()
        {
            if (this._mementos.Count == 0)
            {
                return;
            }

            var memento = this._mementos.Last();
            this._mementos.Remove(memento);

            try
            {
                this._game.Restore(memento);
            }
            catch (Exception)
            {
                this.Undo();
            }
        }
        public void ClearList()
        {
            _mementos.Clear();
        }
    }
    public class SingleGame : IGame
    {
        private int playerPositionX = 2, playerPositionY = 1;
        private int TotalBombs = 0;
        private int TotalTiles = 81;
        private char playerSymbol = '#';
        private char[,] HideMap;
        private char[,] DisplayedMap;
        private char[] bordersSymbols = { '+', '-', '|' };
        private char[] tilesSymbols = { '▓', '░', 'F', 'B' };
        private Dictionary<char, ConsoleColor> SymbolsColors = new Dictionary<char, ConsoleColor>();
        private List<(int, int)> ZeroOpened = new List<(int, int)>();
        public Caretaker caretaker;

        public SingleGame()
        {
            StartGame();
        }
        public string GetGame()
        {
            return "Single Player Game";
        }
        public void Play()
        {
            Caretaker caretaker = new Caretaker(this);
            this.caretaker = caretaker;
            while (true)
            {
                RenderMap();
                PlayerMainController();
                Thread.Sleep(10);
            }
            Console.ReadLine();
        }
        public void StartGame()
        {
            TotalBombs = 0;
            TotalTiles = 81;
            SymbolsColors.Clear();
            SeedColors();
            HideMap = GenerateHideMap(37, 19);
            DisplayedMap = GenerateDisplayedMap(37, 19);
            ZeroOpened.Clear();
            playerPositionX = 2;
            playerPositionY = 1;
        }
        public void SeedColors()
        {
            AssignСolor('F', ConsoleColor.Cyan);
            AssignСolor('B', ConsoleColor.Red);
            AssignСolor('+', ConsoleColor.Green);
            AssignСolor('-', ConsoleColor.Green);
            AssignСolor('|', ConsoleColor.Green);
            AssignСolor('#', ConsoleColor.Yellow);
        }
        public void AssignСolor(char symbol, ConsoleColor color)
        {
            SymbolsColors.Add(symbol, color);
        }
        public ConsoleColor ReadColor(char key)
        {
            bool keyFound = false;
            foreach (char k in SymbolsColors.Keys)
            {
                if (k == key)
                    keyFound = true;
            }
            if (keyFound == false)
                throw new ArgumentOutOfRangeException();
            return SymbolsColors[key];
        }
        public void RenderMap()
        {
            for (int x = 0; x < DisplayedMap.GetLength(0); x++)
            {
                for (int y = 0; y < DisplayedMap.GetLength(1); y++)
                {
                    if (SymbolsColors.ContainsKey(DisplayedMap[x, y]))
                    {
                        Console.ForegroundColor = SymbolsColors[DisplayedMap[x, y]];
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    Console.SetCursorPosition(x, y);
                    Console.Write(DisplayedMap[x, y]);
                }
            }
            if (SymbolsColors.ContainsKey(playerSymbol))
            {
                Console.ForegroundColor = SymbolsColors[playerSymbol];
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.SetCursorPosition(playerPositionX, playerPositionY);
            Console.Write(playerSymbol);

            Console.SetCursorPosition(39, 1);
            Console.Write($"Всего Бомб - {TotalBombs} ");
            Console.SetCursorPosition(39, 2);
            Console.Write($"Всего Плиток Осталось - {TotalTiles} ");
        }
        public char[,] GenerateDisplayedMap(int width, int height)
        {
            char[,] result = new char[width, height];
            Random r = new Random();
            int x1 = -1;
            int y1 = -1;
            result[0, 0] = '+';
            for (int x = 1; x < result.GetLength(0); x++)
            {
                x1++;
                if (x1 == 0)
                    result[x, 0] = '-';
                else if (x1 == 1)
                    result[x, 0] = '-';
                else if (x1 == 2)
                    result[x, 0] = '-';
                else if (x1 == 3)
                    result[x, 0] = '+';
                if (x1 == 3)
                    x1 = -1;
            }
            for (int y = 1; y < result.GetLength(1); y++)
            {
                y1++;
                if (y1 == 0)
                    result[0, y] = '|';
                else if (y1 == 1)
                    result[0, y] = '+';
                if (y1 == 1)
                    y1 = -1;
            }
            for (int x = 1; x < result.GetLength(0); x++)
            {
                x1++;
                for (int y = 1; y < result.GetLength(1); y++)
                {
                    y1++;
                    if (x1 == 0 && y1 == 1)
                        result[x, y] = '-';
                    else if (x1 == 0 && y1 == 0)
                        result[x, y] = ' ';
                    else if (x1 == 1 && y1 == 1)
                        result[x, y] = '-';
                    else if (x1 == 1 && y1 == 0)
                        result[x, y] = '▓';
                    else if (x1 == 2 && y1 == 1)
                        result[x, y] = '-';
                    else if (x1 == 2 && y1 == 0)
                        result[x, y] = ' ';
                    else if (x1 == 3 && y1 == 1)
                        result[x, y] = '+';
                    else if (x1 == 3 && y1 == 0)
                        result[x, y] = '|';

                    if (y1 == 1)
                        y1 = -1;
                }

                if (x1 == 3)
                    x1 = -1;
            }
            return result;
        }
        public char[,] GenerateHideMap(int width, int height)
        {
            char[,] result = new char[width, height];
            Random r = new Random();
            for (int x = 2; x < result.GetLength(0); x += 4)
            {
                for (int y = 1; y < result.GetLength(1); y += 2)
                {
                    if (r.Next(0, 5) == 1)
                    {
                        result[x, y] = 'B';
                        TotalBombs++;
                    }
                    else
                        result[x, y] = ' ';
                }
            }
            int CountBomb = 0;
            for (int x = 2; x < result.GetLength(0); x += 4)
            {
                for (int y = 1; y < result.GetLength(1); y += 2)
                {
                    if (result[x, y] != 'B')
                    {
                        for (int x1 = x - 4; x1 <= x + 4; x1 += 4)
                        {
                            if (x1 > 0 && x1 < 37)
                            {
                                for (int y1 = y - 2; y1 <= y + 2; y1 += 2)
                                {
                                    if (y1 > 0 && y1 < 19)
                                    {
                                        if (result[x1, y1] == 'B')
                                            CountBomb++;
                                    }
                                }
                            }
                        }
                        result[x, y] = Convert.ToChar(CountBomb + 48);
                        CountBomb = 0;
                    }
                }
            }
            return result;
        }
        public void PlayerMainController()
        {
            PlayerMoveController();
        }
        public void PlayerMoveController()
        {
            ConsoleKeyInfo key = Console.ReadKey();

            switch (key.Key)
            {
                case ConsoleKey.LeftArrow:
                    MovePlayer(-4, 0);
                    break;
                case ConsoleKey.RightArrow:
                    MovePlayer(4, 0);
                    break;
                case ConsoleKey.DownArrow:
                    MovePlayer(0, 2);
                    break;
                case ConsoleKey.UpArrow:
                    MovePlayer(0, -2);
                    break;
                case ConsoleKey.Spacebar:
                    caretaker.Backup();
                    ButtonOpen();
                    break;
                case ConsoleKey.F:
                    caretaker.Backup();
                    ButtonFlag();
                    break;
                case ConsoleKey.B:
                    caretaker.Undo();
                    break;
                case ConsoleKey.Escape:
                    Restart();
                    break;
            }
        }
        public void MovePlayer(int x, int y)
        {
            int targetX = playerPositionX + x,
                targetY = playerPositionY + y;
            if (CheckCollision(targetX, targetY))
            {
                playerPositionX = targetX;
                playerPositionY = targetY;
            }
        }
        public (int, int) GetPlayerCoordinates()
        {
            return (playerPositionX, playerPositionY);
        }
        public bool CheckCollision(int x, int y)
        {
            bool canMove = true;

            if ((x < 1 || x > 36) || (y < 1 || y > 18))
            {
                canMove = false;
            }

            return canMove;

        }
        public void ButtonOpen()
        {
            if (HideMap[playerPositionX, playerPositionY] == 'B')
            {
                DisplayedMap[playerPositionX, playerPositionY] = HideMap[playerPositionX, playerPositionY];
                playerPositionX = 2;
                playerPositionY = 1;
                GameOver();
            }
            else
            {
                if (DisplayedMap[playerPositionX, playerPositionY] != '▓')
                {
                    OpenSquare(playerPositionX, playerPositionY);
                }
                if (DisplayedMap[playerPositionX, playerPositionY] != 'F')
                {
                    if (DisplayedMap[playerPositionX, playerPositionY] == '▓')
                    {
                        TotalTiles--;
                        CheckWin();
                    }
                    DisplayedMap[playerPositionX, playerPositionY] = HideMap[playerPositionX, playerPositionY];
                }

                if (HideMap[playerPositionX, playerPositionY] == '0')
                {
                    ZeroOpened.Add((playerPositionX, playerPositionY));
                    OpenZero(playerPositionX, playerPositionY);
                }
            }
        }
        public char[,] GetHideMap()
        {
            return HideMap;
        }
        public char[,] GetDisplayedMap()
        {
            return DisplayedMap;
        }
        public void OpenZero(int x, int y)
        {
            for (int x1 = x - 4; x1 <= x + 4; x1 += 4)
            {
                if (x1 > 0 && x1 < 37)
                {
                    for (int y1 = y - 2; y1 <= y + 2; y1 += 2)
                    {
                        if (y1 > 0 && y1 < 19)
                        {
                            if (DisplayedMap[x1, y1] == '▓')
                            {
                                TotalTiles--;
                                CheckWin();
                            }
                            DisplayedMap[x1, y1] = HideMap[x1, y1];
                            if (HideMap[x1, y1] == '0')
                            {
                                bool reiteration = false;
                                foreach ((int, int) p in ZeroOpened)
                                {
                                    if ((x1, y1) == p)
                                        reiteration = true;
                                }
                                if (reiteration == false)
                                {
                                    ZeroOpened.Add((x1, y1));
                                    OpenZero(x1, y1);
                                }
                                reiteration = false;
                            }
                        }
                    }
                }
            }
        }
        public void OpenSquare(int x, int y)
        {
            int CountFlag = 0;
            for (int x1 = x - 4; x1 <= x + 4; x1 += 4)
            {
                if (x1 > 0 && x1 < 37)
                {
                    for (int y1 = y - 2; y1 <= y + 2; y1 += 2)
                    {
                        if (y1 > 0 && y1 < 19)
                        {
                            if (DisplayedMap[x1, y1] == 'F')
                                CountFlag++;
                        }
                    }
                }
            }
            if (CountFlag != (int)DisplayedMap[x, y] - 48)
                return;
            for (int x1 = x - 4; x1 <= x + 4; x1 += 4)
            {
                if (x1 > 0 && x1 < 37)
                {
                    for (int y1 = y - 2; y1 <= y + 2; y1 += 2)
                    {
                        if (y1 > 0 && y1 < 19)
                        {
                            if (DisplayedMap[x1, y1] == '▓')
                            {
                                TotalTiles--;
                                CheckWin();
                            }
                            if (DisplayedMap[x1, y1] == 'F')
                                continue;
                            DisplayedMap[x1, y1] = HideMap[x1, y1];
                            if (HideMap[x1, y1] == '0')
                            {
                                bool reiteration = false;
                                foreach ((int, int) p in ZeroOpened)
                                {
                                    if ((x1, y1) == p)
                                        reiteration = true;
                                }
                                if (reiteration == false)
                                {
                                    ZeroOpened.Add((x1, y1));
                                    OpenZero(x1, y1);
                                }
                                reiteration = false;
                            }
                            if (HideMap[x1, y1] == 'B')
                                GameOver();
                        }
                    }
                }
            }
        }
        public void ButtonFlag()
        {
            if (DisplayedMap[playerPositionX, playerPositionY] == '▓')
            {
                if (TotalBombs != 0)
                {
                    DisplayedMap[playerPositionX, playerPositionY] = 'F';
                    TotalBombs--;
                    TotalTiles--;
                    CheckWin();
                }
            }
            else if (DisplayedMap[playerPositionX, playerPositionY] == 'F')
            {
                DisplayedMap[playerPositionX, playerPositionY] = '▓';
                TotalBombs++;
                TotalTiles++;
            }
        }
        public void GameOver()
        {
            Console.SetCursorPosition(0, 19);
            Console.WriteLine("GameOver!");
        }
        public void CheckWin()
        {
            if (TotalTiles == 0)
                YouWin();
        }
        public void YouWin()
        {
            Console.SetCursorPosition(0, 19);
            Console.WriteLine("YouWin!");
        }
        public IMemento Save()
        {
            Data _data;
            _data.TotalTiles = TotalTiles;
            _data.TotalBombs = TotalBombs;
            _data.DisplayedMap = new char[37, 19];
            for (int i = 0; i < DisplayedMap.GetLength(0); i++)
            {
                for (int j = 0; j < DisplayedMap.GetLength(1); j++)
                {
                    _data.DisplayedMap[i, j] = DisplayedMap[i, j];
                }
            }
            _data.ZeroOpened = new List<(int, int)>();
            for (int i = 0; i < ZeroOpened.Count; i++)
            {
                _data.ZeroOpened.Add(ZeroOpened[i]);
            }
            return new SaveGame(_data);
        }
        public void Restore(IMemento memento)
        {
            if (!(memento is SaveGame))
            {
                throw new Exception("Unknown memento class " + memento.ToString());
            }

            Data _data = memento.GetState();
            TotalBombs = _data.TotalBombs;
            TotalTiles = _data.TotalTiles;
            for (int i = 0; i < DisplayedMap.GetLength(0); i++)
            {
                for (int j = 0; j < DisplayedMap.GetLength(1); j++)
                {
                    DisplayedMap[i, j] = _data.DisplayedMap[i, j];
                }
            }
            ZeroOpened = _data.ZeroOpened;
            Console.SetCursorPosition(0, 19);
            Console.WriteLine("               ");
        }
        public void Restart()
        {
            StartGame();
            caretaker.ClearList();
            Console.SetCursorPosition(0, 19);
            Console.WriteLine("               ");
        }
    }
    public class MultiGame : IGame
    {
        private int playerPositionX1 = 2, playerPositionY1 = 1;
        private int TotalBombs1 = 0;
        private int TotalTiles1 = 81;
        private int playerPositionX2 = 52, playerPositionY2 = 1;
        private int TotalBombs2 = 0;
        private int TotalTiles2 = 81;
        private char playerSymbol = '#';
        private char[,] HideMap1;
        private char[,] DisplayedMap1;
        private char[,] HideMap2;
        private char[,] DisplayedMap2;
        private char[] bordersSymbols = { '+', '-', '|' };
        private char[] tilesSymbols = { '▓', '░', 'F', 'B' };
        private Dictionary<char, ConsoleColor> SymbolsColors = new Dictionary<char, ConsoleColor>();
        private List<(int, int)> ZeroOpened1 = new List<(int, int)>();
        private List<(int, int)> ZeroOpened2 = new List<(int, int)>();

        public MultiGame()
        {
            StartGame();
        }
        public string GetGame()
        {
            return "Multiplayer Game";
        }
        public void Play()
        {
            while (true)
            {
                RenderMap();
                PlayerMainController();
                Thread.Sleep(10);
            }
            Console.ReadLine();
        }
        public void StartGame()
        {
            TotalBombs1 = 0;
            TotalTiles1 = 81;
            TotalBombs2 = 0;
            TotalTiles2 = 81;
            SymbolsColors.Clear();
            SeedColors();
            HideMap1 = GenerateHideMap(37, 19,1);
            DisplayedMap1 = GenerateDisplayedMap(37, 19);
            Thread.Sleep(50);
            HideMap2 = GenerateHideMap(37, 19,2);
            DisplayedMap2 = GenerateDisplayedMap(37, 19);
            ZeroOpened1.Clear();
            ZeroOpened2.Clear();
            playerPositionX1 = 2;
            playerPositionY1 = 1;
            playerPositionX2 = 52;
            playerPositionY2 = 1;
        }
        public void SeedColors()
        {
            AssignСolor('F', ConsoleColor.Cyan);
            AssignСolor('B', ConsoleColor.Red);
            AssignСolor('+', ConsoleColor.Green);
            AssignСolor('-', ConsoleColor.Green);
            AssignСolor('|', ConsoleColor.Green);
            AssignСolor('#', ConsoleColor.Yellow);
        }
        public void AssignСolor(char symbol, ConsoleColor color)
        {
            SymbolsColors.Add(symbol, color);
        }
        public ConsoleColor ReadColor(char key)
        {
            bool keyFound = false;
            foreach (char k in SymbolsColors.Keys)
            {
                if (k == key)
                    keyFound = true;
            }
            if (keyFound == false)
                throw new ArgumentOutOfRangeException();
            return SymbolsColors[key];
        }
        public void RenderMap()
        {
            for (int x = 0; x < DisplayedMap1.GetLength(0); x++)
            {
                for (int y = 0; y < DisplayedMap1.GetLength(1); y++)
                {
                    if (SymbolsColors.ContainsKey(DisplayedMap1[x, y]))
                    {
                        Console.ForegroundColor = SymbolsColors[DisplayedMap1[x, y]];
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    Console.SetCursorPosition(x, y);
                    Console.Write(DisplayedMap1[x, y]);
                }
            }
            if (SymbolsColors.ContainsKey(playerSymbol))
            {
                Console.ForegroundColor = SymbolsColors[playerSymbol];
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.SetCursorPosition(playerPositionX1, playerPositionY1);
            Console.Write(playerSymbol);
            Console.SetCursorPosition(1, 19);
            Console.Write("Игрок 1");

            for (int x = 0; x < DisplayedMap2.GetLength(0); x++)
            {
                for (int y = 0; y < DisplayedMap2.GetLength(1); y++)
                {
                    if (SymbolsColors.ContainsKey(DisplayedMap2[x, y]))
                    {
                        Console.ForegroundColor = SymbolsColors[DisplayedMap2[x, y]];
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    Console.SetCursorPosition(x+50, y);
                    Console.Write(DisplayedMap2[x, y]);
                }
            }
            if (SymbolsColors.ContainsKey(playerSymbol))
            {
                Console.ForegroundColor = SymbolsColors[playerSymbol];
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.SetCursorPosition(playerPositionX2, playerPositionY2);
            Console.Write(playerSymbol);
            Console.SetCursorPosition(51, 19);
            Console.Write("Игрок 2");


            Console.SetCursorPosition(2, 22);
            Console.Write($"Всего Бомб (игрок 1) - {TotalBombs1} ");
            Console.SetCursorPosition(2, 23);
            Console.Write($"Всего Плиток Осталось (игрок 1) - {TotalTiles1} ");
            Console.SetCursorPosition(2, 24);
            Console.Write($"Всего Бомб (игрок 2) - {TotalBombs2} ");
            Console.SetCursorPosition(2, 25);
            Console.Write($"Всего Плиток Осталось (игрок 2) - {TotalTiles2} ");
        }
        public char[,] GenerateDisplayedMap(int width, int height)
        {
            char[,] result = new char[width, height];
            Random r = new Random();
            int x1 = -1;
            int y1 = -1;
            result[0, 0] = '+';
            for (int x = 1; x < result.GetLength(0); x++)
            {
                x1++;
                if (x1 == 0)
                    result[x, 0] = '-';
                else if (x1 == 1)
                    result[x, 0] = '-';
                else if (x1 == 2)
                    result[x, 0] = '-';
                else if (x1 == 3)
                    result[x, 0] = '+';
                if (x1 == 3)
                    x1 = -1;
            }
            for (int y = 1; y < result.GetLength(1); y++)
            {
                y1++;
                if (y1 == 0)
                    result[0, y] = '|';
                else if (y1 == 1)
                    result[0, y] = '+';
                if (y1 == 1)
                    y1 = -1;
            }
            for (int x = 1; x < result.GetLength(0); x++)
            {
                x1++;
                for (int y = 1; y < result.GetLength(1); y++)
                {
                    y1++;
                    if (x1 == 0 && y1 == 1)
                        result[x, y] = '-';
                    else if (x1 == 0 && y1 == 0)
                        result[x, y] = ' ';
                    else if (x1 == 1 && y1 == 1)
                        result[x, y] = '-';
                    else if (x1 == 1 && y1 == 0)
                        result[x, y] = '▓';
                    else if (x1 == 2 && y1 == 1)
                        result[x, y] = '-';
                    else if (x1 == 2 && y1 == 0)
                        result[x, y] = ' ';
                    else if (x1 == 3 && y1 == 1)
                        result[x, y] = '+';
                    else if (x1 == 3 && y1 == 0)
                        result[x, y] = '|';

                    if (y1 == 1)
                        y1 = -1;
                }

                if (x1 == 3)
                    x1 = -1;
            }
            return result;
        }
        public char[,] GenerateHideMap(int width, int height,int indexPlayer)
        {
            char[,] result = new char[width, height];
            Random r = new Random();
            for (int x = 2; x < result.GetLength(0); x += 4)
            {
                for (int y = 1; y < result.GetLength(1); y += 2)
                {
                    if (r.Next(0, 5) == 1)
                    {
                        result[x, y] = 'B';
                        if (indexPlayer == 1)
                            TotalBombs1++;
                        else
                            TotalBombs2++;
                    }
                    else
                        result[x, y] = ' ';
                }
            }
            int CountBomb = 0;
            for (int x = 2; x < result.GetLength(0); x += 4)
            {
                for (int y = 1; y < result.GetLength(1); y += 2)
                {
                    if (result[x, y] != 'B')
                    {
                        for (int x1 = x - 4; x1 <= x + 4; x1 += 4)
                        {
                            if (x1 > 0 && x1 < 37)
                            {
                                for (int y1 = y - 2; y1 <= y + 2; y1 += 2)
                                {
                                    if (y1 > 0 && y1 < 19)
                                    {
                                        if (result[x1, y1] == 'B')
                                            CountBomb++;
                                    }
                                }
                            }
                        }
                        result[x, y] = Convert.ToChar(CountBomb + 48);
                        CountBomb = 0;
                    }
                }
            }
            return result;
        }
        public void PlayerMainController()
        {
            PlayerMoveController();
        }
        public void PlayerMoveController()
        {
            ConsoleKeyInfo key = Console.ReadKey();

            switch (key.Key)
            {
                case ConsoleKey.LeftArrow:
                    MovePlayer(-4, 0,2);
                    break;
                case ConsoleKey.RightArrow:
                    MovePlayer(4, 0, 2);
                    break;
                case ConsoleKey.DownArrow:
                    MovePlayer(0, 2, 2);
                    break;
                case ConsoleKey.UpArrow:
                    MovePlayer(0, -2, 2);
                    break;
                case ConsoleKey.Spacebar:
                    ButtonOpen2();
                    break;
                case ConsoleKey.Enter:
                    ButtonFlag2();
                    break;

                case ConsoleKey.A:
                    MovePlayer(-4, 0, 1);
                    break;
                case ConsoleKey.D:
                    MovePlayer(4, 0, 1);
                    break;
                case ConsoleKey.S:
                    MovePlayer(0, 2, 1);
                    break;
                case ConsoleKey.W:
                    MovePlayer(0, -2, 1);
                    break;
                case ConsoleKey.G:
                    ButtonOpen1();
                    break;
                case ConsoleKey.F:
                    ButtonFlag1();
                    break;
                case ConsoleKey.Escape:
                    Restart();
                    break;
            }
        }
        public void MovePlayer(int x, int y,int indexPlayer)
        {
            if (indexPlayer == 1)
            {
                int targetX = playerPositionX1 + x,
                    targetY = playerPositionY1 + y;
                if (CheckCollision1(targetX, targetY))
                {
                    playerPositionX1 = targetX;
                    playerPositionY1 = targetY;
                }
            }
            else
            {
                int targetX = playerPositionX2 + x,
                    targetY = playerPositionY2 + y;
                if (CheckCollision2(targetX, targetY))
                {
                    playerPositionX2 = targetX;
                    playerPositionY2 = targetY;
                }
            }
        }
        public (int, int) GetPlayerCoordinates1()
        {
            return (playerPositionX1, playerPositionY1);
        }
        public (int, int) GetPlayerCoordinates2()
        {
            return (playerPositionX2, playerPositionY2);
        }
        public bool CheckCollision1(int x, int y)
        {
            bool canMove = true;

            if ((x < 1 || x > 36) || (y < 1 || y > 18))
            {
                canMove = false;
            }

            return canMove;

        }
        public bool CheckCollision2(int x, int y)
        {
            bool canMove = true;

            if ((x < 51 || x > 86) || (y < 1 || y > 18))
            {
                canMove = false;
            }

            return canMove;

        }
        public void ButtonOpen1()
        {
            if (HideMap1[playerPositionX1, playerPositionY1] == 'B')
            {
                DisplayedMap1[playerPositionX1, playerPositionY1] = HideMap1[playerPositionX1, playerPositionY1];
                playerPositionX1 = 2;
                playerPositionY1 = 1;
                GameOver(2);
            }
            else
            {
                if (DisplayedMap1[playerPositionX1, playerPositionY1] != '▓')
                {
                    OpenSquare1(playerPositionX1, playerPositionY1);
                }
                if (DisplayedMap1[playerPositionX1, playerPositionY1] != 'F')
                {
                    if (DisplayedMap1[playerPositionX1, playerPositionY1] == '▓')
                    {
                        TotalTiles1--;
                        CheckWin(1);
                    }
                    DisplayedMap1[playerPositionX1, playerPositionY1] = HideMap1[playerPositionX1, playerPositionY1];
                }

                if (HideMap1[playerPositionX1, playerPositionY1] == '0')
                {
                    ZeroOpened1.Add((playerPositionX1, playerPositionY1));
                    OpenZero1(playerPositionX1, playerPositionY1);
                }
            }
        }
        public void ButtonOpen2()
        {
            if (HideMap2[playerPositionX2 - 50, playerPositionY2] == 'B')
            {
                DisplayedMap2[playerPositionX2-50, playerPositionY2] = HideMap2[playerPositionX2 - 50, playerPositionY2];
                playerPositionX2 = 52;
                playerPositionY2 = 1;
                GameOver(1);
            }
            else
            {
                if (DisplayedMap2[playerPositionX2 - 50, playerPositionY2] != '▓')
                {
                    OpenSquare2(playerPositionX2 - 50, playerPositionY2);
                }
                if (DisplayedMap2[playerPositionX2 - 50, playerPositionY2] != 'F')
                {
                    if (DisplayedMap2[playerPositionX2 - 50, playerPositionY2] == '▓')
                    {
                        TotalTiles2--;
                        CheckWin(2);
                    }
                    DisplayedMap2[playerPositionX2 - 50, playerPositionY2] = HideMap2[playerPositionX2 - 50, playerPositionY2];
                }

                if (HideMap2[playerPositionX2 - 50, playerPositionY2] == '0')
                {
                    ZeroOpened2.Add((playerPositionX2 - 50, playerPositionY2));
                    OpenZero2(playerPositionX2 - 50, playerPositionY2);
                }
            }
        }
        public char[,] GetHideMap()
        {
            return HideMap1;
        }
        public char[,] GetDisplayedMap()
        {
            return DisplayedMap1;
        }
        public void OpenZero1(int x, int y)
        {
            for (int x1 = x - 4; x1 <= x + 4; x1 += 4)
            {
                if (x1 > 0 && x1 < 37)
                {
                    for (int y1 = y - 2; y1 <= y + 2; y1 += 2)
                    {
                        if (y1 > 0 && y1 < 19)
                        {
                            if (DisplayedMap1[x1, y1] == '▓')
                            {
                                TotalTiles1--;
                                CheckWin(1);
                            }
                            DisplayedMap1[x1, y1] = HideMap1[x1, y1];
                            if (HideMap1[x1, y1] == '0')
                            {
                                bool reiteration = false;
                                foreach ((int, int) p in ZeroOpened1)
                                {
                                    if ((x1, y1) == p)
                                        reiteration = true;
                                }
                                if (reiteration == false)
                                {
                                    ZeroOpened1.Add((x1, y1));
                                    OpenZero1(x1, y1);
                                }
                                reiteration = false;
                            }
                        }
                    }
                }
            }
        }
        public void OpenZero2(int x, int y)
        {
            for (int x1 = x - 4; x1 <= x + 4; x1 += 4)
            {
                if (x1 > 0 && x1 < 37)
                {
                    for (int y1 = y - 2; y1 <= y + 2; y1 += 2)
                    {
                        if (y1 > 0 && y1 < 19)
                        {
                            if (DisplayedMap2[x1, y1] == '▓')
                            {
                                TotalTiles2--;
                                CheckWin(2);
                            }
                            DisplayedMap2[x1, y1] = HideMap2[x1, y1];
                            if (HideMap2[x1, y1] == '0')
                            {
                                bool reiteration = false;
                                foreach ((int, int) p in ZeroOpened2)
                                {
                                    if ((x1, y1) == p)
                                        reiteration = true;
                                }
                                if (reiteration == false)
                                {
                                    ZeroOpened2.Add((x1, y1));
                                    OpenZero2(x1, y1);
                                }
                                reiteration = false;
                            }
                        }
                    }
                }
            }
        }
        public void OpenSquare1(int x, int y)
        {
            int CountFlag = 0;
            for (int x1 = x - 4; x1 <= x + 4; x1 += 4)
            {
                if (x1 > 0 && x1 < 37)
                {
                    for (int y1 = y - 2; y1 <= y + 2; y1 += 2)
                    {
                        if (y1 > 0 && y1 < 19)
                        {
                            if (DisplayedMap1[x1, y1] == 'F')
                                CountFlag++;
                        }
                    }
                }
            }
            if (CountFlag != (int)DisplayedMap1[x, y] - 48)
                return;
            for (int x1 = x - 4; x1 <= x + 4; x1 += 4)
            {
                if (x1 > 0 && x1 < 37)
                {
                    for (int y1 = y - 2; y1 <= y + 2; y1 += 2)
                    {
                        if (y1 > 0 && y1 < 19)
                        {
                            if (DisplayedMap1[x1, y1] == '▓')
                            {
                                TotalTiles1--;
                                CheckWin(1);
                            }
                            if (DisplayedMap1[x1, y1] == 'F')
                                continue;
                            DisplayedMap1[x1, y1] = HideMap1[x1, y1];
                            if (HideMap1[x1, y1] == '0')
                            {
                                bool reiteration = false;
                                foreach ((int, int) p in ZeroOpened1)
                                {
                                    if ((x1, y1) == p)
                                        reiteration = true;
                                }
                                if (reiteration == false)
                                {
                                    ZeroOpened1.Add((x1, y1));
                                    OpenZero1(x1, y1);
                                }
                                reiteration = false;
                            }
                            if (HideMap1[x1, y1] == 'B')
                                GameOver(2);
                        }
                    }
                }
            }
        }
        public void OpenSquare2(int x, int y)
        {
            int CountFlag = 0;
            for (int x1 = x - 4; x1 <= x + 4; x1 += 4)
            {
                if (x1 > 0 && x1 < 37)
                {
                    for (int y1 = y - 2; y1 <= y + 2; y1 += 2)
                    {
                        if (y1 > 0 && y1 < 19)
                        {
                            if (DisplayedMap2[x1, y1] == 'F')
                                CountFlag++;
                        }
                    }
                }
            }
            if (CountFlag != (int)DisplayedMap2[x, y] - 48)
                return;
            for (int x1 = x - 4; x1 <= x + 4; x1 += 4)
            {
                if (x1 > 0 && x1 < 37)
                {
                    for (int y1 = y - 2; y1 <= y + 2; y1 += 2)
                    {
                        if (y1 > 0 && y1 < 19)
                        {
                            if (DisplayedMap2[x1, y1] == '▓')
                            {
                                TotalTiles2--;
                                CheckWin(2);
                            }
                            if (DisplayedMap2[x1, y1] == 'F')
                                continue;
                            DisplayedMap2[x1, y1] = HideMap2[x1, y1];
                            if (HideMap2[x1, y1] == '0')
                            {
                                bool reiteration = false;
                                foreach ((int, int) p in ZeroOpened2)
                                {
                                    if ((x1, y1) == p)
                                        reiteration = true;
                                }
                                if (reiteration == false)
                                {
                                    ZeroOpened2.Add((x1, y1));
                                    OpenZero2(x1, y1);
                                }
                                reiteration = false;
                            }
                            if (HideMap2[x1, y1] == 'B')
                                GameOver(1);
                        }
                    }
                }
            }
        }
        public void ButtonFlag1()
        {
            if (DisplayedMap1[playerPositionX1, playerPositionY1] == '▓')
            {
                if (TotalBombs1 != 0)
                {
                    DisplayedMap1[playerPositionX1, playerPositionY1] = 'F';
                    TotalBombs1--;
                    TotalTiles1--;
                    CheckWin(1);
                }
            }
            else if (DisplayedMap1[playerPositionX1, playerPositionY1] == 'F')
            {
                DisplayedMap1[playerPositionX1, playerPositionY1] = '▓';
                TotalBombs1++;
                TotalTiles1++;
            }
        }
        public void ButtonFlag2()
        {
            if (DisplayedMap2[playerPositionX2 - 50, playerPositionY2] == '▓')
            {
                if (TotalBombs2 != 0)
                {
                    DisplayedMap2[playerPositionX2 - 50, playerPositionY2] = 'F';
                    TotalBombs2--;
                    TotalTiles2--;
                    CheckWin(2);
                }
            }
            else if (DisplayedMap2[playerPositionX2 - 50, playerPositionY2] == 'F')
            {
                DisplayedMap2[playerPositionX2 - 50, playerPositionY2] = '▓';
                TotalBombs2++;
                TotalTiles2++;
            }
        }
        public void GameOver(int indexPlayer)
        {
            Console.SetCursorPosition(0, 27);
            Console.WriteLine($"Winner Player {indexPlayer}!");
        }
        public void CheckWin(int indexPlayer)
        {
            if (indexPlayer == 1)
            {
                if (TotalTiles1 == 0)
                    YouWin(indexPlayer);
            }
            else
            {
                if (TotalTiles2 == 0)
                    YouWin(indexPlayer);
            }
        }
        public void YouWin(int indexPlayer)
        {
            Console.SetCursorPosition(0, 27);
            Console.WriteLine($"Winner Player {indexPlayer}!");
        }
        public void Restart()
        {
            StartGame();
            Console.SetCursorPosition(0, 27);
            Console.WriteLine("                         ");
        }
    }
    public class AIGame : IGame
    {
        private int playerPositionX1 = 2, playerPositionY1 = 1;
        private int TotalBombs1 = 0;
        private int TotalTiles1 = 81;
        private int playerPositionX2 = 52, playerPositionY2 = 1;
        private int TotalBombs2 = 0;
        private int TotalTiles2 = 81;
        private char playerSymbol = '#';
        private char[,] HideMap1;
        private char[,] DisplayedMap1;
        private char[,] HideMap2;
        private char[,] DisplayedMap2;
        private char[] bordersSymbols = { '+', '-', '|' };
        private char[] tilesSymbols = { '▓', '░', 'F', 'B' };
        private Dictionary<char, ConsoleColor> SymbolsColors = new Dictionary<char, ConsoleColor>();
        private List<(int, int)> ZeroOpened1 = new List<(int, int)>();
        private List<(int, int)> ZeroOpened2 = new List<(int, int)>();

        private bool gameOver = false;
        private int indexWinner = 0;
        static object locker = new object();
        public AIGame()
        {
            StartGame();
        }
        public string GetGame()
        {
            return "Multiplayer Game";
        }
        public void Play()
        {
            Thread myThread = new Thread(new ThreadStart(AiAlgorithm));
            myThread.Start();
            while (true)
            {

                PlayerMainController();
                RenderMap();

                Thread.Sleep(10);
            }
            Console.ReadLine();

        }
        public void StartGame()
        {
            TotalBombs1 = 0;
            TotalTiles1 = 81;
            TotalBombs2 = 0;
            TotalTiles2 = 81;
            SymbolsColors.Clear();
            SeedColors();
            HideMap1 = GenerateHideMap(37, 19, 1);
            DisplayedMap1 = GenerateDisplayedMap(37, 19);
            Thread.Sleep(50);
            HideMap2 = GenerateHideMap(37, 19, 2);
            DisplayedMap2 = GenerateDisplayedMap(37, 19);
            ZeroOpened1.Clear();
            ZeroOpened2.Clear();
            playerPositionX1 = 2;
            playerPositionY1 = 1;
            playerPositionX2 = 52;
            playerPositionY2 = 1;
            gameOver = false;
        }
        public void SeedColors()
        {
            AssignСolor('F', ConsoleColor.Cyan);
            AssignСolor('B', ConsoleColor.Red);
            AssignСolor('+', ConsoleColor.Green);
            AssignСolor('-', ConsoleColor.Green);
            AssignСolor('|', ConsoleColor.Green);
            AssignСolor('#', ConsoleColor.Yellow);
        }
        public void AssignСolor(char symbol, ConsoleColor color)
        {
            SymbolsColors.Add(symbol, color);
        }
        public ConsoleColor ReadColor(char key)
        {
            bool keyFound = false;
            foreach (char k in SymbolsColors.Keys)
            {
                if (k == key)
                    keyFound = true;
            }
            if (keyFound == false)
                throw new ArgumentOutOfRangeException();
            return SymbolsColors[key];
        }
        public void RenderMap()
        {
            lock (locker)
            {
                for (int x = 0; x < DisplayedMap1.GetLength(0); x++)
                {
                    for (int y = 0; y < DisplayedMap1.GetLength(1); y++)
                    {
                        if (SymbolsColors.ContainsKey(DisplayedMap1[x, y]))
                        {
                            Console.ForegroundColor = SymbolsColors[DisplayedMap1[x, y]];
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        Console.SetCursorPosition(x, y);
                        Console.Write(DisplayedMap1[x, y]);
                    }
                }
                if (SymbolsColors.ContainsKey(playerSymbol))
                {
                    Console.ForegroundColor = SymbolsColors[playerSymbol];
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.SetCursorPosition(playerPositionX1, playerPositionY1);
                Console.Write(playerSymbol);
                Console.SetCursorPosition(1, 19);
                Console.Write("Игрок 1");

                for (int x = 0; x < DisplayedMap2.GetLength(0); x++)
                {
                    for (int y = 0; y < DisplayedMap2.GetLength(1); y++)
                    {
                        if (SymbolsColors.ContainsKey(DisplayedMap2[x, y]))
                        {
                            Console.ForegroundColor = SymbolsColors[DisplayedMap2[x, y]];
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        Console.SetCursorPosition(x + 50, y);
                        Console.Write(DisplayedMap2[x, y]);
                    }
                }
                if (SymbolsColors.ContainsKey(playerSymbol))
                {
                    Console.ForegroundColor = SymbolsColors[playerSymbol];
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.SetCursorPosition(playerPositionX2, playerPositionY2);
                Console.Write(playerSymbol);
                Console.SetCursorPosition(51, 19);
                Console.Write("Игрок 2");


                Console.SetCursorPosition(2, 22);
                Console.Write($"Всего Бомб (игрок 1) - {TotalBombs1} ");
                Console.SetCursorPosition(2, 23);
                Console.Write($"Всего Плиток Осталось (игрок 1) - {TotalTiles1} ");
                Console.SetCursorPosition(2, 24);
                Console.Write($"Всего Бомб (игрок 2) - {TotalBombs2} ");
                Console.SetCursorPosition(2, 25);
                Console.Write($"Всего Плиток Осталось (игрок 2) - {TotalTiles2} ");

                if(gameOver)
                {
                    Console.SetCursorPosition(0, 27);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Winner Player {indexWinner}!");
                }
            }
        }
        public char[,] GenerateDisplayedMap(int width, int height)
        {
            char[,] result = new char[width, height];
            Random r = new Random();
            int x1 = -1;
            int y1 = -1;
            result[0, 0] = '+';
            for (int x = 1; x < result.GetLength(0); x++)
            {
                x1++;
                if (x1 == 0)
                    result[x, 0] = '-';
                else if (x1 == 1)
                    result[x, 0] = '-';
                else if (x1 == 2)
                    result[x, 0] = '-';
                else if (x1 == 3)
                    result[x, 0] = '+';
                if (x1 == 3)
                    x1 = -1;
            }
            for (int y = 1; y < result.GetLength(1); y++)
            {
                y1++;
                if (y1 == 0)
                    result[0, y] = '|';
                else if (y1 == 1)
                    result[0, y] = '+';
                if (y1 == 1)
                    y1 = -1;
            }
            for (int x = 1; x < result.GetLength(0); x++)
            {
                x1++;
                for (int y = 1; y < result.GetLength(1); y++)
                {
                    y1++;
                    if (x1 == 0 && y1 == 1)
                        result[x, y] = '-';
                    else if (x1 == 0 && y1 == 0)
                        result[x, y] = ' ';
                    else if (x1 == 1 && y1 == 1)
                        result[x, y] = '-';
                    else if (x1 == 1 && y1 == 0)
                        result[x, y] = '▓';
                    else if (x1 == 2 && y1 == 1)
                        result[x, y] = '-';
                    else if (x1 == 2 && y1 == 0)
                        result[x, y] = ' ';
                    else if (x1 == 3 && y1 == 1)
                        result[x, y] = '+';
                    else if (x1 == 3 && y1 == 0)
                        result[x, y] = '|';

                    if (y1 == 1)
                        y1 = -1;
                }

                if (x1 == 3)
                    x1 = -1;
            }
            return result;
        }
        public char[,] GenerateHideMap(int width, int height, int indexPlayer)
        {
            char[,] result = new char[width, height];
            Random r = new Random();
            for (int x = 2; x < result.GetLength(0); x += 4)
            {
                for (int y = 1; y < result.GetLength(1); y += 2)
                {
                    if (r.Next(0, 10) == 1)
                    {
                        result[x, y] = 'B';
                        if (indexPlayer == 1)
                            TotalBombs1++;
                        else
                            TotalBombs2++;
                    }
                    else
                        result[x, y] = ' ';
                }
            }
            int CountBomb = 0;
            for (int x = 2; x < result.GetLength(0); x += 4)
            {
                for (int y = 1; y < result.GetLength(1); y += 2)
                {
                    if (result[x, y] != 'B')
                    {
                        for (int x1 = x - 4; x1 <= x + 4; x1 += 4)
                        {
                            if (x1 > 0 && x1 < 37)
                            {
                                for (int y1 = y - 2; y1 <= y + 2; y1 += 2)
                                {
                                    if (y1 > 0 && y1 < 19)
                                    {
                                        if (result[x1, y1] == 'B')
                                            CountBomb++;
                                    }
                                }
                            }
                        }
                        result[x, y] = Convert.ToChar(CountBomb + 48);
                        CountBomb = 0;
                    }
                }
            }
            return result;
        }
        public void PlayerMainController()
        {
            PlayerMoveController();
        }
        public void PlayerMoveController()
        {
            ConsoleKeyInfo key = Console.ReadKey();

            switch (key.Key)
            {
                case ConsoleKey.LeftArrow:
                    MovePlayer(-4, 0, 1);
                    break;
                case ConsoleKey.RightArrow:
                    MovePlayer(4, 0, 1);
                    break;
                case ConsoleKey.DownArrow:
                    MovePlayer(0, 2, 1);
                    break;
                case ConsoleKey.UpArrow:
                    MovePlayer(0, -2, 1);
                    break;
                case ConsoleKey.Spacebar:
                    ButtonOpen1();
                    break;
                case ConsoleKey.F:
                    ButtonFlag1();
                    break;
                case ConsoleKey.Escape:
                    Restart();
                    break;
            }
        }
        public void MovePlayer(int x, int y, int indexPlayer)
        {
            if (indexPlayer == 1)
            {
                int targetX = playerPositionX1 + x,
                    targetY = playerPositionY1 + y;
                if (CheckCollision1(targetX, targetY))
                {
                    playerPositionX1 = targetX;
                    playerPositionY1 = targetY;
                }
            }
            else
            {
                int targetX = playerPositionX2 + x,
                    targetY = playerPositionY2 + y;
                if (CheckCollision2(targetX, targetY))
                {
                    playerPositionX2 = targetX;
                    playerPositionY2 = targetY;
                }
            }
        }
        public (int, int) GetPlayerCoordinates1()
        {
            return (playerPositionX1, playerPositionY1);
        }
        public (int, int) GetPlayerCoordinates2()
        {
            return (playerPositionX2, playerPositionY2);
        }
        public bool CheckCollision1(int x, int y)
        {
            bool canMove = true;

            if ((x < 1 || x > 36) || (y < 1 || y > 18))
            {
                canMove = false;
            }

            return canMove;

        }
        public bool CheckCollision2(int x, int y)
        {
            bool canMove = true;

            if ((x < 51 || x > 86) || (y < 1 || y > 18))
            {
                canMove = false;
            }

            return canMove;

        }
        public void ButtonOpen1()
        {
            if (HideMap1[playerPositionX1, playerPositionY1] == 'B')
            {
                DisplayedMap1[playerPositionX1, playerPositionY1] = HideMap1[playerPositionX1, playerPositionY1];
                playerPositionX1 = 2;
                playerPositionY1 = 1;
                GameOver(2);
            }
            else
            {
                if (DisplayedMap1[playerPositionX1, playerPositionY1] != '▓')
                {
                    OpenSquare1(playerPositionX1, playerPositionY1);
                }
                if (DisplayedMap1[playerPositionX1, playerPositionY1] != 'F')
                {
                    if (DisplayedMap1[playerPositionX1, playerPositionY1] == '▓')
                    {
                        TotalTiles1--;
                        CheckWin(1);
                    }
                    DisplayedMap1[playerPositionX1, playerPositionY1] = HideMap1[playerPositionX1, playerPositionY1];
                }

                if (HideMap1[playerPositionX1, playerPositionY1] == '0')
                {
                    ZeroOpened1.Add((playerPositionX1, playerPositionY1));
                    OpenZero1(playerPositionX1, playerPositionY1);
                }
            }
        }
        public void ButtonOpen2()
        {
            if (HideMap2[playerPositionX2 - 50, playerPositionY2] == 'B')
            {
                DisplayedMap2[playerPositionX2 - 50, playerPositionY2] = HideMap2[playerPositionX2 - 50, playerPositionY2];
                playerPositionX2 = 52;
                playerPositionY2 = 1;
                GameOver(1);
            }
            else
            {
                if (DisplayedMap2[playerPositionX2 - 50, playerPositionY2] != '▓')
                {
                    OpenSquare2(playerPositionX2 - 50, playerPositionY2);
                }
                if (DisplayedMap2[playerPositionX2 - 50, playerPositionY2] != 'F')
                {
                    if (DisplayedMap2[playerPositionX2 - 50, playerPositionY2] == '▓')
                    {
                        TotalTiles2--;
                        CheckWin(2);
                    }
                    DisplayedMap2[playerPositionX2 - 50, playerPositionY2] = HideMap2[playerPositionX2 - 50, playerPositionY2];
                }

                if (HideMap2[playerPositionX2 - 50, playerPositionY2] == '0')
                {
                    ZeroOpened2.Add((playerPositionX2 - 50, playerPositionY2));
                    OpenZero2(playerPositionX2 - 50, playerPositionY2);
                }
            }
        }
        public char[,] GetHideMap()
        {
            return HideMap1;
        }
        public char[,] GetDisplayedMap()
        {
            return DisplayedMap1;
        }
        public void OpenZero1(int x, int y)
        {
            for (int x1 = x - 4; x1 <= x + 4; x1 += 4)
            {
                if (x1 > 0 && x1 < 37)
                {
                    for (int y1 = y - 2; y1 <= y + 2; y1 += 2)
                    {
                        if (y1 > 0 && y1 < 19)
                        {
                            if (DisplayedMap1[x1, y1] == '▓')
                            {
                                TotalTiles1--;
                                CheckWin(1);
                            }
                            DisplayedMap1[x1, y1] = HideMap1[x1, y1];
                            if (HideMap1[x1, y1] == '0')
                            {
                                bool reiteration = false;
                                foreach ((int, int) p in ZeroOpened1)
                                {
                                    if ((x1, y1) == p)
                                        reiteration = true;
                                }
                                if (reiteration == false)
                                {
                                    ZeroOpened1.Add((x1, y1));
                                    OpenZero1(x1, y1);
                                }
                                reiteration = false;
                            }
                        }
                    }
                }
            }
        }
        public void OpenZero2(int x, int y)
        {
            for (int x1 = x - 4; x1 <= x + 4; x1 += 4)
            {
                if (x1 > 0 && x1 < 37)
                {
                    for (int y1 = y - 2; y1 <= y + 2; y1 += 2)
                    {
                        if (y1 > 0 && y1 < 19)
                        {
                            if (DisplayedMap2[x1, y1] == '▓')
                            {
                                TotalTiles2--;
                                CheckWin(2);
                            }
                            DisplayedMap2[x1, y1] = HideMap2[x1, y1];
                            if (HideMap2[x1, y1] == '0')
                            {
                                bool reiteration = false;
                                foreach ((int, int) p in ZeroOpened2)
                                {
                                    if ((x1, y1) == p)
                                        reiteration = true;
                                }
                                if (reiteration == false)
                                {
                                    ZeroOpened2.Add((x1, y1));
                                    OpenZero2(x1, y1);
                                }
                                reiteration = false;
                            }
                        }
                    }
                }
            }
        }
        public void OpenSquare1(int x, int y)
        {
            int CountFlag = 0;
            for (int x1 = x - 4; x1 <= x + 4; x1 += 4)
            {
                if (x1 > 0 && x1 < 37)
                {
                    for (int y1 = y - 2; y1 <= y + 2; y1 += 2)
                    {
                        if (y1 > 0 && y1 < 19)
                        {
                            if (DisplayedMap1[x1, y1] == 'F')
                                CountFlag++;
                        }
                    }
                }
            }
            if (CountFlag != (int)DisplayedMap1[x, y] - 48)
                return;
            for (int x1 = x - 4; x1 <= x + 4; x1 += 4)
            {
                if (x1 > 0 && x1 < 37)
                {
                    for (int y1 = y - 2; y1 <= y + 2; y1 += 2)
                    {
                        if (y1 > 0 && y1 < 19)
                        {
                            if (DisplayedMap1[x1, y1] == '▓')
                            {
                                TotalTiles1--;
                                CheckWin(1);
                            }
                            if (DisplayedMap1[x1, y1] == 'F')
                                continue;
                            DisplayedMap1[x1, y1] = HideMap1[x1, y1];
                            if (HideMap1[x1, y1] == '0')
                            {
                                bool reiteration = false;
                                foreach ((int, int) p in ZeroOpened1)
                                {
                                    if ((x1, y1) == p)
                                        reiteration = true;
                                }
                                if (reiteration == false)
                                {
                                    ZeroOpened1.Add((x1, y1));
                                    OpenZero1(x1, y1);
                                }
                                reiteration = false;
                            }
                            if (HideMap1[x1, y1] == 'B')
                                GameOver(2);
                        }
                    }
                }
            }
        }
        public void OpenSquare2(int x, int y)
        {
            int CountFlag = 0;
            for (int x1 = x - 4; x1 <= x + 4; x1 += 4)
            {
                if (x1 > 0 && x1 < 37)
                {
                    for (int y1 = y - 2; y1 <= y + 2; y1 += 2)
                    {
                        if (y1 > 0 && y1 < 19)
                        {
                            if (DisplayedMap2[x1, y1] == 'F')
                                CountFlag++;
                        }
                    }
                }
            }
            if (CountFlag != (int)DisplayedMap2[x, y] - 48)
                return;
            for (int x1 = x - 4; x1 <= x + 4; x1 += 4)
            {
                if (x1 > 0 && x1 < 37)
                {
                    for (int y1 = y - 2; y1 <= y + 2; y1 += 2)
                    {
                        if (y1 > 0 && y1 < 19)
                        {
                            if (DisplayedMap2[x1, y1] == '▓')
                            {
                                TotalTiles2--;
                                CheckWin(2);
                            }
                            if (DisplayedMap2[x1, y1] == 'F')
                                continue;
                            DisplayedMap2[x1, y1] = HideMap2[x1, y1];
                            if (HideMap2[x1, y1] == '0')
                            {
                                bool reiteration = false;
                                foreach ((int, int) p in ZeroOpened2)
                                {
                                    if ((x1, y1) == p)
                                        reiteration = true;
                                }
                                if (reiteration == false)
                                {
                                    ZeroOpened2.Add((x1, y1));
                                    OpenZero2(x1, y1);
                                }
                                reiteration = false;
                            }
                            if (HideMap2[x1, y1] == 'B')
                                GameOver(1);
                        }
                    }
                }
            }
        }
        public void ButtonFlag1()
        {
            if (DisplayedMap1[playerPositionX1, playerPositionY1] == '▓')
            {
                if (TotalBombs1 != 0)
                {
                    DisplayedMap1[playerPositionX1, playerPositionY1] = 'F';
                    TotalBombs1--;
                    TotalTiles1--;
                    CheckWin(1);
                }
            }
            else if (DisplayedMap1[playerPositionX1, playerPositionY1] == 'F')
            {
                DisplayedMap1[playerPositionX1, playerPositionY1] = '▓';
                TotalBombs1++;
                TotalTiles1++;
            }
        }
        public void ButtonFlag2()
        {
            if (DisplayedMap2[playerPositionX2 - 50, playerPositionY2] == '▓')
            {
                if (TotalBombs2 != 0)
                {
                    DisplayedMap2[playerPositionX2 - 50, playerPositionY2] = 'F';
                    TotalBombs2--;
                    TotalTiles2--;
                    CheckWin(2);
                }
            }
            else if (DisplayedMap2[playerPositionX2 - 50, playerPositionY2] == 'F')
            {
                DisplayedMap2[playerPositionX2 - 50, playerPositionY2] = '▓';
                TotalBombs2++;
                TotalTiles2++;
            }
        }
        public void GameOver(int indexPlayer)
        {  
            gameOver = true;
            indexWinner = indexPlayer;
        }
        public void CheckWin(int indexPlayer)
        {
            if (indexPlayer == 1)
            {
                if (TotalTiles1 == 0)
                    YouWin(indexPlayer);
            }
            else
            {
                if (TotalTiles2 == 0)
                    YouWin(indexPlayer);
            }
        }
        public void YouWin(int indexPlayer)
        {
            gameOver = true;
            indexWinner = indexPlayer;
        }
        public void Restart()
        {
            StartGame();
            Console.SetCursorPosition(0, 27);
            Console.WriteLine("                         ");
            Play();
        }
        public void AiAlgorithm()
        {
            bool changes = false;
            void RandomOpen()
            {
                Random r = new Random();
                int randomX = r.Next(0, 8) * 4;
                int randomY = r.Next(0, 8) * 2;
                int x;
                int y;
                playerPositionX2 = 52;
                playerPositionY2 = 1;
                playerPositionX2 += randomX;
                playerPositionY2 += randomY;
                if (DisplayedMap2[playerPositionX2-50, playerPositionY2] == '▓')
                {
                    ButtonOpen2();
                    changes = true;
                }
                else
                {
                    RandomOpen();
                }

                RenderMap();
            }
            void Opened()
            {
                int closed = 0;
                int mines = 0;
                int x = playerPositionX2-50;
                int y = playerPositionY2;
                if (DisplayedMap2[playerPositionX2 - 50, playerPositionY2] > 0)
                {
                    for (int x1 = x - 4; x1 <= x + 4; x1 += 4)
                    {
                        if (x1 > 0 && x1 < 37)
                        {
                            for (int y1 = y - 2; y1 <= y + 2; y1 += 2)
                            {
                                if (y1 > 0 && y1 < 19)
                                {
                                    if (DisplayedMap2[x1, y1] == '▓')
                                    {
                                        closed++;
                                    }
                                    else if (DisplayedMap2[x1, y1] == 'F')
                                    {
                                        mines++;
                                    }
                                }
                            }
                        }
                    }
                    if (closed > 0)
                    {
                        if ((int)DisplayedMap2[x, y] - 48 == mines)
                        {
                            OpenSquare2(playerPositionX2 - 50, playerPositionY2);
                            changes = true;
                        }
                        if ((int)DisplayedMap2[playerPositionX2 - 50, playerPositionY2] - 48 == mines + closed)
                        {
                            for (int x1 = x - 4; x1 <= x + 4; x1 += 4)
                            {
                                if (x1 > 0 && x1 < 37)
                                {
                                    for (int y1 = y - 2; y1 <= y + 2; y1 += 2)
                                    {
                                        if (y1 > 0 && y1 < 19)
                                        {
                                            if (DisplayedMap2[x1, y1] == '▓')
                                            {
                                                playerPositionX2 = x1 + 50;
                                                playerPositionY2 = y1;
                                                ButtonFlag2();
                                                changes = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            void RandomOpen2()
            {
                int closed = 0;
                int mines = 0;
                int x = playerPositionX2 - 50;
                int y = playerPositionY2;
                for (int x1 = x - 4; x1 <= x + 4; x1 += 4)
                {
                    if (x1 > 0 && x1 < 37)
                    {
                        for (int y1 = y - 2; y1 <= y + 2; y1 += 2)
                        {
                            if (y1 > 0 && y1 < 19)
                            {
                                if (DisplayedMap2[x1, y1] == '▓')
                                {
                                    closed++;
                                }
                                else if (DisplayedMap2[x1, y1] == 'F')
                                {
                                    mines++;
                                }
                            }
                        }
                    }
                }
                if (closed > 0)
                {
                    for (int x1 = x - 4; x1 <= x + 4; x1 += 4)
                    {
                        if (x1 > 0 && x1 < 37)
                        {
                            for (int y1 = y - 2; y1 <= y + 2; y1 += 2)
                            {
                                if (y1 > 0 && y1 < 19)
                                {
                                    if (DisplayedMap2[x1, y1] == '▓')
                                    {
                                        playerPositionX2 = x1 + 50;
                                        playerPositionY2 = y1;
                                        ButtonOpen2();
                                        changes = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            RandomOpen();
            while (gameOver == false)
            {

                for (int i = 52; i < 86; i += 4)
                {
                    for (int j = 1; j < 18; j += 2)
                    {
                        if (DisplayedMap2[i - 50, j] != '▓' && DisplayedMap2[i - 50, j] != 'F')
                        {
                            playerPositionX2 = i;
                            playerPositionY2 = j;
                            Opened();
                            RenderMap();
                            CheckWin(1);
                            if (gameOver)
                                break;
                            Thread.Sleep(100);

                        }
                    }
                }
                if (changes == false)
                {
                    for (int i = 52; i < 86; i += 4)
                    {
                        for (int j = 1; j < 18; j += 2)
                        {
                            if (DisplayedMap2[i - 50, j] == '▓')
                            {
                                playerPositionX2 = i;
                                playerPositionY2 = j;
                                ButtonOpen2();
                                changes = true;
                                RenderMap();
                                if (gameOver)
                                    break;
                                Thread.Sleep(100);
                                break;
                            }
                        }
                        if(changes = true)
                        {
                            break;
                        }
                    }
                }
                changes = false;
                RenderMap();
                Thread.Sleep(1000);
            }

        }
    }
}
