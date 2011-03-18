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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.MainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.SearchResultsPanel = new System.Windows.Forms.Panel();
            this.SearchResultsTreeView = new System.Windows.Forms.TreeView();
            this.SearchResultsImageList = new System.Windows.Forms.ImageList(this.components);
            this.panel2 = new System.Windows.Forms.Panel();
            this.SearchTextBox = new System.Windows.Forms.TextBox();
            this.LayoutPanel = new System.Windows.Forms.Panel();
            this.SearchPaneToolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.ViewPaneToolStrip = new System.Windows.Forms.ToolStrip();
            this.panel1 = new System.Windows.Forms.Panel();
            this.MainStatusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.ActionProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.MainTips = new System.Windows.Forms.ToolTip(this.components);
            this.MapBlockBrowserToolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.BlockCoordToolStripLabel = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.ClearSearchButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.CityModeToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.DungeonModeToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.BlockModeToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton5 = new System.Windows.Forms.ToolStripButton();
            this.SetArena2ToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.SearchModelsToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.SearchBlocksToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.SearchLocationsToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.AboutToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.mapBlockBrowser1 = new DaggerfallModelling.BrowserControls.MapBlockBrowser();
            this.modelBrowser1 = new DaggerfallModelling.BrowserControls.ModelBrowser();
            this.MainSplitContainer.Panel1.SuspendLayout();
            this.MainSplitContainer.Panel2.SuspendLayout();
            this.MainSplitContainer.SuspendLayout();
            this.SearchResultsPanel.SuspendLayout();
            this.panel2.SuspendLayout();
            this.LayoutPanel.SuspendLayout();
            this.SearchPaneToolStrip.SuspendLayout();
            this.ViewPaneToolStrip.SuspendLayout();
            this.panel1.SuspendLayout();
            this.MainStatusStrip.SuspendLayout();
            this.MapBlockBrowserToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainSplitContainer
            // 
            this.MainSplitContainer.BackColor = System.Drawing.SystemColors.Window;
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
            this.MainSplitContainer.Panel1.Controls.Add(this.SearchPaneToolStrip);
            this.MainSplitContainer.Panel1MinSize = 320;
            // 
            // MainSplitContainer.Panel2
            // 
            this.MainSplitContainer.Panel2.Controls.Add(this.ViewPaneToolStrip);
            this.MainSplitContainer.Panel2.Controls.Add(this.panel1);
            this.MainSplitContainer.Size = new System.Drawing.Size(1184, 762);
            this.MainSplitContainer.SplitterDistance = 320;
            this.MainSplitContainer.TabIndex = 0;
            // 
            // SearchResultsPanel
            // 
            this.SearchResultsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchResultsPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SearchResultsPanel.Controls.Add(this.SearchResultsTreeView);
            this.SearchResultsPanel.Location = new System.Drawing.Point(-2, 56);
            this.SearchResultsPanel.Name = "SearchResultsPanel";
            this.SearchResultsPanel.Size = new System.Drawing.Size(318, 338);
            this.SearchResultsPanel.TabIndex = 4;
            // 
            // SearchResultsTreeView
            // 
            this.SearchResultsTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.SearchResultsTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SearchResultsTreeView.HideSelection = false;
            this.SearchResultsTreeView.ImageIndex = 0;
            this.SearchResultsTreeView.ImageList = this.SearchResultsImageList;
            this.SearchResultsTreeView.Location = new System.Drawing.Point(0, 0);
            this.SearchResultsTreeView.Name = "SearchResultsTreeView";
            this.SearchResultsTreeView.SelectedImageIndex = 0;
            this.SearchResultsTreeView.Size = new System.Drawing.Size(316, 336);
            this.SearchResultsTreeView.TabIndex = 2;
            this.SearchResultsTreeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.SearchResultsTreeView_BeforeExpand);
            this.SearchResultsTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.SearchResultsTree_AfterSelect);
            // 
            // SearchResultsImageList
            // 
            this.SearchResultsImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("SearchResultsImageList.ImageStream")));
            this.SearchResultsImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.SearchResultsImageList.Images.SetKeyName(0, "models");
            this.SearchResultsImageList.Images.SetKeyName(1, "blocks");
            this.SearchResultsImageList.Images.SetKeyName(2, "locations");
            this.SearchResultsImageList.Images.SetKeyName(3, "region");
            this.SearchResultsImageList.Images.SetKeyName(4, "cities");
            this.SearchResultsImageList.Images.SetKeyName(5, "dungeons");
            this.SearchResultsImageList.Images.SetKeyName(6, "graveyards");
            this.SearchResultsImageList.Images.SetKeyName(7, "homes");
            this.SearchResultsImageList.Images.SetKeyName(8, "religions");
            this.SearchResultsImageList.Images.SetKeyName(9, "taverns");
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
            this.SearchTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SearchTextBox_KeyPress);
            // 
            // LayoutPanel
            // 
            this.LayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.LayoutPanel.BackColor = System.Drawing.SystemColors.ControlDark;
            this.LayoutPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LayoutPanel.Controls.Add(this.mapBlockBrowser1);
            this.LayoutPanel.Controls.Add(this.MapBlockBrowserToolStrip);
            this.LayoutPanel.Location = new System.Drawing.Point(-2, 396);
            this.LayoutPanel.Name = "LayoutPanel";
            this.LayoutPanel.Size = new System.Drawing.Size(318, 343);
            this.LayoutPanel.TabIndex = 3;
            // 
            // SearchPaneToolStrip
            // 
            this.SearchPaneToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SetArena2ToolStripButton,
            this.toolStripSeparator1,
            this.SearchModelsToolStripButton,
            this.SearchBlocksToolStripButton,
            this.SearchLocationsToolStripButton,
            this.toolStripSeparator2});
            this.SearchPaneToolStrip.Location = new System.Drawing.Point(0, 0);
            this.SearchPaneToolStrip.Name = "SearchPaneToolStrip";
            this.SearchPaneToolStrip.Size = new System.Drawing.Size(316, 25);
            this.SearchPaneToolStrip.TabIndex = 0;
            this.SearchPaneToolStrip.Text = "toolStrip1";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // ViewPaneToolStrip
            // 
            this.ViewPaneToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AboutToolStripButton});
            this.ViewPaneToolStrip.Location = new System.Drawing.Point(0, 0);
            this.ViewPaneToolStrip.Name = "ViewPaneToolStrip";
            this.ViewPaneToolStrip.Size = new System.Drawing.Size(856, 25);
            this.ViewPaneToolStrip.TabIndex = 0;
            this.ViewPaneToolStrip.Text = "toolStrip2";
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
            // MainStatusStrip
            // 
            this.MainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2,
            this.ActionProgressBar});
            this.MainStatusStrip.Location = new System.Drawing.Point(0, 740);
            this.MainStatusStrip.Name = "MainStatusStrip";
            this.MainStatusStrip.Size = new System.Drawing.Size(1184, 22);
            this.MainStatusStrip.TabIndex = 1;
            this.MainStatusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(825, 17);
            this.toolStripStatusLabel2.Spring = true;
            // 
            // ActionProgressBar
            // 
            this.ActionProgressBar.Name = "ActionProgressBar";
            this.ActionProgressBar.Size = new System.Drawing.Size(150, 16);
            // 
            // MainTips
            // 
            this.MainTips.AutoPopDelay = 5000;
            this.MainTips.InitialDelay = 500;
            this.MainTips.IsBalloon = true;
            this.MainTips.ReshowDelay = 100;
            // 
            // MapBlockBrowserToolStrip
            // 
            this.MapBlockBrowserToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CityModeToolStripButton,
            this.DungeonModeToolStripButton,
            this.BlockModeToolStripButton,
            this.toolStripSeparator3,
            this.BlockCoordToolStripLabel,
            this.toolStripButton2,
            this.toolStripButton3,
            this.toolStripButton4,
            this.toolStripButton5,
            this.toolStripSeparator4});
            this.MapBlockBrowserToolStrip.Location = new System.Drawing.Point(0, 0);
            this.MapBlockBrowserToolStrip.Name = "MapBlockBrowserToolStrip";
            this.MapBlockBrowserToolStrip.Size = new System.Drawing.Size(316, 25);
            this.MapBlockBrowserToolStrip.TabIndex = 1;
            this.MapBlockBrowserToolStrip.Text = "toolStrip1";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // BlockCoordToolStripLabel
            // 
            this.BlockCoordToolStripLabel.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.BlockCoordToolStripLabel.Name = "BlockCoordToolStripLabel";
            this.BlockCoordToolStripLabel.Size = new System.Drawing.Size(30, 22);
            this.BlockCoordToolStripLabel.Text = "0 x 0";
            this.BlockCoordToolStripLabel.Visible = false;
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Image = global::DaggerfallModelling.Properties.Resources.lightbulb;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(274, 17);
            this.toolStripStatusLabel1.Text = "C:\\dosgames\\DAGGER\\ARENA2 [HARDCODED]";
            // 
            // ClearSearchButton
            // 
            this.ClearSearchButton.Image = global::DaggerfallModelling.Properties.Resources.cancel;
            this.ClearSearchButton.Location = new System.Drawing.Point(295, 6);
            this.ClearSearchButton.Name = "ClearSearchButton";
            this.ClearSearchButton.Size = new System.Drawing.Size(20, 20);
            this.ClearSearchButton.TabIndex = 2;
            this.MainTips.SetToolTip(this.ClearSearchButton, "Clear Search");
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
            // CityModeToolStripButton
            // 
            this.CityModeToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.CityModeToolStripButton.Enabled = false;
            this.CityModeToolStripButton.Image = global::DaggerfallModelling.Properties.Resources.CitiesFilter;
            this.CityModeToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.CityModeToolStripButton.Name = "CityModeToolStripButton";
            this.CityModeToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.CityModeToolStripButton.Text = "City Mode";
            // 
            // DungeonModeToolStripButton
            // 
            this.DungeonModeToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.DungeonModeToolStripButton.Enabled = false;
            this.DungeonModeToolStripButton.Image = global::DaggerfallModelling.Properties.Resources.DungeonsFilter;
            this.DungeonModeToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.DungeonModeToolStripButton.Name = "DungeonModeToolStripButton";
            this.DungeonModeToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.DungeonModeToolStripButton.Text = "Dungeon Mode";
            // 
            // BlockModeToolStripButton
            // 
            this.BlockModeToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BlockModeToolStripButton.Enabled = false;
            this.BlockModeToolStripButton.Image = global::DaggerfallModelling.Properties.Resources.bricks;
            this.BlockModeToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BlockModeToolStripButton.Name = "BlockModeToolStripButton";
            this.BlockModeToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.BlockModeToolStripButton.Text = "Block Mode";
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Enabled = false;
            this.toolStripButton2.Image = global::DaggerfallModelling.Properties.Resources.arrow_up;
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton2.Text = "toolStripButton2";
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton3.Enabled = false;
            this.toolStripButton3.Image = global::DaggerfallModelling.Properties.Resources.arrow_down;
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton3.Text = "toolStripButton3";
            // 
            // toolStripButton4
            // 
            this.toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton4.Enabled = false;
            this.toolStripButton4.Image = global::DaggerfallModelling.Properties.Resources.arrow_left;
            this.toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton4.Name = "toolStripButton4";
            this.toolStripButton4.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton4.Text = "toolStripButton4";
            // 
            // toolStripButton5
            // 
            this.toolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton5.Enabled = false;
            this.toolStripButton5.Image = global::DaggerfallModelling.Properties.Resources.arrow_right;
            this.toolStripButton5.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton5.Name = "toolStripButton5";
            this.toolStripButton5.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton5.Text = "toolStripButton5";
            // 
            // SetArena2ToolStripButton
            // 
            this.SetArena2ToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SetArena2ToolStripButton.Image = global::DaggerfallModelling.Properties.Resources.lightbulb;
            this.SetArena2ToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SetArena2ToolStripButton.Name = "SetArena2ToolStripButton";
            this.SetArena2ToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.SetArena2ToolStripButton.Text = "Set Arena2 Folder";
            // 
            // SearchModelsToolStripButton
            // 
            this.SearchModelsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SearchModelsToolStripButton.Image = global::DaggerfallModelling.Properties.Resources.brick;
            this.SearchModelsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SearchModelsToolStripButton.Name = "SearchModelsToolStripButton";
            this.SearchModelsToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.SearchModelsToolStripButton.Text = "Search Models";
            this.SearchModelsToolStripButton.Click += new System.EventHandler(this.SearchModelsToolStripButton_Click);
            // 
            // SearchBlocksToolStripButton
            // 
            this.SearchBlocksToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SearchBlocksToolStripButton.Image = global::DaggerfallModelling.Properties.Resources.bricks;
            this.SearchBlocksToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SearchBlocksToolStripButton.Name = "SearchBlocksToolStripButton";
            this.SearchBlocksToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.SearchBlocksToolStripButton.Text = "Search Blocks";
            this.SearchBlocksToolStripButton.Click += new System.EventHandler(this.SearchBlocksToolStripButton_Click);
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
            this.SearchLocationsToolStripButton.Click += new System.EventHandler(this.SearchLocationsToolStripButton_Click);
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
            // mapBlockBrowser1
            // 
            this.mapBlockBrowser1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.mapBlockBrowser1.BlocksFile = null;
            this.mapBlockBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mapBlockBrowser1.Location = new System.Drawing.Point(0, 25);
            this.mapBlockBrowser1.MapsFile = null;
            this.mapBlockBrowser1.Name = "mapBlockBrowser1";
            this.mapBlockBrowser1.Size = new System.Drawing.Size(316, 316);
            this.mapBlockBrowser1.TabIndex = 0;
            this.mapBlockBrowser1.Text = "mapBlockBrowser1";
            this.mapBlockBrowser1.ModeChanged += new DaggerfallModelling.BrowserControls.MapBlockBrowser.ModeChangedEventHandler(this.mapBlockBrowser1_ModeChanged);
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
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1184, 762);
            this.Controls.Add(this.MainStatusStrip);
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
            this.SearchResultsPanel.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.LayoutPanel.ResumeLayout(false);
            this.LayoutPanel.PerformLayout();
            this.SearchPaneToolStrip.ResumeLayout(false);
            this.SearchPaneToolStrip.PerformLayout();
            this.ViewPaneToolStrip.ResumeLayout(false);
            this.ViewPaneToolStrip.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.MainStatusStrip.ResumeLayout(false);
            this.MainStatusStrip.PerformLayout();
            this.MapBlockBrowserToolStrip.ResumeLayout(false);
            this.MapBlockBrowserToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer MainSplitContainer;
        private System.Windows.Forms.ToolStrip SearchPaneToolStrip;
        private System.Windows.Forms.ToolStripButton SetArena2ToolStripButton;
        private System.Windows.Forms.StatusStrip MainStatusStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.TreeView SearchResultsTreeView;
        private System.Windows.Forms.Panel LayoutPanel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStrip ViewPaneToolStrip;
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
        private System.Windows.Forms.ToolTip MainTips;
        private System.Windows.Forms.ImageList SearchResultsImageList;
        private DaggerfallModelling.BrowserControls.MapBlockBrowser mapBlockBrowser1;
        private System.Windows.Forms.ToolStrip MapBlockBrowserToolStrip;
        private System.Windows.Forms.ToolStripButton CityModeToolStripButton;
        private System.Windows.Forms.ToolStripButton DungeonModeToolStripButton;
        private System.Windows.Forms.ToolStripButton BlockModeToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripLabel BlockCoordToolStripLabel;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.ToolStripButton toolStripButton4;
        private System.Windows.Forms.ToolStripButton toolStripButton5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
    }
}

