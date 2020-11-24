﻿using System;

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

        public static void PressAnyKeyToContinue()
        {
            WriteLine("...");
            WriteLine("PRESS ANY KEY TO CONTINUE");
            var key = Console.ReadKey(true);
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
            consoleLines.AddConsoleLine(" ------------- ", ConsoleColor.Black, ConsoleColor.White);
            consoleLines.AddConsoleLine("|  Main Menu  |");
            consoleLines.AddConsoleLine(" ------------- ");
            consoleLines.AddConsoleLine("(M) Show Change History for 1 or (M)ore Cards");
            consoleLines.AddConsoleLine("(J) Show (J)SON for 1 or more Cards");
            consoleLines.AddConsoleLine("(X) Create E(X)tract files");
            consoleLines.AddConsoleLine("(W) Create (W)ork Metrics Analysis from JQL Query");
            consoleLines.AddConsoleLine("(A) Epic (A)nalysis - Find and Analyze - Yep, this exists");
            consoleLines.AddConsoleLine("");
            consoleLines.AddConsoleLine("(I) View (I)tem Status values for work metrics");
            consoleLines.AddConsoleLine("(C) Config Menu");
            consoleLines.AddConsoleLine("Enter selection or E to exit.");
        }

        public static void BuildConfigMenu()
        {
            consoleLines.AddConsoleLine(" --------------- ", ConsoleColor.Black, ConsoleColor.White);
            consoleLines.AddConsoleLine("|  Config Menu  |");
            consoleLines.AddConsoleLine(" --------------- ");
            consoleLines.AddConsoleLine("(R) Rebuild Login Configuation");
            consoleLines.AddConsoleLine("(V) View JiraConsole (this app) config");
            consoleLines.AddConsoleLine(string.Format("(J) View Jira Info for {0}",JiraUtil.JiraRepo.ServerInfo.BaseUrl));
            consoleLines.AddConsoleLine("");
            consoleLines.AddConsoleLine("(M) Main Menu");
            consoleLines.AddConsoleLine("Enter selection or (E) to exit.");
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
