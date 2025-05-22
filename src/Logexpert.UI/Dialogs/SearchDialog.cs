using LogExpert.Core.Entities;
using LogExpert.UI.Dialogs;

using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace LogExpert.Dialogs;

[SupportedOSPlatform("windows")]
public partial class SearchDialog : Form
{
    #region Fields

    private static readonly int MAX_HISTORY = 30;

    #endregion

    #region cTor

    public SearchDialog()
    {
        InitializeComponent();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        Load += OnSearchDialogLoad;
    }

    #endregion

    #region Properties

    public SearchParams SearchParams { get; set; } = new();

    #endregion

    #region Events handler

    private void OnSearchDialogLoad(object? sender, EventArgs e)
    {
        if (SearchParams != null)
        {
            if (SearchParams.isFromTop)
            {
                radioButtonFromTop.Checked = true;
            }
            else
            {
                radioButtonFromSelected.Checked = true;
            }

            if (SearchParams.isForward)
            {
                radioButtonForward.Checked = true;
            }
            else
            {
                radioButtonBackward.Checked = true;
            }

            checkBoxRegex.Checked = SearchParams.isRegex;
            checkBoxCaseSensitive.Checked = SearchParams.isCaseSensitive;
            foreach (string item in SearchParams.historyList)
            {
                comboBoxSearchFor.Items.Add(item);
            }

            if (comboBoxSearchFor.Items.Count > 0)
            {
                comboBoxSearchFor.SelectedIndex = 0;
            }
        }
        else
        {
            radioButtonFromSelected.Checked = true;
            radioButtonForward.Checked = true;
            SearchParams = new SearchParams();
        }
    }

    private void OnButtonRegexClick(object sender, EventArgs e)
    {
        RegexHelperDialog dlg = new()
        {
            Owner = this,
            CaseSensitive = checkBoxCaseSensitive.Checked,
            Pattern = comboBoxSearchFor.Text
        };

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            checkBoxCaseSensitive.Checked = dlg.CaseSensitive;
            comboBoxSearchFor.Text = dlg.Pattern;
        }
    }

    private void OnButtonOkClick(object sender, EventArgs e)
    {
        try
        {
            if (checkBoxRegex.Checked)
            {
                if (string.IsNullOrWhiteSpace(comboBoxSearchFor.Text))
                {
                    throw new ArgumentException("Search text is empty");
                }

                Regex.IsMatch("", comboBoxSearchFor.Text);
            }

            SearchParams.searchText = comboBoxSearchFor.Text;
            SearchParams.isCaseSensitive = checkBoxCaseSensitive.Checked;
            SearchParams.isForward = radioButtonForward.Checked;
            SearchParams.isFromTop = radioButtonFromTop.Checked;
            SearchParams.isRegex = checkBoxRegex.Checked;
            SearchParams.historyList.Remove(comboBoxSearchFor.Text);
            SearchParams.historyList.Insert(0, comboBoxSearchFor.Text);

            if (SearchParams.historyList.Count > MAX_HISTORY)
            {
                SearchParams.historyList.RemoveAt(SearchParams.historyList.Count - 1);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during creation of search parameter\r\n{ex.Message}");
        }
    }

    #endregion

    private void OnButtonCancelClick(object sender, EventArgs e)
    {
        Close();
    }
}