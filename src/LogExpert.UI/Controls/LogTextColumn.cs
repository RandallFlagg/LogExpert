using System.Runtime.Versioning;

namespace LogExpert.UI.Controls;

public class LogTextColumn : DataGridViewColumn
{
    #region cTor

    [SupportedOSPlatform("windows")]
    public LogTextColumn () : base(new LogGridCell())
    {
    }

    #endregion
}
