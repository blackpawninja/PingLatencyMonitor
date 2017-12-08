using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using PingLatencyMonitor.Annotations;

namespace PingLatencyMonitor
{
    public class PingReplyWrapper : INotifyPropertyChanged
    {
        private const int LatencyThresholdOk = 249;
        private const int LatencyThresholdWarning = 500;

        public PingReplyWrapper(PingReply reply)
        {
            ReplyData = reply;
        }

        private PingReply _pingReply;
        public PingReply ReplyData
        {
            get { return _pingReply; }
            set
            {
                _pingReply = value;
                OnPropertyChanged();
            }
        }

        public string Latency => IsSuccess ? $"{ReplyData.RoundtripTime}ms" : "LOSS  ";

        public bool IsSuccess => (ReplyData != null) && (ReplyData.Status == IPStatus.Success);

        public SolidColorBrush BorderBrush
        {
            get
            {
                var converter = new BrushConverter();

                if (!IsSuccess)
                    return (SolidColorBrush)converter.ConvertFrom("#D32F2F");

                if (ReplyData.RoundtripTime <= LatencyThresholdOk)
                    return (SolidColorBrush)converter.ConvertFrom("#43A047");

                if (ReplyData.RoundtripTime <= LatencyThresholdWarning)
                    return (SolidColorBrush)converter.ConvertFrom("#FBC02D");


                return (SolidColorBrush)converter.ConvertFrom("#F4511E");
            }
        }

        public SolidColorBrush BackgroundBrush
        {
            get
            {
                var converter = new BrushConverter();

                if (!IsSuccess)
                    return (SolidColorBrush)converter.ConvertFrom("#F44336");

                if (ReplyData.RoundtripTime <= LatencyThresholdOk)
                    return (SolidColorBrush)converter.ConvertFrom("#66BB6A");

                if (ReplyData.RoundtripTime <= LatencyThresholdWarning)
                    return (SolidColorBrush)converter.ConvertFrom("#FFEE58");


                return (SolidColorBrush)converter.ConvertFrom("#FF7043");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
