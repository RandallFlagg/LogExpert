using LogExpert.Core.Config;

namespace LogExpert.Core.Entities.EventArgs
{
    public class ConfigChangedEventArgs : System.EventArgs
    {
        #region Fields

        #endregion

        #region cTor

        public ConfigChangedEventArgs(SettingsFlags changeFlags)
        {
            Flags = changeFlags;
        }

        #endregion

        #region Properties

        public SettingsFlags Flags { get; }

        #endregion
    }
}