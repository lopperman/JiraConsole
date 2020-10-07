using System;
using Atlassian.Jira;

namespace JConsole.JHelpers
{
    public class JHelper
    {
        public JHelper()
        {
        }

        public static T GetValue<T>(String value)
        {
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Unable to convert '{0}' to Type: {1}", value, typeof(T).FullName));
            }
        }
    }

}
