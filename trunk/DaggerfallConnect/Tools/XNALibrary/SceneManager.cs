// Project:         XNALibrary
// Description:     Simple XNA game library for DaggerfallConnect.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace XNALibrary
{

    /// <summary>
    /// Helper class for loading Daggerfall content and constructing a scene graph.
    /// </summary>
    public class SceneManager
    {

        #region Class Variables

        // Daggerfall content managers
        private TextureManager textureManager = null;
        private ModelManager modelManager = null;
        private BlockManager blockManager = null;
        private MapsFile mapsFile = null;

        // Scene
        private SceneNode root;

        // Ground height
        private int groundHeight = -1;

        #endregion

        #region Properties

        /// <summary>
        /// Gets root scene node.
        /// </summary>
        public SceneNode Root
        {
            get { return root; }
        }

        /// <summary>
        /// Gets or sets TextureManager for content loading.
        /// </summary>
        public TextureManager TextureManager
        {
            get { return textureManager; }
            set { textureManager = value; }
        }

        /// <summary>
        /// Gets or sets ModelManager for content loading.
        ///  For best results model caching should be enabled in ModelManager.
        /// </summary>
        public ModelManager ModelManager
        {
            get { return modelManager; }
            set { modelManager = value; }
        }

        /// <summary>
        /// Gets or sets BlockManager for content loading.
        /// </summary>
        public BlockManager BlockManager
        {
            get { return blockManager; }
            set { blockManager = value; }
        }

        /// <summary>
        /// Gets or sets MapsFile for content loading.
        /// </summary>
        public MapsFile MapManager
        {
            get { return mapsFile; }
            set { mapsFile = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor. Takes references to content managers to enable content loading.
        ///  Note that content loading will throw an exception if attempting to load
        ///  content with a null manager.
        /// <param name="textureManager">TextureManager reference or null.</param>
        /// <param name="modelManager">ModelManager reference or null.</param>
        /// <param name="blockManager">BlockManager reference or null.</param>
        /// <param name="mapsManager">MapsFile reference or null.</param>
        /// </summary>
        public SceneManager(TextureManager textureManager,
            ModelManager modelManager,
            BlockManager blockManager,
            MapsFile mapsManager)
        {
            // Create default scene
            ResetScene();

            // Store content managers
            this.textureManager = textureManager;
            this.modelManager = modelManager;
            this.blockManager = blockManager;
            this.mapsFile = mapsManager;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reset entire scene back to empty root node.
        /// </summary>
        public void ResetScene()
        {
            root = new SceneNode();
            root.DrawBoundsColor = Color.Red;
        }

        /// <summary>
        /// Prepare scene for rendering.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last frame.</param>
        public void Update(TimeSpan elapsedTime)
        {
            // Update nodes
            UpdateNode(root, Matrix.Identity);
        }

        /// <summary>
        /// Adds a model node to the scene.
        /// </summary>
        /// <param name="parent">Parent node to receive model node child.</param>
        /// <param name="id">ModelID to load.</param>
        /// <returns>ModelNode.</returns>
        public ModelNode AddModelNode(SceneNode parent, uint id)
        {
            // Use root node if no parent specified
            if (parent == null)
                parent = root;

            // Create scene node
            ModelNode node = new ModelNode(LoadDaggerfallModel(id));
            parent.Add(node);

            return node;
        }

        /// <summary>
        /// Adds a billboard node to the scene.
        /// </summary>
        /// <param name="parent">Parent node to receive billboard node child.</param>
        /// <param name="flatItem">BlockManager.FlatItem.</param>
        /// <returns>BillboardNode.</returns>
        public BillboardNode AddBillboardNode(SceneNode parent, BlockManager.FlatItem flatItem)
        {
            // Use root node if no parent specified
            if (parent == null)
                parent = root;

            // Load flat item
            flatItem = LoadDaggerfallFlat(flatItem);

            // Create billboard node
            BillboardNode node = new BillboardNode(flatItem);
            node.LocalBounds = flatItem.BoundingSphere;
            parent.Add(node);

            return node;
        }

        /// <summary>
        /// Adds a block node by name.
        /// </summary>
        /// <param name="parent">Parent node.</param>
        /// <param name="name">Name of block.</param>
        /// <returns>SceneNode.</returns>
        public SceneNode AddBlockNode(SceneNode parent, string name)
        {
            return AddBlockNode(parent, blockManager.LoadBlock(name));
        }

        /// <summary>
        /// Adds a block node to the scene. 
        /// </summary>
        /// <param name="parent">Parent node.</param>
        /// <param name="name">Block data.</param>
        /// <returns>SceneNode.</returns>
        public SceneNode AddBlockNode(SceneNode parent, BlockManager.BlockData block)
        {
            // Validate
            if (block == null)
                return null;

            // Use root node if no parent specified
            if (parent == null)
                parent = root;

            // Build block node
            switch (block.DFBlock.Type)
            {
                case DFBlock.BlockTypes.Rmb:
                    return BuildRMBNode(parent, block);
                case DFBlock.BlockTypes.Rdb:
                    return BuildRDBNode(parent, block);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Adds an exterior location node to the scene.
        /// </summary>
        /// <param name="parent">Parent node.</param>
        /// <param name="dfLocation">Location data.</param>
        /// <returns>SceneNode.</returns>
        public SceneNode AddExteriorLocationNode(SceneNode parent, ref DFLocation dfLocation)
        {
            // Validate
            if (mapsFile == null)
                throw new Exception("MapManager is not set.");

            // Use root node if no parent specified
            if (parent == null)
                parent = root;

            // Create location node
            SceneNode locationNode = new SceneNode();

            // Get dimensions of exterior location array
            int width = dfLocation.Exterior.ExteriorData.Width;
            int height = dfLocation.Exterior.ExteriorData.Height;

            // Build exterior node from blocks
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Get final block name
                    string name = blockManager.BlocksFile.CheckName(
                        mapsFile.GetRmbBlockName(ref dfLocation, x, y));

                    // Create block position data
                    SceneNode blockNode = AddBlockNode(locationNode, name);
                    blockNode.Matrix *= Matrix.CreateTranslation(
                        new Vector3(x * BlockManager.RMBSide, 0, -(y * BlockManager.RMBSide)));

                    // Add to location node
                    locationNode.Add(blockNode);
                }
            }

            // Add location node to scene
            parent.Add(locationNode);

            return locationNode;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Builds an exterior node with all child nodes.
        /// </summary>
        /// <param name="parent">Parent node.</param>
        /// <param name="block">BlockManager.BlockData.</param>
        /// <returns>SceneNode.</returns>
        private SceneNode BuildRMBNode(SceneNode parent, BlockManager.BlockData block)
        {
            // Create block parent node
            SceneNode blockNode = new SceneNode();

            // Add ground plane node
            blockManager.BuildRmbGroundPlane(textureManager, ref block);
            GroundPlaneNode groundNode = new GroundPlaneNode(
                block.GroundPlaneVertexBuffer,
                block.GroundPlaneVertices.Length / 3);
            blockNode.Add(groundNode);

            // Add model nodes
            ModelNode modelNode;
            foreach (var model in block.Models)
            {
                modelNode = AddModelNode(blockNode, model.ModelId);
                modelNode.Matrix = model.Matrix;
            }

            // Add billboard nodes
            BillboardNode billboardNode;
            foreach (var flat in block.Flats)
            {
                billboardNode = AddBillboardNode(blockNode, flat);
                billboardNode.Matrix = Matrix.Identity;
                billboardNode.DrawBoundsColor = Color.Green;
            }

            // Add block node to scene
            parent.Add(blockNode);

            return blockNode;
        }

        /// <summary>
        /// Builds a dungeon node with all child nodes.
        /// </summary>
        /// <param name="parent">Parent node.</param>
        /// <param name="block">BlockManager.BlockData.</param>
        /// <returns>SceneNode.</returns>
        private SceneNode BuildRDBNode(SceneNode parent, BlockManager.BlockData block)
        {
            // Create block parent node
            SceneNode blockNode = new SceneNode();

            // Add model nodes
            ModelNode modelNode;
            foreach (var model in block.Models)
            {
                modelNode = AddModelNode(blockNode, model.ModelId);
                modelNode.Matrix = model.Matrix;
            }

            // Add billboard nodes
            BillboardNode billboardNode;
            foreach (var flat in block.Flats)
            {
                billboardNode = AddBillboardNode(blockNode, flat);
                billboardNode.Matrix = Matrix.Identity;
                billboardNode.DrawBoundsColor = Color.Green;
            }

            // Add block node to scene
            parent.Add(blockNode);

            return blockNode;
        }

        #endregion

        #region Content Loading

        /// <summary>
        /// Loads a Daggerfall texture.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <param name="flags">TextureManager.TextureCreateFlags.</param>
        /// <returns>TextureManager key.</returns>
        private int LoadDaggerfallTexture(int archive, int record, TextureManager.TextureCreateFlags flags)
        {
            // Cannot proceed if TextureManager null
            if (textureManager == null)
                throw new Exception("TextureManager is not set.");

            return textureManager.LoadTexture(archive, record, flags);
        }

        /// <summary>
        /// Loads a Daggerfall flat (billboard).
        /// </summary>
        /// <param name="flatItem">BlockManager.FlatItem.</param>
        /// <returns>BlockManager.FlatItem.</returns>
        private BlockManager.FlatItem LoadDaggerfallFlat(BlockManager.FlatItem flatItem)
        {
            // Set climate ground flats archive if this is a scenery flat
            if (flatItem.FlatType == BlockManager.FlatType.Scenery)
            {
                // Just use temperate (504) if no location set
                //if (currentLocation == null)
                    flatItem.TextureArchive = 504;
                //else
                //    info.TextureArchive = currentLocation.Value.GroundFlatsArchive;
            }

            // Load texture
            flatItem.TextureKey = LoadDaggerfallTexture(
                flatItem.TextureArchive,
                flatItem.TextureRecord,
                TextureManager.TextureCreateFlags.Dilate |
                TextureManager.TextureCreateFlags.PreMultiplyAlpha);

            // Get dimensions and scale of this texture image
            // We do this as TextureManager may have just pulled texture key from cache.
            // without loading file. We need the following information to create bounds.
            string path = Path.Combine(TextureManager.Arena2Path, TextureFile.IndexToFileName(flatItem.TextureArchive));
            System.Drawing.Size size = TextureFile.QuickSize(path, flatItem.TextureRecord);
            System.Drawing.Size scale = TextureFile.QuickScale(path, flatItem.TextureRecord);
            flatItem.Size.X = size.Width;
            flatItem.Size.Y = size.Height;

            // Apply scale
            if (flatItem.BlockType == DFBlock.BlockTypes.Rdb && flatItem.TextureArchive > 499)
            {
                // Foliage (TEXTURE.500 and up) do not seem to use scaling
                // in dungeons. Disable scaling for now.
                flatItem.Size.X = size.Width;
                flatItem.Size.Y = size.Height;
            }
            else
            {
                // Scale billboard
                int xChange = (int)(size.Width * (scale.Width / 256.0f));
                int yChange = (int)(size.Height * (scale.Height / 256.0f));
                flatItem.Size.X = size.Width + xChange;
                flatItem.Size.Y = size.Height + yChange;
            }

            // Set origin of outdoor flats to centre-bottom.
            // Sink them just a little so they don't look too floaty.
            if (flatItem.BlockType == DFBlock.BlockTypes.Rmb)
            {
                flatItem.Origin.Y = (groundHeight + flatItem.Size.Y / 2) - 4;
            }

            // Set bounding sphere
            flatItem.BoundingSphere.Center.X = flatItem.Origin.X + flatItem.Position.X;
            flatItem.BoundingSphere.Center.Y = flatItem.Origin.Y + flatItem.Position.Y;
            flatItem.BoundingSphere.Center.Z = flatItem.Origin.Z + flatItem.Position.Z;
            if (flatItem.Size.X > flatItem.Size.Y)
                flatItem.BoundingSphere.Radius = flatItem.Size.X / 2;
            else
                flatItem.BoundingSphere.Radius = flatItem.Size.Y / 2;

            return flatItem;
        }

        /// <summary>
        /// Loads a Daggerfall model and any textures required.
        /// </summary>
        /// <param name="id">ID of model.</param>
        /// <returns>ModelManager.ModelData.</returns>
        private ModelManager.ModelData LoadDaggerfallModel(uint id)
        {
            // Cannot proceed if ModelManager null
            if (modelManager == null)
                throw new Exception("ModelManager is not set.");

            // Load model and textures
            ModelManager.ModelData model = modelManager.GetModelData(id);
            for (int i = 0; i < model.SubMeshes.Length; i++)
            {
                // Load texture
                model.SubMeshes[i].TextureKey = LoadDaggerfallTexture(
                    model.DFMesh.SubMeshes[i].TextureArchive,
                    model.DFMesh.SubMeshes[i].TextureRecord,
                    TextureManager.TextureCreateFlags.ApplyClimate |
                    TextureManager.TextureCreateFlags.MipMaps |
                    TextureManager.TextureCreateFlags.PowerOfTwo);
            }

            return model;
        }

        /// <summary>
        /// Loads a Daggerfall block.
        /// </summary>
        /// <param name="name">Block name.</param>
        /// <returns>BlockManager.BlockData.</returns>
        private BlockManager.BlockData LoadDaggerfallBlock(string name)
        {
            // Cannot proceed if BlockManager null
            if (blockManager == null)
                throw new Exception("BlockManager is not set.");

            // Clear block cache
            blockManager.ClearBlocks();

            // Load block
            return blockManager.LoadBlock(name);
        }

        #endregion

        #region Updating

        /// <summary>
        /// Update node.
        /// </summary>
        /// <param name="node">SceneNode.</param>
        /// <param name="matrix">Cumulative Matrix.</param>
        /// <returns>Transformed and merged BoundingSphere.</returns>
        private BoundingSphere UpdateNode(SceneNode node, Matrix matrix)
        {
            // Transform bounds
            BoundingSphere bounds = node.LocalBounds;
            bounds.Center = Vector3.Transform(bounds.Center, node.Matrix * matrix);

            // Update child nodes
            foreach (SceneNode child in node.Children)
            {
                bounds = BoundingSphere.CreateMerged(
                    bounds,
                    UpdateNode(child, node.Matrix * matrix));
            }

            // Store transformed bounds
            node.TransformedBounds = bounds;

            // TODO: Get distance to camera

            // TODO: Run actions

            return bounds;
        }

        #endregion

    }

}
