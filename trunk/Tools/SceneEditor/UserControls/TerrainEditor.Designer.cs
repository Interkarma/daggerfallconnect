namespace SceneEditor.UserControls
{
    partial class TerrainEditor
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.CrosshairImage = new System.Windows.Forms.PictureBox();
            this.HeightMapPreview = new System.Windows.Forms.PictureBox();
            this.TerrainEditorToolTips = new System.Windows.Forms.ToolTip(this.components);
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SmoothButton = new System.Windows.Forms.Button();
            this.AutoPaintButton = new System.Windows.Forms.Button();
            this.PerlinTerrainButton = new System.Windows.Forms.Button();
            this.UniformLowerButton = new System.Windows.Forms.Button();
            this.UniformRaiseButton = new System.Windows.Forms.Button();
            this.PerlinLowerButton = new System.Windows.Forms.Button();
            this.PerlinRaiseButton = new System.Windows.Forms.Button();
            this.GlobalGroupBox = new System.Windows.Forms.GroupBox();
            this.GlobalSeedUpDown = new System.Windows.Forms.NumericUpDown();
            this.GlobalAmplitudeUpDown = new System.Windows.Forms.NumericUpDown();
            this.GlobalFrequencyUpDown = new System.Windows.Forms.NumericUpDown();
            this.GlobalStepUpDown = new System.Windows.Forms.NumericUpDown();
            this.DeformationsGroupBox = new System.Windows.Forms.GroupBox();
            this.DeformationsToolStrip = new System.Windows.Forms.ToolStrip();
            this.DeformUpDownButton = new System.Windows.Forms.ToolStripButton();
            this.DeformSmoothButton = new System.Windows.Forms.ToolStripButton();
            this.DeformBumpsButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.DeformRadiusTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.DeformFromImageButton = new System.Windows.Forms.ToolStripButton();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CrosshairImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.HeightMapPreview)).BeginInit();
            this.GlobalGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalSeedUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalAmplitudeUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalFrequencyUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalStepUpDown)).BeginInit();
            this.DeformationsGroupBox.SuspendLayout();
            this.DeformationsToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.CrosshairImage);
            this.panel1.Controls.Add(this.HeightMapPreview);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(256, 256);
            this.panel1.TabIndex = 0;
            // 
            // CrosshairImage
            // 
            this.CrosshairImage.Image = global::SceneEditor.Properties.Resources.Crosshair;
            this.CrosshairImage.Location = new System.Drawing.Point(124, 132);
            this.CrosshairImage.Name = "CrosshairImage";
            this.CrosshairImage.Size = new System.Drawing.Size(3, 3);
            this.CrosshairImage.TabIndex = 1;
            this.CrosshairImage.TabStop = false;
            // 
            // HeightMapPreview
            // 
            this.HeightMapPreview.BackColor = System.Drawing.Color.Black;
            this.HeightMapPreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.HeightMapPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HeightMapPreview.Location = new System.Drawing.Point(0, 0);
            this.HeightMapPreview.Name = "HeightMapPreview";
            this.HeightMapPreview.Size = new System.Drawing.Size(256, 256);
            this.HeightMapPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.HeightMapPreview.TabIndex = 0;
            this.HeightMapPreview.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(153, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Height";
            this.TerrainEditorToolTips.SetToolTip(this.label4, "Amplitude for Perlin noise (overall height)");
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(63, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Seed";
            this.TerrainEditorToolTips.SetToolTip(this.label3, "Seed for Perlin noise");
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(153, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Bumps";
            this.TerrainEditorToolTips.SetToolTip(this.label2, "Frequency for Perlin noise (overall bumpiness)");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(63, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Step";
            this.TerrainEditorToolTips.SetToolTip(this.label1, "Step height for global deformations");
            // 
            // SmoothButton
            // 
            this.SmoothButton.Image = global::SceneEditor.Properties.Resources.contrast_low;
            this.SmoothButton.Location = new System.Drawing.Point(35, 77);
            this.SmoothButton.Name = "SmoothButton";
            this.SmoothButton.Size = new System.Drawing.Size(23, 23);
            this.SmoothButton.TabIndex = 24;
            this.TerrainEditorToolTips.SetToolTip(this.SmoothButton, "Smooth terrain height map");
            this.SmoothButton.UseVisualStyleBackColor = true;
            this.SmoothButton.Click += new System.EventHandler(this.SmoothButton_Click);
            // 
            // AutoPaintButton
            // 
            this.AutoPaintButton.Image = global::SceneEditor.Properties.Resources.paintbrush;
            this.AutoPaintButton.Location = new System.Drawing.Point(64, 77);
            this.AutoPaintButton.Name = "AutoPaintButton";
            this.AutoPaintButton.Size = new System.Drawing.Size(23, 23);
            this.AutoPaintButton.TabIndex = 21;
            this.TerrainEditorToolTips.SetToolTip(this.AutoPaintButton, "Auto-paint blend map based on height");
            this.AutoPaintButton.UseVisualStyleBackColor = true;
            this.AutoPaintButton.Click += new System.EventHandler(this.AutoPaintButton_Click);
            // 
            // PerlinTerrainButton
            // 
            this.PerlinTerrainButton.Image = global::SceneEditor.Properties.Resources.chart_curve;
            this.PerlinTerrainButton.Location = new System.Drawing.Point(6, 77);
            this.PerlinTerrainButton.Name = "PerlinTerrainButton";
            this.PerlinTerrainButton.Size = new System.Drawing.Size(23, 23);
            this.PerlinTerrainButton.TabIndex = 20;
            this.TerrainEditorToolTips.SetToolTip(this.PerlinTerrainButton, "Set terrain to Perlin based on seed, bumps, and height");
            this.PerlinTerrainButton.UseVisualStyleBackColor = true;
            this.PerlinTerrainButton.Click += new System.EventHandler(this.PerlinTerrainButton_Click);
            // 
            // UniformLowerButton
            // 
            this.UniformLowerButton.Image = global::SceneEditor.Properties.Resources.arrow_down;
            this.UniformLowerButton.Location = new System.Drawing.Point(6, 48);
            this.UniformLowerButton.Name = "UniformLowerButton";
            this.UniformLowerButton.Size = new System.Drawing.Size(23, 23);
            this.UniformLowerButton.TabIndex = 4;
            this.TerrainEditorToolTips.SetToolTip(this.UniformLowerButton, "Uniform lower by step");
            this.UniformLowerButton.UseVisualStyleBackColor = true;
            this.UniformLowerButton.Click += new System.EventHandler(this.UniformLowerButton_Click);
            // 
            // UniformRaiseButton
            // 
            this.UniformRaiseButton.Image = global::SceneEditor.Properties.Resources.arrow_up;
            this.UniformRaiseButton.Location = new System.Drawing.Point(6, 19);
            this.UniformRaiseButton.Name = "UniformRaiseButton";
            this.UniformRaiseButton.Size = new System.Drawing.Size(23, 23);
            this.UniformRaiseButton.TabIndex = 1;
            this.TerrainEditorToolTips.SetToolTip(this.UniformRaiseButton, "Uniform raise by step");
            this.UniformRaiseButton.UseVisualStyleBackColor = true;
            this.UniformRaiseButton.Click += new System.EventHandler(this.UniformRaiseButton_Click);
            // 
            // PerlinLowerButton
            // 
            this.PerlinLowerButton.Image = global::SceneEditor.Properties.Resources.bullet_arrow_down;
            this.PerlinLowerButton.Location = new System.Drawing.Point(35, 48);
            this.PerlinLowerButton.Name = "PerlinLowerButton";
            this.PerlinLowerButton.Size = new System.Drawing.Size(23, 23);
            this.PerlinLowerButton.TabIndex = 7;
            this.TerrainEditorToolTips.SetToolTip(this.PerlinLowerButton, "Perlin lower by step");
            this.PerlinLowerButton.UseVisualStyleBackColor = true;
            this.PerlinLowerButton.Click += new System.EventHandler(this.PerlinLowerButton_Click);
            // 
            // PerlinRaiseButton
            // 
            this.PerlinRaiseButton.Image = global::SceneEditor.Properties.Resources.bullet_arrow_up;
            this.PerlinRaiseButton.Location = new System.Drawing.Point(35, 19);
            this.PerlinRaiseButton.Name = "PerlinRaiseButton";
            this.PerlinRaiseButton.Size = new System.Drawing.Size(23, 23);
            this.PerlinRaiseButton.TabIndex = 5;
            this.TerrainEditorToolTips.SetToolTip(this.PerlinRaiseButton, "Perlin raise by step");
            this.PerlinRaiseButton.UseVisualStyleBackColor = true;
            this.PerlinRaiseButton.Click += new System.EventHandler(this.PerlinRaiseButton_Click);
            // 
            // GlobalGroupBox
            // 
            this.GlobalGroupBox.Controls.Add(this.SmoothButton);
            this.GlobalGroupBox.Controls.Add(this.AutoPaintButton);
            this.GlobalGroupBox.Controls.Add(this.PerlinTerrainButton);
            this.GlobalGroupBox.Controls.Add(this.GlobalSeedUpDown);
            this.GlobalGroupBox.Controls.Add(this.GlobalAmplitudeUpDown);
            this.GlobalGroupBox.Controls.Add(this.label4);
            this.GlobalGroupBox.Controls.Add(this.UniformLowerButton);
            this.GlobalGroupBox.Controls.Add(this.label3);
            this.GlobalGroupBox.Controls.Add(this.GlobalFrequencyUpDown);
            this.GlobalGroupBox.Controls.Add(this.label2);
            this.GlobalGroupBox.Controls.Add(this.GlobalStepUpDown);
            this.GlobalGroupBox.Controls.Add(this.label1);
            this.GlobalGroupBox.Controls.Add(this.UniformRaiseButton);
            this.GlobalGroupBox.Controls.Add(this.PerlinLowerButton);
            this.GlobalGroupBox.Controls.Add(this.PerlinRaiseButton);
            this.GlobalGroupBox.Location = new System.Drawing.Point(3, 265);
            this.GlobalGroupBox.Name = "GlobalGroupBox";
            this.GlobalGroupBox.Size = new System.Drawing.Size(256, 109);
            this.GlobalGroupBox.TabIndex = 8;
            this.GlobalGroupBox.TabStop = false;
            this.GlobalGroupBox.Text = "Global";
            // 
            // GlobalSeedUpDown
            // 
            this.GlobalSeedUpDown.Location = new System.Drawing.Point(97, 51);
            this.GlobalSeedUpDown.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.GlobalSeedUpDown.Name = "GlobalSeedUpDown";
            this.GlobalSeedUpDown.Size = new System.Drawing.Size(51, 20);
            this.GlobalSeedUpDown.TabIndex = 16;
            this.GlobalSeedUpDown.ValueChanged += new System.EventHandler(this.GlobalSeedUpDown_ValueChanged);
            // 
            // GlobalAmplitudeUpDown
            // 
            this.GlobalAmplitudeUpDown.DecimalPlaces = 4;
            this.GlobalAmplitudeUpDown.Increment = new decimal(new int[] {
            5,
            0,
            0,
            262144});
            this.GlobalAmplitudeUpDown.Location = new System.Drawing.Point(191, 51);
            this.GlobalAmplitudeUpDown.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.GlobalAmplitudeUpDown.Name = "GlobalAmplitudeUpDown";
            this.GlobalAmplitudeUpDown.Size = new System.Drawing.Size(59, 20);
            this.GlobalAmplitudeUpDown.TabIndex = 15;
            this.GlobalAmplitudeUpDown.Value = new decimal(new int[] {
            90,
            0,
            0,
            131072});
            this.GlobalAmplitudeUpDown.ValueChanged += new System.EventHandler(this.GlobalAmplitudeUpDown_ValueChanged);
            // 
            // GlobalFrequencyUpDown
            // 
            this.GlobalFrequencyUpDown.DecimalPlaces = 4;
            this.GlobalFrequencyUpDown.Increment = new decimal(new int[] {
            5,
            0,
            0,
            262144});
            this.GlobalFrequencyUpDown.Location = new System.Drawing.Point(191, 22);
            this.GlobalFrequencyUpDown.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.GlobalFrequencyUpDown.Name = "GlobalFrequencyUpDown";
            this.GlobalFrequencyUpDown.Size = new System.Drawing.Size(59, 20);
            this.GlobalFrequencyUpDown.TabIndex = 11;
            this.GlobalFrequencyUpDown.Value = new decimal(new int[] {
            125,
            0,
            0,
            262144});
            this.GlobalFrequencyUpDown.ValueChanged += new System.EventHandler(this.GlobalFrequencyUpDown_ValueChanged);
            // 
            // GlobalStepUpDown
            // 
            this.GlobalStepUpDown.Location = new System.Drawing.Point(97, 22);
            this.GlobalStepUpDown.Maximum = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.GlobalStepUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.GlobalStepUpDown.Name = "GlobalStepUpDown";
            this.GlobalStepUpDown.Size = new System.Drawing.Size(51, 20);
            this.GlobalStepUpDown.TabIndex = 9;
            this.GlobalStepUpDown.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
            // 
            // DeformationsGroupBox
            // 
            this.DeformationsGroupBox.Controls.Add(this.DeformationsToolStrip);
            this.DeformationsGroupBox.Location = new System.Drawing.Point(3, 380);
            this.DeformationsGroupBox.Name = "DeformationsGroupBox";
            this.DeformationsGroupBox.Size = new System.Drawing.Size(256, 46);
            this.DeformationsGroupBox.TabIndex = 9;
            this.DeformationsGroupBox.TabStop = false;
            this.DeformationsGroupBox.Text = "Deformations";
            // 
            // DeformationsToolStrip
            // 
            this.DeformationsToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.DeformationsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.DeformRadiusTextBox,
            this.toolStripSeparator2,
            this.DeformUpDownButton,
            this.DeformSmoothButton,
            this.DeformBumpsButton,
            this.toolStripSeparator1,
            this.DeformFromImageButton});
            this.DeformationsToolStrip.Location = new System.Drawing.Point(3, 16);
            this.DeformationsToolStrip.Name = "DeformationsToolStrip";
            this.DeformationsToolStrip.Size = new System.Drawing.Size(250, 25);
            this.DeformationsToolStrip.TabIndex = 0;
            this.DeformationsToolStrip.Text = "toolStrip1";
            // 
            // DeformUpDownButton
            // 
            this.DeformUpDownButton.Checked = true;
            this.DeformUpDownButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DeformUpDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.DeformUpDownButton.Image = global::SceneEditor.Properties.Resources.RaiseLower;
            this.DeformUpDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.DeformUpDownButton.Name = "DeformUpDownButton";
            this.DeformUpDownButton.Size = new System.Drawing.Size(23, 22);
            this.DeformUpDownButton.Text = "Deform Up Down";
            // 
            // DeformSmoothButton
            // 
            this.DeformSmoothButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.DeformSmoothButton.Image = global::SceneEditor.Properties.Resources.Smooth1;
            this.DeformSmoothButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.DeformSmoothButton.Name = "DeformSmoothButton";
            this.DeformSmoothButton.Size = new System.Drawing.Size(23, 22);
            this.DeformSmoothButton.Text = "Smooth";
            // 
            // DeformBumpsButton
            // 
            this.DeformBumpsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.DeformBumpsButton.Image = global::SceneEditor.Properties.Resources.Bumps;
            this.DeformBumpsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.DeformBumpsButton.Name = "DeformBumpsButton";
            this.DeformBumpsButton.Size = new System.Drawing.Size(23, 22);
            this.DeformBumpsButton.Text = "Bumps";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(45, 22);
            this.toolStripLabel1.Text = "Radius:";
            // 
            // DeformRadiusTextBox
            // 
            this.DeformRadiusTextBox.Name = "DeformRadiusTextBox";
            this.DeformRadiusTextBox.Size = new System.Drawing.Size(50, 25);
            this.DeformRadiusTextBox.Text = "32";
            this.DeformRadiusTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.DeformRadiusTextBox_KeyPress);
            this.DeformRadiusTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.DeformRadiusTextBox_Validating);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // DeformFromImageButton
            // 
            this.DeformFromImageButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.DeformFromImageButton.Image = global::SceneEditor.Properties.Resources.image;
            this.DeformFromImageButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.DeformFromImageButton.Name = "DeformFromImageButton";
            this.DeformFromImageButton.Size = new System.Drawing.Size(23, 22);
            this.DeformFromImageButton.Text = "Deform From Image";
            // 
            // TerrainEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DeformationsGroupBox);
            this.Controls.Add(this.GlobalGroupBox);
            this.Controls.Add(this.panel1);
            this.Name = "TerrainEditor";
            this.Size = new System.Drawing.Size(260, 549);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.CrosshairImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.HeightMapPreview)).EndInit();
            this.GlobalGroupBox.ResumeLayout(false);
            this.GlobalGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalSeedUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalAmplitudeUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalFrequencyUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalStepUpDown)).EndInit();
            this.DeformationsGroupBox.ResumeLayout(false);
            this.DeformationsGroupBox.PerformLayout();
            this.DeformationsToolStrip.ResumeLayout(false);
            this.DeformationsToolStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox HeightMapPreview;
        private System.Windows.Forms.Button UniformRaiseButton;
        private System.Windows.Forms.Button UniformLowerButton;
        private System.Windows.Forms.Button PerlinRaiseButton;
        private System.Windows.Forms.Button PerlinLowerButton;
        private System.Windows.Forms.PictureBox CrosshairImage;
        private System.Windows.Forms.ToolTip TerrainEditorToolTips;
        private System.Windows.Forms.GroupBox GlobalGroupBox;
        private System.Windows.Forms.NumericUpDown GlobalAmplitudeUpDown;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown GlobalFrequencyUpDown;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown GlobalStepUpDown;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown GlobalSeedUpDown;
        private System.Windows.Forms.Button PerlinTerrainButton;
        private System.Windows.Forms.Button AutoPaintButton;
        private System.Windows.Forms.Button SmoothButton;
        private System.Windows.Forms.GroupBox DeformationsGroupBox;
        private System.Windows.Forms.ToolStrip DeformationsToolStrip;
        private System.Windows.Forms.ToolStripButton DeformUpDownButton;
        private System.Windows.Forms.ToolStripButton DeformSmoothButton;
        private System.Windows.Forms.ToolStripButton DeformBumpsButton;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripTextBox DeformRadiusTextBox;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton DeformFromImageButton;
    }
}
