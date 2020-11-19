namespace MonitoredUndo
{
    /// <summary>
    /// A service representing the top level of the undo / redo system.
    /// It contains one or more UndoRoots, accessible via an indexer.
    /// </summary>
    public interface IUndoService
    {
        /// <summary>
        /// Get (or create) an UndoRoot for the specified object or document instance.
        /// </summary>
        /// <param name="root">The object that represents the root of the document or object hierarchy.</param>
        /// <returns>An UndoRoot instance for this object.</returns>
        UndoRoot this[object root] { get; }

        /// <summary>
        /// Clear the cached UndoRoots.
        /// </summary>
        void Clear();
    }
}