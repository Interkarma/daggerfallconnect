namespace IntermediateSeries_Tutorial_1
{
    partial class MainForm
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
            this.FilenamesListBox = new System.Windows.Forms.ListBox();
            this.DescriptionLabel = new System.Windows.Forms.Label();
            this.ImagesPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // FilenamesListBox
            // 
            this.FilenamesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.FilenamesListBox.FormattingEnabled = true;
            this.FilenamesListBox.IntegralHeight = false;
            this.FilenamesListBox.Location = new System.Drawing.Point(12, 12);
            this.FilenamesListBox.Name = "FilenamesListBox";
            this.FilenamesListBox.Size = new System.Drawing.Size(120, 706);
            this.FilenamesListBox.TabIndex = 0;
            this.FilenamesListBox.SelectedIndexChanged += new System.EventHandler(this.FilenamesListBox_SelectedIndexChanged);
            // 
            // DescriptionLabel
            // 
            this.DescriptionLabel.AutoSize = true;
            this.DescriptionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DescriptionLabel.Location = new System.Drawing.Point(138, 12);
            this.DescriptionLabel.Name = "DescriptionLabel";
            this.DescriptionLabel.Size = new System.Drawing.Size(104, 24);
            this.DescriptionLabel.TabIndex = 1;
            this.DescriptionLabel.Text = "Description";
            // 
            // ImagesPanel
            // 
            this.ImagesPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ImagesPanel.AutoScroll = true;
            this.ImagesPanel.BackColor = System.Drawing.SystemColors.Window;
            this.ImagesPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ImagesPanel.Location = new System.Drawing.Point(142, 39);
            this.ImagesPanel.Name = "ImagesPanel";
            this.ImagesPanel.Size = new System.Drawing.Size(854, 679);
            this.ImagesPanel.TabIndex = 2;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 730);
            this.Controls.Add(this.ImagesPanel);
            this.Controls.Add(this.DescriptionLabel);
            this.Controls.Add(this.FilenamesListBox);
            this.Name = "MainForm";
            this.Text = "Simple Image Viewer";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox FilenamesListBox;
        private System.Windows.Forms.Label DescriptionLabel;
        private System.Windows.Forms.FlowLayoutPanel ImagesPanel;
    }
}

