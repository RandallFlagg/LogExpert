using LogExpert.Controls.LogWindow;

namespace LogExpert.Entities.EventArgs
{
    public class FilterListChangedEventArgs
    {
        #region Fields

        #endregion

        #region cTor

        public FilterListChangedEventArgs(LogWindow logWindow)
        {
            LogWindow = logWindow;
        }

        #endregion

        #region Properties

        public LogWindow LogWindow { get; }

        #endregion
    }
}