// Project:         DaggerfallModelling
// Description:     Explore and export 3D models from Daggerfall.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Imports

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

    }

}
