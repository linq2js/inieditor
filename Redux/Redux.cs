using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Redux
{
    public class Redux<TModel>
    {
        public Redux(TModel model)
        {
            Model = model;
        }

        public TModel Model { get; }

        private IList<Subscriber<TModel>> _subscribers = new List<Subscriber<TModel>>();
        private readonly HashSet<Control> _views = new HashSet<Control>();

        public void Subscribe(Control view)
        {
            _views.Add(view);
        }

        public void Unsubscribe(Control view)
        {
            _views.Remove(view);
            _subscribers = _subscribers.Where(x => x.Target != view).ToList();
        }

        private Action Subscribe(Delegate handler, Delegate[] selectors)
        {
            var subscriber = new Subscriber<TModel>(handler, selectors);
            _subscribers.Add(subscriber);
            //Invoke(new[] {subscriber});

            return delegate
            {
                _subscribers.Remove(subscriber);
            };
        }

        public Action Subscribe<T1>(
            Func<TModel, T1> s1,
            Action<T1> action)
        {
            return Subscribe(action, new Delegate[] {s1});
        }

        public Action Subscribe<T1, T2>(
            Func<TModel, T1> s1,
            Func<TModel, T2> s2,
            Action<T1, T2> action)
        {
            return Subscribe(action, new Delegate[] {s1, s2});
        }

        public Action Subscribe<T1, T2, T3>(
            Func<TModel, T1> s1,
            Func<TModel, T2> s2,
            Func<TModel, T3> s3,
            Action<T1, T2, T3> action)
        {
            return Subscribe(action, new Delegate[] { s1, s2, s3 });
        }

        public Action Subscribe<T1, T2, T3, T4>(
            Func<TModel, T1> s1,
            Func<TModel, T2> s2,
            Func<TModel, T3> s3,
            Func<TModel, T4> s4,
            Action<T1, T2, T3, T4> action)
        {
            return Subscribe(action, new Delegate[] { s1, s2, s3, s4 });
        }

        public Action Subscribe<T1, T2, T3, T4, T5>(
            Func<TModel, T1> s1,
            Func<TModel, T2> s2,
            Func<TModel, T3> s3,
            Func<TModel, T4> s4,
            Func<TModel, T5> s5,
            Action<T1, T2, T3, T4, T5> action)
        {
            return Subscribe(action, new Delegate[] { s1, s2, s3, s4, s5 });
        }

        public Action Subscribe<T1, T2, T3, T4, T5, T6>(
            Func<TModel, T1> s1,
            Func<TModel, T2> s2,
            Func<TModel, T3> s3,
            Func<TModel, T4> s4,
            Func<TModel, T5> s5,
            Func<TModel, T6> s6,
            Action<T1, T2, T3, T4, T5, T6> action)
        {
            return Subscribe(action, new Delegate[] { s1, s2, s3, s4, s5, s6 });
        }

        public Action Subscribe<T1, T2, T3, T4, T5, T6, T7>(
            Func<TModel, T1> s1,
            Func<TModel, T2> s2,
            Func<TModel, T3> s3,
            Func<TModel, T4> s4,
            Func<TModel, T5> s5,
            Func<TModel, T6> s6,
            Func<TModel, T7> s7,
            Action<T1, T2, T3, T4, T5, T6, T7> action)
        {
            return Subscribe(action, new Delegate[] { s1, s2, s3, s4, s5, s6, s7 });
        }

        public Action Subscribe<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Func<TModel, T1> s1,
            Func<TModel, T2> s2,
            Func<TModel, T3> s3,
            Func<TModel, T4> s4,
            Func<TModel, T5> s5,
            Func<TModel, T6> s6,
            Func<TModel, T7> s7,
            Func<TModel, T8> s8,
            Func<TModel, T9> s9,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action)
        {
            return Subscribe(action, new Delegate[] { s1, s2, s3, s4, s5, s6, s7, s8, s9 });
        }

        public Action Subscribe<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<TModel, T1> s1,
            Func<TModel, T2> s2,
            Func<TModel, T3> s3,
            Func<TModel, T4> s4,
            Func<TModel, T5> s5,
            Func<TModel, T6> s6,
            Func<TModel, T7> s7,
            Func<TModel, T8> s8,
            Action<T1, T2, T3, T4, T5, T6, T7, T8> action)
        {
            return Subscribe(action, new Delegate[] { s1, s2, s3, s4, s5, s6, s7, s8 });
        }

        private Timer _updateTimer;

        public void Update()
        {
            Invoke(_subscribers);
        }

        public void Update(Action<TModel> action)
        {
            action(Model);
            Update();
        }

        public T Update<T>(Func<TModel, T> action)
        {
            var result = action(Model);
            Update();
            return result;
        }

        public async void Update(Func<TModel, Task> action)
        {
            await action(Model);
            Update();
        }

        private void Invoke(IEnumerable<Subscriber<TModel>> subscribers)
        {
            Action<Delegate> invoker = null;
            
            foreach (var view in _views)
            {
                if (invoker == null)
                {
                    invoker = x => view.Invoke(x);
                }
                else
                {
                    var prev = invoker;

                    invoker = x =>
                    {
                        prev.DynamicInvoke(x);
                    };
                }
            }

            void InternalInvoke()
            {
                foreach (var subscriber in subscribers)
                {
                    subscriber.Invoke(Model);
                }
            }

            if (invoker == null)
            {
                InternalInvoke();
            }
            else
            {
                invoker.Invoke((Action) InternalInvoke);
            }
            
        }
    }
}
