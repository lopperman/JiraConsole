using System;
using Atlassian.Jira;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using RestSharp;
using Newtonsoft.Json.Linq;
using System.Text;

namespace JConsole
{
    public class JiraRepo: IJiraRepo
    {
        private Jira _jira;
        

        public JiraRepo(string server, string userName, string password)
        {
            JiraRestClientSettings settings = new JiraRestClientSettings();
            settings.EnableUserPrivacyMode = true;
            

            _jira = Atlassian.Jira.Jira.CreateRestClient(server, userName, password,settings);

            _jira.Issues.MaxIssuesPerRequest = 500;

        }

        public Jira GetJira()
        {
            return _jira;
        }

        public ServerInfo ServerInfo
        {
            get
            {
                return _jira.ServerInfo.GetServerInfoAsync().Result;
            }
        }


        public Project GetProject(string key)
        {
            return _jira.Projects.GetProjectAsync(key).GetAwaiter().GetResult();
        }

        public async Task<Project> GetProjectAsync(string key)
        {
            return await _jira.Projects.GetProjectAsync(key);
        }


        public async Task<Issue> GetIssueAsync(string key)
        {
            return await _jira.Issues.GetIssueAsync(key) as Issue;
        }

        public List<IssueChangeLog> GetIssueChangeLogs(Issue issue)
        {
            return GetIssueChangeLogs(issue.Key.Value);
        }

        public List<IssueChangeLog> GetIssueChangeLogs(string issueKey)
        {
            return GetChangeLogsAsync(issueKey).Result;
        }

        public List<JiraFilter> GetJiraFiltersFavorites()
        {
            return _jira.Filters.GetFavouritesAsync().GetAwaiter().GetResult().ToList();
        }

        //TODO:  implement /rest/api/2/issue/{issueIdOrKey}/transitions


        public List<IssueStatus> GetIssueTypeStatuses(string projKey, string issueType)
        {


            return GetIssueTypeStatusesAsync(projKey, issueType).GetAwaiter().GetResult().ToList();
        }


        
        public async Task<List<IssueStatus>> GetIssueTypeStatusesAsync(string projKey, string issueType, CancellationToken token = default(CancellationToken))
        {

            var ret = new List<IssueStatus>();


            var resourceUrl = String.Format("rest/api/3/project/{0}/statuses", projKey);
            var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
            Newtonsoft.Json.Linq.JToken response = await _jira.RestClient.ExecuteRequestAsync(Method.GET, resourceUrl, null, token)
                .ConfigureAwait(false);

            foreach (var parent in response)
            {
                if (parent["name"].ToString() == issueType)
                {
                    var items = parent["statuses"].Select(a => JsonConvert.DeserializeObject<IssueStatus>(a.ToString(), serializerSettings));
                    ret.AddRange(items);
                }
            }

            return ret;
        }

        public List<Issue> GetSubTasksAsList(Issue issue)
        {
            return GetSubTasks(issue).GetAwaiter().GetResult().ToList();
        }

        public async Task<List<Issue>> GetSubTasks(Issue issue, CancellationToken token = default(CancellationToken))
        {
            List<Issue> result = new List<Issue>();

            int incr = 0;
            int total = 0;


            do
            {

                IPagedQueryResult<Issue> response = await issue.GetSubTasksAsync(10, incr, token).ConfigureAwait(false);

                total = response.TotalItems;

                incr += response.Count();

                result.AddRange(response);
            }
            while (incr < total);

            return result;
        }

        public async Task<List<IssueChangeLog>> GetChangeLogsAsync(string issueKey, CancellationToken token = default(CancellationToken))
        {
            List<IssueChangeLog> result = new List<IssueChangeLog>();

            int incr = 0;
            int total = 0;

            do
            {
                var resourceUrl = String.Format("rest/api/3/issue/{0}/changelog?maxResults=100&startAt={1}", issueKey, incr);
                var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
                var response = await _jira.RestClient.ExecuteRequestAsync(Method.GET, resourceUrl, null, token)
                    .ConfigureAwait(false);

                JToken changeLogs = response["values"];
                JToken totalChangeLogs = response["total"];

                if (totalChangeLogs != null)
                {
                    total = JsonConvert.DeserializeObject<Int32>(totalChangeLogs.ToString(), serializerSettings);
                }

                if (changeLogs != null)
                {
                    var items = changeLogs.Select(cl => JsonConvert.DeserializeObject<IssueChangeLog>(cl.ToString(), serializerSettings));

                    incr += items.Count();

                    result.AddRange(items);
                }
            }
            while (incr < total);

            return result;
        }

        public List<Issue> GetIssues(string jql, bool basicFields, params string[] additionalFields)
        {
            List<Issue> issues = new List<Issue>();

            int incr = 0;
            int total = 0;

            IssueSearchOptions searchOptions = new IssueSearchOptions(jql);
            searchOptions.FetchBasicFields = basicFields;
            searchOptions.MaxIssuesPerRequest = _jira.Issues.MaxIssuesPerRequest;
            
            if (additionalFields != null)
            {
                searchOptions.AdditionalFields = additionalFields.ToList();
            }


            do
            {
                searchOptions.StartAt = incr;

                Task<IPagedQueryResult<Issue>> results = _jira.Issues.GetIssuesFromJqlAsync(searchOptions);
                results.Wait();

                total = results.Result.TotalItems;

                foreach (Issue i in results.Result)
                {
                    issues.Add((Issue)i);
                }

                incr += results.Result.Count();
            }
            while (incr < total);

            return issues;

        }

        public List<Issue> GetIssues(string jql)
        {
            return GetIssues(jql, true);        
        }

        /// <summary>
        /// Get Issues (Slow) - faster to use GetIssues(string jql)
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public List<Issue> GetIssues(params string[] keys)
        {
            List<Issue> issues = new List<Issue>();
            if (keys != null && keys.Length > 0)
            {
                for (int i = 0; i < keys.Count(); i ++)
                {
                    var issue = GetIssue(keys[i]);
                    if (issue != null)
                    {
                        issues.Add(issue);
                    }
                        
                }
            }
            return issues;
        }

        public Issue GetIssue(string key)
        {
            //IssueSearchOptions options = new IssueSearchOptions(string.Format("project={0}", config.jiraProjectKey));
            IssueSearchOptions options = new IssueSearchOptions(string.Format("Key = {0}", key));
            options.MaxIssuesPerRequest = 50; //this is wishful thinking on my part -- client has this set at 20 -- unless you're a Jira admin, got to live with it.
            options.FetchBasicFields = true;            

            return GetJira().Issues.Queryable.Where(x=>x.Key == key).FirstOrDefault();

        }

        public List<IssueType> GetIssueTypes(string projectKey)
        {
            var proj = GetProject(projectKey);

            if (proj != null)
            {
                return proj.GetIssueTypesAsync().Result.ToList();
            }

            return null;

        }

        public List<ProjectComponent> GetProjectComponents(string projectKey)
        {
            return GetProject(projectKey).GetComponentsAsync().Result.ToList();
        }


        public List<IssueStatus> GetIssueStatuses()
        {

            return _jira.Statuses.GetStatusesAsync().Result.ToList();

        }




        //public Task<List<Issue>> GetEpicBySummaryAsync(string project, string summary)
        //{
        //    return Task.Factory.StartNew(() => (from i in _jira.Issues
        //                                        where i.Project == project && i.Type == "Epic" && i.Summary == summary
        //                                        select i).ToList());
        //}

        //public Task<List<Issue>> GetStoryIssuesAsync(string project)
        //{
        //    return Task.Factory.StartNew(() => (from i in _jira.Issues
        //                                        where i.Project == project && i.Type == "Story"
        //                                        select i).ToList());
        //}

        //public async Task<List<Issue>> GetEpicsAsync(string project)
        //{
        //    return _jira.Issues.Queryable.Where(i=>i.Project == project && i.Type = )


        //    //return Task.Factory.StartNew(() => (from i in _jira.Issues
        //    //                                    where i.Project == project && i.Type == "Epic"
        //    //                                    select i).ToList());
        //}

        //public Task<Issue> CreateIssueAsync(string project, string summary, string description, string epicKey = null, string guid = null, string assignee = null, string reporter = null)
        //{
        //    return Task.Factory.StartNew(() =>
        //    {
        //        Issue newIssue = _jira.CreateIssue(project);
        //        newIssue.Summary = summary;
        //        newIssue.Description = description;
        //        newIssue.Assignee = assignee;
        //        newIssue.Reporter = reporter;
        //        newIssue.Priority = "Low";
        //        newIssue.Type = "Story";

        //        if (epicKey != null)
        //            newIssue["Epic Link"] = epicKey;

        //        newIssue.SaveChanges();
        //        newIssue.Refresh();
        //        return newIssue;
        //    });
        //}

        //public Task<Issue> UpdateIssueAsync(Issue issue)
        //{
        //    return Task.Factory.StartNew(() =>
        //    {
        //        issue.SaveChanges();
        //        issue.Refresh();
        //        return issue;
        //    });
        //}
    }

    public interface IJiraRepo
    {
        Project GetProject(string key);
        Task<Project> GetProjectAsync(string key);
        Task<Issue> GetIssueAsync(string key);
        List<IssueChangeLog> GetIssueChangeLogs(Issue issue);
        List<Issue> GetIssues(string jql);
    }
}
