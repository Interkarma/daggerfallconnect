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
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
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

        const uint defaultModelId = 456;
        const string defaultBlockName = "MAGEAA13.RMB";

        SceneDocument document;
        SceneDocumentProxy documentProxy;
        PropertyGrid propertyGrid;

        bool terrainEditMode = false;
        QuadTerrainProxy currentTerrainProxy = null;

        int deformStartY;

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
            ToggleToolPaletteButton.Enabled = false;
            ToggleToolPaletteButton.Checked = false;
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
        /// Tree view has been clicked.
        /// </summary>
        private void DocumentTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // Select clicked node
            DocumentTreeView.SelectedNode = e.Node;
        }

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
                ToggleToolPaletteButton.Enabled = true;
                TerrainEditorPanel.Visible = true;
                ToggleToolPaletteButton.Checked = true;
                terrainEditMode = true;
                currentTerrainProxy = (QuadTerrainProxy)proxy;
                terrainEditor1.SetTerrain(currentTerrainProxy.Component, document);
                (proxy as QuadTerrainProxy).Component.EnablePicking = true;
            }
            else
            {
                ToggleToolPaletteButton.Enabled = false;
                TerrainEditorPanel.Visible = false;
                ToggleToolPaletteButton.Checked = false;
                terrainEditMode = false;
                if (currentTerrainProxy != null)
                {
                    currentTerrainProxy.Component.EnablePicking = false;
                    currentTerrainProxy = null;
                    terrainEditor1.ClearTerrain();
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
                    DeformStart(e.X, e.Y);
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
                terrainEditMode)
            {
                DeformContinue(e.X, e.Y);
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
                terrainEditor1.EndDeformUpDown();
            }
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

                // Message pump is no longer idle, so keep on ticking manually
                worldControl.Tick();
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
                currentTerrainProxy.Component.SetBlend(0, terrainEditor1.GetBlendMap0Data());
                currentTerrainProxy.Component.SetBlend(1, terrainEditor1.GetBlendMap1Data());
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

        /// <summary>
        /// Start deforming terrain.
        /// </summary>
        /// <param name="x">Mouse X.</param>
        /// <param name="y">Mouse Y.</param>
        private void DeformStart(int x, int y)
        {
            if (terrainEditor1.CurrentEditAction == UserControls.TerrainEditor.CursorEditAction.DeformUpDown)
            {
                deformStartY = y;
                terrainEditor1.BeginDeformUpDown(0);
            }
            else if (terrainEditor1.CurrentEditAction == UserControls.TerrainEditor.CursorEditAction.Paint)
            {
                terrainEditor1.PaintTerrain();
            }
        }

        /// <summary>
        /// Continue editing terrain.
        /// </summary>
        /// <param name="x">Mouse X.</param>
        /// <param name="y">Mouse Y.</param>
        private void DeformContinue(int x, int y)
        {
            if (terrainEditor1.CurrentEditAction == UserControls.TerrainEditor.CursorEditAction.DeformUpDown &&
                terrainEditor1.DeformInProgress)
            {
                float amount = (float)(deformStartY - y) * 0.002f;
                terrainEditor1.SetDeformUpDown(amount);
            }
            else if (terrainEditor1.CurrentEditAction == UserControls.TerrainEditor.CursorEditAction.Paint)
            {
                terrainEditor1.PaintTerrain();
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

            // Add model component
            DaggerfallModelProxy modelProxy = AddModelProxy(entityProxy, 456);

            // Add quad terrain component
            QuadTerrainProxy terrainProxy = AddQuadTerrainComponentProxy(entityProxy);
            terrainProxy.Position = new Vector3(-1024f, 0, -1024f);
            terrainProxy.Scale = new Vector3(4, 0.16f, 4);
            terrainProxy.TextureRepeat = 100f;
            terrainProxy.NormalStrength = 0.01f;

            // Add light component
            LightProxy lightProxy = AddLightProxy(entityProxy);
            lightProxy.LightType = LightProxy.LightProxyTypes.Directional;
            lightProxy.Direction = new Vector3(0.25f, -1.0f, 0f);

            // Expand nodes
            documentProxy.TreeNode.Expand();
            entityProxy.TreeNode.Expand();

            // Select terrain node
            //DocumentTreeView.SelectedNode = terrainProxy.TreeNode;

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
            // Create new quad terrain
            QuadTerrainComponent quadTerrain = new QuadTerrainComponent(worldControl.Core, QuadTerrainComponent.TerrainSize.Small);

            // Create new quad terrain proxy
            QuadTerrainProxy quadTerrainProxy = new QuadTerrainProxy(document, parent, quadTerrain);

            // Add new quad terrain proxy to tree view
            TreeNode quadTerrainNode = AddTreeNode(parent.TreeNode, quadTerrainProxy);

            // Assign tree node to proxy
            quadTerrainProxy.TreeNode = quadTerrainNode;

            return quadTerrainProxy;
        }

        /// <summary>
        /// Creates a new model component proxy.
        /// </summary>
        private DaggerfallModelProxy AddModelProxy(EntityProxy parent, uint id)
        {
            // Create new model
            DaggerfallModelComponent model = new DaggerfallModelComponent(worldControl.Core, id);

            // Create proxy for component
            DaggerfallModelProxy modelProxy = new DaggerfallModelProxy(document, parent, model);

            // Add new proxy to tree view
            TreeNode node = AddTreeNode(parent.TreeNode, modelProxy);

            return modelProxy;
        }

        /// <summary>
        /// Creates a new block component proxy.
        /// </summary>
        private DaggerfallBlockProxy AddBlockProxy(EntityProxy parent, string name)
        {
            // Create new block
            DaggerfallBlockComponent block = new DaggerfallBlockComponent(worldControl.Core);
            block.LoadBlock(defaultBlockName, MapsFile.DefaultClimateSettings, worldControl.Core.ActiveScene);

            // Create proxy for component
            DaggerfallBlockProxy blockProxy = new DaggerfallBlockProxy(document, parent, block);

            // Add new proxy to tree view
            TreeNode node = AddTreeNode(parent.TreeNode, blockProxy);

            return blockProxy;
        }

        /// <summary>
        /// Creates a new sphere primitive proxy.
        /// </summary>
        private SphereProxy AddSphereProxy(EntityProxy parent)
        {
            SphereProxy sphere = new SphereProxy(document, parent);
            TreeNode node = AddTreeNode(parent.TreeNode, sphere);

            return sphere;
        }

        /// <summary>
        /// Creates a new light proxy.
        /// </summary>
        private LightProxy AddLightProxy(EntityProxy parent)
        {
            LightProxy light = new LightProxy(document, parent);
            TreeNode node = AddTreeNode(parent.TreeNode, light);

            return light;
        }

        #endregion

        #region Scene Context Menu Events

        /// <summary>
        /// Called when context menu is opened.
        /// </summary>
        private void SceneContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            // Check if user has opened context menu on an entity node
            bool enableComponentMenus = false;
            BaseEditorProxy proxy = GetSelectedProxy();
            EntityProxy entity = GetSelectedEntity();
            if (entity != null)
                enableComponentMenus = true;

            // Disable/Enable component menus
            EnableComponentsMenu(enableComponentMenus);

            // Cannot delete scene document proxy
            if (proxy is SceneDocumentProxy)
                DeleteSceneObjectMenu.Enabled = false;
            else
                DeleteSceneObjectMenu.Enabled = true;
        }

        /// <summary>
        /// Called when an object is deleted.
        /// </summary>
        private void DeleteSceneObjectMenu_Click(object sender, EventArgs e)
        {
            // Get editor proxy
            BaseEditorProxy proxy = GetSelectedProxy();

            // Cannot delete scene proxy or null proxy
            if (proxy == null || proxy is SceneDocumentProxy)
                return;

            // Select parent node based on type
            TreeNode parentNode = null;
            if (proxy is EntityProxy)
                parentNode = documentProxy.TreeNode;
            else
                parentNode = proxy.TreeNode.Parent;

            // Change selected item to parent
            DocumentTreeView.SelectedNode = parentNode;

            // Delete selected item
            RemoveSceneItem(proxy);
        }

        /// <summary>
        /// Removes scene item from document and scene.
        /// </summary>
        /// <param name="proxy">Proxy to delete.</param>
        private void RemoveSceneItem(BaseEditorProxy proxy)
        {
            // Cannot delete scene proxy or null proxy
            if (proxy == null || proxy is SceneDocumentProxy)
                return;

            // Handle removing entity
            if (proxy is EntityProxy)
            {
                // Remove all child proxies
                foreach (TreeNode node in proxy.TreeNode.Nodes)
                {
                    (node.Tag as BaseEditorProxy).Remove();
                }
            }
            else
            {
                // Just remove this proxy
                proxy.Remove();
            }

            // Delete tree node
            DocumentTreeView.Nodes.Remove(proxy.TreeNode);
        }

        /// <summary>
        /// Gets selected editor proxy in scene view.
        /// </summary>
        /// <returns>Editor proxy.</returns>
        private BaseEditorProxy GetSelectedProxy()
        {
            // Get selected proxy
            BaseEditorProxy proxy = null;
            if (DocumentTreeView.SelectedNode != null)
                proxy = DocumentTreeView.SelectedNode.Tag as BaseEditorProxy;

            return proxy;
        }

        /// <summary>
        /// Gets selected entity in editor, either from directly
        ///  selecting entity or one of its child proxies.
        /// </summary>
        /// <returns></returns>
        private EntityProxy GetSelectedEntity()
        {
            // Get selected proxy
            BaseEditorProxy proxy = GetSelectedProxy();

            // Return entity directly
            if (proxy is EntityProxy)
                return proxy as EntityProxy;

            // Return entity from component
            if (proxy is BaseComponentProxy)
            {
                return proxy.EntityProxy;
            }

            return null;
        }

        /// <summary>
        /// Enable or disable components context menu.
        /// </summary>
        /// <param name="enable">Enable or disable flag.</param>
        private void EnableComponentsMenu(bool enable)
        {
            AddDaggerfallModelMenuItem.Enabled = enable;
            AddDaggerfallBlockMenuItem.Enabled = enable;
            AddCubeMenuItem.Enabled = enable;
            AddSphereMenuItem.Enabled = enable;
            AddLightMenuItem.Enabled = enable;
            AddQuadTerrainMenuItem.Enabled = enable;
        }

        /// <summary>
        /// Called when a new entity is requested.
        /// </summary>
        private void AddEntityMenuItem_Click(object sender, EventArgs e)
        {
            // Add entity
            EntityProxy entity = AddEntityProxy();
        }

        /// <summary>
        /// Called when a new Daggerfall model is requested.
        /// </summary>
        private void AddDaggerfallModelMenuItem_Click(object sender, EventArgs e)
        {
            // Get selected entity
            EntityProxy entity = GetSelectedEntity();
            if (entity == null)
                return;

            // Add model
            DaggerfallModelProxy modelProxy = AddModelProxy(entity, defaultModelId);
        }

        /// <summary>
        /// Called when a new Daggerfall block is requested.
        /// </summary>
        private void AddDaggerfallBlockMenuItem_Click(object sender, EventArgs e)
        {
            // Get selected entity
            EntityProxy entity = GetSelectedEntity();
            if (entity == null)
                return;

            // Add block
            DaggerfallBlockProxy blockProxy = AddBlockProxy(entity, defaultBlockName);
        }

        /// <summary>
        /// Called when a new terrain is requested.
        /// </summary>
        private void AddQuadTerrainMenuItem_Click(object sender, EventArgs e)
        {
            // Get selected entity
            EntityProxy entity = GetSelectedEntity();
            if (entity == null)
                return;

            // Add quad terrain
            QuadTerrainProxy terrainProxy = AddQuadTerrainComponentProxy(entity);
        }

        /// <summary>
        /// Called when a new light is requested.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddLightMenuItem_Click(object sender, EventArgs e)
        {
            // Get selected entity
            EntityProxy entity = GetSelectedEntity();
            if (entity == null)
                return;

            // Add light
            LightProxy lightProxy = AddLightProxy(entity);
        }

        #endregion

    }

}
