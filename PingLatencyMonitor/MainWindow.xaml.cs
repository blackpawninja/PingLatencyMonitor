using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using PingLatencyMonitor.Annotations;
using System.Net.NetworkInformation;

namespace PingLatencyMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private const int MaxReply = 20;
        private const int PingInterval = 500;
        
        public MainWindow()
        {
            InitializeComponent();

            PingTarget = "8.8.8.8";
            PingReplies = new ObservableCollection<PingReplyWrapper>();
            LatencyAverage = 0;
            LossPercentage = 0;
            IsRunning = false;

            _ping = new Ping();
            _ping.PingCompleted += OnPingCompleted;
        }

        private void OnPingCompleted(object sender, PingCompletedEventArgs e)
        {
            if (PingReplies.Count() > MaxReply) Dispatcher.Invoke(() => PingReplies.RemoveAt(PingReplies.Count - 1));

            LastReply = new PingReplyWrapper(e.Reply);
            Dispatcher.Invoke(() => PingReplies.Insert(0, LastReply));
            
            CalculateAverage();
            CalculatePacketLoss();

            Task.Delay(PingInterval).ContinueWith(_ => { if (IsRunning) StartPing(); });
        }

        private void StartPing()
        {
            IsRunning = true;

            try
            {
                _ping.SendAsync(PingTarget, null);
            }
            catch (InvalidOperationException)
            {
                // safe exception
            }
            catch (Exception)
            {
                IsRunning = false;
            }
        }

        private void StopPing()
        {
            IsRunning = false;
            _ping.SendAsyncCancel();
        }

        private void CalculateAverage()
        {
            LatencyAverage = Convert.ToInt16(PingReplies.Average(r => (r.IsSuccess) ? r.ReplyData.RoundtripTime : 0));
        }

        private void CalculatePacketLoss()
        {
            var lossCount = (double)PingReplies.Count(r => !r.IsSuccess);
            if (lossCount < 1)
            {
                LossPercentage = 0;
                return;
            }

            var allCount = (double)PingReplies.Count;
            
            LossPercentage = (lossCount / allCount) * 100;
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly Ping _ping;

        private bool _isRunning;
        public Boolean IsRunning
        {
            get { return _isRunning; }
            set
            {
                _isRunning = value;
                OnPropertyChanged(nameof(IsNotRunning));
                OnPropertyChanged(nameof(ButtonText));
            }
        }

        public bool IsNotRunning => !IsRunning;

        public string ButtonText => IsRunning ? "STOP" : "START";

        private string _pingTarget;
        public string PingTarget
        {
            get { return _pingTarget; }
            set
            {
                _pingTarget = value;
                OnPropertyChanged();
            }
        }

        private PingReplyWrapper _lastReply;
        public PingReplyWrapper LastReply
        {
            get { return _lastReply; }
            set
            {
                _lastReply = value;
                OnPropertyChanged();
            }
        }
        
        private ObservableCollection<PingReplyWrapper> _pingReplies;
        public ObservableCollection<PingReplyWrapper> PingReplies
        {
            get { return _pingReplies; }
            set
            {
                _pingReplies = value;
                OnPropertyChanged();
            }
        }

        private int _latencyAverage;
        public int LatencyAverage
        {
            get { return _latencyAverage; }
            set
            {
                _latencyAverage = value;
                OnPropertyChanged();
            }
        }

        private double _lossPercentage;
        public double LossPercentage
        {
            get { return _lossPercentage; }
            set
            {
                _lossPercentage = value;
                OnPropertyChanged();
            }
        }

        private void Ping_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsRunning || !string.IsNullOrEmpty(PingTarget);
        }

        private void Ping_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (IsRunning) StopPing();
            else StartPing();
        }
    }
}
