using System;

namespace JiraCon
{

    public static class ConsoleUtil
    {
        static ConsoleLines consoleLines = new ConsoleLines();

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

        public static ConsoleLines Lines
        {
            get
            {
                return consoleLines;
            }
        }

        public static void ResetConsoleColors()
        {
            Console.BackgroundColor = defaultBackground;
            Console.ForegroundColor = defaultForeground;
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

        public static void BuildInitializedMenu()
        {
            consoleLines.AddConsoleLine("----------");
            consoleLines.AddConsoleLine("Main Menu", ConsoleColor.Black, ConsoleColor.White);
            consoleLines.AddConsoleLine("----------");
            consoleLines.AddConsoleLine("(M) Show Change History for 1 or more Cards");
            consoleLines.AddConsoleLine("(J) Create extract files");
            consoleLines.AddConsoleLine("");
            consoleLines.AddConsoleLine("(K) Re-create config file.");
            consoleLines.AddConsoleLine("Enter selection or E to exit.");
        }

        public static void BuildNotInitializedQueue()
        {
            consoleLines.AddConsoleLine("This application can be initialized with");
            consoleLines.AddConsoleLine("1. path to config file with arguments");
            consoleLines.AddConsoleLine("");
            consoleLines.AddConsoleLine("For Example:  john.doe@wwt.com SECRETAPIKEY https://client.atlassian.net");
            consoleLines.AddConsoleLine("Please initialize application now per the above example:");

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
            SetDefaultConsoleColors();

        }

        public static void WriteAppend(string text)
        {
            WriteAppend(text, false);    
        }

        public static void WriteAppend(string text, bool endOfLine)
        {
            WriteAppend(text, defaultForeground, defaultBackground,endOfLine);
        }


        public static void WriteAppend(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            WriteAppend(text, foregroundColor, backgroundColor, false);
        }

        public static void WriteAppend(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor, bool endOfLine)
        {
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.Write(text);
            if (endOfLine)
            {
                Console.WriteLine();
            }
            SetDefaultConsoleColors();
        }

    }

}
