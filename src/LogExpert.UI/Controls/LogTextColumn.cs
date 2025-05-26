using System.Windows.Forms;

namespace LogExpert.UI.Controls
{
    public class LogTextColumn : DataGridViewColumn
    {
        #region cTor

        public LogTextColumn() : base(new LogGridCell())
        {
        }

        #endregion
    }
}
