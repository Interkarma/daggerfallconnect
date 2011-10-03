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
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace XNALibrary
{
    /// <summary>
    /// Draws a scene as visualised by a camera.
    /// </summary>
    public class Renderer
    {
        #region Class Variables

        // Scene
        protected Scene scene;

        // Camera
        protected Camera camera;

        // Batches
        protected Dictionary<int, List<BatchItem>> batches;

        // Bounds
        private RenderableBoundingSphere renderableBounds;

        // XNA
        private GraphicsDevice graphicsDevice;
        private Color backgroundColor;
        private BasicEffect basicEffect;

        // Textures
        private TextureManager textureManager;

        // Options
        private RendererOptions rendererOptions = RendererOptions.Flats;

        // Picking
        private Ray pointerRay = new Ray();
        private const int defaultIntersectionCapacity = 35;
        private List<Intersection.NodeIntersection> pointerNodeIntersections;
        private ModelNode pointerOverModelNode = null;

        // Line drawing
        private BasicEffect lineEffect;
        private VertexDeclaration lineVertexDeclaration;
        private VertexPositionColor[] planeLines = new VertexPositionColor[64];
        private VertexPositionColor[] actionLines = new VertexPositionColor[64];

        // Appearance
        private Color modelHighlightColor = Color.Gold;
        private Color actionHighlightColor = Color.CornflowerBlue;

        // Sub-Components
        protected Sky sky = null;
        protected Compass compass = null;
        protected BillboardManager billboardManager = null;

#if DEBUG
        // Performance
        private Stopwatch stopwatch = new Stopwatch();
        private long drawTime = 0;
#endif

        #endregion

        #region Class Structures

        /// <summary>
        /// Renderable item.
        /// </summary>
        protected struct BatchItem
        {
            public bool Indexed;
            public Matrix Matrix;
            public int NumVertices;
            public VertexBuffer VertexBuffer;
            public IndexBuffer IndexBuffer;
            public int StartIndex;
            public int PrimitiveCount;
            public Texture2D Texture;
        }

        /// <summary>
        /// Flags for renderer options.
        /// </summary>
        [Flags]
        public enum RendererOptions
        {
            /// <summary>No flags set.</summary>
            None = 0,

            /// <summary>Render sky behind scene.</summary>
            SkyPlane = 1,

            /// <summary>Render compass.</summary>
            Compass = 2,

            /// <summary>Render flats (e.g. trees, rocks, animals).</summary>
            Flats = 4,

            /// <summary>Highlights model under pointer.</summary>
            Picking = 8,
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets TextureManager set at construction.
        /// </summary>
        public TextureManager TextureManager
        {
            get { return textureManager; }
        }

        /// <summary>
        /// Gets or sets scene to render.
        /// </summary>
        public Scene Scene
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
        /// Gets or sets model highlight colour used in picking.
        /// </summary>
        public Color ModelHighlightColor
        {
            get { return modelHighlightColor; }
            set { modelHighlightColor = value; }
        }

        /// <summary>
        /// Gets sky component. Must call InitialiseSky()
        ///  before using.
        /// </summary>
        public Sky Sky
        {
            get { return sky; }
        }

        /// <summary>
        /// Gets BasicEffect used for default rendering.
        /// </summary>
        public BasicEffect BasicEffect
        {
            get { return basicEffect; }
        }

        /// <summary>
        /// Gets or sets renderer options flags.
        /// </summary>
        public RendererOptions Options
        {
            get { return rendererOptions; }
            set { rendererOptions = value; }
        }

        /// <summary>
        /// Gets or sets pointer ray.
        /// </summary>
        public Ray PointerRay
        {
            get { return pointerRay; }
            set { pointerRay = value; }
        }

        /// <summary>
        /// Gets node under pointer.
        /// </summary>
        public SceneNode PointerOverNode
        {
            get { return pointerOverModelNode; }
        }

#if DEBUG
        /// <summary>
        /// Gets last draw time in milliseconds.
        /// </summary>
        public long DrawTime
        {
            get { return drawTime; }
        }
#endif

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="textureManager">TextureManager.</param>
        public Renderer(TextureManager textureManager)
        {
            // Renderer uses same graphics device as texture manager
            this.graphicsDevice = textureManager.GraphicsDevice;
            this.textureManager = textureManager;

            // Create null scene manager
            scene = new Scene();

            // Create batching dictionary
            batches = new Dictionary<int, List<BatchItem>>();

            // Set default background colour
            backgroundColor = Color.CornflowerBlue;

            // Setup renderable bounds
            renderableBounds = new RenderableBoundingSphere(graphicsDevice);

            // Create intersections list
            pointerNodeIntersections = new List<Intersection.NodeIntersection>();

            // Setup billboard component
            billboardManager = new BillboardManager(graphicsDevice);
            billboardManager.TextureManager = textureManager;

            // Create default effect and camera
            CreateDefaultBasicEffect();
            CreateDefaultSceneCamera();

            // Setup line drawing
            lineVertexDeclaration = new VertexDeclaration(
                VertexPositionColor.VertexDeclaration.GetVertexElements());
            lineEffect = new BasicEffect(graphicsDevice);
            lineEffect.LightingEnabled = false;
            lineEffect.TextureEnabled = false;
            lineEffect.VertexColorEnabled = true;
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
        /// Update pointer ray for picking.
        ///  Uses view and projection matrices from current camera.
        /// </summary>
        /// <param name="x">Pointer X in viewport.</param>
        /// <param name="y">Pointer Y in viewport.</param>
        public void UpdatePointerRay(int x, int y)
        {
            // Get matrices
            Matrix view = camera.View;
            Matrix projection = camera.Projection;

            // Unproject vectors into view area
            Viewport vp = graphicsDevice.Viewport;
            Vector3 near = vp.Unproject(new Vector3(x, y, 0), projection, view, Matrix.Identity);
            Vector3 far = vp.Unproject(new Vector3(x, y, 1), projection, view, Matrix.Identity);

            // Create ray
            Vector3 direction = far - near;
            direction.Normalize();
            pointerRay.Position = near;
            pointerRay.Direction = direction;
        }

        /// <summary>
        /// Initialse sky component for this renderer.
        ///  Must be called before a sky background can be drawn.
        /// </summary>
        /// <param name="arena2Path">Path to Arena2 folder.</param>
        public void InitialiseSky(string arena2Path)
        {
            sky = new Sky(graphicsDevice, arena2Path);
        }

        /// <summary>
        /// Initialse compass component for this renderer.
        ///  Must be called before a compass can be drawn.
        /// </summary>
        /// <param name="arena2Path">Path to Arena2 folder.</param>
        public void InitialiseCompass(string arena2Path)
        {
            compass = new Compass(graphicsDevice, arena2Path);
        }

        /// <summary>
        /// Render active scene.
        /// </summary>
        public void Draw()
        {
#if DEBUG
            // Start timing
            stopwatch.Reset();
            stopwatch.Start();
#endif

            // Reset intersections
            pointerNodeIntersections.Clear();
            pointerNodeIntersections.Capacity = defaultIntersectionCapacity;

            // Clear batches from previous frame
            ClearBatches();

            // Batch visible elements
            BatchNode(scene.Root, true);

            // Draw background
            DrawBackground();

            // Draw visible geometry
            DrawBatches();

            // Model highlighting
            if (HasOptionsFlags(RendererOptions.Picking))
            {
                PointerModelIntersectionsTest();
                HighlightModelUnderPointer();
            }

            // Draw billboard batches
            if (HasOptionsFlags(RendererOptions.Flats))
            {
                billboardManager.Draw(camera);
            }

            // Draw compass
            if (HasOptionsFlags(RendererOptions.Compass))
            {
                compass.Draw(camera);
            }

#if DEBUG
            // End timing
            stopwatch.Stop();
            drawTime = stopwatch.ElapsedMilliseconds;
#endif
        }

        /// <summary>
        /// Recursively draws node bounds from starting node down.
        ///  Equivalent to calling DrawBounds(null).
        /// </summary>
        public void DrawBounds()
        {
            DrawBounds(null);
        }

        /// <summary>
        /// Recursively draws node bounds from starting node down.
        /// </summary>
        /// <param name="node">Start node, or null for root.</param>
        public void DrawBounds(SceneNode node)
        {
            // Use root node if no start specified
            if (node == null)
                node = scene.Root;

            // Test node bounds against camera frustum
            if (!camera.BoundingFrustum.Intersects(node.TransformedBounds))
                return;

            // Draw child bounds
            foreach (SceneNode child in node.Children)
            {
                DrawBounds(child);
            }

            // Draw node bounds
            renderableBounds.Color = node.DrawBoundsColor;
            renderableBounds.Draw(
                node.TransformedBounds,
                camera.View,
                camera.Projection,
                Matrix.Identity);
        }

        /// <summary>
        /// Determines if specified renderer option flags are set.
        /// </summary>
        /// <param name="flags">RendererOptionsFlags.</param>
        /// <returns>True if flags set.</returns>
        public bool HasOptionsFlags(RendererOptions flags)
        {
            return flags == (rendererOptions & flags);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates default basic effect for rendering.
        ///  All nodes will render using this effect.
        /// </summary>
        private void CreateDefaultBasicEffect()
        {
            basicEffect = new BasicEffect(graphicsDevice);
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

        /// <summary>
        /// Tests pointer against model intersections to
        ///  resolve actual model intersection at face level.
        /// </summary>
        private void PointerModelIntersectionsTest()
        {
            // Nothing to do if no intersections
            if (pointerNodeIntersections.Count == 0)
                return;

            // Sort intersections by distance
            pointerNodeIntersections.Sort();

            // Iterate intersections
            float? intersection = null;
            float? closestIntersection = null;
            Intersection.NodeIntersection closestModelIntersection = null;
            foreach (var ni in pointerNodeIntersections)
            {
                // Ensure node is a ModelNode
                if (false == (ni.Node is ModelNode))
                    continue;

                // Get model
                ModelNode node = (ModelNode)ni.Node;
                ModelManager.ModelData model = node.Model;

                // Test model
                bool insideBoundingSphere;
                int subMeshResult, planeResult;
                intersection = Intersection.RayIntersectsDFMesh(
                    pointerRay,
                    node.Matrix,
                    ref model,
                    out insideBoundingSphere,
                    out subMeshResult,
                    out planeResult);

                if (intersection != null)
                {
                    if (closestIntersection == null || intersection < closestIntersection)
                    {
                        closestIntersection = intersection;
                        closestModelIntersection = ni;
                    }
                }
            }

            // Store closest intersection
            if (closestModelIntersection != null)
                pointerOverModelNode = (ModelNode)closestModelIntersection.Node;
            else
                pointerOverModelNode = null;
        }

        /// <summary>
        /// Highlights model under pointer.
        /// </summary>
        private void HighlightModelUnderPointer()
        {
            if (pointerOverModelNode != null)
            {
                if (pointerOverModelNode.Action.Enabled)
                {
                    // Handle action-enabled nodes
                    HighlightActionChain(pointerOverModelNode);
                }
                else
                {
                    // Just highlight model
                    ModelManager.ModelData model = pointerOverModelNode.Model;
                    DrawNativeMesh(modelHighlightColor, ref model, pointerOverModelNode.Matrix);
                }
            }
        }

        /// <summary>
        /// Highlights an action chain.
        /// </summary>
        /// <param name="start">Start node.</param>
        private void HighlightActionChain(ModelNode start)
        {
            // Link back to start node if there is a parent
            while (start.Action.PreviousNode != null)
            {
                start = (ModelNode)start.Action.PreviousNode;
            }

            // Link through chain from start
            int lineCount = 0;
            ModelNode node = start;
            while (node != null)
            {
                // Highlight model
                ModelManager.ModelData model = node.Model;
                DrawNativeMesh(actionHighlightColor, ref model, node.Matrix);

                // Get line start
                actionLines[lineCount].Color = Color.Red;
                actionLines[lineCount++].Position = node.TransformedBounds.Center;

                // Get next node
                node = (ModelNode)node.Action.NextNode;

                // Store end point of line if there is another node in chain
                if (node != null)
                {
                    actionLines[lineCount].Color = Color.Red;
                    actionLines[lineCount++].Position = node.TransformedBounds.Center;
                }
                else
                {
                    lineCount--;
                }
            }

            // Draw action connection lines
            if (lineCount >= 2)
            {
                // Set render states
                graphicsDevice.DepthStencilState = DepthStencilState.None;

                // Set view and projection matrices
                lineEffect.View = camera.View;
                lineEffect.Projection = camera.Projection;
                lineEffect.World = Matrix.Identity;

                // Draw lines
                lineEffect.CurrentTechnique.Passes[0].Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, actionLines, 0, lineCount / 2);
            }
        }

        #endregion

        #region Batching

        /// <summary>
        /// Clears any batched data from previous frame.
        /// </summary>
        protected void ClearBatches()
        {
            // Clear local batches
            foreach (var batch in batches)
            {
                batch.Value.Clear();
            }

            // Clear billboard batches
            billboardManager.ClearBatch();

            // Clear picked model node
            pointerOverModelNode = null;
        }

        /// <summary>
        /// Add batch item.
        /// </summary>
        /// <param name="textureKey">Texture key.</param>
        protected void AddBatch(int textureKey, BatchItem batchItem)
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
        /// <param name="pointerIntersects">True if pointer intersects.</param>
        protected void BatchNode(SceneNode node, bool pointerIntersects)
        {
            // Do nothing if not visible
            if (!node.Visible)
                return;

            // Test node bounds against camera frustum
            if (!camera.BoundingFrustum.Intersects(node.TransformedBounds))
                return;

            // Test if pointer still intersects at this level
            float? intersectDistance = null;
            if (pointerIntersects)
            {
                intersectDistance = pointerRay.Intersects(node.TransformedBounds);
                if (intersectDistance == null)
                    pointerIntersects = false;
            }

            // Batch children of this node
            foreach (SceneNode child in node.Children)
            {
                BatchNode(child, pointerIntersects);
            }

            // Batch node
            if (node is ModelNode)
            {
                BatchModelNode((ModelNode)node);
                if (pointerIntersects)
                {
                    Intersection.NodeIntersection ni =
                        new Intersection.NodeIntersection(intersectDistance, node);
                    pointerNodeIntersections.Add(ni);
                }
            }
            else if (node is GroundPlaneNode)
            {
                BatchGroundPlaneNode((GroundPlaneNode)node);
            }
            else if (node is BillboardNode)
            {
                BatchBillboardNode((BillboardNode)node);
            }
        }

        /// <summary>
        /// Batch a model node for rendering.
        /// </summary>
        /// <param name="node">ModelNode.</param>
        protected void BatchModelNode(ModelNode node)
        {
            // Batch submeshes
            BatchItem batchItem;
            foreach (var submesh in node.Model.SubMeshes)
            {
                batchItem.Indexed = true;
                batchItem.Matrix = node.Matrix;
                batchItem.NumVertices = node.Model.Vertices.Length;
                batchItem.VertexBuffer = node.Model.VertexBuffer;
                batchItem.IndexBuffer = node.Model.IndexBuffer;
                batchItem.StartIndex = submesh.StartIndex;
                batchItem.PrimitiveCount = submesh.PrimitiveCount;
                batchItem.Texture = null;
                AddBatch(submesh.TextureKey, batchItem);
            }
        }

        /// <summary>
        /// Batch a ground plane node for rendering.
        /// </summary>
        /// <param name="node">GroundPlaneNode.</param>
        protected void BatchGroundPlaneNode(GroundPlaneNode node)
        {
            // Batch ground plane
            BatchItem batchItem;
            batchItem.Indexed = false;
            batchItem.Matrix = node.Matrix;
            batchItem.NumVertices = node.PrimitiveCount * 3;
            batchItem.VertexBuffer = node.VertexBuffer;
            batchItem.IndexBuffer = null;
            batchItem.StartIndex = 0;
            batchItem.PrimitiveCount = node.PrimitiveCount;
            batchItem.Texture = textureManager.GetGroundPlaneTexture(node.TextureKey);
            AddBatch(TextureManager.GroundBatchKey, batchItem);
        }

        /// <summary>
        /// Batch a billboard node for rendering.
        /// </summary>
        /// <param name="node">BillboardNode.</param>
        protected void BatchBillboardNode(BillboardNode node)
        {
            // Batch billboard
            billboardManager.AddBatch(camera, node);
        }

        #endregion

        #region Rendering

        /// <summary>
        /// Clears device and renders a background.
        /// </summary>
        protected void DrawBackground()
        {
            if (sky != null && HasOptionsFlags(RendererOptions.SkyPlane))
            {
                graphicsDevice.Clear(sky.ClearColor);
                sky.Draw(camera);
            }
            else
            {
                graphicsDevice.Clear(backgroundColor);
            }
        }

        /// <summary>
        /// Renders batches of visible geometry.
        /// </summary>
        protected void DrawBatches()
        {
            // Update view and projection matrices
            basicEffect.View = camera.View;
            basicEffect.Projection = camera.Projection;

            // Set render states
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            graphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;

            // Iterate batches
            foreach (var batch in batches)
            {
                // Do nothing if batch empty
                if (batch.Value.Count == 0)
                    continue;

                // Set texture
                if (batch.Key != TextureManager.GroundBatchKey)
                {
                    basicEffect.Texture = textureManager.GetTexture(batch.Key);
                    basicEffect.CurrentTechnique.Passes[0].Apply();
                }

                // Iterate batch items
                foreach (var batchItem in batch.Value)
                {
                    // Handle terrain textures
                    if (batch.Key == TextureManager.GroundBatchKey)
                    {
                        basicEffect.Texture = batchItem.Texture;
                        basicEffect.CurrentTechnique.Passes[0].Apply();
                    }

                    // Set vertex buffer
                    graphicsDevice.SetVertexBuffer(batchItem.VertexBuffer);

                    // Set transform
                    basicEffect.World = batchItem.Matrix;
                    basicEffect.CurrentTechnique.Passes[0].Apply();

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
            }
        }

        /// <summary>
        /// Draw native mesh as wireframe lines.
        /// </summary>
        /// <param name="color">Line color.</param>
        /// <param name="model">ModelManager.Model.</param>
        /// <param name="matrix">Matrix.</param>
        protected void DrawNativeMesh(Color color, ref ModelManager.ModelData model, Matrix matrix)
        {
            // Scale up just a little to make outline visually pop
            matrix = Matrix.CreateScale(1.015f) * matrix;

            // Set view and projection matrices
            lineEffect.View = camera.View;
            lineEffect.Projection = camera.Projection;
            lineEffect.World = matrix;

            // Draw faces
            foreach (var subMesh in model.DFMesh.SubMeshes)
            {
                foreach (var plane in subMesh.Planes)
                {
                    DrawNativeFace(color, plane.Points, matrix);
                }
            }
        }

        /// <summary>
        /// Draw a native face as a line list.
        /// </summary>
        /// <param name="color">Line color.</param>
        /// <param name="points">DFMesh.DFPoint.</param>
        /// <param name="matrix">Matrix.</param>
        protected void DrawNativeFace(Color color, DFMesh.DFPoint[] points, Matrix matrix)
        {
            // Build line primitives for this face
            int lineCount = 0;
            Vector3 vertex1, vertex2;
            for (int p = 0; p < points.Length - 1; p++)
            {
                // Add first point
                vertex1.X = points[p].X;
                vertex1.Y = -points[p].Y;
                vertex1.Z = -points[p].Z;
                planeLines[lineCount].Color = color;
                planeLines[lineCount++].Position = vertex1;

                // Add second point
                vertex2.X = points[p + 1].X;
                vertex2.Y = -points[p + 1].Y;
                vertex2.Z = -points[p + 1].Z;
                planeLines[lineCount].Color = color;
                planeLines[lineCount++].Position = vertex2;
            }

            // Join final point to first point
            vertex1.X = points[0].X;
            vertex1.Y = -points[0].Y;
            vertex1.Z = -points[0].Z;
            planeLines[lineCount].Color = color;
            planeLines[lineCount++].Position = vertex1;
            vertex2.X = points[points.Length - 1].X;
            vertex2.Y = -points[points.Length - 1].Y;
            vertex2.Z = -points[points.Length - 1].Z;
            planeLines[lineCount].Color = color;
            planeLines[lineCount++].Position = vertex2;

            lineEffect.CurrentTechnique.Passes[0].Apply();

            // Draw lines
            graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, planeLines, 0, lineCount / 2);
        }

        #endregion
    }
}
