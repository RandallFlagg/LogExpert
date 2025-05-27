using LogExpert.Core.Enums;
using System.Drawing;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace LogExpert.Dialogs;

[SupportedOSPlatform("windows")]
public partial class ProjectLoadDlg : Form
{
    #region Fields

    #endregion

    #region cTor

    public ProjectLoadDlg()
    {
        InitializeComponent();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
    }

    #endregion

    #region Properties

    public ProjectLoadDlgResult ProjectLoadResult { get; set; } = ProjectLoadDlgResult.Cancel;

    #endregion

    #region Events handler

    private void OnButtonCloseTabsClick(object sender, EventArgs e)
    {
        ProjectLoadResult = ProjectLoadDlgResult.CloseTabs;
        Close();
    }

    private void OnButtonNewWindowClick(object sender, EventArgs e)
    {
        ProjectLoadResult = ProjectLoadDlgResult.NewWindow;
        Close();
    }

    private void OnButtonIgnoreClick(object sender, EventArgs e)
    {
        ProjectLoadResult = ProjectLoadDlgResult.IgnoreLayout;
        Close();
    }

    #endregion
}