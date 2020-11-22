using System;
using System.Collections.Generic;
using System.Linq;

namespace JConsole.Utilities
{

    public class JIssueStateMetrics
    {
        private JStates _jStates = null;

        public JIssueStateMetrics(JStates jstates)
        {
            _jStates = jstates;
        }

        

    }


    public class JStates
    {
        private List<JState> _states = new List<JState>();

        public List<JState> GetStates
        {
            get
            {
                return _states;
            }
        }

        public void AddState(string stateName, bool activeWork, bool passiveWork)
        {
            if (GetState(stateName)==null)
            {
                _states.Add(new JState(stateName, activeWork, passiveWork));
            }
        }

        public JState GetState(string stateName)
        {
            return _states.FirstOrDefault(x => x.State.CompareTo(stateName) == 0);
        }
    }

    public class JState: IComparable<JState>
    {
        public JState()
        {
        }

        public JState(string state, bool active, bool passive)
        {
            State = state;
            Active = active;
            Passive = passive;
        }

        public string State { get; set; }
        public bool Active { get; set; }
        public bool Passive{ get; set; }

        public int CompareTo(JState other)
        {
            return string.Compare(this.State, other.State,true);
        }
    }
}
