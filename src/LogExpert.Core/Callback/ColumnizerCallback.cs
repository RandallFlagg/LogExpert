using LogExpert.Core.Interface;

namespace LogExpert.Classes.ILogLineColumnizerCallback
{
    public class ColumnizerCallback : LogExpert.ILogLineColumnizerCallback, IAutoLogLineColumnizerCallback
    {
        #region Fields

        private protected ILogWindow _logWindow;
        private protected IPluginRegistry _pluginRegistry;

        #endregion

        #region cTor

        public ColumnizerCallback (ILogWindow logWindow)
        {
            _logWindow = logWindow;
        }

        private ColumnizerCallback (ColumnizerCallback original)
        {
            _logWindow = original._logWindow;
            LineNum = original.GetLineNum();
        }

        #endregion

        #region Properties

        public int LineNum { get; set; }

        #endregion

        #region Public methods

        public ColumnizerCallback CreateCopy ()
        {
            return new ColumnizerCallback(this);
        }

        public int GetLineNum ()
        {
            return LineNum;
        }

        public string GetFileName ()
        {
            return _logWindow.GetCurrentFileName(GetLineNum());
        }

        public ILogLine GetLogLine (int lineNum)
        {
            return _logWindow.GetLine(lineNum);
        }

        public IList<ILogLineColumnizer> GetRegisteredColumnizers ()
        {
            return _pluginRegistry.RegisteredColumnizers;
        }

        public int GetLineCount ()
        {
            return _logWindow.LogFileReader.LineCount;
        }

        public void SetLineNum (int lineNum)
        {
            LineNum = lineNum;
        }

        #endregion
    }
}
