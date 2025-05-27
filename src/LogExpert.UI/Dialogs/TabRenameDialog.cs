﻿using System.Drawing;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace LogExpert.UI.Dialogs;

[SupportedOSPlatform("windows")]
public partial class TabRenameDialog : Form
{
    #region cTor

    public TabRenameDialog()
    {
        InitializeComponent();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
    }

    #endregion

    #region Properties

    public string TabName
    {
        get => textBoxTabName.Text;
        set => textBoxTabName.Text = value;
    }

    #endregion

    #region Events handler

    private void OnTabRenameDlgKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    #endregion
}