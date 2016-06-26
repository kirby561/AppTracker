using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        // Keep track of the sorted column so we can switch it
        private String _currentSortedColumnName = null;
        private bool _sortedColumnIsAscending = true;

        public MainWindow() {
            InitializeComponent();

            // Create timer
            _checkProcessesTimer = new Timer();
            _checkProcessesTimer.AutoReset = true;
            _checkProcessesTimer.Interval = _timePeriod;
            _checkProcessesTimer.Elapsed += OnCheckProcessesTimerElapsed;
            _checkProcessesTimer.Start();

            // Set the list item source
            _processListView.ItemsSource = _displayedList;

            // Hook up column header events for sorting
            InitializeColumnEvents();
        }

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

        private void OnCheckProcessesTimerElapsed(object sender, ElapsedEventArgs e) {
            Process[] processes = Process.GetProcesses();

            foreach (Process process in processes) {
                bool wasNew = _timeTracker.AddTime(process.ProcessName, _timePeriod);

                if (wasNew) {
                    _displayedList.Add(_timeTracker.GetEntry(process.ProcessName));
                }
            }

            App.Current.Dispatcher.Invoke(() => {
                _processListView.Items.Refresh();
            });
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e) {
            SizeLastColumn();
        }

        private void OnColumnHeaderClicked(object sender, RoutedEventArgs e) {
            GridViewColumnHeader header = sender as GridViewColumnHeader;
            String headerName = header.Content as String;

            Func<ProcessEntry, object> sortItemAccessor = null;
            if ("Process".Equals(headerName))
                sortItemAccessor = (entry => entry.Name);
            else if ("Time (Hours)".Equals(headerName))
                sortItemAccessor = (entry => entry.Time);

            if (headerName.Equals(_currentSortedColumnName) && _sortedColumnIsAscending) {
                _displayedList = _displayedList.OrderByDescending(sortItemAccessor).ToList();
                _sortedColumnIsAscending = false;
            } else {
                _displayedList = _displayedList.OrderBy(sortItemAccessor).ToList();
                _sortedColumnIsAscending = true;
            }
            _processListView.ItemsSource = _displayedList;
            _currentSortedColumnName = headerName;
        }

        private void OnProcessListViewSizeChanged(object sender, SizeChangedEventArgs e) {
            SizeLastColumn();
        }
    }
}
