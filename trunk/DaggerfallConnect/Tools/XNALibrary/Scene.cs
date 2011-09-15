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
    /// Manages a scene graph of SceneNode objects.
    /// </summary>
    public class Scene
    {

        #region Class Variables

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

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public Scene()
        {
            // Create default scene
            ResetScene();
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
        /// Adds a node to the scene.
        /// </summary>
        /// <param name="parent">Parent node, or NULL for root.</param>
        /// <param name="node">SceneNode to add to parent.</param>
        public void AddNode(SceneNode parent, SceneNode node)
        {
            if (parent == null)
                root.Add(node);
            else
                parent.Add(node);
        }

        /// <summary>
        /// Adds a model node to the scene.
        /// </summary>
        /// <param name="parent">Parent node.</param>
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
        /// <param name="dfLocation">Optional DFLocation for this billboard's climate swap. Can be null.</param>
        /// <returns>BillboardNode.</returns>
        public BillboardNode AddBillboardNode(SceneNode parent, BlockManager.FlatItem flatItem, DFLocation? dfLocation)
        {
            // Use root node if no parent specified
            if (parent == null)
                parent = root;

            // Set climate ground flats archive if this is a scenery flat
            if (flatItem.FlatType == BlockManager.FlatType.Scenery)
            {
                // Just use temperate (504) if no location set
                if (dfLocation == null)
                    flatItem.TextureArchive = 504;
                else
                    flatItem.TextureArchive = 504;
            }

            // Load flat item
            flatItem = LoadDaggerfallFlat(flatItem);

            // Create billboard node
            BillboardNode node = new BillboardNode();
            //BillboardNode node = new BillboardNode(flatItem);
            node.LocalBounds = flatItem.BoundingSphere;
            parent.Add(node);

            return node;
        }

        /// <summary>
        /// Adds a block node by name.
        /// </summary>
        /// <param name="parent">Parent node.</param>
        /// <param name="name">Name of block.</param>
        /// <param name="dfLocation">Optional DFLocation for this block's climate swap. Can be null.</param>
        /// <returns>SceneNode.</returns>
        public SceneNode AddBlockNode(SceneNode parent, string name, DFLocation? dfLocation)
        {
            return null;
            //return AddBlockNode(parent, contentHelper.BlockManager.LoadBlock(name), dfLocation);
        }

        /// <summary>
        /// Adds a block node to the scene. 
        /// </summary>
        /// <param name="parent">Parent node.</param>
        /// <param name="name">Block data.</param>
        /// <param name="dfLocation">Optional DFLocation for this block's climate swap. Can be null.</param>
        /// <returns>SceneNode.</returns>
        public SceneNode AddBlockNode(SceneNode parent, BlockManager.BlockData block, DFLocation? dfLocation)
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
                    return BuildRMBNode(parent, block, dfLocation);
                case DFBlock.BlockTypes.Rdb:
                    return BuildRDBNode(parent, block, dfLocation);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Adds an exterior location node to the scene.
        /// </summary>
        /// <param name="parent">Parent node.</param>
        /// <param name="location">Location data.</param>
        /// <returns>SceneNode.</returns>
        public SceneNode AddExteriorLocationNode(SceneNode parent, ref DFLocation location)
        {
            // Cannot proceed if content helper is null
            //if (contentHelper == null)
            //    throw new Exception("ContentHelper is not set.");

            // Use root node if no parent specified
            if (parent == null)
                parent = root;

            // Create location node
            SceneNode locationNode = new SceneNode();

            // Get dimensions of exterior location array
            int width = location.Exterior.ExteriorData.Width;
            int height = location.Exterior.ExteriorData.Height;

            // Build exterior node from blocks
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Get final block name
                    string name = "UNCOMMENT BELOW";
                    //string name = contentHelper.BlockManager.BlocksFile.CheckName(
                    //    contentHelper.MapManager.MapsFile.GetRmbBlockName(ref location, x, y));

                    // Create block position data
                    SceneNode blockNode = AddBlockNode(locationNode, name, location);
                    blockNode.Position = new Vector3(
                        x * BlockManager.RMBSide,
                        0f,
                        -(y * BlockManager.RMBSide));
                    //blockNode.Matrix *= Matrix.CreateTranslation(
                    //    new Vector3(x * BlockManager.RMBSide, 0f, -(y * BlockManager.RMBSide)));

                    // Add block to location node
                    locationNode.Add(blockNode);
                }
            }

            // Add location node to scene
            parent.Add(locationNode);

            return locationNode;
        }

        /// <summary>
        /// Adds a dungeon location node to the scene.
        /// </summary>
        /// <param name="parent">Parent node.</param>
        /// <param name="location">Location data.</param>
        /// <returns>SceneNode.</returns>
        public SceneNode AddDungeonLocationNode(SceneNode parent, ref DFLocation location)
        {
            // Cannot proceed if content helper is null
            //if (contentHelper == null)
            //    throw new Exception("ContentHelper is not set.");

            // Use root node if no parent specified
            if (parent == null)
                parent = root;

            // Create location node
            SceneNode locationNode = new SceneNode();

            // Create dungeon layout
            foreach (var block in location.Dungeon.Blocks)
            {
                // TODO: Handle duplicate block coordinates (e.g. Orsinium)

                // Create block position data
                SceneNode blockNode = AddBlockNode(locationNode, block.BlockName, null);
                blockNode.Position = new Vector3(
                    block.X * BlockManager.RDBSide,
                    0f,
                    -(block.Z * BlockManager.RDBSide));
                //blockNode.Matrix *= Matrix.CreateTranslation(
                //        new Vector3(block.X * BlockManager.RDBSide, 0f, -(block.Z * BlockManager.RDBSide)));
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
        /// <param name="dfLocation">Optional DFLocation for this block's climate swap. Can be null.</param>
        /// <returns>SceneNode.</returns>
        private SceneNode BuildRMBNode(SceneNode parent, BlockManager.BlockData block, DFLocation? dfLocation)
        {
            // Create block parent node
            SceneNode blockNode = new SceneNode();

            // Add ground plane node
            //contentHelper.BlockManager.BuildRmbGroundPlane(contentHelper.TextureManager, ref block);
            GroundPlaneNode groundNode = new GroundPlaneNode(
                block.GroundPlaneVertexBuffer,
                block.GroundPlaneVertices.Length / 3);
            groundNode.Position = new Vector3(0f, groundHeight, 0f);
            blockNode.Add(groundNode);

            // Add model nodes
            ModelNode modelNode;
            Vector3 scale;
            Quaternion rotation;
            Vector3 translation;
            foreach (var model in block.Models)
            {
                // Decompose matrix
                model.Matrix.Decompose(out scale, out rotation, out translation);

                // Assign to node
                modelNode = AddModelNode(blockNode, model.ModelId);
                modelNode.Position = translation;
                //rotation.
                //modelNode.Rotation = model.Rotation;
            }

            // Add billboard nodes
            BillboardNode billboardNode;
            foreach (var flat in block.Flats)
            {
                billboardNode = AddBillboardNode(blockNode, flat, dfLocation);
                //billboardNode.Position = billboardNode.Flat.Origin + billboardNode.Flat.Position;
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
        /// <param name="dfLocation">Optional DFLocation for this block's climate swap. Can be null.</param>
        /// <returns>SceneNode.</returns>
        private SceneNode BuildRDBNode(SceneNode parent, BlockManager.BlockData block, DFLocation? dfLocation)
        {
            // Create block parent node
            SceneNode blockNode = new SceneNode();

            // Dictionary to link action records with scene nodes
            Dictionary<int, SceneNode> actionKeyNodeDict = new Dictionary<int, SceneNode>();

            // Add model nodes
            ModelNode modelNode;
            foreach (var model in block.Models)
            {
                // Add model node to scene
                modelNode = AddModelNode(blockNode, model.ModelId);
                //modelNode.Position = model.Position;
                //modelNode.Rotation = model.Rotation;

                // Setup action data
                if (model.HasActionRecord)
                {
                    // Enable action
                    SceneNode.ActionRecord action = modelNode.Action;
                    action.Enabled = true;
                    action.Rotation = model.ActionRecord.Rotation;
                    action.Translation = model.ActionRecord.Translation;
                    modelNode.Action = action;

                    // Link action key to model node
                    actionKeyNodeDict.Add(model.ActionKey, modelNode);
                    
                }
            }

            // Chain action data
            foreach (var model in block.Models)
            {
                if (model.HasActionRecord)
                {
                    SceneNode node = actionKeyNodeDict[model.ActionKey];
                    int nextActionKey = model.ActionRecord.NextActionKey;
                    if (actionKeyNodeDict.ContainsKey(nextActionKey))
                    {
                        // Link to next node
                        SceneNode nextNode = actionKeyNodeDict[nextActionKey];
                        SceneNode.ActionRecord action = node.Action;
                        action.NextNode = nextNode;
                        node.Action = action;

                        // Link to previous node
                        action = nextNode.Action;
                        action.PreviousNode = node;
                        nextNode.Action = action;
                    }
                }
            }

            // Add billboard nodes
            BillboardNode billboardNode;
            foreach (var flat in block.Flats)
            {
                billboardNode = AddBillboardNode(blockNode, flat, dfLocation);
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
            // Cannot proceed if content helper is null
            //if (contentHelper == null)
            //    throw new Exception("ContentHelper is not set.");

            return 0;
            //return contentHelper.TextureManager.LoadTexture(archive, record, flags);
        }

        /// <summary>
        /// Loads a Daggerfall flat (billboard).
        /// </summary>
        /// <param name="flatItem">BlockManager.FlatItem.</param>
        /// <returns>BlockManager.FlatItem.</returns>
        private BlockManager.FlatItem LoadDaggerfallFlat(BlockManager.FlatItem flatItem)
        {
            // Load texture
            flatItem.TextureKey = LoadDaggerfallTexture(
                flatItem.TextureArchive,
                flatItem.TextureRecord,
                TextureManager.TextureCreateFlags.Dilate |
                TextureManager.TextureCreateFlags.PreMultiplyAlpha);

            // Get dimensions and scale of this texture image
            // We do this as TextureManager may have just pulled texture key from cache.
            // without loading file. We need the following information to create bounds.
            string path = "FIX BELOW";
            //string path = Path.Combine(contentHelper.TextureManager.Arena2Path,
            //    TextureFile.IndexToFileName(flatItem.TextureArchive));
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
                flatItem.Origin.Y = (flatItem.Size.Y / 2) - 4;
            }

            // Set bounding sphere radius
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
            // Cannot proceed if content helper is null
            //if (contentHelper == null)
            //    throw new Exception("ContentHelper is not set.");

            // Load model and textures
            //ModelManager.ModelData model = contentHelper.ModelManager.GetModelData(id);
            //for (int i = 0; i < model.SubMeshes.Length; i++)
            //{
            //    // Load texture
            //    model.SubMeshes[i].TextureKey = LoadDaggerfallTexture(
            //        model.DFMesh.SubMeshes[i].TextureArchive,
            //        model.DFMesh.SubMeshes[i].TextureRecord,
            //        TextureManager.TextureCreateFlags.ApplyClimate |
            //        TextureManager.TextureCreateFlags.MipMaps |
            //        TextureManager.TextureCreateFlags.PowerOfTwo);
            //}

            //return model;

            return new ModelManager.ModelData();
        }

        /// <summary>
        /// Loads a Daggerfall block.
        /// </summary>
        /// <param name="name">Block name.</param>
        /// <returns>BlockManager.BlockData.</returns>
        private BlockManager.BlockData LoadDaggerfallBlock(string name)
        {
            // Cannot proceed if content helper is null
            //if (contentHelper == null)
            //    throw new Exception("ContentHelper is not set.");

            // Clear block cache
            //contentHelper.BlockManager.ClearBlocks();

            // Load block
            //return contentHelper.BlockManager.LoadBlock(name);

            return new BlockManager.BlockData();
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
            // Create node transforms
            Matrix rotationX = Matrix.CreateRotationX(node.Rotation.X);
            Matrix rotationY = Matrix.CreateRotationY(node.Rotation.Y);
            Matrix rotationZ = Matrix.CreateRotationZ(node.Rotation.Z);
            Matrix translation = Matrix.CreateTranslation(node.Position);

            // Update cumulative matrix with node transforms
            Matrix cumulativeMatrix = Matrix.Identity;
            Matrix.Multiply(ref cumulativeMatrix, ref rotationX, out cumulativeMatrix);
            Matrix.Multiply(ref cumulativeMatrix, ref rotationY, out cumulativeMatrix);
            Matrix.Multiply(ref cumulativeMatrix, ref rotationZ, out cumulativeMatrix);
            Matrix.Multiply(ref cumulativeMatrix, ref translation, out cumulativeMatrix);
            Matrix.Multiply(ref cumulativeMatrix, ref matrix, out cumulativeMatrix);

            // Transform bounds
            BoundingSphere bounds = node.LocalBounds;
            Vector3.Transform(ref bounds.Center, ref cumulativeMatrix, out bounds.Center);

            // Update child nodes
            foreach (SceneNode child in node.Children)
            {
                bounds = BoundingSphere.CreateMerged(
                    bounds,
                    UpdateNode(child, cumulativeMatrix));
            }

            // Store transformed bounds
            node.TransformedBounds = bounds;

            // Store cumulative matrix
            node.Matrix = cumulativeMatrix;

            // TODO: Get distance to camera

            // TODO: Run actions

            return bounds;
        }

        #endregion

    }

}
