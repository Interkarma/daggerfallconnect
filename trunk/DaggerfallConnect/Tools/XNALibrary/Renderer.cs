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
    /// Takes scene and camera objects as properties and draws the scene
    ///  as visualised by the camera. If using the default scene and camera
    ///  created at construction, don't forget to set content managers
    ///  (TextureManager, ModelManager, BlockManager) so the scene can load content.
    /// </summary>
    public class Renderer
    {
        #region Class Variables

        // Scene
        SceneManager scene;

        // Camera and input
        Camera camera;

        // Batches
        private Dictionary<int, List<BatchItem>> batches;

        // Bounds
        private RenderableBoundingSphere renderableBounds;

        // XNA
        private GraphicsDevice graphicsDevice;
        private Color backgroundColor;
        private VertexDeclaration vertexDeclaration;
        private BasicEffect basicEffect;

        // Sub-Components
        BillboardManager billboardManager;
        // TODO: Create sky manager

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
        /// Gets or sets scene to render.
        /// </summary>
        public SceneManager Scene
        {
            get { return scene; }
            set { scene = value; }
        }

        /// <summary>
        /// Gets or sets active camera used to look into scene.
        /// </summary>
        public Camera Camera
        {
            get { return camera; }
            set { camera = value; }
        }

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

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="device">Graphics Device.</param>
        public Renderer(GraphicsDevice graphicsDevice)
        {
            // Store graphics device
            this.graphicsDevice = graphicsDevice;

            // Create null scene manager
            scene = new SceneManager(null, null, null);

            // Create vertex declaration
            vertexDeclaration = new VertexDeclaration(
                graphicsDevice, VertexPositionNormalTexture.VertexElements);

            // Create batching dictionary
            batches = new Dictionary<int, List<BatchItem>>();

            // Set default background colour
            backgroundColor = Color.CornflowerBlue;

            // Setup renderable bounds
            renderableBounds = new RenderableBoundingSphere(graphicsDevice);

            // Setup components
            billboardManager = new BillboardManager(graphicsDevice);

            // Create default effect and camera
            CreateDefaultBasicEffect();
            CreateDefaultSceneCamera();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates camera aspect ratio after viewport changes size.
        /// <param name="width">New width. Use -1 for auto from viewport.</param>
        /// <param name="height">New height. Use -1 for auto from viewport.</param>
        /// </summary>
        public void UpdateCameraAspectRatio(int width, int height)
        {
            // Handle auto width and height
            if (width == -1) width = graphicsDevice.Viewport.Width;
            if (height == -1) height = graphicsDevice.Viewport.Height;

            // Set new width and height
            if (camera != null)
            {
                camera.SetAspectRatio(
                    (float)width /
                    (float)height);
            }
        }

        /// <summary>
        /// Render active scene.
        /// </summary>
        public void Draw()
        {
            // Clear batches from previous frame
            ClearBatches();

            // Batch visible elements
            BatchNode(scene.Root, Matrix.Identity);

            // Draw background
            DrawBackground();

            // Draw visible geometry
            DrawBatches();

            // Draw billboard batch
            billboardManager.TextureManager = scene.TextureManager;
            billboardManager.Draw(camera);
        }

        /// <summary>
        /// Renders bounding volumes for any node with
        ///  flag set to draw bounds. This involves
        ///  another pass over the scene.
        /// </summary>
        public void DrawBounds()
        {
            DrawNodeBounds(scene.Root);
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
            UpdateCameraAspectRatio(-1, -1);
            camera.MovementBounds = new BoundingBox(
                new Vector3(float.MinValue, float.MinValue, float.MinValue),
                new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));
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
                camera,
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

            // Set render states
            graphicsDevice.RenderState.DepthBufferEnable = true;
            graphicsDevice.RenderState.AlphaBlendEnable = false;
            graphicsDevice.RenderState.AlphaTestEnable = false;
            graphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

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
                basicEffect.Texture = scene.TextureManager.GetTexture(batch.Key);

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
