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
    public class ChildB : INotifyPropertyChanged, ISupportsUndo
    {
        #region Constructors

        public ChildB()
        {
            _ID = Guid.NewGuid();
        }

        public ChildB(Guid id)
        {
            _ID = id;
        }

        #endregion
        #region Properties

        private Guid _ID;
        public Guid ID
        {
            get { return _ID; }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value == _Name)
                    return;

                DefaultChangeFactory.OnChanging(this, "Name", _Name, value);

                _Name = value;
                OnPropertyChanged("Name");
            }
        }


        private RootDocument _Root;
        public RootDocument Root
        {
            get { return _Root; }
            set
            {
                if (value == _Root)
                    return;

                _Root = value;
                OnPropertyChanged("Root");
            }
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
        #region ISupportsUndo Members

        public object GetUndoRoot()
        {
            return this.Root;
        }

        #endregion
    }
}
