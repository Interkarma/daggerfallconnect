namespace SceneEditor
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
            this.ToolboxPanel = new System.Windows.Forms.Panel();
            this.ResourceTabControl = new System.Windows.Forms.TabControl();
            this.SceneTabPage = new System.Windows.Forms.TabPage();
            this.DocumentTreeView = new System.Windows.Forms.TreeView();
            this.SceneContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddEntityMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.AddPrimitiveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddCubeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddSphereMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddQuadTerrainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.makePrefabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DocumentImageList = new System.Windows.Forms.ImageList(this.components);
            this.SceneToolStrip = new System.Windows.Forms.ToolStrip();
            this.NewSceneButton = new System.Windows.Forms.ToolStripButton();
            this.OpenSceneButton = new System.Windows.Forms.ToolStripButton();
            this.SaveSceneButton = new System.Windows.Forms.ToolStripSplitButton();
            this.SaveSceneMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveSceneAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.PropertiesPanel = new System.Windows.Forms.Panel();
            this.WorldPanel = new System.Windows.Forms.Panel();
            this.WorldToolStrip = new System.Windows.Forms.ToolStrip();
            this.PlayButton = new System.Windows.Forms.ToolStripButton();
            this.RestartPlayButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.AboutButton = new System.Windows.Forms.ToolStripButton();
            this.UndoButton = new System.Windows.Forms.ToolStripButton();
            this.RedoButton = new System.Windows.Forms.ToolStripButton();
            this.MainStatusStrip = new System.Windows.Forms.StatusStrip();
            this.ToggleTerrainEditorButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.TerrainEditorPanel = new System.Windows.Forms.Panel();
            this.terrainEditor1 = new SceneEditor.UserControls.TerrainEditor();
            this.worldControl = new SceneEditor.Controls.WorldControl();
            this.ToolboxPanel.SuspendLayout();
            this.ResourceTabControl.SuspendLayout();
            this.SceneTabPage.SuspendLayout();
            this.SceneContextMenuStrip.SuspendLayout();
            this.SceneToolStrip.SuspendLayout();
            this.WorldPanel.SuspendLayout();
            this.WorldToolStrip.SuspendLayout();
            this.TerrainEditorPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ToolboxPanel
            // 
            this.ToolboxPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ToolboxPanel.Controls.Add(this.ResourceTabControl);
            this.ToolboxPanel.Controls.Add(this.SceneToolStrip);
            this.ToolboxPanel.Controls.Add(this.PropertiesPanel);
            this.ToolboxPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.ToolboxPanel.Location = new System.Drawing.Point(0, 0);
            this.ToolboxPanel.Name = "ToolboxPanel";
            this.ToolboxPanel.Size = new System.Drawing.Size(320, 862);
            this.ToolboxPanel.TabIndex = 2;
            // 
            // ResourceTabControl
            // 
            this.ResourceTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ResourceTabControl.Appearance = System.Windows.Forms.TabAppearance.Buttons;
            this.ResourceTabControl.Controls.Add(this.SceneTabPage);
            this.ResourceTabControl.ImageList = this.DocumentImageList;
            this.ResourceTabControl.ItemSize = new System.Drawing.Size(80, 19);
            this.ResourceTabControl.Location = new System.Drawing.Point(0, 25);
            this.ResourceTabControl.Margin = new System.Windows.Forms.Padding(0);
            this.ResourceTabControl.Name = "ResourceTabControl";
            this.ResourceTabControl.SelectedIndex = 0;
            this.ResourceTabControl.Size = new System.Drawing.Size(319, 481);
            this.ResourceTabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.ResourceTabControl.TabIndex = 4;
            // 
            // SceneTabPage
            // 
            this.SceneTabPage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SceneTabPage.Controls.Add(this.DocumentTreeView);
            this.SceneTabPage.ImageKey = "Scene";
            this.SceneTabPage.Location = new System.Drawing.Point(4, 23);
            this.SceneTabPage.Margin = new System.Windows.Forms.Padding(0);
            this.SceneTabPage.Name = "SceneTabPage";
            this.SceneTabPage.Size = new System.Drawing.Size(311, 454);
            this.SceneTabPage.TabIndex = 0;
            this.SceneTabPage.Text = "Scene";
            this.SceneTabPage.UseVisualStyleBackColor = true;
            // 
            // DocumentTreeView
            // 
            this.DocumentTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DocumentTreeView.ContextMenuStrip = this.SceneContextMenuStrip;
            this.DocumentTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DocumentTreeView.HideSelection = false;
            this.DocumentTreeView.ImageIndex = 0;
            this.DocumentTreeView.ImageList = this.DocumentImageList;
            this.DocumentTreeView.Location = new System.Drawing.Point(0, 0);
            this.DocumentTreeView.Margin = new System.Windows.Forms.Padding(0);
            this.DocumentTreeView.Name = "DocumentTreeView";
            this.DocumentTreeView.SelectedImageIndex = 0;
            this.DocumentTreeView.Size = new System.Drawing.Size(309, 452);
            this.DocumentTreeView.TabIndex = 4;
            this.DocumentTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.SceneTreeView_AfterSelect);
            // 
            // SceneContextMenuStrip
            // 
            this.SceneContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.toolStripSeparator5,
            this.renameToolStripMenuItem,
            this.makePrefabToolStripMenuItem,
            this.toolStripSeparator6,
            this.deleteToolStripMenuItem});
            this.SceneContextMenuStrip.Name = "SceneContextMenuStrip";
            this.SceneContextMenuStrip.Size = new System.Drawing.Size(141, 104);
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddEntityMenuItem,
            this.toolStripSeparator2,
            this.AddPrimitiveMenuItem,
            this.AddQuadTerrainMenuItem});
            this.newToolStripMenuItem.Image = global::SceneEditor.Properties.Resources.page_white;
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.newToolStripMenuItem.Text = "New";
            // 
            // AddEntityMenuItem
            // 
            this.AddEntityMenuItem.Image = global::SceneEditor.Properties.Resources.link;
            this.AddEntityMenuItem.Name = "AddEntityMenuItem";
            this.AddEntityMenuItem.Size = new System.Drawing.Size(140, 22);
            this.AddEntityMenuItem.Text = "Entity";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(137, 6);
            // 
            // AddPrimitiveMenuItem
            // 
            this.AddPrimitiveMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddCubeMenuItem,
            this.AddSphereMenuItem});
            this.AddPrimitiveMenuItem.Image = global::SceneEditor.Properties.Resources.brick;
            this.AddPrimitiveMenuItem.Name = "AddPrimitiveMenuItem";
            this.AddPrimitiveMenuItem.Size = new System.Drawing.Size(140, 22);
            this.AddPrimitiveMenuItem.Text = "Primitive";
            // 
            // AddCubeMenuItem
            // 
            this.AddCubeMenuItem.Image = global::SceneEditor.Properties.Resources.vector;
            this.AddCubeMenuItem.Name = "AddCubeMenuItem";
            this.AddCubeMenuItem.Size = new System.Drawing.Size(110, 22);
            this.AddCubeMenuItem.Text = "Cube";
            // 
            // AddSphereMenuItem
            // 
            this.AddSphereMenuItem.Image = global::SceneEditor.Properties.Resources.vector;
            this.AddSphereMenuItem.Name = "AddSphereMenuItem";
            this.AddSphereMenuItem.Size = new System.Drawing.Size(110, 22);
            this.AddSphereMenuItem.Text = "Sphere";
            // 
            // AddQuadTerrainMenuItem
            // 
            this.AddQuadTerrainMenuItem.Image = global::SceneEditor.Properties.Resources.color_swatch;
            this.AddQuadTerrainMenuItem.Name = "AddQuadTerrainMenuItem";
            this.AddQuadTerrainMenuItem.Size = new System.Drawing.Size(140, 22);
            this.AddQuadTerrainMenuItem.Text = "QuadTerrain";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(137, 6);
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.Image = global::SceneEditor.Properties.Resources.textfield_rename;
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.renameToolStripMenuItem.Text = "Rename";
            // 
            // makePrefabToolStripMenuItem
            // 
            this.makePrefabToolStripMenuItem.Image = global::SceneEditor.Properties.Resources.cog;
            this.makePrefabToolStripMenuItem.Name = "makePrefabToolStripMenuItem";
            this.makePrefabToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.makePrefabToolStripMenuItem.Text = "Make Prefab";
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(137, 6);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Image = global::SceneEditor.Properties.Resources.cross;
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            // 
            // DocumentImageList
            // 
            this.DocumentImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("DocumentImageList.ImageStream")));
            this.DocumentImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.DocumentImageList.Images.SetKeyName(0, "Unknown");
            this.DocumentImageList.Images.SetKeyName(1, "Scene");
            this.DocumentImageList.Images.SetKeyName(2, "Environment");
            this.DocumentImageList.Images.SetKeyName(3, "Entity");
            this.DocumentImageList.Images.SetKeyName(4, "ScreenScene");
            this.DocumentImageList.Images.SetKeyName(5, "Light");
            this.DocumentImageList.Images.SetKeyName(6, "Geometry");
            this.DocumentImageList.Images.SetKeyName(7, "PhysicsCollider");
            this.DocumentImageList.Images.SetKeyName(8, "Panel");
            this.DocumentImageList.Images.SetKeyName(9, "StackPanel");
            this.DocumentImageList.Images.SetKeyName(10, "Text");
            this.DocumentImageList.Images.SetKeyName(11, "Script");
            this.DocumentImageList.Images.SetKeyName(12, "Prefab");
            this.DocumentImageList.Images.SetKeyName(13, "Folder");
            this.DocumentImageList.Images.SetKeyName(14, "QuadTerrain");
            // 
            // SceneToolStrip
            // 
            this.SceneToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewSceneButton,
            this.OpenSceneButton,
            this.SaveSceneButton,
            this.toolStripSeparator1});
            this.SceneToolStrip.Location = new System.Drawing.Point(0, 0);
            this.SceneToolStrip.Name = "SceneToolStrip";
            this.SceneToolStrip.Size = new System.Drawing.Size(318, 25);
            this.SceneToolStrip.TabIndex = 3;
            this.SceneToolStrip.Text = "toolStrip1";
            // 
            // NewSceneButton
            // 
            this.NewSceneButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.NewSceneButton.Image = global::SceneEditor.Properties.Resources.page_white;
            this.NewSceneButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.NewSceneButton.Name = "NewSceneButton";
            this.NewSceneButton.Size = new System.Drawing.Size(23, 22);
            this.NewSceneButton.Text = "New Scene";
            // 
            // OpenSceneButton
            // 
            this.OpenSceneButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.OpenSceneButton.Image = global::SceneEditor.Properties.Resources.folder_page;
            this.OpenSceneButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.OpenSceneButton.Name = "OpenSceneButton";
            this.OpenSceneButton.Size = new System.Drawing.Size(23, 22);
            this.OpenSceneButton.Text = "Open Scene";
            // 
            // SaveSceneButton
            // 
            this.SaveSceneButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SaveSceneButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SaveSceneMenuItem,
            this.SaveSceneAsMenuItem});
            this.SaveSceneButton.Image = global::SceneEditor.Properties.Resources.disk;
            this.SaveSceneButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SaveSceneButton.Name = "SaveSceneButton";
            this.SaveSceneButton.Size = new System.Drawing.Size(32, 22);
            this.SaveSceneButton.Text = "Save Scene";
            // 
            // SaveSceneMenuItem
            // 
            this.SaveSceneMenuItem.Image = global::SceneEditor.Properties.Resources.disk;
            this.SaveSceneMenuItem.Name = "SaveSceneMenuItem";
            this.SaveSceneMenuItem.Size = new System.Drawing.Size(123, 22);
            this.SaveSceneMenuItem.Text = "Save";
            // 
            // SaveSceneAsMenuItem
            // 
            this.SaveSceneAsMenuItem.Image = global::SceneEditor.Properties.Resources.drive_disk;
            this.SaveSceneAsMenuItem.Name = "SaveSceneAsMenuItem";
            this.SaveSceneAsMenuItem.Size = new System.Drawing.Size(123, 22);
            this.SaveSceneAsMenuItem.Text = "Save As...";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // PropertiesPanel
            // 
            this.PropertiesPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PropertiesPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PropertiesPanel.Location = new System.Drawing.Point(-1, 508);
            this.PropertiesPanel.Name = "PropertiesPanel";
            this.PropertiesPanel.Size = new System.Drawing.Size(320, 353);
            this.PropertiesPanel.TabIndex = 2;
            // 
            // WorldPanel
            // 
            this.WorldPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.WorldPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.WorldPanel.Controls.Add(this.TerrainEditorPanel);
            this.WorldPanel.Controls.Add(this.worldControl);
            this.WorldPanel.Controls.Add(this.WorldToolStrip);
            this.WorldPanel.Controls.Add(this.MainStatusStrip);
            this.WorldPanel.Location = new System.Drawing.Point(323, 0);
            this.WorldPanel.Name = "WorldPanel";
            this.WorldPanel.Size = new System.Drawing.Size(1101, 862);
            this.WorldPanel.TabIndex = 3;
            // 
            // WorldToolStrip
            // 
            this.WorldToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToggleTerrainEditorButton,
            this.toolStripSeparator4,
            this.PlayButton,
            this.RestartPlayButton,
            this.toolStripSeparator3,
            this.AboutButton,
            this.UndoButton,
            this.RedoButton});
            this.WorldToolStrip.Location = new System.Drawing.Point(0, 0);
            this.WorldToolStrip.Name = "WorldToolStrip";
            this.WorldToolStrip.Size = new System.Drawing.Size(1099, 25);
            this.WorldToolStrip.TabIndex = 1;
            this.WorldToolStrip.Text = "toolStrip2";
            // 
            // PlayButton
            // 
            this.PlayButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PlayButton.Image = global::SceneEditor.Properties.Resources.control_play_blue;
            this.PlayButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PlayButton.Name = "PlayButton";
            this.PlayButton.Size = new System.Drawing.Size(23, 22);
            this.PlayButton.Text = "Play Scene";
            // 
            // RestartPlayButton
            // 
            this.RestartPlayButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RestartPlayButton.Image = global::SceneEditor.Properties.Resources.control_repeat_blue;
            this.RestartPlayButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RestartPlayButton.Name = "RestartPlayButton";
            this.RestartPlayButton.Size = new System.Drawing.Size(23, 22);
            this.RestartPlayButton.Text = "Replay Scene";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // AboutButton
            // 
            this.AboutButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.AboutButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.AboutButton.Image = global::SceneEditor.Properties.Resources.help;
            this.AboutButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.AboutButton.Name = "AboutButton";
            this.AboutButton.Size = new System.Drawing.Size(23, 22);
            this.AboutButton.Text = "About Deep Engine";
            // 
            // UndoButton
            // 
            this.UndoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.UndoButton.Image = global::SceneEditor.Properties.Resources.arrow_undo;
            this.UndoButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.UndoButton.Name = "UndoButton";
            this.UndoButton.Size = new System.Drawing.Size(23, 22);
            this.UndoButton.Text = "Undo";
            this.UndoButton.Click += new System.EventHandler(this.UndoButton_Click);
            // 
            // RedoButton
            // 
            this.RedoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RedoButton.Image = global::SceneEditor.Properties.Resources.arrow_redo;
            this.RedoButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RedoButton.Name = "RedoButton";
            this.RedoButton.Size = new System.Drawing.Size(23, 22);
            this.RedoButton.Text = "Redo";
            this.RedoButton.Click += new System.EventHandler(this.RedoButton_Click);
            // 
            // MainStatusStrip
            // 
            this.MainStatusStrip.Location = new System.Drawing.Point(0, 838);
            this.MainStatusStrip.Name = "MainStatusStrip";
            this.MainStatusStrip.Size = new System.Drawing.Size(1099, 22);
            this.MainStatusStrip.TabIndex = 2;
            this.MainStatusStrip.Text = "statusStrip1";
            // 
            // ToggleTerrainEditorButton
            // 
            this.ToggleTerrainEditorButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToggleTerrainEditorButton.Image = global::SceneEditor.Properties.Resources.color_swatch;
            this.ToggleTerrainEditorButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToggleTerrainEditorButton.Name = "ToggleTerrainEditorButton";
            this.ToggleTerrainEditorButton.Size = new System.Drawing.Size(23, 22);
            this.ToggleTerrainEditorButton.Text = "Toggle Terrain Editor";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // TerrainEditorPanel
            // 
            this.TerrainEditorPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TerrainEditorPanel.Controls.Add(this.terrainEditor1);
            this.TerrainEditorPanel.Location = new System.Drawing.Point(0, 25);
            this.TerrainEditorPanel.Name = "TerrainEditorPanel";
            this.TerrainEditorPanel.Size = new System.Drawing.Size(270, 329);
            this.TerrainEditorPanel.TabIndex = 3;
            // 
            // terrainEditor1
            // 
            this.terrainEditor1.Location = new System.Drawing.Point(3, 3);
            this.terrainEditor1.Name = "terrainEditor1";
            this.terrainEditor1.Size = new System.Drawing.Size(260, 322);
            this.terrainEditor1.TabIndex = 0;
            // 
            // worldControl
            // 
            this.worldControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.worldControl.Location = new System.Drawing.Point(0, 25);
            this.worldControl.MinimumSize = new System.Drawing.Size(320, 200);
            this.worldControl.Name = "worldControl";
            this.worldControl.Size = new System.Drawing.Size(1099, 813);
            this.worldControl.TabIndex = 0;
            this.worldControl.Text = "DeepEngine - WorldControl";
            this.worldControl.InitializeCompleted += new System.EventHandler(this.WorldControl_InitializeCompleted);
            this.worldControl.OnTick += new System.EventHandler(this.WorldControl_OnTick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1424, 862);
            this.Controls.Add(this.WorldPanel);
            this.Controls.Add(this.ToolboxPanel);
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Deep Engine Scene Editor";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ToolboxPanel.ResumeLayout(false);
            this.ToolboxPanel.PerformLayout();
            this.ResourceTabControl.ResumeLayout(false);
            this.SceneTabPage.ResumeLayout(false);
            this.SceneContextMenuStrip.ResumeLayout(false);
            this.SceneToolStrip.ResumeLayout(false);
            this.SceneToolStrip.PerformLayout();
            this.WorldPanel.ResumeLayout(false);
            this.WorldPanel.PerformLayout();
            this.WorldToolStrip.ResumeLayout(false);
            this.WorldToolStrip.PerformLayout();
            this.TerrainEditorPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel ToolboxPanel;
        private System.Windows.Forms.Panel PropertiesPanel;
        private System.Windows.Forms.Panel WorldPanel;
        private Controls.WorldControl worldControl;
        private System.Windows.Forms.ToolStrip SceneToolStrip;
        private System.Windows.Forms.ToolStripButton OpenSceneButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.TreeView DocumentTreeView;
        private System.Windows.Forms.ToolStrip WorldToolStrip;
        private System.Windows.Forms.ToolStripButton PlayButton;
        private System.Windows.Forms.ToolStripButton RestartPlayButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.StatusStrip MainStatusStrip;
        private System.Windows.Forms.ToolStripButton AboutButton;
        private System.Windows.Forms.ToolStripButton UndoButton;
        private System.Windows.Forms.ToolStripButton RedoButton;
        private System.Windows.Forms.ImageList DocumentImageList;
        private System.Windows.Forms.TabControl ResourceTabControl;
        private System.Windows.Forms.TabPage SceneTabPage;
        private System.Windows.Forms.ContextMenuStrip SceneContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AddEntityMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem makePrefabToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AddQuadTerrainMenuItem;
        private System.Windows.Forms.ToolStripSplitButton SaveSceneButton;
        private System.Windows.Forms.ToolStripMenuItem SaveSceneMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SaveSceneAsMenuItem;
        private System.Windows.Forms.ToolStripButton NewSceneButton;
        private System.Windows.Forms.ToolStripMenuItem AddPrimitiveMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AddCubeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AddSphereMenuItem;
        private System.Windows.Forms.ToolStripButton ToggleTerrainEditorButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.Panel TerrainEditorPanel;
        private UserControls.TerrainEditor terrainEditor1;
    }
}

