using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;

using LogExpert.Core.Classes.Columnizer;
using LogExpert.Core.Classes.Filter;
using LogExpert.Core.Config;
using LogExpert.Core.Entities;
using LogExpert.Core.Interface;
using LogExpert.Dialogs;
using LogExpert.UI.Entities;
using LogExpert.UI.Extensions;

using WeifenLuo.WinFormsUI.Docking;

namespace LogExpert.UI.Controls.LogTabWindow;

public partial class LogTabWindow
{
    #region Public methods

    [SupportedOSPlatform("windows")]
    public LogWindow.LogWindow AddTempFileTab (string fileName, string title)
    {
        return AddFileTab(fileName, true, title, false, null);
    }

    [SupportedOSPlatform("windows")]
    public LogWindow.LogWindow AddFilterTab (FilterPipe pipe, string title, ILogLineColumnizer preProcessColumnizer)
    {
        LogWindow.LogWindow logWin = AddFileTab(pipe.FileName, true, title, false, preProcessColumnizer);
        if (pipe.FilterParams.SearchText.Length > 0)
        {
            ToolTip tip = new(components);

            tip.SetToolTip(logWin,
                "Filter: \"" + pipe.FilterParams.SearchText + "\"" +
                (pipe.FilterParams.IsInvert ? " (Invert match)" : "") +
                (pipe.FilterParams.ColumnRestrict ? "\nColumn restrict" : "")
            );

            tip.AutomaticDelay = 10;
            tip.AutoPopDelay = 5000;
            var data = logWin.Tag as LogWindowData;
            data.ToolTip = tip;
        }

        return logWin;
    }

    [SupportedOSPlatform("windows")]
    public LogWindow.LogWindow AddFileTabDeferred (string givenFileName, bool isTempFile, string title, bool forcePersistenceLoading, ILogLineColumnizer preProcessColumnizer)
    {
        return AddFileTab(givenFileName, isTempFile, title, forcePersistenceLoading, preProcessColumnizer, true);
    }

    [SupportedOSPlatform("windows")]
    public LogWindow.LogWindow AddFileTab (string givenFileName, bool isTempFile, string title, bool forcePersistenceLoading, ILogLineColumnizer preProcessColumnizer, bool doNotAddToDockPanel = false)
    {
        var logFileName = FindFilenameForSettings(givenFileName);
        LogWindow.LogWindow win = FindWindowForFile(logFileName);
        if (win != null)
        {
            if (!isTempFile)
            {
                AddToFileHistory(givenFileName);
            }

            SelectTab(win);
            return win;
        }

        EncodingOptions encodingOptions = new();
        FillDefaultEncodingFromSettings(encodingOptions);
        LogWindow.LogWindow logWindow = new(this, logFileName, isTempFile, forcePersistenceLoading, ConfigManager);

        logWindow.GivenFileName = givenFileName;

        if (preProcessColumnizer != null)
        {
            logWindow.ForceColumnizerForLoading(preProcessColumnizer);
        }

        if (isTempFile)
        {
            logWindow.TempTitleName = title;
            encodingOptions.Encoding = new UnicodeEncoding(false, false);
        }

        AddLogWindow(logWindow, title, doNotAddToDockPanel);
        if (!isTempFile)
        {
            AddToFileHistory(givenFileName);
        }

        var data = logWindow.Tag as LogWindowData;
        data.Color = _defaultTabColor;
        SetTabColor(logWindow, _defaultTabColor);
        //data.tabPage.BorderColor = this.defaultTabBorderColor;
        if (!isTempFile)
        {
            foreach (ColorEntry colorEntry in ConfigManager.Settings.FileColors)
            {
                if (colorEntry.FileName.ToUpperInvariant().Equals(logFileName.ToUpperInvariant(), StringComparison.Ordinal))
                {
                    data.Color = colorEntry.Color;
                    SetTabColor(logWindow, colorEntry.Color);
                    break;
                }
            }
        }

        if (!isTempFile)
        {
            SetTooltipText(logWindow, logFileName);
        }

        if (givenFileName.EndsWith(".lxp", StringComparison.Ordinal))
        {
            logWindow.ForcedPersistenceFileName = givenFileName;
        }

        // this.BeginInvoke(new LoadFileDelegate(logWindow.LoadFile), new object[] { logFileName, encoding });
        Task.Run(() => logWindow.LoadFile(logFileName, encodingOptions));
        return logWindow;
    }

    [SupportedOSPlatform("windows")]
    public LogWindow.LogWindow AddMultiFileTab (string[] fileNames)
    {
        if (fileNames.Length < 1)
        {
            return null;
        }

        LogWindow.LogWindow logWindow = new(this, fileNames[^1], false, false, ConfigManager);
        AddLogWindow(logWindow, fileNames[^1], false);
        multiFileToolStripMenuItem.Checked = true;
        multiFileEnabledStripMenuItem.Checked = true;
        EncodingOptions encodingOptions = new();
        FillDefaultEncodingFromSettings(encodingOptions);
        BeginInvoke(new LoadMultiFilesDelegate(logWindow.LoadFilesAsMulti), fileNames, encodingOptions);
        AddToFileHistory(fileNames[0]);
        return logWindow;
    }

    [SupportedOSPlatform("windows")]
    public void LoadFiles (string[] fileNames)
    {
        Invoke(new AddFileTabsDelegate(AddFileTabs), [fileNames]);
    }

    [SupportedOSPlatform("windows")]
    public void OpenSearchDialog ()
    {
        if (CurrentLogWindow == null)
        {
            return;
        }

        SearchDialog dlg = new();
        AddOwnedForm(dlg);
        dlg.TopMost = TopMost;
        SearchParams.HistoryList = ConfigManager.Settings.SearchHistoryList;
        dlg.SearchParams = SearchParams;
        DialogResult res = dlg.ShowDialog();
        if (res == DialogResult.OK && dlg.SearchParams != null && !string.IsNullOrWhiteSpace(dlg.SearchParams.SearchText))
        {
            SearchParams = dlg.SearchParams;
            SearchParams.IsFindNext = false;
            CurrentLogWindow.StartSearch();
        }
    }

    public ILogLineColumnizer GetColumnizerHistoryEntry (string fileName)
    {
        ColumnizerHistoryEntry entry = FindColumnizerHistoryEntry(fileName);
        if (entry != null)
        {
            foreach (ILogLineColumnizer columnizer in PluginRegistry.PluginRegistry.Instance.RegisteredColumnizers)
            {
                if (columnizer.GetName().Equals(entry.ColumnizerName, StringComparison.Ordinal))
                {
                    return columnizer;
                }
            }

            ConfigManager.Settings.ColumnizerHistoryList.Remove(entry); // no valid name -> remove entry
        }

        return null;
    }

    public void SwitchTab (bool shiftPressed)
    {
        var index = dockPanel.Contents.IndexOf(dockPanel.ActiveContent);
        if (shiftPressed)
        {
            index--;
            if (index < 0)
            {
                index = dockPanel.Contents.Count - 1;
            }

            if (index < 0)
            {
                return;
            }
        }
        else
        {
            index++;
            if (index >= dockPanel.Contents.Count)
            {
                index = 0;
            }
        }

        if (index < dockPanel.Contents.Count)
        {
            (dockPanel.Contents[index] as DockContent).Activate();
        }
    }

    public void ScrollAllTabsToTimestamp (DateTime timestamp, LogWindow.LogWindow senderWindow)
    {
        lock (_logWindowList)
        {
            foreach (LogWindow.LogWindow logWindow in _logWindowList)
            {
                if (logWindow != senderWindow)
                {
                    if (logWindow.ScrollToTimestamp(timestamp, false, false))
                    {
                        ShowLedPeak(logWindow);
                    }
                }
            }
        }
    }

    public ILogLineColumnizer FindColumnizerByFileMask (string fileName)
    {
        foreach (ColumnizerMaskEntry entry in ConfigManager.Settings.Preferences.ColumnizerMaskList)
        {
            if (entry.Mask != null)
            {
                try
                {
                    if (Regex.IsMatch(fileName, entry.Mask))
                    {
                        ILogLineColumnizer columnizer = ColumnizerPicker.FindColumnizerByName(entry.ColumnizerName, PluginRegistry.PluginRegistry.Instance.RegisteredColumnizers);
                        return columnizer;
                    }
                }
                catch (ArgumentException e)
                {
                    _logger.Error(e, "RegEx-error while finding columnizer: ");
                    // occurs on invalid regex patterns
                }
            }
        }

        return null;
    }

    public HighlightGroup FindHighlightGroupByFileMask (string fileName)
    {
        foreach (HighlightMaskEntry entry in ConfigManager.Settings.Preferences.HighlightMaskList)
        {
            if (entry.Mask != null)
            {
                try
                {
                    if (Regex.IsMatch(fileName, entry.Mask))
                    {
                        HighlightGroup group = FindHighlightGroup(entry.HighlightGroupName);
                        return group;
                    }
                }
                catch (ArgumentException e)
                {
                    _logger.Error(e, "RegEx-error while finding columnizer: ");
                    // occurs on invalid regex patterns
                }
            }
        }

        return null;
    }

    public void SelectTab (ILogWindow logWindow)
    {
        logWindow.Activate();
    }

    [SupportedOSPlatform("windows")]
    public void SetForeground ()
    {
        NativeMethods.SetForegroundWindow(Handle);
        if (WindowState == FormWindowState.Minimized)
        {
            if (_wasMaximized)
            {
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                WindowState = FormWindowState.Normal;
            }
        }
    }

    // called from LogWindow when follow tail was changed
    [SupportedOSPlatform("windows")]
    public void FollowTailChanged (LogWindow.LogWindow logWindow, bool isEnabled, bool offByTrigger)
    {
        if (logWindow.Tag is not LogWindowData data)
        {
            return;
        }

        if (isEnabled)
        {
            data.TailState = 0;
        }
        else
        {
            data.TailState = offByTrigger ? 2 : 1;
        }

        if (Preferences.ShowTailState)
        {
            Icon icon = GetIcon(data.DiffSum, data);
            BeginInvoke(new SetTabIconDelegate(SetTabIcon), logWindow, icon);
        }
    }

    [SupportedOSPlatform("windows")]
    public void NotifySettingsChanged (object sender, SettingsFlags flags)
    {
        if (sender != this)
        {
            NotifyWindowsForChangedPrefs(flags);
        }
    }

    public IList<WindowFileEntry> GetListOfOpenFiles ()
    {
        IList<WindowFileEntry> list = [];
        lock (_logWindowList)
        {
            foreach (LogWindow.LogWindow logWindow in _logWindowList)
            {
                list.Add(new WindowFileEntry(logWindow));
            }
        }

        return list;
    }

    #endregion
}