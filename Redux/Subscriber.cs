using System;
using System.Linq;

namespace Redux
{
    public class Subscriber<TModel>
    {
        public Subscriber(Delegate handler, Delegate[] selectors)
        {
            Handler = handler;
            Selectors = selectors;
            Target = handler.Target;
        }

        public Delegate Handler { get; }

        public Delegate[] Selectors { get; }

        public object Target { get; }

        public object[] LastValues { get; private set; }

        public void Invoke(TModel model)
        {
            var newValues = Selectors.Select(x => x.DynamicInvoke(model)).ToArray();
            if (LastValues != null && LastValues.SequenceEqual(newValues)) return;
            Handler.DynamicInvoke(newValues);
            LastValues = newValues;
        }
    }
}
