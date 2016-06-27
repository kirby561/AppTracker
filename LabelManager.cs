using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTracker {
    public class LabelManager {
        private Dictionary<String, HashSet<String>> _labelMap = new Dictionary<String, HashSet<String>>();
        private ObservableCollection<String> _possibleLabels = new ObservableCollection<String>();

        /// <summary>
        /// Labels the given process name with the given label.
        /// </summary>
        /// <param name="processName">The process to label</param>
        /// <param name="label">The label to use</param>
        public void Label(String processName, String label) {
            if (!_labelMap.ContainsKey(processName))
                _labelMap[processName] = new HashSet<String>();
            _labelMap[processName].Add(label);
        }

        /// <summary>
        /// Removes the given label from the given process name.
        /// </summary>
        /// <param name="processName">The process name</param>
        /// <param name="label">The label to remove</param>
        public void Unlabel(String processName, String label) {
            if (!_labelMap.ContainsKey(processName))
                return;
            _labelMap[processName].Remove(label);
        }

        /// <summary>
        /// Checks if the given process name is labeled with the given label.
        /// </summary>
        /// <param name="processName">The process name</param>
        /// <param name="label">The label to check</param>
        /// <returns>True if it is labeled, false otherwise.</returns>
        public bool HasLabel(String processName, String label) {
            if (!_labelMap.ContainsKey(processName))
                return false;
            return _labelMap[processName].Contains(label);
        }

        /// <summary>
        /// Indicates if the label exists or not.
        /// </summary>
        /// <param name="label">The label to check</param>
        /// <returns>True if the label already exists.  False otherwise.</returns>
        public bool LabelExists(String label) {
            return _possibleLabels.Contains(label);
        }

        /// <summary>
        /// Gets the labels that have been created.
        /// </summary>
        /// <returns>An observable collection of the labels</returns>
        public ObservableCollection<String> GetLabels() {
            return _possibleLabels;
        }

        /// <summary>
        /// Adds the given label if it doesnt already exist.
        /// </summary>
        /// <param name="label">The label to add</param>
        public void AddLabel(String label) {
            if (!_possibleLabels.Contains(label))
                _possibleLabels.Add(label);
        }

        /// <summary>
        /// Removes the label from the list.
        /// </summary>
        /// <param name="label">The label to remove</param>
        public void RemoveLabel(String label) {
            _possibleLabels.Remove(label);
        }
    }
}
