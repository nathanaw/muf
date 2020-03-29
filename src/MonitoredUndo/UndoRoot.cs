using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonitoredUndo
{

    /// <summary>
    /// Tracks the ChangeSets and behavior for a single root object (or document).
    /// </summary>
    public class UndoRoot
    {

        

        // WeakReference because we don't want the undo stack to keep something locked in memory.
        private WeakReference _Root;

        // The list of undo / redo actions.
        private Stack<ChangeSet> _UndoStack;
        private Stack<ChangeSet> _RedoStack;

        // Tracks whether a batch (or batches) has been started.
        private int _IsInBatchCounter = 0;

        // Determines whether the undo framework will consolidate (or de-dupe) changes to the same property within the batch.
        private bool _ConsolidateChangesForSameInstance = false;

        // When in a batch, changes are grouped into this ChangeSet.
        private ChangeSet _CurrentBatchChangeSet;

        // Is the system currently undoing or redoing a changeset.
        private bool _IsUndoingOrRedoing = false;

        

        

        public event EventHandler UndoStackChanged;

        public event EventHandler RedoStackChanged;

        

        

        /// <summary>
        /// Create a new UndoRoot to track undo / redo actions for a given instance / document.
        /// </summary>
        /// <param name="root">The "root" instance of the object hierarchy. All changesets will 
        /// need to passs a reference to this instance when they track changes.</param>
        public UndoRoot(object root)
        {
            _Root = new WeakReference(root);
            _UndoStack = new Stack<ChangeSet>();
            _RedoStack = new Stack<ChangeSet>();
        }

        

        

        /// <summary>
        /// The instance that represents the root (or document) for this set of changes.
        /// </summary>
        /// <remarks>
        /// This is needed so that a single instance of the application can track undo histories 
        /// for multiple "root" or "document" instances at the same time. These histories should not 
        /// overlap or show in the same undo history.
        /// </remarks>
        public object Root
        {
            get
            {
                if (null != _Root && _Root.IsAlive)
                    return _Root.Target;
                else
                    return null;
            }
        }

        /// <summary>
        /// A collection of undoable change sets for the current Root.
        /// </summary>
        public IEnumerable<ChangeSet> UndoStack
        {
            get { return _UndoStack; }
        }

        /// <summary>
        /// A collection of redoable change sets for the current Root.
        /// </summary>
        public IEnumerable<ChangeSet> RedoStack
        {
            get { return _RedoStack; }
        }

        /// <summary>
        /// Is this UndoRoot currently collecting changes as part of a batch.
        /// </summary>
        public bool IsInBatch
        {
            get
            {
                return _IsInBatchCounter > 0;
            }
        }

        /// <summary>
        /// Is this UndoRoot currently undoing or redoing a change set.
        /// </summary>
        public bool IsUndoingOrRedoing
        {
            get
            {
                return _IsUndoingOrRedoing;
            }
        }

        /// <summary>
        /// Should changes to the same property be consolidated (de-duped).
        /// </summary>
        public bool ConsolidateChangesForSameInstance
        {
            get
            {
                return _ConsolidateChangesForSameInstance;
            }
        }

        public bool CanUndo
        {
            get
            {
                return _UndoStack.Count > 0;
            }
        }

        public bool CanRedo
        {
            get
            {
                return _RedoStack.Count > 0;
            }
        }

        

        

        /// <summary>
        /// Tells the UndoRoot that all subsequent changes should be part of a single ChangeSet.
        /// </summary>
        public void BeginChangeSetBatch(string batchDescription, bool consolidateChangesForSameInstance)
        {
            // We don't want to add additional changes representing the operations that happen when undoing or redoing a change.
            if (_IsUndoingOrRedoing)
                return;

            _IsInBatchCounter++;

            if (_IsInBatchCounter == 1)
            {
                _ConsolidateChangesForSameInstance = consolidateChangesForSameInstance;
                _CurrentBatchChangeSet = new ChangeSet(this, batchDescription, null);
                _UndoStack.Push(_CurrentBatchChangeSet);
                OnUndoStackChanged();
            }
        }

        /// <summary>
        /// Tells the UndoRoot that it can stop collecting Changes into a single ChangeSet.
        /// </summary>
        public void EndChangeSetBatch()
        {
            _IsInBatchCounter--;

            if (_IsInBatchCounter < 0)
                _IsInBatchCounter = 0;

            if (_IsInBatchCounter == 0)
            {
                _ConsolidateChangesForSameInstance = false;
                _CurrentBatchChangeSet = null;
            }
        }

        /// <summary>
        /// Undo the first available ChangeSet.
        /// </summary>
        public void Undo()
        {
            var last = _UndoStack.FirstOrDefault();
            if (null != last)
                Undo(last);
        }


        /// <summary>
        /// Undo all changesets up to and including the lastChangeToUndo.
        /// </summary>
        public void Undo(ChangeSet lastChangeToUndo)
        {
            if (IsInBatch)
                throw new InvalidOperationException("Cannot perform an Undo when the Undo Service is collecting a batch of changes. The batch must be completed first.");

            if (!_UndoStack.Contains(lastChangeToUndo))
                throw new InvalidOperationException("The specified change does not exist in the list of undoable changes. Perhaps it has already been undone.");

            System.Diagnostics.Debug.WriteLine("Starting UNDO: " + lastChangeToUndo.Description);

            bool done = false;
            _IsUndoingOrRedoing = true;

            try
            {
                do
                {
                    var changeSet = _UndoStack.Pop();
                    OnUndoStackChanged();

                    if (changeSet == lastChangeToUndo || _UndoStack.Count == 0)
                        done = true;

                    changeSet.Undo();

                    _RedoStack.Push(changeSet);
                    OnRedoStackChanged();

                } while (!done);
            }
            finally
            {
                _IsUndoingOrRedoing = false;
            }

        }

        /// <summary>
        /// Redo the first available ChangeSet.
        /// </summary>
        public void Redo()
        {
            var last = _RedoStack.FirstOrDefault();
            if (null != last)
                Redo(last);
        }

        /// <summary>
        /// Redo ChangeSets up to and including the lastChangeToRedo.
        /// </summary>
        public void Redo(ChangeSet lastChangeToRedo)
        {
            if (IsInBatch)
                throw new InvalidOperationException("Cannot perform a Redo when the Undo Service is collecting a batch of changes. The batch must be completed first.");

            if (!_RedoStack.Contains(lastChangeToRedo))
                throw new InvalidOperationException("The specified change does not exist in the list of redoable changes. Perhaps it has already been redone.");

            System.Diagnostics.Debug.WriteLine("Starting REDO: " + lastChangeToRedo.Description);

            bool done = false;
            _IsUndoingOrRedoing = true;
            try
            {
                do
                {
                    var changeSet = _RedoStack.Pop();
                    OnRedoStackChanged();

                    if (changeSet == lastChangeToRedo || _RedoStack.Count == 0)
                        done = true;

                    changeSet.Redo();

                    _UndoStack.Push(changeSet);
                    OnUndoStackChanged();

                } while (!done);
            }
            finally
            {
                _IsUndoingOrRedoing = false;
            }
        }

        /// <summary>
        /// Add a change to the Undo history. The change will be added to the existing batch, if in a batch. 
        /// Otherwise, a new ChangeSet will be created.
        /// </summary>
        /// <param name="change">The change to add to the history.</param>
        /// <param name="description">The description of this change.</param>
        public void AddChange(Change change, string description)
        {
            // System.Diagnostics.Debug.WriteLine("Starting AddChange: " + description);

            // We don't want to add additional changes representing the operations that happen when undoing or redoing a change.
            if (_IsUndoingOrRedoing)
                return;

            //  If batched, add to the current ChangeSet, otherwise add a new ChangeSet.
            if (IsInBatch)
            {
                _CurrentBatchChangeSet.AddChange(change);
                //System.Diagnostics.Debug.WriteLine("AddChange: BATCHED " + description);
            }
            else
            {
                _UndoStack.Push(new ChangeSet(this, description, change));
                OnUndoStackChanged();
                //System.Diagnostics.Debug.WriteLine("AddChange: " + description);
            }

            // Prune the RedoStack
            _RedoStack.Clear();
            OnRedoStackChanged();
        }

        /// <summary>
        /// Adds a new changeset to the undo history. The change set will be added to the existing batch, if in a batch. 
        /// </summary>
        /// <param name="changeSet">The ChangeSet to add.</param>
        public void AddChange(ChangeSet changeSet)
        {
            // System.Diagnostics.Debug.WriteLine("Starting AddChange: " + description);

            // We don't want to add additional changes representing the operations that happen when undoing or redoing a change.
            if (_IsUndoingOrRedoing)
                return;

            //  If batched, add to the current ChangeSet, otherwise add a new ChangeSet.
            if (IsInBatch)
            {
                foreach (var chg in changeSet.Changes)
                {
                    _CurrentBatchChangeSet.AddChange(chg);
                    //System.Diagnostics.Debug.WriteLine("AddChange: BATCHED " + description);
                }
            }
            else
            {
                _UndoStack.Push(changeSet);
                OnUndoStackChanged();
                //System.Diagnostics.Debug.WriteLine("AddChange: " + description);
            }

            // Prune the RedoStack
            _RedoStack.Clear();
            OnRedoStackChanged();
        }

        public void Clear()
        {
            if (IsInBatch || _IsUndoingOrRedoing)
                throw new InvalidOperationException("Unable to clear the undo history because the system is collecting a batch of changes, or is in the process of undoing / redoing a change.");

            _UndoStack.Clear();
            _RedoStack.Clear();
            OnUndoStackChanged();
            OnRedoStackChanged();
        }

        

        

        private void OnUndoStackChanged()
        {
            if (null != UndoStackChanged)
                UndoStackChanged(this, EventArgs.Empty);
        }

        private void OnRedoStackChanged()
        {
            if (null != RedoStackChanged)
                RedoStackChanged(this, EventArgs.Empty);
        }

        

    }

}
