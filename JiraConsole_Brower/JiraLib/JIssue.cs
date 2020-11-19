using System;
using System.Collections.Generic;
using System.Linq;
using Atlassian.Jira;
using Newtonsoft.Json;

namespace JiraCon
{
    public class JIssue
    {
        private Issue _issue = null;

        private List<string> _components = null;
        private SortedDictionary<string, string[]> _customFields = null;
        private List<string> _labels = null;
        private List<JIssue> _subTasks = null;
        private List<JIssueChangeLog> _changeLogs = null;
        private List<string> preDevStatuses = new List<string>();
        private List<string> preQAStatuses = new List<string>();

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
        public string Summary { get; set; }

        private void Initialize()
        {
            preDevStatuses.Add("backlog");
            preDevStatuses.Add("ready for development");
            preDevStatuses.Add("ready for story writing");
            preDevStatuses.Add("resolving dependencies");
            preDevStatuses.Add("in design");
            preDevStatuses.Add("ready for design");

            preQAStatuses.AddRange(preDevStatuses);
            preQAStatuses.Add("ready for code review");
            preQAStatuses.Add("verifying");
            preQAStatuses.Add("code review");
            preQAStatuses.Add("ready for qa");




            if (_issue == null) return;

            Key = _issue.Key.Value;
            Summary = _issue.Summary.Replace(",", " ");
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
        public List<string> GetStateChanges()
        {
            var ret = new List<string>();

            foreach (JIssueChangeLog cl in _changeLogs)
            {
                foreach (JIssueChangeLogItem cli in cl.Items)
                {
                    if (cli.FieldName == "status")
                    {
                        ret.Add(string.Format("{0}\tStatus from: {1} to: {2}", cl.CreatedDate.ToShortDateString(), cli.FromValue, cli.ToValue));
                    }
                }
            }
                   


            return ret;
        }

        public int InDevBackwardsCount
        {
            get
            {
                return ChangeLogs.Where(x => x.Items.Exists(y => y.FieldName == "status" && y.FromValue == "In Development"  && preDevStatuses.Contains(y.ToValue.ToLower()))).Count();
            }
        }

        public int InQABackwardsCount
        {
            get
            {
                return ChangeLogs.Where(x => x.Items.Exists(y => y.FieldName == "status" && y.FromValue == "In QA" && preQAStatuses.Contains(y.ToValue.ToLower()))).Count();
            }
        }

        public int AfterQAMovedBackwardsCount
        {
            get
            {
                return ChangeLogs.Where(x => x.Items.Exists(y => y.FieldName == "status" && y.FromValue != "In QA" && !preQAStatuses.Contains(y.FromValue) && (preQAStatuses.Contains(y.ToValue) || y.ToValue == "In QA"))).Count();
            }
        }

        public List<string> FailedQASummary
        {
            get
            {
                var ret = new List<string>();



                List<JIssueChangeLog> cLogs = ChangeLogs.Where(x => x.Items.Exists(y => y.FieldName == "labels" && y.ToValue.ToLower().Contains("fail"))).ToList();
                var QAFailCLogs = ChangeLogs.Where(x => x.Items.Exists(y => y.ToValue == "QA Failed")).ToList();
                if (QAFailCLogs.Count() > 0)
                {
                    cLogs.AddRange(QAFailCLogs);
                }


                if (cLogs.Count() > 0 || InQABackwardsCount > 0)
                {
                   //key,type,summary,failedQADate,determinedBy,comments
                    foreach (var cl in cLogs)
                    {
                        foreach (var cli in cl.Items.Where(x=>x.ToValue == "QA Failed"))
                        {
                            ret.Add(string.Format("{0},{1},{2},QAFailed Status,", Key, IssueType, cl.CreatedDate.ToShortDateString(),Summary));
                        }
                        foreach (var cli in cl.Items.Where(x => x.FieldName == "labels" && x.ToValue.ToLower().Contains("fail")).ToList())
                        {
                            ret.Add(string.Format("{0},{1},{4},{2},QAFailed Label,'{3}' Label Added", Key, IssueType, cl.CreatedDate.ToShortDateString(),cli.ToValue,Summary));
                        }
                    }


                    //if (InQABackwardsCount > 0)
                    //{
                    //    ret.Add(string.Format("{0} - QA Info -- Card was moved backwards from QA ** {1} ** times.", Key, InQABackwardsCount));
                    //}

                }

                return ret;
            }
        }

        /// <summary>
        /// Calculate Business days between first In Dev to First Done or Ready for Demo
        /// </summary>
        public string CycleTimeSummary
        {
            get
            {
                string ret = string.Empty;

                List<DateTime> toInDev = new List<DateTime>();
                List<DateTime> toDemo = new List<DateTime>();
                List<DateTime> toDone = new List<DateTime>();

                foreach (var cl in ChangeLogs)
                {
                    foreach (var cli in cl.Items)
                    {
                        if (cli.FieldName == "status" && cli.ToValue == "In Development")
                        {
                            toInDev.Add(cl.CreatedDate);
                            break;
                        }
                        else if (cli.FieldName == "status" && (cli.ToValue == "done" || cli.ToValue == "Done"))
                        {
                            toDone.Add(cl.CreatedDate);
                            break;
                        }
                        else if (cli.FieldName == "status" && (cli.ToValue == "Ready for Demo"))
                        {
                            toDemo.Add(cl.CreatedDate);
                            break;
                        }

                    }
                }

                toInDev.Sort();
                toDone.Sort();
                toDemo.Sort();

                var devDoneDate = GetDevDoneDate(toDemo, toDone);

                if (!devDoneDate.HasValue)
                {
                    return "N/A";
                }

                if (devDoneDate.Value < new DateTime(2020,06,1))
                {
                    return "n/a";
                }
                if (toInDev.Count() == 0)
                {
                    return "n/a";
                }

                if (toInDev[0].Date < new DateTime(2020,6,1))
                {
                    return "n/a";
                }

                if (toInDev.Count == 1)
                {

                    DateTime dev = toInDev[0].Date;

                    double days = JHelper.BusinessDaysUntil(dev,devDoneDate.Value.Date);
                    ret = string.Format("{0},{5},{6},Checked,{1},days,{2},{3},{4}", Key, days, dev.ToShortDateString(), devDoneDate.Value.ToShortDateString(),toInDev.Count,IssueType,Summary);

                }
                else if (toInDev.Count > 1)
                {
                    DateTime dev = toInDev[0];

                    double days = JHelper.BusinessDaysUntil(dev, devDoneDate.Value.Date);
                    ret = string.Format("{0},{5},{6},NotSure,{1},days,{2},{3},{4}", Key, days, dev.ToShortDateString(), devDoneDate.Value.ToShortDateString(),toInDev.Count,IssueType,Summary);
                }


                return ret;
            }
        }

        public DateTime? GetDevDoneDate()
        {
            string ret = string.Empty;

            List<DateTime> toDemo = new List<DateTime>();
            List<DateTime> toDone = new List<DateTime>();

            foreach (var cl in ChangeLogs)
            {
                foreach (var cli in cl.Items)
                {
                    if (cli.FieldName == "status" && (cli.ToValue == "done" || cli.ToValue == "Done"))
                    {
                        toDone.Add(cl.CreatedDate);
                        break;
                    }
                    else if (cli.FieldName == "status" && (cli.ToValue == "Ready for Demo"))
                    {
                        toDemo.Add(cl.CreatedDate);
                        break;
                    }

                }
            }

            toDone.Sort();
            toDemo.Sort();

            return GetDevDoneDate(toDemo, toDone);

            
        }

        public DateTime? GetDevDoneDate(List<DateTime> readyDemoDates, List<DateTime> doneDates)
        {
            DateTime? ret = null;

            DateTime? latestDemoDate = readyDemoDates.OrderBy(x => x.Date).LastOrDefault();
            DateTime? latestDoneDate = doneDates.OrderBy(x => x.Date).LastOrDefault();

            if (latestDemoDate.HasValue && latestDemoDate.Value != DateTime.MinValue)
            {
                ret = latestDemoDate.Value;
            }
            else if (latestDoneDate.HasValue && latestDoneDate.Value != DateTime.MinValue)
            {
                ret = latestDoneDate.Value;
            }

            return ret;
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
