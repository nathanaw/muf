using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections;
using System.Globalization;

namespace MonitoredUndo
{

    public static class DefaultChangeFactory
    {

        /// <summary>
        /// Construct a Change instance with actions for undo / redo.
        /// </summary>
        /// <param name="instance">The instance that changed.</param>
        /// <param name="propertyName">The property name that changed. (Case sensitive, used by reflection.)</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        /// <returns>A Change that can be added to the UndoRoot's undo stack.</returns>
        public static Change GetChange(object instance, string propertyName, object oldValue, object newValue)
        {
            var undoMetadata = instance as IUndoMetadata;
            if (null != undoMetadata)
            {
                if (!undoMetadata.CanUndoProperty(propertyName, oldValue, newValue))
                    return null;
            }

            var change = new DelegateChange(
                                instance,
                                () =>
                                {
                                    instance.GetType().GetProperty(propertyName).SetValue(instance, oldValue, null);

                                },
                                () => instance.GetType().GetProperty(propertyName).SetValue(instance, newValue, null),
                                new ChangeKey<object, string>(instance, propertyName)
                            );

            return change;
        }

        /// <summary>
        /// Construct a Change instance with actions for undo / redo.
        /// </summary>
        /// <param name="instance">The instance that changed.</param>
        /// <param name="propertyName">The property name that changed. (Case sensitive, used by reflection.)</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        public static void OnChanging(object instance, string propertyName, object oldValue, object newValue)
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
        public static void OnChanging(object instance, string propertyName, object oldValue, object newValue, string descriptionOfChange)
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
        public static IList<Change> GetCollectionChange(object instance, string propertyName, object collection, NotifyCollectionChangedEventArgs e)
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
                        var change = new DelegateChange(
                                            instance,
                                            () => ((IList)collection).Remove(item),
                                            () => ((IList)collection).Add(item),
                                            new ChangeKey<object, string, object>(instance, propertyName, item)
                                        );

                        ret.Add(change);
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        var change = new DelegateChange(
                                            instance,
                                            () => ((IList)collection).Add(item),
                                            () => ((IList)collection).Remove(item),
                                            new ChangeKey<object, string, object>(instance, propertyName, item)
                                        );

                        ret.Add(change);
                    }

                    break;

#if !SILVERLIGHT
                case NotifyCollectionChangedAction.Move:
                    throw new NotSupportedException("Undoing collection index changes is not implemented.");
#endif

                case NotifyCollectionChangedAction.Replace:
                    throw new NotSupportedException("Undoing collection replace changes is not implemented.");

                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException("Undoing collection reset changes is not implemented.");

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
        public static void OnCollectionChanged(object instance, string propertyName, object collection, NotifyCollectionChangedEventArgs e)
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
        public static void OnCollectionChanged(object instance, string propertyName, object collection, NotifyCollectionChangedEventArgs e, string descriptionOfChange)
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
