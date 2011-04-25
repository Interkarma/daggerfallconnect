// Project:         DaggerfallModelling
// Description:     Explore and export 3D models from Daggerfall.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using XNALibrary;

#endregion

namespace DaggerfallModelling.ViewControls
{

    /// <summary>
    /// Explore a location from a single block to full cities and dungeons.
    ///  This view takes a DFBlock or DFLocation to build scene layout.
    ///  Loading is separated by block/location and exterior/dungeon methods so it is
    ///  possible to keep load times to a minimum.
    /// </summary>
    public class LocationView : ViewBase
    {

        #region Class Variables

        // Layout
        private const float rmbBlockSide = 4096.0f;
        private const float rdbBlockSide = 2048.0f;
        private BlockPosition[] exteriorLayout = new BlockPosition[64];
        private BlockPosition[] dungeonLayout = new BlockPosition[32];
        private Dictionary<int, int> exteriorLayoutDict = new Dictionary<int,int>();
        private Dictionary<int, int> dungeonLayoutDict =  new Dictionary<int,int>();
        private int exteriorLayoutCount = 0;
        private int dungeonLayoutCount = 0;

        // Batching
        private const int batchArrayLength = 768;
        private BatchModes batchMode = BatchModes.SingleExteriorBlock;
        private BatchOptions batchOptions = BatchOptions.RmbGroundPlane | BatchOptions.RmbGroundFlats;
        private Dictionary<int, BatchItemArray> exteriorBatches = new Dictionary<int, BatchItemArray>();
        private Dictionary<int, BatchItemArray> dungeonBatches = new Dictionary<int, BatchItemArray>();

        // Location
        private string currentBlockName = string.Empty;
        private int currentLatitude = -1;
        private int currentLongitude = -1;

        // Status message
        private string currentStatus = string.Empty;

        // Appearance
        private Color backgroundColor = Color.LightGray;

        // XNA
        private VertexDeclaration modelVertexDeclaration;
        private VertexDeclaration lineVertexDeclaration;
        private BasicEffect lineEffect;
        private BasicEffect modelEffect;
        private BoundingFrustum viewFrustum;

        // Cameras
        private Camera exteriorTopDownCamera = new Camera();
        private Camera exteriorFreeCamera = new Camera();
        private Camera dungeonTopDownCamera = new Camera();
        private Camera dungeonFreeCamera = new Camera();
        private static float cameraFloorHeight = 60.0f;
        private static float cameraCeilingHeight = 10000.0f;
        private static float cameraStartHeight = 6000.0f;
        private static float cameraDungeonFreedom = 1000.0f;

        // Movement
        private Vector3 cameraVelocity;
        private float cameraStep = 5.0f;
        private float wheelStep = 100.0f;

        // Ray testing
        private const int defaultIntersectionCapacity = 35;
        private uint? mouseOverModel = null;
        private List<ModelIntersection> modelIntersections;
        private RenderableBoundingBox renderableBoundingBox;
        private RenderableBoundingSphere renderableBoundingSphere;

        // Line drawing
        VertexPositionColor[] planeLines = new VertexPositionColor[64];
        
#if DEBUG
        // Performance profiling
        private WeakReference gcTracker = new WeakReference(new object());
        private long drawTime;
        private int visibleTriangles;
        private int visibleBatches;
        private int maxBatchArrayLength;
        private int garbageCollections = 0;
#endif

        #endregion

        #region Class Structures

        /// <summary>
        /// Specifies which layout data should be rendered.
        /// </summary>
        public enum BatchModes
        {
            /// <summary>Render single exterior block only.</summary>
            SingleExteriorBlock,
            /// <summary>Render single dungeon block only.</summary>
            SingleDungeonBlock,
            /// <summary>Render full exterior location.</summary>
            FullExterior,
            /// <summary>Render full dungeon location.</summary>
            FullDungeon,
            /// <summary>Render interior of specified location.</summary>
            Interior,
        }

        /// <summary>
        /// Describes optional rendering features. Options can be combined.
        /// </summary>
        [Flags]
        public enum BatchOptions
        {
            /// <summary>No flags set.</summary>
            None = 0,
            /// <summary>Render ground plane below exterior blocks.</summary>
            RmbGroundPlane = 1,
            /// <summary>Render miscellaneous ground objects (e.g. signs and gravestones).</summary>
            RmbGroundObjects = 2,
            /// <summary>Render ground scenery (e.g. rocks and trees) in exterior blocks.</summary>
            RmbGroundFlats = 4,
            /// <summary>Render flats (e.g. NPCs and lights) in dungeon blocks. </summary>
            RdbFlats = 8,
        }

        /// <summary>
        /// Describes how a block is positioned in world space.
        /// </summary>
        private struct BlockPosition
        {
            public string name;
            public Vector3 position;
            public BlockManager.Block block;
        }

        /// <summary>
        /// Stores a fixes array of BatchItem which is allocated
        ///  during scene build to minimise garbage collections laters.
        /// </summary>
        private struct BatchItemArray
        {
            public int Length;
            public BatchItem[] BatchItems;
        }

        /// <summary>
        /// Represents a visible submesh. These batches are grouped by
        ///  texture while walking the scene then all executed at once.
        ///  This reduces the number of texture changes and begin-end blocks.
        /// </summary>
        private struct BatchItem
        {
            public Matrix ModelTransform;
            public VertexPositionNormalTexture[] Vertices;
            public int[] Indices;
        }

        #endregion

        #region SubClasses

        /// <summary>
        /// Describes a model that has intersected with a ray.
        ///  Used when sorting intersections for face-accurate picking.
        /// </summary>
        private class ModelIntersection : IComparable<ModelIntersection>
        {
            // Variables
            private float? distance;
            private uint? modelId;
            private Matrix matrix;

            // Properties
            public float? Distance
            {
                get { return distance; }
                set { distance = value; }
            }
            public uint? ModelID
            {
                get { return modelId; }
                set { modelId = value; }
            }
            public Matrix Matrix
            {
                get { return matrix; }
                set { matrix = value; }
            }

            // Constructors
            public ModelIntersection()
            {
                this.distance = null;
                this.modelId = null;
                this.matrix = Matrix.Identity;
            }
            public ModelIntersection(float? distance, uint? modelId, Matrix matrix)
            {
                this.distance = distance;
                this.modelId = modelId;
                this.matrix = matrix;
            }

            // IComparable
            public int CompareTo(ModelIntersection other)
            {
                int returnValue = -1;
                if (other.Distance < this.Distance)
                    returnValue = 1;
                else if (other.Distance == this.Distance)
                    returnValue = 0;
                return returnValue;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets batch mode for drawing operations.
        /// </summary>
        public BatchModes BatchMode
        {
            get { return batchMode; }
            set { ChangeBatchMode(value); }
        }

        /// <summary>
        /// Gets active camera.
        /// </summary>
        public Camera ActiveCamera
        {
            get { return GetActiveCamera(); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public LocationView(ViewHost host)
            : base(host)
        {
            // Start in normal camera mode
            CameraMode = CameraModes.TopDown;
            renderableBoundingBox = new RenderableBoundingBox(host.GraphicsDevice);
            renderableBoundingSphere = new RenderableBoundingSphere(host.GraphicsDevice);

            // Create model intersections list
            modelIntersections = new List<ModelIntersection>();
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Called by host when view should initialise.
        /// </summary>
        public override void Initialize()
        {
            // Create vertex declaration
            modelVertexDeclaration = new VertexDeclaration(host.GraphicsDevice, VertexPositionNormalTexture.VertexElements);

            // Setup model basic effect
            modelEffect = new BasicEffect(host.GraphicsDevice, null);
            modelEffect.World = Matrix.Identity;
            modelEffect.TextureEnabled = true;
            modelEffect.PreferPerPixelLighting = true;
            modelEffect.EnableDefaultLighting();
            modelEffect.AmbientLightColor = new Vector3(0.4f, 0.4f, 0.4f);
            modelEffect.SpecularColor = new Vector3(0.2f, 0.2f, 0.2f);

            // Create vertex declaration
            lineVertexDeclaration = new VertexDeclaration(host.GraphicsDevice, VertexPositionColor.VertexElements);

            // Setup line BasicEffect
            lineEffect = new BasicEffect(host.GraphicsDevice, null);
            lineEffect.LightingEnabled = false;
            lineEffect.TextureEnabled = false;
            lineEffect.VertexColorEnabled = true;

            // Setup initial camera positions
            InitCameraPosition();

            // Setup camera projection matrices
            UpdateProjectionMatrix();

            // Create view frustum
            viewFrustum = new BoundingFrustum(Matrix.Identity);
        }

        /// <summary>
        /// Called by host when view should update animation.
        /// </summary>
        public override void Tick()
        {
            // Update cameras
            UpdateCameras();

#if DEBUG
            // Track garbage collections
            if (!gcTracker.IsAlive)
            {
                garbageCollections++;
                gcTracker = new WeakReference(new object());
            }
#endif
        }

        /// <summary>
        /// Called by host when view should redraw.
        /// </summary>
        public override void Draw()
        {
#if DEBUG
            // Reset performance metrics
            visibleBatches = 0;
            visibleTriangles = 0;
            maxBatchArrayLength = 0;
            long startTime = host.Timer.ElapsedTicks;
#endif

            // Execute pipeline
            host.GraphicsDevice.Clear(backgroundColor);
            ClearBatches();
            BatchScene();
            MouseModelIntersection();
            SetRenderStates();
            DrawBatches();

#if DEBUG
            // Get total draw time (will always be at least 1 tick)
            drawTime = (host.Timer.ElapsedTicks - startTime) + 1;

            // Display performance status in debug mode
            string performance = string.Format(
                "GarbageCollections={0}, VisibleBatches={1}, MaxBatchArrayLength={2}, VisibleTriangles={3}, DrawTime={4}, FPS={5:0.00}",
                garbageCollections,
                visibleBatches,
                maxBatchArrayLength,
                visibleTriangles,
                drawTime,
                System.Diagnostics.Stopwatch.Frequency / (float)drawTime);
            host.StatusMessage = performance;
#endif
        }

        /// <summary>
        /// Called by host when view should resize.
        /// </summary>
        public override void Resize()
        {
            // Update projection matrix and refresh
            UpdateProjectionMatrix();
            host.Refresh();
        }

        /// <summary>
        /// Called when view should track mouse movement.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public override void OnMouseMove(MouseEventArgs e)
        {
            // Update mouse ray
            host.UpdateMouseRay(e.X, e.Y, ActiveCamera.View, ActiveCamera.Projection);

            // Top down camera movement
            if (CameraMode == CameraModes.TopDown)
            {
                // Scene dragging
                if (host.RightMouseDown)
                {
                    ActiveCamera.Translate(
                        (float)-host.MousePosDelta.X * cameraStep,
                        0f,
                        (float)-host.MousePosDelta.Y * cameraStep);
                }
            }
        }

        /// <summary>
        /// Called when the mouse wheel is scrolled.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public override void OnMouseWheel(MouseEventArgs e)
        {
            // Top down camera movement
            if (CameraMode == CameraModes.TopDown)
            {
                float amount = ((float)e.Delta / 120.0f) * wheelStep;
                ActiveCamera.Translate(0, -amount, 0);
            }
        }

        /// <summary>
        /// Called when a mouse button is pressed.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public override void OnMouseDown(MouseEventArgs e)
        {
            // Clear camera velocity for any mouse down event
            cameraVelocity = Vector3.Zero;
        }

        /// <summary>
        /// Called when a mouse button is released.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public override void OnMouseUp(MouseEventArgs e)
        {
            // Normal camera movement
            if (CameraMode == CameraModes.TopDown)
            {
                // Scene dragging
                if (e.Button == MouseButtons.Right)
                {
                    // Set scroll velocity on right mouse up
                    cameraVelocity = new Vector3(
                        -host.MouseVelocity.X * cameraStep,
                        0.0f,
                        -host.MouseVelocity.Y * cameraStep);

                    // Cap velocity at very small amounts to limit drifting
                    if (cameraVelocity.X > -cameraStep && cameraVelocity.X < cameraStep) cameraVelocity.X = 0.0f;
                    if (cameraVelocity.Z > -cameraStep && cameraVelocity.Z < cameraStep) cameraVelocity.Z = 0.0f;
                }
            }
        }

        /// <summary>
        /// Called when user double-clicks mouse.
        /// </summary>
        /// <param name="e">MouseEventArgs.</param>
        public override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (mouseOverModel != null)
            {
                host.ShowModelView(mouseOverModel.Value, Climate);
            }
        }

        /// <summary>
        /// Called when the view is resumed after being inactive.
        ///  Allows view to perform any layout or other requirements before redraw.
        /// </summary>
        public override void ResumeView()
        {
            // Climate swaps in dungeons now implemented yet.
            // Set climate type manually for now to ensure
            // dungeons do not use climate swaps.
            if (batchMode == BatchModes.SingleExteriorBlock ||
                batchMode == BatchModes.FullExterior)
            {
                host.TextureManager.Climate = base.Climate;
            }
            else
            {
                host.TextureManager.Climate = DFLocation.ClimateType.None;
            }

            // Clear scroll velocity
            cameraVelocity = Vector3.Zero;

            // Resume view
            UpdateProjectionMatrix();
            UpdateStatusMessage();
            host.Refresh();
        }

        /// <summary>
        /// Called to change camera mode.
        /// </summary>
        /// <param name="mode">New camera mode.</param>
        protected override void OnChangeCameraMode(CameraModes cameraMode)
        {
            base.OnChangeCameraMode(cameraMode);

            // Clear camera velocity
            cameraVelocity = Vector3.Zero;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Moves active camera to X-Z origin of specified block.
        ///  Nothing happens if block is not found.
        /// </summary>
        /// <param name="name">Name of block.</param>
        public void MoveToBlock(int x, int z)
        {
            // Search for block in active layout
            int count;
            BlockPosition[] layout;
            Dictionary<int, int> layoutDict;
            GetLayoutArray(out layout, out count);
            GetLayoutDict(out layoutDict);
            BlockPosition foundBlock = new BlockPosition();
            int key = GetBlockKey(x, z);
            if (layoutDict.ContainsKey(key))
                foundBlock = layout[layoutDict[key]];
            else
                return;

            // Move active camera to block position
            Vector3 pos = ActiveCamera.Position;
            pos.X = foundBlock.position.X + foundBlock.block.BoundingBox.Max.X / 2;
            pos.Z = foundBlock.position.Z + foundBlock.block.BoundingBox.Min.Z / 2;
            ActiveCamera.Position = pos;
        }

        #endregion

        #region Rendering Pipeline

        /// <summary>
        /// Clears batch lists. These are rebuilt each scene
        ///  from visible submeshes.
        /// </summary>
        private void ClearBatches()
        {
            // Get batches
            Dictionary<int, BatchItemArray> batches;
            GetBatches(out batches);

            // Clear each batch list
            List<int> keys = new List<int>(batches.Keys);
            foreach (int key in keys)
            {
                // Zero out length of each array
                BatchItemArray batchArray = batches[key];
                batchArray.Length = 0;
                batches[key] = batchArray;
            }
        }

        /// <summary>
        /// Step through scene elements testing intersections
        ///  and batching visible triangles.
        /// </summary>
        private void BatchScene()
        {
            // Get batch layout data            
            int count;
            BlockPosition[] layout;
            GetLayoutArray(out layout, out count);
            if (layout == null)
                return;
            
            // Update view frustum
            viewFrustum.Matrix = ActiveCamera.BoundingFrustumMatrix;

            // Reset model intersections list
            modelIntersections.Clear();
            modelIntersections.Capacity = defaultIntersectionCapacity;

            // Step through block layout
            float? intersectDistance;
            bool mouseInBlock = false;
            Matrix blockTransform;
            BoundingBox blockBounds;
            for (int i = 0; i < count; i++)
            {
                // Update block if required
                if (layout[i].block.UpdateRequired)
                    UpdateBlock(ref layout[i].block);

                // Create transformed block bounding box
                blockTransform = Matrix.CreateTranslation(layout[i].position);
                blockBounds.Min = Vector3.Transform(layout[i].block.BoundingBox.Min, blockTransform);
                blockBounds.Max = Vector3.Transform(layout[i].block.BoundingBox.Max, blockTransform);

                // Do nothing further if block is not visible
                if (!viewFrustum.Intersects(blockBounds))
                    continue;

                // Test mouse ray against block bounds.
                // Only performed when not auto-scrolling.
                if (cameraVelocity == Vector3.Zero)
                {
                    intersectDistance = host.MouseRay.Intersects(blockBounds);
                    if (intersectDistance != null)
                        mouseInBlock = true;
                    else
                        mouseInBlock = false;
                }

                // Batch block
                BatchBlock(ref layout[i].block, ref blockTransform, mouseInBlock);
            }
        }

        /// <summary>
        /// Step through a block to find visible models.
        /// </summary>
        /// <param name="block">BlockManager.Block</param>
        /// <param name="blockTransform">Block transform.</param>
        /// <param name="mouseInBlock">True if mouse ray in block bounds.</param>
        private void BatchBlock(ref BlockManager.Block block, ref Matrix blockTransform, bool mouseInBlock)
        {
            // Iterate each model in this block
            int modelIndex = 0;
            float? intersectDistance;
            Matrix modelTransform;
            BoundingSphere modelBounds;
            foreach (var modelInfo in block.Models)
            {
                // Create transformed model bounding sphere
                modelTransform = modelInfo.Matrix * blockTransform;
                modelBounds.Center = Vector3.Transform(modelInfo.BoundingSphere.Center, modelTransform);
                modelBounds.Radius = modelInfo.BoundingSphere.Radius;

                // Do nothing further if model not visible
                if (!viewFrustum.Intersects(modelBounds))
                    continue;

                // Test mouse ray against model bounds
                if (mouseInBlock)
                {
                    intersectDistance = host.MouseRay.Intersects(modelBounds);
                    if (intersectDistance != null)
                    {
                        // Add to intersection list
                        ModelIntersection mi = new ModelIntersection(
                            intersectDistance,
                            modelInfo.ModelId,
                            modelTransform);
                        modelIntersections.Add(mi);
                    }
                }

                // Add the model
                ModelManager.Model model = host.ModelManager.GetModel(modelInfo.ModelId);
                BatchModel(ref model, ref modelTransform);

                // Increment index
                modelIndex++;
            }

            //// Optionally draw gound plane for this block
            //if (batchMode == BatchModes.SingleExteriorBlock ||
            //    batchMode == BatchModes.FullExterior &&
            //    BatchOptions.RmbGroundPlane == (batchOptions & BatchOptions.RmbGroundPlane))
            //{
            //    // Translate ground down a few units to reduce
            //    // z-fighting with other ground-aligned planes
            //    Matrix groundTransform = blockTransform * Matrix.CreateTranslation(0, -7, 0);

            //    // Draw ground plane
            //    //DrawGroundPlane(ref block, ref groundTransform);
            //}
        }

        /// <summary>
        /// Step through a model and add triangles to batch.
        /// </summary>
        /// <param name="model">ModelManager.Model.</param>
        /// <param name="modelTransform">Model transform.</param>
        private void BatchModel(ref ModelManager.Model model, ref Matrix modelTransform)
        {
            // Exit if no model loaded
            if (model.Vertices == null)
                return;

            // Get batches
            Dictionary<int, BatchItemArray> batches;
            GetBatches(out batches);

            // Batch submeshes
            foreach (var subMesh in model.SubMeshes)
            {
#if DEBUG
                // Increment triangle counter
                visibleTriangles += subMesh.Indices.Length / 3;
#endif

                // Add subMesh to batch
                if (batches.ContainsKey(subMesh.TextureKey))
                {
                    // Add reference to vertex and index data to batch
                    BatchItemArray batchArray = batches[subMesh.TextureKey];
                    int index = batchArray.Length;
                    batchArray.BatchItems[index].ModelTransform = modelTransform;
                    batchArray.BatchItems[index].Vertices = model.Vertices;
                    batchArray.BatchItems[index].Indices = subMesh.Indices;
                    batchArray.Length++;
                    batches[subMesh.TextureKey] = batchArray;
                }
            }
        }

        /*
        /// <summary>
        /// Draw a ground plane.
        /// </summary>
        /// <param name="block">BlockManager.Block.</param>
        /// <param name="groundTransform">Ground transform.</param>
        private void DrawGroundPlane(ref BlockManager.Block block, ref Matrix groundTransform)
        {
            // Set world matrix
            modelEffect.World = groundTransform;

            // Set vertex declaration
            host.GraphicsDevice.VertexDeclaration = modelVertexDeclaration;

            // Set terrain texture atlas
            modelEffect.Texture = host.TextureManager.TerrainAtlas;

            // TEST: Increment counters
            textureChanges++;
            trianglesDrawn += 512;

            // Set clamp mode
            host.GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
            host.GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Clamp;

            modelEffect.Begin();
            modelEffect.CurrentTechnique.Passes[0].Begin();

            host.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, block.GroundPlaneVertices, 0, 512);

            modelEffect.CurrentTechnique.Passes[0].End();
            modelEffect.End();
        }
        */

        /// <summary>
        /// Tests mouse ray against model intersections to
        ///  determine actual intersection at face level.
        /// </summary>
        private void MouseModelIntersection()
        {
            // Nothing to do if no intersections
            if (modelIntersections.Count == 0)
                return;

            // Sort intersections by distance
            modelIntersections.Sort();

            // Iterate intersections
            float? intersection = null;
            float? closestIntersection = null;
            ModelIntersection closestModelIntersection = null;
            foreach (var mi in modelIntersections)
            {
                // Get model
                ModelManager.Model model = host.ModelManager.GetModel(mi.ModelID.Value);

                // Test model
                bool insideBoundingSphere;
                int subMeshResult, planeResult;
                intersection = Intersection.RayIntersectsDFMesh(
                    host.MouseRay,
                    mi.Matrix,
                    ref model,
                    out insideBoundingSphere,
                    out subMeshResult,
                    out planeResult);

                if (intersection != null)
                {
                    if (closestIntersection == null || intersection < closestIntersection)
                    {
                        closestIntersection = intersection;
                        closestModelIntersection = mi;
                    }
                }
            }

            // Draw bounding mesh on closest model
            if (closestModelIntersection != null)
            {
                // Draw bounding box to see what has been intersected
                ModelManager.Model model = host.ModelManager.GetModel(closestModelIntersection.ModelID.Value);
                DrawNativeMesh(Color.Gold, ref model, closestModelIntersection.Matrix);

                // Store modelid of model under mouse
                mouseOverModel = closestModelIntersection.ModelID;
            }
        }

        /// <summary>
        /// Draw native mesh as wireframe lines.
        /// </summary>
        /// <param name="color">Line color.</param>
        /// <param name="model">ModelManager.Model.</param>
        /// <param name="matrix">Matrix.</param>
        private void DrawNativeMesh(Color color, ref ModelManager.Model model, Matrix matrix)
        {
            // Scale up just a little to make outline visually pop
            matrix = Matrix.CreateScale(1.015f) * matrix;

            // Set view and projection matrices
            lineEffect.View = ActiveCamera.View;
            lineEffect.Projection = ActiveCamera.Projection;
            lineEffect.World = matrix;

            // Set vertex declaration
            host.GraphicsDevice.VertexDeclaration = lineVertexDeclaration;

            // Draw faces
            foreach (var subMesh in model.DFMesh.SubMeshes)
            {
                foreach (var plane in subMesh.Planes)
                {
                    DrawNativeFace(color, plane.Points, ref matrix);
                }
            }
        }

        /// <summary>
        /// Draw a native face as a line list.
        /// </summary>
        /// <param name="color">Line color.</param>
        /// <param name="points">DFMesh.DFPoint.</param>
        /// <param name="matrix">Matrix.</param>
        private void DrawNativeFace(Color color, DFMesh.DFPoint[] points, ref Matrix matrix)
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

            lineEffect.Begin();
            lineEffect.CurrentTechnique.Passes[0].Begin();

            // Draw lines
            host.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, planeLines, 0, lineCount / 2);

            lineEffect.CurrentTechnique.Passes[0].End();
            lineEffect.End();
        }

        /// <summary>
        /// Sets render states prior to drawing.
        /// </summary>
        private void SetRenderStates()
        {
            // Set render states
            host.GraphicsDevice.RenderState.DepthBufferEnable = true;
            host.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            host.GraphicsDevice.RenderState.AlphaTestEnable = false;
            host.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            // Set anisotropy based on camera mode
            if (cameraMode == CameraModes.Free)
            {
                // Set max anisotropy
                host.GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Anisotropic;
                host.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Anisotropic;
                host.GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Linear;
                host.GraphicsDevice.SamplerStates[0].MaxAnisotropy = host.GraphicsDevice.GraphicsDeviceCapabilities.MaxAnisotropy;
            }
            else
            {
                // Set zero anisotropy
                host.GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Linear;
                host.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Linear;
                host.GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Linear;
                host.GraphicsDevice.SamplerStates[0].MaxAnisotropy = 0;
            }
        }

        /// <summary>
        /// Draw batches of visible triangles that have been sorted by texture
        ///  to minimise begin-end blocks.
        /// </summary>
        private void DrawBatches()
        {
            // Get batches
            Dictionary<int, BatchItemArray> batches;
            GetBatches(out batches);

            // Set vertex declaration
            host.GraphicsDevice.VertexDeclaration = modelVertexDeclaration;

            // Update view and projection matrices
            modelEffect.View = ActiveCamera.View;
            modelEffect.Projection = ActiveCamera.Projection;

            // Set sampler state
            host.GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            host.GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            // Iterate batch lists
            foreach (var item in batches)
            {
                BatchItemArray batchArray = item.Value;

                // Do nothing if batch empty
                if (batchArray.Length == 0)
                    continue;

                // Track max length of batch array
#if DEBUG
                if (batchArray.Length > maxBatchArrayLength)
                    maxBatchArrayLength = batchArray.Length;
#endif

                modelEffect.Texture = host.TextureManager.GetTexture(item.Key);

                modelEffect.Begin();
                modelEffect.CurrentTechnique.Passes[0].Begin();

                // Iterate batch items
                for (int i = 0; i < batchArray.Length; i++)
                {
                    modelEffect.World = batchArray.BatchItems[i].ModelTransform;
                    modelEffect.CommitChanges();

                    host.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                        batchArray.BatchItems[i].Vertices, 0, batchArray.BatchItems[i].Vertices.Length,
                        batchArray.BatchItems[i].Indices, 0, batchArray.BatchItems[i].Indices.Length / 3);
                }

                modelEffect.CurrentTechnique.Passes[0].End();
                modelEffect.End();

#if DEBUG
                // Update visible batch counter
                visibleBatches++;
#endif

            }
        }

        #endregion

        #region Map Loading

        /// <summary>
        /// Loads a single block as an exterior location.
        ///  This will replace existing exterior layout.
        /// </summary>
        /// <param name="blockName">Block name.</param>
        public void LoadExteriorBlock(string blockName, DFLocation.ClimateType climate)
        {
            // Do nothing loading same block as current
            if (blockName == currentBlockName)
                return;

            // Set climate
            Climate = climate;
            
            // Build single-block exterior layout
            BuildExteriorLayout(ref blockName);

            // Set status message
            currentStatus = string.Format("Viewing RMB block {0}.", blockName);

            // Store block name
            currentBlockName = blockName;

            // Init camera
            cameraVelocity = Vector3.Zero;
            InitCameraPosition();
        }

        /// <summary>
        /// Loads a single block as a dungeon location.
        ///  This will replace existing dungeon layout.
        /// </summary>
        /// <param name="blockName">Block name.</param>
        public void LoadDungeonBlock(string blockName)
        {
            // Do nothing loading same block as current
            if (blockName == currentBlockName)
                return;

            // Disable climate for dungeon blocks
            Climate = DFLocation.ClimateType.None;

            // Build single-block layout
            BuildDungeonLayout(ref blockName);

            // Set status message
            currentStatus = string.Format("Viewing RDB block {0}.", blockName);

            // Store block name
            currentBlockName = blockName;

            // Init camera
            cameraVelocity = Vector3.Zero;
            InitCameraPosition();
        }

        /// <summary>
        /// Loads a full location, including dungeon layout if present.
        ///  This will replace existing layout.
        /// </summary>
        /// <param name="dfLocation">DFLocation.</param>
        public void LoadLocation(ref DFLocation dfLocation)
        {
            // Do nothing if loading same location as current
            if (currentLatitude == dfLocation.MapTableData.Latitude &&
                currentLongitude == dfLocation.MapTableData.Longitude)
                return;

            // Set climate
            Climate = dfLocation.Climate;

            // Build layout
            BuildExteriorLayout(ref dfLocation);

            // Optionally build dungeon layout
            if (dfLocation.HasDungeon)
                BuildDungeonLayout(ref dfLocation);

            // Set status message
            currentStatus = string.Format("Exploring {0}.", dfLocation.Name);

            // Store location coordinates
            currentLatitude = (int)dfLocation.MapTableData.Latitude;
            currentLongitude = (int)dfLocation.MapTableData.Longitude;

            // Init camera
            cameraVelocity = Vector3.Zero;
            InitCameraPosition();
        }

        #endregion

        #region Layout Methods

        /// <summary>
        /// Build exterior layout from a single block.
        /// </summary>
        /// <param name="blockName">Block name.</param>
        private void BuildExteriorLayout(ref string blockName)
        {
            // Get block key and name
            string name = host.BlockManager.BlocksFile.CheckName(blockName);
            int key = GetBlockKey(0, 0);

            // Create block position data
            BlockPosition blockPosition = new BlockPosition();
            blockPosition.name = name;
            blockPosition.block = host.BlockManager.LoadBlock(name);

            // Set block position
            blockPosition.position = new Vector3(0, 0, 0);

            // Build ground plane
            host.BlockManager.BuildRmbGroundPlane(host.TextureManager, ref blockPosition.block);

            // Add to layout array
            exteriorLayout[0] = blockPosition;
            exteriorLayoutCount = 1;

            // Clear batches dict
            exteriorBatches.Clear();

            // Add to layout dictionary
            exteriorLayoutDict.Clear();
            exteriorLayoutDict.Add(key, 0);

            // Bounds are equivalent to block
            BoundingBox bounds = blockPosition.block.BoundingBox;
            bounds.Min.Y = cameraFloorHeight;
            bounds.Max.Y = cameraCeilingHeight;
            exteriorTopDownCamera.Bounds = bounds;
            exteriorFreeCamera.Bounds = bounds;
        }

        /// <summary>
        /// Build dungeon layout from a single block.
        /// </summary>
        /// <param name="blockName">Block name.</param>
        private void BuildDungeonLayout(ref string blockName)
        {
            // Get block key
            int key = GetBlockKey(0, 0);

            // Create block position data
            BlockPosition blockPosition = new BlockPosition();
            blockPosition.name = blockName;
            blockPosition.block = host.BlockManager.LoadBlock(blockName);

            // Set block position
            blockPosition.position = new Vector3(0, 0, 0);

            // Add to layout array
            dungeonLayout[0] = blockPosition;
            dungeonLayoutCount = 1;

            // Clear batches dict
            dungeonBatches.Clear();

            // Add to layout dictionary
            dungeonLayoutDict.Clear();
            dungeonLayoutDict.Add(key, 0);

            // Set top down bounds to have a higher ceiling
            BoundingBox topDownBounds = blockPosition.block.BoundingBox;
            topDownBounds.Max.Y = cameraCeilingHeight;
            dungeonTopDownCamera.Bounds = topDownBounds;

            // Set initial free camera bounds
            SetDungeonFreeCameraBounds(blockPosition.block.BoundingBox);
        }

        /// <summary>
        /// Build exterior layout from a full location.
        /// </summary>
        /// <param name="dfLocation">DFLocation.</param>
        private void BuildExteriorLayout(ref DFLocation dfLocation)
        {
            // Get dimensions of exterior location array
            int width = dfLocation.Exterior.ExteriorData.Width;
            int height = dfLocation.Exterior.ExteriorData.Height;

            // Bounding box for layout
            BoundingBox bounds = new BoundingBox();

            // Reset layout dictionary
            exteriorLayoutDict.Clear();

            // Clear batches dict
            exteriorBatches.Clear();

            // Create exterior layout
            exteriorLayoutCount = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Get block key and name
                    string name = host.BlockManager.BlocksFile.CheckName(host.MapsFile.GetRmbBlockName(ref dfLocation, x, y));
                    int key = GetBlockKey(x, y);

                    // Create block position data
                    BlockPosition blockPosition = new BlockPosition();
                    blockPosition.name = name;
                    blockPosition.block = host.BlockManager.LoadBlock(name);
                    
                    // Set block position
                    blockPosition.position = new Vector3(x * rmbBlockSide, 0, -(y * rmbBlockSide));

                    // Build ground plane
                    host.BlockManager.BuildRmbGroundPlane(host.TextureManager, ref blockPosition.block);

                    // Add to layout array
                    exteriorLayout[exteriorLayoutCount] = blockPosition;

                    // Add to layout dictionary
                    exteriorLayoutDict.Add(key, exteriorLayoutCount);

                    // Increment count
                    exteriorLayoutCount++;

                    // Merge bounding boxes
                    Vector3 min = blockPosition.block.BoundingBox.Min + blockPosition.position;
                    Vector3 max = blockPosition.block.BoundingBox.Max + blockPosition.position;
                    bounds = BoundingBox.CreateMerged(bounds, new BoundingBox(min, max));
                }
            }

            // Set bounds
            bounds.Min.Y = cameraFloorHeight;
            bounds.Max.Y = cameraCeilingHeight;
            exteriorTopDownCamera.Bounds = bounds;
            exteriorFreeCamera.Bounds = bounds;
        }

        /// <summary>
        /// Build exterior layout from a full location.
        /// </summary>
        /// <param name="dfLocation">DFLocation.</param>
        private void BuildDungeonLayout(ref DFLocation dfLocation)
        {
            // Bounding box for layout
            BoundingBox bounds = new BoundingBox();

            // Reset layout dictionary
            dungeonLayoutDict.Clear();

            // Clear batches dict
            dungeonBatches.Clear();

            // Create dungeon layout
            dungeonLayoutCount = 0;
            foreach (var block in dfLocation.Dungeon.Blocks)
            {
                // Get block key
                int key = GetBlockKey(block.X, block.Z);

                // Some dungeons (e.g. Orsinium) encode more than one block with identical coordinates.
                // It is not yet known if Daggerfall uses the first or subsequent blocks.
                // We are using the first instance here until research shows otherwise.
                if (dungeonLayoutDict.ContainsKey(key))
                    continue;

                // Create block position data
                BlockPosition blockPosition = new BlockPosition();
                blockPosition.name = block.BlockName;
                blockPosition.block = host.BlockManager.LoadBlock(block.BlockName);

                // Set block position
                blockPosition.position = new Vector3(block.X * rdbBlockSide, 0f, -(block.Z * rdbBlockSide));

                // Add to layout array
                dungeonLayout[dungeonLayoutCount] = blockPosition;

                // Add to layout dictionary
                dungeonLayoutDict.Add(key, dungeonLayoutCount);

                // Increment count
                dungeonLayoutCount++;

                // Merge bounding boxes
                Vector3 min = blockPosition.block.BoundingBox.Min + blockPosition.position;
                Vector3 max = blockPosition.block.BoundingBox.Max + blockPosition.position;
                bounds = BoundingBox.CreateMerged(bounds, new BoundingBox(min, max));
            }

            // Set top down bounds to have a higher ceiling
            BoundingBox topDownBounds = bounds;
            topDownBounds.Max.Y = cameraCeilingHeight;
            dungeonTopDownCamera.Bounds = topDownBounds;

            // Set initial free camera bounds
            SetDungeonFreeCameraBounds(bounds);
        }

        /// <summary>
        /// Derive unique block key from coordinates.
        /// </summary>
        /// <param name="x">X position of block in layout.</param>
        /// <param name="z">Z position of block in layout</param>
        /// <returns>Block key.</returns>
        private int GetBlockKey(int x, int z)
        {
            return z * 100 + x;
        }

        /// <summary>
        /// Gets appropriate layout array to use based on batch mode.
        /// </summary>
        /// <param name="layout">Layout array output.</param>
        /// <param name="count">Layout count output.</param>
        private void GetLayoutArray(out BlockPosition[] layout, out int count)
        {
            // Reset outputs
            layout = null;
            count = 0;

            // Get layout information
            switch (batchMode)
            {
                case BatchModes.SingleExteriorBlock:
                case BatchModes.FullExterior:
                    if (exteriorLayoutCount > 0)
                    {
                        layout = exteriorLayout;
                        count = exteriorLayoutCount;
                    }
                    break;
                case BatchModes.SingleDungeonBlock:
                case BatchModes.FullDungeon:
                    if (dungeonLayoutCount > 0)
                    {
                        layout = dungeonLayout;
                        count = dungeonLayoutCount;
                    }
                    break;
            }
        }

        /// <summary>
        /// Gets appropriate layout dictionary based on batch mode.
        /// </summary>
        /// <param name="layoutDict">Layout dictionary output.</param>
        private void GetLayoutDict(out Dictionary<int, int> layoutDict)
        {
            // Get layout dictionary
            switch (batchMode)
            {
                case BatchModes.SingleExteriorBlock:
                case BatchModes.FullExterior:
                    layoutDict = exteriorLayoutDict;
                    break;
                case BatchModes.SingleDungeonBlock:
                case BatchModes.FullDungeon:
                    layoutDict = dungeonLayoutDict;
                    break;
                default:
                    layoutDict = new Dictionary<int, int>();
                    break;
            }
        }

        /// <summary>
        /// Gets appropriate batch dictionary based on batch mode.
        /// </summary>
        /// <param name="batches">Batch dictionary output.</param>
        private void GetBatches(out Dictionary<int, BatchItemArray> batches)
        {
            // Get layout dictionary
            switch (batchMode)
            {
                case BatchModes.SingleExteriorBlock:
                case BatchModes.FullExterior:
                    batches = exteriorBatches;
                    break;
                case BatchModes.SingleDungeonBlock:
                case BatchModes.FullDungeon:
                    batches = dungeonBatches;
                    break;
                default:
                    batches = new Dictionary<int, BatchItemArray>();
                    break;
            }
        }

        #endregion

        #region Resource Loading

        /// <summary>
        /// Ensures block content is loaded and bounding box correctly sized.
        /// </summary>
        /// <param name="block">BlockManager.Block.</param>
        private bool UpdateBlock(ref BlockManager.Block block)
        {
            // Get batches
            Dictionary<int, BatchItemArray> batches;
            GetBatches(out batches);

            // Load model textures
            int textureKey;
            Vector3 min, max;
            float minVertical = float.MaxValue, maxVertical = float.MinValue;
            for (int i = 0; i < block.Models.Count; i++)
            {
                // Get model info
                BlockManager.ModelInfo info = block.Models[i];

                // Load model resource
                ModelManager.Model model;
                host.ModelManager.LoadModel(info.ModelId, out model);

                // Load texture resources for this model
                for (int sm = 0; sm < model.SubMeshes.Length; sm++)
                {
                    // Load texture for this submesh
                    textureKey = host.TextureManager.LoadTexture(
                        model.SubMeshes[sm].TextureArchive,
                        model.SubMeshes[sm].TextureRecord);

                    // Store texture key in model
                    model.SubMeshes[sm].TextureKey = textureKey;

                    // Start a new batch if this is a texture we haven't seen before
                    if (!batches.ContainsKey(textureKey))
                    {
                        // Create new batch array.
                        // These arrays are of a fixed length as dynamic
                        // objects create lots of garbage at run-time.
                        // The Length is simply reset and the array reused
                        // each frame.
                        BatchItemArray batchArray;
                        batchArray.Length = 0;
                        batchArray.BatchItems = new BatchItem[batchArrayLength];
                        batches.Add(textureKey, batchArray);
                    }
                }

                // Set model info bounds
                info.BoundingBox = model.BoundingBox;
                info.BoundingSphere = model.BoundingSphere;

                // Track position of transformed model for correct vertical bounds
                min = Vector3.Transform(model.BoundingBox.Min, info.Matrix);
                max = Vector3.Transform(model.BoundingBox.Max, info.Matrix);
                if (min.Y < minVertical) minVertical = min.Y;
                if (max.Y > maxVertical) maxVertical = max.Y;

                // Set model info
                block.Models[i] = info;
            }

            // Ensure vertical limits are not still at test values.
            // This can happen when a block has zero models.
            if (minVertical == float.MaxValue) minVertical = 0;
            if (maxVertical == float.MinValue) maxVertical = 256f;

            // Update block vertical bounds
            block.BoundingBox.Min.Y = minVertical;
            block.BoundingBox.Max.Y = maxVertical;

            // Update dungeon camera vertical limits
            if (batchMode == BatchModes.SingleDungeonBlock ||
                batchMode == BatchModes.FullDungeon)
            {
                UpdateDungeonFreeCameraBounds(block.BoundingBox);
            }

            // Clear update flag
            block.UpdateRequired = false;

            return true;
        }

        #endregion

        #region Camera Methods

        /// <summary>
        /// Gets active camera based on batch mode and camera mode.
        /// </summary>
        /// <returns>Camera.</returns>
        private Camera GetActiveCamera()
        {
            switch (batchMode)
            {
                case BatchModes.SingleExteriorBlock:
                case BatchModes.FullExterior:
                    return (cameraMode == CameraModes.Free) ? exteriorFreeCamera : exteriorTopDownCamera;
                case BatchModes.SingleDungeonBlock:
                case BatchModes.FullDungeon:
                default:
                    return (cameraMode == CameraModes.Free) ? dungeonFreeCamera : dungeonTopDownCamera;
            }
        }

        /// <summary>
        /// Initialise camera positions.
        /// </summary>
        private void InitCameraPosition()
        {
            InitTopDownCameraPosition();
            InitFreeCameraPosition();
        }

        /// <summary>
        /// Sets top-down camera to starting position.
        /// </summary>
        private void InitTopDownCameraPosition()
        {
            // Reset top down cameras
            exteriorTopDownCamera.ResetReference();
            dungeonTopDownCamera.ResetReference();

            // Set position
            exteriorTopDownCamera.CentreInBounds(cameraStartHeight);
            dungeonTopDownCamera.CentreInBounds(cameraStartHeight);

            // Set reference
            exteriorTopDownCamera.Reference = new Vector3(0f, -1.0f, -0.01f);
            dungeonTopDownCamera.Reference = new Vector3(0f, -1.0f, -0.01f);

            // Update
            exteriorTopDownCamera.Update(Camera.UpdateFlags.None);
            dungeonTopDownCamera.Update(Camera.UpdateFlags.None);
        }

        /// <summary>
        /// Sets free camera to starting position.
        /// </summary>
        private void InitFreeCameraPosition()
        {
            // Reset free cameras
            exteriorFreeCamera.ResetReference();
            dungeonFreeCamera.ResetReference();

            // Set exterior position
            Vector3 exteriorPos = new Vector3(
                exteriorFreeCamera.Bounds.Max.X / 2,
                cameraFloorHeight,
                exteriorFreeCamera.Bounds.Max.Z);
            exteriorFreeCamera.Position = exteriorPos;

            // Set dungeon free camera position
            Vector3 dungeonPos = new Vector3(
                dungeonFreeCamera.Bounds.Min.X + (dungeonFreeCamera.Bounds.Max.X - dungeonFreeCamera.Bounds.Min.X) / 2,
                1024f,
                dungeonFreeCamera.Bounds.Max.Z);
            dungeonFreeCamera.Position = dungeonPos;

            // Set reference
            exteriorFreeCamera.Reference = new Vector3(0f, 0f, -1f);
            dungeonFreeCamera.Reference = new Vector3(0f, 0f, -1f);

            // Update
            exteriorFreeCamera.Update(Camera.UpdateFlags.None);
            dungeonFreeCamera.Update(Camera.UpdateFlags.None);
        }

        /// <summary>
        /// Conditionally updates cameras.
        /// </summary>
        private void UpdateCameras()
        {
            // Update based on camera mode
            if (CameraMode == CameraModes.Free)
            {
                // Set input flags
                Camera.UpdateFlags flags = Camera.UpdateFlags.None;
                if (host.Focused)
                {
                    flags |= Camera.UpdateFlags.Keyboard;
                }
                if (host.MouseInClientArea)
                {
                    flags |= Camera.UpdateFlags.Mouse;
                }

                // Update based on batch mode
                switch (batchMode)
                {
                    case BatchModes.SingleExteriorBlock:
                    case BatchModes.FullExterior:
                        exteriorFreeCamera.Update(flags);
                        break;
                    case BatchModes.SingleDungeonBlock:
                    case BatchModes.FullDungeon:
                        dungeonFreeCamera.Update(flags);
                        break;

                }
            }
            else if (CameraMode == CameraModes.TopDown)
            {
                // Apply camera velocity
                ActiveCamera.Translate(cameraVelocity.X, 0f, cameraVelocity.Z);

                // Update based on batch mode
                switch (batchMode)
                {
                    case BatchModes.SingleExteriorBlock:
                    case BatchModes.FullExterior:
                        exteriorTopDownCamera.Update(Camera.UpdateFlags.None);
                        break;
                    case BatchModes.SingleDungeonBlock:
                    case BatchModes.FullDungeon:
                        dungeonTopDownCamera.Update(Camera.UpdateFlags.None);
                        break;
                }
            }
        }

        /// <summary>
        /// Set dungeon free camera bounds.
        ///  This is done at initial layout.
        /// </summary>
        /// <param name="bounds">BoundingBox.</param>
        private void SetDungeonFreeCameraBounds(BoundingBox bounds)
        {
            bounds.Min.X -= cameraDungeonFreedom;
            bounds.Max.X += cameraDungeonFreedom;
            bounds.Min.Y -= cameraDungeonFreedom;
            bounds.Max.Y += cameraDungeonFreedom;
            bounds.Min.Z -= cameraDungeonFreedom;
            bounds.Max.Z += cameraDungeonFreedom;
            dungeonFreeCamera.Bounds = bounds;
        }

        /// <summary>
        /// Update dungeon free camera bounds.
        ///  This is done after loading block resources.
        /// </summary>
        /// <param name="bounds">BoundingBox.</param>
        private void UpdateDungeonFreeCameraBounds(BoundingBox bounds)
        {
            BoundingBox newBounds = dungeonFreeCamera.Bounds;

            if (bounds.Min.Y < newBounds.Min.Y)
                newBounds.Min.Y = bounds.Min.Y - cameraDungeonFreedom;
            if (bounds.Max.Y > newBounds.Max.Y)
                newBounds.Max.Y = bounds.Max.Y + cameraDungeonFreedom;

            dungeonFreeCamera.Bounds = newBounds;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates projection matrix to current view size.
        /// </summary>
        private void UpdateProjectionMatrix()
        {
            // Update aspect ratio for all cameras
            float aspectRatio = (float)host.ClientRectangle.Width / (float)host.ClientRectangle.Height;
            exteriorTopDownCamera.SetAspectRatio(aspectRatio);
            exteriorFreeCamera.SetAspectRatio(aspectRatio);
            dungeonTopDownCamera.SetAspectRatio(aspectRatio);
            dungeonFreeCamera.SetAspectRatio(aspectRatio);
        }

        /// <summary>
        /// Updates status message.
        /// </summary>
        private void UpdateStatusMessage()
        {
            // Set the message
            host.StatusMessage = currentStatus;
        }

        /// <summary>
        /// Sets batch mode.
        /// </summary>
        /// <param name="mode"></param>
        private void ChangeBatchMode(BatchModes mode)
        {
            // Apply mode
            batchMode = mode;
        }

        #endregion

    }

}
