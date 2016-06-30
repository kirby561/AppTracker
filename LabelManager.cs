using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
                    foreach (String label in _possibleLabels) {
                        writer.WriteStartElement("Label");
                        writer.WriteValue(label);
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndElement();

                // Now write the dictionary
                writer.WriteStartElement("Dictionary");
                {
                    foreach (String key in _labelMap.Keys) {
                        writer.WriteStartElement("Entry");
                        {
                            writer.WriteAttributeString("Key", key);

                            foreach (String label in _labelMap[key]) {
                                writer.WriteStartElement("Label");
                                writer.WriteAttributeString("Name", label);
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
            // ?? TODO
            StringReader stringReader = new StringReader(xml);
            XmlReader reader = XmlReader.Create(stringReader);

            // Start with the root
            reader.ReadStartElement("LabelManager");
            {
                Console.WriteLine(reader.Name);

                // First read the possible labels
                _possibleLabels.Clear();
                reader.ReadStartElement("Labels");
                {
                    while (reader.Name == "Label") {
                        String content = reader.ReadElementContentAsString();
                        Console.WriteLine(content);
                        _possibleLabels.Add(content);
                    }
                }
                reader.ReadEndElement();

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
