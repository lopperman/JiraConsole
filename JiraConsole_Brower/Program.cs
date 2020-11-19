using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using Atlassian.Jira;

namespace JiraCon
{
    class MainClass
    {
        private static bool _initialized = false;
        static JiraConfiguration config = null;
        static JiraRestClientSettings _settings = null;
        private static string[] _args = null;

        public static void Main(string[] args)
        {

            ConsoleUtil.InitializeConsole(ConsoleColor.White, ConsoleColor.Black);

            if (args == null || args.Length == 0)
            {
                args = ConfigHelper.GetConfig();
            }

            _args = args;
            bool showMenu = true;

            while (showMenu)
            {
                showMenu = MainMenu();
            }
            ConsoleUtil.Lines.ByeBye();
            Environment.Exit(0);
        }


        //private static void KillConfig()
        //{
        //    var personalFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Library/Application Support/JiraCon";
        //    if (Directory.Exists(personalFolder))
        //    {
        //        string configFile = Path.Combine(personalFolder, configFileName);
        //        if (File.Exists(configFile))
        //        {
        //            File.Delete(configFile);
        //        }
        //    }

        //    ConsoleUtil.WriteLine("Config file has been deleted. Run program again to create new config file. Press any key to exit.",ConsoleColor.White,ConsoleColor.DarkMagenta,true);
        //    Console.ReadKey();
        //}

        //private static string[] GetConfig()
        //{
        //    string[] ret = null;

        //    var personalFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Library/Application Support/JiraCon";
        //    if (!Directory.Exists(personalFolder))
        //    {
        //        Directory.CreateDirectory(personalFolder);
        //    }
        //    string configFile = Path.Combine(personalFolder, configFileName);

        //    if (File.Exists(configFile))
        //    {
        //        //check to confirm file has 3 arguments
        //        using (StreamReader reader = new StreamReader(configFile))
        //        {
        //            var text = reader.ReadLine();
        //            if (!string.IsNullOrWhiteSpace(text))
        //            {
        //                var arr = text.Split(' ');
        //                if (arr.Length == 3)
        //                {
        //                    ret = arr;
        //                }
        //            }
        //        }
        //    }

        //    if (ret == null)
        //    {
        //        string userName = "";
        //        string apiToken = "";
        //        string jiraBaseUrl = "";

        //        userName = GetConsoleInput("Missing config -- please enter username (email address) for Jira login:");
        //        apiToken = GetConsoleInput("Missing config -- please enter API token for Jira login:");
        //        jiraBaseUrl = GetConsoleInput("Missing config -- please enter base url for Jira instance:");

        //        bool validCredentials = false;
        //        //test connection
        //        try
        //        {
        //            ConsoleUtil.WriteLine("testing Jira connection ...");
        //            var testConn = new JiraRepo(jiraBaseUrl, userName, apiToken);

        //            if (testConn != null)
        //            {
        //                var test = testConn.GetJira().IssueTypes.GetIssueTypesAsync().Result.ToList();
        //                if (test != null && test.Count > 0)
        //                {
        //                    validCredentials = true;
        //                    ConsoleUtil.WriteLine("testing Jira connection ... successful");
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            ConsoleUtil.WriteLine("testing Jira connection ... failed");
        //            ConsoleUtil.WriteLine(ex.Message);
        //        }

        //        if (!validCredentials)
        //        {
        //            return GetConfig();
        //        }
        //        else
        //        {
        //            using (StreamWriter writer = new StreamWriter(configFile))
        //            {
        //                writer.WriteLine(string.Format("{0} {1} {2}", userName, apiToken, jiraBaseUrl));
        //            }
        //            return GetConfig();
        //        }
        //    }

        //    return ret;

        //}


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
                    ConsoleUtil.BuildNotInitializedQueue();
                    ConsoleUtil.Lines.WriteQueuedLines(true);
                    string vs = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(vs)) vs = string.Empty;
                    string[] arr = vs.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    config = ConfigHelper.BuildConfig(arr);

                    ConsoleUtil.Lines.configInfo = string.Format("User: {0}", config.jiraUserName);


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
                        ConsoleUtil.WriteLine("Successfully connected to Jira as " + config.jiraUserName);
                        ConsoleUtil.Lines.configInfo = string.Format("User: {0}", config.jiraUserName);

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
            ConsoleUtil.BuildInitializedMenu();
            ConsoleUtil.Lines.WriteQueuedLines(true);

            var resp = Console.ReadKey();
            if (resp.Key == ConsoleKey.M)
            {
                ConsoleUtil.WriteLine("");
                ConsoleUtil.WriteLine("Enter 1 or more card keys separated by a space (e.g. POS-123 POS-456 BAM-789), or E to exit.", ConsoleColor.Black, ConsoleColor.White, false);
                var keys = Console.ReadLine().ToUpper();
                if (string.IsNullOrWhiteSpace(keys))
                {
                    return true;
                }
                if (keys.ToUpper() == "E")
                {
                    return false;
                }

                string[] arr = keys.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length >= 1)
                {
                    ConsoleUtil.WriteLine("Would you like to include changes to card description and comments? (enter Y to include)");
                    resp = Console.ReadKey();
                    if (resp.Key == ConsoleKey.Y)
                    {
                        AnalyzeIssues(keys,true);
                    }
                    else
                    {
                        AnalyzeIssues(keys,false);
                    }

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
                var keys = Console.ReadKey();
                if (keys.Key == ConsoleKey.Y)
                {
                    CreateExtractFiles(jql);
                    ConsoleUtil.WriteLine("");
                    ConsoleUtil.WriteLine("Press any key to continue.");
                    Console.ReadKey();
                }
                return true;

            }
            else if (resp.Key == ConsoleKey.K)
            {
                ConfigHelper.KillConfig();
                return false;
            }
            return false;
        }

        private static void CreateExtractFiles(string jql)
        {
            try
            {
                Console.WriteLine("** Hey Paul, don't forget to include the filter options for each file type **");
                Console.ReadLine();

                DateTime now = DateTime.Now;
                string fileNameSuffix = string.Format("_{0:0000}{1}{2:00}_{3}.txt", now.Year, now.ToString("MMM"), now.Day, now.ToString("hhmmss"));

                string cycleTimeFile = String.Format("JiraCon_CycleTime_{0}", fileNameSuffix);
                string qaFailFile = String.Format("JiraCon_QAFailure_{0}", fileNameSuffix);
                string velocityFile = String.Format("JiraCon_Velocity_{0}", fileNameSuffix);
                string changeHistoryFile = String.Format("JiraCon_ChangeHistory_{0}", fileNameSuffix);
                string extractConfigFile = String.Format("JiraCon_ExtractConfig_{0}", fileNameSuffix);


                ConsoleUtil.WriteLine(string.Format("getting issues from JQL:{0}",Environment.NewLine));
                ConsoleUtil.WriteLine(string.Format("{0}", jql));
                ConsoleUtil.WriteLine("");

                var issues = JiraUtil.JiraRepo.GetIssues(jql);

                ConsoleUtil.WriteLine(string.Format("Retrieved {0} issues", issues.Count()));

                List<JIssue> jissues = new List<JIssue>();

                foreach (var issue in issues)
                {
                    ConsoleUtil.WriteLine(string.Format("getting changelogs for {0}", issue.Key.Value));
                    JIssue newIssue = new JIssue(issue);
                    newIssue.AddChangeLogs(JiraUtil.JiraRepo.GetIssueChangeLogs(issue));

                    jissues.Add(newIssue);
                }

                var extractFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "JiraCon");
                if (!Directory.Exists(extractFolder))
                {
                    Directory.CreateDirectory(extractFolder);
                }

                ConsoleUtil.WriteLine("Calculating QA Failures ...");
                CreateQAFailExtract(jissues, Path.Combine(extractFolder, qaFailFile));
                ConsoleUtil.WriteLine(string.Format("Created qa failures file ({0})", qaFailFile));

                ConsoleUtil.WriteLine("Calculating cycle times...");
                CreateCycleTimeExtract(jissues, Path.Combine(extractFolder, cycleTimeFile));
                ConsoleUtil.WriteLine(string.Format("Created cycle time file ({0})", cycleTimeFile));

                ConsoleUtil.WriteLine("Calculating velocities ...");
                CreateVelocityExtract(jissues, Path.Combine(extractFolder, velocityFile));
                ConsoleUtil.WriteLine(string.Format("Created velocity file ({0})", velocityFile));

                //ConsoleUtil.WriteLine("Organizing change logs ...");
                //CreateChangeLogExtract(jissues, Path.Combine(extractFolder, changeHistoryFile));
                //ConsoleUtil.WriteLine(string.Format("Created change log file ({0})", changeHistoryFile));

                ConsoleUtil.WriteLine("writing config for this extract process ...");
                CreateConfigExtract(Path.Combine(extractFolder, extractConfigFile),jql);

                ConsoleUtil.WriteLine(string.Format("Created config file ({0})", extractConfigFile));
                ConsoleUtil.WriteLine("");
                ConsoleUtil.WriteLine(string.Format("Files are located in: {0}", extractFolder));
                ConsoleUtil.WriteLine("");

            }
            catch (Exception ex)
            {
                ConsoleUtil.WriteLine("*** An error has occurred ***", ConsoleColor.DarkRed, ConsoleColor.Gray, false);
                ConsoleUtil.WriteLine(ex.Message, ConsoleColor.DarkRed, ConsoleColor.Gray, false);
                ConsoleUtil.WriteLine(ex.StackTrace, ConsoleColor.DarkRed, ConsoleColor.Gray, false);
            }
        }

        private static void CreateConfigExtract(string file, string jql)
        {
            using (StreamWriter w = new StreamWriter(file,false))
            {
                w.WriteLine("***** JQL Used for Extract *****");
                w.WriteLine("");

                w.WriteLine(jql);
            }
        }

        private static void CreateChangeLogExtract(List<JIssue> issues, string file)
        {
            //issues = issues.OrderBy(x => x.Key).ToList();

            //using (StreamWriter writer = new StreamWriter(file))
            //{

            //    writer.WriteLine("key,type,status,name");
            //    foreach (var iss in issues)
            //    {
            //        DateTime? devDoneDate = iss.GetDevDoneDate();
            //        if (devDoneDate.HasValue)
            //        {
            //            //writer.WriteLine(string.Format("{0},{1},{2}", iss.Key, iss.IssueType,iss.StatusName,iss.devDoneDate.Value.ToShortDateString()));
            //        }

            //    }
            //}

        }

        private static void CreateVelocityExtract(List<JIssue> issues, string file)
        {
            issues = issues.OrderBy(x => x.Key).ToList();

            using (StreamWriter writer = new StreamWriter(file))
            {

                writer.WriteLine("key,type,summary,doneDate");
                foreach (var iss in issues)
                {
                    DateTime? devDoneDate = iss.GetDevDoneDate();
                    if (devDoneDate.HasValue)
                    {
                        writer.WriteLine(string.Format("{0},{1},{2},{3}", iss.Key, iss.IssueType, iss.Summary, devDoneDate.Value.ToShortDateString()));
                    }

                }
            }

        }

        private static void CreateCycleTimeExtract(List<JIssue> issues, string file)
        {
            issues = issues.OrderBy(x => x.Key).ToList();

            using (StreamWriter writer = new StreamWriter(file))
            {

                writer.WriteLine("key,type,summary,confidence,cycleTime,cycleTimeUom,inDevDate,doneDate,inDevCount");
                foreach (var iss in issues)
                {
                    DateTime? devDoneDate = iss.GetDevDoneDate();
                    if (devDoneDate.HasValue)
                    {
                        string ct = iss.CycleTimeSummary;
                        if (!string.IsNullOrWhiteSpace(ct) && ct.ToLower() != "n/a")
                        {
                            writer.WriteLine(ct);
                        }
                    }
                }
            }
        }

        private static void CreateQAFailExtract(List<JIssue> issues, string file)
        {
            issues = issues.OrderBy(x => x.Key).ToList();

            using (StreamWriter writer = new StreamWriter(file))
            {

                writer.WriteLine("key,type,failedQADate,determinedBy,comments");
                foreach (var iss in issues)
                {
                    DateTime? devDoneDate = iss.GetDevDoneDate();
                    if (iss.FailedQASummary.Count > 0)
                    {
                        foreach (var s in iss.FailedQASummary)
                        {
                            writer.WriteLine(s);
                        }
                    }

                }
            }

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

        public static void AnalyzeIssues(string cardNumbers, bool includeDescAndComments)
        {
            string[] arr = cardNumbers.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < arr.Length; i++)
            {
                AnalyzeOneIssue(arr[i],includeDescAndComments);
            }
        }

        //public static string AnalyzeIssue(string key)
        //{
        //    //var settings = new JiraRestClientSettings();
        //    //settings.EnableUserPrivacyMode = false;

        //    ConsoleUtil.WriteLine("");
        //    ConsoleUtil.WriteLine("***** Jira Card: " + key, ConsoleColor.DarkBlue, ConsoleColor.White, false);

        //    StringBuilder sb = new StringBuilder();

        //    IssueSearchOptions options = new IssueSearchOptions(string.Format("project={0}", config.jiraProjectKey));
        //    options.MaxIssuesPerRequest = 50; //this is wishful thinking on my part -- client has this set at 20 -- unless you're a Jira admin, got to live with it.
        //    options.FetchBasicFields = true;

        //    var issue = JiraUtil.JiraRepo.GetJira().Issues.Queryable.Where(x => x.Project == config.jiraProjectKey && x.Key == key).FirstOrDefault();

        //    if (issue == null)
        //    {
        //        sb.AppendLine(string.Format("***** Jira Card: " + key + " NOT FOUND!", ConsoleColor.DarkBlue, ConsoleColor.White, false));
        //        return sb.ToString();
        //    }

        //    var labels = issue.Labels;

        //    var backClr = Console.BackgroundColor;
        //    var foreClr = Console.ForegroundColor;

        //    var changeLogs = issue.GetChangeLogsAsync().Result.ToList<IssueChangeLog>();

        //    List<string> importantFields = new List<string>();
        //    importantFields.Add("status");
        //    importantFields.Add("feature team choices");
        //    importantFields.Add("labels");
        //    importantFields.Add("resolution");



        //    for (int i = 0; i < changeLogs.Count; i++)
        //    {
        //        IssueChangeLog changeLog = changeLogs[i];

                

        //        foreach (IssueChangeLogItem cli in changeLog.Items)
        //        {
        //            if (!importantFields.Contains(cli.FieldName.ToLower()))
        //            {
        //                continue;
        //            }
        //            else
        //            {


        //                if (cli.FieldName.ToLower().StartsWith("status"))
        //                {
        //                    sb.AppendFormat("{0} - Changed On {1}, {2} field changed from '{3}' to ", issue.Key, changeLog.CreatedDate.ToString(), cli.FieldName, cli.FromValue);

        //                    sb.AppendFormat("{0}", cli.ToValue);
        //                    sb.AppendFormat(Environment.NewLine);
        //                }
        //                else if (cli.FieldName.ToLower().StartsWith("label"))
        //                {
        //                    sb.AppendFormat("{0} - Changed On {1}, {2} field changed from '{3}' to ", issue.Key, changeLog.CreatedDate.ToString(), cli.FieldName, cli.FromValue);

        //                    sb.AppendFormat("{0}", cli.ToValue);
        //                    sb.AppendFormat(Environment.NewLine);
        //                }

        //                else
        //                {
        //                    sb.AppendLine(string.Format("{0} - Changed On {1}, {2} field changed from '{3}' to '{4}'", issue.Key, changeLog.CreatedDate.ToString(), cli.FieldName, cli.FromValue, cli.ToValue));

        //                }

        //            }
        //        }
        //    }
        //    sb.AppendLine(string.Format("***** Jira Card: " + key + " END", ConsoleColor.DarkBlue, ConsoleColor.White, false));

        //    return sb.ToString();
        //}

        public static void AnalyzeOneIssue(string key, bool includeDescAndComments)
        {

            ConsoleUtil.WriteLine("");
            ConsoleUtil.WriteLine("***** Jira Card: " + key, ConsoleColor.DarkBlue, ConsoleColor.White, false);
            ConsoleUtil.SetDefaultConsoleColors();

            var issue = JiraUtil.JiraRepo.GetIssue(key);

            if (issue == null)
            {
                ConsoleUtil.WriteLine("***** Jira Card: " + key + " NOT FOUND!", ConsoleColor.DarkBlue, ConsoleColor.White, false);
                ConsoleUtil.SetDefaultConsoleColors();
                return;
            }


            ConsoleUtil.WriteLine("***** loading change logs for {0}: " + key);

            var changeLogs = issue.GetChangeLogsAsync().Result.ToList<IssueChangeLog>();

            ConsoleUtil.WriteLine(string.Format("Found {0} change logs for {1}", changeLogs.Count, key));

            for (int i = 0; i < changeLogs.Count; i++)
            {
                ConsoleUtil.SetDefaultConsoleColors();

                IssueChangeLog changeLog = changeLogs[i];


                foreach (IssueChangeLogItem cli in changeLog.Items)
                {
                    if (cli.FieldName.ToLower().StartsWith("desc") || cli.FieldName.ToLower().StartsWith("comment"))
                    {
                        if (includeDescAndComments)
                        {
                            ConsoleUtil.WriteAppend(string.Format("{0} - Changed On {1}, {2} field changed ", issue.Key, changeLog.CreatedDate.ToString(), cli.FieldName), ConsoleUtil.DefaultConsoleForeground, ConsoleUtil.DefaultConsoleBackground, true);
                            ConsoleUtil.WriteAppend(string.Format("\t{0} changed from ",cli.FieldName), true);
                            ConsoleUtil.WriteAppend(string.Format("{0}", cli.FromValue), ConsoleColor.DarkGreen, ConsoleUtil.DefaultConsoleBackground, true);
                            ConsoleUtil.WriteAppend(string.Format("\t{0} changed to ", cli.FieldName), true);
                            ConsoleUtil.WriteAppend(string.Format("{0}", cli.ToValue), ConsoleColor.Green, ConsoleUtil.DefaultConsoleBackground, true);
                        }
                    }
                    else
                    {


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

            ConsoleUtil.SetDefaultConsoleColors();
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
