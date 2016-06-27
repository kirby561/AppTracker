using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AppTracker {
    /// <summary>
    /// Interaction logic for LabelEditorWindow.xaml
    /// </summary>
    public partial class LabelEditorWindow : Window {
        private LabelManager _labelManager;

        public LabelEditorWindow(LabelManager labelManager) {
            InitializeComponent();

            _labelManager = labelManager;
            _labelListView.ItemsSource = _labelManager.GetLabels();
        }

        private void OnNewClicked(object sender, RoutedEventArgs e) {
            InputWindow input = new InputWindow("Enter a label");
            input.ShowDialog();

            String result = input.Input;
            if (!String.IsNullOrEmpty(result)) {
                // Does it exist?
                if (!_labelManager.LabelExists(result)) {
                    // Add it
                    _labelManager.AddLabel(result);
                } else {
                    // No reason to add it twice
                    MessageBox.Show("That label already exists.");
                }
            }
        }

        private void OnDeleteClicked(object sender, RoutedEventArgs e) {
            List<String> labelsToRemove = new List<String>();
            foreach (String item in _labelListView.SelectedItems) {
                labelsToRemove.Add(item);
            }

            foreach (String item in labelsToRemove) {
                _labelManager.RemoveLabel(item);
            }
        }

        private void OnDoneClicked(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
