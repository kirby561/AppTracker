using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTracker {
    class TimeTracker {
        private Dictionary<String, ProcessEntry> _appTimes = new Dictionary<String, ProcessEntry>();

        /// <summary>
        /// Adds the given amount of time to the given process.
        /// </summary>
        /// <param name="process">The process name to add the time to</param>
        /// <param name="timeToAdd">The amount of time to add in milliseconds</param>
        /// <returns>Returns true if the process was not in the list previously.  False otherwise.</returns>
        public bool AddTime(String process, double timeToAdd) {
            bool wasNew = false;

            if (_appTimes.ContainsKey(process)) {
                ProcessEntry time = _appTimes[process];
                time.Time += timeToAdd;
            } else {
                ProcessEntry newEntry = new ProcessEntry(process, timeToAdd);
                _appTimes[process] = newEntry;
                wasNew = true;
            }

            return wasNew;
        }

        public ProcessEntry GetEntry(String processName) {
            return _appTimes[processName];
        }
    }
}
