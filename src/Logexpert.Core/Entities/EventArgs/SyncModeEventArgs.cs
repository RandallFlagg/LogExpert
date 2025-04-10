namespace LogExpert.Core.Entities.EventArgs
{
    public class SyncModeEventArgs : System.EventArgs
    {
        #region Fields

        #endregion

        #region cTor

        public SyncModeEventArgs(bool isSynced)
        {
            IsTimeSynced = isSynced;
        }

        #endregion

        #region Properties

        public bool IsTimeSynced { get; }

        #endregion
    }
}