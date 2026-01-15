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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region [Properties]
        double _windowLeft = 0;
        double _windowTop = 0;
        double _windowWidth = 0;
        double _windowHeight = 0;

        CancellationTokenSource _cts = new CancellationTokenSource();
        #endregion

        readonly MainViewModel _vm = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            //this.DataContext = this; // ⇦ very important for INotifyPropertyChanged!
            this.DataContext = _vm; // ⇦ very important for INotifyPropertyChanged!
            Debug.WriteLine($"[INFO] Application version {App.GetCurrentAssemblyVersion()}");
           
        }

        async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = $"WER Viewer - v{App.GetCurrentAssemblyVersion()}";
            _windowTop = ConfigManager.Get("WindowTop", defaultValue: 200d);
            _windowLeft = ConfigManager.Get("WindowLeft", defaultValue: 250d);
            _windowWidth = ConfigManager.Get("WindowWidth", defaultValue: 1000d);
            _windowHeight = ConfigManager.Get("WindowHeight", defaultValue: 800d);

            _vm.Status = $"🔔 Restoring window position";

            // Check if position is on any screen
            this.RestorePosition(_windowLeft, _windowTop, _windowWidth, _windowHeight);

            #region [Async UI pattern]
            //var url = await _weatherService.GetForecastUrlAsync(_latitude, _longitude);
            //if (url != null)
            //{
            //    var wind = await LoadWindSpeed(url.WeeklyForecast);
            //    await Dispatcher.InvokeAsync(() =>
            //    {
            //        msgBar.BarText = $"🔔 Setting background wind speed to {wind}";
            //        spProgress.Visibility = Visibility.Visible;
            //    }, System.Windows.Threading.DispatcherPriority.Background);
            //}
            #endregion

            btnRefresh.IsEnabled = false;
            spProgress.Visibility = Visibility.Visible;
            await _vm.LoadAsync();
            await Dispatcher.InvokeAsync(async () =>
            {
                await Task.Delay(800);
                btnRefresh.IsEnabled = true;
                spProgress.Visibility = Visibility.Hidden;
                if (_vm.GetCount() == 0)
                {
                    _vm.Status = "⚠ No WER reports found.";
                    imgEmpty.Visibility = Visibility.Visible;
                }
                else
                {
                    _vm.Status = $"{_vm.GetCount()} WER reports found";
                    imgEmpty.Visibility = Visibility.Hidden;
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
            
        }

        void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.IsInvalidOrZero())
                return;

            Debug.WriteLine($"[INFO] New size: {e.NewSize.Width:N0},{e.NewSize.Height:N0}");

            // Add in some margin
            //spBackground.Width = e.NewSize.Width - 10;
            //spBackground.Height = e.NewSize.Height - 10;
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ConfigManager.Set("WindowTop", value: this.Top.IsInvalid() ? 200d : this.Top);
            ConfigManager.Set("WindowLeft", value: this.Left.IsInvalid() ? 250d : this.Left);
            if (!this.Width.IsInvalid() && this.Width >= 800) { ConfigManager.Set("WindowWidth", value: this.Width); }
            else { ConfigManager.Set("WindowWidth", value: 1000); } // restore default
            if (!this.Height.IsInvalid() && this.Height >= 600) { ConfigManager.Set("WindowHeight", value: this.Height); }
            else { ConfigManager.Set("WindowHeight", value: 800); } // restore default
            _cts?.Cancel(); // Signal any loops/timers that it's time to shut it down.
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
            btnRefresh.IsEnabled = false;
            spProgress.Visibility = Visibility.Visible;
            await _vm.LoadAsync();
            await Dispatcher.InvokeAsync(async () =>
            {
                await Task.Delay(800);
                btnRefresh.IsEnabled = true;
                spProgress.Visibility = Visibility.Hidden;
                if (_vm.GetCount() == 0)
                {
                    _vm.Status = $"⚠ No WER reports found";
                    imgEmpty.Visibility = Visibility.Visible;
                }
                else
                {
                    _vm.Status = $"{_vm.GetCount()} WER reports found";
                    imgEmpty.Visibility = Visibility.Hidden;
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
        }
    }
}
