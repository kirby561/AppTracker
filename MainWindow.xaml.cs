using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AppTracker {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private System.Timers.Timer _checkProcessesTimer;
        private Mutex _timeTrackerLock = new Mutex();
        private TimeTracker _timeTracker = new TimeTracker();
        private LabelManager _labelManager = new LabelManager();
        private double _timePeriod = 5000;
        private List<ProcessEntry> _processList = new List<ProcessEntry>();
        private List<ProcessEntry> _displayedList = new List<ProcessEntry>();
        private String _executablePath;
        private String _executableDirectory;

        // Keep track of the sorted column so we can switch it
        private String _currentSortedColumnName = "Process";
        private bool _sortedColumnIsAscending = true;

        /// <summary>
        /// Initalizes the main window.
        /// </summary>
        public MainWindow() {
            InitializeComponent();
            _executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            _executableDirectory = Directory.GetParent(_executablePath).FullName;

            // Create timer
            _checkProcessesTimer = new System.Timers.Timer();
            _checkProcessesTimer.AutoReset = true;
            _checkProcessesTimer.Interval = _timePeriod;
            _checkProcessesTimer.Elapsed += OnCheckProcessesTimerElapsed;
            _checkProcessesTimer.Start();

            // Set the list item source
            _processListView.ItemsSource = _processList;
            _processListView.SelectionChanged += OnSelectionChanged;

            // Load our existing processes from disk
            InitializeTimeTracker();

            // Hook up column header events for sorting
            InitializeColumnEvents();

            // Load our labels
            LoadLabelMap();

            // Aoply filters, which also sets the list of items to display.
            ApplyFilters();
        }

        /// <summary>
        /// Loads the time tracker data from our data file.
        /// </summary>
        private void InitializeTimeTracker() {
            String csvData = AtomicFileManager.ReadAtomicFile(GetDataFile());
            _timeTracker.LoadCsv(csvData);

            // Load it into the displayed list and sort
            foreach (ProcessEntry entry in _timeTracker.GetDictionary().Values) {
                _processList.Add(entry);
            }

            // Sort it
            SortByColumnHeader(_currentSortedColumnName, _sortedColumnIsAscending);
        }

        /// <summary>
        /// Loads the list of labels and their mappings.
        /// </summary>
        private void LoadLabelMap() {
            String labelXml = null;
            String labelFile = GetLabelMapFile();

            if (File.Exists(labelFile))
                labelXml = File.ReadAllText(labelFile);

            if (labelXml != null)
                _labelManager.LoadXml(labelXml);
        }

        /// <summary>
        /// Sets the click events on all the column headers.
        /// </summary>
        private void InitializeColumnEvents() {
            GridView itemsView = _processListView.View as GridView;
            if (itemsView == null) {
                throw new Exception("Expected a GridView so we can resize the columns.");
            }

            foreach (GridViewColumn column in itemsView.Columns) {
                GridViewColumnHeader header = column.Header as GridViewColumnHeader;
                header.Click += OnColumnHeaderClicked;
            }
        }

        /// <summary>
        /// Sizes the last column to fill the remaining space.
        /// </summary>
        private void SizeLastColumn() {
            GridView itemsView = _processListView.View as GridView;
            if (itemsView == null) {
                throw new Exception("Expected a GridView so we can resize the columns.");
            }

            double widthAvailable = _processListView.ActualWidth;
            GridViewColumnCollection columns = itemsView.Columns;
            int index = 0;
            double widthCount = 0;
            foreach (GridViewColumn column in columns) {
                if (index < columns.Count - 1) {
                    widthCount += column.ActualWidth;
                } else {
                    // Resize the last column to fill the remaining space if 
                    // it doesn't make it 0 size
                    double widthRemaining = widthAvailable - widthCount;
                    if (widthRemaining > 0) {
                        column.Width = widthRemaining;
                    }
                }
                index++;
            }
        }

        /// <summary>
        /// Applies filters to the list
        /// </summary>
        private void ApplyFilters() {
            _displayedList.Clear();
            foreach (ProcessEntry process in _processList) {
                if (!_labelManager.IsFiltered(process.Name))
                    _displayedList.Add(process);
            }
            _processListView.ItemsSource = _displayedList;
            _processListView.Items.Refresh();
        }

        /// <summary>
        /// Sorts the list by the given column.
        /// </summary>
        /// <param name="headerName">The header name of the column to sort on.</param>
        /// <param name="isAscending">True if the order should be ascending, false for descending</param>
        private void SortByColumnHeader(String headerName, bool isAscending) {
            Func<ProcessEntry, object> sortItemAccessor = null;
            if ("Process".Equals(headerName))
                sortItemAccessor = (entry => entry.Name);
            else if ("Time (ms)".Equals(headerName))
                sortItemAccessor = (entry => entry.Time);

            if (headerName.Equals(_currentSortedColumnName) && !isAscending) {
                _processList = _processList.OrderByDescending(sortItemAccessor).ToList();
                _sortedColumnIsAscending = false;
            } else {
                _processList = _processList.OrderBy(sortItemAccessor).ToList();
                _sortedColumnIsAscending = true;
            }
            _currentSortedColumnName = headerName;
        }

        /// <summary>
        /// Returns the directory where 
        /// </summary>
        /// <returns></returns>
        private String GetDataDirectory() {
            String result = _executableDirectory + "/Data";

            // Make sure the directory exists
            try {
                Directory.CreateDirectory(result);
            } catch (IOException exception) { }

            return result;
        }

        /// <summary>
        /// Gets the data file to persist our process information to.
        /// </summary>
        /// <returns>The path to the data file.</returns>
        private String GetDataFile() {
            return GetDataDirectory() + "/ProcessData.csv";
        }

        private String GetLabelMapFile() {
            return GetDataDirectory() + "/LabelMaps.xml";
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (_processListView.SelectedItem != null) {
                ListViewItem lvi = (ListViewItem)_processListView.ItemContainerGenerator.ContainerFromItem(_processListView.SelectedItem);

                // Set the context menu items for this entry
                if (lvi != null) {
                    MenuItem labelsItem = lvi.ContextMenu.Items.GetItemAt(0) as MenuItem;
                    
                    if (labelsItem != null) {
                        if (labelsItem.ItemsSource == null) {
                            labelsItem.Click += OnLabelClicked;
                            labelsItem.ItemContainerGenerator.StatusChanged +=
                                (object statusSender, EventArgs eventArgs) => {
                                    // Wait for containers to be generated
                                    if (labelsItem.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                                        return;

                                    String selectedProcessName = ((ProcessEntry)_processListView.SelectedItem).Name;

                                    // Setup the context menu based on what labels already apply
                                    //    to the selected item.
                                    foreach (Label label in labelsItem.Items) {
                                        MenuItem labelContainer = labelsItem.ItemContainerGenerator.ContainerFromItem(label) as MenuItem;
                                        labelContainer.IsChecked = _labelManager.HasLabel(selectedProcessName, label.Name);
                                    }
                                };
                        }

                        labelsItem.ItemsSource = _labelManager.GetLabels();
                        labelsItem.Items.Refresh();
                    }
                }
            }
        }

        private void OnLabelClicked(object sender, RoutedEventArgs e) {
            MenuItem item = e.OriginalSource as MenuItem;
            if (item != sender) {
                ProcessEntry selectedItem = _processListView.SelectedItem as ProcessEntry;
                Console.WriteLine(selectedItem.Name);
                Label label = item.Header as Label;
                _labelManager.Label(selectedItem.Name, label.Name);
                item.IsChecked = true;
            }
        }

        private void OnCheckProcessesTimerElapsed(object sender, ElapsedEventArgs e) {
            // Take the opportunity to save our label state
            App.Current.Dispatcher.Invoke(() => {
                String xml = _labelManager.ToXml();
                File.WriteAllText(GetLabelMapFile(), xml);
            });

            Process[] processes = Process.GetProcesses();
            bool needsSort = false;

            _timeTrackerLock.WaitOne();
            foreach (Process process in processes) {
                bool wasNew = _timeTracker.AddTime(process.ProcessName, _timePeriod);

                if (wasNew) {
                    _processList.Add(_timeTracker.GetEntry(process.ProcessName));
                    needsSort = true;
                }
            }

            // Persist the tracker to disk.  This could be more efficient
            //    but this will work for now.
            String csvString = _timeTracker.ToCsv();
            _timeTrackerLock.ReleaseMutex();
            
            String dataFile = GetDataFile();
            AtomicFileManager.WriteAtomicFile(dataFile, csvString);

            App.Current.Dispatcher.Invoke(() => {
                if (needsSort)
                    SortByColumnHeader(_currentSortedColumnName, _sortedColumnIsAscending);

                ApplyFilters();
            });
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e) {
            SizeLastColumn();
        }

        private void OnColumnHeaderClicked(object sender, RoutedEventArgs e) {
            GridViewColumnHeader header = sender as GridViewColumnHeader;
            String headerName = header.Content as String;

            SortByColumnHeader(headerName, (!headerName.Equals(_currentSortedColumnName) || !_sortedColumnIsAscending));
            ApplyFilters();
        }

        private void OnProcessListViewSizeChanged(object sender, SizeChangedEventArgs e) {
            SizeLastColumn();
        }

        private void OnMenuExitClicked(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        private void OnMenuExportClicked(object sender, RoutedEventArgs e) {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".csv";
            saveFileDialog.AddExtension = true;
            saveFileDialog.Filter = "Comma-Separated-Value files|*.csv";
            if (saveFileDialog.ShowDialog() == true) {
                _timeTrackerLock.WaitOne();
                String csvText = _timeTracker.ToCsv();
                _timeTrackerLock.ReleaseMutex();

                File.WriteAllText(saveFileDialog.FileName, csvText);
            }
        }

        private void OnMenuLabelsClicked(object sender, RoutedEventArgs e) {
            LabelEditorWindow labelEditor = new LabelEditorWindow(_labelManager);
            labelEditor.ShowDialog();
        }

        private void OnMenuFiltersClicked(object sender, RoutedEventArgs e) {
            FilterEditorWindow filterEditor = new FilterEditorWindow(_labelManager);
            filterEditor.ShowDialog();
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            String xml = _labelManager.ToXml();
            File.WriteAllText(GetLabelMapFile(), xml);
        }
    }
}
