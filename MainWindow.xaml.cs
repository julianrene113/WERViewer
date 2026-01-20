using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WERViewer
{
    /// <summary>
    /// UI for Windows Error Reporting 
    /// </summary>
    public partial class MainWindow : Window
    {
        #region [Properties]
        double _windowLeft = 0;
        double _windowTop = 0;
        double _windowWidth = 0;
        double _windowHeight = 0;
        string _reportPath = "";
        #endregion
        
        readonly MainViewModel _vm = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = _vm; // ⇦ context must be set for INotifyPropertyChanged
            Debug.WriteLine($"[INFO] Application version {App.GetCurrentAssemblyVersion()}");

            var cvrt = 67246336;
            Debug.WriteLine($"[INFO] {cvrt:X8}");
        }

        #region [Events]
        /// <summary>
        /// <see cref="System.Windows.Window"/> event
        /// </summary>
        async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = $"WER Viewer - v{App.GetCurrentAssemblyVersion()}";

            string progDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            Debug.WriteLine($"[INFO] User's CommonApplicationData location is \"{progDataPath}\"");
            _windowTop = ConfigManager.Get("WindowTop", defaultValue: 200d);
            _windowLeft = ConfigManager.Get("WindowLeft", defaultValue: 250d);
            _windowWidth = ConfigManager.Get("WindowWidth", defaultValue: 1200d);
            _windowHeight = ConfigManager.Get("WindowHeight", defaultValue: 800d);
            _reportPath = ConfigManager.Get("ReportPath", defaultValue: System.IO.Path.Combine(progDataPath, @"Microsoft\Windows\WER\ReportArchive"));
            _vm.Status = $"🔔 Restoring window position";

            // Check if position is on any screen
            this.RestorePosition(_windowLeft, _windowTop, _windowWidth, _windowHeight);

            btnRefresh.IsEnabled = false;
            spProgress.Visibility = Visibility.Visible;
            await _vm.LoadAsync(_reportPath);
            await Dispatcher.InvokeAsync(async () =>
            {
                await Task.Delay(1000); // prevent spamming
                btnRefresh.IsEnabled = true;
                spProgress.Visibility = Visibility.Hidden;
                if (_vm.GetCount() == 0)
                {
                    _vm.Status = $"⚠ No WER reports found{Environment.NewLine}{_reportPath}";
                    imgEmpty.Visibility = Visibility.Visible;
                }
                else
                {
                    _vm.Status = $"{_vm.GetCount()} WER reports found{Environment.NewLine}{_reportPath}";
                    imgEmpty.Visibility = Visibility.Hidden;
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
            
        }

        /// <summary>
        /// <see cref="System.Windows.Window"/> event
        /// </summary>
        void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.IsInvalidOrZero())
                return;
            Debug.WriteLine($"[INFO] New size: {e.NewSize.Width:N0},{e.NewSize.Height:N0}");
        }

        /// <summary>
        /// <see cref="System.Windows.Window"/> event
        /// </summary>
        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ConfigManager.Set("ReportPath", value: _reportPath);
            ConfigManager.Set("WindowTop", value: this.Top.IsInvalid() ? 200d : this.Top);
            ConfigManager.Set("WindowLeft", value: this.Left.IsInvalid() ? 250d : this.Left);
            if (!this.Width.IsInvalid() && this.Width >= 800) { ConfigManager.Set("WindowWidth", value: this.Width); }
            else { ConfigManager.Set("WindowWidth", value: 1200); } // restore default
            if (!this.Height.IsInvalid() && this.Height >= 600) { ConfigManager.Set("WindowHeight", value: this.Height); }
            else { ConfigManager.Set("WindowHeight", value: 800); } // restore default
            _vm?.Cancel(); // Signal any loops/timers that it's time to shut it down.
        }

        /// <summary>
        /// <see cref="System.Windows.Window"/> event
        /// </summary>
        void Window_Activated(object sender, EventArgs e) => _vm.IsActivated = true;

        /// <summary>
        /// <see cref="System.Windows.Window"/> event
        /// </summary>
        void Window_Deactivated(object sender, EventArgs e) => _vm.IsActivated = false;

        /// <summary>
        /// <see cref="System.Windows.Controls.Button"/> event
        /// </summary>
        async void Button_Click(object sender, RoutedEventArgs e)
        {
            _vm.Status = $"Fetching reports…";
            btnRefresh.IsEnabled = false;
            spProgress.Visibility = Visibility.Visible;
            await _vm.LoadAsync(_reportPath);
            await Dispatcher.InvokeAsync(async () =>
            {
                await Task.Delay(1000); // prevent spamming
                btnRefresh.IsEnabled = true;
                spProgress.Visibility = Visibility.Hidden;
                if (_vm.GetCount() == 0)
                {
                    _vm.Status = $"⚠ No WER reports found{Environment.NewLine}{_reportPath}";
                    imgEmpty.Visibility = Visibility.Visible;
                }
                else
                {
                    _vm.Status = $"{_vm.GetCount()} WER reports found{Environment.NewLine}{_reportPath}";
                    imgEmpty.Visibility = Visibility.Hidden;
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
        }
        #endregion
    }
}
