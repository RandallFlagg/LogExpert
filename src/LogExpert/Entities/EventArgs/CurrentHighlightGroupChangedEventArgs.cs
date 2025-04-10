using LogExpert.Controls.LogWindow;
using LogExpert.Core.Entities;

namespace LogExpert.Entities.EventArgs
{
    public class CurrentHighlightGroupChangedEventArgs
    {
        #region Fields

        #endregion

        #region cTor

        public CurrentHighlightGroupChangedEventArgs(LogWindow logWindow, HilightGroup currentGroup)
        {
            LogWindow = logWindow;
            CurrentGroup = currentGroup;
        }

        #endregion

        #region Properties

        public LogWindow LogWindow { get; }

        public HilightGroup CurrentGroup { get; }

        #endregion
    }
}