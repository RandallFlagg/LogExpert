using LogExpert.Core.Classes;
using LogExpert.Core.Entities;

using System;
using System.Threading;
using System.Windows.Forms;

namespace LogExpert.Controls
{
    internal class LogTabPage : TabPage
    {
        #region Fields

        private const int DIFF_MAX = 100;
        private int _diffSum = 0;
        private readonly object _diffSumLock = new();
        private readonly Thread ledThread;
        private bool shouldStop = false;

        #endregion

        #region cTor

        public LogTabPage(LogWindow.LogWindow logWindow, string title)
            : base("MMi" + (title ?? Util.GetNameFromPath(logWindow.FileName)))
        {
            TabTitle = title;
            TabTitle ??= Util.GetNameFromPath(logWindow.FileName);
            LogWindow = logWindow;
            LogWindow.FileSizeChanged += FileSizeChanged;
            LogWindow.TailFollowed += TailFollowed;
            ledThread = new Thread(new ThreadStart(LedThreadProc));
            ledThread.IsBackground = true;
            ledThread.Start();
        }

        #endregion

        #region Properties

        public LogWindow.LogWindow LogWindow { get; }


        public int LineDiff
        {
            get
            {
                lock (_diffSumLock)
                {
                    return _diffSum;
                }
            }
        }

        public bool Dirty { get; set; } = false;

        public bool IsActiveTab { get; set; } = false;

        public string TabTitle { get; set; } = "";

        #endregion

        #region Public methods

        public void Delete()
        {
            shouldStop = true;
            ledThread.Interrupt();
            ledThread.Join();
        }

        #endregion

        #region Private Methods

        private void LedThreadProc()
        {
            while (!shouldStop)
            {
                try
                {
                    Thread.Sleep(200);
                }
                catch (Exception)
                {
                    return;
                }
                lock (_diffSumLock)
                {
                    if (_diffSum > 0)
                    {
                        _diffSum -= 10;
                        if (_diffSum < 0)
                        {
                            _diffSum = 0;
                        }

                        Parent?.Invalidate(); // redraw LEDs
                    }
                }
            }
        }

        #endregion

        #region Events handler

        private void FileSizeChanged(object sender, LogEventArgs e)
        {
            int diff = e.LineCount - e.PrevLineCount;
            if (diff < 0)
            {
                diff = DIFF_MAX;
                return;
            }
            lock (_diffSumLock)
            {
                _diffSum += diff;
                if (_diffSum > DIFF_MAX)
                {
                    _diffSum = DIFF_MAX;
                }
            }
            Dirty = true;
            Parent.Invalidate();
        }

        private void TailFollowed(object sender, EventArgs e)
        {
            if (IsActiveTab)
            {
                Dirty = false;
                Parent.Invalidate();
            }
        }

        #endregion
    }
}