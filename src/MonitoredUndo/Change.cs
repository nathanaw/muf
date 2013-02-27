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
    public abstract class Change
    {

        #region Member Variables

        private object _Target;
        private bool _Undone = false;
        private object _ChangeKey;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new change item.
        /// </summary>
        /// <param name="target">The object that this change affects.</param>
        /// <param name="changeKey">An object, that will be used to detect changes that affect the same "field". 
        /// This object should implement or override object.Equals() and return true if the changes are for the same field.
        /// This is used when the undo UndoRoot has started a batch, or when the UndoRoot.ConsolidateChangesForSameInstance is true.
        /// A string will work, but should be sufficiently unique within the scope of changes that affect this Target instance.
        /// Another good option is to use the Tuple<> class to uniquely identify the change. The Tuple could contain
        /// the object, and a string representing the property name. For a collection change, you might include the 
        /// instance, the property name, and the item added/removed from the collection.
        /// </param>
        protected Change(object target, object changeKey)
        {
            _Target = target; // new WeakReference(target);
            _ChangeKey = changeKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// A reference to the object that this change is for.
        /// </summary>
        public object Target { get { return _Target; } }

        /// <summary>
        /// The change "key" that uniquely identifies this instance. (see commends on the constructor.)
        /// </summary>
        public object ChangeKey { get { return _ChangeKey; } }

        /// <summary>
        /// Has this change been undone.
        /// </summary>
        public bool Undone { get { return _Undone; } }

        #endregion

        #region Public Methods

        /// <summary>
        /// When consolidating events, we want to keep the original (first) "Undo"
        /// but use the most recent Redo. This will pull the Redo from the 
        /// specified Change and apply it to this instance.
        /// </summary>
        public abstract void MergeWith(Change latestChange);

        #endregion

        #region Internal

        /// <summary>
        /// Apply the undo logic from this instance, and raise the ISupportsUndoNotification.UndoHappened event.
        /// </summary>
        internal void Undo()
        {
            PerformUndo();

            _Undone = true;

            var notify = Target as ISupportUndoNotification;
            if (null != notify)
                notify.UndoHappened(this);
        }

        /// <summary>
        /// Overridden in derived classes to contain the actual Undo logic.
        /// </summary>
        protected abstract void PerformUndo();

        /// <summary>
        /// Apply the redo logic from this instance, and raise the ISupportsUndoNotification.RedoHappened event.
        /// </summary>
        internal void Redo()
        {
            PerformRedo();

            _Undone = false;

            var notify = Target as ISupportUndoNotification;
            if (null != notify)
                notify.RedoHappened(this);
        }

        /// <summary>
        /// Overridden in derived classes to contain the actual Redo logic.
        /// </summary>
        protected abstract void PerformRedo();

        #endregion

    }

    /// <summary>
    /// Represents an individual change, with the commands to undo / redo the change as needed.
    /// </summary>
    public class DelegateChange : Change
    {

        #region Member Variables

        private Action _UndoAction;
        private Action _RedoAction;

        #endregion

        #region Constructors

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

        #endregion


        #region Public Methods

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

        #endregion

        #region Internal

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

        #endregion

    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class PropertyChange : Change
    {

        #region Member Variables

        private readonly string _PropertyName;
        // both should be weak
        private readonly object _OldValue;
        private object _NewValue;

        #endregion

        #region Constructors

        public PropertyChange(object instance, string propertyName, object oldValue, object newValue)
            : base(instance, new ChangeKey<object, string>(instance, propertyName))
        {
            this._PropertyName = propertyName;
            this._OldValue = oldValue;
            this._NewValue = newValue;
        }

        #endregion


        #region Public Properties

        public string PropertyName
        {
            get { return _PropertyName; }
        }

        public object OldValue
        {
            get { return _OldValue; }
        }

        public object NewValue
        {
            get { return _NewValue; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// When consolidating events, we want to keep the original "Undo"
        /// but use the most recent Redo. This will pull the Redo from the 
        /// specified Change and apply it to this instance.
        /// </summary>
        public override void MergeWith(Change latestChange)
        {
            var other = latestChange as PropertyChange;

            if (null != other)
                this._NewValue = other._NewValue;
        }

        #endregion

        #region Internal

        protected override void PerformUndo()
        {
            Target.GetType().GetProperty(PropertyName).SetValue(Target, OldValue, null);
        }

        protected override void PerformRedo()
        {
            Target.GetType().GetProperty(PropertyName).SetValue(Target, NewValue, null);
        }

        private string DebuggerDisplay
        {
            get
            {
                return string.Format(
                    "PropertyChange(Property={0}, Target={{{1}}}, NewValue={{{2}}}, OldValue={{{3}}})",
                    PropertyName, Target, NewValue, OldValue);
            }
        }

        #endregion

    }

    // TODO collection changes need more tests

    public abstract class CollectionChange : Change
    {
        #region Member Variables

        private readonly IList _Collection;
        private readonly string _PropertyName;

        #endregion

        #region Constructors

        protected CollectionChange(object target, string propertyName, IList collection, object changeKey)
            : base(target, changeKey)
        {
            this._PropertyName = propertyName;
            this._Collection = collection;
        }

        #endregion

        #region Properties

        public IList Collection
        {
            get { return _Collection; }
        }

        public string PropertyName
        {
            get { return _PropertyName; }
        }

        #endregion
    }

    public abstract class CollectionAddRemoveChangeBase : CollectionChange
    {
        
        #region Member Variables

        private readonly object _Element;
        private readonly int _Index;

        protected object _RedoElement;
        protected int _RedoIndex;

        #endregion


        #region Constructors

        public CollectionAddRemoveChangeBase(object target, string propertyName, IList collection, int index, object element)
            : base(target, propertyName, collection,
                   new ChangeKey<object, string, object>(target, propertyName, element))
        {
            this._Element = element;
            this._Index = index;

            this._RedoElement = element;
            this._RedoIndex = index;
        }

        #endregion

        #region Public Properties

        public object Element
        {
            get { return _Element; }
        }

        public int Index
        {
            get { return _Index; }
        }

        #endregion

        #region Public Methods

        public override void MergeWith(Change latestChange)
        {
            var other = latestChange as CollectionAddRemoveChangeBase;

            if (null != other)
            {
                this._RedoElement = other._RedoElement;
                this._RedoIndex = other._RedoIndex;
            }
        }

        #endregion

        #region Internal

        protected override void PerformUndo()
        {
            Collection.Remove(Element);
        }

        protected override void PerformRedo()
        {
            Collection.Insert(_RedoIndex, _RedoElement);
        }

        protected string DebuggerDisplay
        {
            get
            {
                return string.Format(
                    "{4}(Property={0}, Target={{{1}}}, Index={2}, Element={{{3}}})",
                    PropertyName, Target, Index, Element, GetType().Name);
            }
        }

        #endregion
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class CollectionAddChange : CollectionAddRemoveChangeBase
    {
        #region Member Variables

        #endregion


        #region Constructors

        public CollectionAddChange(object target, string propertyName, IList collection, int index, object element)
            : base(target, propertyName, collection, index, element) {}

        #endregion

        #region Internal

        protected override void PerformUndo()
        {
            Collection.Remove(Element);
        }

        protected override void PerformRedo()
        {
            Collection.Insert(_RedoIndex, _RedoElement);
        }

        #endregion
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class CollectionRemoveChange : CollectionAddRemoveChangeBase
    {
        #region Constructors

        public CollectionRemoveChange(object target, string propertyName, IList collection, int index, object element)
            : base(target, propertyName, collection, index, element) {}

        #endregion

        #region Internal

        protected override void PerformUndo()
        {
            Collection.Insert(Index, Element);
        }

        protected override void PerformRedo()
        {
            Collection.Remove(_RedoElement);
        }

        #endregion
    }

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

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class CollectionReplaceChange : CollectionChange
    {
        #region Member Variables

        private readonly int _Index;
        private readonly object _OldItem;
        private readonly object _NewItem;

        private int _RedoIndex;
        private object _RedoNewItem;

        #endregion

        #region Constructors

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

        #endregion

        #region Public Properties

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

        #endregion

        #region Public Methods

        public override void MergeWith(Change latestChange)
        {
            var other = latestChange as CollectionReplaceChange;

            if (null != other)
            {
                this._RedoIndex = other._RedoIndex;
                this._RedoNewItem = other._RedoNewItem;
            }
        }

        #endregion

        #region Internal

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
                    "CollectionReplaceChange(Property={0}, Target={{{1}}}, Index={2}, NewItem={{{3}}}, OldItem={{{4}}})",
                    PropertyName, Target, Index, NewItem, OldItem);
            }
        }

        #endregion
    }

}
