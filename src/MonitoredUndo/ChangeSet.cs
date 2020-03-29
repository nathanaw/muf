using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonitoredUndo
{

    /// <summary>
    /// A set of changes that represent a single "unit of change".
    /// </summary>
    public class ChangeSet
    {

        

        private UndoRoot _UndoRoot;
        private string _Description;
        private IList<Change> _Changes;
        private bool _Undone = false;

        

        

        /// <summary>
        /// Create a ChangeSet for the specified UndoRoot.
        /// </summary>
        /// <param name="undoRoot">The UndoRoot that this ChangeSet belongs to.</param>
        /// <param name="description">A description of the change.</param>
        /// <param name="change">The Change instance that can perform the undo / redo as needed.</param>
        public ChangeSet(UndoRoot undoRoot, string description, Change change)
        {
            _UndoRoot = undoRoot;
            _Changes = new List<Change>();
            _Description = description;

            if (null != change)
                AddChange(change);
        }

        

        

        /// <summary>
        /// The associated UndoRoot.
        /// </summary>
        public UndoRoot UndoRoot { get { return _UndoRoot; } }

        /// <summary>
        /// A description of this set of changes.
        /// </summary>
        public string Description { get { return _Description; } }

        /// <summary>
        /// Has this ChangeSet been undone.
        /// </summary>
        public bool Undone { get { return _Undone; } }

        /// <summary>
        /// The changes that are part of this ChangeSet
        /// </summary>
        public IEnumerable<Change> Changes
        {
            get
            {
                return _Changes;
            }
        }

        

        

        /// <summary>
        /// Add a change to this ChangeSet.
        /// </summary>
        /// <param name="change"></param>
        internal void AddChange(Change change)
        {
            if (_UndoRoot.ConsolidateChangesForSameInstance)
            {
                //var dupes = _Changes.Where(c => null != c.ChangeKey && c.ChangeKey.Equals(change.ChangeKey)).ToList();
                //if (null != dupes && dupes.Count > 0)
                //    dupes.ForEach(c => _Changes.Remove(c));

                var dupe = _Changes.FirstOrDefault(c => null != c.ChangeKey && c.ChangeKey.Equals(change.ChangeKey));
                if (null != dupe)
                {
                    dupe.MergeWith(change);
                    // System.Diagnostics.Debug.WriteLine("AddChange: MERGED");
                }
                else
                {
                    _Changes.Add(change);
                }
            }
            else
            {
                _Changes.Add(change);
            }
        }

        /// <summary>
        /// Undo all Changes in this ChangeSet.
        /// </summary>
        internal void Undo()
        {
            foreach (var change in _Changes.Reverse())
                change.Undo();

            _Undone = true;
        }

        /// <summary>
        /// Redo all Changes in this ChangeSet.
        /// </summary>
        internal void Redo()
        {
            foreach (var change in _Changes)
                change.Redo();

            _Undone = false;
        }

        

    }

}
