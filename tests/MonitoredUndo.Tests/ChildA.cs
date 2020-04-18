using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MonitoredUndo;

namespace MonitoredUndo.Tests
{
    public class ChildA : INotifyPropertyChanged, ISupportsUndo, IUndoMetadata
    {
        

        public ChildA()
        {
            _ID = Guid.NewGuid();
        }

        public ChildA(Guid id)
        {
            _ID = id;
        }

        
        

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

                // This line will log the property change with the undo framework.
                DefaultChangeFactory.Current.OnChanging(this, "Name", _Name, value, "Name changed.");

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

                // This line will log the property change with the undo framework.
                DefaultChangeFactory.Current.OnChanging(this, "Root", _Root, value);

                _Root = value;
                OnPropertyChanged("Root");
            }
        }



        // This property is undoable, as long as the Name property is not "DISABLE_UNDO".
        // This shows the usage of the IUndoMetadata interface, which can be used to determine whether
        // a property is undoable.
        private string _UndoableSometimes;
        public string UndoableSometimes
        {
            get { return _UndoableSometimes; }
            set
            {
                if (value == _UndoableSometimes)
                    return;

                // This line will log the property change with the undo framework.
                DefaultChangeFactory.Current.OnChanging(this, "UndoableSometimes", _UndoableSometimes, value);

                _UndoableSometimes = value;
                OnPropertyChanged("UndoableSometimes");
            }
        }

        
        

        public object GetUndoRoot()
        {
            return Root;
        }

        
        

        public bool CanUndoProperty(string propertyName, object oldValue, object newValue)
        {
            if (propertyName == "UndoableSometimes")
            {
                if (Name == "DISABLE_UNDO")
                    return false;
            }

            return true;
        }

        public bool CanUndoCollectionChange(string propertyName, object collection, System.Collections.Specialized.NotifyCollectionChangedEventArgs args)
        {
            return true;
        }

        
        

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
            Type type = GetType();

            // Look for a *public* property with the specified name
            System.Reflection.PropertyInfo pi = type.GetProperty(propertyName);
            if (pi == null)
            {
                // There is no matching property - notify the developer
                string msg = "OnPropertyChanged was invoked with invalid " +
                                "property name {0}. {0} is not a public " +
                                "property of {1}.";
                msg = string.Format(msg, propertyName, type.FullName);
                System.Diagnostics.Debug.Fail(msg);
            }
        }

        
    }

}
