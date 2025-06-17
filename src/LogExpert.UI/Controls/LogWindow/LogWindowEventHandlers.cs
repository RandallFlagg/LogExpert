using System.ComponentModel;
using System.Runtime.Versioning;

using LogExpert.Core.Classes;
using LogExpert.Core.Classes.Filter;
using LogExpert.Core.Classes.Highlight;
using LogExpert.Core.Config;
using LogExpert.Core.Entities;
using LogExpert.Core.EventArguments;
using LogExpert.Core.Interface;
using LogExpert.Dialogs;
using LogExpert.UI.Dialogs;
using LogExpert.UI.Entities;
using LogExpert.UI.Extensions;

namespace LogExpert.UI.Controls.LogWindow;

partial class LogWindow
{
    [SupportedOSPlatform("windows")]
    private void AutoResizeFilterBox ()
    {
        filterSplitContainer.SplitterDistance = filterComboBox.Left + filterComboBox.GetMaxTextWidth();
    }

    #region Events handler

    protected void OnProgressBarUpdate (ProgressEventArgs e)
    {
        ProgressBarUpdate?.Invoke(this, e);
    }

    protected void OnStatusLine (StatusLineEventArgs e)
    {
        StatusLineEvent?.Invoke(this, e);
    }

    protected void OnGuiState (GuiStateArgs e)
    {
        GuiStateUpdate?.Invoke(this, e);
    }

    protected void OnTailFollowed (EventArgs e)
    {
        TailFollowed?.Invoke(this, e);
    }

    protected void OnFileNotFound (EventArgs e)
    {
        FileNotFound?.Invoke(this, e);
    }

    protected void OnFileRespawned (EventArgs e)
    {
        FileRespawned?.Invoke(this, e);
    }

    protected void OnFilterListChanged (LogWindow source)
    {
        FilterListChanged?.Invoke(this, new FilterListChangedEventArgs(source));
    }

    protected void OnCurrentHighlightListChanged ()
    {
        CurrentHighlightGroupChanged?.Invoke(this, new CurrentHighlightGroupChangedEventArgs(this, _currentHighlightGroup));
    }

    protected void OnBookmarkAdded ()
    {
        BookmarkAdded?.Invoke(this, EventArgs.Empty);
    }

    protected void OnBookmarkRemoved ()
    {
        BookmarkRemoved?.Invoke(this, EventArgs.Empty);
    }

    protected void OnBookmarkTextChanged (Bookmark bookmark)
    {
        BookmarkTextChanged?.Invoke(this, new BookmarkEventArgs(bookmark));
    }

    protected void OnColumnizerChanged (ILogLineColumnizer columnizer)
    {
        ColumnizerChanged?.Invoke(this, new ColumnizerEventArgs(columnizer));
    }

    protected void OnRegisterCancelHandler (IBackgroundProcessCancelHandler handler)
    {
        lock (_cancelHandlerList)
        {
            _cancelHandlerList.Add(handler);
        }
    }

    protected void OnDeRegisterCancelHandler (IBackgroundProcessCancelHandler handler)
    {
        lock (_cancelHandlerList)
        {
            _cancelHandlerList.Remove(handler);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnLogWindowLoad (object sender, EventArgs e)
    {
        var setLastColumnWidth = _parentLogTabWin.Preferences.SetLastColumnWidth;
        var lastColumnWidth = _parentLogTabWin.Preferences.LastColumnWidth;
        var fontName = _parentLogTabWin.Preferences.FontName;
        var fontSize = _parentLogTabWin.Preferences.FontSize;

        PreferencesChanged(fontName, fontSize, setLastColumnWidth, lastColumnWidth, true, SettingsFlags.GuiOrColors);
    }

    [SupportedOSPlatform("windows")]
    private void OnLogWindowDisposed (object sender, EventArgs e)
    {
        _waitingForClose = true;
        _parentLogTabWin.HighlightSettingsChanged -= OnParentHighlightSettingsChanged;
        _logFileReader?.DeleteAllContent();

        FreeFromTimeSync();
    }

    [SupportedOSPlatform("windows")]
    private void OnLogFileReaderLoadingStarted (object sender, LoadFileEventArgs e)
    {
        Invoke(LoadingStarted, e);
    }

    [SupportedOSPlatform("windows")]
    private void OnLogFileReaderFinishedLoading (object sender, EventArgs e)
    {
        //Thread.CurrentThread.Name = "FinishedLoading event thread";
        _logger.Info("Finished loading.");
        _isLoading = false;
        _isDeadFile = false;
        if (!_waitingForClose)
        {
            Invoke(new MethodInvoker(LoadingFinished));
            Invoke(new MethodInvoker(LoadPersistenceData));
            Invoke(new MethodInvoker(SetGuiAfterLoading));
            _loadingFinishedEvent.Set();
            _externaLoadingFinishedEvent.Set();
            _timeSpreadCalc.SetLineCount(_logFileReader.LineCount);

            if (_reloadMemento != null)
            {
                Invoke(new PositionAfterReloadFx(PositionAfterReload), _reloadMemento);
            }

            if (filterTailCheckBox.Checked)
            {
                _logger.Info("Refreshing filter view because of reload.");
                Invoke(new MethodInvoker(FilterSearch)); // call on proper thread
            }

            HandleChangedFilterList();
        }

        _reloadMemento = null;
    }

    [SupportedOSPlatform("windows")]
    private void OnLogFileReaderFileNotFound (object sender, EventArgs e)
    {
        if (!IsDisposed && !Disposing)
        {
            _logger.Info("Handling file not found event.");
            _isDeadFile = true;
            BeginInvoke(new MethodInvoker(LogfileDead));
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnLogFileReaderRespawned (object sender, EventArgs e)
    {
        BeginInvoke(new MethodInvoker(LogfileRespawned));
    }

    [SupportedOSPlatform("windows")]
    private void OnLogWindowClosing (object sender, CancelEventArgs e)
    {
        if (Preferences.AskForClose)
        {
            if (MessageBox.Show("Sure to close?", "LogExpert", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }
        }

        SavePersistenceData(false);
        CloseLogWindow();
    }

    [SupportedOSPlatform("windows")]
    private void OnDataGridViewColumnDividerDoubleClick (object sender, DataGridViewColumnDividerDoubleClickEventArgs e)
    {
        e.Handled = true;
        AutoResizeColumns(dataGridView);
    }

    /**
   * Event handler for the Load event from LogfileReader
   */
    [SupportedOSPlatform("windows")]
    private void OnLogFileReaderLoadFile (object sender, LoadFileEventArgs e)
    {
        if (e.NewFile)
        {
            _logger.Info("File created anew.");

            // File was new created (e.g. rollover)
            _isDeadFile = false;
            UnRegisterLogFileReaderEvents();
            dataGridView.CurrentCellChanged -= OnDataGridViewCurrentCellChanged;
            MethodInvoker invoker = ReloadNewFile;
            BeginInvoke(invoker);
            //Thread loadThread = new Thread(new ThreadStart(ReloadNewFile));
            //loadThread.Start();
            _logger.Debug("Reloading invoked.");
        }
        else if (_isLoading)
        {
            BeginInvoke(UpdateProgress, e);
        }
    }

    private void OnFileSizeChanged (object sender, LogEventArgs e)
    {
        //OnFileSizeChanged(e);  // now done in UpdateGrid()
        _logger.Info("Got FileSizeChanged event. prevLines:{0}, curr lines: {1}", e.PrevLineCount, e.LineCount);

        // - now done in the thread that works on the event args list
        //if (e.IsRollover)
        //{
        //  ShiftBookmarks(e.RolloverOffset);
        //  ShiftFilterPipes(e.RolloverOffset);
        //}

        //UpdateGridCallback callback = new UpdateGridCallback(UpdateGrid);
        //this.BeginInvoke(callback, new object[] { e });
        lock (_logEventArgsList)
        {
            _logEventArgsList.Add(e);
            _logEventArgsEvent.Set();
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnDataGridViewCellValueNeeded (object sender, DataGridViewCellValueEventArgs e)
    {
        var startCount = CurrentColumnizer?.GetColumnCount() ?? 0;

        e.Value = GetCellValue(e.RowIndex, e.ColumnIndex);

        // The new column could be find dynamically.
        // Only support add new columns for now.
        // TODO: Support reload all columns?
        if (CurrentColumnizer != null && CurrentColumnizer.GetColumnCount() > startCount)
        {
            for (var i = startCount; i < CurrentColumnizer.GetColumnCount(); i++)
            {
                var colName = CurrentColumnizer.GetColumnNames()[i];
                dataGridView.Columns.Add(PaintHelper.CreateTitleColumn(colName));
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnDataGridViewCellValuePushed (object sender, DataGridViewCellValueEventArgs e)
    {
        if (!CurrentColumnizer.IsTimeshiftImplemented())
        {
            return;
        }

        ILogLine line = _logFileReader.GetLogLine(e.RowIndex);
        var offset = CurrentColumnizer.GetTimeOffset();
        CurrentColumnizer.SetTimeOffset(0);
        ColumnizerCallbackObject.SetLineNum(e.RowIndex);
        IColumnizedLogLine cols = CurrentColumnizer.SplitLine(ColumnizerCallbackObject, line);
        CurrentColumnizer.SetTimeOffset(offset);
        if (cols.ColumnValues.Length <= e.ColumnIndex - 2)
        {
            return;
        }

        var oldValue = cols.ColumnValues[e.ColumnIndex - 2].FullValue;
        var newValue = (string)e.Value;
        //string oldValue = (string) this.dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
        CurrentColumnizer.PushValue(ColumnizerCallbackObject, e.ColumnIndex - 2, newValue, oldValue);
        dataGridView.Refresh();
        TimeSpan timeSpan = new(CurrentColumnizer.GetTimeOffset() * TimeSpan.TicksPerMillisecond);
        var span = timeSpan.ToString();
        var index = span.LastIndexOf('.');
        if (index > 0)
        {
            span = span.Substring(0, index + 4);
        }

        SetTimeshiftValue(span);
        SendGuiStateUpdate();
    }

    [SupportedOSPlatform("windows")]
    private void OnDataGridViewRowHeightInfoNeeded (object sender, DataGridViewRowHeightInfoNeededEventArgs e)
    {
        e.Height = GetRowHeight(e.RowIndex);
    }

    [SupportedOSPlatform("windows")]
    private void OnDataGridViewCurrentCellChanged (object sender, EventArgs e)
    {
        if (dataGridView.CurrentRow != null)
        {
            _statusEventArgs.CurrentLineNum = dataGridView.CurrentRow.Index + 1;
            SendStatusLineUpdate();
            if (syncFilterCheckBox.Checked)
            {
                SyncFilterGridPos();
            }

            if (CurrentColumnizer.IsTimeshiftImplemented() && Preferences.TimestampControl)
            {
                SyncTimestampDisplay();
            }

            //MethodInvoker invoker = new MethodInvoker(DisplayCurrentFileOnStatusline);
            //invoker.BeginInvoke(null, null);
        }
    }

    private void OnDataGridViewCellEndEdit (object sender, DataGridViewCellEventArgs e)
    {
        StatusLineText(string.Empty);
    }

    [SupportedOSPlatform("windows")]
    private void OnEditControlKeyUp (object sender, KeyEventArgs e)
    {
        UpdateEditColumnDisplay((DataGridViewTextBoxEditingControl)sender);
    }

    [SupportedOSPlatform("windows")]
    private void OnEditControlKeyPress (object sender, KeyPressEventArgs e)
    {
        UpdateEditColumnDisplay((DataGridViewTextBoxEditingControl)sender);
    }

    [SupportedOSPlatform("windows")]
    private void OnEditControlClick (object sender, EventArgs e)
    {
        UpdateEditColumnDisplay((DataGridViewTextBoxEditingControl)sender);
    }

    [SupportedOSPlatform("windows")]
    private void OnEditControlKeyDown (object sender, KeyEventArgs e)
    {
        UpdateEditColumnDisplay((DataGridViewTextBoxEditingControl)sender);
    }

    [SupportedOSPlatform("windows")]
    private void OnDataGridViewPaint (object sender, PaintEventArgs e)
    {
        if (ShowBookmarkBubbles)
        {
            AddBookmarkOverlays();
        }
    }

    // ======================================================================================
    // Filter Grid stuff
    // ======================================================================================

    [SupportedOSPlatform("windows")]
    private void OnFilterSearchButtonClick (object sender, EventArgs e)
    {
        FilterSearch();
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterGridViewCellPainting (object sender, DataGridViewCellPaintingEventArgs e)
    {
        var gridView = (BufferedDataGridView)sender;

        if (e.RowIndex < 0 || e.ColumnIndex < 0 || _filterResultList.Count <= e.RowIndex)
        {
            e.Handled = false;
            return;
        }

        var lineNum = _filterResultList[e.RowIndex];
        ILogLine line = _logFileReader.GetLogLineWithWait(lineNum).Result;

        if (line != null)
        {
            HighlightEntry entry = FindFirstNoWordMatchHilightEntry(line);
            e.Graphics.SetClip(e.CellBounds);
            if ((e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected)
            {
                Brush brush;
                if (gridView.Focused)
                {
                    brush = new SolidBrush(e.CellStyle.SelectionBackColor);
                }
                else
                {
                    var color = Color.FromArgb(255, 170, 170, 170);
                    brush = new SolidBrush(color);
                }

                e.Graphics.FillRectangle(brush, e.CellBounds);
                brush.Dispose();
            }
            else
            {
                Color bgColor = Color.White;
                // paint direct filter hits with different bg color
                //if (this.filterParams.SpreadEnabled && this.filterHitList.Contains(lineNum))
                //{
                //  bgColor = Color.FromArgb(255, 220, 220, 220);
                //}
                if (!DebugOptions.DisableWordHighlight)
                {
                    if (entry != null)
                    {
                        bgColor = entry.BackgroundColor;
                    }
                }
                else
                {
                    if (entry != null)
                    {
                        bgColor = entry.BackgroundColor;
                    }
                }

                e.CellStyle.BackColor = bgColor;
                e.PaintBackground(e.ClipBounds, false);
            }

            if (DebugOptions.DisableWordHighlight)
            {
                e.PaintContent(e.CellBounds);
            }
            else
            {
                PaintCell(e, filterGridView, false, entry);
            }

            if (e.ColumnIndex == 0)
            {
                if (_bookmarkProvider.IsBookmarkAtLine(lineNum))
                {
                    Rectangle r = new(e.CellBounds.Left + 2, e.CellBounds.Top + 2, 6, 6);
                    r = e.CellBounds;
                    r.Inflate(-2, -2);
                    Brush brush = new SolidBrush(BookmarkColor);
                    e.Graphics.FillRectangle(brush, r);
                    brush.Dispose();

                    Bookmark bookmark = _bookmarkProvider.GetBookmarkForLine(lineNum);

                    if (bookmark.Text.Length > 0)
                    {
                        StringFormat format = new()
                        {
                            LineAlignment = StringAlignment.Center,
                            Alignment = StringAlignment.Center
                        };

                        Brush brush2 = new SolidBrush(Color.FromArgb(255, 190, 100, 0));
                        Font font = new("Verdana", Preferences.FontSize, FontStyle.Bold);
                        e.Graphics.DrawString("!", font, brush2, new RectangleF(r.Left, r.Top, r.Width, r.Height), format);
                        font.Dispose();
                        brush2.Dispose();
                    }
                }
            }

            e.Paint(e.CellBounds, DataGridViewPaintParts.Border);
            e.Handled = true;
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterGridViewCellValueNeeded (object sender, DataGridViewCellValueEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || _filterResultList.Count <= e.RowIndex)
        {
            e.Value = "";
            return;
        }

        var lineNum = _filterResultList[e.RowIndex];
        e.Value = GetCellValue(lineNum, e.ColumnIndex);
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterGridViewRowHeightInfoNeeded (object sender, DataGridViewRowHeightInfoNeededEventArgs e)
    {
        e.Height = _lineHeight;
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterComboBoxKeyDown (object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            FilterSearch();
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterGridViewColumnDividerDoubleClick (object sender,
        DataGridViewColumnDividerDoubleClickEventArgs e)
    {
        e.Handled = true;
        AutoResizeColumnsFx fx = AutoResizeColumns;
        BeginInvoke(fx, filterGridView);
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterGridViewCellDoubleClick (object sender, DataGridViewCellEventArgs e)
    {
        if (e.ColumnIndex == 0)
        {
            ToggleBookmark();
            return;
        }

        if (filterGridView.CurrentRow != null && e.RowIndex >= 0)
        {
            var lineNum = _filterResultList[filterGridView.CurrentRow.Index];
            SelectAndEnsureVisible(lineNum, true);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnRangeCheckBoxCheckedChanged (object sender, EventArgs e)
    {
        filterRangeComboBox.Enabled = rangeCheckBox.Checked;
        CheckForFilterDirty();
    }

    [SupportedOSPlatform("windows")]
    private void OnDataGridViewScroll (object sender, ScrollEventArgs e)
    {
        if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
        {
            if (dataGridView.DisplayedRowCount(false) + dataGridView.FirstDisplayedScrollingRowIndex >= dataGridView.RowCount)
            {
                //this.guiStateArgs.FollowTail = true;
                if (!_guiStateArgs.FollowTail)
                {
                    FollowTailChanged(true, false);
                }

                OnTailFollowed(EventArgs.Empty);
            }
            else
            {
                //this.guiStateArgs.FollowTail = false;
                if (_guiStateArgs.FollowTail)
                {
                    FollowTailChanged(false, false);
                }
            }

            SendGuiStateUpdate();
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterGridViewKeyDown (object sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Enter:
                {
                    if (filterGridView.CurrentCellAddress.Y >= 0 && filterGridView.CurrentCellAddress.Y < _filterResultList.Count)
                    {
                        var lineNum = _filterResultList[filterGridView.CurrentCellAddress.Y];
                        SelectLine(lineNum, false, true);
                        e.Handled = true;
                    }

                    break;
                }
            case Keys.Tab when e.Modifiers == Keys.None:
                dataGridView.Focus();
                e.Handled = true;
                break;
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnDataGridViewKeyDown (object sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Tab when e.Modifiers == Keys.None:
                {
                    filterGridView.Focus();
                    e.Handled = true;
                    break;
                }
        }

        //switch (e.KeyCode)
        //{
        //    case Keys.Tab when e.Modifiers == Keys.Control:
        //        //this.parentLogTabWin.SwitchTab(e.Shift);
        //        break;
        //}

        _shouldCallTimeSync = true;
    }

    [SupportedOSPlatform("windows")]
    private void OnDataGridViewPreviewKeyDown (object sender, PreviewKeyDownEventArgs e)
    {
        if (e.KeyCode == Keys.Tab && e.Control)
        {
            e.IsInputKey = true;
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnDataGridViewCellContentDoubleClick (object sender, DataGridViewCellEventArgs e)
    {
        if (dataGridView.CurrentCell != null)
        {
            dataGridView.BeginEdit(false);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnSyncFilterCheckBoxCheckedChanged (object sender, EventArgs e)
    {
        if (syncFilterCheckBox.Checked)
        {
            SyncFilterGridPos();
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnDataGridViewLeave (object sender, EventArgs e)
    {
        InvalidateCurrentRow(dataGridView);
    }

    [SupportedOSPlatform("windows")]
    private void OnDataGridViewEnter (object sender, EventArgs e)
    {
        InvalidateCurrentRow(dataGridView);
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterGridViewEnter (object sender, EventArgs e)
    {
        InvalidateCurrentRow(filterGridView);
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterGridViewLeave (object sender, EventArgs e)
    {
        InvalidateCurrentRow(filterGridView);
    }

    [SupportedOSPlatform("windows")]
    private void OnDataGridViewResize (object sender, EventArgs e)
    {
        if (_logFileReader != null && dataGridView.RowCount > 0 && _guiStateArgs.FollowTail)
        {
            dataGridView.FirstDisplayedScrollingRowIndex = dataGridView.RowCount - 1;
        }
    }

    private void OnDataGridViewSelectionChanged (object sender, EventArgs e)
    {
        UpdateSelectionDisplay();
    }

    [SupportedOSPlatform("windows")]
    private void OnSelectionChangedTriggerSignal (object sender, EventArgs e)
    {
        var selCount = 0;
        try
        {
            _logger.Debug("Selection changed trigger");
            selCount = dataGridView.SelectedRows.Count;
            if (selCount > 1)
            {
                StatusLineText(selCount + " selected lines");
            }
            else
            {
                if (IsMultiFile)
                {
                    MethodInvoker invoker = DisplayCurrentFileOnStatusline;
                    invoker.BeginInvoke(null, null);
                }
                else
                {
                    StatusLineText("");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in selectionChangedTrigger_Signal selcount {0}", selCount);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterKnobControlValueChanged (object sender, EventArgs e)
    {
        CheckForFilterDirty();
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterToTabButtonClick (object sender, EventArgs e)
    {
        FilterToTab();
    }

    private void OnPipeDisconnected (object sender, EventArgs e)
    {
        if (sender.GetType() == typeof(FilterPipe))
        {
            lock (_filterPipeList)
            {
                _filterPipeList.Remove((FilterPipe)sender);
                if (_filterPipeList.Count == 0)
                // reset naming counter to 0 if no more open filter tabs for this source window
                {
                    _filterPipeNameCounter = 0;
                }
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnAdvancedButtonClick (object sender, EventArgs e)
    {
        _showAdvanced = !_showAdvanced;
        ShowAdvancedFilterPanel(_showAdvanced);
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterSplitContainerMouseDown (object sender, MouseEventArgs e)
    {
        ((SplitContainer)sender).IsSplitterFixed = true;
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterSplitContainerMouseUp (object sender, MouseEventArgs e)
    {
        ((SplitContainer)sender).IsSplitterFixed = false;
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterSplitContainerMouseMove (object sender, MouseEventArgs e)
    {
        var splitContainer = (SplitContainer)sender;
        if (splitContainer.IsSplitterFixed)
        {
            if (e.Button.Equals(MouseButtons.Left))
            {
                if (splitContainer.Orientation.Equals(Orientation.Vertical))
                {
                    if (e.X > 0 && e.X < splitContainer.Width)
                    {
                        splitContainer.SplitterDistance = e.X;
                        splitContainer.Refresh();
                    }
                }
                else
                {
                    if (e.Y > 0 && e.Y < splitContainer.Height)
                    {
                        splitContainer.SplitterDistance = e.Y;
                        splitContainer.Refresh();
                    }
                }
            }
            else
            {
                splitContainer.IsSplitterFixed = false;
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterSplitContainerMouseDoubleClick (object sender, MouseEventArgs e)
    {
        AutoResizeFilterBox();
    }

    #region Context Menu

    [SupportedOSPlatform("windows")]
    private void OnDataGridContextMenuStripOpening (object sender, CancelEventArgs e)
    {
        var lineNum = -1;
        if (dataGridView.CurrentRow != null)
        {
            lineNum = dataGridView.CurrentRow.Index;
        }

        if (lineNum == -1)
        {
            return;
        }

        var refLineNum = lineNum;

        copyToTabToolStripMenuItem.Enabled = dataGridView.SelectedCells.Count > 0;
        scrollAllTabsToTimestampToolStripMenuItem.Enabled = CurrentColumnizer.IsTimeshiftImplemented()
                                                            &&
                                                            GetTimestampForLine(ref refLineNum, false) !=
                                                            DateTime.MinValue;

        locateLineInOriginalFileToolStripMenuItem.Enabled = IsTempFile &&
                                                            FilterPipe != null &&
                                                            FilterPipe.GetOriginalLineNum(lineNum) != -1;

        markEditModeToolStripMenuItem.Enabled = !dataGridView.CurrentCell.ReadOnly;

        // Remove all "old" plugin entries
        var index = dataGridContextMenuStrip.Items.IndexOf(pluginSeparator);

        if (index > 0)
        {
            for (var i = index + 1; i < dataGridContextMenuStrip.Items.Count;)
            {
                dataGridContextMenuStrip.Items.RemoveAt(i);
            }
        }

        // Add plugin entries
        var isAdded = false;
        if (PluginRegistry.PluginRegistry.Instance.RegisteredContextMenuPlugins.Count > 0)
        {
            IList<int> lines = GetSelectedContent();
            foreach (IContextMenuEntry entry in PluginRegistry.PluginRegistry.Instance.RegisteredContextMenuPlugins)
            {
                LogExpertCallback callback = new(this);
                var menuText = entry.GetMenuText(lines.Count, CurrentColumnizer, callback.GetLogLine(lines[0]));

                if (menuText != null)
                {
                    var disabled = menuText.StartsWith('_');
                    if (disabled)
                    {
                        menuText = menuText[1..];
                    }

                    ToolStripItem item = dataGridContextMenuStrip.Items.Add(menuText, null, OnHandlePluginContextMenu);
                    item.Tag = new ContextMenuPluginEventArgs(entry, lines, CurrentColumnizer, callback);
                    item.Enabled = !disabled;
                    isAdded = true;
                }
            }
        }

        pluginSeparator.Visible = isAdded;

        // enable/disable Temp Highlight item
        tempHighlightsToolStripMenuItem.Enabled = _tempHighlightEntryList.Count > 0;

        markCurrentFilterRangeToolStripMenuItem.Enabled = string.IsNullOrEmpty(filterRangeComboBox.Text) == false;

        if (CurrentColumnizer.IsTimeshiftImplemented())
        {
            IList<WindowFileEntry> list = _parentLogTabWin.GetListOfOpenFiles();
            syncTimestampsToToolStripMenuItem.Enabled = true;
            syncTimestampsToToolStripMenuItem.DropDownItems.Clear();
            EventHandler ev = OnHandleSyncContextMenu;
            Font italicFont = new(syncTimestampsToToolStripMenuItem.Font.FontFamily, syncTimestampsToToolStripMenuItem.Font.Size, FontStyle.Italic);

            foreach (WindowFileEntry fileEntry in list)
            {
                if (fileEntry.LogWindow != this)
                {
                    var item = syncTimestampsToToolStripMenuItem.DropDownItems.Add(fileEntry.Title, null, ev) as ToolStripMenuItem;
                    item.Tag = fileEntry;
                    item.Checked = TimeSyncList != null && TimeSyncList.Contains(fileEntry.LogWindow);
                    if (fileEntry.LogWindow.TimeSyncList != null && !fileEntry.LogWindow.TimeSyncList.Contains(this))
                    {
                        item.Font = italicFont;
                        item.ForeColor = Color.Blue;
                    }

                    item.Enabled = fileEntry.LogWindow.CurrentColumnizer.IsTimeshiftImplemented();
                }
            }
        }
        else
        {
            syncTimestampsToToolStripMenuItem.Enabled = false;
        }

        freeThisWindowFromTimeSyncToolStripMenuItem.Enabled = TimeSyncList != null &&
                                                              TimeSyncList.Count > 1;
    }

    [SupportedOSPlatform("windows")]
    private void OnHandlePluginContextMenu (object sender, EventArgs args)
    {
        if (sender is ToolStripItem item)
        {
            var menuArgs = item.Tag as ContextMenuPluginEventArgs;
            IList<int> logLines = menuArgs.LogLines;
            menuArgs.Entry.MenuSelected(logLines.Count, menuArgs.Columnizer, menuArgs.Callback.GetLogLine(logLines[0]));
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnHandleSyncContextMenu (object sender, EventArgs args)
    {
        if (sender is ToolStripItem item)
        {
            var entry = item.Tag as WindowFileEntry;

            if (TimeSyncList != null && TimeSyncList.Contains(entry.LogWindow))
            {
                FreeSlaveFromTimesync(entry.LogWindow);
            }
            else
            //AddSlaveToTimesync(entry.LogWindow);
            {
                AddOtherWindowToTimesync(entry.LogWindow);
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnCopyToolStripMenuItemClick (object sender, EventArgs e)
    {
        CopyMarkedLinesToClipboard();
    }

    private void OnCopyToTabToolStripMenuItemClick (object sender, EventArgs e)
    {
        CopyMarkedLinesToTab();
    }

    [SupportedOSPlatform("windows")]
    private void OnScrollAllTabsToTimestampToolStripMenuItemClick (object sender, EventArgs e)
    {
        if (CurrentColumnizer.IsTimeshiftImplemented())
        {
            var currentLine = dataGridView.CurrentCellAddress.Y;
            if (currentLine > 0 && currentLine < dataGridView.RowCount)
            {
                var lineNum = currentLine;
                DateTime timeStamp = GetTimestampForLine(ref lineNum, false);
                if (timeStamp.Equals(DateTime.MinValue)) // means: invalid
                {
                    return;
                }

                _parentLogTabWin.ScrollAllTabsToTimestamp(timeStamp, this);
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnLocateLineInOriginalFileToolStripMenuItemClick (object sender, EventArgs e)
    {
        if (dataGridView.CurrentRow != null && FilterPipe != null)
        {
            var lineNum = FilterPipe.GetOriginalLineNum(dataGridView.CurrentRow.Index);
            if (lineNum != -1)
            {
                FilterPipe.LogWindow.SelectLine(lineNum, false, true);
                _parentLogTabWin.SelectTab(FilterPipe.LogWindow);
            }
        }
    }

    private void OnToggleBoomarkToolStripMenuItemClick (object sender, EventArgs e)
    {
        ToggleBookmark();
    }

    [SupportedOSPlatform("windows")]
    private void OnMarkEditModeToolStripMenuItemClick (object sender, EventArgs e)
    {
        StartEditMode();
    }

    private void OnLogWindowSizeChanged (object sender, EventArgs e)
    {
        //AdjustMinimumGridWith();
        AdjustHighlightSplitterWidth();
    }

    #region BookMarkList

    [SupportedOSPlatform("windows")]
    private void OnColumnRestrictCheckBoxCheckedChanged (object sender, EventArgs e)
    {
        columnButton.Enabled = columnRestrictCheckBox.Checked;
        if (columnRestrictCheckBox.Checked) // disable when nothing to filter
        {
            columnNamesLabel.Visible = true;
            _filterParams.ColumnRestrict = true;
            columnNamesLabel.Text = CalculateColumnNames(_filterParams);
        }
        else
        {
            columnNamesLabel.Visible = false;
        }

        CheckForFilterDirty();
    }

    [SupportedOSPlatform("windows")]
    private void OnColumnButtonClick (object sender, EventArgs e)
    {
        _filterParams.CurrentColumnizer = _currentColumnizer;
        FilterColumnChooser chooser = new(_filterParams);
        if (chooser.ShowDialog() == DialogResult.OK)
        {
            columnNamesLabel.Text = CalculateColumnNames(_filterParams);

            //CheckForFilterDirty(); //!!!GBro: Indicate to redo the search if search columns were changed
            filterSearchButton.Image = _searchButtonImage;
            saveFilterButton.Enabled = false;
        }
    }

    #endregion

    #region Column Header Context Menu

    [SupportedOSPlatform("windows")]
    private void OnDataGridViewCellContextMenuStripNeeded (object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
    {
        if (e.RowIndex >= 0 && e.RowIndex < dataGridView.RowCount && !dataGridView.Rows[e.RowIndex].Selected)
        {
            SelectLine(e.RowIndex, false, true);
        }
        else if (e.RowIndex < 0)
        {
            e.ContextMenuStrip = columnContextMenuStrip;
        }

        if (e.ContextMenuStrip == columnContextMenuStrip)
        {
            _selectedCol = e.ColumnIndex;
        }
    }

    //private void boomarkDataGridView_CellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
    //{
    //  if (e.RowIndex > 0 && e.RowIndex < this.boomarkDataGridView.RowCount
    //      && !this.boomarkDataGridView.Rows[e.RowIndex].Selected)
    //  {
    //    this.boomarkDataGridView.Rows[e.RowIndex].Selected = true;
    //    this.boomarkDataGridView.CurrentCell = this.boomarkDataGridView.Rows[e.RowIndex].Cells[0];
    //  }
    //  if (e.ContextMenuStrip == this.columnContextMenuStrip)
    //  {
    //    this.selectedCol = e.ColumnIndex;
    //  }
    //}

    [SupportedOSPlatform("windows")]
    private void OnFilterGridViewCellContextMenuStripNeeded (object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
    {
        if (e.ContextMenuStrip == columnContextMenuStrip)
        {
            _selectedCol = e.ColumnIndex;
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnColumnContextMenuStripOpening (object sender, CancelEventArgs e)
    {
        Control ctl = columnContextMenuStrip.SourceControl;
        var gridView = ctl as BufferedDataGridView;
        var frozen = false;
        if (_freezeStateMap.TryGetValue(ctl, out var value))
        {
            frozen = value;
        }

        freezeLeftColumnsUntilHereToolStripMenuItem.Checked = frozen;

        if (frozen)
        {
            freezeLeftColumnsUntilHereToolStripMenuItem.Text = "Frozen";
        }
        else
        {
            if (ctl is BufferedDataGridView)
            {
                freezeLeftColumnsUntilHereToolStripMenuItem.Text = $"Freeze left columns until here ({gridView.Columns[_selectedCol].HeaderText})";
            }
        }


        DataGridViewColumn col = gridView.Columns[_selectedCol];
        moveLeftToolStripMenuItem.Enabled = col != null && col.DisplayIndex > 0;
        moveRightToolStripMenuItem.Enabled = col != null && col.DisplayIndex < gridView.Columns.Count - 1;

        if (gridView.Columns.Count - 1 > _selectedCol)
        {
            //        DataGridViewColumn colRight = gridView.Columns[this.selectedCol + 1];
            DataGridViewColumn colRight = gridView.Columns.GetNextColumn(col, DataGridViewElementStates.None, DataGridViewElementStates.None);
            moveRightToolStripMenuItem.Enabled = colRight != null && colRight.Frozen == col.Frozen;
        }

        if (_selectedCol > 0)
        {
            //DataGridViewColumn colLeft = gridView.Columns[this.selectedCol - 1];
            DataGridViewColumn colLeft = gridView.Columns.GetPreviousColumn(col, DataGridViewElementStates.None, DataGridViewElementStates.None);

            moveLeftToolStripMenuItem.Enabled = colLeft != null && colLeft.Frozen == col.Frozen;
        }

        DataGridViewColumn colLast = gridView.Columns[gridView.Columns.Count - 1];
        moveToLastColumnToolStripMenuItem.Enabled = colLast != null && colLast.Frozen == col.Frozen;

        // Fill context menu with column names
        //
        EventHandler ev = OnHandleColumnItemContextMenu;
        allColumnsToolStripMenuItem.DropDownItems.Clear();
        foreach (DataGridViewColumn column in gridView.Columns)
        {
            if (column.HeaderText.Length > 0)
            {
                var item = allColumnsToolStripMenuItem.DropDownItems.Add(column.HeaderText, null, ev) as ToolStripMenuItem;
                item.Tag = column;
                item.Enabled = !column.Frozen;
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnHandleColumnItemContextMenu (object sender, EventArgs args)
    {
        if (sender is ToolStripItem item)
        {
            var column = item.Tag as DataGridViewColumn;
            column.Visible = true;
            column.DataGridView.FirstDisplayedScrollingColumnIndex = column.Index;
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnFreezeLeftColumnsUntilHereToolStripMenuItemClick (object sender, EventArgs e)
    {
        Control ctl = columnContextMenuStrip.SourceControl;
        var frozen = false;

        if (_freezeStateMap.TryGetValue(ctl, out var value))
        {
            frozen = value;
        }

        frozen = !frozen;
        _freezeStateMap[ctl] = frozen;

        if (ctl is BufferedDataGridView gridView)
        {
            ApplyFrozenState(gridView);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnMoveToLastColumnToolStripMenuItemClick (object sender, EventArgs e)
    {
        var gridView = columnContextMenuStrip.SourceControl as BufferedDataGridView;
        DataGridViewColumn col = gridView.Columns[_selectedCol];
        if (col != null)
        {
            col.DisplayIndex = gridView.Columns.Count - 1;
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnMoveLeftToolStripMenuItemClick (object sender, EventArgs e)
    {
        var gridView = columnContextMenuStrip.SourceControl as BufferedDataGridView;
        DataGridViewColumn col = gridView.Columns[_selectedCol];
        if (col != null && col.DisplayIndex > 0)
        {
            col.DisplayIndex -= 1;
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnMoveRightToolStripMenuItemClick (object sender, EventArgs e)
    {
        var gridView = columnContextMenuStrip.SourceControl as BufferedDataGridView;
        DataGridViewColumn col = gridView.Columns[_selectedCol];
        if (col != null && col.DisplayIndex < gridView.Columns.Count - 1)
        {
            col.DisplayIndex = col.DisplayIndex + 1;
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnHideColumnToolStripMenuItemClick (object sender, EventArgs e)
    {
        var gridView = columnContextMenuStrip.SourceControl as BufferedDataGridView;
        DataGridViewColumn col = gridView.Columns[_selectedCol];
        col.Visible = false;
    }

    [SupportedOSPlatform("windows")]
    private void OnRestoreColumnsToolStripMenuItemClick (object sender, EventArgs e)
    {
        var gridView = columnContextMenuStrip.SourceControl as BufferedDataGridView;
        foreach (DataGridViewColumn col in gridView.Columns)
        {
            col.Visible = true;
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnTimeSpreadingControlLineSelected (object sender, SelectLineEventArgs e)
    {
        SelectLine(e.Line, false, true);
    }

    [SupportedOSPlatform("windows")]
    private void OnBookmarkCommentToolStripMenuItemClick (object sender, EventArgs e)
    {
        AddBookmarkAndEditComment();
    }

    [SupportedOSPlatform("windows")]
    private void OnHighlightSelectionInLogFileToolStripMenuItemClick (object sender, EventArgs e)
    {
        if (dataGridView.EditingControl is DataGridViewTextBoxEditingControl ctl)
        {
            var he = new HighlightEntry()
            {
                SearchText = ctl.SelectedText,
                ForegroundColor = Color.Red,
                BackgroundColor = Color.Yellow,
                IsRegEx = false,
                IsCaseSensitive = true,
                IsLedSwitch = false,
                IsSetBookmark = false,
                IsActionEntry = false,
                ActionEntry = null,
                IsWordMatch = false
            };

            lock (_tempHighlightEntryListLock)
            {
                _tempHighlightEntryList.Add(he);
            }

            dataGridView.CancelEdit();
            dataGridView.EndEdit();
            RefreshAllGrids();
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnHighlightSelectionInLogFilewordModeToolStripMenuItemClick (object sender, EventArgs e)
    {
        if (dataGridView.EditingControl is DataGridViewTextBoxEditingControl ctl)
        {
            HighlightEntry he = new()
            {
                SearchText = ctl.SelectedText,
                ForegroundColor = Color.Red,
                BackgroundColor = Color.Yellow,
                IsRegEx = false,
                IsCaseSensitive = true,
                IsLedSwitch = false,
                IsStopTail = false,
                IsSetBookmark = false,
                IsActionEntry = false,
                ActionEntry = null,
                IsWordMatch = true
            };

            lock (_tempHighlightEntryListLock)
            {
                _tempHighlightEntryList.Add(he);
            }

            dataGridView.CancelEdit();
            dataGridView.EndEdit();
            RefreshAllGrids();
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnEditModeCopyToolStripMenuItemClick (object sender, EventArgs e)
    {
        if (dataGridView.EditingControl is DataGridViewTextBoxEditingControl ctl)
        {
            if (Util.IsNull(ctl.SelectedText) == false)
            {
                Clipboard.SetText(ctl.SelectedText);
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnRemoveAllToolStripMenuItemClick (object sender, EventArgs e)
    {
        RemoveTempHighlights();
    }

    [SupportedOSPlatform("windows")]
    private void OnMakePermanentToolStripMenuItemClick (object sender, EventArgs e)
    {
        lock (_tempHighlightEntryListLock)
        {
            lock (_currentHighlightGroupLock)
            {
                _currentHighlightGroup.HighlightEntryList.AddRange(_tempHighlightEntryList);
                RemoveTempHighlights();
                OnCurrentHighlightListChanged();
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnMarkCurrentFilterRangeToolStripMenuItemClick (object sender, EventArgs e)
    {
        MarkCurrentFilterRange();
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterForSelectionToolStripMenuItemClick (object sender, EventArgs e)
    {
        if (dataGridView.EditingControl is DataGridViewTextBoxEditingControl ctl)
        {
            splitContainerLogWindow.Panel2Collapsed = false;
            ResetFilterControls();
            FilterSearch(ctl.SelectedText);
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnSetSelectedTextAsBookmarkCommentToolStripMenuItemClick (object sender, EventArgs e)
    {
        if (dataGridView.EditingControl is DataGridViewTextBoxEditingControl ctl)
        {
            AddBookmarkComment(ctl.SelectedText);
        }
    }

    private void OnDataGridViewCellClick (object sender, DataGridViewCellEventArgs e)
    {
        _shouldCallTimeSync = true;
    }

    [SupportedOSPlatform("windows")]
    private void OnDataGridViewCellDoubleClick (object sender, DataGridViewCellEventArgs e)
    {
        if (e.ColumnIndex == 0)
        {
            ToggleBookmark();
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnDataGridViewOverlayDoubleClicked (object sender, OverlayEventArgs e)
    {
        BookmarkComment(e.BookmarkOverlay.Bookmark);
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterRegexCheckBoxMouseUp (object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            RegexHelperDialog dlg = new()
            {
                ExpressionHistoryList = ConfigManager.Settings.RegexHistory.ExpressionHistoryList,
                TesttextHistoryList = ConfigManager.Settings.RegexHistory.TesttextHistoryList,
                Owner = this,
                CaseSensitive = filterCaseSensitiveCheckBox.Checked,
                Pattern = filterComboBox.Text
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ConfigManager.Settings.RegexHistory.ExpressionHistoryList = dlg.ExpressionHistoryList;
                ConfigManager.Settings.RegexHistory.TesttextHistoryList = dlg.TesttextHistoryList;

                filterCaseSensitiveCheckBox.Checked = dlg.CaseSensitive;
                filterComboBox.Text = dlg.Pattern;

                ConfigManager.Save(SettingsFlags.RegexHistory);
            }
        }
    }

    #endregion

    #region Filter-Highlight

    [SupportedOSPlatform("windows")]
    private void OnToggleHighlightPanelButtonClick (object sender, EventArgs e)
    {
        ToggleHighlightPanel(highlightSplitContainer.Panel2Collapsed);
    }

    private void OnSaveFilterButtonClick (object sender, EventArgs e)
    {
        FilterParams newParams = _filterParams.Clone();
        newParams.Color = Color.FromKnownColor(KnownColor.Black);
        ConfigManager.Settings.FilterList.Add(newParams);
        OnFilterListChanged(this);
    }

    [SupportedOSPlatform("windows")]
    private void OnDeleteFilterButtonClick (object sender, EventArgs e)
    {
        var index = filterListBox.SelectedIndex;
        if (index >= 0)
        {
            var filterParams = (FilterParams)filterListBox.Items[index];
            ConfigManager.Settings.FilterList.Remove(filterParams);
            OnFilterListChanged(this);
            if (filterListBox.Items.Count > 0)
            {
                filterListBox.SelectedIndex = filterListBox.Items.Count - 1;
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterUpButtonClick (object sender, EventArgs e)
    {
        var i = filterListBox.SelectedIndex;
        if (i > 0)
        {
            var filterParams = (FilterParams)filterListBox.Items[i];
            ConfigManager.Settings.FilterList.RemoveAt(i);
            i--;
            ConfigManager.Settings.FilterList.Insert(i, filterParams);
            OnFilterListChanged(this);
            filterListBox.SelectedIndex = i;
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterDownButtonClick (object sender, EventArgs e)
    {
        var i = filterListBox.SelectedIndex;
        if (i < 0)
        {
            return;
        }

        if (i < filterListBox.Items.Count - 1)
        {
            var filterParams = (FilterParams)filterListBox.Items[i];
            ConfigManager.Settings.FilterList.RemoveAt(i);
            i++;
            ConfigManager.Settings.FilterList.Insert(i, filterParams);
            OnFilterListChanged(this);
            filterListBox.SelectedIndex = i;
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterListBoxMouseDoubleClick (object sender, MouseEventArgs e)
    {
        if (filterListBox.SelectedIndex >= 0)
        {
            var filterParams = (FilterParams)filterListBox.Items[filterListBox.SelectedIndex];
            FilterParams newParams = filterParams.Clone();
            //newParams.historyList = ConfigManager.Settings.filterHistoryList;
            _filterParams = newParams;
            ReInitFilterParams(_filterParams);
            ApplyFilterParams();
            CheckForAdvancedButtonDirty();
            CheckForFilterDirty();
            filterSearchButton.Image = _searchButtonImage;
            saveFilterButton.Enabled = false;
            if (hideFilterListOnLoadCheckBox.Checked)
            {
                ToggleHighlightPanel(false);
            }

            if (filterOnLoadCheckBox.Checked)
            {
                FilterSearch();
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterListBoxDrawItem (object sender, DrawItemEventArgs e)
    {
        e.DrawBackground();
        if (e.Index >= 0)
        {
            var filterParams = (FilterParams)filterListBox.Items[e.Index];
            Rectangle rectangle = new(0, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);

            Brush brush;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                brush = new SolidBrush(filterListBox.BackColor);
            }
            else
            {
                brush = new SolidBrush(filterParams.Color);
            }

            e.Graphics.DrawString(filterParams.SearchText, e.Font, brush,
                new PointF(rectangle.Left, rectangle.Top));
            e.DrawFocusRectangle();
            brush.Dispose();
        }
    }

    [SupportedOSPlatform("windows")]
    // Color for filter list entry
    private void OnColorToolStripMenuItemClick (object sender, EventArgs e)
    {
        var i = filterListBox.SelectedIndex;
        if (i < filterListBox.Items.Count && i >= 0)
        {
            var filterParams = (FilterParams)filterListBox.Items[i];
            ColorDialog dlg = new()
            {
                CustomColors = [filterParams.Color.ToArgb()],
                Color = filterParams.Color
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                filterParams.Color = dlg.Color;
                filterListBox.Refresh();
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterCaseSensitiveCheckBoxCheckedChanged (object sender, EventArgs e)
    {
        CheckForFilterDirty();
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterRegexCheckBoxCheckedChanged (object sender, EventArgs e)
    {
        fuzzyKnobControl.Enabled = !filterRegexCheckBox.Checked;
        fuzzyLabel.Enabled = !filterRegexCheckBox.Checked;
        CheckForFilterDirty();
    }

    [SupportedOSPlatform("windows")]
    private void OnInvertFilterCheckBoxCheckedChanged (object sender, EventArgs e)
    {
        CheckForFilterDirty();
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterRangeComboBoxTextChanged (object sender, EventArgs e)
    {
        CheckForFilterDirty();
    }

    [SupportedOSPlatform("windows")]
    private void OnFuzzyKnobControlValueChanged (object sender, EventArgs e)
    {
        CheckForFilterDirty();
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterComboBoxTextChanged (object sender, EventArgs e)
    {
        CheckForFilterDirty();
    }

    [SupportedOSPlatform("windows")]
    private void OnSetBookmarksOnSelectedLinesToolStripMenuItemClick (object sender, EventArgs e)
    {
        SetBookmarksForSelectedFilterLines();
    }

    private void OnParentHighlightSettingsChanged (object sender, EventArgs e)
    {
        var groupName = _guiStateArgs.HighlightGroupName;
        SetCurrentHighlightGroup(groupName);
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterOnLoadCheckBoxMouseClick (object sender, MouseEventArgs e)
    {
        HandleChangedFilterOnLoadSetting();
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterOnLoadCheckBoxKeyPress (object sender, KeyPressEventArgs e)
    {
        HandleChangedFilterOnLoadSetting();
    }

    [SupportedOSPlatform("windows")]
    private void OnHideFilterListOnLoadCheckBoxMouseClick (object sender, MouseEventArgs e)
    {
        HandleChangedFilterOnLoadSetting();
    }

    [SupportedOSPlatform("windows")]
    private void OnFilterToTabToolStripMenuItemClick (object sender, EventArgs e)
    {
        FilterToTab();
    }

    private void OnTimeSyncListWindowRemoved (object sender, EventArgs e)
    {
        var syncList = sender as TimeSyncList;
        lock (_timeSyncListLock)
        {
            if (syncList.Count == 0 || (syncList.Count == 1 && syncList.Contains(this)))
            {
                if (syncList == TimeSyncList)
                {
                    TimeSyncList = null;
                    OnSyncModeChanged();
                }
            }
        }
    }

    private void OnFreeThisWindowFromTimeSyncToolStripMenuItemClick (object sender, EventArgs e)
    {
        FreeFromTimeSync();
    }

    [SupportedOSPlatform("windows")]
    private void OnSplitContainerSplitterMoved (object sender, SplitterEventArgs e)
    {
        advancedFilterSplitContainer.SplitterDistance = FILTER_ADVANCED_SPLITTER_DISTANCE;
    }

    [SupportedOSPlatform("windows")]
    private void OnMarkFilterHitsInLogViewToolStripMenuItemClick (object sender, EventArgs e)
    {
        SearchParams p = new()
        {
            SearchText = _filterParams.SearchText,
            IsRegex = _filterParams.IsRegex,
            IsCaseSensitive = _filterParams.IsCaseSensitive
        };

        AddSearchHitHighlightEntry(p);
    }

    [SupportedOSPlatform("windows")]
    private void OnColumnComboBoxSelectionChangeCommitted (object sender, EventArgs e)
    {
        SelectColumn();
    }

    [SupportedOSPlatform("windows")]
    private void OnColumnComboBoxKeyDown (object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            SelectColumn();
            dataGridView.Focus();
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnColumnComboBoxPreviewKeyDown (object sender, PreviewKeyDownEventArgs e)
    {
        if (e.KeyCode == Keys.Down && e.Modifiers == Keys.Alt)
        {
            columnComboBox.DroppedDown = true;
        }

        if (e.KeyCode == Keys.Enter)
        {
            e.IsInputKey = true;
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnBookmarkProviderBookmarkRemoved (object sender, EventArgs e)
    {
        if (!_isLoading)
        {
            dataGridView.Refresh();
            filterGridView.Refresh();
        }
    }

    [SupportedOSPlatform("windows")]
    private void OnBookmarkProviderBookmarkAdded (object sender, EventArgs e)
    {
        if (!_isLoading)
        {
            dataGridView.Refresh();
            filterGridView.Refresh();
        }
    }

    private void OnBookmarkProviderAllBookmarksRemoved (object sender, EventArgs e)
    {
        // nothing
    }

    private void OnLogWindowLeave (object sender, EventArgs e)
    {
        InvalidateCurrentRow();
    }

    private void OnLogWindowEnter (object sender, EventArgs e)
    {
        InvalidateCurrentRow();
    }

    [SupportedOSPlatform("windows")]
    private void OnDataGridViewRowUnshared (object sender, DataGridViewRowEventArgs e)
    {
        if (_logger.IsTraceEnabled)
        {
            _logger.Trace($"Row unshared line {e.Row.Cells[1].Value}");
        }
    }

    #endregion

    #endregion

    #endregion

    [SupportedOSPlatform("windows")]
    private void MeasureItem (object sender, MeasureItemEventArgs e)
    {
        e.ItemHeight = filterListBox.Font.Height;
    }
}
