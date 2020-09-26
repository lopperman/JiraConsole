using System;
using Atlassian.Jira;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JConsole
{
    public class JiraRepo: IJiraRepo
    {
        private Atlassian.Jira.Jira _jira;

        public JiraRepo(string server, string userName, string password)
        {
            _jira = Atlassian.Jira.Jira.CreateRestClient(server, userName, password);

            _jira.Issues.MaxIssuesPerRequest = 500;

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
            return await _jira.Issues.GetIssueAsync(key);
        }

        public List<IssueChangeLog> GetIssueChangeLogs(Issue issue)
        {
            var res = issue.GetChangeLogsAsync().Result.ToList();

            return res;
        }

        //public List<IssueChangeLog> GetIssueChangeLogs_Test(Issue iss)
        //{

        //    iss.GetChangeLogsAsync..


        //    List<IssueChangeLog> ret = new List<IssueChangeLog>();

        //    int incr = 0;
        //    int total = 0;

        //    do
        //    {
        //        //searchOptions.StartAt = incr;
        //        //searchOptions.MaxIssuesPerRequest = 500;

        //        //iss.GetChangeLogsAsync.startAt = incr;

        //        Task<IEnumerable<IssueChangeLog>> results = iss.GetChangeLogsAsync.GetIssueChangeLogs_Pagination
        //        results.Wait();

        //        total = results.Result.TotalItems;

        //        foreach (Issue i in results.Result)
        //        {
        //            issues.Add(i);
        //        }

        //        incr += results.Result.Count();
        //    }
        //    while (incr < total);

        //    return issues;


        //}


        public List<Issue> GetIssues(string jql)
        {
            List<Issue> issues = new List<Issue>();

            int incr = 0;
            int total = 0;

//            IssueSearchOptions searchOptions = new IssueSearchOptions(jql);

            do
            {
                //searchOptions.StartAt = incr;
                //searchOptions.MaxIssuesPerRequest = 500;

                Task<IPagedQueryResult<Issue>> results = _jira.Issues.GetIssuesFromJqlAsync(jql, _jira.Issues.MaxIssuesPerRequest, incr);
                results.Wait();

                total = results.Result.TotalItems;

                foreach (Issue i in results.Result)
                {
                    issues.Add(i);
                }

                incr += results.Result.Count();
            }
            while (incr < total);

            return issues;
        
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
