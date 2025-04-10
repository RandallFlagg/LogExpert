using LogExpert.Core.Interface;

namespace LogExpert.Core.Entities
{
    public class FileViewContext
    {
        #region Fields

        #endregion

        #region cTor

        public FileViewContext(ILogPaintContext logPaintContext, ILogView logView)
        {
            LogPaintContext = logPaintContext;
            LogView = logView;
        }

        #endregion

        #region Properties

        public ILogPaintContext LogPaintContext { get; }

        public ILogView LogView { get; }

        #endregion
    }
}