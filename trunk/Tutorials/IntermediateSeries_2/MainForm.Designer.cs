namespace IntermediateSeries_Tutorial2
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
            this.MeshIndexTrackBar = new System.Windows.Forms.TrackBar();
            this.RenderPanel = new System.Windows.Forms.Panel();
            this.RenderPictureBox = new System.Windows.Forms.PictureBox();
            this.TexturesPanel = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.MeshIndexTrackBar)).BeginInit();
            this.RenderPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RenderPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // MeshIndexTrackBar
            // 
            this.MeshIndexTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.MeshIndexTrackBar.Location = new System.Drawing.Point(12, 12);
            this.MeshIndexTrackBar.Maximum = 10250;
            this.MeshIndexTrackBar.Name = "MeshIndexTrackBar";
            this.MeshIndexTrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.MeshIndexTrackBar.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.MeshIndexTrackBar.Size = new System.Drawing.Size(45, 706);
            this.MeshIndexTrackBar.TabIndex = 0;
            this.MeshIndexTrackBar.Scroll += new System.EventHandler(this.MeshIndexTrackBar_Scroll);
            // 
            // RenderPanel
            // 
            this.RenderPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.RenderPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.RenderPanel.Controls.Add(this.RenderPictureBox);
            this.RenderPanel.Location = new System.Drawing.Point(63, 12);
            this.RenderPanel.Name = "RenderPanel";
            this.RenderPanel.Size = new System.Drawing.Size(687, 706);
            this.RenderPanel.TabIndex = 1;
            // 
            // RenderPictureBox
            // 
            this.RenderPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.RenderPictureBox.Location = new System.Drawing.Point(3, 3);
            this.RenderPictureBox.Name = "RenderPictureBox";
            this.RenderPictureBox.Size = new System.Drawing.Size(679, 698);
            this.RenderPictureBox.TabIndex = 0;
            this.RenderPictureBox.TabStop = false;
            // 
            // TexturesPanel
            // 
            this.TexturesPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TexturesPanel.BackColor = System.Drawing.SystemColors.Window;
            this.TexturesPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TexturesPanel.Location = new System.Drawing.Point(756, 12);
            this.TexturesPanel.Name = "TexturesPanel";
            this.TexturesPanel.Size = new System.Drawing.Size(240, 706);
            this.TexturesPanel.TabIndex = 2;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 730);
            this.Controls.Add(this.TexturesPanel);
            this.Controls.Add(this.RenderPanel);
            this.Controls.Add(this.MeshIndexTrackBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Simple Mesh Viewer";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.MeshIndexTrackBar)).EndInit();
            this.RenderPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.RenderPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar MeshIndexTrackBar;
        private System.Windows.Forms.Panel RenderPanel;
        private System.Windows.Forms.FlowLayoutPanel TexturesPanel;
        private System.Windows.Forms.PictureBox RenderPictureBox;
    }
}

