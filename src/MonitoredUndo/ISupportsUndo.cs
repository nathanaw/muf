namespace MonitoredUndo
{

    /// <summary>
    /// Implement on objects that support undo.
    /// </summary>
    public interface ISupportsUndo
    {

        /// <summary>
        /// Gets the "root document" or "root object" that this instance is part of.
        /// Returning null from this method effectively disables undo support.
        /// </summary>
        /// <returns></returns>
        object GetUndoRoot();

    }

}
