// Project:         Deep Engine
// Description:     3D game engine for Ruins of Hill Deep and Daggerfall Workshop projects.
// Copyright:       Copyright (C) 2012 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Imports
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
#endregion

namespace ProjectEditor.Dialogs
{

    public partial class NewProjectDialog : Form
    {

        #region Properties

        /// <summary>
        /// Gets or sets project name.
        /// </summary>
        string ProjectName
        {
            get { return ProjectNameTextBox.Text; }
            set { ProjectNameTextBox.Text = value; }
        }

        /// <summary>
        /// Gets or sets project parent path.
        /// </summary>
        string ProjectPath
        {
            get { return ProjectPathTextBox.Text; }
            set { ProjectPathTextBox.Text = value; }
        }

        /// <summary>
        /// Gets combined projectpath\projectname path.
        /// </summary>
        string ProjectFolder
        {
            get { return Path.Combine(ProjectPath, ProjectName); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public NewProjectDialog()
        {
            InitializeComponent();
        }

        #endregion

        #region Button Events

        /// <summary>
        /// User clicked to browse output folder.
        /// </summary>
        private void BrowseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ProjectPath = dlg.SelectedPath;
            }
        }

        /// <summary>
        /// User clicked to create project
        /// </summary>
        private void CreateProjectButton_Click(object sender, EventArgs e)
        {
            // Check name not empty
            if (string.IsNullOrEmpty(ProjectName))
            {
                ShowErrorMessage("Project name can not be empty.");
                return;
            }

            // Check path not empty
            if (string.IsNullOrEmpty(ProjectPath))
            {
                ShowErrorMessage("Project path can not be empty.");
                return;
            }

            // Check project path does not exist
            if (Directory.Exists(ProjectFolder))
            {
                ShowErrorMessage(string.Format("Project folder '{0}' already exists.", ProjectFolder));
                return;
            }

            // Create project path
            try
            {
                Directory.CreateDirectory(ProjectFolder);
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
                return;
            }

            // Set result and close out
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Shows an error message to the user.
        /// </summary>
        /// <param name="message">Error string.</param>
        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        #endregion

    }

}
