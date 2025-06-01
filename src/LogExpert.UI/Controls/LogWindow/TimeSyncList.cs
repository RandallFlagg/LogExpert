namespace LogExpert.UI.Controls.LogWindow;

/// <summary>
/// Holds all windows which are in sync via timestamp
/// </summary>
public class TimeSyncList
{
    #region Fields

    private readonly IList<LogWindow> logWindowList = new List<LogWindow>();

    #endregion

    #region Delegates

    public delegate void WindowRemovedEventHandler (object sender, EventArgs e);

    #endregion

    #region Events

    public event WindowRemovedEventHandler WindowRemoved;

    #endregion

    #region Properties

    public DateTime CurrentTimestamp { get; set; }

    public int Count => logWindowList.Count;

    #endregion

    #region Public methods

    public void AddWindow (LogWindow logWindow)
    {
        lock (logWindowList)
        {
            if (!logWindowList.Contains(logWindow))
            {
                logWindowList.Add(logWindow);
            }
        }
    }


    public void RemoveWindow (LogWindow logWindow)
    {
        lock (logWindowList)
        {
            logWindowList.Remove(logWindow);
        }

        OnWindowRemoved();
    }


    /// <summary>
    /// Scrolls all LogWindows to the given timestamp
    /// </summary>
    /// <param name="timestamp"></param>
    /// <param name="sender"></param>
    public void NavigateToTimestamp (DateTime timestamp, LogWindow sender)
    {
        CurrentTimestamp = timestamp;
        lock (logWindowList)
        {
            foreach (LogWindow logWindow in logWindowList)
            {
                if (sender != logWindow)
                {
                    logWindow.ScrollToTimestamp(timestamp, false, false);
                }
            }
        }
    }


    public bool Contains (LogWindow logWindow)
    {
        return logWindowList.Contains(logWindow);
    }

    #endregion

    #region Private Methods

    private void OnWindowRemoved ()
    {
        WindowRemoved?.Invoke(this, new EventArgs());
    }

    #endregion
}