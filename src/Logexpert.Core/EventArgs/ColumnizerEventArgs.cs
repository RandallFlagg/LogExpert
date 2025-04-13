namespace LogExpert.Core.EventArgs
{
    public class ColumnizerEventArgs(ILogLineColumnizer columnizer) : System.EventArgs
    {
        #region Properties

        public ILogLineColumnizer Columnizer { get; } = columnizer;

        #endregion
    }
}