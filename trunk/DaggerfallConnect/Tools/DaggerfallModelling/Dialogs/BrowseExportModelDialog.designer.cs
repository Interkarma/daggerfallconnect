namespace DaggerfallModelling.Dialogs
{
    partial class BrowseExportModelDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BrowseExportModelDialog));
            this.OutputPathTextBox = new System.Windows.Forms.TextBox();
            this.MyOKButton = new System.Windows.Forms.Button();
            this.MyCancelButton = new System.Windows.Forms.Button();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.OrientationComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ImageFormatComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // OutputPathTextBox
            // 
            this.OutputPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputPathTextBox.Location = new System.Drawing.Point(12, 25);
            this.OutputPathTextBox.Name = "OutputPathTextBox";
            this.OutputPathTextBox.Size = new System.Drawing.Size(339, 20);
            this.OutputPathTextBox.TabIndex = 1;
            // 
            // MyOKButton
            // 
            this.MyOKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.MyOKButton.Location = new System.Drawing.Point(12, 129);
            this.MyOKButton.Name = "MyOKButton";
            this.MyOKButton.Size = new System.Drawing.Size(75, 23);
            this.MyOKButton.TabIndex = 7;
            this.MyOKButton.Text = "OK";
            this.MyOKButton.UseVisualStyleBackColor = true;
            this.MyOKButton.Click += new System.EventHandler(this.MyOKButton_Click);
            // 
            // MyCancelButton
            // 
            this.MyCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.MyCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.MyCancelButton.Location = new System.Drawing.Point(329, 129);
            this.MyCancelButton.Name = "MyCancelButton";
            this.MyCancelButton.Size = new System.Drawing.Size(75, 23);
            this.MyCancelButton.TabIndex = 8;
            this.MyCancelButton.Text = "Cancel";
            this.MyCancelButton.UseVisualStyleBackColor = true;
            // 
            // BrowseButton
            // 
            this.BrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseButton.Image = global::DaggerfallModelling.Properties.Resources.folder_explore;
            this.BrowseButton.Location = new System.Drawing.Point(357, 23);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(47, 23);
            this.BrowseButton.TabIndex = 2;
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Output Path";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(165, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Orientation (e.g. Blender is Z_UP)";
            // 
            // OrientationComboBox
            // 
            this.OrientationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.OrientationComboBox.FormattingEnabled = true;
            this.OrientationComboBox.Items.AddRange(new object[] {
            "X_UP",
            "Y_UP",
            "Z_UP"});
            this.OrientationComboBox.Location = new System.Drawing.Point(12, 77);
            this.OrientationComboBox.Name = "OrientationComboBox";
            this.OrientationComboBox.Size = new System.Drawing.Size(128, 21);
            this.OrientationComboBox.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(220, 61);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Image Format";
            // 
            // ImageFormatComboBox
            // 
            this.ImageFormatComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ImageFormatComboBox.FormattingEnabled = true;
            this.ImageFormatComboBox.Items.AddRange(new object[] {
            "JPEG Image (*.JPG)",
            "PNG Image (*.PNG)",
            "TIF Image (*.TIF)",
            "Windows Bitmap (*.BMP)"});
            this.ImageFormatComboBox.Location = new System.Drawing.Point(223, 77);
            this.ImageFormatComboBox.Name = "ImageFormatComboBox";
            this.ImageFormatComboBox.Size = new System.Drawing.Size(181, 21);
            this.ImageFormatComboBox.TabIndex = 6;
            // 
            // BrowseExportModelDialog
            // 
            this.AcceptButton = this.MyOKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.MyCancelButton;
            this.ClientSize = new System.Drawing.Size(416, 164);
            this.Controls.Add(this.ImageFormatComboBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.OrientationComboBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BrowseButton);
            this.Controls.Add(this.OutputPathTextBox);
            this.Controls.Add(this.MyCancelButton);
            this.Controls.Add(this.MyOKButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BrowseExportModelDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Export Model";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox OutputPathTextBox;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.Button MyOKButton;
        private System.Windows.Forms.Button MyCancelButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox OrientationComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox ImageFormatComboBox;
    }
}