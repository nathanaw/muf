using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MonitoredUndo
{

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class CollectionMoveChange : CollectionChange
    {
        #region Member Variables

        private readonly int _NewIndex;
        private readonly int _OldIndex;

        private int _RedoNewIndex;
        private int _RedoOldIndex;

        #endregion

        #region Constructors

        public CollectionMoveChange(object target, string propertyName, IList collection, int newIndex, int oldIndex)
            : base(target, propertyName, collection,
                   new ChangeKey<object, string, object>(
                       target, propertyName, new ChangeKey<int, int>(oldIndex, newIndex)))
        {
            this._NewIndex = newIndex;
            this._OldIndex = oldIndex;

            this._RedoNewIndex = newIndex;
            this._RedoOldIndex = oldIndex;
        }

        #endregion

        #region Public Properties

        public int NewIndex
        {
            get { return _NewIndex; }
        }

        public int OldIndex
        {
            get { return _OldIndex; }
        }

        #endregion

        #region Public Methods

        public override void MergeWith(Change latestChange)
        {
            var other = latestChange as CollectionMoveChange;

            if (null != other)
            {
                this._RedoOldIndex = other._RedoOldIndex;
                this._RedoNewIndex = other._RedoNewIndex;
            }
            // FIXME should only affect undo
        }

        #endregion

        #region Internal

        protected override void PerformUndo()
        {
            Collection.GetType().GetMethod("Move").Invoke(Collection, new object[] { NewIndex, OldIndex });
        }

        protected override void PerformRedo()
        {
            Collection.GetType().GetMethod("Move").Invoke(Collection, new object[] { _RedoOldIndex, _RedoNewIndex });
        }

        private string DebuggerDisplay
        {
            get
            {
                return string.Format(
                    "CollectionMoveChange(Property={0}, Target={{{1}}}, OldIndex={2}, NewIndex={{{3}}})",
                    PropertyName, Target, OldIndex, NewIndex);
            }
        }

        #endregion
    }


}
