﻿using System.Drawing;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace LogExpert.UI.Dialogs;

[SupportedOSPlatform("windows")]
public partial class SearchProgressDialog : Form
{
    #region Fields

    #endregion

    #region cTor

    public SearchProgressDialog()
    {
        InitializeComponent();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        ShouldStop = false;
    }

    #endregion

    #region Properties

    public bool ShouldStop { get; private set; }

    #endregion

    #region Events handler

    private void OnButtonCancelClick(object sender, EventArgs e)
    {
        ShouldStop = true;
    }

    #endregion
}