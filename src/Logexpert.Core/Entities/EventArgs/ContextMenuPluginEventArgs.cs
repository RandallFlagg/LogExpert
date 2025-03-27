namespace LogExpert.Core.Entities.EventArgs
{
    public class ContextMenuPluginEventArgs : System.EventArgs
    {
        #region Fields

        #endregion

        #region cTor

        public ContextMenuPluginEventArgs(IContextMenuEntry entry, IList<int> logLines, ILogLineColumnizer columnizer,
            ILogExpertCallback callback)
        {
            Entry = entry;
            LogLines = logLines;
            Columnizer = columnizer;
            Callback = callback;
        }

        #endregion

        #region Properties

        public IContextMenuEntry Entry { get; }

        public IList<int> LogLines { get; }

        public ILogLineColumnizer Columnizer { get; }

        public ILogExpertCallback Callback { get; }

        #endregion
    }
}