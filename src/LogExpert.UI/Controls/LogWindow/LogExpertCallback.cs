using LogExpert.Core.Callback;

namespace LogExpert.UI.Controls.LogWindow;

internal class LogExpertCallback (LogWindow logWindow) : ColumnizerCallback(logWindow), ILogExpertCallback
{
    #region Public methods

    public void AddTempFileTab (string fileName, string title)
    {
        LogWindow.AddTempFileTab(fileName, title);
    }

    public void AddPipedTab (IList<LineEntry> lineEntryList, string title)
    {
        LogWindow.WritePipeTab(lineEntryList, title);
    }

    public string GetTabTitle ()
    {
        return LogWindow.Text;
    }

    #endregion
}
