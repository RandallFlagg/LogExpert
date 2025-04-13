namespace LogExpert.Core.EventArgs
{
    public class SelectLineEventArgs(int line) : System.EventArgs
    {
        #region Properties

        public int Line { get; } = line;

        #endregion
    }
}