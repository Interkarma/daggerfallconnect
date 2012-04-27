namespace ProjectEditor
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
            this.ProjectPanel = new System.Windows.Forms.Panel();
            this.ProjectListView = new System.Windows.Forms.ListView();
            this.ProjectToolStrip = new System.Windows.Forms.ToolStrip();
            this.NewProjectButton = new System.Windows.Forms.ToolStripButton();
            this.OpenProjectButton = new System.Windows.Forms.ToolStripButton();
            this.SaveProjectButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.SceneImageList = new System.Windows.Forms.ImageList(this.components);
            this.SceneTreeView = new System.Windows.Forms.TreeView();
            this.SceneContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddEntityMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.AddDaggerfallMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddDaggerfallModelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddDaggerfallBlockMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddPrimitiveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddCubeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddSphereMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddLightMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddQuadTerrainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.DeleteSceneObjectMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ScenePropertiesPanel = new System.Windows.Forms.Panel();
            this.WorldPanel = new System.Windows.Forms.Panel();
            this.MainStatusStrip = new System.Windows.Forms.StatusStrip();
            this.TerrainEditorPanel = new System.Windows.Forms.Panel();
            this.terrainEditor1 = new ProjectEditor.UserControls.TerrainEditor();
            this.worldControl = new ProjectEditor.Controls.WorldControl();
            this.WorldToolStrip = new System.Windows.Forms.ToolStrip();
            this.UndoButton = new System.Windows.Forms.ToolStripButton();
            this.RedoButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.PlayButton = new System.Windows.Forms.ToolStripButton();
            this.RestartPlayButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.ScenePanel = new System.Windows.Forms.Panel();
            this.SceneToolStrip = new System.Windows.Forms.ToolStrip();
            this.ProjectPanel.SuspendLayout();
            this.ProjectToolStrip.SuspendLayout();
            this.SceneContextMenuStrip.SuspendLayout();
            this.WorldPanel.SuspendLayout();
            this.TerrainEditorPanel.SuspendLayout();
            this.WorldToolStrip.SuspendLayout();
            this.ScenePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ProjectPanel
            // 
            this.ProjectPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ProjectPanel.Controls.Add(this.ProjectListView);
            this.ProjectPanel.Controls.Add(this.ProjectToolStrip);
            this.ProjectPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.ProjectPanel.Location = new System.Drawing.Point(0, 0);
            this.ProjectPanel.Name = "ProjectPanel";
            this.ProjectPanel.Size = new System.Drawing.Size(320, 862);
            this.ProjectPanel.TabIndex = 2;
            // 
            // ProjectListView
            // 
            this.ProjectListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ProjectListView.Location = new System.Drawing.Point(0, 26);
            this.ProjectListView.Name = "ProjectListView";
            this.ProjectListView.Size = new System.Drawing.Size(318, 834);
            this.ProjectListView.TabIndex = 4;
            this.ProjectListView.UseCompatibleStateImageBehavior = false;
            // 
            // ProjectToolStrip
            // 
            this.ProjectToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewProjectButton,
            this.OpenProjectButton,
            this.SaveProjectButton,
            this.toolStripSeparator1});
            this.ProjectToolStrip.Location = new System.Drawing.Point(0, 0);
            this.ProjectToolStrip.Name = "ProjectToolStrip";
            this.ProjectToolStrip.Size = new System.Drawing.Size(318, 25);
            this.ProjectToolStrip.TabIndex = 3;
            this.ProjectToolStrip.Text = "toolStrip1";
            // 
            // NewProjectButton
            // 
            this.NewProjectButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.NewProjectButton.Image = global::ProjectEditor.Properties.Resources.database_add;
            this.NewProjectButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.NewProjectButton.Name = "NewProjectButton";
            this.NewProjectButton.Size = new System.Drawing.Size(23, 22);
            this.NewProjectButton.Text = "New Project";
            this.NewProjectButton.Click += new System.EventHandler(this.NewProjectButton_Click);
            // 
            // OpenProjectButton
            // 
            this.OpenProjectButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.OpenProjectButton.Image = global::ProjectEditor.Properties.Resources.folder_page;
            this.OpenProjectButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.OpenProjectButton.Name = "OpenProjectButton";
            this.OpenProjectButton.Size = new System.Drawing.Size(23, 22);
            this.OpenProjectButton.Text = "Open Project";
            // 
            // SaveProjectButton
            // 
            this.SaveProjectButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SaveProjectButton.Enabled = false;
            this.SaveProjectButton.Image = global::ProjectEditor.Properties.Resources.disk;
            this.SaveProjectButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SaveProjectButton.Name = "SaveProjectButton";
            this.SaveProjectButton.Size = new System.Drawing.Size(23, 22);
            this.SaveProjectButton.Text = "Save Project";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // SceneImageList
            // 
            this.SceneImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("SceneImageList.ImageStream")));
            this.SceneImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.SceneImageList.Images.SetKeyName(0, "Unknown");
            this.SceneImageList.Images.SetKeyName(1, "Scene");
            this.SceneImageList.Images.SetKeyName(2, "Environment");
            this.SceneImageList.Images.SetKeyName(3, "Entity");
            this.SceneImageList.Images.SetKeyName(4, "ScreenScene");
            this.SceneImageList.Images.SetKeyName(5, "Light");
            this.SceneImageList.Images.SetKeyName(6, "Geometry");
            this.SceneImageList.Images.SetKeyName(7, "PhysicsCollider");
            this.SceneImageList.Images.SetKeyName(8, "Panel");
            this.SceneImageList.Images.SetKeyName(9, "StackPanel");
            this.SceneImageList.Images.SetKeyName(10, "Text");
            this.SceneImageList.Images.SetKeyName(11, "Script");
            this.SceneImageList.Images.SetKeyName(12, "Prefab");
            this.SceneImageList.Images.SetKeyName(13, "Folder");
            this.SceneImageList.Images.SetKeyName(14, "QuadTerrain");
            // 
            // SceneTreeView
            // 
            this.SceneTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SceneTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.SceneTreeView.ContextMenuStrip = this.SceneContextMenuStrip;
            this.SceneTreeView.HideSelection = false;
            this.SceneTreeView.ImageIndex = 0;
            this.SceneTreeView.ImageList = this.SceneImageList;
            this.SceneTreeView.Location = new System.Drawing.Point(0, 25);
            this.SceneTreeView.Margin = new System.Windows.Forms.Padding(0);
            this.SceneTreeView.Name = "SceneTreeView";
            this.SceneTreeView.SelectedImageIndex = 0;
            this.SceneTreeView.Size = new System.Drawing.Size(317, 477);
            this.SceneTreeView.TabIndex = 4;
            this.SceneTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.SceneTreeView_AfterSelect);
            this.SceneTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.DocumentTreeView_NodeMouseClick);
            // 
            // SceneContextMenuStrip
            // 
            this.SceneContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.toolStripSeparator5,
            this.DeleteSceneObjectMenu});
            this.SceneContextMenuStrip.Name = "SceneContextMenuStrip";
            this.SceneContextMenuStrip.Size = new System.Drawing.Size(108, 54);
            this.SceneContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.SceneContextMenuStrip_Opening);
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddEntityMenuItem,
            this.toolStripSeparator2,
            this.AddDaggerfallMenuItem,
            this.AddPrimitiveMenuItem,
            this.AddLightMenuItem,
            this.AddQuadTerrainMenuItem});
            this.newToolStripMenuItem.Image = global::ProjectEditor.Properties.Resources.page_white;
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.newToolStripMenuItem.Text = "New";
            // 
            // AddEntityMenuItem
            // 
            this.AddEntityMenuItem.Image = global::ProjectEditor.Properties.Resources.link;
            this.AddEntityMenuItem.Name = "AddEntityMenuItem";
            this.AddEntityMenuItem.Size = new System.Drawing.Size(140, 22);
            this.AddEntityMenuItem.Text = "Entity";
            this.AddEntityMenuItem.Click += new System.EventHandler(this.AddEntityMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(137, 6);
            // 
            // AddDaggerfallMenuItem
            // 
            this.AddDaggerfallMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddDaggerfallModelMenuItem,
            this.AddDaggerfallBlockMenuItem});
            this.AddDaggerfallMenuItem.Name = "AddDaggerfallMenuItem";
            this.AddDaggerfallMenuItem.Size = new System.Drawing.Size(140, 22);
            this.AddDaggerfallMenuItem.Text = "Daggerfall";
            // 
            // AddDaggerfallModelMenuItem
            // 
            this.AddDaggerfallModelMenuItem.Name = "AddDaggerfallModelMenuItem";
            this.AddDaggerfallModelMenuItem.Size = new System.Drawing.Size(108, 22);
            this.AddDaggerfallModelMenuItem.Text = "Model";
            this.AddDaggerfallModelMenuItem.Click += new System.EventHandler(this.AddDaggerfallModelMenuItem_Click);
            // 
            // AddDaggerfallBlockMenuItem
            // 
            this.AddDaggerfallBlockMenuItem.Name = "AddDaggerfallBlockMenuItem";
            this.AddDaggerfallBlockMenuItem.Size = new System.Drawing.Size(108, 22);
            this.AddDaggerfallBlockMenuItem.Text = "Block";
            this.AddDaggerfallBlockMenuItem.Click += new System.EventHandler(this.AddDaggerfallBlockMenuItem_Click);
            // 
            // AddPrimitiveMenuItem
            // 
            this.AddPrimitiveMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddCubeMenuItem,
            this.AddSphereMenuItem});
            this.AddPrimitiveMenuItem.Image = global::ProjectEditor.Properties.Resources.brick;
            this.AddPrimitiveMenuItem.Name = "AddPrimitiveMenuItem";
            this.AddPrimitiveMenuItem.Size = new System.Drawing.Size(140, 22);
            this.AddPrimitiveMenuItem.Text = "Primitive";
            // 
            // AddCubeMenuItem
            // 
            this.AddCubeMenuItem.Image = global::ProjectEditor.Properties.Resources.vector;
            this.AddCubeMenuItem.Name = "AddCubeMenuItem";
            this.AddCubeMenuItem.Size = new System.Drawing.Size(110, 22);
            this.AddCubeMenuItem.Text = "Cube";
            // 
            // AddSphereMenuItem
            // 
            this.AddSphereMenuItem.Image = global::ProjectEditor.Properties.Resources.vector;
            this.AddSphereMenuItem.Name = "AddSphereMenuItem";
            this.AddSphereMenuItem.Size = new System.Drawing.Size(110, 22);
            this.AddSphereMenuItem.Text = "Sphere";
            // 
            // AddLightMenuItem
            // 
            this.AddLightMenuItem.Image = global::ProjectEditor.Properties.Resources.lightbulb;
            this.AddLightMenuItem.Name = "AddLightMenuItem";
            this.AddLightMenuItem.Size = new System.Drawing.Size(140, 22);
            this.AddLightMenuItem.Text = "Light";
            this.AddLightMenuItem.Click += new System.EventHandler(this.AddLightMenuItem_Click);
            // 
            // AddQuadTerrainMenuItem
            // 
            this.AddQuadTerrainMenuItem.Image = global::ProjectEditor.Properties.Resources.color_swatch;
            this.AddQuadTerrainMenuItem.Name = "AddQuadTerrainMenuItem";
            this.AddQuadTerrainMenuItem.Size = new System.Drawing.Size(140, 22);
            this.AddQuadTerrainMenuItem.Text = "QuadTerrain";
            this.AddQuadTerrainMenuItem.Click += new System.EventHandler(this.AddQuadTerrainMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(104, 6);
            // 
            // DeleteSceneObjectMenu
            // 
            this.DeleteSceneObjectMenu.Image = global::ProjectEditor.Properties.Resources.cross;
            this.DeleteSceneObjectMenu.Name = "DeleteSceneObjectMenu";
            this.DeleteSceneObjectMenu.Size = new System.Drawing.Size(107, 22);
            this.DeleteSceneObjectMenu.Text = "Delete";
            this.DeleteSceneObjectMenu.Click += new System.EventHandler(this.DeleteSceneObjectMenu_Click);
            // 
            // ScenePropertiesPanel
            // 
            this.ScenePropertiesPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ScenePropertiesPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ScenePropertiesPanel.Location = new System.Drawing.Point(0, 505);
            this.ScenePropertiesPanel.Name = "ScenePropertiesPanel";
            this.ScenePropertiesPanel.Size = new System.Drawing.Size(317, 354);
            this.ScenePropertiesPanel.TabIndex = 2;
            // 
            // WorldPanel
            // 
            this.WorldPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.WorldPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.WorldPanel.Controls.Add(this.MainStatusStrip);
            this.WorldPanel.Controls.Add(this.TerrainEditorPanel);
            this.WorldPanel.Controls.Add(this.worldControl);
            this.WorldPanel.Controls.Add(this.WorldToolStrip);
            this.WorldPanel.Controls.Add(this.ScenePanel);
            this.WorldPanel.Location = new System.Drawing.Point(323, 0);
            this.WorldPanel.Name = "WorldPanel";
            this.WorldPanel.Size = new System.Drawing.Size(1101, 862);
            this.WorldPanel.TabIndex = 3;
            // 
            // MainStatusStrip
            // 
            this.MainStatusStrip.Location = new System.Drawing.Point(0, 838);
            this.MainStatusStrip.Name = "MainStatusStrip";
            this.MainStatusStrip.Size = new System.Drawing.Size(780, 22);
            this.MainStatusStrip.SizingGrip = false;
            this.MainStatusStrip.TabIndex = 2;
            this.MainStatusStrip.Text = "statusStrip1";
            // 
            // TerrainEditorPanel
            // 
            this.TerrainEditorPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TerrainEditorPanel.Controls.Add(this.terrainEditor1);
            this.TerrainEditorPanel.Location = new System.Drawing.Point(0, 25);
            this.TerrainEditorPanel.Name = "TerrainEditorPanel";
            this.TerrainEditorPanel.Size = new System.Drawing.Size(270, 606);
            this.TerrainEditorPanel.TabIndex = 3;
            // 
            // terrainEditor1
            // 
            this.terrainEditor1.Location = new System.Drawing.Point(3, 3);
            this.terrainEditor1.Name = "terrainEditor1";
            this.terrainEditor1.Size = new System.Drawing.Size(260, 601);
            this.terrainEditor1.TabIndex = 0;
            this.terrainEditor1.OnHeightMapChanged += new System.EventHandler(this.TerrainEditor_OnHeightMapChanged);
            this.terrainEditor1.OnBlendMapChanged += new System.EventHandler(this.TerrainEditor_OnBlendMapChanged);
            // 
            // worldControl
            // 
            this.worldControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.worldControl.Arena2Path = "";
            this.worldControl.Location = new System.Drawing.Point(0, 25);
            this.worldControl.MinimumSize = new System.Drawing.Size(320, 200);
            this.worldControl.Name = "worldControl";
            this.worldControl.Size = new System.Drawing.Size(776, 813);
            this.worldControl.TabIndex = 0;
            this.worldControl.Text = "DeepEngine - WorldControl";
            this.worldControl.InitializeCompleted += new System.EventHandler(this.WorldControl_InitializeCompleted);
            this.worldControl.OnTick += new System.EventHandler(this.WorldControl_OnTick);
            this.worldControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WorldControl_MouseDown);
            this.worldControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.WorldControl_MouseMove);
            this.worldControl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WorldControl_MouseUp);
            // 
            // WorldToolStrip
            // 
            this.WorldToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.UndoButton,
            this.RedoButton,
            this.toolStripSeparator3,
            this.PlayButton,
            this.RestartPlayButton,
            this.toolStripSeparator4});
            this.WorldToolStrip.Location = new System.Drawing.Point(0, 0);
            this.WorldToolStrip.Name = "WorldToolStrip";
            this.WorldToolStrip.Size = new System.Drawing.Size(780, 25);
            this.WorldToolStrip.TabIndex = 1;
            this.WorldToolStrip.Text = "toolStrip2";
            // 
            // UndoButton
            // 
            this.UndoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.UndoButton.Image = global::ProjectEditor.Properties.Resources.arrow_undo;
            this.UndoButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.UndoButton.Name = "UndoButton";
            this.UndoButton.Size = new System.Drawing.Size(23, 22);
            this.UndoButton.Text = "Undo";
            this.UndoButton.Click += new System.EventHandler(this.UndoButton_Click);
            // 
            // RedoButton
            // 
            this.RedoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RedoButton.Image = global::ProjectEditor.Properties.Resources.arrow_redo;
            this.RedoButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RedoButton.Name = "RedoButton";
            this.RedoButton.Size = new System.Drawing.Size(23, 22);
            this.RedoButton.Text = "Redo";
            this.RedoButton.Click += new System.EventHandler(this.RedoButton_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // PlayButton
            // 
            this.PlayButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PlayButton.Enabled = false;
            this.PlayButton.Image = global::ProjectEditor.Properties.Resources.control_play_blue;
            this.PlayButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PlayButton.Name = "PlayButton";
            this.PlayButton.Size = new System.Drawing.Size(23, 22);
            this.PlayButton.Text = "Resume Scene";
            // 
            // RestartPlayButton
            // 
            this.RestartPlayButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RestartPlayButton.Enabled = false;
            this.RestartPlayButton.Image = global::ProjectEditor.Properties.Resources.control_repeat_blue;
            this.RestartPlayButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RestartPlayButton.Name = "RestartPlayButton";
            this.RestartPlayButton.Size = new System.Drawing.Size(23, 22);
            this.RestartPlayButton.Text = "Restart Scene";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // ScenePanel
            // 
            this.ScenePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ScenePanel.Controls.Add(this.SceneToolStrip);
            this.ScenePanel.Controls.Add(this.SceneTreeView);
            this.ScenePanel.Controls.Add(this.ScenePropertiesPanel);
            this.ScenePanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.ScenePanel.Location = new System.Drawing.Point(780, 0);
            this.ScenePanel.Name = "ScenePanel";
            this.ScenePanel.Size = new System.Drawing.Size(319, 860);
            this.ScenePanel.TabIndex = 4;
            // 
            // SceneToolStrip
            // 
            this.SceneToolStrip.Location = new System.Drawing.Point(0, 0);
            this.SceneToolStrip.Name = "SceneToolStrip";
            this.SceneToolStrip.Size = new System.Drawing.Size(317, 25);
            this.SceneToolStrip.TabIndex = 5;
            this.SceneToolStrip.Text = "toolStrip1";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1424, 862);
            this.Controls.Add(this.WorldPanel);
            this.Controls.Add(this.ProjectPanel);
            this.MinimumSize = new System.Drawing.Size(1024, 512);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Deep Engine Project Editor";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ProjectPanel.ResumeLayout(false);
            this.ProjectPanel.PerformLayout();
            this.ProjectToolStrip.ResumeLayout(false);
            this.ProjectToolStrip.PerformLayout();
            this.SceneContextMenuStrip.ResumeLayout(false);
            this.WorldPanel.ResumeLayout(false);
            this.WorldPanel.PerformLayout();
            this.TerrainEditorPanel.ResumeLayout(false);
            this.WorldToolStrip.ResumeLayout(false);
            this.WorldToolStrip.PerformLayout();
            this.ScenePanel.ResumeLayout(false);
            this.ScenePanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel ProjectPanel;
        private System.Windows.Forms.Panel ScenePropertiesPanel;
        private System.Windows.Forms.Panel WorldPanel;
        private Controls.WorldControl worldControl;
        private System.Windows.Forms.ToolStrip ProjectToolStrip;
        private System.Windows.Forms.ToolStripButton OpenProjectButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.TreeView SceneTreeView;
        private System.Windows.Forms.ToolStrip WorldToolStrip;
        private System.Windows.Forms.ToolStripButton PlayButton;
        private System.Windows.Forms.ToolStripButton RestartPlayButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.StatusStrip MainStatusStrip;
        private System.Windows.Forms.ToolStripButton UndoButton;
        private System.Windows.Forms.ToolStripButton RedoButton;
        private System.Windows.Forms.ImageList SceneImageList;
        private System.Windows.Forms.ContextMenuStrip SceneContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AddEntityMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem DeleteSceneObjectMenu;
        private System.Windows.Forms.ToolStripMenuItem AddQuadTerrainMenuItem;
        private System.Windows.Forms.ToolStripButton NewProjectButton;
        private System.Windows.Forms.ToolStripMenuItem AddPrimitiveMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AddCubeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AddSphereMenuItem;
        private System.Windows.Forms.Panel TerrainEditorPanel;
        private UserControls.TerrainEditor terrainEditor1;
        private System.Windows.Forms.ToolStripMenuItem AddDaggerfallMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AddDaggerfallModelMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AddDaggerfallBlockMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AddLightMenuItem;
        private System.Windows.Forms.Panel ScenePanel;
        private System.Windows.Forms.ToolStrip SceneToolStrip;
        private System.Windows.Forms.ToolStripButton SaveProjectButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ListView ProjectListView;
    }
}

