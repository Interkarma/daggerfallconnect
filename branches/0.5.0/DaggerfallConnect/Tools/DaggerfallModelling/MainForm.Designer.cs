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
            this.SearchPanel = new System.Windows.Forms.Panel();
            this.ClearSearchButton = new System.Windows.Forms.Button();
            this.SearchTextBox = new System.Windows.Forms.TextBox();
            this.SearchLabel = new System.Windows.Forms.Label();
            this.LayoutPanel = new System.Windows.Forms.Panel();
            this.AutoMapViewer = new DaggerfallModelling.ViewControls.AutoMapView();
            this.AutoMapToolStrip = new System.Windows.Forms.ToolStrip();
            this.ExteriorModeToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.DungeonModeToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.BlockNameToolStripLabel = new System.Windows.Forms.ToolStripLabel();
            this.SearchPaneToolStrip = new System.Windows.Forms.ToolStrip();
            this.SetArena2ToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.SearchModelsToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.SearchBlocksToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.SearchLocationsToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.ViewPaneToolStrip = new System.Windows.Forms.ToolStrip();
            this.AboutToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.ViewThumbsToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.ViewSingleModelToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.ViewBlockToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.ContentPanel = new System.Windows.Forms.Panel();
            this.ContentView = new DaggerfallModelling.ViewControls.ContentViewHost();
            this.MainStatusStrip = new System.Windows.Forms.StatusStrip();
            this.Arena2PathStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.ModelViewStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.MainTips = new System.Windows.Forms.ToolTip(this.components);
            this.ViewLocationToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.MainSplitContainer.Panel1.SuspendLayout();
            this.MainSplitContainer.Panel2.SuspendLayout();
            this.MainSplitContainer.SuspendLayout();
            this.SearchResultsPanel.SuspendLayout();
            this.SearchPanel.SuspendLayout();
            this.LayoutPanel.SuspendLayout();
            this.AutoMapToolStrip.SuspendLayout();
            this.SearchPaneToolStrip.SuspendLayout();
            this.ViewPaneToolStrip.SuspendLayout();
            this.ContentPanel.SuspendLayout();
            this.MainStatusStrip.SuspendLayout();
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
            this.MainSplitContainer.Panel1.Controls.Add(this.SearchPanel);
            this.MainSplitContainer.Panel1.Controls.Add(this.LayoutPanel);
            this.MainSplitContainer.Panel1.Controls.Add(this.SearchPaneToolStrip);
            this.MainSplitContainer.Panel1MinSize = 324;
            // 
            // MainSplitContainer.Panel2
            // 
            this.MainSplitContainer.Panel2.Controls.Add(this.ViewPaneToolStrip);
            this.MainSplitContainer.Panel2.Controls.Add(this.ContentPanel);
            this.MainSplitContainer.Size = new System.Drawing.Size(1184, 762);
            this.MainSplitContainer.SplitterDistance = 324;
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
            this.SearchResultsPanel.Size = new System.Drawing.Size(322, 334);
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
            this.SearchResultsTreeView.Size = new System.Drawing.Size(320, 332);
            this.SearchResultsTreeView.TabIndex = 2;
            this.SearchResultsTreeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.SearchResultsTreeView_BeforeExpand);
            this.SearchResultsTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.SearchResultsTree_AfterSelect);
            // 
            // SearchResultsImageList
            // 
            this.SearchResultsImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("SearchResultsImageList.ImageStream")));
            this.SearchResultsImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.SearchResultsImageList.Images.SetKeyName(0, "find");
            this.SearchResultsImageList.Images.SetKeyName(1, "models");
            this.SearchResultsImageList.Images.SetKeyName(2, "blocks");
            this.SearchResultsImageList.Images.SetKeyName(3, "locations");
            this.SearchResultsImageList.Images.SetKeyName(4, "region");
            this.SearchResultsImageList.Images.SetKeyName(5, "cities");
            this.SearchResultsImageList.Images.SetKeyName(6, "dungeons");
            this.SearchResultsImageList.Images.SetKeyName(7, "graveyards");
            this.SearchResultsImageList.Images.SetKeyName(8, "homes");
            this.SearchResultsImageList.Images.SetKeyName(9, "religions");
            this.SearchResultsImageList.Images.SetKeyName(10, "taverns");
            // 
            // SearchPanel
            // 
            this.SearchPanel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.SearchPanel.Controls.Add(this.ClearSearchButton);
            this.SearchPanel.Controls.Add(this.SearchTextBox);
            this.SearchPanel.Controls.Add(this.SearchLabel);
            this.SearchPanel.Location = new System.Drawing.Point(-2, 25);
            this.SearchPanel.Name = "SearchPanel";
            this.SearchPanel.Size = new System.Drawing.Size(322, 31);
            this.SearchPanel.TabIndex = 1;
            // 
            // ClearSearchButton
            // 
            this.ClearSearchButton.Image = global::DaggerfallModelling.Properties.Resources.cancel;
            this.ClearSearchButton.Location = new System.Drawing.Point(299, 6);
            this.ClearSearchButton.Name = "ClearSearchButton";
            this.ClearSearchButton.Size = new System.Drawing.Size(20, 20);
            this.ClearSearchButton.TabIndex = 2;
            this.MainTips.SetToolTip(this.ClearSearchButton, "Clear Search");
            this.ClearSearchButton.UseVisualStyleBackColor = true;
            // 
            // SearchTextBox
            // 
            this.SearchTextBox.AcceptsReturn = true;
            this.SearchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchTextBox.Location = new System.Drawing.Point(29, 6);
            this.SearchTextBox.Name = "SearchTextBox";
            this.SearchTextBox.Size = new System.Drawing.Size(268, 20);
            this.SearchTextBox.TabIndex = 1;
            this.SearchTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SearchTextBox_KeyPress);
            // 
            // SearchLabel
            // 
            this.SearchLabel.Image = global::DaggerfallModelling.Properties.Resources.find;
            this.SearchLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.SearchLabel.Location = new System.Drawing.Point(5, 9);
            this.SearchLabel.Name = "SearchLabel";
            this.SearchLabel.Size = new System.Drawing.Size(18, 13);
            this.SearchLabel.TabIndex = 0;
            this.MainTips.SetToolTip(this.SearchLabel, "Enter search string");
            // 
            // LayoutPanel
            // 
            this.LayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.LayoutPanel.BackColor = System.Drawing.SystemColors.ControlDark;
            this.LayoutPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LayoutPanel.Controls.Add(this.AutoMapViewer);
            this.LayoutPanel.Controls.Add(this.AutoMapToolStrip);
            this.LayoutPanel.Location = new System.Drawing.Point(-2, 392);
            this.LayoutPanel.Name = "LayoutPanel";
            this.LayoutPanel.Size = new System.Drawing.Size(322, 347);
            this.LayoutPanel.TabIndex = 3;
            // 
            // AutoMapViewer
            // 
            this.AutoMapViewer.BackColor = System.Drawing.Color.Gray;
            this.AutoMapViewer.BlocksFile = null;
            this.AutoMapViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AutoMapViewer.Location = new System.Drawing.Point(0, 25);
            this.AutoMapViewer.MapsFile = null;
            this.AutoMapViewer.Name = "AutoMapViewer";
            this.AutoMapViewer.Size = new System.Drawing.Size(320, 320);
            this.AutoMapViewer.TabIndex = 2;
            this.AutoMapViewer.Text = "AutoMapViewer";
            this.AutoMapViewer.MouseOverBlockChanged += new DaggerfallModelling.ViewControls.AutoMapView.MouseOverBlockChangedEventHandler(this.AutoMapView_MouseOverBlockChanged);
            this.AutoMapViewer.ModeChanged += new DaggerfallModelling.ViewControls.AutoMapView.ModeChangedEventHandler(this.AutoMapView_ModeChanged);
            this.AutoMapViewer.SelectedBlockChanged += new DaggerfallModelling.ViewControls.AutoMapView.SelectedBlockChangedEventHandler(this.AutoMapView_SelectedBlockChanged);
            // 
            // AutoMapToolStrip
            // 
            this.AutoMapToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ExteriorModeToolStripButton,
            this.DungeonModeToolStripButton,
            this.toolStripSeparator3,
            this.BlockNameToolStripLabel});
            this.AutoMapToolStrip.Location = new System.Drawing.Point(0, 0);
            this.AutoMapToolStrip.Name = "AutoMapToolStrip";
            this.AutoMapToolStrip.Size = new System.Drawing.Size(320, 25);
            this.AutoMapToolStrip.TabIndex = 1;
            this.AutoMapToolStrip.Text = "toolStrip1";
            // 
            // ExteriorModeToolStripButton
            // 
            this.ExteriorModeToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ExteriorModeToolStripButton.Enabled = false;
            this.ExteriorModeToolStripButton.Image = global::DaggerfallModelling.Properties.Resources.CitiesFilter;
            this.ExteriorModeToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ExteriorModeToolStripButton.Name = "ExteriorModeToolStripButton";
            this.ExteriorModeToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.ExteriorModeToolStripButton.Text = "Exterior Mode";
            this.ExteriorModeToolStripButton.Click += new System.EventHandler(this.ExteriorModeToolStripButton_Click);
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
            this.DungeonModeToolStripButton.Click += new System.EventHandler(this.DungeonModeToolStripButton_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // BlockNameToolStripLabel
            // 
            this.BlockNameToolStripLabel.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.BlockNameToolStripLabel.Name = "BlockNameToolStripLabel";
            this.BlockNameToolStripLabel.Size = new System.Drawing.Size(97, 22);
            this.BlockNameToolStripLabel.Text = "BlockName [0, 0]";
            this.BlockNameToolStripLabel.Visible = false;
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
            this.SearchPaneToolStrip.Size = new System.Drawing.Size(320, 25);
            this.SearchPaneToolStrip.TabIndex = 0;
            this.SearchPaneToolStrip.Text = "toolStrip1";
            // 
            // SetArena2ToolStripButton
            // 
            this.SetArena2ToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SetArena2ToolStripButton.Image = global::DaggerfallModelling.Properties.Resources.lightbulb_off;
            this.SetArena2ToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SetArena2ToolStripButton.Name = "SetArena2ToolStripButton";
            this.SetArena2ToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.SetArena2ToolStripButton.Text = "Set Arena2 Folder";
            this.SetArena2ToolStripButton.Click += new System.EventHandler(this.SetArena2ToolStripButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // SearchModelsToolStripButton
            // 
            this.SearchModelsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SearchModelsToolStripButton.Image = global::DaggerfallModelling.Properties.Resources.model_find;
            this.SearchModelsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SearchModelsToolStripButton.Name = "SearchModelsToolStripButton";
            this.SearchModelsToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.SearchModelsToolStripButton.Text = "Search Models";
            this.SearchModelsToolStripButton.Click += new System.EventHandler(this.SearchModelsToolStripButton_Click);
            // 
            // SearchBlocksToolStripButton
            // 
            this.SearchBlocksToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SearchBlocksToolStripButton.Image = global::DaggerfallModelling.Properties.Resources.block_find;
            this.SearchBlocksToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SearchBlocksToolStripButton.Name = "SearchBlocksToolStripButton";
            this.SearchBlocksToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.SearchBlocksToolStripButton.Text = "Search Blocks";
            this.SearchBlocksToolStripButton.Click += new System.EventHandler(this.SearchBlocksToolStripButton_Click);
            // 
            // SearchLocationsToolStripButton
            // 
            this.SearchLocationsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SearchLocationsToolStripButton.Image = global::DaggerfallModelling.Properties.Resources.location_find;
            this.SearchLocationsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SearchLocationsToolStripButton.Name = "SearchLocationsToolStripButton";
            this.SearchLocationsToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.SearchLocationsToolStripButton.Text = "Search Locations";
            this.SearchLocationsToolStripButton.Click += new System.EventHandler(this.SearchLocationsToolStripButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // ViewPaneToolStrip
            // 
            this.ViewPaneToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AboutToolStripButton,
            this.ViewThumbsToolStripButton,
            this.ViewSingleModelToolStripButton,
            this.ViewBlockToolStripButton,
            this.ViewLocationToolStripButton,
            this.toolStripSeparator4});
            this.ViewPaneToolStrip.Location = new System.Drawing.Point(0, 0);
            this.ViewPaneToolStrip.Name = "ViewPaneToolStrip";
            this.ViewPaneToolStrip.Size = new System.Drawing.Size(852, 25);
            this.ViewPaneToolStrip.TabIndex = 0;
            this.ViewPaneToolStrip.Text = "toolStrip2";
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
            this.AboutToolStripButton.Click += new System.EventHandler(this.AboutToolStripButton_Click);
            // 
            // ViewThumbsToolStripButton
            // 
            this.ViewThumbsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ViewThumbsToolStripButton.Image = global::DaggerfallModelling.Properties.Resources.application_view_tile;
            this.ViewThumbsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ViewThumbsToolStripButton.Name = "ViewThumbsToolStripButton";
            this.ViewThumbsToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.ViewThumbsToolStripButton.Text = "View Thumbnails";
            // 
            // ViewSingleModelToolStripButton
            // 
            this.ViewSingleModelToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ViewSingleModelToolStripButton.Image = global::DaggerfallModelling.Properties.Resources.brick;
            this.ViewSingleModelToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ViewSingleModelToolStripButton.Name = "ViewSingleModelToolStripButton";
            this.ViewSingleModelToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.ViewSingleModelToolStripButton.Text = "View Single Model";
            // 
            // ViewBlockToolStripButton
            // 
            this.ViewBlockToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ViewBlockToolStripButton.Image = global::DaggerfallModelling.Properties.Resources.bricks;
            this.ViewBlockToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ViewBlockToolStripButton.Name = "ViewBlockToolStripButton";
            this.ViewBlockToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.ViewBlockToolStripButton.Text = "View Block";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // ContentPanel
            // 
            this.ContentPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ContentPanel.BackColor = System.Drawing.Color.Gray;
            this.ContentPanel.Controls.Add(this.ContentView);
            this.ContentPanel.Location = new System.Drawing.Point(0, 25);
            this.ContentPanel.Name = "ContentPanel";
            this.ContentPanel.Size = new System.Drawing.Size(862, 715);
            this.ContentPanel.TabIndex = 2;
            // 
            // ContentView
            // 
            this.ContentView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ContentView.Location = new System.Drawing.Point(0, 0);
            this.ContentView.Name = "ContentView";
            this.ContentView.Size = new System.Drawing.Size(862, 715);
            this.ContentView.TabIndex = 0;
            this.ContentView.Text = "ContentView";
            // 
            // MainStatusStrip
            // 
            this.MainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Arena2PathStatusLabel,
            this.ModelViewStatusLabel,
            this.toolStripStatusLabel2});
            this.MainStatusStrip.Location = new System.Drawing.Point(0, 740);
            this.MainStatusStrip.Name = "MainStatusStrip";
            this.MainStatusStrip.Size = new System.Drawing.Size(1184, 22);
            this.MainStatusStrip.TabIndex = 1;
            this.MainStatusStrip.Text = "statusStrip1";
            // 
            // Arena2PathStatusLabel
            // 
            this.Arena2PathStatusLabel.AutoSize = false;
            this.Arena2PathStatusLabel.Image = global::DaggerfallModelling.Properties.Resources.lightbulb;
            this.Arena2PathStatusLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Arena2PathStatusLabel.Name = "Arena2PathStatusLabel";
            this.Arena2PathStatusLabel.Size = new System.Drawing.Size(328, 17);
            this.Arena2PathStatusLabel.Text = "Please set your Arena2 folder.";
            this.Arena2PathStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ModelViewStatusLabel
            // 
            this.ModelViewStatusLabel.Image = global::DaggerfallModelling.Properties.Resources.information;
            this.ModelViewStatusLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ModelViewStatusLabel.Name = "ModelViewStatusLabel";
            this.ModelViewStatusLabel.Size = new System.Drawing.Size(16, 17);
            this.ModelViewStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(825, 17);
            this.toolStripStatusLabel2.Spring = true;
            // 
            // MainTips
            // 
            this.MainTips.AutoPopDelay = 5000;
            this.MainTips.InitialDelay = 500;
            this.MainTips.IsBalloon = true;
            this.MainTips.ReshowDelay = 100;
            // 
            // ViewLocationToolStripButton
            // 
            this.ViewLocationToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ViewLocationToolStripButton.Image = global::DaggerfallModelling.Properties.Resources.world;
            this.ViewLocationToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ViewLocationToolStripButton.Name = "ViewLocationToolStripButton";
            this.ViewLocationToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.ViewLocationToolStripButton.Text = "View Location";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1184, 762);
            this.Controls.Add(this.MainStatusStrip);
            this.Controls.Add(this.MainSplitContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Daggerfall Modelling";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.MainSplitContainer.Panel1.ResumeLayout(false);
            this.MainSplitContainer.Panel1.PerformLayout();
            this.MainSplitContainer.Panel2.ResumeLayout(false);
            this.MainSplitContainer.Panel2.PerformLayout();
            this.MainSplitContainer.ResumeLayout(false);
            this.SearchResultsPanel.ResumeLayout(false);
            this.SearchPanel.ResumeLayout(false);
            this.SearchPanel.PerformLayout();
            this.LayoutPanel.ResumeLayout(false);
            this.LayoutPanel.PerformLayout();
            this.AutoMapToolStrip.ResumeLayout(false);
            this.AutoMapToolStrip.PerformLayout();
            this.SearchPaneToolStrip.ResumeLayout(false);
            this.SearchPaneToolStrip.PerformLayout();
            this.ViewPaneToolStrip.ResumeLayout(false);
            this.ViewPaneToolStrip.PerformLayout();
            this.ContentPanel.ResumeLayout(false);
            this.MainStatusStrip.ResumeLayout(false);
            this.MainStatusStrip.PerformLayout();
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
        private System.Windows.Forms.ToolStripStatusLabel Arena2PathStatusLabel;
        private System.Windows.Forms.ToolStrip ViewPaneToolStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripButton AboutToolStripButton;
        private System.Windows.Forms.Panel ContentPanel;
        private System.Windows.Forms.Panel SearchPanel;
        private System.Windows.Forms.Label SearchLabel;
        private System.Windows.Forms.TextBox SearchTextBox;
        private System.Windows.Forms.Button ClearSearchButton;
        private System.Windows.Forms.Panel SearchResultsPanel;
        private System.Windows.Forms.ToolStripButton SearchModelsToolStripButton;
        private System.Windows.Forms.ToolStripButton SearchBlocksToolStripButton;
        private System.Windows.Forms.ToolStripButton SearchLocationsToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolTip MainTips;
        private System.Windows.Forms.ImageList SearchResultsImageList;
        private System.Windows.Forms.ToolStrip AutoMapToolStrip;
        private System.Windows.Forms.ToolStripButton ExteriorModeToolStripButton;
        private System.Windows.Forms.ToolStripButton DungeonModeToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripLabel BlockNameToolStripLabel;
        private DaggerfallModelling.ViewControls.AutoMapView AutoMapViewer;
        private System.Windows.Forms.ToolStripStatusLabel ModelViewStatusLabel;
        private System.Windows.Forms.ToolStripButton ViewThumbsToolStripButton;
        private System.Windows.Forms.ToolStripButton ViewBlockToolStripButton;
        private System.Windows.Forms.ToolStripButton ViewSingleModelToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private DaggerfallModelling.ViewControls.ContentViewHost ContentView;
        private System.Windows.Forms.ToolStripButton ViewLocationToolStripButton;
    }
}

