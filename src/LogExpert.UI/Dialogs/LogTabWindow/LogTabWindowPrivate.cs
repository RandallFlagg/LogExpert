using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security;
using System.Text;

using LogExpert.Core.Classes;
using LogExpert.Core.Classes.Columnizer;
using LogExpert.Core.Classes.Persister;
using LogExpert.Core.Config;
using LogExpert.Core.Entities;
using LogExpert.Core.Enums;
using LogExpert.Core.EventArguments;
using LogExpert.Dialogs;
using LogExpert.PluginRegistry.FileSystem;
using LogExpert.UI.Dialogs;
using LogExpert.UI.Entities;
using LogExpert.UI.Extensions;

using WeifenLuo.WinFormsUI.Docking;

namespace LogExpert.UI.Controls.LogTabWindow;

public partial class LogTabWindow
{
    #region Private Methods

    /// <summary>
    /// Creates a temp file with the text content of the clipboard and opens the temp file in a new tab.
    /// </summary>
    [SupportedOSPlatform("windows")]
    private void PasteFromClipboard ()
    {
        if (Clipboard.ContainsText())
        {
            var text = Clipboard.GetText();
            var fileName = Path.GetTempFileName();

            using (FileStream fStream = new(fileName, FileMode.Append, FileAccess.Write, FileShare.Read))
            using (StreamWriter writer = new(fStream, Encoding.Unicode))
            {
                writer.Write(text);
                writer.Close();
            }

            var title = "Clipboard";
            LogWindow.LogWindow logWindow = AddTempFileTab(fileName, title);
            if (logWindow.Tag is LogWindowData data)
            {
                SetTooltipText(logWindow, "Pasted on " + DateTime.Now);
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void InitToolWindows ()
    {
        InitBookmarkWindow();
    }

    [SupportedOSPlatform("windows")]
    private void DestroyToolWindows ()
    {
        DestroyBookmarkWindow();
    }

    [SupportedOSPlatform("windows")]
    private void InitBookmarkWindow ()
    {
        _bookmarkWindow = new BookmarkWindow
        {
            HideOnClose = true,
            ShowHint = DockState.DockBottom
        };

        _bookmarkWindow.PreferencesChanged(ConfigManager.Settings.Preferences, false, SettingsFlags.All, ConfigManager.Instance);
        _bookmarkWindow.VisibleChanged += OnBookmarkWindowVisibleChanged;
        _firstBookmarkWindowShow = true;
    }

    [SupportedOSPlatform("windows")]
    private void DestroyBookmarkWindow ()
    {
        _bookmarkWindow.HideOnClose = false;
        _bookmarkWindow.Close();
    }

    private void SaveLastOpenFilesList ()
    {
        ConfigManager.Settings.LastOpenFilesList.Clear();
        foreach (DockContent content in dockPanel.Contents)
        {
            if (content is LogWindow.LogWindow logWin)
            {
                if (!logWin.IsTempFile)
                {
                    ConfigManager.Settings.LastOpenFilesList.Add(logWin.GivenFileName);
                }
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void SaveWindowPosition ()
    {
        SuspendLayout();
        if (WindowState == FormWindowState.Normal)
        {
            ConfigManager.Settings.AppBounds = Bounds;
            ConfigManager.Settings.IsMaximized = false;
        }
        else
        {
            ConfigManager.Settings.AppBoundsFullscreen = Bounds;
            ConfigManager.Settings.IsMaximized = true;
            WindowState = FormWindowState.Normal;
            ConfigManager.Settings.AppBounds = Bounds;
        }

        ResumeLayout();
    }

    private void SetTooltipText (LogWindow.LogWindow logWindow, string logFileName)
    {
        logWindow.ToolTipText = logFileName;
    }

    private void FillDefaultEncodingFromSettings (EncodingOptions encodingOptions)
    {
        if (ConfigManager.Settings.Preferences.DefaultEncoding != null)
        {
            try
            {
                encodingOptions.DefaultEncoding = Encoding.GetEncoding(ConfigManager.Settings.Preferences.DefaultEncoding);
            }
            catch (ArgumentException)
            {
                _logger.Warn("Encoding " + ConfigManager.Settings.Preferences.DefaultEncoding + " is not a valid encoding");
                encodingOptions.DefaultEncoding = null;
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void AddFileTabs (string[] fileNames)
    {
        foreach (var fileName in fileNames)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                if (fileName.EndsWith(".lxj"))
                {
                    LoadProject(fileName, false);
                }
                else
                {
                    AddFileTab(fileName, false, null, false, null);
                }
            }
        }

        Activate();
    }

    [SupportedOSPlatform("windows")]
    private void AddLogWindow (LogWindow.LogWindow logWindow, string title, bool doNotAddToPanel)
    {
        logWindow.CloseButton = true;
        logWindow.TabPageContextMenuStrip = tabContextMenuStrip;
        SetTooltipText(logWindow, title);
        logWindow.DockAreas = DockAreas.Document | DockAreas.Float;

        if (!doNotAddToPanel)
        {
            logWindow.Show(dockPanel);
        }

        LogWindowData data = new()
        {
            DiffSum = 0
        };

        logWindow.Tag = data;

        lock (_logWindowList)
        {
            _logWindowList.Add(logWindow);
        }

        logWindow.FileSizeChanged += OnFileSizeChanged;
        logWindow.TailFollowed += OnTailFollowed;
        logWindow.Disposed += OnLogWindowDisposed;
        logWindow.FileNotFound += OnLogWindowFileNotFound;
        logWindow.FileRespawned += OnLogWindowFileRespawned;
        logWindow.FilterListChanged += OnLogWindowFilterListChanged;
        logWindow.CurrentHighlightGroupChanged += OnLogWindowCurrentHighlightGroupChanged;
        logWindow.SyncModeChanged += OnLogWindowSyncModeChanged;

        logWindow.Visible = true;
    }

    [SupportedOSPlatform("windows")]
    private void DisconnectEventHandlers (LogWindow.LogWindow logWindow)
    {
        logWindow.FileSizeChanged -= OnFileSizeChanged;
        logWindow.TailFollowed -= OnTailFollowed;
        logWindow.Disposed -= OnLogWindowDisposed;
        logWindow.FileNotFound -= OnLogWindowFileNotFound;
        logWindow.FileRespawned -= OnLogWindowFileRespawned;
        logWindow.FilterListChanged -= OnLogWindowFilterListChanged;
        logWindow.CurrentHighlightGroupChanged -= OnLogWindowCurrentHighlightGroupChanged;
        logWindow.SyncModeChanged -= OnLogWindowSyncModeChanged;

        var data = logWindow.Tag as LogWindowData;
        //data.tabPage.MouseClick -= tabPage_MouseClick;
        //data.tabPage.TabDoubleClick -= tabPage_TabDoubleClick;
        //data.tabPage.ContextMenuStrip = null;
        //data.tabPage = null;
    }

    [SupportedOSPlatform("windows")]
    private void AddToFileHistory (string fileName)
    {
        bool FindName (string s) => s.ToUpperInvariant().Equals(fileName.ToUpperInvariant(), StringComparison.Ordinal);

        var index = ConfigManager.Settings.FileHistoryList.FindIndex(FindName);

        if (index != -1)
        {
            ConfigManager.Settings.FileHistoryList.RemoveAt(index);
        }

        ConfigManager.Settings.FileHistoryList.Insert(0, fileName);

        while (ConfigManager.Settings.FileHistoryList.Count > MAX_FILE_HISTORY)
        {
            ConfigManager.Settings.FileHistoryList.RemoveAt(ConfigManager.Settings.FileHistoryList.Count - 1);
        }

        ConfigManager.Save(SettingsFlags.FileHistory);

        FillHistoryMenu();
    }

    [SupportedOSPlatform("windows")]
    private LogWindow.LogWindow FindWindowForFile (string fileName)
    {
        lock (_logWindowList)
        {
            foreach (LogWindow.LogWindow logWindow in _logWindowList)
            {
                if (logWindow.FileName.ToUpperInvariant().Equals(fileName.ToUpperInvariant(), StringComparison.Ordinal))
                {
                    return logWindow;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Checks if the file name is a settings file. If so, the contained logfile name
    /// is returned. If not, the given file name is returned unchanged.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private string FindFilenameForSettings (string fileName)
    {
        if (fileName.EndsWith(".lxp"))
        {
            PersistenceData persistenceData = Persister.LoadOptionsOnly(fileName);
            if (persistenceData == null)
            {
                return fileName;
            }

            if (!string.IsNullOrEmpty(persistenceData.FileName))
            {
                IFileSystemPlugin fs = PluginRegistry.PluginRegistry.Instance.FindFileSystemForUri(persistenceData.FileName);
                if (fs != null && !fs.GetType().Equals(typeof(LocalFileSystem)))
                {
                    return persistenceData.FileName;
                }

                // On relative paths the URI check (and therefore the file system plugin check) will fail.
                // So fs == null and fs == LocalFileSystem are handled here like normal files.
                if (Path.IsPathRooted(persistenceData.FileName))
                {
                    return persistenceData.FileName;
                }

                // handle relative paths in .lxp files
                var dir = Path.GetDirectoryName(fileName);
                return Path.Combine(dir, persistenceData.FileName);
            }
        }

        return fileName;
    }

    [SupportedOSPlatform("windows")]
    private void FillHistoryMenu ()
    {
        ToolStripDropDown strip = new ToolStripDropDownMenu();

        foreach (var file in ConfigManager.Settings.FileHistoryList)
        {
            ToolStripItem item = new ToolStripMenuItem(file);
            strip.Items.Add(item);
        }

        strip.ItemClicked += OnHistoryItemClicked;
        strip.MouseUp += OnStripMouseUp;
        lastUsedToolStripMenuItem.DropDown = strip;
    }

    [SupportedOSPlatform("windows")]
    private void RemoveLogWindow (LogWindow.LogWindow logWindow)
    {
        lock (_logWindowList)
        {
            _logWindowList.Remove(logWindow);
        }

        DisconnectEventHandlers(logWindow);
    }

    [SupportedOSPlatform("windows")]
    private void RemoveAndDisposeLogWindow (LogWindow.LogWindow logWindow, bool dontAsk)
    {
        if (CurrentLogWindow == logWindow)
        {
            ChangeCurrentLogWindow(null);
        }

        lock (_logWindowList)
        {
            _logWindowList.Remove(logWindow);
        }

        logWindow.Close(dontAsk);
    }

    [SupportedOSPlatform("windows")]
    private void ShowHighlightSettingsDialog ()
    {
        HighlightDialog dlg = new(ConfigManager)
        {
            KeywordActionList = PluginRegistry.PluginRegistry.Instance.RegisteredKeywordActions,
            Owner = this,
            TopMost = TopMost,
            HighlightGroupList = HighlightGroupList,
            PreSelectedGroupName = groupsComboBoxHighlightGroups.Text
        };

        DialogResult res = dlg.ShowDialog();

        if (res == DialogResult.OK)
        {
            HighlightGroupList = dlg.HighlightGroupList;
            FillHighlightComboBox();
            ConfigManager.Settings.Preferences.HighlightGroupList = HighlightGroupList;
            ConfigManager.Save(SettingsFlags.HighlightSettings);
            OnHighlightSettingsChanged();
        }
    }

    [SupportedOSPlatform("windows")]
    private void FillHighlightComboBox ()
    {
        var currentGroupName = groupsComboBoxHighlightGroups.Text;
        groupsComboBoxHighlightGroups.Items.Clear();
        foreach (HighlightGroup group in HighlightGroupList)
        {
            groupsComboBoxHighlightGroups.Items.Add(group.GroupName);
            if (group.GroupName.Equals(currentGroupName, StringComparison.Ordinal))
            {
                groupsComboBoxHighlightGroups.Text = group.GroupName;
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void OpenFileDialog ()
    {
        OpenFileDialog openFileDialog = new();

        if (CurrentLogWindow != null)
        {
            FileInfo info = new(CurrentLogWindow.FileName);
            openFileDialog.InitialDirectory = info.DirectoryName;
        }
        else
        {
            if (!string.IsNullOrEmpty(ConfigManager.Settings.LastDirectory))
            {
                openFileDialog.InitialDirectory = ConfigManager.Settings.LastDirectory;
            }
            else
            {
                try
                {
                    openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
                catch (SecurityException e)
                {
                    _logger.Warn(e, "Insufficient rights for GetFolderPath(): ");
                    // no initial directory if insufficient rights
                }
            }
        }

        openFileDialog.Multiselect = true;

        if (DialogResult.OK == openFileDialog.ShowDialog(this))
        {
            FileInfo info = new(openFileDialog.FileName);
            if (info.Directory.Exists)
            {
                ConfigManager.Settings.LastDirectory = info.DirectoryName;
                ConfigManager.Save(SettingsFlags.FileHistory);
            }

            if (info.Exists)
            {
                LoadFiles(openFileDialog.FileNames, false);
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void LoadFiles (string[] names, bool invertLogic)
    {
        Array.Sort(names);

        if (names.Length == 1)
        {
            if (names[0].EndsWith(".lxj"))
            {
                LoadProject(names[0], true);
                return;
            }

            AddFileTab(names[0], false, null, false, null);
            return;
        }

        MultiFileOption option = ConfigManager.Settings.Preferences.MultiFileOption;
        if (option == MultiFileOption.Ask)
        {
            MultiLoadRequestDialog dlg = new();
            DialogResult res = dlg.ShowDialog();

            if (res == DialogResult.Yes)
            {
                option = MultiFileOption.SingleFiles;
            }
            else if (res == DialogResult.No)
            {
                option = MultiFileOption.MultiFile;
            }
            else
            {
                return;
            }
        }
        else
        {
            if (invertLogic)
            {
                option = option == MultiFileOption.SingleFiles
                    ? MultiFileOption.MultiFile
                    : MultiFileOption.SingleFiles;
            }
        }

        if (option == MultiFileOption.SingleFiles)
        {
            AddFileTabs(names);
        }
        else
        {
            AddMultiFileTab(names);
        }
    }

    private void SetColumnizerHistoryEntry (string fileName, ILogLineColumnizer columnizer)
    {
        ColumnizerHistoryEntry entry = FindColumnizerHistoryEntry(fileName);
        if (entry != null)
        {
            _ = ConfigManager.Settings.ColumnizerHistoryList.Remove(entry);

        }

        ConfigManager.Settings.ColumnizerHistoryList.Add(new ColumnizerHistoryEntry(fileName, columnizer.GetName()));

        if (ConfigManager.Settings.ColumnizerHistoryList.Count > MAX_COLUMNIZER_HISTORY)
        {
            ConfigManager.Settings.ColumnizerHistoryList.RemoveAt(0);
        }
    }

    private ColumnizerHistoryEntry FindColumnizerHistoryEntry (string fileName)
    {
        foreach (ColumnizerHistoryEntry entry in ConfigManager.Settings.ColumnizerHistoryList)
        {
            if (entry.FileName.Equals(fileName, StringComparison.Ordinal))
            {
                return entry;
            }
        }

        return null;
    }

    [SupportedOSPlatform("windows")]
    private void ToggleMultiFile ()
    {
        if (CurrentLogWindow != null)
        {
            CurrentLogWindow.SwitchMultiFile(!CurrentLogWindow.IsMultiFile);
            multiFileToolStripMenuItem.Checked = CurrentLogWindow.IsMultiFile;
            multiFileEnabledStripMenuItem.Checked = CurrentLogWindow.IsMultiFile;
        }
    }

    [SupportedOSPlatform("windows")]
    private void ChangeCurrentLogWindow (LogWindow.LogWindow newLogWindow)
    {
        if (newLogWindow == _currentLogWindow)
        {
            return; // do nothing if wishing to set the same window
        }

        LogWindow.LogWindow oldLogWindow = _currentLogWindow;
        _currentLogWindow = newLogWindow;
        var titleName = _showInstanceNumbers ? "LogExpert #" + _instanceNumber : "LogExpert";

        if (oldLogWindow != null)
        {
            oldLogWindow.StatusLineEvent -= OnStatusLineEvent;
            oldLogWindow.ProgressBarUpdate -= OnProgressBarUpdate;
            oldLogWindow.GuiStateUpdate -= OnGuiStateUpdate;
            oldLogWindow.ColumnizerChanged -= OnColumnizerChanged;
            oldLogWindow.BookmarkAdded -= OnBookmarkAdded;
            oldLogWindow.BookmarkRemoved -= OnBookmarkRemoved;
            oldLogWindow.BookmarkTextChanged -= OnBookmarkTextChanged;
            DisconnectToolWindows(oldLogWindow);
        }

        if (newLogWindow != null)
        {
            newLogWindow.StatusLineEvent += OnStatusLineEvent;
            newLogWindow.ProgressBarUpdate += OnProgressBarUpdate;
            newLogWindow.GuiStateUpdate += OnGuiStateUpdate;
            newLogWindow.ColumnizerChanged += OnColumnizerChanged;
            newLogWindow.BookmarkAdded += OnBookmarkAdded;
            newLogWindow.BookmarkRemoved += OnBookmarkRemoved;
            newLogWindow.BookmarkTextChanged += OnBookmarkTextChanged;

            Text = newLogWindow.IsTempFile
                ? titleName + @" - " + newLogWindow.TempTitleName
                : titleName + @" - " + newLogWindow.FileName;

            multiFileToolStripMenuItem.Checked = CurrentLogWindow.IsMultiFile;
            multiFileToolStripMenuItem.Enabled = true;
            multiFileEnabledStripMenuItem.Checked = CurrentLogWindow.IsMultiFile;
            cellSelectModeToolStripMenuItem.Checked = true;
            cellSelectModeToolStripMenuItem.Enabled = true;
            closeFileToolStripMenuItem.Enabled = true;
            searchToolStripMenuItem.Enabled = true;
            filterToolStripMenuItem.Enabled = true;
            goToLineToolStripMenuItem.Enabled = true;
            //ConnectToolWindows(newLogWindow);
        }
        else
        {
            Text = titleName;
            multiFileToolStripMenuItem.Checked = false;
            multiFileEnabledStripMenuItem.Checked = false;
            checkBoxFollowTail.Checked = false;
            mainMenuStrip.Enabled = true;
            timeshiftToolStripMenuItem.Enabled = false;
            timeshiftToolStripMenuItem.Checked = false;
            timeshiftMenuTextBox.Text = "";
            timeshiftMenuTextBox.Enabled = false;
            multiFileToolStripMenuItem.Enabled = false;
            cellSelectModeToolStripMenuItem.Checked = false;
            cellSelectModeToolStripMenuItem.Enabled = false;
            closeFileToolStripMenuItem.Enabled = false;
            searchToolStripMenuItem.Enabled = false;
            filterToolStripMenuItem.Enabled = false;
            goToLineToolStripMenuItem.Enabled = false;
            dragControlDateTime.Visible = false;
        }
    }

    private void ConnectToolWindows (LogWindow.LogWindow logWindow)
    {
        ConnectBookmarkWindow(logWindow);
    }

    private void ConnectBookmarkWindow (LogWindow.LogWindow logWindow)
    {
        FileViewContext ctx = new(logWindow, logWindow);
        _bookmarkWindow.SetBookmarkData(logWindow.BookmarkData);
        _bookmarkWindow.SetCurrentFile(ctx);
    }

    private void DisconnectToolWindows (LogWindow.LogWindow logWindow)
    {
        DisconnectBookmarkWindow(logWindow);
    }

    private void DisconnectBookmarkWindow (LogWindow.LogWindow logWindow)
    {
        _bookmarkWindow.SetBookmarkData(null);
        _bookmarkWindow.SetCurrentFile(null);
    }

    [SupportedOSPlatform("windows")]
    private void GuiStateUpdateWorker (GuiStateArgs e)
    {
        _skipEvents = true;
        checkBoxFollowTail.Checked = e.FollowTail;
        mainMenuStrip.Enabled = e.MenuEnabled;
        timeshiftToolStripMenuItem.Enabled = e.TimeshiftPossible;
        timeshiftToolStripMenuItem.Checked = e.TimeshiftEnabled;
        timeshiftMenuTextBox.Text = e.TimeshiftText;
        timeshiftMenuTextBox.Enabled = e.TimeshiftEnabled;
        multiFileToolStripMenuItem.Enabled = e.MultiFileEnabled; // disabled for temp files
        multiFileToolStripMenuItem.Checked = e.IsMultiFileActive;
        multiFileEnabledStripMenuItem.Checked = e.IsMultiFileActive;
        cellSelectModeToolStripMenuItem.Checked = e.CellSelectMode;
        RefreshEncodingMenuBar(e.CurrentEncoding);

        if (e.TimeshiftPossible && ConfigManager.Settings.Preferences.TimestampControl)
        {
            dragControlDateTime.MinDateTime = e.MinTimestamp;
            dragControlDateTime.MaxDateTime = e.MaxTimestamp;
            dragControlDateTime.DateTime = e.Timestamp;
            dragControlDateTime.Visible = true;
            dragControlDateTime.Enabled = true;
            dragControlDateTime.Refresh();
        }
        else
        {
            dragControlDateTime.Visible = false;
            dragControlDateTime.Enabled = false;
        }

        toolStripButtonBubbles.Checked = e.ShowBookmarkBubbles;
        groupsComboBoxHighlightGroups.Text = e.HighlightGroupName;
        columnFinderToolStripMenuItem.Checked = e.ColumnFinderVisible;

        _skipEvents = false;
    }

    [SupportedOSPlatform("windows")]
    private void ProgressBarUpdateWorker (ProgressEventArgs e)
    {
        if (e.Value <= e.MaxValue && e.Value >= e.MinValue)
        {
            try
            {
                loadProgessBar.Minimum = e.MinValue;
                loadProgessBar.Maximum = e.MaxValue;
                loadProgessBar.Value = e.Value;
                loadProgessBar.Visible = e.Visible;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during ProgressBarUpdateWorker value {0}, min {1}, max {2}, visible {3}", e.Value, e.MinValue, e.MaxValue, e.Visible);
            }

            Invoke(new MethodInvoker(statusStrip.Refresh));
        }
    }

    [SupportedOSPlatform("windows")]
    private void StatusLineEventWorker (StatusLineEventArgs e)
    {
        if (e != null)
        {
            //_logger.logDebug("StatusLineEvent: text = " + e.StatusText);
            labelStatus.Text = e.StatusText;
            labelStatus.Size = TextRenderer.MeasureText(labelStatus.Text, labelStatus.Font);
            labelLines.Text = $" {e.LineCount} lines";
            labelLines.Size = TextRenderer.MeasureText(labelLines.Text, labelLines.Font);
            labelSize.Text = Util.GetFileSizeAsText(e.FileSize);
            labelSize.Size = TextRenderer.MeasureText(labelSize.Text, labelSize.Font);
            labelCurrentLine.Text = $"Line: {e.CurrentLineNum}";
            labelCurrentLine.Size = TextRenderer.MeasureText(labelCurrentLine.Text, labelCurrentLine.Font);
            if (statusStrip.InvokeRequired)
            {
                statusStrip.BeginInvoke(new MethodInvoker(statusStrip.Refresh));
            }
            else
            {
                statusStrip.Refresh();
            }
        }
    }

    // tailState: 0,1,2 = on/off/off by Trigger
    // syncMode: 0 = normal (no), 1 = time synced
    [SupportedOSPlatform("windows")]
    private Icon CreateLedIcon (int level, bool dirty, int tailState, int syncMode)
    {
        Rectangle iconRect = _leds[0];
        iconRect.Height = 16; // (DockPanel's damn hardcoded height) // this.leds[this.leds.Length - 1].Bottom;
        iconRect.Width = iconRect.Right + 6;
        Bitmap bmp = new(iconRect.Width, iconRect.Height);
        var gfx = Graphics.FromImage(bmp);

        var offsetFromTop = 4;

        for (var i = 0; i < _leds.Length; ++i)
        {
            Rectangle ledRect = _leds[i];
            ledRect.Offset(0, offsetFromTop);

            if (level >= _leds.Length - i)
            {
                gfx.FillRectangle(_ledBrushes[i], ledRect);
            }
            else
            {
                gfx.FillRectangle(_offLedBrush, ledRect);
            }
        }

        var ledSize = 3;
        var ledGap = 1;
        Rectangle lastLed = _leds[^1];
        Rectangle dirtyLed = new(lastLed.Right + 2, lastLed.Bottom - ledSize, ledSize, ledSize);
        Rectangle tailLed = new(dirtyLed.Location, dirtyLed.Size);
        tailLed.Offset(0, -(ledSize + ledGap));
        Rectangle syncLed = new(tailLed.Location, dirtyLed.Size);
        syncLed.Offset(0, -(ledSize + ledGap));

        syncLed.Offset(0, offsetFromTop);
        tailLed.Offset(0, offsetFromTop);
        dirtyLed.Offset(0, offsetFromTop);

        if (dirty)
        {
            gfx.FillRectangle(_dirtyLedBrush, dirtyLed);
        }
        else
        {
            gfx.FillRectangle(_offLedBrush, dirtyLed);
        }

        // tailMode 4 means: don't show
        if (tailState < 3)
        {
            gfx.FillRectangle(_tailLedBrush[tailState], tailLed);
        }

        if (syncMode == 1)
        {
            gfx.FillRectangle(_syncLedBrush, syncLed);
        }
        //else
        //{
        //  gfx.FillRectangle(this.offLedBrush, syncLed);
        //}

        // see http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=345656
        // GetHicon() creates an unmanaged handle which must be destroyed. The Clone() workaround creates
        // a managed copy of icon. then the unmanaged win32 handle is destroyed
        var iconHandle = bmp.GetHicon();
        var icon = Icon.FromHandle(iconHandle).Clone() as Icon;
        Win32.DestroyIcon(iconHandle);

        gfx.Dispose();
        bmp.Dispose();
        return icon;
    }

    [SupportedOSPlatform("windows")]
    private void CreateIcons ()
    {
        for (var syncMode = 0; syncMode <= 1; syncMode++) // LED indicating time synced tabs
        {
            for (var tailMode = 0; tailMode < 4; tailMode++)
            {
                for (var i = 0; i < 6; ++i)
                {
                    _ledIcons[i, 0, tailMode, syncMode] = CreateLedIcon(i, false, tailMode, syncMode);
                }

                for (var i = 0; i < 6; ++i)
                {
                    _ledIcons[i, 1, tailMode, syncMode] = CreateLedIcon(i, true, tailMode, syncMode);
                }
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void FileNotFound (LogWindow.LogWindow logWin)
    {
        var data = logWin.Tag as LogWindowData;
        BeginInvoke(new SetTabIconDelegate(SetTabIcon), logWin, _deadIcon);
        dragControlDateTime.Visible = false;
    }

    [SupportedOSPlatform("windows")]
    private void FileRespawned (LogWindow.LogWindow logWin)
    {
        var data = logWin.Tag as LogWindowData;
        Icon icon = GetIcon(0, data);
        BeginInvoke(new SetTabIconDelegate(SetTabIcon), logWin, icon);
    }

    [SupportedOSPlatform("windows")]
    private void ShowLedPeak (LogWindow.LogWindow logWin)
    {
        var data = logWin.Tag as LogWindowData;
        lock (data)
        {
            data.DiffSum = DIFF_MAX;
        }

        Icon icon = GetIcon(data.DiffSum, data);
        BeginInvoke(new SetTabIconDelegate(SetTabIcon), logWin, icon);
    }

    private int GetLevelFromDiff (int diff)
    {
        if (diff > 60)
        {
            diff = 60;
        }

        var level = diff / 10;
        if (diff > 0 && level == 0)
        {
            level = 2;
        }
        else if (level == 0)
        {
            level = 1;
        }

        return level - 1;
    }

    [SupportedOSPlatform("windows")]
    private void LedThreadProc ()
    {
        Thread.CurrentThread.Name = "LED Thread";
        while (!_shouldStop)
        {
            try
            {
                Thread.Sleep(200);
            }
            catch
            {
                return;
            }

            lock (_logWindowList)
            {
                foreach (LogWindow.LogWindow logWindow in _logWindowList)
                {
                    var data = logWindow.Tag as LogWindowData;
                    if (data.DiffSum > 0)
                    {
                        data.DiffSum -= 10;
                        if (data.DiffSum < 0)
                        {
                            data.DiffSum = 0;
                        }

                        Icon icon = GetIcon(data.DiffSum, data);
                        BeginInvoke(new SetTabIconDelegate(SetTabIcon), logWindow, icon);
                    }
                }
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void SetTabIcon (LogWindow.LogWindow logWindow, Icon icon)
    {
        if (logWindow != null)
        {
            logWindow.Icon = icon;
            logWindow.DockHandler.Pane?.TabStripControl.Invalidate(false);
        }
    }

    private Icon GetIcon (int diff, LogWindowData data)
    {
        Icon icon =
            _ledIcons[
                GetLevelFromDiff(diff), data.Dirty ? 1 : 0, Preferences.ShowTailState ? data.TailState : 3,
                data.SyncMode
            ];
        return icon;
    }

    [SupportedOSPlatform("windows")]
    private void RefreshEncodingMenuBar (Encoding encoding)
    {
        toolStripEncodingASCIIItem.Checked = false;
        toolStripEncodingANSIItem.Checked = false;
        toolStripEncodingUTF8Item.Checked = false;
        toolStripEncodingUTF16Item.Checked = false;
        toolStripEncodingISO88591Item.Checked = false;

        if (encoding == null)
        {
            return;
        }

        if (encoding is ASCIIEncoding)
        {
            toolStripEncodingASCIIItem.Checked = true;
        }
        else if (encoding.Equals(Encoding.Default))
        {
            toolStripEncodingANSIItem.Checked = true;
        }
        else if (encoding is UTF8Encoding)
        {
            toolStripEncodingUTF8Item.Checked = true;
        }
        else if (encoding is UnicodeEncoding)
        {
            toolStripEncodingUTF16Item.Checked = true;
        }
        else if (encoding.Equals(Encoding.GetEncoding("iso-8859-1")))
        {
            toolStripEncodingISO88591Item.Checked = true;
        }

        toolStripEncodingANSIItem.Text = Encoding.Default.HeaderName;
    }

    [SupportedOSPlatform("windows")]
    private void OpenSettings (int tabToOpen)
    {
        SettingsDialog dlg = new(ConfigManager.Settings.Preferences, this, tabToOpen, ConfigManager)
        {
            TopMost = TopMost
        };

        if (DialogResult.OK == dlg.ShowDialog())
        {
            ConfigManager.Settings.Preferences = dlg.Preferences;
            ConfigManager.Save(SettingsFlags.Settings);
            NotifyWindowsForChangedPrefs(SettingsFlags.Settings);
        }
    }

    [SupportedOSPlatform("windows")]
    private void NotifyWindowsForChangedPrefs (SettingsFlags flags)
    {
        _logger.Info("The preferences have changed");
        ApplySettings(ConfigManager.Settings, flags);

        lock (_logWindowList)
        {
            foreach (LogWindow.LogWindow logWindow in _logWindowList)
            {
                logWindow.PreferencesChanged(ConfigManager.Settings.Preferences, false, flags);
            }
        }

        _bookmarkWindow.PreferencesChanged(ConfigManager.Settings.Preferences, false, flags);

        HighlightGroupList = ConfigManager.Settings.Preferences.HighlightGroupList;
        if ((flags & SettingsFlags.HighlightSettings) == SettingsFlags.HighlightSettings)
        {
            OnHighlightSettingsChanged();
        }
    }

    [SupportedOSPlatform("windows")]
    private void ApplySettings (Settings settings, SettingsFlags flags)
    {
        if ((flags & SettingsFlags.WindowPosition) == SettingsFlags.WindowPosition)
        {
            TopMost = alwaysOnTopToolStripMenuItem.Checked = settings.AlwaysOnTop;
            dragControlDateTime.DragOrientation = settings.Preferences.TimestampControlDragOrientation;
            hideLineColumnToolStripMenuItem.Checked = settings.HideLineColumn;
        }

        if ((flags & SettingsFlags.FileHistory) == SettingsFlags.FileHistory)
        {
            FillHistoryMenu();
        }

        if ((flags & SettingsFlags.GuiOrColors) == SettingsFlags.GuiOrColors)
        {
            SetTabIcons(settings.Preferences);
        }

        if ((flags & SettingsFlags.ToolSettings) == SettingsFlags.ToolSettings)
        {
            FillToolLauncherBar();
        }

        if ((flags & SettingsFlags.HighlightSettings) == SettingsFlags.HighlightSettings)
        {
            FillHighlightComboBox();
        }
    }

    [SupportedOSPlatform("windows")]
    private void SetTabIcons (Preferences preferences)
    {
        _tailLedBrush[0] = new SolidBrush(preferences.ShowTailColor);
        CreateIcons();
        lock (_logWindowList)
        {
            foreach (LogWindow.LogWindow logWindow in _logWindowList)
            {
                var data = logWindow.Tag as LogWindowData;
                Icon icon = GetIcon(data.DiffSum, data);
                BeginInvoke(new SetTabIconDelegate(SetTabIcon), logWindow, icon);
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void SetToolIcon (ToolEntry entry, ToolStripItem item)
    {
        Icon icon = Win32.LoadIconFromExe(entry.IconFile, entry.IconIndex);
        if (icon != null)
        {
            item.Image = icon.ToBitmap();
            if (item is ToolStripMenuItem)
            {
                item.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            }
            else
            {
                item.DisplayStyle = ToolStripItemDisplayStyle.Image;
            }

            Win32.DestroyIcon(icon.Handle);
            icon.Dispose();
        }

        if (!string.IsNullOrEmpty(entry.Cmd))
        {
            item.ToolTipText = entry.Name;
        }
    }

    [SupportedOSPlatform("windows")]
    private void ToolButtonClick (ToolEntry toolEntry)
    {
        if (string.IsNullOrEmpty(toolEntry.Cmd))
        {
            //TODO TabIndex => To Enum
            OpenSettings(2);
            return;
        }

        if (CurrentLogWindow != null)
        {
            ILogLine line = CurrentLogWindow.GetCurrentLine();
            ILogFileInfo info = CurrentLogWindow.GetCurrentFileInfo();
            if (line != null && info != null)
            {
                ArgParser parser = new(toolEntry.Args);
                var argLine = parser.BuildArgs(line, CurrentLogWindow.GetRealLineNum() + 1, info, this);
                if (argLine != null)
                {
                    StartTool(toolEntry.Cmd, argLine, toolEntry.Sysout, toolEntry.ColumnizerName, toolEntry.WorkingDir);
                }
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void StartTool (string cmd, string args, bool sysoutPipe, string columnizerName, string workingDir)
    {
        if (string.IsNullOrEmpty(cmd))
        {
            return;
        }

        Process process = new();
        ProcessStartInfo startInfo = new(cmd, args);
        if (!Util.IsNull(workingDir))
        {
            startInfo.WorkingDirectory = workingDir;
        }

        process.StartInfo = startInfo;
        process.EnableRaisingEvents = true;

        if (sysoutPipe)
        {
            ILogLineColumnizer columnizer = ColumnizerPicker.DecideColumnizerByName(columnizerName, PluginRegistry.PluginRegistry.Instance.RegisteredColumnizers);

            _logger.Info("Starting external tool with sysout redirection: {0} {1}", cmd, args);
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            //process.OutputDataReceived += pipe.DataReceivedEventHandler;
            try
            {
                _ = process.Start();
            }
            catch (Win32Exception e)
            {
                _logger.Error(e);
                MessageBox.Show(e.Message);
                return;
            }

            SysoutPipe pipe = new(process.StandardOutput);

            LogWindow.LogWindow logWin = AddTempFileTab(pipe.FileName,
                CurrentLogWindow.IsTempFile
                    ? CurrentLogWindow.TempTitleName
                    : Util.GetNameFromPath(CurrentLogWindow.FileName) + "->E");
            logWin.ForceColumnizer(columnizer);

            process.Exited += pipe.ProcessExitedEventHandler;
            //process.BeginOutputReadLine();
        }
        else
        {
            _logger.Info("Starting external tool: {0} {1}", cmd, args);

            try
            {
                startInfo.UseShellExecute = false;
                _ = process.Start();
            }
            catch (Exception e)
            {
                _logger.Error(e);
                MessageBox.Show(e.Message);
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void CloseAllTabs ()
    {
        IList<Form> closeList = [];
        lock (_logWindowList)
        {
            foreach (DockContent content in dockPanel.Contents)
            {
                if (content is LogWindow.LogWindow window)
                {
                    closeList.Add(window);
                }
            }
        }

        foreach (Form form in closeList)
        {
            form.Close();
        }
    }

    private void SetTabColor (LogWindow.LogWindow logWindow, Color color)
    {
        //tabPage.BackLowColor = color;
        //tabPage.BackLowColorDisabled = Color.FromArgb(255,
        //  Math.Max(0, color.R - 50),
        //  Math.Max(0, color.G - 50),
        //  Math.Max(0, color.B - 50)
        //  );
    }

    [SupportedOSPlatform("windows")]
    private void LoadProject (string projectFileName, bool restoreLayout)
    {
        ProjectData projectData = ProjectPersister.LoadProjectData(projectFileName);
        var hasLayoutData = projectData.TabLayoutXml != null;

        if (hasLayoutData && restoreLayout && _logWindowList.Count > 0)
        {
            ProjectLoadDlg dlg = new();
            if (DialogResult.Cancel != dlg.ShowDialog())
            {
                switch (dlg.ProjectLoadResult)
                {
                    case ProjectLoadDlgResult.IgnoreLayout:
                        hasLayoutData = false;
                        break;
                    case ProjectLoadDlgResult.CloseTabs:
                        CloseAllTabs();
                        break;
                    case ProjectLoadDlgResult.NewWindow:
                        LogExpertProxy.NewWindow([projectFileName]);
                        return;
                }
            }
        }

        if (projectData != null)
        {
            foreach (var fileName in projectData.MemberList)
            {
                if (hasLayoutData)
                {
                    AddFileTabDeferred(fileName, false, null, true, null);
                }
                else
                {
                    AddFileTab(fileName, false, null, true, null);
                }
            }

            if (hasLayoutData && restoreLayout)
            {
                // Re-creating tool (non-document) windows is needed because the DockPanel control would throw strange errors
                DestroyToolWindows();
                InitToolWindows();
                RestoreLayout(projectData.TabLayoutXml);
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void ApplySelectedHighlightGroup ()
    {
        var groupName = groupsComboBoxHighlightGroups.Text;
        CurrentLogWindow?.SetCurrentHighlightGroup(groupName);
    }

    [SupportedOSPlatform("windows")]
    private void FillToolLauncherBar ()
    {
        char[] labels =
        [
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
            'U', 'V', 'W', 'X', 'Y', 'Z'
        ];
        toolsToolStripMenuItem.DropDownItems.Clear();
        toolsToolStripMenuItem.DropDownItems.Add(configureToolStripMenuItem);
        toolsToolStripMenuItem.DropDownItems.Add(configureToolStripSeparator);
        externalToolsToolStrip.Items.Clear();
        var num = 0;
        externalToolsToolStrip.SuspendLayout();
        foreach (ToolEntry tool in Preferences.ToolEntries)
        {
            if (tool.IsFavourite)
            {
                ToolStripButton button = new("" + labels[num % 26])
                {
                    Alignment = ToolStripItemAlignment.Left,
                    Tag = tool
                };

                SetToolIcon(tool, button);
                externalToolsToolStrip.Items.Add(button);
            }

            num++;
            ToolStripMenuItem menuItem = new(tool.Name)
            {
                Tag = tool
            };

            SetToolIcon(tool, menuItem);
            toolsToolStripMenuItem.DropDownItems.Add(menuItem);
        }

        externalToolsToolStrip.ResumeLayout();

        externalToolsToolStrip.Visible = num > 0; // do not show bar if no tool uses it
    }

    private void RunGC ()
    {
        _logger.Info($"Running GC. Used mem before: {GC.GetTotalMemory(false):N0}");
        GC.Collect();
        _logger.Info($"GC done.    Used mem after:  {GC.GetTotalMemory(true):N0}");
    }

    private void DumpGCInfo ()
    {
        _logger.Info($"-------- GC info -----------\r\nUsed mem: {GC.GetTotalMemory(false):N0}");
        for (var i = 0; i < GC.MaxGeneration; ++i)
        {
            _logger.Info($"Generation {i} collect count: {GC.CollectionCount(i)}");
        }

        _logger.Info("----------------------------");
    }

    private void ThrowExceptionFx ()
    {
        throw new Exception("This is a test exception thrown by an async delegate");
    }

    private void ThrowExceptionThreadFx ()
    {
        throw new Exception("This is a test exception thrown by a background thread");
    }

    private string SaveLayout ()
    {
        using MemoryStream memStream = new(2000);
        using StreamReader r = new(memStream);
        dockPanel.SaveAsXml(memStream, Encoding.UTF8, true);

        memStream.Seek(0, SeekOrigin.Begin);
        var resultXml = r.ReadToEnd();

        r.Close();

        return resultXml;
    }

    [SupportedOSPlatform("windows")]
    private void RestoreLayout (string layoutXml)
    {
        using MemoryStream memStream = new(2000);
        using StreamWriter w = new(memStream);
        w.Write(layoutXml);
        w.Flush();

        memStream.Seek(0, SeekOrigin.Begin);

        dockPanel.LoadFromXml(memStream, DeserializeDockContent, true);
    }

    [SupportedOSPlatform("windows")]
    private IDockContent DeserializeDockContent (string persistString)
    {
        if (persistString.Equals(WindowTypes.BookmarkWindow.ToString(), StringComparison.Ordinal))
        {
            return _bookmarkWindow;
        }

        if (persistString.StartsWith(WindowTypes.LogWindow.ToString()))
        {
            var fileName = persistString[(WindowTypes.LogWindow.ToString().Length + 1)..];
            LogWindow.LogWindow win = FindWindowForFile(fileName);
            if (win != null)
            {
                return win;
            }

            _logger.Warn($"Layout data contains non-existing LogWindow for {fileName}");
        }

        return null;
    }

    private void OnHighlightSettingsChanged ()
    {
        HighlightSettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}