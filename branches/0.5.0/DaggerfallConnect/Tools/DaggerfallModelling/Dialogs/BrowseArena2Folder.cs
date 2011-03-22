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
    /// Dialog to select ARENA2 path.
    /// </summary>
    public partial class BrowseArena2Folder : Form
    {

        #region Class Variables

        private string Arena2PathValue = string.Empty;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets Arena2 path displayed in dialog.
        /// </summary>
        public string Arena2Path
        {
            get
            {
                return Arena2PathValue;
            }

            set
            {
                Arena2PathValue = value;
                Arena2PathTextBox.Text = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BrowseArena2Folder()
        {
            InitializeComponent();
        }

        #endregion

        #region Private Methods

        private void MyOKButton_Click(object sender, EventArgs e)
        {
            if (TestArena2Path())
            {
                DialogResult = DialogResult.OK;
                Arena2PathValue = Arena2PathTextBox.Text;
                Close();
            }
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = "Select the ARENA2 folder of your DAGGERFALL installation.";
            dlg.ShowNewFolderButton = false;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Arena2Path = dlg.SelectedPath;
            }
        }

        private bool TestArena2Path()
        {
            // Test path exists
            if (!Directory.Exists(Arena2PathTextBox.Text))
            {
                MessageBox.Show("Path does not exist.", "Invalid Path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Test some key files exist
            string arch3d = Path.Combine(Arena2PathTextBox.Text, "ARCH3D.BSA");
            string blocks = Path.Combine(Arena2PathTextBox.Text, "BLOCKS.BSA");
            string maps = Path.Combine(Arena2PathTextBox.Text, "MAPS.BSA");
            if (!File.Exists(arch3d) ||
                !File.Exists(blocks) ||
                !File.Exists(maps))
            {
                MessageBox.Show("Not a valid Arena2 folder.", "Invalid Path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        #endregion

    }

}
