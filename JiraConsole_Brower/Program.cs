using System;
using Atlassian.Jira;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using JiraConsole_Brower.ConsoleHelpers;
using System.Diagnostics;
using System.Threading;

namespace JiraConsole_Brower
{
    class MainClass
    {
        private static bool _initialized = false;
        static JiraConfiguration config = null;
        static string filePath_JiraExpandedIssuesCSV ;
        public static ConsoleColor defaultForeground;
        public static ConsoleColor defaultBackground;
        static ConsoleLines consoleLines = new ConsoleLines();
        static JiraRestClientSettings _settings = null;
        static Jira _jira = null;
        private static string[] _args = null;

        public static void Main(string[] args)
        {
            _args = args;

            bool showMenu = true;

            if (!_initialized)
            {
                defaultForeground = Console.ForegroundColor;
                defaultBackground = Console.BackgroundColor;
                Console.ForegroundColor = defaultForeground;
                Console.BackgroundColor = defaultBackground;
                Console.Clear();
            }
            while (showMenu)
            {
                showMenu = MainMenu();
            }
            consoleLines.ByeBye();
            Environment.Exit(0);
        }

        private static bool MainMenu()
        {

            if (!_initialized)
            {
                if (config == null && _args != null)
                {
                    config = ConfigHelper.BuildConfig(_args);
                }
                if (config == null)
                {
                    BuildNotInitializedQueue();
                    consoleLines.WriteQueuedLines(true);
                    string vs = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(vs)) vs = string.Empty;
                    string[] arr = vs.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    config = ConfigHelper.BuildConfig(arr);

                    if (config != null)
                    {
                        if (CreateRestClient())
                            _initialized = true;
                            return _initialized;
                        }
                        else
                        {
                            _initialized = false;
                            WriteLine("Invalid arguments!", ConsoleColor.Yellow, ConsoleColor.DarkBlue, false);
                            WriteLine("Enter arguments like this:  JiraConsole_Brower \"john.doe@atlassian.net\" \"JO5qzY7UfH8wxfi4ru4A3G4C\" \"https://company.atlasssian.net\" \"PRJ\"");
                            WriteLine("Do you want to try again? (Y/N):");
                            ConsoleKeyInfo keyInfo = Console.ReadKey();
                            if (keyInfo.Key == ConsoleKey.Y)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }

                    }
                }

            }

            return InitializedMenu();

        }

        private static bool InitializedMenu()
        {
            BuildInitializedMenu();
            consoleLines.WriteQueuedLines(true);

            var resp = Console.ReadKey();
            if (resp.Key == ConsoleKey.S)
            {
                WriteLine("");
                WriteLine("Enter Jira Card Key(s) separated by a space (e.g. POS-123 POS-234), or E to exit.", ConsoleColor.Black, ConsoleColor.White, false);
                var keys = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(keys))
                {
                    return true;
                }
                if (keys.ToUpper() == "E")
                {
                    return false;
                }
                //if (keys.ToUpper() == "S")
                //{
                    string[] arr = keys.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < arr.Length; i++)
                    {
                        AnalyzeOneIssue(arr[i]);
                    }
                //}

                WriteLine("");
                WriteLine("Press any key to continue.");
                Console.ReadKey();
                return true;
            }
            else if (resp.Key == ConsoleKey.M)
            {
                WriteLine("");
                WriteLine("Enter Prefix, then Jira Card Key(s) separated by a space (e.g. POS 123 234), or E to exit.", ConsoleColor.Black, ConsoleColor.White, false);
                var keys = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(keys))
                {
                    return true;
                }
                if (keys.ToUpper() == "E")
                {
                    return false;
                }
                //if (keys.ToUpper() == "M")
                //{
                    string[] arr = keys.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length >= 2)
                    {
                        string prefix = arr[0];
                    AnalyzeIssues(arr[0], keys);
                }


                //}

                WriteLine("");
                WriteLine("Press any key to continue.");
                Console.ReadKey();
                return true;

            }

            return false;
        }

        private static void BuildInitializedMenu()
        {
            consoleLines.AddConsoleLine("----------");
            consoleLines.AddConsoleLine("Main Menu", ConsoleColor.Black, ConsoleColor.White);
            consoleLines.AddConsoleLine("----------");
            consoleLines.AddConsoleLine("(S)how Change History for Card");
            consoleLines.AddConsoleLine("(M)Show Change History for Multiple Cards");
            consoleLines.AddConsoleLine("");
            consoleLines.AddConsoleLine("Enter selection or E to exit.");
        }

        private static bool CreateRestClient()
        {

            try
            {
                List<JiraCard> jiraCards = new List<JiraCard>();

                _settings = new JiraRestClientSettings();
                _settings.EnableUserPrivacyMode = false;


                //IssueSearchOptions options = new IssueSearchOptions(string.Format("project={0}", jiraProjectKey));
                //options.MaxIssuesPerRequest = 50; //this is wishful thinking on my part -- client has this set at 20 -- unless you're a Jira admin, got to live with it.
                //options.FetchBasicFields = true;

                WriteLine(string.Format("connecting to {0}@{1} ...", config.jiraUserName, config.jiraBaseUrl));
                _jira = Jira.CreateRestClient(config.jiraBaseUrl, config.jiraUserName, config.jiraAPIToken, _settings);
                WriteLine("Successfully connected to Jira as " + config.jiraUserName);

            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.Beep();
                Console.Beep();
                Console.WriteLine("Sorry, there seems to be a problem connecting to Jira with the arguments you provided. Error: {0}, {1}\r\n\r\n{2}", ex.Message, ex.Source, ex.StackTrace);
                return false;
            }
            return true;
        }

        private static void BuildNotInitializedQueue()
        {
            consoleLines.AddConsoleLine("This application can be initialized with 4 parameters:");
            consoleLines.AddConsoleLine("1. Jira UserName");
            consoleLines.AddConsoleLine("2. Jira API Token");
            consoleLines.AddConsoleLine("3. Jira Base URL");
            consoleLines.AddConsoleLine("4. Jira Project Key");
            consoleLines.AddConsoleLine("5. OPTIONAL Jira Card Prefix (e.g. POS-)");
            consoleLines.AddConsoleLine("");
            consoleLines.AddConsoleLine("For Example:  JiraConsole_Brower \"john.doe@atlassian.net\" \"JO5qzY7UfH8wxfi4ru4A3G4C\" \"https://company.atlasssian.net\" \"PRJ\"");
            consoleLines.AddConsoleLine("Please initialize application now per the above example:");
           
        }

        private static void WriteLine(string text)
        {
            WriteLine(text, false);
        }

        private static void WriteLine(string text, bool clearScreen)
        {
            WriteLine(text, defaultForeground, defaultBackground, clearScreen);
        }

        private static void WriteLine(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor, bool clearScreen)
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

        private static void WriteExitInfo()
        {
            Console.WriteLine();
            WriteLine("Enter \"E\" to exit.", ConsoleColor.Black, ConsoleColor.Cyan, false);
        }

        private static void SetDefaultConsoleColors()
        {
            Console.ForegroundColor = defaultForeground;
            Console.BackgroundColor = defaultBackground;
        }

        //public static void xMain(string[] args)
        //{
        //    if (args != null && args.Length == 5)
        //    {
        //        Main(args[0], args[1], args[2], args[3], args[4]);
        //    }
        //    else if (args != null && args.Length == 6)
        //    {
        //        AnalyzeOneIssue(args[0], args[1], args[2], args[3], args[4], args[5]);
        //    }
        //    else
        //    {
        //        Console.WriteLine("Incorrect Arguments");
        //        Console.WriteLine("Good-bye");
        //    }
        //}

        public static void AnalyzeIssues(string prefix, string cardNumbers)
        {
            string[] arr = cardNumbers.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < arr.Length; i++)
            {
                AnalyzeOneIssue(string.Format("{0}-{1}",prefix,arr[i]));
            }
        }

        public static void AnalyzeOneIssue(string key)
        {
            //var settings = new JiraRestClientSettings();
            //settings.EnableUserPrivacyMode = false;

            WriteLine("");
            WriteLine("***** Jira Card: " + key, ConsoleColor.DarkBlue, ConsoleColor.White, false);

            IssueSearchOptions options = new IssueSearchOptions(string.Format("project={0}", config.jiraProjectKey));
            options.MaxIssuesPerRequest = 50; //this is wishful thinking on my part -- client has this set at 20 -- unless you're a Jira admin, got to live with it.
            options.FetchBasicFields = true;

            var issue = _jira.Issues.Queryable.Where(x => x.Project == config.jiraProjectKey && x.Key == key).FirstOrDefault();

            if (issue == null)
            {
                WriteLine("***** Jira Card: " + key + " NOT FOUND!", ConsoleColor.DarkBlue, ConsoleColor.White, false);
                return;
            }

            var labels = issue.Labels;

            var backClr = Console.BackgroundColor;
            var foreClr = Console.ForegroundColor;

            var changeLogs = issue.GetChangeLogsAsync().Result.ToList<IssueChangeLog>();

            for (int i = 0; i < changeLogs.Count; i++)
            {
                IssueChangeLog changeLog = changeLogs[i];

                foreach (IssueChangeLogItem cli in changeLog.Items)
                {
                    if (cli.FieldName.ToLower().StartsWith("desc"))
                    {
                        continue;
                    }
                    else if (cli.FieldName.ToLower().StartsWith("comment"))
                    {
                        continue;
                    }
                    else
                    {

                        Console.BackgroundColor = backClr;
                        Console.ForegroundColor = foreClr;

                        if (cli.FieldName.ToLower().StartsWith("status"))
                        {
                            Console.Write("{0} - Changed On {1}, {2} field changed from '{3}' to ", issue.Key, changeLog.CreatedDate.ToString(), cli.FieldName, cli.FromValue);

                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.ForegroundColor = ConsoleColor.White;

                            Console.Write("{0}", cli.ToValue);
                            Console.Write(Environment.NewLine);
                        }
                        else if (cli.FieldName.ToLower().StartsWith("label"))
                        {
                            Console.Write("{0} - Changed On {1}, {2} field changed from '{3}' to ", issue.Key, changeLog.CreatedDate.ToString(), cli.FieldName, cli.FromValue);

                            Console.BackgroundColor = ConsoleColor.Blue;
                            Console.ForegroundColor = ConsoleColor.White;

                            Console.Write("{0}", cli.ToValue);
                            Console.Write(Environment.NewLine);
                        }

                        else
                        {
                            Console.WriteLine("{0} - Changed On {1}, {2} field changed from '{3}' to '{4}'", issue.Key, changeLog.CreatedDate.ToString(), cli.FieldName, cli.FromValue, cli.ToValue);

                        }

                    }
                }
            }
            WriteLine("***** Jira Card: " + key + " END", ConsoleColor.DarkBlue, ConsoleColor.White, false);
        }

        public static void Main(string userName, string apiToken, string jiraUrl, string jiraProjKey, string saveFilePath)
        {
            Console.Clear();


            try
            {
                string jiraUserName = userName;
                string jiraAPIToken = apiToken;
                string jiraBaseUrl = jiraUrl;
                string jiraProjectKey = jiraProjKey;
                string filePath_JiraExpandedIssuesCSV = saveFilePath;


                List<JiraCard> jiraCards = new List<JiraCard>();

                var settings = new JiraRestClientSettings();
                settings.EnableUserPrivacyMode = false;
             

                IssueSearchOptions options = new IssueSearchOptions(string.Format("project={0}", jiraProjectKey));
                options.MaxIssuesPerRequest = 50; //this is wishful thinking on my part -- client has this set at 20 -- unless you're a Jira admin, got to live with it.
                options.FetchBasicFields = true;
              
                Console.WriteLine("connecting to {0}@{1} ...", jiraUserName, jiraBaseUrl);

                var jira = Jira.CreateRestClient(jiraBaseUrl, jiraUserName, jiraAPIToken, settings);

                //var issues = jira.Issues.GetIssuesFromJqlAsync(options).Result;

                int takeCount = 1000;
                int startAt = 0;
                int currentCount = 0;
                int totalCount = 0;
                int noChangeLogCount = 0;

                //basically just paging through the project. If you know what the max issues are for your Jira instance, change the 'takeCount' about to match.
                while (true)
                {

                    //jira = null;
                    //jira = Jira.CreateRestClient(jiraBaseUrl, jiraUserName, jiraAPIToken, settings);

                    //var changed = jira.Issues.Queryable.Where(x => x.Project == jiraProjectKey && x.Type == "Story" && x.Status != "Archive" && x.Status != "Archived").OrderBy(x=>x.Key);
                    //var cardsFromJql = jira.Issues.GetIssuesFromJqlAsync(")

                    var cards = jira.Issues.Queryable.Where(x => x.Project == jiraProjectKey && (x.Status == "In Progress" || x.Status == "Done")).OrderByDescending(x => x.Updated).Skip(startAt).Take(takeCount).ToList();

                    currentCount += cards.Count;
                    startAt += currentCount;


                    foreach (var issue in cards)
                    {
                        JiraCard jiraCard = new JiraCard(issue.JiraIdentifier, issue.Key.Value, issue.Status.Name, "", issue.Created.Value, issue.Updated.Value,issue.Type.Name);

                        var changeLogs = issue.GetChangeLogsAsync().Result.ToList<IssueChangeLog>();

                        if (changeLogs == null || changeLogs.Count == 0)
                        {
                            Console.WriteLine("{0} has 0 change logs", issue.Key);
                            noChangeLogCount += 1;
                        }
                        else
                        {
                            totalCount += 1;
                        }



                        for (int i = 0; i < changeLogs.Count; i++)
                        {
                            IssueChangeLog changeLog = changeLogs[i];

                            foreach (IssueChangeLogItem cli in changeLog.Items)
                            {
                                if (cli.FieldName.ToLower().StartsWith("desc"))
                                {
                                    continue;
                                }
                                else if (cli.FieldName.ToLower().StartsWith("comment"))
                                {
                                    continue;
                                }
                                else
                                {
                                    jiraCard.AddChangeLog(changeLog.Id, changeLog.CreatedDate, cli.FieldName, cli.FieldType, cli.FromId, cli.FromValue, cli.ToId, cli.ToValue);
                                }
                            }
                        }

                        jiraCards.Add(jiraCard);

//                        Console.WriteLine(issue.Key + " change logs:" + changeLogs.Count);
                        Console.WriteLine("{0} - {1} - {2}, {3} change logs",issue.Key,issue.Type, issue.Status, changeLogs.Count);
                    }
                }

                Console.WriteLine("");
                Console.WriteLine("completed data extraction from Jira ...");
                Console.WriteLine("");
                Console.Beep();
                Console.WriteLine("{0} cards without changelogs", noChangeLogCount);
                Console.WriteLine("writing {0} cards with a total of {1} change log events to {2} ...", jiraCards.Count, jiraCards.Sum(x => x.ChangeLogs.Count), filePath_JiraExpandedIssuesCSV);

                WriteCSVFile(jiraCards, filePath_JiraExpandedIssuesCSV);

                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("process completed successfully ...");
                Console.WriteLine("");
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("");
                Console.Beep();
                Console.Beep();
                Console.WriteLine("Sorry bud, there seems to be a problem. Error: {0}, {1}\r\n\r\n{2}", ex.Message, ex.Source, ex.StackTrace);
            }
            finally
            {
                Console.Beep();
            }
        }

        /// <summary>
        /// Build CSV file.
        /// Importing into MS Excel (delimited by ",") works nicely, but I'm also planning on adding a JSON option. 
        /// </summary>
        /// <param name="cards"></param>
        /// <param name="filePath"></param>
        public static void WriteCSVFile(List<JiraCard> cards, string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            StreamWriter writer = new StreamWriter(filePath, false);
            writer.WriteLine("id{0}key{0}cardType{0}status{0}description{0}created{0}updated{0}changeLogId{0}changeLogDt{0}fieldName{0}fieldType{0}fromId{0}fromValue{0}toId{0}toValue{0}DevStart{0}DevDone{0}CycleTimeDays", ",");

            for (int i = 0; i < cards.Count; i ++)
            {
                var card = cards[i];

                for (int j = 0; j < card.ChangeLogs.Count; j++)
                {
                    var cLog = card.ChangeLogs[j];

                    writer.WriteLine("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}{12}{0}{13}{0}{14}{0}{15}{0}{16}{0}{17}{0}{18}", ",", card._id, card.Key, card.CardType, card.Status, "", card.Created, card.Updated, cLog._id, cLog.ChangeLogDt, cLog.FieldName, cLog.FieldType, cLog.FromId, CleanText(cLog.FromValue), cLog.ToId, CleanText(cLog.ToValue), card.DevStartDt.HasValue ? card.DevStartDt.Value.ToString() : "", card.DevDoneDt.HasValue ? card.DevDoneDt.Value.ToString() : "", card.CycleTime.HasValue ? card.CycleTime.ToString() : "");
                }

            }

            writer.Close();
        }

        /// <summary>
        /// Clean out the mess that Jira includes in their description and comments fields. Was having some issues with that, so description and comments currently
        /// aren't written to the text file. I consider that a nice to have, but I'll work on it soon.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string CleanText(string text)
        {
            string ret = string.Empty;

            if (!String.IsNullOrEmpty(text))
            {
                //Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                //ret = rgx.Replace(text, "");
                ret = text.Replace(",", string.Empty);
            }
            return ret;
        }
    }
}
