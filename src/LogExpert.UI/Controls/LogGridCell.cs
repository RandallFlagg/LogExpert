using System;
using System.Windows.Forms;

namespace LogExpert.UI.Controls;

public class LogGridCell : DataGridViewTextBoxCell
{
    #region Properties

    public override Type EditType => typeof(LogCellEditingControl);

    #endregion
}
