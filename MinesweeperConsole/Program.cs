using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace MinesweeperConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            Menu menu = new Menu();
            menu.Play();
            IGame game = null;
            game = menu.GetGame();
            game.Play();
        }
    }
}
