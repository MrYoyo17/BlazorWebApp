using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TestBlazor.Client.Services
{
    public class StopwatchService
    {
        private readonly Stopwatch _stopwatch = new();
        private Timer? _timer;

        public TimeSpan Elapsed => _stopwatch.Elapsed;
        public bool IsRunning => _stopwatch.IsRunning;

        public event Action? OnTick;

        public void Start()
        {
            if (!_stopwatch.IsRunning)
            {
                _stopwatch.Start();
                // Trigger UI updates every 50ms for a smooth display
                _timer = new Timer(_ => 
                {
                    OnTick?.Invoke();
                }, null, 0, 50);
            }
        }

        public void Stop()
        {
            _stopwatch.Stop();
            _timer?.Dispose();
            _timer = null;
            OnTick?.Invoke();
        }

        public void Reset()
        {
            _stopwatch.Reset();
            _timer?.Dispose();
            _timer = null;
            OnTick?.Invoke();
        }
    }
}
