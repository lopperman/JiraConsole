using System;
using System.Collections.Generic;
using Atlassian.Jira;

namespace JConsole
{
    public class JiraData
    {

        private List<Issue> _issues = new List<Issue>();
        private SortedList<string, List<IssueChangeLog>> _changeLog = new SortedList<string, List<IssueChangeLog>>();

        public JiraData()
        {
        }

        public List<Issue> JiraIssues
        {
            get
            {
                return _issues;
            }
        }

        public void AddIssueChangeLogs(string issueKey, List<IssueChangeLog> changeLogs)
        {
            if (_changeLog.ContainsKey(issueKey))
            {
                _changeLog[issueKey] = changeLogs;
            }
            else
            {
                _changeLog.Add(issueKey, changeLogs);
            }
        }

        public List<IssueChangeLog> GetIssueChangeLogs(string issueKey)
        {
            if (_changeLog.ContainsKey(issueKey))
            {
                return _changeLog[issueKey];
            }
            return null;
        }

    }
}
