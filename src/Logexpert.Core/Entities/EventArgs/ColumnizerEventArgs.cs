namespace LogExpert.Core.Entities.EventArgs
{
    public class ColumnizerEventArgs : System.EventArgs
    {
        #region Fields

        #endregion

        #region cTor

        public ColumnizerEventArgs(ILogLineColumnizer columnizer)
        {
            Columnizer = columnizer;
        }

        #endregion

        #region Properties

        public ILogLineColumnizer Columnizer { get; }

        #endregion
    }
}