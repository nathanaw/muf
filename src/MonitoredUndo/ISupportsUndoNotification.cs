namespace MonitoredUndo
{

    /// <summary>
    /// Implement on objects that want to be notified when something is undone or redone.
    /// </summary>
    public interface ISupportUndoNotification
    {

        void UndoHappened(Change change);

        void RedoHappened(Change change);

    }

}
