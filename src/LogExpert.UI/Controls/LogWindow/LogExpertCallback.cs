﻿using LogExpert.Classes.ILogLineColumnizerCallback;
using System.Collections.Generic;

namespace LogExpert.UI.Controls.LogWindow
{
    internal class LogExpertCallback(LogWindow logWindow) : ColumnizerCallback(logWindow), ILogExpertCallback
    {
        #region Public methods

        public void AddTempFileTab(string fileName, string title)
        {
            _logWindow.AddTempFileTab(fileName, title);
        }

        public void AddPipedTab(IList<LineEntry> lineEntryList, string title)
        {
            _logWindow.WritePipeTab(lineEntryList, title);
        }

        public string GetTabTitle()
        {
            return _logWindow.Text;
        }

        #endregion
    }
}
