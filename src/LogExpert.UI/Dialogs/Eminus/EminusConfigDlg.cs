using System;
using System.Drawing;
using System.Windows.Forms;

namespace LogExpert;

internal partial class EminusConfigDlg : Form
{
    #region Fields

    #endregion

    #region cTor

    public EminusConfigDlg(EminusConfig config)
    {
        SuspendLayout();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        TopLevel = false;
        Config = config;

        hostTextBox.Text = config.host;
        portTextBox.Text = string.Empty + config.port;
        passwordTextBox.Text = config.password;

        ResumeLayout();
    }

    #endregion

    #region Properties

    public EminusConfig Config { get; set; }

    #endregion

    #region Public methods

    public void ApplyChanges()
    {
        Config.host = hostTextBox.Text;
        try
        {
            Config.port = short.Parse(portTextBox.Text);
        }
        catch (FormatException)
        {
            Config.port = 0;
        }
        Config.password = passwordTextBox.Text;
    }

    #endregion
}