using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Atlassian.Jira;

namespace JiraCon
{
    class MainClass
    {
        private static bool _initialized = false;
        static JiraConfiguration config = null;
        static ConsoleLines consoleLines = new ConsoleLines();
        static JiraRestClientSettings _settings = null;
        private static string[] _args = null;

        public static void Main(string[] args)
        {

            _args = args;
            bool showMenu = true;

            if (!_initialized)
            {
                ConsoleUtil.InitializeConsole(ConsoleColor.White, ConsoleColor.Black);
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

                    consoleLines.configInfo = string.Format("User: {0}, Project Key: {1}", config.jiraUserName, config.jiraProjectKey);


                    if (config != null)
                    {
                        if (JiraUtil.CreateRestClient(config))
                        {
                            _initialized = true;
                            return _initialized;
                        }
                        else
                        {
                            _initialized = false;
                            ConsoleUtil.WriteLine("Invalid arguments!", ConsoleColor.Yellow, ConsoleColor.DarkBlue, false);
                            ConsoleUtil.WriteLine("Enter path to config file");
                            ConsoleUtil.WriteLine("Do you want to try again? (Y/N):");
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

                else
                {
                    if (JiraUtil.CreateRestClient(config))
                    {
                        _initialized = true;
                        return _initialized;
                    }
                    else
                    {
                        _initialized = false;
                        ConsoleUtil.WriteLine("Invalid arguments!", ConsoleColor.Yellow, ConsoleColor.DarkBlue, false);
                        ConsoleUtil.WriteLine("Enter path to config file");
                        ConsoleUtil.WriteLine("Do you want to try again? (Y/N):");
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
            ConsoleUtil.BuildInitializedMenu(consoleLines);
            consoleLines.WriteQueuedLines(true);

            var resp = Console.ReadKey();
            if (resp.Key == ConsoleKey.M)
            {
                ConsoleUtil.WriteLine("");
                ConsoleUtil.WriteLine("Enter Prefix, then Jira Card Key(s) separated by a space (e.g. POS 123 234), or E to exit.", ConsoleColor.Black, ConsoleColor.White, false);
                var keys = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(keys))
                {
                    return true;
                }
                if (keys.ToUpper() == "E")
                {
                    return false;
                }
                    string[] arr = keys.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length >= 2)
                    {
                        string prefix = arr[0];
                    AnalyzeIssues(arr[0], keys);
                }

                ConsoleUtil.WriteLine("");
                ConsoleUtil.WriteLine("Press any key to continue.");
                Console.ReadKey();
                return true;
            }
            else if (resp.Key == ConsoleKey.J)
            {
                ConsoleUtil.WriteLine("");
                ConsoleUtil.WriteLine("Enter or paste JQL then press enter to continue.");
                var jql = Console.ReadLine();
                ConsoleUtil.WriteLine("");
                ConsoleUtil.WriteLine(string.Format("Enter (Y) to use the following JQL?\r\n\r\n{0}", jql));
                var keys = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(keys))
                {
                    return true;
                }
                if (keys.ToUpper() == "Y")
                {
                    Sandbox(jql);
                    ConsoleUtil.WriteLine("");
                    ConsoleUtil.WriteLine("Press any key to continue.");
                    Console.ReadKey();
                }
                return true;

            }
            return false;
        }


        //**********************************************************************************************************************************************************************************************
        //**********************************************************************************************************************************************************************************************
        //**********************************************************************************************************************************************************************************************
        //**********************************************************************************************************************************************************************************************
        //                                  KEEP FINAL CODE ABOVE THIS LINE
        //**********************************************************************************************************************************************************************************************
        //**********************************************************************************************************************************************************************************************
        //**********************************************************************************************************************************************************************************************
        //**********************************************************************************************************************************************************************************************
        //**********************************************************************************************************************************************************************************************


        private static void Sandbox(string jql)
        {

            //string jql = "project in (BAM, POS) AND issuetype in (Bug, Story, Subtask, Sub-task) AND updatedDate >= 2020-06-01 AND status not in (Backlog, Archive, Archived)";
            //string jql = "key in (BAM-1238, BAM-2170, POS-426, BAM-2154)";
            //string jql = "project in (BAM, POS) AND issuetype in (Bug, Story, Subtask, Sub-task) AND updatedDate >= 2020-06-01 AND status not in (Backlog, Archive, Archived, Bug_Archive) AND statusCategoryChangedDate >= 2020-06-01 AND (labels is EMPTY OR labels not in (JM, JM-Work, JMPOS-Work, JM_Work, jm-work, Metrics-Ignore)) and (component != Infrastructure OR component is EMPTY)";
            var issues = JiraUtil.JiraRepo.GetIssues(jql);

            List<JIssue> jissues = new List<JIssue>();

            foreach (var issue in issues)
            {
                ConsoleUtil.WriteLine(string.Format("getting changelogs for {0}", issue.Key.Value));
                JIssue newIssue = new JIssue(issue);
                newIssue.AddChangeLogs(JiraUtil.JiraRepo.GetIssueChangeLogs(issue));

                jissues.Add(newIssue);
            }

            List<string> saveToFile = new List<string>();

            StreamWriter qaWriter = new StreamWriter("/Users/paulbrower/METRICS_OCT2020_QAFILURE.txt", false);
            StreamWriter cycleTimeWriter = new StreamWriter("/Users/paulbrower/METRICS_OCT2020_CYCLETIME.txt", false);
            StreamWriter cycleTimeVelocity = new StreamWriter("/Users/paulbrower/METRICS_OCT2020_VELOCITY.txt", false);

            jissues = jissues.OrderBy(x => x.Key).ToList();

            string lastKey = string.Empty;
            foreach (var iss in jissues)
            {

                //only righgt if done or has qa failures or is bug

                if (!lastKey.Equals(iss.Key))
                {
                    lastKey = iss.Key;
                    saveToFile.Add("");
                    saveToFile.Add(string.Format("****** ISSUE: {0} -- status: {1}", iss.Key, iss.StatusName));
                    saveToFile.Add(iss.CycleTimeSummary);

                    foreach (var s in iss.FailedQASummary)
                    {
                        saveToFile.Add(s);
                    }

                    DateTime? devDoneDate = iss.GetDevDoneDate();
                    if (devDoneDate.HasValue && devDoneDate.Value.Date > new DateTime(2020,6,1))
                    {
                        cycleTimeVelocity.WriteLine(string.Format("{0},{1},{2}", iss.Key,iss.IssueType, devDoneDate.Value.ToShortDateString()));
                    }

                    if (iss.FailedQASummary.Count > 0)
                    {
                        foreach (var s in iss.FailedQASummary)
                        {
                            qaWriter.WriteLine(s);
                        }
                    }
                    string ct = iss.CycleTimeSummary;
                    if (!string.IsNullOrWhiteSpace(ct) && ct.ToLower() != "n/a")
                    {
                        cycleTimeWriter.WriteLine(ct);
                    }
                }

                foreach (var cl in iss.GetStateChanges())
                {
                    saveToFile.Add(string.Format("{0}:\t{1}", iss.Key, cl));
                }
            }

            string filename = "/Users/paulbrower/METRICS_OCT2020.txt";

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            StreamWriter writer = new StreamWriter(filename,false);

            for (int i = 0; i < saveToFile.Count; i ++)
            {
                writer.WriteLine(saveToFile[i]);
            }

            writer.Close();
            qaWriter.Close();
            cycleTimeWriter.Close();
            cycleTimeVelocity.Close();

            string checksaveToFile = "";

        }


        //private static bool CreateRestClient()
        //{
        //    bool ret = false;
        //    try
        //    {
        //        _settings = new JiraRestClientSettings();
        //        _settings.EnableUserPrivacyMode = true;

        //        ConsoleUtil.WriteLine(string.Format("connecting to {0}@{1} ...", config.jiraUserName, config.jiraBaseUrl));
        //        _jiraRepo = new JiraRepo(config.jiraBaseUrl, config.jiraUserName, config.jiraAPIToken);

        //        if (_jiraRepo != null)
        //        {
        //            var test = _jiraRepo.GetJira().IssueTypes.GetIssueTypesAsync().Result.ToList();
        //            if (test != null && test.Count > 0)
        //            {
        //                ret = true;
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("");
        //        Console.Beep();
        //        Console.Beep();
        //        Console.WriteLine("Sorry, there seems to be a problem connecting to Jira with the arguments you provided. Error: {0}, {1}\r\n\r\n{2}", ex.Message, ex.Source, ex.StackTrace);
        //        return false;
        //    }

        //    if (ret)
        //    {
        //        ConsoleUtil.WriteLine("Successfully connected to Jira as " + config.jiraUserName);
        //        consoleLines.configInfo = string.Format("User: {0}, Project Key: {1}", config.jiraUserName, config.jiraProjectKey);

        //    }

        //    return ret;
        //}

        private static void BuildNotInitializedQueue()
        {
            consoleLines.AddConsoleLine("This application can be initialized with");
            consoleLines.AddConsoleLine("1. path to config file with arguments");
            //consoleLines.AddConsoleLine("OR");
            //consoleLines.AddConsoleLine("2. the following arguments:");
            //consoleLines.AddConsoleLine("   [Jira UserName] [Jira API Token] [Jira Base URL] [Jira Project Key] [OPTIONAL Jira Card Prefix]");
            consoleLines.AddConsoleLine("");
            consoleLines.AddConsoleLine("For Example:  john.doe@wwt.com SECRETAPIKEY https://client.atlassian.net");
            consoleLines.AddConsoleLine("Please initialize application now per the above example:");
           
        }

        //private static void WriteLine(string text)
        //{
        //    WriteLine(text, false);
        //}

        //private static void WriteLine(string text, bool clearScreen)
        //{
        //    WriteLine(text, ConsoleUtil.DefaultConsoleForeground, ConsoleUtil.DefaultConsoleBackground, clearScreen);
        //}

        //private static void WriteLine(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor, bool clearScreen)
        //{
        //    if (clearScreen)
        //    {
        //        Console.Clear();
        //    }
        //    Console.ForegroundColor = foregroundColor;
        //    Console.BackgroundColor = backgroundColor;
        //    Console.WriteLine(text);
        //    ConsoleUtil.SetDefaultConsoleColors();    

        //}


        public static void AnalyzeIssues(string prefix, string cardNumbers)
        {
            string[] arr = cardNumbers.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < arr.Length; i++)
            {
                AnalyzeOneIssue(string.Format("{0}-{1}",prefix,arr[i]));
            }
        }

        public static string AnalyzeIssue(string key)
        {
            //var settings = new JiraRestClientSettings();
            //settings.EnableUserPrivacyMode = false;

            ConsoleUtil.WriteLine("");
            ConsoleUtil.WriteLine("***** Jira Card: " + key, ConsoleColor.DarkBlue, ConsoleColor.White, false);

            StringBuilder sb = new StringBuilder();

            IssueSearchOptions options = new IssueSearchOptions(string.Format("project={0}", config.jiraProjectKey));
            options.MaxIssuesPerRequest = 50; //this is wishful thinking on my part -- client has this set at 20 -- unless you're a Jira admin, got to live with it.
            options.FetchBasicFields = true;

            var issue = JiraUtil.JiraRepo.GetJira().Issues.Queryable.Where(x => x.Project == config.jiraProjectKey && x.Key == key).FirstOrDefault();

            if (issue == null)
            {
                sb.AppendLine(string.Format("***** Jira Card: " + key + " NOT FOUND!", ConsoleColor.DarkBlue, ConsoleColor.White, false));
                return sb.ToString();
            }

            var labels = issue.Labels;

            var backClr = Console.BackgroundColor;
            var foreClr = Console.ForegroundColor;

            var changeLogs = issue.GetChangeLogsAsync().Result.ToList<IssueChangeLog>();

            List<string> importantFields = new List<string>();
            importantFields.Add("status");
            importantFields.Add("feature team choices");
            importantFields.Add("labels");
            importantFields.Add("resolution");



            for (int i = 0; i < changeLogs.Count; i++)
            {
                IssueChangeLog changeLog = changeLogs[i];

                

                foreach (IssueChangeLogItem cli in changeLog.Items)
                {
                    if (!importantFields.Contains(cli.FieldName.ToLower()))
                    {
                        continue;
                    }
                    else
                    {


                        if (cli.FieldName.ToLower().StartsWith("status"))
                        {
                            sb.AppendFormat("{0} - Changed On {1}, {2} field changed from '{3}' to ", issue.Key, changeLog.CreatedDate.ToString(), cli.FieldName, cli.FromValue);

                            sb.AppendFormat("{0}", cli.ToValue);
                            sb.AppendFormat(Environment.NewLine);
                        }
                        else if (cli.FieldName.ToLower().StartsWith("label"))
                        {
                            sb.AppendFormat("{0} - Changed On {1}, {2} field changed from '{3}' to ", issue.Key, changeLog.CreatedDate.ToString(), cli.FieldName, cli.FromValue);

                            sb.AppendFormat("{0}", cli.ToValue);
                            sb.AppendFormat(Environment.NewLine);
                        }

                        else
                        {
                            sb.AppendLine(string.Format("{0} - Changed On {1}, {2} field changed from '{3}' to '{4}'", issue.Key, changeLog.CreatedDate.ToString(), cli.FieldName, cli.FromValue, cli.ToValue));

                        }

                    }
                }
            }
            sb.AppendLine(string.Format("***** Jira Card: " + key + " END", ConsoleColor.DarkBlue, ConsoleColor.White, false));

            return sb.ToString();
        }

        public static void AnalyzeOneIssue(string key)
        {
            ConsoleUtil.WriteLine("");
            ConsoleUtil.WriteLine("***** Jira Card: " + key, ConsoleColor.DarkBlue, ConsoleColor.White, false);

            var issue = JiraUtil.JiraRepo.GetIssue(key);

            if (issue == null)
            {
                ConsoleUtil.WriteLine("***** Jira Card: " + key + " NOT FOUND!", ConsoleColor.DarkBlue, ConsoleColor.White, false);
                return;
            }

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
            ConsoleUtil.WriteLine("***** Jira Card: " + key + " END", ConsoleColor.DarkBlue, ConsoleColor.White, false);
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
