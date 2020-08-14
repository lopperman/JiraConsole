using System;
using System.Collections.Generic;

namespace JiraConsole_Brower.ConsoleHelpers
{
    public class ConsoleLine
    {
        public ConsoleColor Foreground { get; set; }
        public ConsoleColor Background { get; set; }
        public bool UseColors { get; set; }
        public string Text { get; set; }
        public bool WritePartialLine { get; set; }

        public ConsoleLine(string text): this(text,false)
        {
        }

        public ConsoleLine(string text, bool writePartialLine)
        {
            WritePartialLine = writePartialLine;
            UseColors = false;
            Text = text;
        }

        public ConsoleLine(string text, ConsoleColor foreground, ConsoleColor background): this(text,foreground,background,false)
        {
        }

        public ConsoleLine(string text, ConsoleColor foreground, ConsoleColor background, bool writePartialLine)
        {
            Foreground = foreground;
            Background = background;
            Text = text;
            UseColors = true;
            WritePartialLine = writePartialLine;
        }

    }

    public class ConsoleLines
    {
        private SortedDictionary<int, ConsoleLine> _lines = new SortedDictionary<int, ConsoleLine>();

        public SortedDictionary<int, ConsoleLine> Lines
        {
            get
            {
                return _lines;
            }
        }

        public ConsoleLines()
        {
        }

        public bool HasQueuedLines
        {
            get
            {
                return _lines.Count > 0;
            }
        }

        public void AddConsoleLine(string text, ConsoleColor foreground, ConsoleColor background)
        {
            AddConsoleLine(text, foreground, background,false);
        }
        public void AddConsoleLine(string text, ConsoleColor foreground, ConsoleColor background, bool writePartial)
        {
            AddConsoleLine(new ConsoleLine(text, foreground, background));
        }


        public void AddConsoleLine(string text)
        {
            AddConsoleLine(text, false);
        }
        public void AddConsoleLine(string text, bool writePartial)
        {
            AddConsoleLine(new ConsoleLine(text,writePartial));
        }


        public void AddConsoleLine(ConsoleLine cl)
        {
            _lines.Add(_lines.Count, cl);
        }

        public void WriteQueuedLines()
        {
            WriteQueuedLines(false);
        }

        public void WriteQueuedLines(bool clearScreen)
        {
            if (clearScreen)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                Console.WriteLine("JiraConsole (Not Trademarked) - written by Paul Brower");
                Console.ForegroundColor = MainClass.defaultForeground;
                Console.BackgroundColor = MainClass.defaultBackground;
            }
            for (int i = 0; i < _lines.Count; i++)
            {
                ConsoleLine l = _lines[i];
                if (l.UseColors)
                {
                    Console.ForegroundColor = l.Foreground;
                    Console.BackgroundColor = l.Background;
                }
                if (l.WritePartialLine)
                {
                    Console.Write(l.Text);
                }
                else
                {
                    Console.WriteLine(l.Text);
                }
            }
            _lines.Clear();

        }

        public void ByeBye()
        {
            AddConsoleLine("   HAVE A GREAT DAY!!   ", ConsoleColor.DarkBlue, ConsoleColor.Yellow);
            WriteQueuedLines(true);
        }
    }

}
