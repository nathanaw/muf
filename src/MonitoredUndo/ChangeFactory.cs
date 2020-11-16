using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Reflection;
using MonitoredUndo.Changes;

namespace MonitoredUndo
{
    /// <inheritdoc cref="IChangeFactory"/>
    public class ChangeFactory : IChangeFactory
    {
        /// <inheritdoc cref="IChangeFactory.ThrowExceptionOnCollectionResets"/>
        public bool ThrowExceptionOnCollectionResets
        {
            get { return _ThrowExceptionOnCollectionResets; }
            set { _ThrowExceptionOnCollectionResets = value; }
        }
        private bool _ThrowExceptionOnCollectionResets = true;


        /// <inheritdoc cref="IChangeFactory.GetChange(object, string, object, object)"/>
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

        /// <inheritdoc cref="IChangeFactory.OnChanging(object, string, object, object)"/>
        public virtual void OnChanging(object instance, string propertyName, object oldValue, object newValue)
        {
            OnChanging(instance, propertyName, oldValue, newValue, propertyName);
        }

        /// <inheritdoc cref="IChangeFactory.OnChanging(object, string, object, object, string)"/>
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

        /// <inheritdoc cref="IChangeFactory.GetCollectionChange(object, string, object, NotifyCollectionChangedEventArgs)"/>
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
                        Change change = null;
                        if (collection as IList != null)
                        {
                            change = new CollectionAddChange(instance, propertyName, (IList)collection,
                            e.NewStartingIndex, item);
                        }
                        else if (collection as IDictionary != null)
                        {
                            // item is a key value pair - get key and value to be recorded in dictionary change
                            var keyProperty = item.GetType().GetProperty("Key");
                            var valueProperty = item.GetType().GetProperty("Value");
                            change = new DictionaryAddChange(instance, propertyName, (IDictionary)collection,
                                                                 keyProperty.GetValue(item, null), valueProperty.GetValue(item, null));
                        }
                        ret.Add(change);
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        Change change = null;
                        if (collection as IList != null)
                        {
                            change = new CollectionRemoveChange(instance, propertyName, (IList)collection,
                                                                    e.OldStartingIndex, item);
                        }
                        else if (collection as IDictionary != null)
                        {
                            // item is a key value pair - get key and value to be recorded in dictionary change
                            var keyProperty = item.GetType().GetProperty("Key");
                            var valueProperty = item.GetType().GetProperty("Value");
                            change = new DictionaryRemoveChange(instance, propertyName, (IDictionary)collection,
                                                                 keyProperty.GetValue(item, null), valueProperty.GetValue(item, null));
                        }
                        ret.Add(change);
                    }

                    break;

                case NotifyCollectionChangedAction.Move:
                    var moveChange = new CollectionMoveChange(instance, propertyName, (IList)collection,
                                                              e.NewStartingIndex,
                                                              e.OldStartingIndex);
                    ret.Add(moveChange);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        Change change = null;

                        if (collection as IList != null)
                        {
                            change = new CollectionReplaceChange(instance, propertyName, (IList)collection,
                                                                            e.NewStartingIndex, e.OldItems[i], e.NewItems[i]);
                        }
                        else if (collection as IDictionary != null)
                        {
                            // item is a key value pair - get key and value to be recorded in dictionary change
                            var keyProperty = e.OldItems[i].GetType().GetProperty("Key");
                            var oldValueProperty = e.OldItems[i].GetType().GetProperty("Value");
                            var newValueProperty = e.OldItems[i].GetType().GetProperty("Value");
                            change = new DictionaryReplaceChange(
                                instance, propertyName, (IDictionary)collection, keyProperty.GetValue(e.OldItems[i], null), oldValueProperty.GetValue(e.OldItems[i], null), newValueProperty.GetValue(e.NewItems[i], null));
                        }
                        ret.Add(change);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    if (ThrowExceptionOnCollectionResets)
                        throw new NotSupportedException("Undoing collection resets is not supported via the CollectionChanged event. The collection is already null, so the Undo system has no way to capture the set of elements that were previously in the collection.");
                    else
                        break;

                default:
                    throw new NotSupportedException();
            }

            return ret;
        }

        /// <inheritdoc cref="IChangeFactory.OnCollectionChanged(object, string, object, NotifyCollectionChangedEventArgs)"/>
        public virtual void OnCollectionChanged(object instance, string propertyName, object collection, NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(instance, propertyName, collection, e, propertyName);
        }

        /// <inheritdoc cref="IChangeFactory.OnCollectionChanged(object, string, object, NotifyCollectionChangedEventArgs, string)"/>
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
