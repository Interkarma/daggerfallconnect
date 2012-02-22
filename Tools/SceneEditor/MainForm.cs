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
using DeepEngine.Utility;
#endregion

namespace SceneEditor
{

    public partial class MainForm : Form
    {

        #region Fields

        SceneDocument document;
        SceneDocumentProxy documentProxy;
        PropertyGrid propertyGrid;

        bool terrainEditMode = false;
        QuadTerrainProxy currentTerrainProxy = null;

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
            
            // Init terrain editor panel
            TerrainEditorPanel.Visible = false;
            ToggleTerrainEditorButton.Enabled = false;
            ToggleTerrainEditorButton.Checked = false;
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

            // Create default document
            CreateDefaultDocument();
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

            // Get selected proxy
            BaseEditorProxy proxy = DocumentTreeView.SelectedNode.Tag as BaseEditorProxy;
            
            // Enable/disable terrain editor
            if (proxy is QuadTerrainProxy)
            {
                ToggleTerrainEditorButton.Enabled = true;
                TerrainEditorPanel.Visible = true;
                ToggleTerrainEditorButton.Checked = true;
                terrainEditMode = true;
                currentTerrainProxy = (QuadTerrainProxy)proxy;
                (proxy as QuadTerrainProxy).Component.EnablePicking = true;
                
            }
            else
            {
                ToggleTerrainEditorButton.Enabled = false;
                TerrainEditorPanel.Visible = false;
                ToggleTerrainEditorButton.Checked = false;
                terrainEditMode = false;
                if (currentTerrainProxy != null)
                {
                    currentTerrainProxy.Component.EnablePicking = false;
                    currentTerrainProxy = null;
                }

            }
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

        /// <summary>
        /// Called whenever World Control ticks, before scene is presented.
        /// </summary>
        private void WorldControl_OnTick(object sender, EventArgs e)
        {
            if (currentTerrainProxy != null && terrainEditMode)
            {
                // Get current pointer intersection
                QuadTerrainComponent.TerrainIntersectionData pi = currentTerrainProxy.Component.PointerIntersection;
                if (pi.Distance != null)
                {
                    // Position cursor
                    terrainEditor1.SetCursorPosition(pi.MapPosition.X, pi.MapPosition.Y);
                }
                else
                {
                    // Clear cursor
                    terrainEditor1.ClearCursorPosition();
                }
            }
        }

        /// <summary>
        /// Called when mouse is clicked in world control.
        /// </summary>
        private void WorldControl_MouseDown(object sender, MouseEventArgs e)
        {
            // Start deformation
            if (e.Button == MouseButtons.Left &&
                currentTerrainProxy != null &&
                terrainEditMode)
            {
                QuadTerrainComponent.TerrainIntersectionData pi = currentTerrainProxy.Component.PointerIntersection;
                if (pi.Distance != null)
                {
                    terrainEditor1.BeginDeformUpDown(0);
                }
            }
        }

        /// <summary>
        /// Called when mouse moves in world control.
        /// </summary>
        private void WorldControl_MouseMove(object sender, MouseEventArgs e)
        {
            // Set current deformation
            if (e.Button == MouseButtons.Left &&
                currentTerrainProxy != null &&
                terrainEditMode &&
                terrainEditor1.DeformInProgress)
            {
                QuadTerrainComponent.TerrainIntersectionData pi = currentTerrainProxy.Component.PointerIntersection;
                if (pi.Distance != null)
                {
                    terrainEditor1.SetDeformUpDown(0);
                }
            }
        }

        /// <summary>
        /// Called when mouse is released.
        /// </summary>
        private void WorldControl_MouseUp(object sender, MouseEventArgs e)
        {
            // Start deformation
            if (e.Button == MouseButtons.Left &&
                currentTerrainProxy != null &&
                terrainEditMode)
            {
                QuadTerrainComponent.TerrainIntersectionData pi = currentTerrainProxy.Component.PointerIntersection;
                if (pi.Distance != null)
                {
                    terrainEditor1.EndDeformUpDown();
                }
            }
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
            else if (proxy is EntityProxy)
                imageKey = "Entity";
            else if (proxy is QuadTerrainProxy)
                imageKey = "QuadTerrain";
            else if (proxy is SphereProxy)
                imageKey = "Geometry";
            else if (proxy is LightProxy)
                imageKey = "Light";
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

            // Set node in proxt
            proxy.TreeNode = node;

            return node;
        }

        #endregion

        #region TerrainEditor Events

        /// <summary>
        /// Height map changed in terrain editor.
        /// </summary>
        private void TerrainEditor_OnHeightMapChanged(object sender, EventArgs e)
        {
            // Send new height map to terrain node
            if (currentTerrainProxy != null)
            {
                // Set height data
                currentTerrainProxy.Component.SetHeight(terrainEditor1.GetHeightMapData());
            }
        }

        /// <summary>
        /// Blend map changed in terrain editor.
        /// </summary>
        private void TerrainEditor_OnBlendMapChanged(object sender, EventArgs e)
        {
            // Send new height map to terrain node
            if (currentTerrainProxy != null)
            {
                // Set blend data
                currentTerrainProxy.Component.SetBlend(terrainEditor1.GetBlendMapData());
            }
        }

        #endregion

        #region Default Documents

        /// <summary>
        /// Builds a default scene document.
        /// </summary>
        private void CreateDefaultDocument()
        {
            // Lock stacks
            document.LockUndoRedo = true;

            // Set camera position
            document.EditorScene.Camera.Position = new Vector3(0, 2, 50);
            document.EditorScene.Camera.Update();

            // Create entity
            EntityProxy entityProxy = AddEntityProxy();

            // Add sphere primitive component
            SphereProxy sphereProxy = AddSphereProxy(entityProxy);
            sphereProxy.Position = new Vector3(0, 0.5f, 0);

            // Add quad terrain component
            QuadTerrainProxy terrainProxy = AddQuadTerrainComponentProxy(entityProxy);
            terrainProxy.Position = new Vector3(-512, 0, -512);
            terrainProxy.Scale = new Vector3(2, 0.08f, 2);
            terrainProxy.NormalStrength = 0.01f;

            // Add light component
            LightProxy lightProxy = AddLightProxy(entityProxy);
            lightProxy.LightType = LightProxy.LightProxyTypes.Directional;
            lightProxy.Direction = new Vector3(0.25f, -1.0f, 0f);

            // Expand nodes
            documentProxy.TreeNode.Expand();
            entityProxy.TreeNode.Expand();

            // Select terrain node
            DocumentTreeView.SelectedNode = terrainProxy.TreeNode;

            // Unlock stacks
            document.LockUndoRedo = false;
        }

        #endregion

        #region Document Building

        /// <summary>
        /// Creates a new entity proxy.
        /// </summary>
        private EntityProxy AddEntityProxy()
        {
            // Create new entity
            DynamicEntity entity = new DynamicEntity(document.EditorScene);

            // Create new entity proxy
            EntityProxy entityProxy = new EntityProxy(document, entity);

            // Add new entity proxy to tree view
            TreeNode entityNode = AddTreeNode(documentProxy.TreeNode, entityProxy);

            // Assign tree node to proxy
            entityProxy.TreeNode = entityNode;

            return entityProxy;
        }

        /// <summary>
        /// Creates a new quad terrain proxy.
        /// </summary>
        private QuadTerrainProxy AddQuadTerrainComponentProxy(EntityProxy parent)
        {
            // Create default textures
            Texture2D heightMap = new Texture2D(worldControl.GraphicsDevice, 512, 512, false, SurfaceFormat.Color);
            Texture2D blendMap = new Texture2D(worldControl.GraphicsDevice, 512, 512, false, SurfaceFormat.Color);

            // Create new quad terrain
            QuadTerrainComponent quadTerrain = new QuadTerrainComponent(worldControl.Core, heightMap, blendMap, 2);

            // Add to parent entity
            parent.Entity.Components.Add(quadTerrain);

            // Create new quad terrain proxy
            QuadTerrainProxy quadTerrainProxy = new QuadTerrainProxy(document, quadTerrain);

            // Add new quad terrain proxy to tree view
            TreeNode quadTerrainNode = AddTreeNode(parent.TreeNode, quadTerrainProxy);

            // Assign tree node to proxy
            quadTerrainProxy.TreeNode = quadTerrainNode;

            return quadTerrainProxy;
        }

        /// <summary>
        /// Creates a new sphere primitive proxy.
        /// </summary>
        private SphereProxy AddSphereProxy(EntityProxy parent)
        {
            SphereProxy sphere = new SphereProxy(document, parent.Entity);
            TreeNode node = AddTreeNode(parent.TreeNode, sphere);

            return sphere;
        }

        /// <summary>
        /// Creates a new light proxy.
        /// </summary>
        private LightProxy AddLightProxy(EntityProxy parent)
        {
            LightProxy light = new LightProxy(document, parent.Entity);
            TreeNode node = AddTreeNode(parent.TreeNode, light);

            return light;
        }

        #endregion

    }

}
