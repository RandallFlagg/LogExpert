
using LogExpert.Core.Classes.Log;
using LogExpert.Core.Classes.Persister;
using LogExpert.Core.Entities;
using LogExpert.Core.EventHandlers;
using static LogExpert.Core.Classes.Log.LogfileReader;

namespace LogExpert.Core.Interface
{
    //TODO: Add documentation
    public interface ILogWindow
    {
        string GetCurrentFileName(int lineNum);
        ILogLine GetLine(int lineNum);
        DateTime GetTimestampForLineForward(ref int lineNum, bool v);
        DateTime GetTimestampForLine(ref int lastLineNum, bool v);
        int FindTimestampLine_Internal(int lineNum1, int lineNum2, int lastLineNum, DateTime searchTimeStamp, bool v);
        void SelectLine(int lineNum, bool v1, bool v2);
        PersistenceData GetPersistenceData();
        void AddTempFileTab(string fileName, string title);
        void WritePipeTab(IList<LineEntry> lineEntryList, string title);
        void Activate();
        LogfileReader _logFileReader { get; }
        string Text { get; }
        string FileName { get; }
        event FileSizeChangedEventHandler FileSizeChanged; //TODO: All handlers should be moved to Core
        event EventHandler TailFollowed;
        //LogExpert.UI.Controls.LogTabWindow.LogTabWindow.LogWindowData Tag { get; }
    }
}