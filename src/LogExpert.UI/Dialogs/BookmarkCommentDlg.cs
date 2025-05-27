using System.Drawing;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace LogExpert.Dialogs;

[SupportedOSPlatform("windows")]
public partial class BookmarkCommentDlg : Form
{
    #region cTor

    public BookmarkCommentDlg()
    {
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();
    }

    #endregion

    #region Properties

    public string Comment
    {
        set => commentTextBox.Text = value;
        get => commentTextBox.Text;
    }

    #endregion
}