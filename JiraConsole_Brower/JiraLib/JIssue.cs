using System;
using System.Collections.Generic;
using Atlassian.Jira;
using JConsole.JHelpers;
using Newtonsoft.Json;

namespace JConsole.JiraLib
{
    public class JIssue
    {
        private Issue _issue = null;

        private List<string> _components = null;
        private SortedDictionary<string, string[]> _customFields = null;
        private List<string> _labels = null;
        private List<JIssue> _subTasks = null;
        private List<JIssueChangeLog> _changeLogs = null;

        //TODO:  what to do about initializing subTasks and changeLogs

        public JIssue()
        {

        }

        public JIssue(Issue issue)
        {
            this._issue = issue;
            Initialize();
        }

        #region Properties

        public string Key { get; set; }
        public string Project { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ResolutionDate { get; set; }
        public string SecurityLevel { get; set; }
        public string StatusName { get; set; }
        public string StatusDescription { get; set; } 
        public string StatusCategoryKey { get; set; }
        public string StatusCategoryName { get; set; }
        public string ParentIssueKey { get; set; }
        public string IssueType { get; set; }

        private void Initialize()
        {
            if (_issue == null) return;

            Key = _issue.Key.Value;
            Project = _issue.Project;
            CreateDate = _issue.Created ?? null;
            UpdateDate = _issue.Updated ?? null;
            DueDate = _issue.DueDate ?? null;
            ResolutionDate = _issue.ResolutionDate ?? null;
            SecurityLevel = _issue.SecurityLevel == null ? null : _issue.SecurityLevel.Description;
            if (_issue.Status != null)
            {
                StatusName = _issue.Status.Name;
                StatusDescription = _issue.Status.Description;
                StatusCategoryKey = _issue.Status.StatusCategory.Key;
                StatusCategoryName = _issue.Status.StatusCategory.Name;
            }
            ParentIssueKey = _issue.ParentIssueKey;
            IssueType = _issue.Type.Name;
        }

        [JsonIgnore]
        public Issue JiraIssue
        {
            get
            {
                return _issue;
            }
            set
            {
                _issue = value;
                Initialize();

            }
        }

        public void AddSubTask(JIssue issue)
        {
            SubTasks.Add(issue);
        }

        public void AddSubTask(Issue issue)
        {
            SubTasks.Add(new JIssue(issue));
        }

        public void AddChangeLogs(IEnumerable<IssueChangeLog> logs)
        {
            foreach (var l in logs)
            {
                AddChangeLog(l);
            }
        }

        public void AddChangeLog(IssueChangeLog changeLog)
        {
            ChangeLogs.Add(new JIssueChangeLog(changeLog));
        }

        public List<JIssueChangeLog> ChangeLogs
        {
            get
            {
                if (_changeLogs == null) _changeLogs = new List<JIssueChangeLog>();
                return _changeLogs;
            }
            set
            {
                _changeLogs = value;
            }

        }

        public SortedDictionary<string,string[]> CustomFields
        {
            get
            {
                return _customFields;
            }
            set
            {
                _customFields = value;
            }
        }

        public List<string> Components
        {
            get
            {
                if (_components == null)
                {
                    _components = new List<string>();
                }
                return _components;
            }
            set
            {
                _components = value;
            }

        }

        public List<string> Labels
        {
            get
            {
                if (_labels == null)
                {
                    _labels = new List<string>();
                }
                return Labels;
            }
            set
            {
                _labels = value;
            }

        }

        public List<JIssue> SubTasks
        {
            get
            {
                if (_subTasks == null) _subTasks = new List<JIssue>();
                return _subTasks;
            }
            set
            {
                _subTasks = value;
            }
        }

        public List<T> GetCustomFieldValues<T>(string customFieldName)
        {
            List<T> ret = null;

            if (_customFields.ContainsKey(customFieldName) && _customFields[customFieldName] != null)
            {
                ret = new List<T>();
                foreach (string s in _customFields[customFieldName])
                {
                    ret.Add(JHelper.GetValue<T>(s));
                }
            }

            return ret;
        }

        #endregion


    }
}
