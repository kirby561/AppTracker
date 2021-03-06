﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AppTracker {
    /// <summary>
    /// Interaction logic for FilterEditorWindow.xaml
    /// </summary>
    public partial class FilterEditorWindow : Window {
        private LabelManager _labelManager;
        private List<Label> _labels;

        public FilterEditorWindow(LabelManager labelManager) {
            InitializeComponent();

            _labelManager = labelManager;
            _labels = _labelManager.GetLabels();
            _labelListView.ItemsSource = _labels;
            _labelListView.ItemContainerGenerator.StatusChanged += OnItemGeneratorStatusChanged;
        }

        /// <summary>
        /// Saves the filters that are selected.
        /// </summary>
        private void SaveFilters() {
            // Filter everything for the moment
            foreach (Label label in _labels) {
                _labelManager.FilterLabel(label.Name);
            }

            // Unfilter whatever is selected in the list
            foreach (Label label in _labelListView.SelectedItems) {
                if (label != null) {
                    _labelManager.UnfilterLabel(label.Name);
                } else {
                    Console.WriteLine("Item is not a label");
                }
            }
        }

        private void OnItemGeneratorStatusChanged(object sender, EventArgs e) {
            if (_labelListView.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated) {
                UpdateLabelList();
            }
        }

        /// <summary>
        /// Synchronizes the selection state of the list with whether or not 
        /// 
        /// </summary>
        private void UpdateLabelList() {
            foreach (Label label in _labels) {
                // Get the actual list view item for this
                ListViewItem item = _labelListView.ItemContainerGenerator.ContainerFromItem(label) as ListViewItem;
                if (item != null) {
                    item.IsSelected = !label.IsFiltered;
                } else {
                    // What?
                    Console.WriteLine("Container is not a ListViewItem for label " + label.ToString());
                }
            }
        }

        private void OnSaveClicked(object sender, RoutedEventArgs e) {
            // Save the filters they selected
            SaveFilters();

            Close();
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e) {
            Close();
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e) {
            _labelListView.Focus();
        }
    }
}
