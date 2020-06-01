using System;
using Atlassian.Jira;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace JiraConsole_Brower
{
    class MainClass
    {

        public static void Main(string[] args)
        {
            if (args != null && args.Length == 5)
            {
                Main(args[0], args[1], args[2], args[3], args[4]);
            }
            else
            {
                Console.WriteLine("Incorrect Arguments");
                Console.WriteLine("Good-bye");
            }
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
                settings.EnableUserPrivacyMode = true;


                IssueSearchOptions options = new IssueSearchOptions(string.Format("project={0}", jiraProjectKey));
                options.MaxIssuesPerRequest = 500;
                options.FetchBasicFields = true;

                Console.WriteLine("connecting to {0}@{1} ...", jiraUserName, jiraBaseUrl);

                var jira = Jira.CreateRestClient(jiraBaseUrl, jiraUserName, jiraAPIToken, settings);


                var issues = jira.Issues.GetIssuesFromJqlAsync(options).Result;

                var proj = jira.Projects.GetProjectAsync(jiraProjectKey).Result;

                int takeCount = 50;
                int startAt = 0;
                int currentCount = 0;

                while (true)
                {
                    var changed = jira.Issues.Queryable.Where(x => x.Project == "POS" && x.Type == "Story" && x.Status != "Archive" && x.Status != "Archived");
                    var changedList = changed.Skip(startAt).Take(takeCount).ToList();

                    if (changedList.Count == 0) break;

                    currentCount += changedList.Count;
                    startAt += currentCount;


                    foreach (var issue in changedList)
                    {

                        JiraCard jiraCard = new JiraCard(issue.JiraIdentifier, issue.Key.Value, issue.Status.Name, "", issue.Created.Value, issue.Updated.Value);

                        var changeLogs = issue.GetChangeLogsAsync().Result.ToList<IssueChangeLog>();


                        for (int i = 0; i < changeLogs.Count; i++)
                        {
                            IssueChangeLog changeLog = changeLogs[i];

                            foreach (IssueChangeLogItem cli in changeLog.Items)
                            {
                                jiraCard.AddChangeLog(changeLog.Id, changeLog.CreatedDate, cli.FieldName, cli.FieldType, cli.FromId, cli.FromValue, cli.ToId, cli.ToValue);
                            }

                        }

                        jiraCards.Add(jiraCard);

                        Console.WriteLine(issue.Key + " change logs:" + changeLogs.Count);
                    }
                }

                Console.WriteLine("");
                Console.WriteLine("completed data extraction from Jira ...");
                Console.WriteLine("");
                Console.Beep();
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

        public static void WriteCSVFile(List<JiraCard> cards, string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            StreamWriter writer = new StreamWriter(filePath, false);
            writer.WriteLine("id{0}key{0}status{0}description{0}created{0}updated{0}changeLogId{0}changeLogDt{0}fieldName{0}fieldType{0}fromId{0}fromValue{0}toId{0}toValue{0}DevStart{0}DevDone{0}CycleTimeDays", ",");

            foreach (var card in cards)
            {
                foreach (var cLog in card.ChangeLogs)
                {
                    if (!cLog.FieldName.ToLower().StartsWith("desc") && !cLog.FieldName.ToLower().StartsWith("comment"))
                    {
                        writer.WriteLine("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}{12}{0}{13}{0}{14}{0}{15}{0}{16}{0}{17}", ",", card._id, card.Key, card.Status, "", card.Created, card.Updated, cLog._id, cLog.ChangeLogDt, cLog.FieldName, cLog.FieldType, cLog.FromId, CleanText(cLog.FromValue), cLog.ToId, CleanText(cLog.ToValue),card.DevStartDt.HasValue ? card.DevStartDt.Value.ToString() : "",card.DevDoneDt.HasValue ? card.DevDoneDt.Value.ToString() : "",card.CycleTime.HasValue ? card.CycleTime.ToString() : "");
                    }

                }
            }

            writer.Close();
        }

        public static string CleanText(string text)
        {
            string ret = string.Empty;

            if (!String.IsNullOrEmpty(text))
            {
                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                ret = rgx.Replace(text, "");
            }
            return ret;
        }
    }
}
