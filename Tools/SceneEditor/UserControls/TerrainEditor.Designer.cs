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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TerrainEditor));
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
            this.PaintingGroupBox = new System.Windows.Forms.GroupBox();
            this.DeformationsToolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.DeformRadiusTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.DeformUpDownButton = new System.Windows.Forms.ToolStripButton();
            this.DeformSmoothButton = new System.Windows.Forms.ToolStripButton();
            this.DeformBumpsButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.TexturePaintButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.TexturesGroupBox = new System.Windows.Forms.GroupBox();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.Texture4RadioButton = new System.Windows.Forms.RadioButton();
            this.Texture3RadioButton = new System.Windows.Forms.RadioButton();
            this.Texture2RadioButton = new System.Windows.Forms.RadioButton();
            this.Texture1RadioButton = new System.Windows.Forms.RadioButton();
            this.Texture1PictureBox = new System.Windows.Forms.PictureBox();
            this.Texture2PictureBox = new System.Windows.Forms.PictureBox();
            this.Texture3PictureBox = new System.Windows.Forms.PictureBox();
            this.Texture4PictureBox = new System.Windows.Forms.PictureBox();
            this.Texture5PictureBox = new System.Windows.Forms.PictureBox();
            this.Texture6PictureBox = new System.Windows.Forms.PictureBox();
            this.Texture7PictureBox = new System.Windows.Forms.PictureBox();
            this.Texture8PictureBox = new System.Windows.Forms.PictureBox();
            this.Texture0PictureBox = new System.Windows.Forms.PictureBox();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CrosshairImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.HeightMapPreview)).BeginInit();
            this.GlobalGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalSeedUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalAmplitudeUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalFrequencyUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalStepUpDown)).BeginInit();
            this.PaintingGroupBox.SuspendLayout();
            this.DeformationsToolStrip.SuspendLayout();
            this.TexturesGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Texture1PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Texture2PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Texture3PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Texture4PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Texture5PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Texture6PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Texture7PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Texture8PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Texture0PictureBox)).BeginInit();
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
            // PaintingGroupBox
            // 
            this.PaintingGroupBox.Controls.Add(this.DeformationsToolStrip);
            this.PaintingGroupBox.Location = new System.Drawing.Point(3, 472);
            this.PaintingGroupBox.Name = "PaintingGroupBox";
            this.PaintingGroupBox.Size = new System.Drawing.Size(256, 46);
            this.PaintingGroupBox.TabIndex = 9;
            this.PaintingGroupBox.TabStop = false;
            this.PaintingGroupBox.Text = "Painting";
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
            this.TexturePaintButton,
            this.toolStripSeparator3});
            this.DeformationsToolStrip.Location = new System.Drawing.Point(3, 16);
            this.DeformationsToolStrip.Name = "DeformationsToolStrip";
            this.DeformationsToolStrip.Size = new System.Drawing.Size(250, 25);
            this.DeformationsToolStrip.TabIndex = 0;
            this.DeformationsToolStrip.Text = "toolStrip1";
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
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // TexturePaintButton
            // 
            this.TexturePaintButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.TexturePaintButton.Image = ((System.Drawing.Image)(resources.GetObject("TexturePaintButton.Image")));
            this.TexturePaintButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.TexturePaintButton.Name = "TexturePaintButton";
            this.TexturePaintButton.Size = new System.Drawing.Size(23, 22);
            this.TexturePaintButton.Text = "Paint Texture";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // TexturesGroupBox
            // 
            this.TexturesGroupBox.Controls.Add(this.Texture0PictureBox);
            this.TexturesGroupBox.Controls.Add(this.radioButton5);
            this.TexturesGroupBox.Controls.Add(this.Texture8PictureBox);
            this.TexturesGroupBox.Controls.Add(this.Texture7PictureBox);
            this.TexturesGroupBox.Controls.Add(this.Texture6PictureBox);
            this.TexturesGroupBox.Controls.Add(this.Texture5PictureBox);
            this.TexturesGroupBox.Controls.Add(this.Texture4PictureBox);
            this.TexturesGroupBox.Controls.Add(this.Texture3PictureBox);
            this.TexturesGroupBox.Controls.Add(this.Texture2PictureBox);
            this.TexturesGroupBox.Controls.Add(this.Texture1PictureBox);
            this.TexturesGroupBox.Controls.Add(this.radioButton4);
            this.TexturesGroupBox.Controls.Add(this.radioButton3);
            this.TexturesGroupBox.Controls.Add(this.radioButton2);
            this.TexturesGroupBox.Controls.Add(this.radioButton1);
            this.TexturesGroupBox.Controls.Add(this.Texture4RadioButton);
            this.TexturesGroupBox.Controls.Add(this.Texture3RadioButton);
            this.TexturesGroupBox.Controls.Add(this.Texture2RadioButton);
            this.TexturesGroupBox.Controls.Add(this.Texture1RadioButton);
            this.TexturesGroupBox.Location = new System.Drawing.Point(3, 380);
            this.TexturesGroupBox.Name = "TexturesGroupBox";
            this.TexturesGroupBox.Size = new System.Drawing.Size(256, 86);
            this.TexturesGroupBox.TabIndex = 10;
            this.TexturesGroupBox.TabStop = false;
            this.TexturesGroupBox.Text = "Textures";
            // 
            // radioButton4
            // 
            this.radioButton4.AutoSize = true;
            this.radioButton4.Location = new System.Drawing.Point(156, 54);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(14, 13);
            this.radioButton4.TabIndex = 18;
            this.radioButton4.TabStop = true;
            this.radioButton4.UseVisualStyleBackColor = true;
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(106, 54);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(14, 13);
            this.radioButton3.TabIndex = 17;
            this.radioButton3.TabStop = true;
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(56, 54);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(14, 13);
            this.radioButton2.TabIndex = 16;
            this.radioButton2.TabStop = true;
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(6, 54);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(14, 13);
            this.radioButton1.TabIndex = 15;
            this.radioButton1.TabStop = true;
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // Texture4RadioButton
            // 
            this.Texture4RadioButton.AutoSize = true;
            this.Texture4RadioButton.Location = new System.Drawing.Point(156, 25);
            this.Texture4RadioButton.Name = "Texture4RadioButton";
            this.Texture4RadioButton.Size = new System.Drawing.Size(14, 13);
            this.Texture4RadioButton.TabIndex = 3;
            this.Texture4RadioButton.TabStop = true;
            this.Texture4RadioButton.UseVisualStyleBackColor = true;
            // 
            // Texture3RadioButton
            // 
            this.Texture3RadioButton.AutoSize = true;
            this.Texture3RadioButton.Location = new System.Drawing.Point(106, 25);
            this.Texture3RadioButton.Name = "Texture3RadioButton";
            this.Texture3RadioButton.Size = new System.Drawing.Size(14, 13);
            this.Texture3RadioButton.TabIndex = 2;
            this.Texture3RadioButton.TabStop = true;
            this.Texture3RadioButton.UseVisualStyleBackColor = true;
            // 
            // Texture2RadioButton
            // 
            this.Texture2RadioButton.AutoSize = true;
            this.Texture2RadioButton.Location = new System.Drawing.Point(56, 25);
            this.Texture2RadioButton.Name = "Texture2RadioButton";
            this.Texture2RadioButton.Size = new System.Drawing.Size(14, 13);
            this.Texture2RadioButton.TabIndex = 1;
            this.Texture2RadioButton.TabStop = true;
            this.Texture2RadioButton.UseVisualStyleBackColor = true;
            // 
            // Texture1RadioButton
            // 
            this.Texture1RadioButton.AutoSize = true;
            this.Texture1RadioButton.Location = new System.Drawing.Point(6, 25);
            this.Texture1RadioButton.Name = "Texture1RadioButton";
            this.Texture1RadioButton.Size = new System.Drawing.Size(14, 13);
            this.Texture1RadioButton.TabIndex = 0;
            this.Texture1RadioButton.TabStop = true;
            this.Texture1RadioButton.UseVisualStyleBackColor = true;
            // 
            // Texture1PictureBox
            // 
            this.Texture1PictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Texture1PictureBox.Location = new System.Drawing.Point(26, 19);
            this.Texture1PictureBox.Name = "Texture1PictureBox";
            this.Texture1PictureBox.Size = new System.Drawing.Size(24, 24);
            this.Texture1PictureBox.TabIndex = 19;
            this.Texture1PictureBox.TabStop = false;
            this.TerrainEditorToolTips.SetToolTip(this.Texture1PictureBox, "Texture 1");
            // 
            // Texture2PictureBox
            // 
            this.Texture2PictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Texture2PictureBox.Location = new System.Drawing.Point(76, 19);
            this.Texture2PictureBox.Name = "Texture2PictureBox";
            this.Texture2PictureBox.Size = new System.Drawing.Size(24, 24);
            this.Texture2PictureBox.TabIndex = 20;
            this.Texture2PictureBox.TabStop = false;
            this.TerrainEditorToolTips.SetToolTip(this.Texture2PictureBox, "Texture 2");
            // 
            // Texture3PictureBox
            // 
            this.Texture3PictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Texture3PictureBox.Location = new System.Drawing.Point(126, 19);
            this.Texture3PictureBox.Name = "Texture3PictureBox";
            this.Texture3PictureBox.Size = new System.Drawing.Size(24, 24);
            this.Texture3PictureBox.TabIndex = 21;
            this.Texture3PictureBox.TabStop = false;
            this.TerrainEditorToolTips.SetToolTip(this.Texture3PictureBox, "Texture 3");
            // 
            // Texture4PictureBox
            // 
            this.Texture4PictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Texture4PictureBox.Location = new System.Drawing.Point(176, 19);
            this.Texture4PictureBox.Name = "Texture4PictureBox";
            this.Texture4PictureBox.Size = new System.Drawing.Size(24, 24);
            this.Texture4PictureBox.TabIndex = 22;
            this.Texture4PictureBox.TabStop = false;
            this.TerrainEditorToolTips.SetToolTip(this.Texture4PictureBox, "Texture 4");
            // 
            // Texture5PictureBox
            // 
            this.Texture5PictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Texture5PictureBox.Location = new System.Drawing.Point(26, 49);
            this.Texture5PictureBox.Name = "Texture5PictureBox";
            this.Texture5PictureBox.Size = new System.Drawing.Size(24, 24);
            this.Texture5PictureBox.TabIndex = 23;
            this.Texture5PictureBox.TabStop = false;
            this.TerrainEditorToolTips.SetToolTip(this.Texture5PictureBox, "Texture 5");
            // 
            // Texture6PictureBox
            // 
            this.Texture6PictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Texture6PictureBox.Location = new System.Drawing.Point(76, 49);
            this.Texture6PictureBox.Name = "Texture6PictureBox";
            this.Texture6PictureBox.Size = new System.Drawing.Size(24, 24);
            this.Texture6PictureBox.TabIndex = 24;
            this.Texture6PictureBox.TabStop = false;
            this.TerrainEditorToolTips.SetToolTip(this.Texture6PictureBox, "Texture 6");
            // 
            // Texture7PictureBox
            // 
            this.Texture7PictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Texture7PictureBox.Location = new System.Drawing.Point(126, 49);
            this.Texture7PictureBox.Name = "Texture7PictureBox";
            this.Texture7PictureBox.Size = new System.Drawing.Size(24, 24);
            this.Texture7PictureBox.TabIndex = 25;
            this.Texture7PictureBox.TabStop = false;
            this.TerrainEditorToolTips.SetToolTip(this.Texture7PictureBox, "Texture 7");
            // 
            // Texture8PictureBox
            // 
            this.Texture8PictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Texture8PictureBox.Location = new System.Drawing.Point(176, 49);
            this.Texture8PictureBox.Name = "Texture8PictureBox";
            this.Texture8PictureBox.Size = new System.Drawing.Size(24, 24);
            this.Texture8PictureBox.TabIndex = 26;
            this.Texture8PictureBox.TabStop = false;
            this.TerrainEditorToolTips.SetToolTip(this.Texture8PictureBox, "Texture 8");
            // 
            // Texture0PictureBox
            // 
            this.Texture0PictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Texture0PictureBox.Location = new System.Drawing.Point(226, 49);
            this.Texture0PictureBox.Name = "Texture0PictureBox";
            this.Texture0PictureBox.Size = new System.Drawing.Size(24, 24);
            this.Texture0PictureBox.TabIndex = 29;
            this.Texture0PictureBox.TabStop = false;
            this.TerrainEditorToolTips.SetToolTip(this.Texture0PictureBox, "Texture 0 (Clear Texture)");
            // 
            // radioButton5
            // 
            this.radioButton5.AutoSize = true;
            this.radioButton5.Checked = true;
            this.radioButton5.Location = new System.Drawing.Point(206, 54);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(14, 13);
            this.radioButton5.TabIndex = 28;
            this.radioButton5.TabStop = true;
            this.radioButton5.UseVisualStyleBackColor = true;
            // 
            // TerrainEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TexturesGroupBox);
            this.Controls.Add(this.PaintingGroupBox);
            this.Controls.Add(this.GlobalGroupBox);
            this.Controls.Add(this.panel1);
            this.Name = "TerrainEditor";
            this.Size = new System.Drawing.Size(260, 605);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.CrosshairImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.HeightMapPreview)).EndInit();
            this.GlobalGroupBox.ResumeLayout(false);
            this.GlobalGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalSeedUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalAmplitudeUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalFrequencyUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalStepUpDown)).EndInit();
            this.PaintingGroupBox.ResumeLayout(false);
            this.PaintingGroupBox.PerformLayout();
            this.DeformationsToolStrip.ResumeLayout(false);
            this.DeformationsToolStrip.PerformLayout();
            this.TexturesGroupBox.ResumeLayout(false);
            this.TexturesGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Texture1PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Texture2PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Texture3PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Texture4PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Texture5PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Texture6PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Texture7PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Texture8PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Texture0PictureBox)).EndInit();
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
        private System.Windows.Forms.GroupBox PaintingGroupBox;
        private System.Windows.Forms.ToolStrip DeformationsToolStrip;
        private System.Windows.Forms.ToolStripButton DeformUpDownButton;
        private System.Windows.Forms.ToolStripButton DeformSmoothButton;
        private System.Windows.Forms.ToolStripButton DeformBumpsButton;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripTextBox DeformRadiusTextBox;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.GroupBox TexturesGroupBox;
        private System.Windows.Forms.RadioButton radioButton4;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton Texture4RadioButton;
        private System.Windows.Forms.RadioButton Texture3RadioButton;
        private System.Windows.Forms.RadioButton Texture2RadioButton;
        private System.Windows.Forms.RadioButton Texture1RadioButton;
        private System.Windows.Forms.ToolStripButton TexturePaintButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.PictureBox Texture0PictureBox;
        private System.Windows.Forms.RadioButton radioButton5;
        private System.Windows.Forms.PictureBox Texture8PictureBox;
        private System.Windows.Forms.PictureBox Texture7PictureBox;
        private System.Windows.Forms.PictureBox Texture6PictureBox;
        private System.Windows.Forms.PictureBox Texture5PictureBox;
        private System.Windows.Forms.PictureBox Texture4PictureBox;
        private System.Windows.Forms.PictureBox Texture3PictureBox;
        private System.Windows.Forms.PictureBox Texture2PictureBox;
        private System.Windows.Forms.PictureBox Texture1PictureBox;
    }
}
