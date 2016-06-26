using System;
using System.Collections.Generic;
using System.IO;
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

        /// <summary>
        /// Gets the entry for the given process name.
        /// </summary>
        /// <param name="processName">The process name</param>
        /// <returns>The entry for the given process name</returns>
        public ProcessEntry GetEntry(String processName) {
            return _appTimes[processName];
        }

        /// <summary>
        /// Gets the data held by this time tracker as a dictionary.
        /// </summary>
        /// <returns>A dictionary of ProcessEntry's tracked by this TimeTracker</returns>
        public Dictionary<String, ProcessEntry> GetDictionary() {
            return _appTimes;
        }

        /// <summary>
        /// Converts the data in this TimeTracker to a Comma-Separated-Value string
        /// </summary>
        /// <returns>A CSV string with this tracker's data</returns>
        public String ToCsv() {
            String result = "Process Name, Time (ms)\n";

            foreach (ProcessEntry entry in _appTimes.Values) {
                result += entry.Name + "," + entry.Time + "\n";
            }

            return result;
        }

        /// <summary>
        /// Initializes the data in this tracker from the given Comma-Separated-Value string
        /// </summary>
        /// <param name="csvString">The string to parse</param>
        public void LoadCsv(String csvString) {
            if (csvString == null) {
                // Nothing to read
                return;
            }

            StringReader reader = new StringReader(csvString);
            String columnNames = reader.ReadLine();
            String nextLine = reader.ReadLine();

            while (nextLine != null) {
                String[] fields = nextLine.Split(',');
                String processName = fields[0];
                String timeString = fields[1];
                double time = -1;

                if (!Double.TryParse(timeString, out time)) {
                    Console.WriteLine("Invalid time for process " + processName + ": " + timeString);
                } else {
                    _appTimes.Add(processName, new ProcessEntry(processName, time));
                }

                nextLine = reader.ReadLine();
            }
        }
    }
}
