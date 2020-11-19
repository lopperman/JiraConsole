using System;
using System.IO;
using System.Linq;
using Terminal.Gui;

namespace JiraCon
{
    public static class ConfigHelper
    {
        const string configFileName = "JiraConConfig.txt";

        internal static JiraConfiguration BuildConfig(string[] args)
        {
            JiraConfiguration config = null;

            if (args.Length == 1)
            {
                return LoadConfigFile(args[0]);
            }

            if (args.Length == 1)
            {
                config = LoadConfigFile(args[0]);
            }
            else if (args.Length == 3)
            {
                config = new JiraConfiguration(args[0], args[1], args[2]);
            }

            return config;
        }

        private static JiraConfiguration LoadConfigFile(string configFilePath)
        {
            JiraConfiguration configuration = null;

            StreamReader reader = null;

            try
            {
                reader = new StreamReader(configFilePath);
                string line1 = reader.ReadLine();
                if (!string.IsNullOrWhiteSpace(line1))
                {
                    string[] arr = line1.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    configuration = BuildConfig(arr);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Loading Config File: {0}", ex.Message);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }

            return configuration;
        }

        public static void KillConfig()
        {
            var personalFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Library/Application Support/JiraCon";
            if (Directory.Exists(personalFolder))
            {
                string configFile = Path.Combine(personalFolder, configFileName);
                if (File.Exists(configFile))
                {
                    File.Delete(configFile);
                }
            }

            ConsoleUtil.WriteLine("Config file has been deleted. Run program again to create new config file. Press any key to exit.", ConsoleColor.White, ConsoleColor.DarkMagenta, true);
            Console.ReadKey();
        }

        public static string[] GetConfig()
        {
            string[] ret = null;

            var personalFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Library/Application Support/JiraCon";
            if (!Directory.Exists(personalFolder))
            {
                Directory.CreateDirectory(personalFolder);
            }
            string configFile = Path.Combine(personalFolder, configFileName);

            if (File.Exists(configFile))
            {
                //check to confirm file has 3 arguments
                using (StreamReader reader = new StreamReader(configFile))
                {
                    var text = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        var arr = text.Split(' ');
                        if (arr.Length == 3)
                        {
                            ret = arr;
                        }
                    }
                }
            }

            if (ret == null)
            {
                string userName = "";
                string apiToken = "";
                string jiraBaseUrl = "";

                userName = GetConsoleInput("Missing config -- please enter username (email address) for Jira login:");
                apiToken = GetConsoleInput("Missing config -- please enter API token for Jira login:");
                jiraBaseUrl = GetConsoleInput("Missing config -- please enter base url for Jira instance:");

                bool validCredentials = false;
                //test connection
                try
                {
                    ConsoleUtil.WriteLine("testing Jira connection ...");
                    var testConn = new JiraRepo(jiraBaseUrl, userName, apiToken);

                    if (testConn != null)
                    {
                        var test = testConn.GetJira().IssueTypes.GetIssueTypesAsync().Result.ToList();
                        if (test != null && test.Count > 0)
                        {
                            validCredentials = true;
                            ConsoleUtil.WriteLine("testing Jira connection ... successful");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ConsoleUtil.WriteLine("testing Jira connection ... failed");
                    ConsoleUtil.WriteLine(ex.Message);
                }

                if (!validCredentials)
                {
                    return GetConfig();
                }
                else
                {
                    using (StreamWriter writer = new StreamWriter(configFile))
                    {
                        writer.WriteLine(string.Format("{0} {1} {2}", userName, apiToken, jiraBaseUrl));
                    }
                    return GetConfig();
                }
            }

            return ret;

        }

        private static string GetConsoleInput(string message)
        {
            Console.WriteLine("...");
            ConsoleUtil.WriteLine(message);
            var ret = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(ret))
            {
                return GetConsoleInput(message);
            }
            ConsoleUtil.WriteLine("");
            ConsoleUtil.WriteLine(string.Format("Enter 'Y' to Use '{0}', otherwise enter 'E' to exit or another key to enter new value", ret));
            var key = Console.ReadKey();

            if (key.Key == ConsoleKey.E)
            {
                Environment.Exit(0);
            }
            if (key.Key != ConsoleKey.Y)
            {
                return GetConsoleInput(message);
            }

            return ret;

        }

    }

    public class JiraConfiguration
    {

        public JiraConfiguration(string jiraUserName, string jiraAPIToken, string jiraBaseUrl)
        {
            this.jiraUserName = jiraUserName;
            this.jiraAPIToken = jiraAPIToken;
            this.jiraBaseUrl = jiraBaseUrl;
        }

        public bool IsValid { get; set; }
        public string jiraUserName { get; set; }
        public string jiraAPIToken { get; set; }
        public string jiraBaseUrl { get; set; }

    }
}
