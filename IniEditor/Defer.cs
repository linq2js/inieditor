using System;
using System.Collections.Generic;
using System.Linq;

namespace IniEditor
{
    public interface IPromise
    {
        /// <summary>
        /// Attaches a callback to be executed when the Deferred is resolved
        /// </summary>
        IPromise Done(Action callback);
        /// <summary>
        /// Attaches a callback to be executed when the Deferred is rejected
        /// </summary>
        IPromise Fail(Action<Exception> callback);
        /// <summary>
        /// Attaches a callback to be executed after all the Done an the Fail callbacks
        /// </summary>
        IPromise Always(Action callback);

        /// <summary>
        /// Attaches the passed IPromise to this IPromise
        /// </summary>
        IPromise Then(IPromise nextPromise);

        Boolean IsPending();
        Boolean IsResolved();
        Boolean IsRejected();

        /// <summary>
        /// Error in the Deferred Operation (if IsRejected)
        /// </summary>
        Exception Error { get; }
    }


    public static class DeferredExtensions
    {
        public static IPromise<T> Then<T>(this Deferred<T> deferred, Action success, Action<Exception> fail = null)
        {
            return Then(deferred, x => success(), fail);
        }

        public static IPromise<T> Then<T>(this Deferred<T> deferred, Action<T> success, Action<Exception> fail = null)
        {
            var promise = deferred.Then(x =>
            {
                success(x);
                return x;
            });

            if (fail != null)
            {
                promise.Fail(fail);
            }

            return promise;
        }
    }

    public class Deferred : IPromise
    {
        public enum Statuses
        {
            Pending,
            Resolved,
            Rejected
        }

        #region ~Ctor

        public Deferred()
        {
            State = Statuses.Pending;
        }

        #endregion

        public Statuses State { get; protected set; }
        public Exception Error { get; protected set; }

        protected List<Action> DoneCallbacks = new List<Action>();
        protected List<Action<Exception>> FailCallbacks = new List<Action<Exception>>();
        protected List<Action> AlwaysCallbacks = new List<Action>();

        #region IPromise Members

        public virtual IPromise Done(Action callback)
        {
            lock (DoneCallbacks)
            {
                DoneCallbacks.Add(callback);
                if (State == Statuses.Resolved)
                    callback();
                return this.Promise();
            }
        }

        public virtual IPromise Fail(Action<Exception> callback)
        {
            lock (FailCallbacks)
            {
                FailCallbacks.Add(callback);
                if (State == Statuses.Rejected)
                    callback(Error);
                return this.Promise();
            }
        }

        public virtual IPromise Always(Action callback)
        {
            lock (AlwaysCallbacks)
            {
                AlwaysCallbacks.Add(callback);
                if (State == Statuses.Rejected || State == Statuses.Resolved)
                    callback();
                return this.Promise();
            }
        }

        public virtual IPromise Then(IPromise nextPromise)
        {
            var nextDeferred = new Deferred();
            this.Done(() =>
                {
                    nextPromise
                        .Done(() =>
                        {
                            nextDeferred.Resolve();
                        })
                        .Fail((error) =>
                        {
                            nextDeferred.Reject(error);
                        });
                })
                .Fail((error) =>
                {
                    nextDeferred.Reject(error);
                });
            return nextDeferred.Promise();
        }

        public virtual Boolean IsResolved()
        {
            lock (DoneCallbacks)
            {
                lock (FailCallbacks)
                {
                    lock (AlwaysCallbacks)
                    {
                        return this.State == Statuses.Resolved;
                    }
                }
            }
        }

        public virtual Boolean IsPending()
        {
            lock (DoneCallbacks)
            {
                lock (FailCallbacks)
                {
                    lock (AlwaysCallbacks)
                    {
                        return this.State == Statuses.Pending;
                    }
                }
            }
        }

        public virtual Boolean IsRejected()
        {
            return State == Statuses.Rejected;
        }

        #endregion

        public virtual IPromise Resolve()
        {
            if (State != Statuses.Pending)
                throw new InvalidOperationException();

            lock (DoneCallbacks)
            {
                lock (AlwaysCallbacks)
                {
                    State = Statuses.Resolved;
                    foreach (var callBack in DoneCallbacks)
                        callBack();
                    foreach (var callBack in AlwaysCallbacks)
                        callBack();
                }
            }
            return this;
        }

        public virtual IPromise Reject(Exception error = null)
        {
            if (State != Statuses.Pending)
                throw new InvalidOperationException();

            lock (FailCallbacks)
            {
                lock (AlwaysCallbacks)
                {
                    Error = error;
                    State = Statuses.Rejected;
                    foreach (var callBack in FailCallbacks)
                        callBack(error);
                    foreach (var callBack in AlwaysCallbacks)
                        callBack();
                }
            }
            return this;
        }

        public virtual IPromise Promise()
        {
            return this;
        }

        public static IPromise<IPromise[]> When(params IPromise[] promises)
        {
            var def = new Deferred<IPromise[]>();
            foreach (var prom in promises)
            {
                prom.Done(() =>
                    {
                        lock (promises)
                        {
                            if (promises.All(p => p.IsResolved()))
                            {
                                def.Resolve(promises);
                            }
                        }
                    })
                    .Fail((error) =>
                    {
                        if (promises.All(p => !p.IsPending()))
                        {
                            def.Reject();
                        }
                    });
            }
            return def.Promise();
        }
    }

    public interface IPromise<TResult> : IPromise
    {
        /// <summary>
        /// Attaches a callback to be executed when the Deferred is resolved
        /// </summary>
        IPromise<TResult> Done(Action<TResult> callback);
        /// <summary>
        /// Attaches a callback to be executed when the Deferred is rejected
        /// </summary>
        new IPromise<TResult> Fail(Action<Exception> callback);
        /// <summary>
        /// Attaches a callback to be executed after all the Done an the Fail callbacks
        /// </summary>
        new IPromise<TResult> Always(Action callback);
        /// <summary>
        /// Result of the Deferred Operation (if IsResolved)
        /// </summary>
        TResult Result { get; }

        /// <summary>
        /// Alters the result if the IPromise is resolved
        /// </summary>
        IPromise<TResult> Then(Func<TResult, TResult> doneFilter);
        IPromise<TNextResult> Then<TNextResult>(Func<TResult, TNextResult> doneFilter);
        /// <summary>
        /// Attaches the passed IPromise to this IPromise
        /// </summary>
        IPromise<TNextResult> Then<TNextResult>(Func<TResult, IPromise<TNextResult>> nextPromiseFactory);

        IPromise Then(Func<TResult, IPromise> nextPromiseFactory);
    }

    public class Deferred<TResult> : Deferred, IPromise<TResult>
    {
        #region ~Ctor

        public Deferred()
            : base()
        {
            Result = default(TResult);
        }

        #endregion

        public TResult Result { get; protected set; }

        protected List<Action<TResult>> DoneCallbacksT = new List<Action<TResult>>();

        #region IPromise Members

        public new IPromise<TResult> Always(Action callback)
        {
            lock (AlwaysCallbacks)
            {
                base.Always(callback);
                return this.Promise();
            }
        }

        #endregion

        #region IPromise<TResult> Members

        public IPromise<TResult> Done(Action<TResult> callback)
        {
            lock (DoneCallbacksT)
            {
                DoneCallbacksT.Add(callback);
                if (State == Statuses.Resolved)
                    callback(Result);
                return this.Promise();
            }
        }

        public new IPromise<TResult> Fail(Action<Exception> callback)
        {
            lock (FailCallbacks)
            {
                base.Fail(callback);
                return this.Promise();
            }
        }

        public virtual IPromise<TResult> Then(Func<TResult, TResult> doneFilter)
        {
            var nextDeferred = new Deferred<TResult>();
            this.Done((result) =>
                {
                    nextDeferred.Resolve(doneFilter(result));
                })
                .Fail((error) =>
                {
                    nextDeferred.Reject(error);
                });
            return nextDeferred.Promise();
        }

        public virtual IPromise<TNextResult> Then<TNextResult>(Func<TResult, TNextResult> doneFilter)
        {
            var nextDeferred = new Deferred<TNextResult>();
            this.Done((result) =>
                {
                    nextDeferred.Resolve(doneFilter(result));
                })
                .Fail((error) =>
                {
                    nextDeferred.Reject(error);
                });
            return nextDeferred.Promise();
        }

        public virtual IPromise Then(Func<TResult, IPromise> nextPromiseFactory)
        {
            var nextDeferred = new Deferred();
            this.Done((result) =>
                {
                    nextPromiseFactory(result)
                        .Done(() =>
                        {
                            nextDeferred.Resolve();
                        })
                        .Fail((error) =>
                        {
                            nextDeferred.Reject(error);
                        });
                })
                .Fail((error) =>
                {
                    nextDeferred.Reject(error);
                });
            return nextDeferred.Promise();
        }

        public virtual IPromise<TNextResult> Then<TNextResult>(Func<TResult, IPromise<TNextResult>> nextPromiseFactory)
        {
            var nextDeferred = new Deferred<TNextResult>();
            this.Done((result) =>
                {
                    nextPromiseFactory(result)
                        .Done((nextResult) =>
                        {
                            nextDeferred.Resolve(nextResult);
                        })
                        .Fail((error) =>
                        {
                            nextDeferred.Reject(error);
                        });
                })
                .Fail((error) =>
                {
                    nextDeferred.Reject(error);
                });
            return nextDeferred.Promise();
        }

        #endregion

        #region Deferred Members

        public override IPromise Resolve()
        {
            throw new NotSupportedException();
        }

        public new IPromise<TResult> Promise()
        {
            return this;
        }

        #endregion

        public virtual IPromise<TResult> Resolve(TResult result)
        {
            if (State != Statuses.Pending)
                throw new InvalidOperationException();

            lock (DoneCallbacksT)
            {
                Result = result;
                foreach (var callBack in DoneCallbacksT)
                    callBack(Result);
                base.Resolve();
            }
            return this;
        }
    }
}
