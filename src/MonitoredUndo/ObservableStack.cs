/*
    Based on sample code posted on Stack Overflow by 'Ernie S' (https://stackoverflow.com/users/1324284/ernie-s)
    at https://stackoverflow.com/questions/3127136/observable-stack-and-queue/56177896#56177896
*/
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace MonitoredUndo
{
    /// <summary>
    /// Observable version of the generic Stack collection
    /// </summary>
    internal class ObservableStack<T> : Stack<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Constructors

        public ObservableStack() : base()
        {
        }

        public ObservableStack(IEnumerable<T> collection) : base(collection)
        {
        }

        public ObservableStack(int capacity) : base(capacity)
        {
        }

        #endregion

        #region Overrides

        public new virtual T Pop()
        {
            T item = base.Pop();
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);

            return item;
        }

        public new virtual void Push(T item)
        {
            base.Push(item);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item);
        }

        public new virtual void Clear()
        {
            base.Clear();
            OnCollectionChanged(NotifyCollectionChangedAction.Reset, default);
        }

        #endregion

        #region CollectionChanged

        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, T item)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action,
                item,
                item == null ? -1 : 0)
            );

            OnPropertyChanged(nameof(Count));
        }

        #endregion

        #region PropertyChanged

        public virtual event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string proertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(proertyName));
        }

        #endregion
    }
}
