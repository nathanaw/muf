using System.Collections.Generic;
using System.Collections.Specialized;

namespace MonitoredUndo
{
    /// <summary>
    /// A factory for creating <see cref="Change"/> instances.
    /// </summary>
    public interface IChangeFactory
    {
        /// <summary>
        /// Whether to throw an exception when attempting to create a collection change for a collection that has been reset.
        /// </summary>
        bool ThrowExceptionOnCollectionResets { get; set; }

        /// <summary>
        /// Construct a Change instance with actions for undo / redo.
        /// </summary>
        /// <param name="instance">The instance that changed.</param>
        /// <param name="propertyName">The property name that changed. (Case sensitive, used by reflection.)</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        /// <returns>A Change that can be added to the UndoRoot's undo stack.</returns>
        Change GetChange(object instance, string propertyName, object oldValue, object newValue);

        /// <summary>
        /// Construct a Change instance with actions for undo / redo.
        /// </summary>
        /// <param name="instance">The instance that changed.</param>
        /// <param name="propertyName">The property name that exposes the collection that changed. (Case sensitive, used by reflection.)</param>
        /// <param name="collection">The collection that had an item added / removed.</param>
        /// <param name="e">The NotifyCollectionChangedEventArgs event args parameter, with info about the collection change.</param>
        /// <returns>A Change that can be added to the UndoRoot's undo stack.</returns>
        IList<Change> GetCollectionChange(object instance, string propertyName, object collection, NotifyCollectionChangedEventArgs e);

        /// <summary>
        /// Construct a Change instance with actions for undo / redo.
        /// </summary>
        /// <param name="instance">The instance that changed.</param>
        /// <param name="propertyName">The property name that changed. (Case sensitive, used by reflection.)</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        void OnChanging(object instance, string propertyName, object oldValue, object newValue);

        /// <summary>
        /// Construct a Change instance with actions for undo / redo.
        /// </summary>
        /// <param name="instance">The instance that changed.</param>
        /// <param name="propertyName">The property name that changed. (Case sensitive, used by reflection.)</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        /// <param name="descriptionOfChange">A description of this change.</param>
        void OnChanging(object instance, string propertyName, object oldValue, object newValue, string descriptionOfChange);

        /// <summary>
        /// Construct a Change instance with actions for undo / redo.
        /// </summary>
        /// <param name="instance">The instance that changed.</param>
        /// <param name="propertyName">The property name that exposes the collection that changed. (Case sensitive, used by reflection.)</param>
        /// <param name="collection">The collection that had an item added / removed.</param>
        /// <param name="e">The NotifyCollectionChangedEventArgs event args parameter, with info about the collection change.</param>
        void OnCollectionChanged(object instance, string propertyName, object collection, NotifyCollectionChangedEventArgs e);

        /// <summary>
        /// Construct a Change instance with actions for undo / redo.
        /// </summary>
        /// <param name="instance">The instance that changed.</param>
        /// <param name="propertyName">The property name that exposes the collection that changed. (Case sensitive, used by reflection.)</param>
        /// <param name="collection">The collection that had an item added / removed.</param>
        /// <param name="e">The NotifyCollectionChangedEventArgs event args parameter, with info about the collection change.</param>
        /// <param name="descriptionOfChange">A description of the change.</param>
        void OnCollectionChanged(object instance, string propertyName, object collection, NotifyCollectionChangedEventArgs e, string descriptionOfChange);
    }
}