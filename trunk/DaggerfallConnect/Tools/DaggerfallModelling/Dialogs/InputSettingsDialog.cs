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
    /// Dialog to manage input settings.
    /// </summary>
    public partial class InputSettingsDialog : Form
    {

        #region Class Variables
        #endregion

        #region Properties

        public bool InvertMouseY
        {
            get { return cbInvertMouseY.Checked; }
            set { cbInvertMouseY.Checked = value; }
        }

        public bool InvertGamePadY
        {
            get { return cbInvertGamePadY.Checked; }
            set { cbInvertGamePadY.Checked = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public InputSettingsDialog()
        {
            InitializeComponent();
        }

        #endregion

        #region Private Methods

        private void MyOKButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        #endregion

    }

}
