using System.Drawing;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace LogExpert.UI.Dialogs;

[SupportedOSPlatform("windows")]
public partial class MultiLoadRequestDialog : Form
{
    #region cTor

    public MultiLoadRequestDialog()
    {
        InitializeComponent();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
    }

    #endregion
}