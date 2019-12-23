using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace MinesweeperConsole
{
    enum Buttons
    {
        Single,
        Multi,
        AI
    }
    class Menu
    {
        private string[] names = { "Single Player", "Multiplayer", "AgainstComputer" };

        public Buttons buttons { get; set; } = Buttons.Single;

        public Menu()
        {

        }
        public void RenderMenu()
        {
            var width = Console.WindowWidth;
            float padding = width / 2 + 52 / 2;
            for (int i = 0; i < 3; i++)
            {
                Console.Write("\n");
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("{0," + padding + "}", "█───█─███─█──█─███─███─█───█─███─███─████─███─████");
            Console.WriteLine("{0," + padding + "}", "██─██──█──██─█─█───█───█───█─█───█───█──█─█───█──█");
            Console.WriteLine("{0," + padding + "}", "█─█─█──█──█─██─███─███─█─█─█─███─███─████─███─████");
            Console.WriteLine("{0," + padding + "}", "█───█──█──█──█─█─────█─█████─█───█───█────█───█─█ ");
            Console.WriteLine("{0," + padding + "}", "█───█─███─█──█─███─███──█─█──███─███─█────███─█─█ ");

            Console.ForegroundColor = ConsoleColor.Green;
            for (int i = 0; i < Console.WindowHeight / 7; i++)
            {
                Console.Write("\n");
            }

            for(int i = 0;i < names.Length; i++)
            {
                if(i == (int)buttons)
                {
                    padding = (width / 2 - names[i].Length / 2)-1;
                    Console.Write("{0," + padding + "}", "-");
                    for (int j = 0; j < names[i].Length + 1; j++)
                    {
                        Console.Write("-");
                    }
                    Console.Write("\n");

                    padding = width / 2 + (names[i].Length + 4) / 2;
                    Console.WriteLine("{0," + padding + "}","| " + names[i] + " |");

                    padding = (width / 2 - names[i].Length / 2) - 1;
                    Console.Write("{0," + padding + "}", "-");
                    for (int j = 0; j < names[i].Length + 1; j++)
                    {
                        Console.Write("-");
                    }
                    Console.Write("\n");
                }
                else
                {
                    padding = width / 2 + names[i].Length / 2;
                    Console.WriteLine("{0," + padding + "}", names[i]);
                }
            }
        }
        public string GetName(Buttons buttons)
        {
            string result = names[0];
            switch(buttons)
            {
                case Buttons.Single:
                    result = names[0];
                    break;
                case Buttons.Multi:
                    result = names[1];
                    break;
                case Buttons.AI:
                    result = names[2];
                    break;
            }
            return result;
        }
        public bool MenuController()
        {
            ConsoleKeyInfo key = Console.ReadKey();
            switch (key.Key)
            {
                case ConsoleKey.DownArrow:
                    if((int)buttons != 2)
                        buttons++;
                    return true;
                    break;
                case ConsoleKey.UpArrow:
                    if ((int)buttons != 0)
                        buttons--;
                    return true;
                    break;
                case ConsoleKey.Enter:
                    return false;
                    break;
            }
            return true;
        }
        public void Play()
        {
            bool menuContinue = true;
            while (menuContinue)
            {
                RenderMenu();
                menuContinue = MenuController();
                Thread.Sleep(10);
                Console.Clear();
            }
        }
        public IGame GetGame()
        {
            switch (buttons)
            {
                case Buttons.Single:
                    return new CreatorSingleGame().Create();
                    break;
                case Buttons.Multi:
                    return new CreatorMultiGame().Create();
                    break;
                case Buttons.AI:
                    return new CreatorAIGame().Create();
                    break;
            }
            return new CreatorSingleGame().Create();
        }
    }
}