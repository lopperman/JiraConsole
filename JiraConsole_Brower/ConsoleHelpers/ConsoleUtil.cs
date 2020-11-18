using System;

namespace JiraCon
{

    public static class ConsoleUtil
    {
        private static ConsoleColor defaultForeground;
        private static ConsoleColor defaultBackground;

        public static ConsoleColor DefaultConsoleBackground
        {
            get
            {
                return defaultBackground;
            }
        }

        public static ConsoleColor DefaultConsoleForeground
        {
            get
            {
                return defaultForeground;
            }
        }

        public static void InitializeConsole(ConsoleColor defForeground, ConsoleColor defBackground)
        {
            defaultForeground = defForeground;
            defaultBackground = defBackground;
            Console.ForegroundColor = defaultForeground;
            Console.BackgroundColor = defaultBackground;
            Console.Clear();

        }

        public static void SetDefaultConsoleColors()
        {
            Console.ForegroundColor = defaultForeground;
            Console.BackgroundColor = defaultBackground;
        }

        public static void BuildInitializedMenu(ConsoleLines consoleLines)
        {
            consoleLines.AddConsoleLine("----------");
            consoleLines.AddConsoleLine("Main Menu", ConsoleColor.Black, ConsoleColor.White);
            consoleLines.AddConsoleLine("----------");
            //consoleLines.AddConsoleLine("(S)how Change History for Card");
            consoleLines.AddConsoleLine("(M)Show Change History for 1 or more Cards");
            //consoleLines.AddConsoleLine("(F)Enter file path that contains 1 card per line, and file path for output");
            consoleLines.AddConsoleLine("(J) Create extract files for all cards from JQL");
            consoleLines.AddConsoleLine("");
            consoleLines.AddConsoleLine("Enter selection or E to exit.");
        }


        public static void WriteLine(string text)
        {
            WriteLine(text, false);
        }

        public static void WriteLine(string text, bool clearScreen)
        {
            WriteLine(text, ConsoleUtil.DefaultConsoleForeground, ConsoleUtil.DefaultConsoleBackground, clearScreen);
        }

        public static void WriteLine(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor, bool clearScreen)
        {
            if (clearScreen)
            {
                Console.Clear();
            }
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.WriteLine(text);
            ConsoleUtil.SetDefaultConsoleColors();

        }

    }

}
