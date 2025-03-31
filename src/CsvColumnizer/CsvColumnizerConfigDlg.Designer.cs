namespace CsvColumnizer
{
  partial class CsvColumnizerConfigDlg
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            okButton = new System.Windows.Forms.Button();
            cancelButton = new System.Windows.Forms.Button();
            delimiterTextBox = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            textBoxQuoteChar = new System.Windows.Forms.TextBox();
            labelQuoteChar = new System.Windows.Forms.Label();
            textboxEscapeChar = new System.Windows.Forms.TextBox();
            labelEscapeChar = new System.Windows.Forms.Label();
            checkBoxEscape = new System.Windows.Forms.CheckBox();
            textBoxCommentChar = new System.Windows.Forms.TextBox();
            labelCommentChar = new System.Windows.Forms.Label();
            checkBoxFieldNames = new System.Windows.Forms.CheckBox();
            numericUpDownMinColumns = new System.Windows.Forms.NumericUpDown();
            labelMinColumns = new System.Windows.Forms.Label();
            labelMinColumnsNoCheck = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)numericUpDownMinColumns).BeginInit();
            SuspendLayout();
            // 
            // okButton
            // 
            okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Location = new System.Drawing.Point(15, 217);
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 25);
            okButton.TabIndex = 0;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += OnOkButtonClick;
            // 
            // cancelButton
            // 
            cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Location = new System.Drawing.Point(96, 217);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 25);
            cancelButton.TabIndex = 1;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            // 
            // delimiterTextBox
            // 
            delimiterTextBox.Location = new System.Drawing.Point(126, 12);
            delimiterTextBox.MaxLength = 1;
            delimiterTextBox.Name = "delimiterTextBox";
            delimiterTextBox.Size = new System.Drawing.Size(28, 23);
            delimiterTextBox.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(15, 15);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(84, 15);
            label1.TabIndex = 3;
            label1.Text = "Delimiter char:";
            // 
            // textBoxQuoteChar
            // 
            textBoxQuoteChar.Location = new System.Drawing.Point(126, 41);
            textBoxQuoteChar.Name = "textBoxQuoteChar";
            textBoxQuoteChar.Size = new System.Drawing.Size(28, 23);
            textBoxQuoteChar.TabIndex = 4;
            // 
            // labelQuoteChar
            // 
            labelQuoteChar.AutoSize = true;
            labelQuoteChar.Location = new System.Drawing.Point(15, 44);
            labelQuoteChar.Name = "labelQuoteChar";
            labelQuoteChar.Size = new System.Drawing.Size(69, 15);
            labelQuoteChar.TabIndex = 5;
            labelQuoteChar.Text = "Quote char:";
            // 
            // textboxEscapeChar
            // 
            textboxEscapeChar.Location = new System.Drawing.Point(126, 70);
            textboxEscapeChar.Name = "textboxEscapeChar";
            textboxEscapeChar.Size = new System.Drawing.Size(28, 23);
            textboxEscapeChar.TabIndex = 6;
            // 
            // labelEscapeChar
            // 
            labelEscapeChar.AutoSize = true;
            labelEscapeChar.Location = new System.Drawing.Point(15, 73);
            labelEscapeChar.Name = "labelEscapeChar";
            labelEscapeChar.Size = new System.Drawing.Size(72, 15);
            labelEscapeChar.TabIndex = 7;
            labelEscapeChar.Text = "Escape char:";
            // 
            // checkBoxEscape
            // 
            checkBoxEscape.AutoSize = true;
            checkBoxEscape.Location = new System.Drawing.Point(15, 99);
            checkBoxEscape.Name = "checkBoxEscape";
            checkBoxEscape.Size = new System.Drawing.Size(114, 19);
            checkBoxEscape.TabIndex = 8;
            checkBoxEscape.Text = "use escape chars";
            checkBoxEscape.UseVisualStyleBackColor = true;
            checkBoxEscape.CheckedChanged += OnEscapeCheckBoxCheckedChanged;
            // 
            // textBoxCommentChar
            // 
            textBoxCommentChar.Location = new System.Drawing.Point(126, 119);
            textBoxCommentChar.Name = "textBoxCommentChar";
            textBoxCommentChar.Size = new System.Drawing.Size(28, 23);
            textBoxCommentChar.TabIndex = 9;
            // 
            // labelCommentChar
            // 
            labelCommentChar.AutoSize = true;
            labelCommentChar.Location = new System.Drawing.Point(12, 124);
            labelCommentChar.Name = "labelCommentChar";
            labelCommentChar.Size = new System.Drawing.Size(90, 15);
            labelCommentChar.TabIndex = 10;
            labelCommentChar.Text = "Comment char:";
            // 
            // checkBoxFieldNames
            // 
            checkBoxFieldNames.AutoSize = true;
            checkBoxFieldNames.Location = new System.Drawing.Point(15, 192);
            checkBoxFieldNames.Name = "checkBoxFieldNames";
            checkBoxFieldNames.Size = new System.Drawing.Size(182, 19);
            checkBoxFieldNames.TabIndex = 11;
            checkBoxFieldNames.Text = "First line contains field names";
            checkBoxFieldNames.UseVisualStyleBackColor = true;
            // 
            // numericUpDownMinColumns
            // 
            numericUpDownMinColumns.Location = new System.Drawing.Point(106, 148);
            numericUpDownMinColumns.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numericUpDownMinColumns.Name = "numericUpDownMinColumns";
            numericUpDownMinColumns.Size = new System.Drawing.Size(48, 23);
            numericUpDownMinColumns.TabIndex = 12;
            // 
            // labelMinColumns
            // 
            labelMinColumns.AutoSize = true;
            labelMinColumns.Location = new System.Drawing.Point(15, 150);
            labelMinColumns.Name = "labelMinColumns";
            labelMinColumns.Size = new System.Drawing.Size(77, 15);
            labelMinColumns.TabIndex = 13;
            labelMinColumns.Text = "Min columns";
            // 
            // labelMinColumnsNoCheck
            // 
            labelMinColumnsNoCheck.AutoSize = true;
            labelMinColumnsNoCheck.Location = new System.Drawing.Point(15, 174);
            labelMinColumnsNoCheck.Name = "labelMinColumnsNoCheck";
            labelMinColumnsNoCheck.Size = new System.Drawing.Size(139, 15);
            labelMinColumnsNoCheck.TabIndex = 14;
            labelMinColumnsNoCheck.Text = "(0 = no minimum check)";
            // 
            // CsvColumnizerConfigDlg
            // 
            ClientSize = new System.Drawing.Size(204, 250);
            ControlBox = false;
            Controls.Add(labelMinColumnsNoCheck);
            Controls.Add(labelMinColumns);
            Controls.Add(numericUpDownMinColumns);
            Controls.Add(checkBoxFieldNames);
            Controls.Add(labelCommentChar);
            Controls.Add(textBoxCommentChar);
            Controls.Add(checkBoxEscape);
            Controls.Add(labelEscapeChar);
            Controls.Add(textboxEscapeChar);
            Controls.Add(labelQuoteChar);
            Controls.Add(textBoxQuoteChar);
            Controls.Add(label1);
            Controls.Add(delimiterTextBox);
            Controls.Add(cancelButton);
            Controls.Add(okButton);
            MaximizeBox = false;
            Name = "CsvColumnizerConfigDlg";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "CSV Columnizer Configuration";
            ((System.ComponentModel.ISupportInitialize)numericUpDownMinColumns).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.TextBox delimiterTextBox;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox textBoxQuoteChar;
    private System.Windows.Forms.Label labelQuoteChar;
    private System.Windows.Forms.TextBox textboxEscapeChar;
    private System.Windows.Forms.Label labelEscapeChar;
    private System.Windows.Forms.CheckBox checkBoxEscape;
    private System.Windows.Forms.TextBox textBoxCommentChar;
    private System.Windows.Forms.Label labelCommentChar;
    private System.Windows.Forms.CheckBox checkBoxFieldNames;
    private System.Windows.Forms.NumericUpDown numericUpDownMinColumns;
    private System.Windows.Forms.Label labelMinColumns;
    private System.Windows.Forms.Label labelMinColumnsNoCheck;
  }
}