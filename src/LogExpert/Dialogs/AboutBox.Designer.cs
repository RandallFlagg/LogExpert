//namespace LogExpert.Dialogs
//{
//    partial class AboutBox
//	{
//		/// <summary>
//		/// Required designer variable.
//		/// </summary>
//		private System.ComponentModel.IContainer components = null;

//		/// <summary>
//		/// Clean up any resources being used.
//		/// </summary>
//		protected override void Dispose(bool disposing)
//		{
//			if (disposing && (components != null))
//			{
//				components.Dispose();
//			}
//			base.Dispose(disposing);
//		}

//        #region Windows Form Designer generated code

//        /// <summary>
//        /// Required method for Designer support - do not modify
//        /// the contents of this method with the code editor.
//        /// </summary>
//        private void InitializeComponent()
//        {
//            tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
//            logoPictureBox = new System.Windows.Forms.PictureBox();
//            labelProductName = new System.Windows.Forms.Label();
//            labelVersion = new System.Windows.Forms.Label();
//            labelCopyright = new System.Windows.Forms.Label();
//            textBoxDescription = new System.Windows.Forms.TextBox();
//            linkLabelURL = new System.Windows.Forms.LinkLabel();
//            panel1 = new System.Windows.Forms.Panel();
//            okButton = new System.Windows.Forms.Button();
//            tableLayoutPanel.SuspendLayout();
//            ((System.ComponentModel.ISupportInitialize)logoPictureBox).BeginInit();
//            panel1.SuspendLayout();
//            SuspendLayout();
//            // 
//            // tableLayoutPanel
//            // 
//            tableLayoutPanel.ColumnCount = 2;
//            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34.25484F));
//            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65.74516F));
//            tableLayoutPanel.Controls.Add(logoPictureBox, 0, 0);
//            tableLayoutPanel.Controls.Add(labelProductName, 1, 0);
//            tableLayoutPanel.Controls.Add(labelVersion, 1, 1);
//            tableLayoutPanel.Controls.Add(labelCopyright, 1, 2);
//            tableLayoutPanel.Controls.Add(textBoxDescription, 1, 4);
//            tableLayoutPanel.Controls.Add(linkLabelURL, 1, 3);
//            tableLayoutPanel.Controls.Add(panel1, 1, 5);
//            tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
//            tableLayoutPanel.Location = new System.Drawing.Point(14, 14);
//            tableLayoutPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
//            tableLayoutPanel.Name = "tableLayoutPanel";
//            tableLayoutPanel.RowCount = 6;
//            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9.912043F));
//            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9.912043F));
//            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9.47226F));
//            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.84032F));
//            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 46.68471F));
//            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.17862F));
//            tableLayoutPanel.Size = new System.Drawing.Size(914, 649);
//            tableLayoutPanel.TabIndex = 0;
//            // 
//            // logoPictureBox
//            // 
//            logoPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
//            logoPictureBox.Image = Properties.Resources.LogLover;
//            logoPictureBox.Location = new System.Drawing.Point(4, 5);
//            logoPictureBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
//            logoPictureBox.Name = "logoPictureBox";
//            tableLayoutPanel.SetRowSpan(logoPictureBox, 6);
//            logoPictureBox.Size = new System.Drawing.Size(305, 639);
//            logoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
//            logoPictureBox.TabIndex = 12;
//            logoPictureBox.TabStop = false;
//            // 
//            // labelProductName
//            // 
//            labelProductName.Dock = System.Windows.Forms.DockStyle.Fill;
//            labelProductName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
//            labelProductName.Location = new System.Drawing.Point(322, 0);
//            labelProductName.Margin = new System.Windows.Forms.Padding(9, 0, 4, 0);
//            labelProductName.MaximumSize = new System.Drawing.Size(0, 26);
//            labelProductName.Name = "labelProductName";
//            labelProductName.Size = new System.Drawing.Size(588, 26);
//            labelProductName.TabIndex = 19;
//            labelProductName.Text = "Product Name";
//            labelProductName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
//            // 
//            // labelVersion
//            // 
//            labelVersion.Dock = System.Windows.Forms.DockStyle.Fill;
//            labelVersion.Location = new System.Drawing.Point(322, 64);
//            labelVersion.Margin = new System.Windows.Forms.Padding(9, 0, 4, 0);
//            labelVersion.MaximumSize = new System.Drawing.Size(0, 26);
//            labelVersion.Name = "labelVersion";
//            labelVersion.Size = new System.Drawing.Size(588, 26);
//            labelVersion.TabIndex = 0;
//            labelVersion.Text = "Version";
//            labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
//            // 
//            // labelCopyright
//            // 
//            labelCopyright.Dock = System.Windows.Forms.DockStyle.Fill;
//            labelCopyright.Location = new System.Drawing.Point(322, 128);
//            labelCopyright.Margin = new System.Windows.Forms.Padding(9, 0, 4, 0);
//            labelCopyright.Name = "labelCopyright";
//            labelCopyright.Size = new System.Drawing.Size(588, 61);
//            labelCopyright.TabIndex = 21;
//            labelCopyright.Text = "Copyright";
//            labelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
//            // 
//            // textBoxDescription
//            // 
//            textBoxDescription.Dock = System.Windows.Forms.DockStyle.Fill;
//            textBoxDescription.Location = new System.Drawing.Point(322, 270);
//            textBoxDescription.Margin = new System.Windows.Forms.Padding(9, 5, 4, 5);
//            textBoxDescription.Multiline = true;
//            textBoxDescription.Name = "textBoxDescription";
//            textBoxDescription.ReadOnly = true;
//            textBoxDescription.ScrollBars = System.Windows.Forms.ScrollBars.Both;
//            textBoxDescription.Size = new System.Drawing.Size(588, 292);
//            textBoxDescription.TabIndex = 23;
//            textBoxDescription.TabStop = false;
//            textBoxDescription.Text = "Description";
//            // 
//            // linkLabelURL
//            // 
//            linkLabelURL.AutoSize = true;
//            linkLabelURL.Dock = System.Windows.Forms.DockStyle.Fill;
//            linkLabelURL.Location = new System.Drawing.Point(317, 189);
//            linkLabelURL.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
//            linkLabelURL.Name = "linkLabelURL";
//            linkLabelURL.Size = new System.Drawing.Size(593, 76);
//            linkLabelURL.TabIndex = 25;
//            linkLabelURL.TabStop = true;
//            linkLabelURL.Text = "https://github.com/LogExperts/LogExpert";
//            linkLabelURL.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
//            linkLabelURL.LinkClicked += OnLinkLabelURLClicked;
//            // 
//            // panel1
//            // 
//            panel1.Controls.Add(okButton);
//            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
//            panel1.Location = new System.Drawing.Point(317, 572);
//            panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
//            panel1.Name = "panel1";
//            panel1.Size = new System.Drawing.Size(593, 72);
//            panel1.TabIndex = 26;
//            // 
//            // okButton
//            // 
//            okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
//            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
//            okButton.Location = new System.Drawing.Point(475, 32);
//            okButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
//            okButton.Name = "okButton";
//            okButton.Size = new System.Drawing.Size(114, 35);
//            okButton.TabIndex = 0;
//            okButton.Text = "&OK";
//            okButton.UseVisualStyleBackColor = true;
//            // 
//            // AboutBox
//            // 
//            ClientSize = new System.Drawing.Size(942, 677);
//            Controls.Add(tableLayoutPanel);
//            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
//            Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
//            MaximizeBox = false;
//            MinimizeBox = false;
//            Name = "AboutBox";
//            Padding = new System.Windows.Forms.Padding(14);
//            ShowIcon = false;
//            ShowInTaskbar = false;
//            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
//            Text = "AboutBox";
//            tableLayoutPanel.ResumeLayout(false);
//            tableLayoutPanel.PerformLayout();
//            ((System.ComponentModel.ISupportInitialize)logoPictureBox).EndInit();
//            panel1.ResumeLayout(false);
//            ResumeLayout(false);
//        }

//        #endregion

//        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
//		private System.Windows.Forms.PictureBox logoPictureBox;
//		private System.Windows.Forms.Label labelProductName;
//		private System.Windows.Forms.Label labelVersion;
//		private System.Windows.Forms.Label labelCopyright;
//		private System.Windows.Forms.TextBox textBoxDescription;
//		private System.Windows.Forms.LinkLabel linkLabelURL;
//		private System.Windows.Forms.Panel panel1;
//		private System.Windows.Forms.Button okButton;
//	}
//}
