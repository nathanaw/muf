using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MonitoredUndo;


namespace MonitoredUndoTests
{
    public class RootDocument : INotifyPropertyChanged, ISupportsUndo
    {
        #region Constructors

        public RootDocument()
        {
            _Bs = new ObservableCollection<ChildB>();
            _Bs.CollectionChanged += Bs_CollectionChanged;
        }

        #endregion
        #region Properties

        private ChildA _A;
        public ChildA A
        {
            get { return _A; }
            set
            {
                if (value == _A)
                    return;

                value.Root = this;

                // This line will log the property change with the undo framework.
                DefaultChangeFactory.OnChanging(this, "A", _A, value);

                _A = value;
                OnPropertyChanged("A");
            }
        }

        private ObservableCollection<ChildB> _Bs;
        public ObservableCollection<ChildB> Bs
        {
            get { return _Bs; }
        }

        #endregion
        #region Collection Changed Handlers

        void Bs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                foreach (ChildB item in e.NewItems)
                    item.Root = this;
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                foreach (ChildB item in e.OldItems)
                    item.Root = null;

            // This line will log the collection change with the undo framework.
            DefaultChangeFactory.OnCollectionChanged(this, "Bs", this.Bs, e);
        }

        #endregion
        #region ISupportsUndo Members

        public object GetUndoRoot()
        {
            return this;
        }

        #endregion

        #region INotifyPropertyChanged

        /// <summary>
        /// The PropertyChanged event is used by consuming code
        /// (like WPF's binding infrastructure) to detect when
        /// a value has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raise the PropertyChanged event for the 
        /// specified property.
        /// </summary>
        /// <param name="propertyName">
        /// A string representing the name of 
        /// the property that changed.</param>
        /// <remarks>
        /// Only raise the event if the value of the property 
        /// has changed from its previous value</remarks>
        protected void OnPropertyChanged(string propertyName)
        {
            // Validate the property name in debug builds
            VerifyProperty(propertyName);

            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Verifies whether the current class provides a property with a given
        /// name. This method is only invoked in debug builds, and results in
        /// a runtime exception if the <see cref="OnPropertyChanged"/> method
        /// is being invoked with an invalid property name. This may happen if
        /// a property's name was changed but not the parameter of the property's
        /// invocation of <see cref="OnPropertyChanged"/>.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        [System.Diagnostics.Conditional("DEBUG")]
        private void VerifyProperty(string propertyName)
        {
            Type type = this.GetType();

            // Look for a *public* property with the specified name
            System.Reflection.PropertyInfo pi = type.GetProperty(propertyName);
            if (pi == null)
            {
                // There is no matching property - notify the developer
                string msg = "OnPropertyChanged was invoked with invalid " +
                                "property name {0}. {0} is not a public " +
                                "property of {1}.";
                msg = String.Format(msg, propertyName, type.FullName);
                System.Diagnostics.Debug.Fail(msg);
            }
        }

        #endregion
    }

}
