using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MonitoredUndo
{

    /// <summary>
    /// Represents an individual change, with the commands to undo / redo the change as needed.
    /// </summary>
    public class DelegateChange : Change
    {

        

        private Action _UndoAction;
        private Action _RedoAction;

        

        

        /// <summary>
        /// Create a new change item.
        /// </summary>
        /// <param name="target">The object that this change affects.</param>
        /// <param name="undoAction">The delegate that will do the Undo logic</param>
        /// <param name="redoAction">The delegate that will do the Redo logic</param>
        /// <param name="changeKey">An object, that will be used to detect changes that affect the same "field". 
        /// This object should implement or override object.Equals() and return true if the changes are for the same field.
        /// This is used when the undo UndoRoot has started a batch, or when the UndoRoot.ConsolidateChangesForSameInstance is true.
        /// A string will work, but should be sufficiently unique within the scope of changes that affect this Target instance.
        /// Another good option is to use the Tuple<> class to uniquely identify the change. The Tuple could contain
        /// the object, and a string representing the property name. For a collection change, you might include the 
        /// instance, the property name, and the item added/removed from the collection.
        /// </param>
        public DelegateChange(object target, Action undoAction, Action redoAction, object changeKey)
            : base(target, changeKey)
        {
            _UndoAction = undoAction; // new WeakReference(undoAction);
            _RedoAction = redoAction; // new WeakReference(redoAction);
        }

        


        

        /// <summary>
        /// When consolidating events, we want to keep the original "Undo"
        /// but use the most recent Redo. This will pull the Redo from the 
        /// specified Change and apply it to this instance.
        /// </summary>
        public override void MergeWith(Change latestChange)
        {
            var other = latestChange as DelegateChange;

            if (null != other)
                this._RedoAction = other._RedoAction;
        }

        

        

        protected override void PerformUndo()
        {
            var action = _UndoAction;
            if (null != action)
                action();
        }

        protected override void PerformRedo()
        {
            var action = _RedoAction;
            if (null != action)
                action();
        }

        

    }


}
