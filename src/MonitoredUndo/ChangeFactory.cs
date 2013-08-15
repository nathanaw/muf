using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Reflection;

namespace MonitoredUndo
{

    public class ChangeFactory
    {

        public bool ThrowExceptionOnCollectionResets
        {
            get { return _ThrowExceptionOnCollectionResets; }
            set { _ThrowExceptionOnCollectionResets = value; }
        }
        private bool _ThrowExceptionOnCollectionResets = true;


        /// <summary>
        /// Construct a Change instance with actions for undo / redo.
        /// </summary>
        /// <param name="instance">The instance that changed.</param>
        /// <param name="propertyName">The property name that changed. (Case sensitive, used by reflection.)</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        /// <returns>A Change that can be added to the UndoRoot's undo stack.</returns>
        public virtual Change GetChange(object instance, string propertyName, object oldValue, object newValue)
        {
            var undoMetadata = instance as IUndoMetadata;
            if (null != undoMetadata)
            {
                if (!undoMetadata.CanUndoProperty(propertyName, oldValue, newValue))
                    return null;
            }

            var change = new PropertyChange(instance, propertyName, oldValue, newValue);

            return change;
        }

        /// <summary>
        /// Construct a Change instance with actions for undo / redo.
        /// </summary>
        /// <param name="instance">The instance that changed.</param>
        /// <param name="propertyName">The property name that changed. (Case sensitive, used by reflection.)</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        public virtual void OnChanging(object instance, string propertyName, object oldValue, object newValue)
        {
            OnChanging(instance, propertyName, oldValue, newValue, propertyName);
        }

        /// <summary>
        /// Construct a Change instance with actions for undo / redo.
        /// </summary>
        /// <param name="instance">The instance that changed.</param>
        /// <param name="propertyName">The property name that changed. (Case sensitive, used by reflection.)</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        /// <param name="descriptionOfChange">A description of this change.</param>
        public virtual void OnChanging(object instance, string propertyName, object oldValue, object newValue, string descriptionOfChange)
        {
            var supportsUndo = instance as ISupportsUndo;
            if (null == supportsUndo)
                return;

            var root = supportsUndo.GetUndoRoot();
            if (null == root)
                return;

            Change change = GetChange(instance, propertyName, oldValue, newValue);

            UndoService.Current[root].AddChange(change, descriptionOfChange);
        }

        /// <summary>
        /// Construct a Change instance with actions for undo / redo.
        /// </summary>
        /// <param name="instance">The instance that changed.</param>
        /// <param name="propertyName">The property name that exposes the collection that changed. (Case sensitive, used by reflection.)</param>
        /// <param name="collection">The collection that had an item added / removed.</param>
        /// <param name="e">The NotifyCollectionChangedEventArgs event args parameter, with info about the collection change.</param>
        /// <returns>A Change that can be added to the UndoRoot's undo stack.</returns>
        public virtual IList<Change> GetCollectionChange(object instance, string propertyName, object collection, NotifyCollectionChangedEventArgs e)
        {
            var undoMetadata = instance as IUndoMetadata;
            if (null != undoMetadata)
            {
                if (!undoMetadata.CanUndoCollectionChange(propertyName, collection, e))
                    return null;
            }

            var ret = new List<Change>();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        var change = new CollectionAddChange(instance, propertyName, (IList) collection,
                                                             e.NewStartingIndex, item);

                        ret.Add(change);
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        var change = new CollectionRemoveChange(instance, propertyName, (IList) collection,
                                                                e.OldStartingIndex, item);

                        ret.Add(change);
                    }

                    break;

#if !SILVERLIGHT
                case NotifyCollectionChangedAction.Move:
                    var moveChange = new CollectionMoveChange(instance, propertyName, (IList) collection,
                                                              e.NewStartingIndex,
                                                              e.OldStartingIndex);
                    ret.Add(moveChange);
                    break;
#endif
                case NotifyCollectionChangedAction.Replace:
                    // FIXME handle multi-item replace event
                    var replaceChange = new CollectionReplaceChange(instance, propertyName, (IList) collection,
                                                                    e.NewStartingIndex, e.OldItems[0], e.NewItems[0]);
                    ret.Add(replaceChange);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    if (ThrowExceptionOnCollectionResets)
                        throw new NotSupportedException("Undoing collection resets is not supported via the CollectionChanged event. The collection is already null, so the Undo system has no way to capture the set of elements that were previously in the collection.");
                    else
                        break;

                    //IList collectionClone = collection.GetType().GetConstructor(new Type[] { collection.GetType() }).Invoke(new object[] { collection }) as IList;

                    //var resetChange = new DelegateChange(
                    //                        instance,
                    //                        () =>
                    //                        {
                    //                            for (int i = 0; i < collectionClone.Count; i++) //for instead foreach to preserve the order
                    //                                ((IList)collection).Add(collectionClone[i]);
                    //                        },
                    //                        () => collection.GetType().GetMethod("Clear").Invoke(collection, null),
                    //                        new ChangeKey<object, string, object>(instance, propertyName, collectionClone)
                    //                    );
                    //ret.Add(resetChange);
                    //break;

                default:
                    throw new NotSupportedException();
            }

            return ret;
        }

        /// <summary>
        /// Construct a Change instance with actions for undo / redo.
        /// </summary>
        /// <param name="instance">The instance that changed.</param>
        /// <param name="propertyName">The property name that exposes the collection that changed. (Case sensitive, used by reflection.)</param>
        /// <param name="collection">The collection that had an item added / removed.</param>
        /// <param name="e">The NotifyCollectionChangedEventArgs event args parameter, with info about the collection change.</param>
        public virtual void OnCollectionChanged(object instance, string propertyName, object collection, NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(instance, propertyName, collection, e, propertyName);
        }

        /// <summary>
        /// Construct a Change instance with actions for undo / redo.
        /// </summary>
        /// <param name="instance">The instance that changed.</param>
        /// <param name="propertyName">The property name that exposes the collection that changed. (Case sensitive, used by reflection.)</param>
        /// <param name="collection">The collection that had an item added / removed.</param>
        /// <param name="e">The NotifyCollectionChangedEventArgs event args parameter, with info about the collection change.</param>
        /// <param name="descriptionOfChange">A description of the change.</param>
        public virtual void OnCollectionChanged(object instance, string propertyName, object collection, NotifyCollectionChangedEventArgs e, string descriptionOfChange)
        {
            var supportsUndo = instance as ISupportsUndo;
            if (null == supportsUndo)
                return;

            var root = supportsUndo.GetUndoRoot();
            if (null == root)
                return;

            // Create the Change instances.
            var changes = GetCollectionChange(instance, propertyName, collection, e);
            if (null == changes)
                return;

            // Add the changes to the UndoRoot
            var undoRoot = UndoService.Current[root];
            foreach (var change in changes)
            {
                undoRoot.AddChange(change, descriptionOfChange);
            }
        }
    }



}
