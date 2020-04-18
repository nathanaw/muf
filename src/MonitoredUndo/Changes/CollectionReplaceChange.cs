using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MonitoredUndo
{

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class CollectionReplaceChange : CollectionChange
    {
        

        private readonly int _Index;
        private readonly object _OldItem;
        private readonly object _NewItem;

        private int _RedoIndex;
        private object _RedoNewItem;

        

        

        public CollectionReplaceChange(object target, string propertyName, IList collection,
                                       int index, object oldItem, object newItem)
            : base(target, propertyName, collection,
                   new ChangeKey<object, string, object>(target, propertyName,
                                                         new ChangeKey<object, object>(oldItem, newItem)))
        {
            this._Index = index;
            this._OldItem = oldItem;
            this._NewItem = newItem;

            this._RedoIndex = index;
            this._RedoNewItem = newItem;
        }

        

        

        public int Index
        {
            get { return _Index; }
        }

        public object OldItem
        {
            get { return _OldItem; }
        }

        public object NewItem
        {
            get { return _NewItem; }
        }

        

        

        public override void MergeWith(Change latestChange)
        {
            var other = latestChange as CollectionReplaceChange;

            if (null != other)
            {
                this._RedoIndex = other._RedoIndex;
                this._RedoNewItem = other._RedoNewItem;
            }
        }

        

        

        protected override void PerformUndo()
        {
            Collection[Index] = OldItem;
        }

        protected override void PerformRedo()
        {
            Collection[_RedoIndex] = _RedoNewItem;
        }

        private string DebuggerDisplay
        {
            get
            {
                return string.Format(
                    "CollectionReplaceChange(Property={0}, Target={{{1}}}, Value={2}, NewItem={{{3}}}, OldItem={{{4}}})",
                    PropertyName, Target, Index, NewItem, OldItem);
            }
        }

        
    }



}
