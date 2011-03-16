namespace DaggerfallModelling
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.MainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.SearchResultsTreeView = new System.Windows.Forms.TreeView();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.panel1 = new System.Windows.Forms.Panel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.ActionProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.panel2 = new System.Windows.Forms.Panel();
            this.SearchTextBox = new System.Windows.Forms.TextBox();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.ClearSearchButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.LayoutPanel = new System.Windows.Forms.Panel();
            this.SetArena2ToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.AboutToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.SearchResultsPanel = new System.Windows.Forms.Panel();
            this.modelBrowser1 = new DaggerfallModelling.BrowserControls.ModelBrowser();
            this.SearchModelsToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.SearchBlocksToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.SearchLocationsToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.MainSplitContainer.Panel1.SuspendLayout();
            this.MainSplitContainer.Panel2.SuspendLayout();
            this.MainSplitContainer.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SearchResultsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainSplitContainer
            // 
            this.MainSplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.MainSplitContainer.IsSplitterFixed = true;
            this.MainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.MainSplitContainer.Name = "MainSplitContainer";
            // 
            // MainSplitContainer.Panel1
            // 
            this.MainSplitContainer.Panel1.Controls.Add(this.SearchResultsPanel);
            this.MainSplitContainer.Panel1.Controls.Add(this.panel2);
            this.MainSplitContainer.Panel1.Controls.Add(this.LayoutPanel);
            this.MainSplitContainer.Panel1.Controls.Add(this.toolStrip1);
            this.MainSplitContainer.Panel1MinSize = 320;
            // 
            // MainSplitContainer.Panel2
            // 
            this.MainSplitContainer.Panel2.Controls.Add(this.toolStrip2);
            this.MainSplitContainer.Panel2.Controls.Add(this.panel1);
            this.MainSplitContainer.Size = new System.Drawing.Size(1184, 762);
            this.MainSplitContainer.SplitterDistance = 320;
            this.MainSplitContainer.TabIndex = 0;
            // 
            // SearchResultsTreeView
            // 
            this.SearchResultsTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SearchResultsTreeView.Location = new System.Drawing.Point(0, 0);
            this.SearchResultsTreeView.Name = "SearchResultsTreeView";
            this.SearchResultsTreeView.Size = new System.Drawing.Size(318, 366);
            this.SearchResultsTreeView.TabIndex = 2;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SetArena2ToolStripButton,
            this.toolStripSeparator1,
            this.SearchModelsToolStripButton,
            this.SearchBlocksToolStripButton,
            this.SearchLocationsToolStripButton,
            this.toolStripSeparator2});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(316, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStrip2
            // 
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.AboutToolStripButton});
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(856, 25);
            this.toolStrip2.TabIndex = 0;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.modelBrowser1);
            this.panel1.Location = new System.Drawing.Point(0, 25);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(858, 715);
            this.panel1.TabIndex = 2;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2,
            this.ActionProgressBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 740);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1184, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(837, 17);
            this.toolStripStatusLabel2.Spring = true;
            // 
            // ActionProgressBar
            // 
            this.ActionProgressBar.Name = "ActionProgressBar";
            this.ActionProgressBar.Size = new System.Drawing.Size(150, 16);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel2.Controls.Add(this.ClearSearchButton);
            this.panel2.Controls.Add(this.SearchTextBox);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Location = new System.Drawing.Point(-2, 25);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(318, 31);
            this.panel2.TabIndex = 1;
            // 
            // SearchTextBox
            // 
            this.SearchTextBox.AcceptsReturn = true;
            this.SearchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchTextBox.Location = new System.Drawing.Point(29, 6);
            this.SearchTextBox.Name = "SearchTextBox";
            this.SearchTextBox.Size = new System.Drawing.Size(264, 20);
            this.SearchTextBox.TabIndex = 1;
            this.SearchTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SearchTextBox_KeyDown);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Image = global::DaggerfallModelling.Properties.Resources.lightbulb_off;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(180, 17);
            this.toolStripStatusLabel1.Text = "Please set your Arena2 Folder.";
            // 
            // ClearSearchButton
            // 
            this.ClearSearchButton.Image = global::DaggerfallModelling.Properties.Resources.cancel;
            this.ClearSearchButton.Location = new System.Drawing.Point(295, 6);
            this.ClearSearchButton.Name = "ClearSearchButton";
            this.ClearSearchButton.Size = new System.Drawing.Size(20, 20);
            this.ClearSearchButton.TabIndex = 2;
            this.ClearSearchButton.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Image = global::DaggerfallModelling.Properties.Resources.find;
            this.label1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label1.Location = new System.Drawing.Point(5, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(18, 13);
            this.label1.TabIndex = 0;
            // 
            // LayoutPanel
            // 
            this.LayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.LayoutPanel.BackColor = System.Drawing.SystemColors.ControlDark;
            this.LayoutPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LayoutPanel.Location = new System.Drawing.Point(-2, 424);
            this.LayoutPanel.Name = "LayoutPanel";
            this.LayoutPanel.Size = new System.Drawing.Size(318, 314);
            this.LayoutPanel.TabIndex = 3;
            // 
            // SetArena2ToolStripButton
            // 
            this.SetArena2ToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SetArena2ToolStripButton.Image = global::DaggerfallModelling.Properties.Resources.lightbulb_off;
            this.SetArena2ToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SetArena2ToolStripButton.Name = "SetArena2ToolStripButton";
            this.SetArena2ToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.SetArena2ToolStripButton.Text = "Set Arena2 Folder";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "toolStripButton1";
            // 
            // AboutToolStripButton
            // 
            this.AboutToolStripButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.AboutToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.AboutToolStripButton.Image = global::DaggerfallModelling.Properties.Resources.help;
            this.AboutToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.AboutToolStripButton.Name = "AboutToolStripButton";
            this.AboutToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.AboutToolStripButton.Text = "About Daggerfall Modelling";
            // 
            // SearchResultsPanel
            // 
            this.SearchResultsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchResultsPanel.Controls.Add(this.SearchResultsTreeView);
            this.SearchResultsPanel.Location = new System.Drawing.Point(-2, 56);
            this.SearchResultsPanel.Name = "SearchResultsPanel";
            this.SearchResultsPanel.Size = new System.Drawing.Size(318, 366);
            this.SearchResultsPanel.TabIndex = 4;
            // 
            // modelBrowser1
            // 
            this.modelBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modelBrowser1.Location = new System.Drawing.Point(0, 0);
            this.modelBrowser1.Name = "modelBrowser1";
            this.modelBrowser1.Size = new System.Drawing.Size(858, 715);
            this.modelBrowser1.TabIndex = 0;
            this.modelBrowser1.Text = "modelBrowser1";
            // 
            // SearchModelsToolStripButton
            // 
            this.SearchModelsToolStripButton.Checked = true;
            this.SearchModelsToolStripButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SearchModelsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SearchModelsToolStripButton.Image = global::DaggerfallModelling.Properties.Resources.brick;
            this.SearchModelsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SearchModelsToolStripButton.Name = "SearchModelsToolStripButton";
            this.SearchModelsToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.SearchModelsToolStripButton.Text = "Search Models";
            // 
            // SearchBlocksToolStripButton
            // 
            this.SearchBlocksToolStripButton.Checked = true;
            this.SearchBlocksToolStripButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SearchBlocksToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SearchBlocksToolStripButton.Image = global::DaggerfallModelling.Properties.Resources.bricks;
            this.SearchBlocksToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SearchBlocksToolStripButton.Name = "SearchBlocksToolStripButton";
            this.SearchBlocksToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.SearchBlocksToolStripButton.Text = "Search Blocks";
            // 
            // SearchLocationsToolStripButton
            // 
            this.SearchLocationsToolStripButton.Checked = true;
            this.SearchLocationsToolStripButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SearchLocationsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SearchLocationsToolStripButton.Image = global::DaggerfallModelling.Properties.Resources.house;
            this.SearchLocationsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SearchLocationsToolStripButton.Name = "SearchLocationsToolStripButton";
            this.SearchLocationsToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.SearchLocationsToolStripButton.Text = "Search Locations";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1184, 762);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.MainSplitContainer);
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Daggerfall Modelling";
            this.MainSplitContainer.Panel1.ResumeLayout(false);
            this.MainSplitContainer.Panel1.PerformLayout();
            this.MainSplitContainer.Panel2.ResumeLayout(false);
            this.MainSplitContainer.Panel2.PerformLayout();
            this.MainSplitContainer.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.SearchResultsPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer MainSplitContainer;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton SetArena2ToolStripButton;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.TreeView SearchResultsTreeView;
        private System.Windows.Forms.Panel LayoutPanel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripProgressBar ActionProgressBar;
        private System.Windows.Forms.ToolStripButton AboutToolStripButton;
        private System.Windows.Forms.Panel panel1;
        private DaggerfallModelling.BrowserControls.ModelBrowser modelBrowser1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox SearchTextBox;
        private System.Windows.Forms.Button ClearSearchButton;
        private System.Windows.Forms.Panel SearchResultsPanel;
        private System.Windows.Forms.ToolStripButton SearchModelsToolStripButton;
        private System.Windows.Forms.ToolStripButton SearchBlocksToolStripButton;
        private System.Windows.Forms.ToolStripButton SearchLocationsToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    }
}

