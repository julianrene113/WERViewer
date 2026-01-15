using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WERViewer
{
    public class MainViewModel : INotifyPropertyChanged
    {
        CancellationTokenSource _cts;
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName)) { return; }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ObservableCollection<WerReport> Reports { get; } = new ObservableCollection<WerReport>();

        readonly WerReportService _service = new WerReportService();

        bool isActivated = false;
        public bool IsActivated
        {
            get => isActivated;
            set
            {
                if (isActivated != value)
                {
                    isActivated = value;
                    OnPropertyChanged();
                }
            }
        }

        string status = "";
        public string Status
        {
            get => status;
            set
            {
                if (status != value)
                {
                    status = value;
                    OnPropertyChanged();
                }
            }
        }

        WerReport selectedReport;
        public WerReport SelectedReport
        {
            get => selectedReport;
            set
            {
                if (selectedReport != value)
                {
                    selectedReport = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainViewModel()
        {
            _cts = new CancellationTokenSource();
        }

        public void Cancel() => _cts?.Cancel();

        public int GetCount() => Reports.Count;

        public async Task LoadAsync()
        {
            Reports.Clear();
            var items = await _service.LoadReportsAsync();
            foreach (var r in items)
            {
                if (_cts.IsCancellationRequested)
                    break;

                Reports.Add(r);
            }
        }

    }

}
