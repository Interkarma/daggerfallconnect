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
            this.ToolboxPanel = new System.Windows.Forms.Panel();
            this.PropertiesPanel = new System.Windows.Forms.Panel();
            this.WorldPanel = new System.Windows.Forms.Panel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.NewSceneButton = new System.Windows.Forms.ToolStripButton();
            this.OpenSceneButton = new System.Windows.Forms.ToolStripButton();
            this.SaveSceneButton = new System.Windows.Forms.ToolStripButton();
            this.AddEntityDropDown = new System.Windows.Forms.ToolStripDropDownButton();
            this.AddWorldEntityButton = new System.Windows.Forms.ToolStripMenuItem();
            this.AddComponentDropDown = new System.Windows.Forms.ToolStripDropDownButton();
            this.AddGeometricPrimitiveComponentButton = new System.Windows.Forms.ToolStripMenuItem();
            this.AddLightComponentButton = new System.Windows.Forms.ToolStripMenuItem();
            this.AddPhysicsColliderComponentButton = new System.Windows.Forms.ToolStripMenuItem();
            this.AddScreenComponentDropDown = new System.Windows.Forms.ToolStripDropDownButton();
            this.AddTextScreenComponentButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolsDropDown = new System.Windows.Forms.ToolStripDropDownButton();
            this.sceneTreeView = new System.Windows.Forms.TreeView();
            this.worldControl = new SceneEditor.Controls.WorldControl();
            this.ToolboxPanel.SuspendLayout();
            this.WorldPanel.SuspendLayout();
            this.toolStrip1.SuspendLayout();
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
            this.WorldPanel.Location = new System.Drawing.Point(323, 0);
            this.WorldPanel.Name = "WorldPanel";
            this.WorldPanel.Size = new System.Drawing.Size(1101, 862);
            this.WorldPanel.TabIndex = 3;
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
            // ToolsDropDown
            // 
            this.ToolsDropDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolsDropDown.Image = global::SceneEditor.Properties.Resources.wrench;
            this.ToolsDropDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolsDropDown.Name = "ToolsDropDown";
            this.ToolsDropDown.Size = new System.Drawing.Size(29, 22);
            this.ToolsDropDown.Text = "Tools";
            // 
            // sceneTreeView
            // 
            this.sceneTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sceneTreeView.Location = new System.Drawing.Point(0, 26);
            this.sceneTreeView.Name = "sceneTreeView";
            this.sceneTreeView.Size = new System.Drawing.Size(318, 481);
            this.sceneTreeView.TabIndex = 4;
            this.sceneTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.SceneTreeView_AfterSelect);
            // 
            // worldControl
            // 
            this.worldControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.worldControl.Location = new System.Drawing.Point(0, 0);
            this.worldControl.Name = "worldControl";
            this.worldControl.Size = new System.Drawing.Size(1099, 860);
            this.worldControl.TabIndex = 0;
            this.worldControl.Text = "DeepEngine - WorldControl";
            this.worldControl.InitializeCompleted += new System.EventHandler(this.WorldControl_InitializeCompleted);
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
            this.WorldPanel.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
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
    }
}

