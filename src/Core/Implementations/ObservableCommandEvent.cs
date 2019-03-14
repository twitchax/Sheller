using System;
using System.Collections.Generic;
using Sheller.Models;

namespace Sheller.Implementations
{
    /// <summary>
    /// Observable for command events.
    /// </summary>
    public class ObservableCommandEvent : IObservable<ICommandEvent>
    {
        internal List<IObserver<ICommandEvent>> Observers { get; private set; } = new List<IObserver<ICommandEvent>>();

        /// <summary>
        /// The Subscribe method.
        /// </summary>
        /// <param name="observer">The observer.</param>
        /// <returns>An <see cref="IDisposable"/>.</returns>
        public IDisposable Subscribe(IObserver<ICommandEvent> observer) 
        {
            if(!Observers.Contains(observer)) 
                Observers.Add(observer);
            return new Unsubscriber(Observers, observer);
        }

        internal void FireEvent(ICommandEvent commandEvent)
        {
            foreach(var observer in Observers)
                observer.OnNext(commandEvent);
        }

        internal void FireError(Exception e)
        {
            foreach(var observer in Observers)
                observer.OnError(e);
        }

        internal void FireCompleted()
        {
            foreach (var observer in Observers.ToArray())
                if (Observers.Contains(observer))
                    observer.OnCompleted();

            Observers.Clear();
        }

        internal static ObservableCommandEvent Merge(ObservableCommandEvent ob1, ObservableCommandEvent ob2)
        {
            var newObservable = new ObservableCommandEvent();
            
            ob1.Observers.ForEach(o => newObservable.Subscribe(o));
            ob2.Observers.ForEach(o => newObservable.Subscribe(o));

            return newObservable;
        }

        private class Unsubscriber : IDisposable
        {
            private List<IObserver<ICommandEvent>> _observers;
            private IObserver<ICommandEvent> _observer;

            public Unsubscriber(List<IObserver<ICommandEvent>> observers, IObserver<ICommandEvent> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                    _observers.Remove(_observer);
            }
        }
    }
}