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
            this.UniformRaiseButton = new System.Windows.Forms.Button();
            this.PerlinLowerButton = new System.Windows.Forms.Button();
            this.UniformLowerButton = new System.Windows.Forms.Button();
            this.PerlinRaiseButton = new System.Windows.Forms.Button();
            this.MaxTerrainButton = new System.Windows.Forms.Button();
            this.MinTerrainButton = new System.Windows.Forms.Button();
            this.PerlinTerrainButton = new System.Windows.Forms.Button();
            this.GlobalDeformationsGroupBox = new System.Windows.Forms.GroupBox();
            this.GlobalSeedUpDown = new System.Windows.Forms.NumericUpDown();
            this.GlobalAmplitudeUpDown = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.GlobalFrequencyUpDown = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.GlobalStepUpDown = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CrosshairImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.HeightMapPreview)).BeginInit();
            this.GlobalDeformationsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalSeedUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalAmplitudeUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalFrequencyUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalStepUpDown)).BeginInit();
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
            this.HeightMapPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HeightMapPreview.Location = new System.Drawing.Point(0, 0);
            this.HeightMapPreview.Name = "HeightMapPreview";
            this.HeightMapPreview.Size = new System.Drawing.Size(256, 256);
            this.HeightMapPreview.TabIndex = 0;
            this.HeightMapPreview.TabStop = false;
            // 
            // UniformRaiseButton
            // 
            this.UniformRaiseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
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
            this.PerlinLowerButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PerlinLowerButton.Image = global::SceneEditor.Properties.Resources.bullet_arrow_down;
            this.PerlinLowerButton.Location = new System.Drawing.Point(35, 48);
            this.PerlinLowerButton.Name = "PerlinLowerButton";
            this.PerlinLowerButton.Size = new System.Drawing.Size(23, 23);
            this.PerlinLowerButton.TabIndex = 7;
            this.TerrainEditorToolTips.SetToolTip(this.PerlinLowerButton, "Perlin lower by step");
            this.PerlinLowerButton.UseVisualStyleBackColor = true;
            this.PerlinLowerButton.Click += new System.EventHandler(this.PerlinLowerButton_Click);
            // 
            // UniformLowerButton
            // 
            this.UniformLowerButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.UniformLowerButton.Image = global::SceneEditor.Properties.Resources.arrow_down;
            this.UniformLowerButton.Location = new System.Drawing.Point(6, 48);
            this.UniformLowerButton.Name = "UniformLowerButton";
            this.UniformLowerButton.Size = new System.Drawing.Size(23, 23);
            this.UniformLowerButton.TabIndex = 4;
            this.TerrainEditorToolTips.SetToolTip(this.UniformLowerButton, "Uniform lower by step");
            this.UniformLowerButton.UseVisualStyleBackColor = true;
            this.UniformLowerButton.Click += new System.EventHandler(this.UniformLowerButton_Click);
            // 
            // PerlinRaiseButton
            // 
            this.PerlinRaiseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PerlinRaiseButton.Image = global::SceneEditor.Properties.Resources.bullet_arrow_up;
            this.PerlinRaiseButton.Location = new System.Drawing.Point(35, 19);
            this.PerlinRaiseButton.Name = "PerlinRaiseButton";
            this.PerlinRaiseButton.Size = new System.Drawing.Size(23, 23);
            this.PerlinRaiseButton.TabIndex = 5;
            this.TerrainEditorToolTips.SetToolTip(this.PerlinRaiseButton, "Perlin raise by step");
            this.PerlinRaiseButton.UseVisualStyleBackColor = true;
            this.PerlinRaiseButton.Click += new System.EventHandler(this.PerlinRaiseButton_Click);
            // 
            // MaxTerrainButton
            // 
            this.MaxTerrainButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MaxTerrainButton.Location = new System.Drawing.Point(90, 77);
            this.MaxTerrainButton.Name = "MaxTerrainButton";
            this.MaxTerrainButton.Size = new System.Drawing.Size(77, 23);
            this.MaxTerrainButton.TabIndex = 18;
            this.MaxTerrainButton.Text = "Raised";
            this.TerrainEditorToolTips.SetToolTip(this.MaxTerrainButton, "Set terrain to maximum height");
            this.MaxTerrainButton.UseVisualStyleBackColor = true;
            this.MaxTerrainButton.Click += new System.EventHandler(this.MaxTerrainButton_Click);
            // 
            // MinTerrainButton
            // 
            this.MinTerrainButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MinTerrainButton.Location = new System.Drawing.Point(6, 77);
            this.MinTerrainButton.Name = "MinTerrainButton";
            this.MinTerrainButton.Size = new System.Drawing.Size(77, 23);
            this.MinTerrainButton.TabIndex = 19;
            this.MinTerrainButton.Text = "Lowered";
            this.TerrainEditorToolTips.SetToolTip(this.MinTerrainButton, "Set terrain to minimum height");
            this.MinTerrainButton.UseVisualStyleBackColor = true;
            this.MinTerrainButton.Click += new System.EventHandler(this.MinTerrainButton_Click);
            // 
            // PerlinTerrainButton
            // 
            this.PerlinTerrainButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PerlinTerrainButton.Location = new System.Drawing.Point(173, 77);
            this.PerlinTerrainButton.Name = "PerlinTerrainButton";
            this.PerlinTerrainButton.Size = new System.Drawing.Size(77, 23);
            this.PerlinTerrainButton.TabIndex = 20;
            this.PerlinTerrainButton.Text = "Perlin";
            this.TerrainEditorToolTips.SetToolTip(this.PerlinTerrainButton, "Set terrain to Perlin based on seed, bumps, and height");
            this.PerlinTerrainButton.UseVisualStyleBackColor = true;
            this.PerlinTerrainButton.Click += new System.EventHandler(this.PerlinTerrainButton_Click);
            // 
            // GlobalDeformationsGroupBox
            // 
            this.GlobalDeformationsGroupBox.Controls.Add(this.PerlinTerrainButton);
            this.GlobalDeformationsGroupBox.Controls.Add(this.MinTerrainButton);
            this.GlobalDeformationsGroupBox.Controls.Add(this.MaxTerrainButton);
            this.GlobalDeformationsGroupBox.Controls.Add(this.GlobalSeedUpDown);
            this.GlobalDeformationsGroupBox.Controls.Add(this.GlobalAmplitudeUpDown);
            this.GlobalDeformationsGroupBox.Controls.Add(this.label4);
            this.GlobalDeformationsGroupBox.Controls.Add(this.UniformLowerButton);
            this.GlobalDeformationsGroupBox.Controls.Add(this.label3);
            this.GlobalDeformationsGroupBox.Controls.Add(this.GlobalFrequencyUpDown);
            this.GlobalDeformationsGroupBox.Controls.Add(this.label2);
            this.GlobalDeformationsGroupBox.Controls.Add(this.GlobalStepUpDown);
            this.GlobalDeformationsGroupBox.Controls.Add(this.label1);
            this.GlobalDeformationsGroupBox.Controls.Add(this.UniformRaiseButton);
            this.GlobalDeformationsGroupBox.Controls.Add(this.PerlinLowerButton);
            this.GlobalDeformationsGroupBox.Controls.Add(this.PerlinRaiseButton);
            this.GlobalDeformationsGroupBox.Location = new System.Drawing.Point(3, 265);
            this.GlobalDeformationsGroupBox.Name = "GlobalDeformationsGroupBox";
            this.GlobalDeformationsGroupBox.Size = new System.Drawing.Size(256, 109);
            this.GlobalDeformationsGroupBox.TabIndex = 8;
            this.GlobalDeformationsGroupBox.TabStop = false;
            this.GlobalDeformationsGroupBox.Text = "Global Deformations";
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
            this.GlobalAmplitudeUpDown.DecimalPlaces = 2;
            this.GlobalAmplitudeUpDown.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.GlobalAmplitudeUpDown.Location = new System.Drawing.Point(191, 51);
            this.GlobalAmplitudeUpDown.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.GlobalAmplitudeUpDown.Name = "GlobalAmplitudeUpDown";
            this.GlobalAmplitudeUpDown.Size = new System.Drawing.Size(59, 20);
            this.GlobalAmplitudeUpDown.TabIndex = 15;
            this.GlobalAmplitudeUpDown.Value = new decimal(new int[] {
            75,
            0,
            0,
            131072});
            this.GlobalAmplitudeUpDown.ValueChanged += new System.EventHandler(this.GlobalAmplitudeUpDown_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(153, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Height";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(63, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Seed";
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
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(153, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Bumps";
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(63, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Step";
            // 
            // TerrainEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.GlobalDeformationsGroupBox);
            this.Controls.Add(this.panel1);
            this.Name = "TerrainEditor";
            this.Size = new System.Drawing.Size(260, 505);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.CrosshairImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.HeightMapPreview)).EndInit();
            this.GlobalDeformationsGroupBox.ResumeLayout(false);
            this.GlobalDeformationsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalSeedUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalAmplitudeUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalFrequencyUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GlobalStepUpDown)).EndInit();
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
        private System.Windows.Forms.GroupBox GlobalDeformationsGroupBox;
        private System.Windows.Forms.NumericUpDown GlobalAmplitudeUpDown;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown GlobalFrequencyUpDown;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown GlobalStepUpDown;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown GlobalSeedUpDown;
        private System.Windows.Forms.Button PerlinTerrainButton;
        private System.Windows.Forms.Button MinTerrainButton;
        private System.Windows.Forms.Button MaxTerrainButton;
    }
}
