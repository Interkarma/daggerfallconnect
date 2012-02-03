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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DeepEngine.Player;
using DeepEngine.World;
using DeepEngine.Components;
using SceneEditor.Documents;
using SceneEditor.Proxies;
#endregion

namespace SceneEditor
{

    public partial class MainForm : Form
    {

        #region Fields

        SceneDocument document;
        SceneDocumentProxy documentProxy;
        PropertyGrid propertyGrid;

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
            // Create document
            document = new SceneDocument(worldControl.Core);

            // Create standard proxies
            documentProxy = new SceneDocumentProxy(document);

            // Add new document to tree view
            DocumentTreeView.Nodes.Clear();
            TreeNode sceneNode = AddTreeNode(null, documentProxy);
            sceneNode.Expand();

            // Assign nodes to proxies
            documentProxy.TreeNode = sceneNode;

            // Subscribe events
            document.OnPushUndo += new EventHandler(Document_OnPushUndo);

            // Update toolbars
            UpdateUndoRedoToolbarItems();
        }

        #endregion

        #region Scene Document Events

        /// <summary>
        /// Called when a property is pushed onto the undo stack.
        /// </summary>
        private void Document_OnPushUndo(object sender, EventArgs e)
        {
            UpdateUndoRedoToolbarItems();
        }

        #endregion

        #region Property Grid

        /// <summary>
        /// Updates property grid based on selection.
        /// </summary>
        private void UpdatePropertyGrid()
        {
            // If nothing selected attach properties to scene environment
            if (DocumentTreeView.SelectedNode == null)
            {
                propertyGrid.SelectedObject = documentProxy;
                return;
            }
            else
            {
                propertyGrid.SelectedObject = DocumentTreeView.SelectedNode.Tag as BaseEditorProxy;
            }
        }

        #endregion

        #region Undo-Redo

        /// <summary>
        /// Pop last undo operation from stack and restore value.
        /// </summary>
        private void UndoButton_Click(object sender, EventArgs e)
        {
            document.PopUndo();
            UpdateUndoRedoToolbarItems();
            propertyGrid.Refresh();
        }

        /// <summary>
        /// Pop last redo operation from stack and restore value.
        /// </summary>
        private void RedoButton_Click(object sender, EventArgs e)
        {
            document.PopRedo();
            UpdateUndoRedoToolbarItems();
            propertyGrid.Refresh();
        }

        #endregion

        #region SceneTreeView Events

        /// <summary>
        /// A tree view item has been selected.
        /// </summary>
        private void SceneTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdatePropertyGrid();
        }

        #endregion

        #region WorldControl Events

        /// <summary>
        /// World Control is now ready to use.
        /// </summary>
        private void WorldControl_InitializeCompleted(object sender, EventArgs e)
        {
            // Create scene document
            NewSceneDocument();
            UpdatePropertyGrid();

            // Set core to render new scene
            worldControl.Core.ActiveScene = document.EditorScene;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates enabled/disabled state of undo and redo toolbar buttons.
        /// </summary>
        private void UpdateUndoRedoToolbarItems()
        {
            UndoButton.Enabled = (document.UndoCount > 0) ? true : false;
            RedoButton.Enabled = (document.RedoCount > 0) ? true : false;
        }

        /// <summary>
        /// Adds a proxy to the document tree.
        /// </summary>
        /// <param name="parent">Parent tree node.</param>
        /// <param name="proxy">Proxy to resource.</param>
        /// <returns>Tree node added.</returns>
        private TreeNode AddTreeNode(TreeNode parent, BaseEditorProxy proxy)
        {
            // Determine image to use
            string imageKey;
            if (proxy is SceneDocumentProxy)
                imageKey = "Scene";
            else
                imageKey = "Unknown";

            // Create new tree node
            TreeNode node = new TreeNode();
            node.Text = proxy.Name;
            node.ImageKey = imageKey;
            node.SelectedImageKey = imageKey;
            node.Tag = proxy;

            // Add new tree node
            if (parent == null)
                DocumentTreeView.Nodes.Add(node);
            else
                parent.Nodes.Add(node);

            return node;
        }

        #endregion

        #region Toolbar Events

        /*
        /// <summary>
        /// Add a new world entity.
        /// </summary>
        private void AddWorldEntityButton_Click(object sender, EventArgs e)
        {
            // Create new entity
            WorldEntity entity = new WorldEntity(document.EditorScene);

            // Create new entity proxy
            WorldEntityProxy entityProxy = new WorldEntityProxy(document, entity);

            // Add new world entity proxy to tree view
            TreeNode entityNode = AddTreeNode(documentProxy.TreeNode, entityProxy);

            // Assign tree node to proxy
            entityProxy.TreeNode = entityNode;
        }
        */

        #endregion

    }

}
