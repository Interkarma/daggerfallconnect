// Project:         Deep Engine
// Description:     3D game engine for Ruins of Hill Deep and Daggerfall Workshop projects.
// Copyright:       Copyright (C) 2012 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SceneEditor.Documents;
#endregion

namespace SceneEditor
{

    public partial class MainForm : Form
    {

        #region Fields

        PropertyGrid propertyGrid;
        SceneDocument document;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            // Create property grid
            propertyGrid = new PropertyGrid();
            propertyGrid.Dock = DockStyle.Fill;
            PropertiesPanel.Controls.Add(propertyGrid);
        }

        #endregion

        #region Document Management

        /// <summary>
        /// Adds a new scene document.
        /// </summary>
        private void NewSceneDocument()
        {
            document = new SceneDocument(worldControl.Core);
        }

        #endregion

        #region Property Grid

        /// <summary>
        /// Updates property grid based on selection.
        /// </summary>
        private void UpdatePropertyGrid()
        {
            // If nothing selected attach properties to scene document
            if (sceneTreeView.SelectedNode == null)
            {
                propertyGrid.SelectedObject = document;
                return;
            }
        }

        #endregion

        #region SceneTreeView Events

        /// <summary>
        /// A tree view item has been selected.
        /// </summary>
        private void SceneTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
        }

        #endregion

        #region WorldControl Events

        /// <summary>
        /// World Control is now ready to use.
        /// </summary>
        private void WorldControl_InitializeCompleted(object sender, EventArgs e)
        {
            NewSceneDocument();
            UpdatePropertyGrid();
        }

        #endregion

    }

}
