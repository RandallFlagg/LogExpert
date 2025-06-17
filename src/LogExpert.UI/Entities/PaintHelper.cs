using System.Runtime.Versioning;

using LogExpert.Core.Classes.Highlight;
using LogExpert.Core.Config;
using LogExpert.Core.Entities;
using LogExpert.Dialogs;
using LogExpert.UI.Controls;
using LogExpert.UI.Interface;

using NLog;

namespace LogExpert.UI.Entities;

//TOOD: This whole class should be refactored and rethought
//TODO: This class should not knoow ConfigManager?
internal static class PaintHelper
{
    #region Fields

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    //TODO Make configurable
    private static Color _bookmarkColor = Color.FromArgb(165, 200, 225);

    #endregion

    #region Public methods

    [SupportedOSPlatform("windows")]
    public static void CellPainting (ILogPaintContextUI logPaintCtx, BufferedDataGridView gridView, int rowIndex, DataGridViewCellPaintingEventArgs e)
    {
        if (rowIndex < 0 || e.ColumnIndex < 0)
        {
            e.Handled = false;
            return;
        }

        ILogLine line = logPaintCtx.GetLogLine(rowIndex);

        if (line != null)
        {
            HighlightEntry entry = logPaintCtx.FindHighlightEntry(line, true);
            e.Graphics.SetClip(e.CellBounds);

            if ((e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected)
            {
                Color backColor = e.CellStyle.SelectionBackColor;
                Brush brush;

                if (gridView.Focused)
                {
                    brush = new SolidBrush(backColor);
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
                Color bgColor = ColorMode.DockBackgroundColor;
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
                PaintCell(logPaintCtx, e, gridView, false, entry);
            }

            if (e.ColumnIndex == 0)
            {
                Bookmark bookmark = logPaintCtx.GetBookmarkForLine(rowIndex);
                if (bookmark != null)
                {
                    Rectangle r; // = new Rectangle(e.CellBounds.Left + 2, e.CellBounds.Top + 2, 6, 6);
                    r = e.CellBounds;
                    r.Inflate(-2, -2);
                    Brush brush = new SolidBrush(logPaintCtx.BookmarkColor);
                    e.Graphics.FillRectangle(brush, r);
                    brush.Dispose();
                    if (bookmark.Text.Length > 0)
                    {
                        StringFormat format = new()
                        {
                            LineAlignment = StringAlignment.Center,
                            Alignment = StringAlignment.Center
                        };

                        Brush brush2 = new SolidBrush(Color.FromArgb(255, 190, 100, 0));
                        Font font = logPaintCtx.MonospacedFont;
                        e.Graphics.DrawString("i", font, brush2, new RectangleF(r.Left, r.Top, r.Width, r.Height), format);
                        brush2.Dispose();
                    }
                }
            }

            e.Paint(e.CellBounds, DataGridViewPaintParts.Border);
            e.Handled = true;
        }
    }

    [SupportedOSPlatform("windows")]
    public static DataGridViewTextBoxColumn CreateMarkerColumn ()
    {
        DataGridViewTextBoxColumn markerColumn = new()
        {
            HeaderText = "",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet,
            Resizable = DataGridViewTriState.False,
            DividerWidth = 1,
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };

        return markerColumn;
    }

    [SupportedOSPlatform("windows")]
    public static DataGridViewTextBoxColumn CreateLineNumberColumn ()
    {
        DataGridViewTextBoxColumn lineNumberColumn = new()
        {
            HeaderText = "Line",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet,
            Resizable = DataGridViewTriState.NotSet,
            DividerWidth = 1,
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };

        return lineNumberColumn;
    }

    [SupportedOSPlatform("windows")]
    public static DataGridViewColumn CreateTitleColumn (string colName)
    {
        DataGridViewColumn titleColumn = new LogTextColumn
        {
            HeaderText = colName,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet,
            Resizable = DataGridViewTriState.NotSet,
            DividerWidth = 1,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };

        return titleColumn;
    }

    [SupportedOSPlatform("windows")]
    public static void SetColumnizer (ILogLineColumnizer columnizer, BufferedDataGridView gridView)
    {
        var rowCount = gridView.RowCount;
        var currLine = gridView.CurrentCellAddress.Y;
        var currFirstLine = gridView.FirstDisplayedScrollingRowIndex;

        try
        {
            gridView.Columns.Clear();
        }
        catch (ArgumentOutOfRangeException ae)
        {
            // Occures sometimes on empty gridViews (no lines) if bookmark window was closed and re-opened in floating mode.
            // Don't know why.
            _logger.Error(ae);
        }

        _ = gridView.Columns.Add(CreateMarkerColumn());

        _ = gridView.Columns.Add(CreateLineNumberColumn());

        foreach (var colName in columnizer.GetColumnNames())
        {
            _ = gridView.Columns.Add(CreateTitleColumn(colName));
        }

        gridView.RowCount = rowCount;

        if (currLine != -1)
        {
            gridView.CurrentCell = gridView.Rows[currLine].Cells[0];
        }

        if (currFirstLine != -1)
        {
            gridView.FirstDisplayedScrollingRowIndex = currFirstLine;
        }

        //gridView.Refresh();
        //AutoResizeColumns(gridView);
    }

    [SupportedOSPlatform("windows")]
    private static void AutoResizeColumns (BufferedDataGridView gridView, bool setLastColumnWidth, int lastColumnWidth)
    {
        try
        {
            gridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
            if (gridView.Columns.Count > 1 && setLastColumnWidth && gridView.Columns[gridView.Columns.Count - 1].Width < lastColumnWidth
            )
            {
                // It seems that using 'MinimumWidth' instead of 'Width' prevents the DataGridView's NullReferenceExceptions
                //gridView.Columns[gridView.Columns.Count - 1].Width = this.Preferences.lastColumnWidth;
                gridView.Columns[gridView.Columns.Count - 1].MinimumWidth = lastColumnWidth;
            }
        }
        catch (NullReferenceException e)
        {
            // See https://connect.microsoft.com/VisualStudio/feedback/details/366943/autoresizecolumns-in-datagridview-throws-nullreferenceexception
            // possible solution => https://stackoverflow.com/questions/36287553/nullreferenceexception-when-trying-to-set-datagridview-column-width-brings-th
            // There are some rare situations with null ref exceptions when resizing columns and on filter finished
            // So catch them here. Better than crashing.
            _logger.Error(e, "Error while resizing columns: ");
        }
    }

    [SupportedOSPlatform("windows")]
    public static void ApplyDataGridViewPrefs (BufferedDataGridView dataGridView, bool setLastColumnWidht, int lastColumnWidth)
    {
        if (dataGridView.Columns.Count > 1)
        {
            if (setLastColumnWidht)
            {
                dataGridView.Columns[dataGridView.Columns.Count - 1].MinimumWidth = lastColumnWidth;
            }
            else
            {
                // Workaround for a .NET bug which brings the DataGridView into an unstable state (causing lots of NullReferenceExceptions).
                dataGridView.FirstDisplayedScrollingColumnIndex = 0;

                dataGridView.Columns[dataGridView.Columns.Count - 1].MinimumWidth = 5; // default
            }
        }

        if (dataGridView.RowCount > 0)
        {
            dataGridView.UpdateRowHeightInfo(0, true);
        }

        dataGridView.Invalidate();
        dataGridView.Refresh();
        AutoResizeColumns(dataGridView, setLastColumnWidht, lastColumnWidth);
    }

    [SupportedOSPlatform("windows")]
    public static Rectangle BorderWidths (DataGridViewAdvancedBorderStyle advancedBorderStyle)
    {
        Rectangle rect = new()
        {
            X = advancedBorderStyle.Left == DataGridViewAdvancedCellBorderStyle.None ? 0 : 1
        };
        if (advancedBorderStyle.Left is DataGridViewAdvancedCellBorderStyle.OutsetDouble or DataGridViewAdvancedCellBorderStyle.InsetDouble)
        {
            rect.X++;
        }

        rect.Y = advancedBorderStyle.Top == DataGridViewAdvancedCellBorderStyle.None ? 0 : 1;
        if (advancedBorderStyle.Top is DataGridViewAdvancedCellBorderStyle.OutsetDouble or DataGridViewAdvancedCellBorderStyle.InsetDouble)
        {
            rect.Y++;
        }

        rect.Width = advancedBorderStyle.Right == DataGridViewAdvancedCellBorderStyle.None ? 0 : 1;
        if (advancedBorderStyle.Right is DataGridViewAdvancedCellBorderStyle.OutsetDouble or DataGridViewAdvancedCellBorderStyle.InsetDouble)
        {
            rect.Width++;
        }

        rect.Height = advancedBorderStyle.Bottom == DataGridViewAdvancedCellBorderStyle.None ? 0 : 1;
        if (advancedBorderStyle.Bottom is DataGridViewAdvancedCellBorderStyle.OutsetDouble or
            DataGridViewAdvancedCellBorderStyle.InsetDouble)
        {
            rect.Height++;
        }

        //rect.Width += this.owningColumn.DividerWidth;
        //rect.Height += this.owningRow.DividerHeight;

        return rect;
    }

    #endregion

    #region Private Methods

    [SupportedOSPlatform("windows")]
    private static void PaintCell (ILogPaintContextUI logPaintCtx, DataGridViewCellPaintingEventArgs e, BufferedDataGridView gridView, bool noBackgroundFill, HighlightEntry groundEntry)
    {
        PaintHighlightedCell(logPaintCtx, e, gridView, noBackgroundFill, groundEntry);
    }

    [SupportedOSPlatform("windows")]
    private static void PaintHighlightedCell (ILogPaintContextUI logPaintCtx, DataGridViewCellPaintingEventArgs e, BufferedDataGridView gridView, bool noBackgroundFill, HighlightEntry groundEntry)
    {
        var value = e.Value ?? string.Empty;

        IList<HighlightMatchEntry> matchList = logPaintCtx.FindHighlightMatches(value as ILogLine);
        // too many entries per line seem to cause problems with the GDI
        while (matchList.Count > 50)
        {
            matchList.RemoveAt(50);
        }

        if (value is Column column)
        {
            if (!string.IsNullOrEmpty(column.FullValue))
            {
                HighlightMatchEntry hme = new()
                {
                    StartPos = 0,
                    Length = column.FullValue.Length
                };

                var he = new HighlightEntry
                {
                    SearchText = column.FullValue,
                    ForegroundColor = groundEntry?.ForegroundColor ?? ColorMode.ForeColor,
                    BackgroundColor = groundEntry?.BackgroundColor ?? Color.Empty,
                    IsRegEx = false,
                    IsCaseSensitive = false,
                    IsLedSwitch = false,
                    IsStopTail = false,
                    IsSetBookmark = false,
                    IsActionEntry = false,
                    IsWordMatch = false
                };

                hme.HighlightEntry = he;

                matchList = MergeHighlightMatchEntries(matchList, hme);
            }
        }

        var leftPad = e.CellStyle.Padding.Left;
        RectangleF rect = new(e.CellBounds.Left + leftPad, e.CellBounds.Top, e.CellBounds.Width, e.CellBounds.Height);
        Rectangle borderWidths = BorderWidths(e.AdvancedBorderStyle);
        Rectangle valBounds = e.CellBounds;
        valBounds.Offset(borderWidths.X, borderWidths.Y);
        valBounds.Width -= borderWidths.Right;
        valBounds.Height -= borderWidths.Bottom;
        if (e.CellStyle.Padding != Padding.Empty)
        {
            valBounds.Offset(e.CellStyle.Padding.Left, e.CellStyle.Padding.Top);
            valBounds.Width -= e.CellStyle.Padding.Horizontal;
            valBounds.Height -= e.CellStyle.Padding.Vertical;
        }


        TextFormatFlags flags =
                TextFormatFlags.Left
                | TextFormatFlags.SingleLine
                | TextFormatFlags.NoPrefix
                | TextFormatFlags.PreserveGraphicsClipping
                | TextFormatFlags.NoPadding
                | TextFormatFlags.VerticalCenter
                | TextFormatFlags.TextBoxControl;

        //          | TextFormatFlags.VerticalCenter
        //          | TextFormatFlags.TextBoxControl
        //          TextFormatFlags.SingleLine
        //TextRenderer.DrawText(e.Graphics, e.Value as String, e.CellStyle.Font, valBounds, Color.FromKnownColor(KnownColor.Black), flags);

        Point wordPos = valBounds.Location;
        Size proposedSize = new(valBounds.Width, valBounds.Height);

        Rectangle r = gridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
        e.Graphics.SetClip(e.CellBounds);

        foreach (HighlightMatchEntry matchEntry in matchList)
        {
            Font font = matchEntry != null && matchEntry.HighlightEntry.IsBold
                ? logPaintCtx.BoldFont
                : logPaintCtx.NormalFont;

            Brush bgBrush = matchEntry.HighlightEntry.BackgroundColor != Color.Empty
                ? new SolidBrush(matchEntry.HighlightEntry.BackgroundColor)
                : null;

            var matchWord = string.Empty;
            if (value is Column again)
            {
                if (!string.IsNullOrEmpty(again.FullValue))
                {
                    matchWord = again.FullValue.Substring(matchEntry.StartPos, matchEntry.Length);
                }
            }

            Size wordSize = TextRenderer.MeasureText(e.Graphics, matchWord, font, proposedSize, flags);
            wordSize.Height = e.CellBounds.Height;
            Rectangle wordRect = new(wordPos, wordSize);

            Color foreColor = matchEntry.HighlightEntry.ForegroundColor;
            if ((e.State & DataGridViewElementStates.Selected) != DataGridViewElementStates.Selected)
            {
                if (!noBackgroundFill && bgBrush != null && !matchEntry.HighlightEntry.NoBackground)
                {
                    e.Graphics.FillRectangle(bgBrush, wordRect);
                }
            }
            else
            {
                if (foreColor.Equals(Color.Black))
                {
                    foreColor = Color.White;
                }
            }

            TextRenderer.DrawText(e.Graphics, matchWord, font, wordRect, foreColor, flags);

            wordPos.Offset(wordSize.Width, 0);
            bgBrush?.Dispose();
        }
    }


    /// <summary>
    /// Builds a list of HilightMatchEntry objects. A HilightMatchEntry spans over a region that is painted with the same foreground and
    /// background colors.
    /// All regions which don't match a word-mode entry will be painted with the colors of a default entry (groundEntry). This is either the
    /// first matching non-word-mode highlight entry or a black-on-white default (if no matching entry was found).
    /// </summary>
    /// <param name="matchList">List of all highlight matches for the current cell</param>
    /// <param name="groundEntry">The entry that is used as the default.</param>
    /// <returns>List of HilightMatchEntry objects. The list spans over the whole cell and contains color infos for every substring.</returns>
    private static IList<HighlightMatchEntry> MergeHighlightMatchEntries (IList<HighlightMatchEntry> matchList, HighlightMatchEntry groundEntry)
    {
        // Fill an area with lenth of whole text with a default hilight entry
        var entryArray = new HighlightEntry[groundEntry.Length];
        for (var i = 0; i < entryArray.Length; ++i)
        {
            entryArray[i] = groundEntry.HighlightEntry;
        }

        // "overpaint" with all matching word match enries
        // Non-word-mode matches will not overpaint because they use the groundEntry
        foreach (HighlightMatchEntry me in matchList)
        {
            var endPos = me.StartPos + me.Length;
            for (var i = me.StartPos; i < endPos; ++i)
            {
                if (me.HighlightEntry.IsWordMatch)
                {
                    entryArray[i] = me.HighlightEntry;
                }
                //else
                //{
                //    //entryArray[i].ForegroundColor = me.HilightEntry.ForegroundColor;
                //}
            }
        }

        // collect areas with same hilight entry and build new highlight match entries for it
        IList<HighlightMatchEntry> mergedList = [];
        if (entryArray.Length > 0)
        {
            HighlightEntry currentEntry = entryArray[0];
            var lastStartPos = 0;
            var pos = 0;
            for (; pos < entryArray.Length; ++pos)
            {
                if (entryArray[pos] != currentEntry)
                {
                    HighlightMatchEntry me = new()
                    {
                        StartPos = lastStartPos,
                        Length = pos - lastStartPos,
                        HighlightEntry = currentEntry
                    };
                    mergedList.Add(me);
                    currentEntry = entryArray[pos];
                    lastStartPos = pos;
                }
            }
            HighlightMatchEntry me2 = new()
            {
                StartPos = lastStartPos,
                Length = pos - lastStartPos,
                HighlightEntry = currentEntry
            };
            mergedList.Add(me2);
        }

        return mergedList;
    }

    #endregion
}