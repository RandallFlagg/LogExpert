using LogExpert.Core.Interface;

namespace LogExpert.Core.Callback;

public class ColumnizerCallback : ILogLineColumnizerCallback, IAutoLogLineColumnizerCallback
{
    #region cTor

    public ColumnizerCallback (ILogWindow logWindow)
    {
        LogWindow = logWindow;
    }

    private ColumnizerCallback (ColumnizerCallback original)
    {
        LogWindow = original.LogWindow;
        LineNum = original.GetLineNum();
    }

    #endregion

    #region Properties

    public int LineNum { get; set; }

    protected ILogWindow LogWindow { get; set; }

    protected IPluginRegistry PluginRegistry { get; set; }

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
        return LogWindow.GetCurrentFileName(GetLineNum());
    }

    public ILogLine GetLogLine (int lineNum)
    {
        return LogWindow.GetLine(lineNum);
    }

    public IList<ILogLineColumnizer> GetRegisteredColumnizers ()
    {
        return PluginRegistry.RegisteredColumnizers;
    }

    public int GetLineCount ()
    {
        return LogWindow.LogFileReader.LineCount;
    }

    public void SetLineNum (int lineNum)
    {
        LineNum = lineNum;
    }

    #endregion
}
