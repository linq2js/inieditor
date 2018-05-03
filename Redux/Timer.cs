using System;
using System.Threading;
using System.Threading.Tasks;

namespace Redux
{
    public class Timer : IDisposable
    {
        private bool _stopped;

        private Timer(CancellationTokenSource token)
        {
            Token = token;
        }


        public CancellationTokenSource Token { get; }

        public void Stop()
        {
            if (_stopped || Token.IsCancellationRequested) return;
            _stopped = true;
            Token.Cancel();
        }

        public static Timer Timeout(int milliseconds, Action action)
        {
            return Timeout(milliseconds, () =>
            {
                action();
                return false;
            });
        }


        public static Timer Timeout(int milliseconds, Func<bool> action)
        {
            var ts = new CancellationTokenSource();
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    if (ts.IsCancellationRequested)
                    {
                        break;
                    }

                    await Task.Delay(milliseconds, ts.Token);
                    if (!action())
                    {
                        break;
                    }
                }
            }, ts.Token);

            return new Timer(ts);
        }

        public static Timer Interval(int milliseconds, Action action)
        {
            return Timeout(milliseconds, () =>
            {
                action();
                return true;
            });
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
