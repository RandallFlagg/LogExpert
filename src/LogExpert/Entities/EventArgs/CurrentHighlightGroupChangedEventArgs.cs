using LogExpert.Controls.LogWindow;
using LogExpert.Core.Entities;

namespace LogExpert.Entities.EventArgs
{
    public class CurrentHighlightGroupChangedEventArgs(LogWindow logWindow, HilightGroup currentGroup)
    {
        #region Properties

        public LogWindow LogWindow { get; } = logWindow;

        public HilightGroup CurrentGroup { get; } = currentGroup;

        #endregion
    }
}