using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        public static Timer Timeout(Action action)
        {
            return Timeout(0, action);
        }

        public static Timer Timeout(int milliseconds, Action action)
        {
            return Timeout(milliseconds, () =>
            {
                action();
                return false;
            });
        }


        public static Timer Timeout(Func<bool> action)
        {
            return Timeout(0, action);
        }

        public static Timer Timeout(int milliseconds, Func<bool> action)
        {
            var ts = new CancellationTokenSource();
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
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
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        throw;
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

        public static Action Throttle(int delay, Action action, Control controlToInvoke = null)
        {
            Guid token;
            return delegate
            {
                token = new Guid();
                Task.Factory.StartNew(async delegate
                {
                    var tempToken = token;
                    await Task.Delay(delay);
                    if (tempToken != token) return;
                    if (controlToInvoke == null)
                    {
                        action();
                    }
                    else
                    {
                        controlToInvoke.Invoke(action);
                    }
                });
            };
        }
    }
}
