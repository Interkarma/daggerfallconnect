// Project:         DaggerfallImaging
// Description:     Explore and export bitmaps from Daggerfall.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DaggerfallImaging2.Dialogs
{
    public partial class BrowseArena2Folder : Form
    {
        private string Arena2PathValue = string.Empty;
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

        public BrowseArena2Folder()
        {
            InitializeComponent();
        }

        private void MyOKButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Arena2PathValue = Arena2PathTextBox.Text;
            Close();
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
    }
}
