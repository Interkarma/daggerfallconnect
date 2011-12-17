// Project:         DaggerfallModelling
// Description:     Explore and export 3D models from Daggerfall.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
#endregion

namespace DaggerfallModelling.Dialogs
{

    /// <summary>
    /// Dialog to select model export path.
    /// </summary>
    public partial class BrowseExportModelDialog : Form
    {

        #region Class Variables

        private string outputPathValue = string.Empty;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets Arena2 path displayed in dialog.
        /// </summary>
        public string OutputPath
        {
            get { return outputPathValue; }
            set
            {
                outputPathValue = value;
                OutputPathTextBox.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets orientation displayed in dialog.
        /// </summary>
        public int Orientation
        {
            get { return OrientationComboBox.SelectedIndex; }
            set { OrientationComboBox.SelectedIndex = value; }
        }

        /// <summary>
        /// Gets or sets image format displayed in dialog.
        /// </summary>
        public int ImageFormat
        {
            get { return ImageFormatComboBox.SelectedIndex; }
            set { ImageFormatComboBox.SelectedIndex = value; }
        }

        /// <summary>
        /// Gets or sets model scale value.
        /// </summary>
        public float ModelScale
        {
            get { return float.Parse(ScaleModelValue.Text); }
            set { ScaleModelValue.Text = value.ToString(); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BrowseExportModelDialog()
        {
            InitializeComponent();
            OrientationComboBox.SelectedIndex = 2;
            ImageFormatComboBox.SelectedIndex = 2;
        }

        #endregion

        #region Private Methods

        private void MyOKButton_Click(object sender, EventArgs e)
        {
            if (TestOutputPath())
            {
                DialogResult = DialogResult.OK;
                outputPathValue = OutputPathTextBox.Text;
                Properties.Settings.Default.ScaleModelBeforeExport = decimal.Parse(ScaleModelValue.Text);
                Close();
            }
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = "Select output path for model export.";
            dlg.ShowNewFolderButton = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                OutputPath = dlg.SelectedPath;
            }
        }

        private bool TestOutputPath()
        {
            // Test path exists
            if (!Directory.Exists(OutputPathTextBox.Text))
            {
                MessageBox.Show("Path does not exist.", "Invalid Path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        #endregion

        #region Form Events

        private void ScaleModelValue_KeyPressed(object sender, KeyPressEventArgs e)
        {
            // Only allow 0-9 and decimal chars to be entered
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // Only allow one decimal point 
            if (e.KeyChar == '.' && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            } 
        }

        private void OnLoad(object sender, EventArgs e)
        {
            // Set scale value
            ScaleModelValue.Text = Properties.Settings.Default.ScaleModelBeforeExport.ToString();
        }

        #endregion

    }

}
