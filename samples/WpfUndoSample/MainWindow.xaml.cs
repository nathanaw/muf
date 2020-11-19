using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MonitoredUndo;
using System.ComponentModel;

namespace WpfUndoSample
{

    // ********************************************************************************************************************
    // NOTE:
    //  For this sample, the window is the "model" and also the "root" of the undo document hierarchy.
    //  As a result, this window is used for bindings and therefor must support INotifyPropertyChanged and ISupportsUndo.
    // 
    // In a production application, using the code-behind like this is probably not the "best practice". 
    // However, for a sample, it simplifies the number of concepts needed to understand how the undo system works.
    // ********************************************************************************************************************
    public partial class MainWindow 
        : Window, INotifyPropertyChanged, ISupportsUndo
    {
        
        

        public MainWindow()
        {
            InitializeComponent();
        }

        
        
        

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            FirstNameTextbox.Focus();
        }


        // The following 4 event handlers support the "CommandBindings" in the window.
        // These hook to the Undo and Redo commands.

        private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Tell the UI whether Undo is available.
            e.CanExecute = UndoService.Current[this].CanUndo;
        }

        private void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Tell the UI whether Redo is available.
            e.CanExecute = UndoService.Current[this].CanRedo;
        }

        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Get the document root. In this case, we pass in "this", which 
            // implements ISupportsUndo. The ISupportsUndo interface is used
            // by the UndoService to locate the appropriate root node of an 
            // undoable document.
            // In this case, we are treating the window as the root of the undoable
            // document, but in a larger system the root would probably be your
            // domain model.
            var undoRoot = UndoService.Current[this]; 
            undoRoot.Undo();
        }

        private void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // A shorthand version of the above call to Undo, except 
            // that this calls Redo.
            UndoService.Current[this].Redo();
        }



        private void Slider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!BatchAgeChanges)
                return;

            // Start a batch to collect all subsequent undo events (for this root)
            // into a single changeset.
            // 
            // Passing "false" for the last parameter tells the system to keep
            // each individual change that is made. If desired, pass "true" to
            // de-dupe these changes and reduce the memory requirements of the
            // changeset.
            UndoService.Current[this].BeginChangeSetBatch("Age Changed", false);

            e.Handled = false;
        }

        private void Slider_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (!BatchAgeChanges)
                return;

            UndoService.Current[this].EndChangeSetBatch();
            
            e.Handled = false;
        }


        

        

        // Below are properties bound to the UI with a XAML binding.
        // NOTE that these properly implement INotifyPropertyChange.
        //  This is critical if the UI is going to stay in sync
        //  with the changes to the data.


        private string _FirstName;
        public string FirstName
        {
            get { return _FirstName; }
            set
            {
                if (value == _FirstName)
                    return;

                // Store this change in the Undo system.
                // This uses the "DefaultChangeFactory" to construct the change, but you can 
                // store changes any way you like.
                DefaultChangeFactory.OnChanging(this, "FirstName", _FirstName, value, "First Name Changed");

                _FirstName = value;
                OnPropertyChanged("FirstName"); // Tells the UI that this property has changed.
                OnPropertyChanged("FullName");  // If FirstName changes, then FullName is also affected.
            }
        }

        private string _LastName;
        public string LastName
        {
            get { return _LastName; }
            set
            {
                if (value == _LastName)
                    return;

                // Store this change in the Undo system.
                // This uses the "DefaultChangeFactory" to construct the change, but you can 
                // store changes any way you like.
                DefaultChangeFactory.OnChanging(this, "LastName", _LastName, value, "Last Name Changed");

                _LastName = value;
                OnPropertyChanged("LastName");  // Tells the UI that this property changed.
                OnPropertyChanged("FullName");  // If LastName changes, then FullName is also affected.
            }
        }

        public string FullName
        {
            get
            {
                return string.Format("{0} {1}", FirstName, LastName);
            }
        }

        
        private int _Age;
        public int Age
        {
            get { return _Age; }
            set
            {
                if (value == _Age)
                    return;

                // Store this change in the Undo system.
                // This uses the "DefaultChangeFactory" to construct the change, but you can 
                // store changes any way you like.
                DefaultChangeFactory.OnChanging(this, "Age", _Age, value, "Age Changed");

                _Age = value;
                OnPropertyChanged("Age");
            }
        }


        private bool _BatchAgeChanges = true;
        public bool BatchAgeChanges
        {
            get { return _BatchAgeChanges; }
            set
            {
                if (value == _BatchAgeChanges)
                    return;

                _BatchAgeChanges = value;
                OnPropertyChanged("BatchAgeChanges");
            }
        }


        // Expose the undo and redo stacks to the UI for binding.

        public IEnumerable<ChangeSet> UndoStack
        {
            get
            {
                return UndoService.Current[this].UndoStack;

            }
        }

        public IEnumerable<ChangeSet> RedoStack
        {
            get
            {
                return UndoService.Current[this].RedoStack;

            }
        }

        

        

        

        

        // This interface is needed on objects that are part of the 
        // document hierarchy. It allows this object to be passed
        // in to the UndoService.Current[] indexer.
        // This method should return the "root" of the document.
        // In this case, we are treating the window as the "root" of the 
        // document. In other cases, the root might be your domain model.
        //
        // See the unit tests for a better example of how this comes into 
        // use for a multi-object document hierarchy.

        public object GetUndoRoot()
        {
            return this;
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
                System.Diagnostics.Debug.Assert(false, msg);
            }
        }

        
   
    }
}
