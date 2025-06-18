using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Versioning;
using System.Text;

using LogExpert.Core.Classes;
using LogExpert.Core.Classes.Persister;
using LogExpert.Core.Config;
using LogExpert.Core.Entities;
using LogExpert.Core.EventArguments;
using LogExpert.Dialogs;
using LogExpert.UI.Dialogs;
using LogExpert.UI.Extensions.LogWindow;

using WeifenLuo.WinFormsUI.Docking;

namespace LogExpert.UI.Controls.LogTabWindow;

internal partial class LogTabWindow
{
    #region Events handler

    private void OnBookmarkWindowVisibleChanged (object sender, EventArgs e)
    {
        _firstBookmarkWindowShow = false;
    }

    private void OnLogTabWindowLoad (object sender, EventArgs e)
    {
        ApplySettings(ConfigManager.Settings, SettingsFlags.All);
        if (ConfigManager.Settings.IsMaximized)
        {
            Bounds = ConfigManager.Settings.AppBoundsFullscreen;
            WindowState = FormWindowState.Maximized;
            Bounds = ConfigManager.Settings.AppBounds;
        }
        else
        {
            if (ConfigManager.Settings.AppBounds.Right > 0)
            {
                Bounds = ConfigManager.Settings.AppBounds;
            }
        }

        if (ConfigManager.Settings.Preferences.OpenLastFiles && _startupFileNames == null)
        {
            var tmpList = ObjectClone.Clone(ConfigManager.Settings.LastOpenFilesList);

            foreach (var name in tmpList)
            {
                if (string.IsNullOrEmpty(name) == false)
                {
                    AddFileTab(name, false, null, false, null);
                }
            }
        }

        if (_startupFileNames != null)
        {
            LoadFiles(_startupFileNames, false);
        }

        _ledThread = new Thread(LedThreadProc)
        {
            IsBackground = true
        };
        _ledThread.Start();

        FillHighlightComboBox();
        FillToolLauncherBar();
#if !DEBUG
        debugToolStripMenuItem.Visible = false;
#endif
    }

    private void OnLogTabWindowClosing (object sender, CancelEventArgs e)
    {
        try
        {
            _shouldStop = true;
            _statusLineEventHandle.Set();
            _statusLineEventWakeupHandle.Set();
            _ledThread.Join();

            IList<LogWindow.LogWindow> deleteLogWindowList = [];
            ConfigManager.Settings.AlwaysOnTop = TopMost && ConfigManager.Settings.Preferences.AllowOnlyOneInstance;
            SaveLastOpenFilesList();

            foreach (var logWindow in _logWindowList)
            {
                deleteLogWindowList.Add(logWindow);
            }

            foreach (var logWindow in deleteLogWindowList)
            {
                RemoveAndDisposeLogWindow(logWindow, true);
            }

            DestroyBookmarkWindow();

            ConfigManager.Instance.ConfigChanged -= OnConfigChanged;

            SaveWindowPosition();
            ConfigManager.Save(SettingsFlags.WindowPosition | SettingsFlags.FileHistory);
        }
        catch (Exception)
        {
            // ignore error (can occur then multipe instances are closed simultaneously or if the
            // window was not constructed completely because of errors)
        }
        finally
        {
            LogExpertProxy?.WindowClosed(this);
        }
    }

    private void OnStripMouseUp (object sender, MouseEventArgs e)
    {
        if (sender is ToolStripDropDown dropDown)
        {
            AddFileTab(dropDown.Text, false, null, false, null);
        }
    }

    private void OnHistoryItemClicked (object sender, ToolStripItemClickedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.ClickedItem.Text) == false)
        {
            AddFileTab(e.ClickedItem.Text, false, null, false, null);
        }
    }

    private void OnLogWindowDisposed (object sender, EventArgs e)
    {
        var logWindow = sender as LogWindow.LogWindow;

        if (sender == CurrentLogWindow)
        {
            ChangeCurrentLogWindow(null);
        }

        RemoveLogWindow(logWindow);

        logWindow.Tag = null;
    }

    private void OnExitToolStripMenuItemClick (object sender, EventArgs e)
    {
        Close();
    }

    private void OnSelectFilterToolStripMenuItemClick (object sender, EventArgs e)
    {
        if (CurrentLogWindow == null)
        {
            return;
        }

        CurrentLogWindow.ColumnizerCallbackObject.LineNum = CurrentLogWindow.GetCurrentLineNum();
        FilterSelectorForm form = new(PluginRegistry.PluginRegistry.Instance.RegisteredColumnizers, CurrentLogWindow.CurrentColumnizer, CurrentLogWindow.ColumnizerCallbackObject, ConfigManager)
        {
            Owner = this,
            TopMost = TopMost
        };
        var res = form.ShowDialog();

        if (res == DialogResult.OK)
        {
            if (form.ApplyToAll)
            {
                lock (_logWindowList)
                {
                    foreach (var logWindow in _logWindowList)
                    {
                        if (logWindow.CurrentColumnizer.GetType() != form.SelectedColumnizer.GetType())
                        {
                            //logWindow.SetColumnizer(form.SelectedColumnizer);
                            SetColumnizerFx fx = logWindow.ForceColumnizer;
                            logWindow.Invoke(fx, form.SelectedColumnizer);
                            SetColumnizerHistoryEntry(logWindow.FileName, form.SelectedColumnizer);
                        }
                        else
                        {
                            if (form.IsConfigPressed)
                            {
                                logWindow.ColumnizerConfigChanged();
                            }
                        }
                    }
                }
            }
            else
            {
                if (CurrentLogWindow.CurrentColumnizer.GetType() != form.SelectedColumnizer.GetType())
                {
                    SetColumnizerFx fx = CurrentLogWindow.ForceColumnizer;
                    CurrentLogWindow.Invoke(fx, form.SelectedColumnizer);
                    SetColumnizerHistoryEntry(CurrentLogWindow.FileName, form.SelectedColumnizer);
                }

                if (form.IsConfigPressed)
                {
                    lock (_logWindowList)
                    {
                        foreach (var logWindow in _logWindowList)
                        {
                            if (logWindow.CurrentColumnizer.GetType() == form.SelectedColumnizer.GetType())
                            {
                                logWindow.ColumnizerConfigChanged();
                            }
                        }
                    }
                }
            }
        }
    }

    private void OnGoToLineToolStripMenuItemClick (object sender, EventArgs e)
    {
        if (CurrentLogWindow == null)
        {
            return;
        }

        GotoLineDialog dlg = new(this);
        var res = dlg.ShowDialog();
        if (res == DialogResult.OK)
        {
            var line = dlg.Line - 1;
            if (line >= 0)
            {
                CurrentLogWindow.GotoLine(line);
            }
        }
    }

    private void OnHighlightingToolStripMenuItemClick (object sender, EventArgs e)
    {
        ShowHighlightSettingsDialog();
    }

    private void OnSearchToolStripMenuItemClick (object sender, EventArgs e)
    {
        OpenSearchDialog();
    }

    private void OnOpenToolStripMenuItemClick (object sender, EventArgs e)
    {
        OpenFileDialog();
    }

    private void OnLogTabWindowDragEnter (object sender, DragEventArgs e)
    {
#if DEBUG
        var formats = e.Data.GetFormats();
        var s = "Dragging something over LogExpert. Formats:  ";
        foreach (var format in formats)
        {
            s += format;
            s += " , ";
        }

        s = s[..^3];
        _logger.Info(s);
#endif
    }

    private void OnLogWindowDragOver (object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effect = DragDropEffects.None;
        }
        else
        {
            e.Effect = DragDropEffects.Copy;
        }
    }

    private void OnLogWindowDragDrop (object sender, DragEventArgs e)
    {
#if DEBUG
        var formats = e.Data.GetFormats();
        var s = "Dropped formats:  ";
        foreach (var format in formats)
        {
            s += format;
            s += " , ";
        }

        s = s[..^3];
        _logger.Debug(s);
#endif

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var o = e.Data.GetData(DataFormats.FileDrop);
            if (o is string[] names)
            {
                LoadFiles(names, (e.KeyState & 4) == 4); // (shift pressed?)
                e.Effect = DragDropEffects.Copy;
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnTimeShiftToolStripMenuItemCheckStateChanged (object sender, EventArgs e)
    {
        if (!_skipEvents && CurrentLogWindow != null)
        {
            CurrentLogWindow.SetTimeshiftValue(timeshiftMenuTextBox.Text);
            timeshiftMenuTextBox.Enabled = timeshiftToolStripMenuItem.Checked;
            CurrentLogWindow.TimeshiftEnabled(timeshiftToolStripMenuItem.Checked,
                timeshiftMenuTextBox.Text);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnAboutToolStripMenuItemClick (object sender, EventArgs e)
    {
        AboutBox aboutBox = new()
        {
            TopMost = TopMost
        };

        aboutBox.ShowDialog();
    }

    private void OnFilterToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.ToggleFilterPanel();
    }

    [SupportedOSPlatform("windows")]
    private void OnMultiFileToolStripMenuItemClick (object sender, EventArgs e)
    {
        ToggleMultiFile();
        fileToolStripMenuItem.HideDropDown();
    }

    [SupportedOSPlatform("windows")]
    private void OnGuiStateUpdate (object sender, GuiStateArgs e)
    {
        BeginInvoke(GuiStateUpdateWorker, e);
    }

    private void OnColumnizerChanged (object sender, ColumnizerEventArgs e)
    {
        _bookmarkWindow?.SetColumnizer(e.Columnizer);
    }

    private void OnBookmarkAdded (object sender, EventArgs e)
    {
        _bookmarkWindow.UpdateView();
    }

    private void OnBookmarkTextChanged (object sender, BookmarkEventArgs e)
    {
        _bookmarkWindow.BookmarkTextChanged(e.Bookmark);
    }

    private void OnBookmarkRemoved (object sender, EventArgs e)
    {
        _bookmarkWindow.UpdateView();
    }

    private void OnProgressBarUpdate (object sender, ProgressEventArgs e)
    {
        Invoke(ProgressBarUpdateWorker, e);
    }

    private void OnStatusLineEvent (object sender, StatusLineEventArgs e)
    {
        StatusLineEventWorker(e);
    }

    private void OnFollowTailCheckBoxClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.FollowTailChanged(checkBoxFollowTail.Checked, false);
    }

    private void OnLogTabWindowKeyDown (object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.W && e.Control)
        {
            CurrentLogWindow?.Close();
        }
        else if (e.KeyCode == Keys.Tab && e.Control)
        {
            SwitchTab(e.Shift);
        }
        else
        {
            CurrentLogWindow?.OnLogWindowKeyDown(sender, e);
        }
    }

    private void OnCloseFileToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.Close();
    }

    [SupportedOSPlatform("windows")]
    private void OnCellSelectModeToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.SetCellSelectionMode(cellSelectModeToolStripMenuItem.Checked);
    }

    private void OnCopyMarkedLinesIntoNewTabToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.CopyMarkedLinesToTab();
    }

    private void OnTimeShiftMenuTextBoxKeyDown (object sender, KeyEventArgs e)
    {
        if (CurrentLogWindow == null)
        {
            return;
        }

        if (e.KeyCode == Keys.Enter)
        {
            e.Handled = true;
            CurrentLogWindow.SetTimeshiftValue(timeshiftMenuTextBox.Text);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnAlwaysOnTopToolStripMenuItemClick (object sender, EventArgs e)
    {
        TopMost = alwaysOnTopToolStripMenuItem.Checked;
    }

    private void OnFileSizeChanged (object sender, LogEventArgs e)
    {
        if (sender.GetType().IsAssignableFrom(typeof(LogWindow.LogWindow)))
        {
            var diff = e.LineCount - e.PrevLineCount;
            if (diff < 0)
            {
                return;
            }

            if (((LogWindow.LogWindow)sender).Tag is LogWindowData data)
            {
                lock (data)
                {
                    data.DiffSum += diff;
                    if (data.DiffSum > DIFF_MAX)
                    {
                        data.DiffSum = DIFF_MAX;
                    }
                }

                //if (this.dockPanel.ActiveContent != null &&
                //    this.dockPanel.ActiveContent != sender || data.tailState != 0)
                if (CurrentLogWindow != null &&
                    CurrentLogWindow != sender || data.TailState != 0)
                {
                    data.Dirty = true;
                }
                var icon = GetIcon(diff, data);
                BeginInvoke(new SetTabIconDelegate(SetTabIcon), (LogWindow.LogWindow)sender, icon);
            }
        }
    }

    private void OnLogWindowFileNotFound (object sender, EventArgs e)
    {
        Invoke(new FileNotFoundDelegate(FileNotFound), sender);
    }

    private void OnLogWindowFileRespawned (object sender, EventArgs e)
    {
        Invoke(new FileRespawnedDelegate(FileRespawned), sender);
    }

    private void OnLogWindowFilterListChanged (object sender, FilterListChangedEventArgs e)
    {
        lock (_logWindowList)
        {
            foreach (var logWindow in _logWindowList)
            {
                if (logWindow != e.LogWindow)
                {
                    logWindow.HandleChangedFilterList();
                }
            }
        }
        ConfigManager.Save(SettingsFlags.FilterList);
    }

    private void OnLogWindowCurrentHighlightGroupChanged (object sender, CurrentHighlightGroupChangedEventArgs e)
    {
        OnHighlightSettingsChanged();
        ConfigManager.Settings.Preferences.HighlightGroupList = HighlightGroupList;
        ConfigManager.Save(SettingsFlags.HighlightSettings);
    }

    private void OnTailFollowed (object sender, EventArgs e)
    {
        if (dockPanel.ActiveContent == null)
        {
            return;
        }
        if (sender.GetType().IsAssignableFrom(typeof(LogWindow.LogWindow)))
        {
            if (dockPanel.ActiveContent == sender)
            {
                var data = ((LogWindow.LogWindow)sender).Tag as LogWindowData;
                data.Dirty = false;
                var icon = GetIcon(data.DiffSum, data);
                BeginInvoke(new SetTabIconDelegate(SetTabIcon), (LogWindow.LogWindow)sender, icon);
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnLogWindowSyncModeChanged (object sender, SyncModeEventArgs e)
    {
        if (!Disposing)
        {
            var data = ((LogWindow.LogWindow)sender).Tag as LogWindowData;
            data.SyncMode = e.IsTimeSynced ? 1 : 0;
            var icon = GetIcon(data.DiffSum, data);
            BeginInvoke(new SetTabIconDelegate(SetTabIcon), (LogWindow.LogWindow)sender, icon);
        }
        else
        {
            _logger.Warn(CultureInfo.InvariantCulture, "Received SyncModeChanged event while disposing. Event ignored.");
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnToggleBookmarkToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.ToggleBookmark();
    }

    [SupportedOSPlatform("windows")]
    private void OnJumpToNextToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.JumpNextBookmark();
    }

    [SupportedOSPlatform("windows")]
    private void OnJumpToPrevToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.JumpPrevBookmark();
    }

    [SupportedOSPlatform("windows")]
    private void OnASCIIToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.ChangeEncoding(Encoding.ASCII);
    }

    [SupportedOSPlatform("windows")]
    private void OnANSIToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.ChangeEncoding(Encoding.Default);
    }

    [SupportedOSPlatform("windows")]
    private void OnUTF8ToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.ChangeEncoding(new UTF8Encoding(false));
    }

    [SupportedOSPlatform("windows")]
    private void OnUTF16ToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.ChangeEncoding(Encoding.Unicode);
    }

    [SupportedOSPlatform("windows")]
    private void OnISO88591ToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.ChangeEncoding(Encoding.GetEncoding("iso-8859-1"));
    }

    [SupportedOSPlatform("windows")]
    private void OnReloadToolStripMenuItemClick (object sender, EventArgs e)
    {
        if (CurrentLogWindow != null)
        {
            var data = CurrentLogWindow.Tag as LogWindowData;
            var icon = GetIcon(0, data);
            BeginInvoke(new SetTabIconDelegate(SetTabIcon), CurrentLogWindow, icon);
            CurrentLogWindow.Reload();
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnSettingsToolStripMenuItemClick (object sender, EventArgs e)
    {
        OpenSettings(0);
    }

    [SupportedOSPlatform("windows")]
    private void OnDateTimeDragControlValueDragged (object sender, EventArgs e)
    {
        if (CurrentLogWindow != null)
        {
            //this.CurrentLogWindow.ScrollToTimestamp(this.dateTimeDragControl.DateTime);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnDateTimeDragControlValueChanged (object sender, EventArgs e)
    {
        CurrentLogWindow?.ScrollToTimestamp(dragControlDateTime.DateTime, true, true);
    }

    [SupportedOSPlatform("windows")]
    private void OnLogTabWindowDeactivate (object sender, EventArgs e)
    {
        CurrentLogWindow?.AppFocusLost();
    }

    [SupportedOSPlatform("windows")]
    private void OnLogTabWindowActivated (object sender, EventArgs e)
    {
        CurrentLogWindow?.AppFocusGained();
    }

    [SupportedOSPlatform("windows")]
    private void OnShowBookmarkListToolStripMenuItemClick (object sender, EventArgs e)
    {
        if (_bookmarkWindow.Visible)
        {
            _bookmarkWindow.Hide();
        }
        else
        {
            // strange: on very first Show() now bookmarks are displayed. after a hide it will work.
            if (_firstBookmarkWindowShow)
            {
                _bookmarkWindow.Show(dockPanel);
                _bookmarkWindow.Hide();
            }

            _bookmarkWindow.Show(dockPanel);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnToolStripButtonOpenClick (object sender, EventArgs e)
    {
        OpenFileDialog();
    }

    [SupportedOSPlatform("windows")]
    private void OnToolStripButtonSearchClick (object sender, EventArgs e)
    {
        OpenSearchDialog();
    }

    [SupportedOSPlatform("windows")]
    private void OnToolStripButtonFilterClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.ToggleFilterPanel();
    }

    [SupportedOSPlatform("windows")]
    private void OnToolStripButtonBookmarkClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.ToggleBookmark();
    }

    [SupportedOSPlatform("windows")]
    private void OnToolStripButtonUpClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.JumpPrevBookmark();
    }

    [SupportedOSPlatform("windows")]
    private void OnToolStripButtonDownClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.JumpNextBookmark();
    }

    [SupportedOSPlatform("windows")]
    private void OnShowHelpToolStripMenuItemClick (object sender, EventArgs e)
    {
        Help.ShowHelp(this, "LogExpert.chm");
    }

    private void OnHideLineColumnToolStripMenuItemClick (object sender, EventArgs e)
    {
        ConfigManager.Settings.HideLineColumn = hideLineColumnToolStripMenuItem.Checked;
        lock (_logWindowList)
        {
            foreach (var logWin in _logWindowList)
            {
                logWin.ShowLineColumn(!ConfigManager.Settings.HideLineColumn);
            }
        }
        _bookmarkWindow.LineColumnVisible = ConfigManager.Settings.HideLineColumn;
    }

    // ==================================================================
    // Tab context menu stuff
    // ==================================================================

    [SupportedOSPlatform("windows")]
    private void OnCloseThisTabToolStripMenuItemClick (object sender, EventArgs e)
    {
        (dockPanel.ActiveContent as LogWindow.LogWindow).Close();
    }

    [SupportedOSPlatform("windows")]
    private void OnCloseOtherTabsToolStripMenuItemClick (object sender, EventArgs e)
    {
        IList<Form> closeList = [];
        lock (_logWindowList)
        {
            foreach (DockContent content in dockPanel.Contents)
            {
                if (content != dockPanel.ActiveContent && content is LogWindow.LogWindow)
                {
                    closeList.Add(content);
                }
            }
        }

        foreach (var form in closeList)
        {
            form.Close();
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnCloseAllTabsToolStripMenuItemClick (object sender, EventArgs e)
    {
        CloseAllTabs();
    }

    [SupportedOSPlatform("windows")]
    private void OnTabColorToolStripMenuItemClick (object sender, EventArgs e)
    {
        var logWindow = dockPanel.ActiveContent as LogWindow.LogWindow;

        if (logWindow.Tag is not LogWindowData data)
        {
            return;
        }

        ColorDialog dlg = new()
        {
            Color = data.Color
        };

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            data.Color = dlg.Color;
            SetTabColor(logWindow, data.Color);
        }

        List<ColorEntry> delList = [];

        foreach (var entry in ConfigManager.Settings.FileColors)
        {
            if (entry.FileName.Equals(logWindow.FileName, StringComparison.Ordinal))
            {
                delList.Add(entry);
            }
        }

        foreach (var entry in delList)
        {
            _ = ConfigManager.Settings.FileColors.Remove(entry);
        }

        ConfigManager.Settings.FileColors.Add(new ColorEntry(logWindow.FileName, dlg.Color));

        while (ConfigManager.Settings.FileColors.Count > MAX_COLOR_HISTORY)
        {
            ConfigManager.Settings.FileColors.RemoveAt(0);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnLogTabWindowSizeChanged (object sender, EventArgs e)
    {
        if (WindowState != FormWindowState.Minimized)
        {
            _wasMaximized = WindowState == FormWindowState.Maximized;
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnSaveProjectToolStripMenuItemClick (object sender, EventArgs e)
    {
        SaveFileDialog dlg = new()
        {
            DefaultExt = "lxj",
            Filter = @"LogExpert session (*.lxj)|*.lxj"
        };

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            var fileName = dlg.FileName;
            List<string> fileNames = [];

            lock (_logWindowList)
            {
                foreach (DockContent content in dockPanel.Contents.Cast<DockContent>())
                {
                    var logWindow = content as LogWindow.LogWindow;
                    var persistenceFileName = logWindow?.SavePersistenceData(true);
                    if (persistenceFileName != null)
                    {
                        fileNames.Add(persistenceFileName);
                    }
                }
            }

            ProjectData projectData = new()
            {
                MemberList = fileNames,
                TabLayoutXml = SaveLayout()
            };
            ProjectPersister.SaveProjectData(fileName, projectData);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnLoadProjectToolStripMenuItemClick (object sender, EventArgs e)
    {
        OpenFileDialog dlg = new()
        {
            DefaultExt = "lxj",
            Filter = @"LogExpert sessions (*.lxj)|*.lxj"
        };

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            var projectFileName = dlg.FileName;
            LoadProject(projectFileName, true);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnToolStripButtonBubblesClick (object sender, EventArgs e)
    {
        if (CurrentLogWindow != null)
        {
            CurrentLogWindow.ShowBookmarkBubbles = toolStripButtonBubbles.Checked;
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnCopyPathToClipboardToolStripMenuItemClick (object sender, EventArgs e)
    {
        var logWindow = dockPanel.ActiveContent as LogWindow.LogWindow;
        Clipboard.SetText(logWindow.Title);
    }

    private void OnFindInExplorerToolStripMenuItemClick (object sender, EventArgs e)
    {
        var logWindow = dockPanel.ActiveContent as LogWindow.LogWindow;

        Process explorer = new();
        explorer.StartInfo.FileName = "explorer.exe";
        explorer.StartInfo.Arguments = "/e,/select," + logWindow.Title;
        explorer.StartInfo.UseShellExecute = false;
        explorer.Start();
    }

    private void TruncateFileToolStripMenuItem_Click (object sender, EventArgs e)
    {
        CurrentLogWindow?.TryToTruncate();
    }

    private void OnExportBookmarksToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.ExportBookmarkList();
    }

    [SupportedOSPlatform("windows")]
    private void OnHighlightGroupsComboBoxDropDownClosed (object sender, EventArgs e)
    {
        ApplySelectedHighlightGroup();
    }

    [SupportedOSPlatform("windows")]
    private void OnHighlightGroupsComboBoxSelectedIndexChanged (object sender, EventArgs e)
    {
        ApplySelectedHighlightGroup();
    }

    [SupportedOSPlatform("windows")]
    private void OnHighlightGroupsComboBoxMouseUp (object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            ShowHighlightSettingsDialog();
        }
    }


    private void OnConfigChanged (object sender, ConfigChangedEventArgs e)
    {
        if (LogExpertProxy != null)
        {
            NotifySettingsChanged(null, e.Flags);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnDumpLogBufferInfoToolStripMenuItemClick (object sender, EventArgs e)
    {
#if DEBUG
        CurrentLogWindow?.DumpBufferInfo();
#endif
    }

    [SupportedOSPlatform("windows")]
    private void OnDumpBufferDiagnosticToolStripMenuItemClick (object sender, EventArgs e)
    {
#if DEBUG
        CurrentLogWindow?.DumpBufferDiagnostic();
#endif
    }

    private void OnRunGCToolStripMenuItemClick (object sender, EventArgs e)
    {
        RunGC();
    }

    private void OnGCInfoToolStripMenuItemClick (object sender, EventArgs e)
    {
        DumpGCInfo();
    }

    [SupportedOSPlatform("windows")]
    private void OnToolsToolStripMenuItemDropDownItemClicked (object sender, ToolStripItemClickedEventArgs e)
    {
        if (e.ClickedItem.Tag is ToolEntry tag)
        {
            ToolButtonClick(tag);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnExternalToolsToolStripItemClicked (object sender, ToolStripItemClickedEventArgs e)
    {
        ToolButtonClick(e.ClickedItem.Tag as ToolEntry);
    }

    [SupportedOSPlatform("windows")]
    private void OnConfigureToolStripMenuItemClick (object sender, EventArgs e)
    {
        OpenSettings(2);
    }

    private void OnThrowExceptionGUIThreadToolStripMenuItemClick (object sender, EventArgs e)
    {
        throw new Exception("This is a test exception thrown by the GUI thread");
    }

    private void OnThrowExceptionBackgroundThToolStripMenuItemClick (object sender, EventArgs e)
    {
        ExceptionFx fx = ThrowExceptionFx;
        fx.BeginInvoke(null, null);
    }

    private void OnThrowExceptionBackgroundThreadToolStripMenuItemClick (object sender, EventArgs e)
    {
        Thread thread = new(ThrowExceptionThreadFx)
        {
            IsBackground = true
        };

        thread.Start();
    }

    private void OnWarnToolStripMenuItemClick (object sender, EventArgs e)
    {
        //_logger.GetLogger().LogLevel = _logger.Level.WARN;
    }

    private void OnInfoToolStripMenuItemClick (object sender, EventArgs e)
    {
        //_logger.Get_logger().LogLevel = _logger.Level.INFO;
    }

    private void OnDebugToolStripMenuItemClick (object sender, EventArgs e)
    {
        //_logger.Get_logger().LogLevel = _logger.Level.DEBUG;
    }

    private void OnLogLevelToolStripMenuItemClick (object sender, EventArgs e)
    {
    }

    private void OnLogLevelToolStripMenuItemDropDownOpening (object sender, EventArgs e)
    {
        //warnToolStripMenuItem.Checked = _logger.Get_logger().LogLevel == _logger.Level.WARN;
        //infoToolStripMenuItem.Checked = _logger.Get_logger().LogLevel == _logger.Level.INFO;
        //debugToolStripMenuItem1.Checked = _logger.Get_logger().LogLevel == _logger.Level.DEBUG;
    }

    [SupportedOSPlatform("windows")]
    private void OnDisableWordHighlightModeToolStripMenuItemClick (object sender, EventArgs e)
    {
        DebugOptions.DisableWordHighlight = disableWordHighlightModeToolStripMenuItem.Checked;
        CurrentLogWindow?.RefreshAllGrids();
    }

    [SupportedOSPlatform("windows")]
    private void OnMultiFileMaskToolStripMenuItemClick (object sender, EventArgs e)
    {
        CurrentLogWindow?.ChangeMultifileMask();
    }

    [SupportedOSPlatform("windows")]
    private void OnMultiFileEnabledStripMenuItemClick (object sender, EventArgs e)
    {
        ToggleMultiFile();
    }

    [SupportedOSPlatform("windows")]
    private void OnLockInstanceToolStripMenuItemClick (object sender, EventArgs e)
    {
        AbstractLogTabWindow.StaticData.CurrentLockedMainWindow = lockInstanceToolStripMenuItem.Checked ? null : this;
    }

    [SupportedOSPlatform("windows")]
    private void OnOptionToolStripMenuItemDropDownOpening (object sender, EventArgs e)
    {
        lockInstanceToolStripMenuItem.Enabled = !ConfigManager.Settings.Preferences.AllowOnlyOneInstance;
        lockInstanceToolStripMenuItem.Checked = AbstractLogTabWindow.StaticData.CurrentLockedMainWindow == this;
    }

    [SupportedOSPlatform("windows")]
    private void OnFileToolStripMenuItemDropDownOpening (object sender, EventArgs e)
    {
        newFromClipboardToolStripMenuItem.Enabled = Clipboard.ContainsText();
    }

    [SupportedOSPlatform("windows")]
    private void OnNewFromClipboardToolStripMenuItemClick (object sender, EventArgs e)
    {
        PasteFromClipboard();
    }

    [SupportedOSPlatform("windows")]
    private void OnOpenURIToolStripMenuItemClick (object sender, EventArgs e)
    {
        OpenUriDialog dlg = new()
        {
            UriHistory = ConfigManager.Settings.UriHistoryList
        };

        if (DialogResult.OK == dlg.ShowDialog())
        {
            if (dlg.Uri.Trim().Length > 0)
            {
                ConfigManager.Settings.UriHistoryList = dlg.UriHistory;
                ConfigManager.Save(SettingsFlags.FileHistory);
                LoadFiles([dlg.Uri], false);
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnColumnFinderToolStripMenuItemClick (object sender, EventArgs e)
    {
        if (CurrentLogWindow != null && !_skipEvents)
        {
            CurrentLogWindow.ToggleColumnFinder(columnFinderToolStripMenuItem.Checked, true);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnDockPanelActiveContentChanged (object sender, EventArgs e)
    {
        if (dockPanel.ActiveContent is LogWindow.LogWindow window)
        {
            CurrentLogWindow = window;
            CurrentLogWindow.LogWindowActivated();
            ConnectToolWindows(CurrentLogWindow);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnTabRenameToolStripMenuItemClick (object sender, EventArgs e)
    {
        if (CurrentLogWindow != null)
        {
            TabRenameDialog dlg = new()
            {
                TabName = CurrentLogWindow.Text
            };

            if (DialogResult.OK == dlg.ShowDialog())
            {
                CurrentLogWindow.Text = dlg.TabName;
            }

            dlg.Dispose();
        }
    }

    #endregion
}