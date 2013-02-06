using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections;

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
        /// <returns>A Change that can be added to the UndoRoot's undo stack.</returns>
        public static void OnChanging(object instance, string propertyName, object oldValue, object newValue)
        {
            var supportsUndo = instance as ISupportsUndo;
            if (null == supportsUndo)
                return;

            var root = supportsUndo.GetUndoRoot();
            if (null == root)
                return;

            Change change = GetChange(instance, propertyName, oldValue, newValue);

            UndoService.Current[root].AddChange(change, propertyName);
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

                case NotifyCollectionChangedAction.Move:
                    throw new NotSupportedException("Undoing collection index changes is not implemented.");

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
        /// <returns>A Change that can be added to the UndoRoot's undo stack.</returns>
        public static void OnCollectionChanged(object instance, string propertyName, object collection, NotifyCollectionChangedEventArgs e)
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
                undoRoot.AddChange(change, propertyName);
            }
        }

    }

    public class ChangeKey<T1, T2>
    {
        private T1 m_One;
        private T2 m_Two;

        public T1 Item1 { get { return m_One; } }
        public T2 Item2 { get { return m_Two; } }

        public ChangeKey(T1 item1, T2 item2)
        {
            m_One = item1;
            m_Two = item2;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            ChangeKey<T1, T2> tuple = obj as ChangeKey<T1, T2>;
            if (tuple == null)
            {
                return false;
            }
            if (object.Equals(this.m_One, tuple.m_One))
            {
                return object.Equals(this.m_Two, tuple.m_Two);
            }
            return false;            
        }

        public override int GetHashCode()
        {
            return CombineHashCodes(m_One.GetHashCode(), m_Two.GetHashCode());
        }

        public override string ToString()
        {
            return string.Format("Tuple of '{0}', '{1}'", m_One, m_Two);
        }


        internal static int CombineHashCodes(int h1, int h2)
        {
            return ((h1 << 5) + h1) ^ h2;
        }
    
    }

    public class ChangeKey<T1, T2, T3>
    {
        private T1 m_One;
        private T2 m_Two;
        private T3 m_Three;

        public T1 Item1 { get { return m_One; } }
        public T2 Item2 { get { return m_Two; } }
        public T3 Item3 { get { return m_Three; } }

        public ChangeKey(T1 item1, T2 item2, T3 item3)
        {
            m_One = item1;
            m_Two = item2;
            m_Three = item3;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            ChangeKey<T1, T2, T3> tuple = obj as ChangeKey<T1, T2, T3>;
            if (tuple == null)
            {
                return false;
            }
            if (object.Equals(this.m_One, tuple.m_One))
            {
                if (object.Equals(this.m_Two, tuple.m_Two))
                {
                    return object.Equals(this.m_Three, tuple.m_Three);
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return CombineHashCodes(m_One.GetHashCode(), CombineHashCodes(m_Two.GetHashCode(), m_Three.GetHashCode()));
        }

        public override string ToString()
        {
            return string.Format("Tuple of '{0}', '{1}', '{2}'", m_One, m_Two, m_Three);
        }

        internal static int CombineHashCodes(int h1, int h2)
        {
            return ((h1 << 5) + h1) ^ h2;
        }

    }

}
