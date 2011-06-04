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
    /// Helper class for rendering Daggerfall environments with XNA.
    ///  Provides basic resource loading, scene graph, and rendering
    ///  using BasicEffect. For more advanced scene management and effects
    ///  override or reimplement as needed.
    /// </summary>
    public class SceneManager : Component
    {

        #region Class Variables

        // Daggerfall resources
        private TextureManager textureManager = null;
        private ModelManager modelManager = null;
        private BlockManager blockManager = null;

        // Scene
        private SceneNode root;

        // Camera
        Camera.InputFlags cameraInputFlags;

        // Batches
        private Dictionary<int, List<BatchItem>> batches;

        // Bounds
        private RenderableBoundingSphere renderableBounds;

        // XNA
        private Color backgroundColor;
        private VertexDeclaration vertexDeclaration;
        private BasicEffect basicEffect;

        // Ground height
        private int groundHeight = -1;

        // Sub-Components
        BillboardManager billboardManager;
        // TODO: Create sky

        #endregion

        #region Class Structures

        /// <summary>
        /// Renderable item.
        /// </summary>
        private struct BatchItem
        {
            public bool Indexed;
            public Matrix Matrix;
            public int NumVertices;
            public VertexBuffer VertexBuffer;
            public IndexBuffer IndexBuffer;
            public int StartIndex;
            public int PrimitiveCount;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets colour used for device clears.
        /// </summary>
        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set { backgroundColor = value; }
        }

        /// <summary>
        /// Gets BasicEffect used for default rendering.
        /// </summary>
        public BasicEffect BasicEffect
        {
            get { return basicEffect; }
        }

        public new Camera Camera
        {
            get { return camera; }
            set
            {
                camera = value;
                billboardManager.Camera = value;
                // TODO: Set sky camera
            }
        }

        /// <summary>
        /// Gets or sets camera input flags.
        /// </summary>
        public Camera.InputFlags CameraInputFlags
        {
            get { return cameraInputFlags; }
            set { cameraInputFlags = value; }
        }

        /// <summary>
        /// Gets root scene node.
        /// </summary>
        public SceneNode Root
        {
            get { return root; }
        }

        /// <summary>
        /// Gets or sets TextureManager for resource loading.
        /// </summary>
        public TextureManager TextureManager
        {
            get { return textureManager; }
            set
            {
                textureManager = value;
                billboardManager.TextureManager = value;
            }
        }

        /// <summary>
        /// Gets or sets ModelManager for resource loading.
        ///  For best results model caching should be enabled.
        /// </summary>
        public ModelManager ModelManager
        {
            get { return modelManager; }
            set { modelManager = value; }
        }

        /// <summary>
        /// Gets or sets BlockManager for resource loading.
        /// </summary>
        public BlockManager BlockManager
        {
            get { return blockManager; }
            set { blockManager = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="device">Graphics Device.</param>
        /// <param name="arena2Path">Path to Arena2 folder.</param>
        public SceneManager(GraphicsDevice graphicsDevice, string arena2Path)
            : base(graphicsDevice, arena2Path)
        {
            // Create vertex declaration
            vertexDeclaration = new VertexDeclaration(
                graphicsDevice, VertexPositionNormalTexture.VertexElements);

            // Batching
            batches = new Dictionary<int, List<BatchItem>>();

            // Set default background colour
            backgroundColor = Color.CornflowerBlue;

            // Set default camera update flags
            cameraInputFlags = Camera.InputFlags.None;

            // Setup renderable bounds
            renderableBounds = new RenderableBoundingSphere(graphicsDevice);

            // Setup components
            billboardManager = new BillboardManager(graphicsDevice, arena2Path);
            billboardManager.Initialize();
            billboardManager.Camera = camera;

            // Create default scene
            root = new SceneNode();
            CreateDefaultBasicEffect();
            CreateDefaultSceneCamera();
        }

        #endregion

        #region Abstract Overrides

        /// <summary>
        /// Called when component must initialise.
        /// </summary>
        public override void Initialize()
        {
        }

        /// <summary>
        /// Prepare scene for rendering.
        ///  Progresses actions.
        ///  Performs visibility testing.
        ///  Batches visible geometry by texture.
        ///  Batches and sorts billboards.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last frame.</param>
        public override void Update(TimeSpan elapsedTime)
        {
            // Clear batches from previous frame
            ClearBatches();

            // Update camera with input flags
            camera.Update(cameraInputFlags, elapsedTime);

            // Update nodes
            UpdateNode(root, Matrix.Identity);

            // Batch visible elements
            BatchNode(root, Matrix.Identity);

            // Apply camera changes
            camera.ApplyChanges();
        }

        /// <summary>
        /// Render scene with BasicEffect.
        /// </summary>
        public override void Draw()
        {
            // Draw background
            DrawBackground();

            // Draw visible geometry
            DrawBatches();

            // Draw billboard batch
            billboardManager.Draw();
        }

        #endregion

        #region Public Methods

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
            //node.Matrix = Matrix.CreateTranslation(flatItem.Position);
            node.LocalBounds = flatItem.BoundingSphere;
            parent.Add(node);

            return node;
        }

        /// <summary>
        /// Adds a block node to the scene. 
        /// </summary>
        /// <param name="parent">Parent node to receive block node child.</param>
        /// <param name="name">Name of block.</param>
        /// <returns>SceneNode.</returns>
        public SceneNode AddBlockNode(SceneNode parent, string name)
        {
            // Use root node if no parent specified
            if (parent == null)
                parent = root;

            // Load block
            BlockManager.BlockData block = LoadDaggerfallBlock(name);
            switch (block.DFBlock.Type)
            {
                case DFBlock.BlockTypes.Rmb:
                    return BuildRMBNode(parent, ref block);
                case DFBlock.BlockTypes.Rdb:
                    return BuildRDBNode(parent, ref block);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Updates camera aspect ratio after viewport changes size.
        /// </summary>
        public void UpdateCameraAspectRatio()
        {
            if (camera != null)
            {
                camera.SetAspectRatio(
                    (float)graphicsDevice.Viewport.Width / 
                    (float)graphicsDevice.Viewport.Height);
            }
        }

        /// <summary>
        /// Renders bounding volumes for any node with
        ///  flag set to draw bounds. This involves
        ///  another pass over the scene.
        /// </summary>
        public void DrawBounds()
        {
            DrawNodeBounds(root);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates default basic effect for rendering.
        ///  All nodes will render using this effect.
        /// </summary>
        private void CreateDefaultBasicEffect()
        {
            basicEffect = new BasicEffect(graphicsDevice, null);
            basicEffect.World = Matrix.Identity;
            basicEffect.TextureEnabled = true;
            basicEffect.PreferPerPixelLighting = true;
            basicEffect.EnableDefaultLighting();
            basicEffect.AmbientLightColor = new Vector3(0.4f, 0.4f, 0.4f);
            basicEffect.SpecularColor = new Vector3(0.2f, 0.2f, 0.2f);
        }

        /// <summary>
        /// Creates default scene camera. This camera is used for all
        ///  culling and rendering operations unless another camera
        ///  is specified.
        /// </summary>
        private void CreateDefaultSceneCamera()
        {
            Camera = new Camera();
            UpdateCameraAspectRatio();
            Camera.MovementBounds = new BoundingBox(
                new Vector3(float.MinValue, float.MinValue, float.MinValue),
                new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));
            Camera.ApplyChanges();
        }

        /// <summary>
        /// Builds an exterior node with all child nodes.
        /// </summary>
        /// <param name="parent">Parent node.</param>
        /// <param name="block">BlockManager.BlockData.</param>
        /// <returns>SceneNode.</returns>
        private SceneNode BuildRMBNode(SceneNode parent, ref BlockManager.BlockData block)
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
                modelNode.DrawBounds = true;
            }

            // Add billboard nodes
            BillboardNode billboardNode;
            foreach (var flat in block.Flats)
            {
                billboardNode = AddBillboardNode(blockNode, flat);
                billboardNode.Matrix = Matrix.Identity;
                billboardNode.DrawBounds = true;
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
        private SceneNode BuildRDBNode(SceneNode parent, ref BlockManager.BlockData block)
        {
            return new SceneNode();
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
            string path = Path.Combine(arena2Path, TextureFile.IndexToFileName(flatItem.TextureArchive));
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
            flatItem.BoundingSphere.Center = flatItem.Origin;
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
        /// <param name="name"></param>
        /// <returns></returns>
        private BlockManager.BlockData LoadDaggerfallBlock(string name)
        {
            // Cannot proceed if BlockManager null
            if (blockManager == null)
                throw new Exception("BlockManager is not set.");

            BlockManager.BlockData block = blockManager.LoadBlock(name);

            return block;
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

        #region Batching

        /// <summary>
        /// Clears any batched data from previous frame.
        /// </summary>
        private void ClearBatches()
        {
            // Clear local batches
            foreach (var batch in batches)
            {
                batch.Value.Clear();
            }

            // Clear billboard batches
            billboardManager.ClearBatch();
        }

        /// <summary>
        /// Add batch item.
        /// </summary>
        /// <param name="textureKey">Texture key.</param>
        private void AddBatch(int textureKey, BatchItem batchItem)
        {
            // Start new batch if required
            if (!batches.ContainsKey(textureKey))
            {
                batches.Add(textureKey, new List<BatchItem>());
            }

            // Add to batch
            batches[textureKey].Add(batchItem);
        }

        /// <summary>
        /// Recursively walks scene and batches visible submeshes.
        /// </summary>
        /// <param name="node">Start node.</param>
        /// <param name="matrix">Cumulative matrix.</param>
        private void BatchNode(SceneNode node, Matrix matrix)
        {
            // Do nothing if not visible
            if (!node.Visible)
                return;

            // Test node bounds against camera frustum
            if (!camera.BoundingFrustum.Intersects(node.TransformedBounds))
                return;

            // Update children of this node
            foreach (SceneNode child in node.Children)
            {
                BatchNode(child, node.Matrix * matrix);
            }

            // TODO: Pointer intersection test

            // Batch model node
            if (node is ModelNode)
                BatchModelNode((ModelNode)node, node.Matrix * matrix);
            else if (node is GroundPlaneNode)
                BatchGroundPlaneNode((GroundPlaneNode)node, node.Matrix * matrix);
            else if (node is BillboardNode)
                BatchBillboardNode((BillboardNode)node, node.Matrix * matrix);
        }

        /// <summary>
        /// Batch a model node for rendering.
        /// </summary>
        /// <param name="node">ModelNode.</param>
        /// <param name="matrix">Matrix.</param>
        private void BatchModelNode(ModelNode node, Matrix matrix)
        {
            // Batch submeshes
            BatchItem batchItem;
            foreach (var submesh in node.Model.SubMeshes)
            {
                batchItem.Indexed = true;
                batchItem.Matrix = matrix;
                batchItem.NumVertices = node.Model.Vertices.Length;
                batchItem.VertexBuffer = node.Model.VertexBuffer;
                batchItem.IndexBuffer = node.Model.IndexBuffer;
                batchItem.StartIndex = submesh.StartIndex;
                batchItem.PrimitiveCount = submesh.PrimitiveCount;
                AddBatch(submesh.TextureKey, batchItem);
            }
        }

        /// <summary>
        /// Batch a ground plane node for rendering.
        /// </summary>
        /// <param name="node">GroundPlaneNode.</param>
        /// <param name="matrix">Matrix.</param>
        private void BatchGroundPlaneNode(GroundPlaneNode node, Matrix matrix)
        {
            // Batch ground plane
            BatchItem batchItem;
            batchItem.Indexed = false;
            batchItem.Matrix = matrix;
            batchItem.NumVertices = node.PrimitiveCount * 3;
            batchItem.VertexBuffer = node.VertexBuffer;
            batchItem.IndexBuffer = null;
            batchItem.StartIndex = 0;
            batchItem.PrimitiveCount = node.PrimitiveCount;
            AddBatch(TextureManager.TerrainAtlasKey, batchItem);
        }

        /// <summary>
        /// Batch a billboard node for rendering.
        /// </summary>
        /// <param name="node">BillboardNode.</param>
        /// <param name="matrix">Matrix.</param>
        private void BatchBillboardNode(BillboardNode node, Matrix matrix)
        {
            // Batch billboard
            BlockManager.FlatItem flat = node.Flat;
            billboardManager.AddBatch(
                flat.Origin,
                flat.Position,
                flat.Size,
                flat.TextureKey,
                matrix);
        }

        #endregion

        #region Rendering

        /// <summary>
        /// Clears device and renders a background.
        /// </summary>
        private void DrawBackground()
        {
            graphicsDevice.Clear(backgroundColor);
        }

        /// <summary>
        /// Renders batches of visible geometry.
        /// </summary>
        private void DrawBatches()
        {
            // Set vertex declaration
            graphicsDevice.VertexDeclaration = vertexDeclaration;

            // Update view and projection matrices
            basicEffect.View = camera.View;
            basicEffect.Projection = camera.Projection;

            // Set sampler state
            graphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            graphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            // Iterate batches
            foreach (var batch in batches)
            {
                // Do nothing if batch empty
                if (batch.Value.Count == 0)
                    continue;

                // Set texture
                basicEffect.Texture = textureManager.GetTexture(batch.Key);

                // Begin
                basicEffect.Begin();
                basicEffect.CurrentTechnique.Passes[0].Begin();

                // Iterate batch items
                foreach (var batchItem in batch.Value)
                {
                    // Set vertex buffer
                    graphicsDevice.Vertices[0].SetSource(
                        batchItem.VertexBuffer,
                        0,
                        VertexPositionNormalTexture.SizeInBytes);

                    // Set transform
                    basicEffect.World = batchItem.Matrix;
                    basicEffect.CommitChanges();

                    // Draw based on indexed flag
                    if (batchItem.Indexed)
                    {
                        // Set index buffer
                        graphicsDevice.Indices = batchItem.IndexBuffer;

                        // Draw indexed primitives
                        graphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            0,
                            0,
                            batchItem.NumVertices,
                            batchItem.StartIndex,
                            batchItem.PrimitiveCount);
                    }
                    else
                    {
                        // Draw primitives
                        graphicsDevice.DrawPrimitives(
                            PrimitiveType.TriangleList,
                            batchItem.StartIndex,
                            batchItem.PrimitiveCount);
                    }
                }

                // End
                basicEffect.CurrentTechnique.Passes[0].End();
                basicEffect.End();
            }
        }

        /// <summary>
        /// Recursively draws node bounds.
        /// </summary>
        /// <param name="node">Start node.</param>
        private void DrawNodeBounds(SceneNode node)
        {
            // Test node bounds against camera frustum
            if (!camera.BoundingFrustum.Intersects(node.TransformedBounds))
                return;

            // Draw child bounds
            foreach (SceneNode child in node.Children)
            {
                DrawNodeBounds(child);
            }

            // Draw node bounds
            if (node.DrawBounds)
            {
                renderableBounds.Color = node.DrawBoundsColor;
                renderableBounds.Draw(
                    node.TransformedBounds,
                    camera.View,
                    camera.Projection,
                    Matrix.Identity);
            }
        }

        #endregion

    }

}
