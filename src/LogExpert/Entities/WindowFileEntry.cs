﻿using LogExpert.Controls.LogWindow;

namespace LogExpert.Entities
{
    /// <summary>
    /// Represents a log file and its window. Used as a kind of handle for menus or list of open files.
    /// </summary>
    internal class WindowFileEntry
    {
        #region Fields

        private const int MAX_LEN = 40;

        #endregion

        #region cTor

        public WindowFileEntry(LogWindow logWindow)
        {
            LogWindow = logWindow;
        }

        #endregion

        #region Properties

        public string Title
        {
            get
            {
                string title = LogWindow.Text;
                if (title.Length > MAX_LEN)
                {
                    title = "..." + title.Substring(title.Length - MAX_LEN);
                }
                return title;
            }
        }

        public string FileName => LogWindow.FileName;


        public LogWindow LogWindow { get; }

        #endregion
    }
}