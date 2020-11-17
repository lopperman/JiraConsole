using System;
using System.IO;

namespace JiraConsole_Brower.ConsoleHelpers
{
    public static class ConfigHelper
    {
        internal static JiraConfiguration BuildConfig(string[] args)
        {
            if (args.Length == 1)
            {
                return LoadConfigFile(args[0]);
            }

            JiraConfiguration config = null;


            if (args.Length == 1)
            {
                config = LoadConfigFile(args[0]);
            }
            else if (args.Length == 3)
            {
                config = new JiraConfiguration(args[0], args[1], args[2]);
            }
            else if (args.Length == 4)
            {
                config = new JiraConfiguration(args[0], args[1], args[2], args[3]);
            }
            else if (args.Length == 5)
            {
                config = new JiraConfiguration(args[0], args[1], args[2], args[3], args[4]);
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
    }

    public class JiraConfiguration
    {
        public JiraConfiguration(string jiraUserName, string jiraAPIToken, string jiraBaseUrl, string jiraProjectKey, string jiraCardPrefix)
        {
            this.jiraUserName = jiraUserName;
            this.jiraAPIToken = jiraAPIToken;
            this.jiraBaseUrl = jiraBaseUrl;
            this.jiraProjectKey = jiraProjectKey;
            this.jiraCardPrefix = jiraCardPrefix;
        }

        public JiraConfiguration(string jiraUserName, string jiraAPIToken, string jiraBaseUrl, string jiraProjectKey)
        {
            this.jiraUserName = jiraUserName;
            this.jiraAPIToken = jiraAPIToken;
            this.jiraBaseUrl = jiraBaseUrl;
            this.jiraProjectKey = jiraProjectKey;
            this.jiraCardPrefix = string.Empty;
        }

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
        public string jiraProjectKey { get; set; }
        public string jiraCardPrefix { get; set; } //e.g. "POS-"

    }
}
