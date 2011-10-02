namespace DaggerfallModelling.Dialogs
{
    partial class InputSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InputSettingsDialog));
            this.MyOKButton = new System.Windows.Forms.Button();
            this.MyCancelButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbGravity = new System.Windows.Forms.CheckBox();
            this.cbCollision = new System.Windows.Forms.CheckBox();
            this.cbInvertGamePadY = new System.Windows.Forms.CheckBox();
            this.cbInvertMouseY = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // MyOKButton
            // 
            this.MyOKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.MyOKButton.Location = new System.Drawing.Point(12, 137);
            this.MyOKButton.Name = "MyOKButton";
            this.MyOKButton.Size = new System.Drawing.Size(75, 23);
            this.MyOKButton.TabIndex = 2;
            this.MyOKButton.Text = "OK";
            this.MyOKButton.UseVisualStyleBackColor = true;
            this.MyOKButton.Click += new System.EventHandler(this.MyOKButton_Click);
            // 
            // MyCancelButton
            // 
            this.MyCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.MyCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.MyCancelButton.Location = new System.Drawing.Point(217, 137);
            this.MyCancelButton.Name = "MyCancelButton";
            this.MyCancelButton.Size = new System.Drawing.Size(75, 23);
            this.MyCancelButton.TabIndex = 3;
            this.MyCancelButton.Text = "Cancel";
            this.MyCancelButton.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbGravity);
            this.groupBox1.Controls.Add(this.cbCollision);
            this.groupBox1.Controls.Add(this.cbInvertGamePadY);
            this.groupBox1.Controls.Add(this.cbInvertMouseY);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(280, 113);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Basic";
            // 
            // cbGravity
            // 
            this.cbGravity.AutoSize = true;
            this.cbGravity.Location = new System.Drawing.Point(191, 70);
            this.cbGravity.Name = "cbGravity";
            this.cbGravity.Size = new System.Drawing.Size(59, 17);
            this.cbGravity.TabIndex = 3;
            this.cbGravity.Text = "Gravity";
            this.cbGravity.UseVisualStyleBackColor = true;
            // 
            // cbCollision
            // 
            this.cbCollision.AutoSize = true;
            this.cbCollision.Location = new System.Drawing.Point(191, 28);
            this.cbCollision.Name = "cbCollision";
            this.cbCollision.Size = new System.Drawing.Size(64, 17);
            this.cbCollision.TabIndex = 2;
            this.cbCollision.Text = "Collision";
            this.cbCollision.UseVisualStyleBackColor = true;
            // 
            // cbInvertGamePadY
            // 
            this.cbInvertGamePadY.AutoSize = true;
            this.cbInvertGamePadY.Location = new System.Drawing.Point(6, 70);
            this.cbInvertGamePadY.Name = "cbInvertGamePadY";
            this.cbInvertGamePadY.Size = new System.Drawing.Size(113, 17);
            this.cbInvertGamePadY.TabIndex = 1;
            this.cbInvertGamePadY.Text = "Invert GamePad Y";
            this.cbInvertGamePadY.UseVisualStyleBackColor = true;
            // 
            // cbInvertMouseY
            // 
            this.cbInvertMouseY.AutoSize = true;
            this.cbInvertMouseY.Location = new System.Drawing.Point(6, 28);
            this.cbInvertMouseY.Name = "cbInvertMouseY";
            this.cbInvertMouseY.Size = new System.Drawing.Size(98, 17);
            this.cbInvertMouseY.TabIndex = 0;
            this.cbInvertMouseY.Text = "Invert Mouse Y";
            this.cbInvertMouseY.UseVisualStyleBackColor = true;
            // 
            // InputSettingsDialog
            // 
            this.AcceptButton = this.MyOKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.MyCancelButton;
            this.ClientSize = new System.Drawing.Size(305, 172);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.MyCancelButton);
            this.Controls.Add(this.MyOKButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputSettingsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Input Settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button MyOKButton;
        private System.Windows.Forms.Button MyCancelButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox cbInvertGamePadY;
        private System.Windows.Forms.CheckBox cbInvertMouseY;
        private System.Windows.Forms.CheckBox cbGravity;
        private System.Windows.Forms.CheckBox cbCollision;
    }
}