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
    /// Helper class for loading Daggerfall environments for XNA.
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
        Camera.UpdateFlags cameraUpdateFlags;

        // Batches
        private Dictionary<int, List<BatchItem>> batches;

        // XNA
        private Color backgroundColor;
        private VertexDeclaration vertexDeclaration;
        private BasicEffect basicEffect;

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

        /// <summary>
        /// Gets or sets camera update flags.
        /// </summary>
        public Camera.UpdateFlags CameraUpdateFlags
        {
            get { return cameraUpdateFlags; }
            set { cameraUpdateFlags = value; }
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
            set { textureManager = value; }
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

            // Create default scene
            root = new SceneNode();
            CreateDefaultBasicEffect();
            CreateDefaultSceneCamera();

            // Set default background colour
            backgroundColor = Color.CornflowerBlue;

            // Set default camera update flags
            cameraUpdateFlags = Camera.UpdateFlags.None;
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
        ///  Batches visible triangles by texture.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last frame.</param>
        public override void Update(TimeSpan elapsedTime)
        {
            ClearBatches();
            camera.Update(cameraUpdateFlags, elapsedTime);
            UpdateNode(root, Matrix.Identity);
            camera.ApplyChanges();
        }

        /// <summary>
        /// Render scene with BasicEffect.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last frame.</param>
        public override void Draw(TimeSpan elapsedTime)
        {
            DrawBackground();
            DrawBatches();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a model node to the scene.
        /// </summary>
        /// <param name="parent">Parent node to receive model node child.</param>
        /// <param name="id">ModelID to load.</param>
        /// <returns></returns>
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
            camera = new Camera();
            UpdateCameraAspectRatio();
            camera.MovementBounds = new BoundingBox(
                new Vector3(float.MinValue, float.MinValue, float.MinValue),
                new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));
            camera.ApplyChanges();
        }

        #endregion

        #region Content Loading

        /// <summary>
        /// Loads a Daggerfall model and any textures required.
        /// </summary>
        /// <param name="id">ID of model.</param>
        /// <returns>ModelManager.ModelData.</returns>
        private ModelManager.ModelData LoadDaggerfallModel(uint id)
        {
            // Cannot proceed if TextureManager null
            if (textureManager == null)
                throw new Exception("TextureManager is not set.");

            // Cannot proceed if ModelManager null
            if (modelManager == null)
                throw new Exception("ModelManager is not set.");

            // Load model and textures
            ModelManager.ModelData model = modelManager.GetModelData(id);
            for (int i = 0; i < model.SubMeshes.Length; i++)
            {
                // Load texture
                int key = textureManager.LoadTexture(
                    model.DFMesh.SubMeshes[i].TextureArchive,
                    model.DFMesh.SubMeshes[i].TextureRecord,
                    TextureManager.TextureCreateFlags.ApplyClimate |
                    TextureManager.TextureCreateFlags.MipMaps |
                    TextureManager.TextureCreateFlags.PowerOfTwo);

                // Set key
                model.SubMeshes[i].TextureKey = key;
            }

            return model;
        }

        #endregion

        #region Updating

        /// <summary>
        /// Clears any batched data from previous frame.
        /// </summary>
        private void ClearBatches()
        {
            foreach (var batch in batches)
            {
                batch.Value.Clear();
            }
        }

        /// <summary>
        /// Recursively updates scene and batches visible submeshes.
        /// </summary>
        /// <param name="node">Start node.</param>
        /// <param name="matrix">Cumulative matrix.</param>
        private void UpdateNode(SceneNode node, Matrix matrix)
        {
            // Do nothing if not visible
            if (!node.Visible)
                return;

            // Update children of this node
            foreach (SceneNode child in node.Children)
            {
                UpdateNode(child, child.Matrix * matrix);
            }

            // TODO: Intersection and collision

            // Test bounds against camera frustum
            if (!camera.BoundingFrustum.Intersects(node.Bounds))
                return;

            // Batch model node
            if (node is ModelNode)
                BatchModelNode((ModelNode)node, matrix);
        }

        /// <summary>
        /// Batch a model node for rendering.
        /// </summary>
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
        /// Renders batches of visible triangles.
        /// </summary>
        private void DrawBatches()
        {
            // Set vertex declaration
            graphicsDevice.VertexDeclaration = vertexDeclaration;

            // Update view and projection matrices
            basicEffect.View = camera.View;
            basicEffect.Projection = camera.Projection;

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

        #endregion

    }

}
