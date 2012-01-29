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
            this.sceneTreeView = new System.Windows.Forms.TreeView();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.NewSceneButton = new System.Windows.Forms.ToolStripButton();
            this.OpenSceneButton = new System.Windows.Forms.ToolStripButton();
            this.SaveSceneButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.AddEntityDropDown = new System.Windows.Forms.ToolStripDropDownButton();
            this.AddWorldEntityButton = new System.Windows.Forms.ToolStripMenuItem();
            this.AddComponentDropDown = new System.Windows.Forms.ToolStripDropDownButton();
            this.AddGeometricPrimitiveComponentButton = new System.Windows.Forms.ToolStripMenuItem();
            this.AddLightComponentButton = new System.Windows.Forms.ToolStripMenuItem();
            this.AddPhysicsColliderComponentButton = new System.Windows.Forms.ToolStripMenuItem();
            this.AddScreenComponentDropDown = new System.Windows.Forms.ToolStripDropDownButton();
            this.AddTextScreenComponentButton = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolsDropDown = new System.Windows.Forms.ToolStripDropDownButton();
            this.PropertiesPanel = new System.Windows.Forms.Panel();
            this.WorldPanel = new System.Windows.Forms.Panel();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.PlayButton = new System.Windows.Forms.ToolStripButton();
            this.StopPlayButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.RestartPlayButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.AboutButton = new System.Windows.Forms.ToolStripButton();
            this.UndoButton = new System.Windows.Forms.ToolStripButton();
            this.RedoButton = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.worldControl = new SceneEditor.Controls.WorldControl();
            this.SceneImageList = new System.Windows.Forms.ImageList(this.components);
            this.ToolboxPanel.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.WorldPanel.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // ToolboxPanel
            // 
            this.ToolboxPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ToolboxPanel.Controls.Add(this.sceneTreeView);
            this.ToolboxPanel.Controls.Add(this.toolStrip1);
            this.ToolboxPanel.Controls.Add(this.PropertiesPanel);
            this.ToolboxPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.ToolboxPanel.Location = new System.Drawing.Point(0, 0);
            this.ToolboxPanel.Name = "ToolboxPanel";
            this.ToolboxPanel.Size = new System.Drawing.Size(320, 862);
            this.ToolboxPanel.TabIndex = 2;
            // 
            // sceneTreeView
            // 
            this.sceneTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sceneTreeView.ImageIndex = 0;
            this.sceneTreeView.ImageList = this.SceneImageList;
            this.sceneTreeView.Location = new System.Drawing.Point(0, 26);
            this.sceneTreeView.Name = "sceneTreeView";
            this.sceneTreeView.SelectedImageIndex = 0;
            this.sceneTreeView.Size = new System.Drawing.Size(318, 481);
            this.sceneTreeView.TabIndex = 4;
            this.sceneTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.SceneTreeView_AfterSelect);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewSceneButton,
            this.OpenSceneButton,
            this.SaveSceneButton,
            this.toolStripSeparator1,
            this.AddEntityDropDown,
            this.AddComponentDropDown,
            this.AddScreenComponentDropDown,
            this.toolStripSeparator2,
            this.ToolsDropDown});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(318, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
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
            this.SaveSceneButton.Image = global::SceneEditor.Properties.Resources.disk;
            this.SaveSceneButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SaveSceneButton.Name = "SaveSceneButton";
            this.SaveSceneButton.Size = new System.Drawing.Size(23, 22);
            this.SaveSceneButton.Text = "Save Scene";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // AddEntityDropDown
            // 
            this.AddEntityDropDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.AddEntityDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddWorldEntityButton});
            this.AddEntityDropDown.Image = global::SceneEditor.Properties.Resources.database_add;
            this.AddEntityDropDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.AddEntityDropDown.Name = "AddEntityDropDown";
            this.AddEntityDropDown.Size = new System.Drawing.Size(29, 22);
            this.AddEntityDropDown.Text = "Add Entity";
            // 
            // AddWorldEntityButton
            // 
            this.AddWorldEntityButton.Image = global::SceneEditor.Properties.Resources.database;
            this.AddWorldEntityButton.Name = "AddWorldEntityButton";
            this.AddWorldEntityButton.Size = new System.Drawing.Size(139, 22);
            this.AddWorldEntityButton.Text = "World Entity";
            // 
            // AddComponentDropDown
            // 
            this.AddComponentDropDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.AddComponentDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddGeometricPrimitiveComponentButton,
            this.AddLightComponentButton,
            this.AddPhysicsColliderComponentButton});
            this.AddComponentDropDown.Image = global::SceneEditor.Properties.Resources.cog_add;
            this.AddComponentDropDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.AddComponentDropDown.Name = "AddComponentDropDown";
            this.AddComponentDropDown.Size = new System.Drawing.Size(29, 22);
            this.AddComponentDropDown.Text = "Add Component";
            // 
            // AddGeometricPrimitiveComponentButton
            // 
            this.AddGeometricPrimitiveComponentButton.Image = global::SceneEditor.Properties.Resources.bricks;
            this.AddGeometricPrimitiveComponentButton.Name = "AddGeometricPrimitiveComponentButton";
            this.AddGeometricPrimitiveComponentButton.Size = new System.Drawing.Size(179, 22);
            this.AddGeometricPrimitiveComponentButton.Text = "Geometric Primitive";
            // 
            // AddLightComponentButton
            // 
            this.AddLightComponentButton.Image = global::SceneEditor.Properties.Resources.lightbulb;
            this.AddLightComponentButton.Name = "AddLightComponentButton";
            this.AddLightComponentButton.Size = new System.Drawing.Size(179, 22);
            this.AddLightComponentButton.Text = "Light";
            // 
            // AddPhysicsColliderComponentButton
            // 
            this.AddPhysicsColliderComponentButton.Image = global::SceneEditor.Properties.Resources.cog;
            this.AddPhysicsColliderComponentButton.Name = "AddPhysicsColliderComponentButton";
            this.AddPhysicsColliderComponentButton.Size = new System.Drawing.Size(179, 22);
            this.AddPhysicsColliderComponentButton.Text = "Physics Collider";
            // 
            // AddScreenComponentDropDown
            // 
            this.AddScreenComponentDropDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.AddScreenComponentDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddTextScreenComponentButton});
            this.AddScreenComponentDropDown.Image = global::SceneEditor.Properties.Resources.monitor_add;
            this.AddScreenComponentDropDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.AddScreenComponentDropDown.Name = "AddScreenComponentDropDown";
            this.AddScreenComponentDropDown.Size = new System.Drawing.Size(29, 22);
            this.AddScreenComponentDropDown.Text = "Add Screen Component";
            // 
            // AddTextScreenComponentButton
            // 
            this.AddTextScreenComponentButton.Image = global::SceneEditor.Properties.Resources.text_allcaps;
            this.AddTextScreenComponentButton.Name = "AddTextScreenComponentButton";
            this.AddTextScreenComponentButton.Size = new System.Drawing.Size(96, 22);
            this.AddTextScreenComponentButton.Text = "Text";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // ToolsDropDown
            // 
            this.ToolsDropDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolsDropDown.Image = global::SceneEditor.Properties.Resources.wrench;
            this.ToolsDropDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolsDropDown.Name = "ToolsDropDown";
            this.ToolsDropDown.Size = new System.Drawing.Size(29, 22);
            this.ToolsDropDown.Text = "Tools";
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
            this.WorldPanel.Controls.Add(this.worldControl);
            this.WorldPanel.Controls.Add(this.toolStrip2);
            this.WorldPanel.Controls.Add(this.statusStrip1);
            this.WorldPanel.Location = new System.Drawing.Point(323, 0);
            this.WorldPanel.Name = "WorldPanel";
            this.WorldPanel.Size = new System.Drawing.Size(1101, 862);
            this.WorldPanel.TabIndex = 3;
            // 
            // toolStrip2
            // 
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.PlayButton,
            this.StopPlayButton,
            this.toolStripSeparator4,
            this.RestartPlayButton,
            this.toolStripSeparator3,
            this.AboutButton,
            this.UndoButton,
            this.RedoButton});
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(1099, 25);
            this.toolStrip2.TabIndex = 1;
            this.toolStrip2.Text = "toolStrip2";
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
            // StopPlayButton
            // 
            this.StopPlayButton.Checked = true;
            this.StopPlayButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.StopPlayButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.StopPlayButton.Image = global::SceneEditor.Properties.Resources.control_stop_blue;
            this.StopPlayButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.StopPlayButton.Name = "StopPlayButton";
            this.StopPlayButton.Size = new System.Drawing.Size(23, 22);
            this.StopPlayButton.Text = "Stop Playing Scene";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // RestartPlayButton
            // 
            this.RestartPlayButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RestartPlayButton.Image = global::SceneEditor.Properties.Resources.control_repeat_blue;
            this.RestartPlayButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RestartPlayButton.Name = "RestartPlayButton";
            this.RestartPlayButton.Size = new System.Drawing.Size(23, 22);
            this.RestartPlayButton.Text = "Reset Play State";
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
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 838);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1099, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // worldControl
            // 
            this.worldControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.worldControl.Location = new System.Drawing.Point(0, 25);
            this.worldControl.Name = "worldControl";
            this.worldControl.Size = new System.Drawing.Size(1099, 813);
            this.worldControl.TabIndex = 0;
            this.worldControl.Text = "DeepEngine - WorldControl";
            this.worldControl.InitializeCompleted += new System.EventHandler(this.WorldControl_InitializeCompleted);
            // 
            // SceneImageList
            // 
            this.SceneImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("SceneImageList.ImageStream")));
            this.SceneImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.SceneImageList.Images.SetKeyName(0, "Scene");
            this.SceneImageList.Images.SetKeyName(1, "ScreenScene");
            this.SceneImageList.Images.SetKeyName(2, "Environment");
            this.SceneImageList.Images.SetKeyName(3, "Entity");
            this.SceneImageList.Images.SetKeyName(4, "LightComponent");
            this.SceneImageList.Images.SetKeyName(5, "Geometry");
            this.SceneImageList.Images.SetKeyName(6, "PhysicsCollider");
            this.SceneImageList.Images.SetKeyName(7, "Panel");
            this.SceneImageList.Images.SetKeyName(8, "StackPanel");
            this.SceneImageList.Images.SetKeyName(9, "Text");
            this.SceneImageList.Images.SetKeyName(10, "Script");
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
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.WorldPanel.ResumeLayout(false);
            this.WorldPanel.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel ToolboxPanel;
        private System.Windows.Forms.Panel PropertiesPanel;
        private System.Windows.Forms.Panel WorldPanel;
        private Controls.WorldControl worldControl;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton NewSceneButton;
        private System.Windows.Forms.ToolStripButton OpenSceneButton;
        private System.Windows.Forms.ToolStripButton SaveSceneButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripDropDownButton AddEntityDropDown;
        private System.Windows.Forms.ToolStripDropDownButton AddComponentDropDown;
        private System.Windows.Forms.ToolStripMenuItem AddWorldEntityButton;
        private System.Windows.Forms.ToolStripMenuItem AddGeometricPrimitiveComponentButton;
        private System.Windows.Forms.ToolStripMenuItem AddLightComponentButton;
        private System.Windows.Forms.ToolStripMenuItem AddPhysicsColliderComponentButton;
        private System.Windows.Forms.ToolStripDropDownButton AddScreenComponentDropDown;
        private System.Windows.Forms.ToolStripMenuItem AddTextScreenComponentButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripDropDownButton ToolsDropDown;
        private System.Windows.Forms.TreeView sceneTreeView;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripButton PlayButton;
        private System.Windows.Forms.ToolStripButton StopPlayButton;
        private System.Windows.Forms.ToolStripButton RestartPlayButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripButton AboutButton;
        private System.Windows.Forms.ToolStripButton UndoButton;
        private System.Windows.Forms.ToolStripButton RedoButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ImageList SceneImageList;
    }
}

