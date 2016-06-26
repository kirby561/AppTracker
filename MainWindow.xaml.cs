﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
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
        private Timer _checkProcessesTimer;
        private TimeTracker _timeTracker = new TimeTracker();
        private double _timePeriod = 5000;
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
            _checkProcessesTimer = new Timer();
            _checkProcessesTimer.AutoReset = true;
            _checkProcessesTimer.Interval = _timePeriod;
            _checkProcessesTimer.Elapsed += OnCheckProcessesTimerElapsed;
            _checkProcessesTimer.Start();

            // Set the list item source
            _processListView.ItemsSource = _displayedList;

            // Load our existing processes from disk
            InitializeTimeTracker();

            // Hook up column header events for sorting
            InitializeColumnEvents();
        }

        /// <summary>
        /// Loads the time tracker data from our data file.
        /// </summary>
        private void InitializeTimeTracker() {
            String csvData = AtomicFileManager.ReadAtomicFile(GetDataFile());
            _timeTracker.LoadCsv(csvData);

            // Load it into the displayed list and sort
            foreach (ProcessEntry entry in _timeTracker.GetDictionary().Values) {
                _displayedList.Add(entry);
            }

            // Sort it
            SortByColumnHeader(_currentSortedColumnName, _sortedColumnIsAscending);
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
                _displayedList = _displayedList.OrderByDescending(sortItemAccessor).ToList();
                _sortedColumnIsAscending = false;
            } else {
                _displayedList = _displayedList.OrderBy(sortItemAccessor).ToList();
                _sortedColumnIsAscending = true;
            }
            _processListView.ItemsSource = _displayedList;
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

        private void OnCheckProcessesTimerElapsed(object sender, ElapsedEventArgs e) {
            Process[] processes = Process.GetProcesses();
            bool needsSort = false;

            foreach (Process process in processes) {
                bool wasNew = _timeTracker.AddTime(process.ProcessName, _timePeriod);

                if (wasNew) {
                    _displayedList.Add(_timeTracker.GetEntry(process.ProcessName));
                    needsSort = true;
                }
            }

            // Persist the tracker to disk.  This could be more efficient
            //    but this will work for now.
            String csvString = _timeTracker.ToCsv();
            String dataFile = GetDataFile();
            AtomicFileManager.WriteAtomicFile(dataFile, csvString);

            App.Current.Dispatcher.Invoke(() => {
                if (needsSort)
                    SortByColumnHeader(_currentSortedColumnName, _sortedColumnIsAscending);

                _processListView.Items.Refresh();
            });
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e) {
            SizeLastColumn();
        }

        private void OnColumnHeaderClicked(object sender, RoutedEventArgs e) {
            GridViewColumnHeader header = sender as GridViewColumnHeader;
            String headerName = header.Content as String;

            SortByColumnHeader(headerName, (!headerName.Equals(_currentSortedColumnName) || !_sortedColumnIsAscending));
        }

        private void OnProcessListViewSizeChanged(object sender, SizeChangedEventArgs e) {
            SizeLastColumn();
        }
    }
}
