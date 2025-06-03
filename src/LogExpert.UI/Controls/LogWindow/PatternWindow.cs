using System.Runtime.Versioning;

using LogExpert.Core.Classes;
using LogExpert.Core.EventArguments;
using LogExpert.Dialogs;

namespace LogExpert.UI.Controls.LogWindow;

[SupportedOSPlatform("windows")]
internal partial class PatternWindow : Form //TODO: Can this be changed to UserControl?
{
    #region Fields

    private readonly List<List<PatternBlock>> _blockList = [];
    private PatternBlock _currentBlock;
    private List<PatternBlock> _currentList;

    private readonly LogWindow _logWindow;
    private PatternArgs _patternArgs = new();

    #endregion

    #region cTor

    public PatternWindow ()
    {
        InitializeComponent();
    }

    public PatternWindow (LogWindow logWindow)
    {
        this._logWindow = logWindow;
        InitializeComponent();
        recalcButton.Enabled = false;
    }

    #endregion

    #region Properties

    public int Fuzzy
    {
        set => fuzzyKnobControl.Value = value;
        get => fuzzyKnobControl.Value;
    }

    public int MaxDiff
    {
        set => maxDiffKnobControl.Value = value;
        get => maxDiffKnobControl.Value;
    }

    public int MaxMisses
    {
        set => maxMissesKnobControl.Value = value;
        get => maxMissesKnobControl.Value;
    }

    public int Weight
    {
        set => weigthKnobControl.Value = value;
        get => weigthKnobControl.Value;
    }

    #endregion

    #region Public methods

    public void SetBlockList (List<PatternBlock> flatBlockList, PatternArgs patternArgs)
    {
        this._patternArgs = patternArgs;
        _blockList.Clear();
        List<PatternBlock> singeList = [];
        //int blockId = -1;
        for (var i = 0; i < flatBlockList.Count; ++i)
        {
            PatternBlock block = flatBlockList[i];
            singeList.Add(block);
            //if (block.blockId != blockId)
            //{
            //  singeList = new List<PatternBlock>();
            //  PatternBlock selfRefBlock = new PatternBlock();
            //  selfRefBlock.targetStart = block.startLine;
            //  selfRefBlock.targetEnd = block.endLine;
            //  selfRefBlock.blockId = block.blockId;
            //  singeList.Add(selfRefBlock);
            //  singeList.Add(block);
            //  this.blockList.Add(singeList);
            //  blockId = block.blockId;
            //}
            //else
            //{
            //  singeList.Add(block);
            //}
        }
        _blockList.Add(singeList);
        Invoke(new MethodInvoker(SetBlockListGuiStuff));
    }


    public void SetColumnizer (ILogLineColumnizer columnizer)
    {
        _logWindow.SetColumnizer(columnizer, patternHitsDataGridView);
        _logWindow.SetColumnizer(columnizer, contentDataGridView);
        patternHitsDataGridView.Columns[0].Width = 20;
        contentDataGridView.Columns[0].Width = 20;

        DataGridViewTextBoxColumn blockInfoColumn = new();
        blockInfoColumn.HeaderText = "Weight";
        blockInfoColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
        blockInfoColumn.Resizable = DataGridViewTriState.False;
        blockInfoColumn.DividerWidth = 1;
        blockInfoColumn.ReadOnly = true;
        blockInfoColumn.Width = 50;

        DataGridViewTextBoxColumn contentInfoColumn = new();
        contentInfoColumn.HeaderText = "Diff";
        contentInfoColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
        contentInfoColumn.Resizable = DataGridViewTriState.False;
        contentInfoColumn.DividerWidth = 1;
        contentInfoColumn.ReadOnly = true;
        contentInfoColumn.Width = 50;

        patternHitsDataGridView.Columns.Insert(1, blockInfoColumn);
        contentDataGridView.Columns.Insert(1, contentInfoColumn);
    }

    public void SetFont (string fontName, float fontSize)
    {
        Font font = new(new FontFamily(fontName), fontSize);
        var lineSpacing = font.FontFamily.GetLineSpacing(FontStyle.Regular);
        var lineSpacingPixel = font.Size * lineSpacing / font.FontFamily.GetEmHeight(FontStyle.Regular);

        patternHitsDataGridView.DefaultCellStyle.Font = font;
        contentDataGridView.DefaultCellStyle.Font = font;
        //this.lineHeight = font.Height + 4;
        patternHitsDataGridView.RowTemplate.Height = font.Height + 4;
        contentDataGridView.RowTemplate.Height = font.Height + 4;
    }

    #endregion

    #region Private Methods

    private void SetBlockListGuiStuff ()
    {
        patternHitsDataGridView.RowCount = 0;
        blockCountLabel.Text = "0";
        contentDataGridView.RowCount = 0;
        blockLinesLabel.Text = "0";
        recalcButton.Enabled = true;
        setRangeButton.Enabled = true;
        if (_blockList.Count > 0)
        {
            SetCurrentList(_blockList[0]);
        }
    }

    private void SetCurrentList (List<PatternBlock> patternList)
    {
        patternHitsDataGridView.RowCount = 0;
        _currentList = patternList;
        patternHitsDataGridView.RowCount = _currentList.Count;
        patternHitsDataGridView.Refresh();
        blockCountLabel.Text = "" + _currentList.Count;
    }

    private int GetLineForHitGrid (int rowIndex)
    {
        int line;
        line = _currentList[rowIndex].TargetStart;
        return line;
    }

    private int GetLineForContentGrid (int rowIndex)
    {
        int line;
        line = _currentBlock.TargetStart + rowIndex;
        return line;
    }

    #endregion

    #region Events handler

    private void patternHitsDataGridView_CellValueNeeded (object sender, DataGridViewCellValueEventArgs e)
    {
        if (_currentList == null || e.RowIndex < 0)
        {
            return;
        }
        var rowIndex = GetLineForHitGrid(e.RowIndex);
        var colIndex = e.ColumnIndex;
        if (colIndex == 1)
        {
            e.Value = _currentList[e.RowIndex].Weigth;
        }
        else
        {
            if (colIndex > 1)
            {
                colIndex--; // correct the additional inserted col
            }

            e.Value = _logWindow.GetCellValue(rowIndex, colIndex);
        }
    }

    private void patternHitsDataGridView_CellPainting (object sender, DataGridViewCellPaintingEventArgs e)
    {
        if (_currentList == null || e.RowIndex < 0)
        {
            return;
        }

        if (e.ColumnIndex == 1)
        {
            e.PaintBackground(e.CellBounds, false);
            var selCount = _patternArgs.EndLine - _patternArgs.StartLine;
            var maxWeight = _patternArgs.MaxDiffInBlock * selCount + selCount;
            if (maxWeight > 0)
            {
                var width = (int)((int)e.Value / (double)maxWeight * e.CellBounds.Width);
                Rectangle rect = new(e.CellBounds.X, e.CellBounds.Y, width, e.CellBounds.Height);
                var alpha = 90 + (int)((int)e.Value / (double)maxWeight * 165);
                var color = Color.FromArgb(alpha, 170, 180, 150);
                Brush brush = new SolidBrush(color);
                rect.Inflate(-2, -1);
                e.Graphics.FillRectangle(brush, rect);
                brush.Dispose();
            }
            e.PaintContent(e.CellBounds);
            e.Handled = true;
        }
        else
        {
            var gridView = (BufferedDataGridView)sender;
            var rowIndex = GetLineForHitGrid(e.RowIndex);
            _logWindow.CellPainting(gridView, rowIndex, e);
        }
    }

    private void patternHitsDataGridView_MouseDoubleClick (object sender, MouseEventArgs e)
    {
        //if (this.currentList == null || patternHitsDataGridView.CurrentRow == null)
        //  return;
        //int rowIndex = GetLineForHitGrid(patternHitsDataGridView.CurrentRow.Index);

        //this.logWindow.SelectLogLine(rowIndex);
    }

    private void patternHitsDataGridView_CurrentCellChanged (object sender, EventArgs e)
    {
        if (_currentList == null || patternHitsDataGridView.CurrentRow == null)
        {
            return;
        }

        if (patternHitsDataGridView.CurrentRow.Index > _currentList.Count - 1)
        {
            return;
        }

        contentDataGridView.RowCount = 0;
        _currentBlock = _currentList[patternHitsDataGridView.CurrentRow.Index];
        contentDataGridView.RowCount = _currentBlock.TargetEnd - _currentBlock.TargetStart + 1;
        contentDataGridView.Refresh();
        contentDataGridView.CurrentCell = contentDataGridView.Rows[0].Cells[0];
        blockLinesLabel.Text = "" + contentDataGridView.RowCount;
    }

    private void contentDataGridView_CellValueNeeded (object sender, DataGridViewCellValueEventArgs e)
    {
        if (_currentBlock == null || e.RowIndex < 0)
        {
            return;
        }
        var rowIndex = GetLineForContentGrid(e.RowIndex);
        var colIndex = e.ColumnIndex;
        if (colIndex == 1)
        {
            QualityInfo qi;
            if (_currentBlock.QualityInfoList.TryGetValue(rowIndex, out qi))
            {
                e.Value = qi.Quality;
            }
            else
            {
                e.Value = "";
            }
        }
        else
        {
            if (colIndex != 0)
            {
                colIndex--; // adjust the inserted column
            }
            e.Value = _logWindow.GetCellValue(rowIndex, colIndex);
        }
    }

    private void contentDataGridView_CellPainting (object sender, DataGridViewCellPaintingEventArgs e)
    {
        if (_currentBlock == null || e.RowIndex < 0)
        {
            return;
        }
        var gridView = (BufferedDataGridView)sender;
        var rowIndex = GetLineForContentGrid(e.RowIndex);
        _logWindow.CellPainting(gridView, rowIndex, e);
    }

    private void contentDataGridView_CellMouseDoubleClick (object sender, DataGridViewCellMouseEventArgs e)
    {
        if (_currentBlock == null || contentDataGridView.CurrentRow == null)
        {
            return;
        }
        var rowIndex = GetLineForContentGrid(contentDataGridView.CurrentRow.Index);

        _logWindow.SelectLogLine(rowIndex);
    }

    private void recalcButton_Click (object sender, EventArgs e)
    {
        _patternArgs.Fuzzy = fuzzyKnobControl.Value;
        _patternArgs.MaxDiffInBlock = maxDiffKnobControl.Value;
        _patternArgs.MaxMisses = maxMissesKnobControl.Value;
        _patternArgs.MinWeight = weigthKnobControl.Value;
        _logWindow.PatternStatistic(_patternArgs);
        recalcButton.Enabled = false;
        setRangeButton.Enabled = false;
    }

    private void closeButton_Click (object sender, EventArgs e)
    {
        Close();
    }

    private void contentDataGridView_ColumnDividerDoubleClick (object sender,
        DataGridViewColumnDividerDoubleClickEventArgs e)
    {
        e.Handled = true;
        contentDataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
    }

    private void patternHitsDataGridView_ColumnDividerDoubleClick (object sender,
        DataGridViewColumnDividerDoubleClickEventArgs e)
    {
        e.Handled = true;
        patternHitsDataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
    }

    private void setRangeButton_Click (object sender, EventArgs e)
    {
        _logWindow.PatternStatisticSelectRange(_patternArgs);
        recalcButton.Enabled = true;
        rangeLabel.Text = "Start: " + _patternArgs.StartLine + "\r\nEnd: " + _patternArgs.EndLine;
    }

    #endregion
}