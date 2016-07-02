using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AppTracker {
    public class Label {

        public String Name { get; set; }
        public bool IsFiltered { get; set; }

        public Label(String name) {
            Name = name;
            IsFiltered = false;
        }

        public Label(String name, bool isFiltered) {
            Name = name;
            IsFiltered = isFiltered;
        }

        public override int GetHashCode() {
            return Name.GetHashCode();
        }

        public override bool Equals(object otherObj) {
            Label other = otherObj as Label;
            if (other != null)
                return Name.Equals(other.Name);
            return false;
        }

        public override String ToString() {
            return Name;
        }

    }

    public class LabelManager {
        /// <summary>
        /// This keeps track of which labels apply to which process.
        /// </summary>
        private Dictionary<String, HashSet<Label>> _processToLabelsMap = new Dictionary<String, HashSet<Label>>();

        /// <summary>
        /// The label map is a map of label name to the actual label object for efficient lookup.
        /// It should 
        /// </summary>
        private Dictionary<String, Label> _labelMap = new Dictionary<String, Label>();

        /// <summary>
        /// Labels the given process name with the given label.
        /// </summary>
        /// <param name="processName">The process to label</param>
        /// <param name="labelName">The label to use</param>
        /// <returns>Returns true if the process was labeled successfully, false otherwise.</returns>
        public bool Label(String processName, String labelName) {
            // Make sure the label exists
            if (!_labelMap.ContainsKey(labelName))
                return false;

            // Make sure the process has a set of labels associated with it
            if (!_processToLabelsMap.ContainsKey(processName))
                _processToLabelsMap[processName] = new HashSet<Label>();

            // Grab the label and add it
            Label label = _labelMap[labelName];
            _processToLabelsMap[processName].Add(label);

            return true;
        }

        /// <summary>
        /// Removes the given label from the given process name.
        /// </summary>
        /// <param name="processName">The process name</param>
        /// <param name="labelName">The label to remove</param>
        /// <returns>Returns true if the label was successfully removed. False otherwise.</returns>
        public bool Unlabel(String processName, String labelName) {
            // Make sure the label exists
            if (!_labelMap.ContainsKey(labelName))
                return false;

            // Make sure the process exists
            if (!_processToLabelsMap.ContainsKey(processName))
                return false;

            // Grab the label and remove it
            Label label = _labelMap[labelName];
            _processToLabelsMap[processName].Remove(label);

            return true;
        }

        /// <summary>
        /// Checks if the given process name is labeled with the given label.
        /// </summary>
        /// <param name="processName">The process name</param>
        /// <param name="labelName">The label to check</param>
        /// <returns>True if it is labeled, false otherwise.</returns>
        public bool HasLabel(String processName, String labelName) {
            // Check that the label exists
            if (!_labelMap.ContainsKey(labelName))
                return false;

            // Check that the process exists
            if (!_processToLabelsMap.ContainsKey(processName))
                return false;

            Label label = _labelMap[labelName];
            return _processToLabelsMap[processName].Contains(label);
        }

        /// <summary>
        /// Indicates if the label exists or not.
        /// </summary>
        /// <param name="label">The label to check</param>
        /// <returns>True if the label already exists.  False otherwise.</returns>
        public bool LabelExists(String label) {
            return _labelMap.ContainsKey(label);
        }

        /// <summary>
        /// Flags the given label as filtered.
        /// </summary>
        /// <param name="labelName">The label to filter</param>
        /// <returns>True if the label was successfully filtered.  False otherwise.</returns>
        public bool FilterLabel(String labelName) {
            if (_labelMap.ContainsKey(labelName)) {
                Label label = _labelMap[labelName];
                label.IsFiltered = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Flags the given label as unfiltered.
        /// </summary>
        /// <param name="labelName">The label to filter</param>
        /// <returns>Returns true if the label was successfully unfiltered.  False otherwise.</returns>
        public bool UnfilterLabel(String labelName) {
            if (_labelMap.ContainsKey(labelName)) {
                Label label = _labelMap[labelName];
                label.IsFiltered = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Indicates if the process is filtered or not based on its labels.
        /// </summary>
        /// <param name="processName">The name of the process to check</param>
        /// <returns>Returns true if the process should be filtered.  False otherwise.</returns>
        public bool IsFiltered(String processName) {
            bool result = false;
            
            if (_processToLabelsMap.ContainsKey(processName)) {
                HashSet<Label> labels = _processToLabelsMap[processName];
                foreach (Label label in labels) {
                    if (label.IsFiltered) {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the labels that have been created.
        /// </summary>
        /// <returns>A list of the labels</returns>
        public List<Label> GetLabels() {
            return _labelMap.Values.ToList();
        }

        /// <summary>
        /// Adds the given label if it doesnt already exist.
        /// </summary>
        /// <param name="labelName">The label to add</param>
        public void AddLabel(String labelName) {
            if (!_labelMap.ContainsKey(labelName))
                _labelMap.Add(labelName, new Label(labelName));
        }

        /// <summary>
        /// Removes the label from the list.
        /// </summary>
        /// <param name="label">The label to remove</param>
        public void RemoveLabel(String label) {
            _labelMap.Remove(label);
        }

        /// <summary>
        /// Serializes this class to an XML string.
        /// </summary>
        /// <returns>The XML string generated</returns>
        public String ToXml() {
            StringBuilder output = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(output);

            writer.WriteStartDocument();

            // Write a root element
            writer.WriteStartElement("LabelManager");
            {
                // Write the label list first
                writer.WriteStartElement("Labels");
                {
                    foreach (Label label in _labelMap.Values) {
                        writer.WriteStartElement("Label");
                        writer.WriteAttributeString("IsFiltered", label.IsFiltered.ToString());
                        writer.WriteValue(label.Name);
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndElement();

                // Now write the dictionary
                writer.WriteStartElement("Dictionary");
                {
                    foreach (String key in _processToLabelsMap.Keys) {
                        writer.WriteStartElement("Entry");
                        {
                            writer.WriteAttributeString("Key", key);

                            foreach (Label label in _processToLabelsMap[key]) {
                                writer.WriteStartElement("Label");
                                writer.WriteAttributeString("Name", label.Name);
                                writer.WriteEndElement();
                            }
                        }
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.Close();

            return output.ToString();
        }

        public void LoadXml(String xml) {
            StringReader stringReader = new StringReader(xml);
            XmlReader reader = XmlReader.Create(stringReader);

            // Start with the root
            reader.ReadStartElement("LabelManager");
            {
                Console.WriteLine(reader.Name);

                // First read the possible labels
                _labelMap.Clear();
                reader.ReadStartElement("Labels");
                if (reader.Name == "Label") {
                    while (reader.Name == "Label") {
                        String isFiltered = reader.GetAttribute("IsFiltered");
                        String content = reader.ReadElementContentAsString();
                        Console.WriteLine(content);
                        _labelMap.Add(content, new Label(content, bool.Parse(isFiltered)));
                    }

                    reader.ReadEndElement();
                }

                // Now read the dictionary
                reader.ReadStartElement("Dictionary");

                if (reader.Name == "Entry") {
                    while (reader.Name == "Entry") {
                        String key = reader.GetAttribute("Key");
                        reader.Read();

                        while (reader.Name == "Label") {
                            String label = reader.GetAttribute("Name");
                            Label(key, label);
                            
                            reader.Read();
                        }
                    }
                    reader.ReadEndElement();
                }
            }
            reader.ReadEndElement();

            reader.Close();
        }
    }
}
