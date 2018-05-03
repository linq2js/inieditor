using System;

namespace Redux
{
    public class SafeInvoker
    {
        private readonly Action _action;
        private readonly SafeInvoker _next;

        public SafeInvoker(Action action, SafeInvoker next)
        {
            _action = action;
            _next = next;
        }

        public void Invoke()
        {
            if (_next != null)
            {
                _next.Invoke();
            }
            else
            {
                _action();
            }
        }
    }
}
