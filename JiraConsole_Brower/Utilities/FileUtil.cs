using System;
using System.IO;
using Newtonsoft.Json;

namespace JConsole.Utilities
{
    public class FileUtil
    {
        public FileUtil()
        {
        }

        public static void SaveToJSON(JiraData obj, string path)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                string data = JsonConvert.SerializeObject(obj,Formatting.None,settings);

                using (StreamWriter writer = new StreamWriter(path, false))
                {
                    writer.Write(data);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Obsolete("Need to figure out how to deserialize Atlassian.Jira.Issue",true)]
        public static JiraData LoadFromJSON(string path)
        {
            JiraData obj = null;

            JsonSerializerSettings settings = new JsonSerializerSettings();
            //settings.ConstructorHandling = 

            try
            {
                string data = string.Empty;
                using (StreamReader reader = new StreamReader(path))
                {
                    data = reader.ReadToEnd();
                }

                obj = JsonConvert.DeserializeObject<JiraData>(data,settings);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return obj;
        }
    }
}
