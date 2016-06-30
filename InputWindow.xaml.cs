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
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class InputWindow : Window {
        /// <summary>
        /// This is the result of the input window.
        /// It will be null if the user didn't enter anything or canceled.
        /// Otherwise it will be set to what they entered.
        /// </summary>
        public String Input { get; private set; }

        public InputWindow(String prompt) {
            InitializeComponent();

            _promptTextBlock.Text = prompt;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e) {
            double height = 0;
            
            foreach (UIElement child in _grid.Children) {
                FrameworkElement element = child as FrameworkElement;
                if (element != null) {
                    height += element.ActualHeight;
                }
            }

            Height = height;

            // Focus the input box
            _inputBox.Focus();
        }

        private void OnOkayClicked(object sender, RoutedEventArgs e) {
            Input = _inputBox.Text;
            Close();
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e) {
            Input = null;
            Close();
        }

        private void OnInputBoxKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                OnOkayClicked(sender, null);
            }
        }
    }
}
