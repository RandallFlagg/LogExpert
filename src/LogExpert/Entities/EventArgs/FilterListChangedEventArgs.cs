using LogExpert.Controls.LogWindow;

namespace LogExpert.Entities.EventArgs
{
    public class FilterListChangedEventArgs(LogWindow logWindow)
    {
        #region Properties

        public LogWindow LogWindow { get; } = logWindow;

        #endregion
    }
}