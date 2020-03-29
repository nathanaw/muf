using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using MonitoredUndo;

namespace WpfUndoSampleMVVM.Core
{
    public class MainWindowViewModel : INotifyPropertyChanged, ISupportsUndo
    {
        private CommandBindingCollection _commandBindings = new CommandBindingCollection();
        
        private ICommand _windowLoadedCommand;
        private ICommand _sliderMouseDownCommand;
        private ICommand _sliderLostMouseCapture;

        public CommandBindingCollection RegisterCommandBindings
        {
            get
            {
                return _commandBindings;
            }
        }

        public MainWindowViewModel()
        {
            InitialiseCommandBindings();
        }

        public ICommand WindowLoadedCommand
        {
            get
            {
                return _windowLoadedCommand ?? (_windowLoadedCommand = new RelayCommand(OnWindowLoaded));
            }
        }

        public ICommand SliderMouseDownCommand
        {
            get
            {
                return _sliderMouseDownCommand ??(_sliderMouseDownCommand = new RelayCommand<MouseButtonEventArgs>(OnSliderMouseDown));
            }
        }

        public ICommand SliderLostMouseCapture
        {
            get
            {
                return _sliderLostMouseCapture ?? (_sliderLostMouseCapture = new RelayCommand<MouseEventArgs>(OnSliderLostMouseCapture));
            }
        }

        private void OnSliderLostMouseCapture(MouseEventArgs e)
        {
            if (!BatchAgeChanges)
                return;

            UndoService.Current[this].EndChangeSetBatch();

            e.Handled = false;
        }

        private void OnSliderMouseDown(MouseButtonEventArgs e)
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

        private void OnWindowLoaded()
        {
            // The undo / redo stack collections are not "Observable", so we 
            // need to manually refresh the UI when they change.
            var root = UndoService.Current[this];
            root.UndoStackChanged += new EventHandler(OnUndoStackChanged);
            root.RedoStackChanged += new EventHandler(OnRedoStackChanged);
            //FirstNameTextbox.Focus();
        }

        // Refresh the UI when the undo stack changes.
        void OnUndoStackChanged(object sender, EventArgs e)
        {
            RefreshUndoStackList();
        }

        // Refresh the UI when the redo stack changes.
        void OnRedoStackChanged(object sender, EventArgs e)
        {
            RefreshUndoStackList();
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
                DefaultChangeFactory.Current.OnChanging(this, "FirstName", _FirstName, value, "First Name Changed");

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
                DefaultChangeFactory.Current.OnChanging(this, "LastName", _LastName, value, "Last Name Changed");

                _LastName = value;
                OnPropertyChanged("LastName");  // Tells the UI that this property changed.
                OnPropertyChanged("FullName");  // If LastName changes, then FullName is also affected.
            }
        }

        public string FullName
        {
            get
            {
                return String.Format("{0} {1}", FirstName, LastName);
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
                DefaultChangeFactory.Current.OnChanging(this, "Age", _Age, value, "Age Changed");

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

        

        

        // Refresh the UI when the undo / redo stacks change.
        private void RefreshUndoStackList()
        {
            // Calling refresh on the CollectionView will tell the UI to rebind the list.
            // If the list were an ObservableCollection, or implemented INotifyCollectionChanged, this would not be needed.
            var cv = CollectionViewSource.GetDefaultView(UndoStack);
            cv.Refresh();

            cv = CollectionViewSource.GetDefaultView(RedoStack);
            cv.Refresh();
        }

        

        private void InitialiseCommandBindings()
        {
            // create command binding for undo command
            var undoBinding = new CommandBinding(ApplicationCommands.Undo, UndoExecuted, UndoCanExecute);
            var redoBinding = new CommandBinding(ApplicationCommands.Redo, RedoExecuted, RedoCanExecute);

            // register the binding to the class
            CommandManager.RegisterClassCommandBinding(typeof(MainWindowViewModel), undoBinding);
            CommandManager.RegisterClassCommandBinding(typeof(MainWindowViewModel), redoBinding);

            CommandBindings.Add(undoBinding);
            CommandBindings.Add(redoBinding);
        }

        private void RedoExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // A shorthand version of the above call to Undo, except 
            // that this calls Redo.
            UndoService.Current[this].Redo();
        }

        private void RedoCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Tell the UI whether Redo is available.
            e.CanExecute = UndoService.Current[this].CanRedo;
        }

        private void UndoExecuted(object sender, ExecutedRoutedEventArgs e)
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

        private void UndoCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Tell the UI whether Undo is available.
            e.CanExecute = UndoService.Current[this].CanUndo;
        }

        public CommandBindingCollection CommandBindings
        {
            get
            {
                return _commandBindings;
            }
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

        
    }
}
