using System;
using System.Collections.Generic;
using System.Linq;
using JiraCon;

namespace JConsole.Utilities
{

    public class WorkMetrics
    {
        private SortedList<JIssue, List<WorkMetric>> _workMetricList = new SortedList<JIssue, List<WorkMetric>>();
        private JiraRepo _repo = null;

        public WorkMetrics(JiraRepo repo)
        {
            if (repo == null)
            {
                throw new NullReferenceException("class 'WorkMetrics' cannot be instantiated with a null JiraRepo object");
            }
            _repo = repo;
        }

        public SortedList<JIssue,List<WorkMetric>> GetWorkMetrics()
        {
            return _workMetricList;
        }

        public List<WorkMetric> AddIssue(JIssue issue)
        {
            var ret = BuildIssueMetrics(issue);
            _workMetricList.Add(issue, ret);
            return ret;
        }

        public void AddIssue(IEnumerable<JIssue> issues)
        {
            foreach (var iss in issues)
            {
                AddIssue(iss);
            }
        }

        private List<WorkMetric> BuildIssueMetrics(JIssue issue)
        {
            var ret = new List<WorkMetric>();

            SortedDictionary<DateTime, JItemStatus> statusStartList = new SortedDictionary<DateTime, JItemStatus>();

            foreach (var changeLog in issue.ChangeLogs)
            {
                var items = changeLog.Items.Where(item => item.FieldName == "status");
                foreach (JIssueChangeLogItem item in items)
                {
                    var itemStatus = _repo.JItemStatuses.SingleOrDefault(y=>y.StatusName.ToLower() == item.ToValue.ToLower());
                    if (itemStatus == null)
                    {
                        var err = string.Format("Error getting JItemStatus for {0} ({1}).  Cannot determine calendar/active work time for state '{2}'", issue.Key, issue.IssueType, item.ToValue);
                        ConsoleUtil.WriteLine(err,ConsoleColor.DarkRed,ConsoleColor.Gray,false);
                        ConsoleUtil.WriteLine("If issue type is an Epic then you should be ok. PRESS ANY KEY TO CONTINUE", ConsoleColor.DarkRed, ConsoleColor.Gray, false);
                        var ok = Console.ReadKey();

                    }
                    if (itemStatus == null)
                    {
                        itemStatus = new JItemStatus(item.ToValue.ToLower(), "ERROR", "ERROR", "ERROR");
                    }
                    statusStartList.Add(changeLog.CreatedDate, itemStatus);
                }
            }

            if (statusStartList.Count == 0)
            {
                return ret;
            }

            var keys = statusStartList.Keys.ToList().OrderBy(x => x).ToList();

            for (int i = 0; i < keys.Count; i ++)
            {
                //takes care of entries for first (if more than one item exists) and last
                if (keys.Count == 1)
                {
                    ret.Add(new WorkMetric(statusStartList[keys[i]], keys[i], DateTime.Now));
                }
                else if (i == keys.Count -1)
                {
                    ret.Add(new WorkMetric(statusStartList[keys[i - 1]], keys[i - 1], keys[i]));
                    ret.Add(new WorkMetric(statusStartList[keys[i]], keys[i], DateTime.Now));
                }
                else if (i > 0)
                {
                    ret.Add(new WorkMetric(statusStartList[keys[i - 1]], keys[i - 1], keys[i]));
                }
            }

            return ret;
        }

    }

    public class WorkMetric
    {
        public JItemStatus ItemStatus { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public WorkMetric(JItemStatus itemStatus, DateTime start, DateTime end)
        {
            ItemStatus = itemStatus;
            Start = start;
            End = end;            
        }

        public double TestTotalDays
        {
            get
            {
                return Math.Round(End.Subtract(Start).TotalDays, 2);
            }
        }
        public double TestTotalHours
        {
            get
            {
                return Math.Round(End.Subtract(Start).TotalHours, 1);
            }
        }
        public bool IncludeForTimeCalc
        {
            get
            {
                return (ItemStatus.ActiveWork || ItemStatus.CalendarWork);
            }
        }



        //        public // what day/hour combination do we use?
    }



}
