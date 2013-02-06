using System.Collections.Specialized;

namespace MonitoredUndo
{

    /// <summary>
    /// Can be used by undo services to determine whether the specified field or 
    /// collection item should be tracked for undo.
    /// Useful when the undo changes are created by a consolidated helper class.
    /// </summary>
    public interface IUndoMetadata
    {

        /// <summary>
        /// Can be used by undo services to determine whether the specified field can be undone.
        /// Useful when the undo changes are created by a consolidated helper class.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        /// <param name="oldValue">The original value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        /// <returns>A boolean, indicating whether the field should be tracked for undo / redo.</returns>
        bool CanUndoProperty(string propertyName, object oldValue, object newValue);

        /// <summary>
        /// Can be used by undo services to determine whether the specified field can be undone.
        /// Useful when the undo changes are created by a consolidated helper class.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        /// <param name="collection">The instance of the collection that had items added or removed.</param>
        /// <param name="args">The INotifyCollectionChanged event args that include details on the type 
        /// of collection operation.</param>
        /// <returns>A boolean, indicating whether the collection item should be tracked for undo / redo.</returns>
        bool CanUndoCollectionChange(string propertyName, object collection, NotifyCollectionChangedEventArgs args);

    }

}
