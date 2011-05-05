// Project:         DaggerfallModelling
// Description:     Explore and export 3D models from Daggerfall.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
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
using DaggerfallModelling.ViewComponents;
#endregion

namespace DaggerfallModelling.ViewControls
{

    /// <summary>
    /// Explore a location from a single block to full cities and dungeons.
    ///  This view takes a DFBlock or DFLocation to build scene layout.
    ///  Provides scene culling, state batching, picking, and collisions.
    /// </summary>
    public class LocationView : ViewBase
    {

        #region Class Variables

        // Scene
        private const float rmbBlockSide = 4096.0f;
        private const float rdbBlockSide = 2048.0f;
        private BlockPosition[] exteriorLayout = new BlockPosition[64];
        private BlockPosition[] dungeonLayout = new BlockPosition[32];
        private int exteriorLayoutCount = 0;
        private int dungeonLayoutCount = 0;

        // Scene block lookup
        private Dictionary<int, int> exteriorLayoutDict = new Dictionary<int, int>();
        private Dictionary<int, int> dungeonLayoutDict = new Dictionary<int, int>();

        // Batching
        private const int batchArrayLength = 1024;
        private BatchModes batchMode = BatchModes.Exterior;
        private Batches batches = new Batches();

        // Batch options
        private BatchOptions batchOptions =
            BatchOptions.SkyPlane | BatchOptions.GroundPlane |
            BatchOptions.MiscFlats | BatchOptions.MousePicking;

        // Components
        private SkyComponent skyManager;
        private BillboardComponent billboardManager;

        // Ground height
        private int groundHeight = -7;

        // Location
        private DFLocation? currentLocation = null;
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

        // Intersection and collision
        private Intersection intersection = new Intersection();
        private const int defaultIntersectionCapacity = 35;
        private uint? mouseOverModel = null;
        private List<ModelIntersection> modelPointerIntersections;
        private List<ModelIntersection> modelCameraIntersections;

        // Line drawing
        VertexPositionColor[] planeLines = new VertexPositionColor[64];

        // Bounding volume drawing
        private RenderableBoundingBox renderableBoundingBox;
        private RenderableBoundingSphere renderableBoundingSphere;
        
#if DEBUG
        // Performance profiling
        private StringBuilder perfStatusBuilder = new StringBuilder(256);
        private WeakReference gcTracker = new WeakReference(new object());
        private int visibleTriangles;
        private int visibleBatches;
        private int maxBatchArrayLength;
        private int garbageCollections = 0;
#endif

        #endregion

        #region Class Structures

        /// <summary>
        /// Specifies which scene layout data should be batched for rendering.
        /// </summary>
        public enum BatchModes
        {
            /// <summary>Render exterior.</summary>
            Exterior,
            /// <summary>Render dungeon.</summary>
            Dungeon,
            /// <summary>Render interior.</summary>
            Interior,
        }

        /// <summary>
        /// Flags for optional data to be batched for rendering.
        /// </summary>
        [Flags]
        public enum BatchOptions
        {
            /// <summary>No flags set.</summary>
            None = 0,
            /// <summary>Render sky behind exterior blocks.</summary>
            SkyPlane = 1,
            /// <summary>Render ground plane below exterior blocks.</summary>
            GroundPlane = 2,
            /// <summary>Render miscellaneous flats (e.g. trees, rocks, NPCs, animals).</summary>
            MiscFlats = 4,
            /// <summary>Render editor flats (e.g. monster spawn points, start position).</summary>
            EditorFlats = 8,
            /// <summary>Mouse-model picking.</summary>
            MousePicking = 16,
            /// <summary>Controller-model picking.</summary>
            ControllerPicking = 32,
        }

        /// <summary>
        /// Contains batches of visible items for renderer.
        /// </summary>
        private struct Batches
        {
            public Dictionary<int, BatchModelArray> Models;
        }

        /// <summary>
        /// Stores a fixed array of submeshes which is allocated
        ///  once to minimise garbage collections later.
        /// </summary>
        private struct BatchModelArray
        {
            public int Length;
            public BatchModelItem[] BatchItems;
        }

        /// <summary>
        /// Represents a visible submesh.
        /// </summary>
        private struct BatchModelItem
        {
            public bool Indexed;
            public Matrix ModelTransform;
            public VertexBuffer VertexBuffer;
            public IndexBuffer IndexBuffer;
            public VertexPositionNormalTexture[] Vertices;
            public short[] Indices;
            public int StartIndex;
            public int PrimitiveCount;
        }

        /// <summary>
        /// Describes how a block is positioned in world space.
        /// </summary>
        private struct BlockPosition
        {
            public string name;
            public Vector3 position;
            public BlockManager.BlockData block;
        }

        #endregion

        #region SubClasses

        /// <summary>
        /// Describes a model that has intersected with something.
        ///  Used when sorting intersections for face-accurate picking
        ///  and collision tests.
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

        /// <summary>
        /// Gets sky manager.
        /// </summary>
        public SkyComponent SkyManager
        {
            get { return skyManager; }
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

            // Create model intersections lists
            modelPointerIntersections = new List<ModelIntersection>();
            modelCameraIntersections = new List<ModelIntersection>();
        }

        #endregion

        #region Abstract Overrides

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

            // Create sky component
            skyManager = new SkyComponent(host);
            skyManager.Initialize();
            skyManager.Camera = exteriorFreeCamera;
            skyManager.Enabled = true;

            // Create billboard component
            billboardManager = new BillboardComponent(host);
            billboardManager.Initialize();
            billboardManager.Camera = exteriorFreeCamera;
            billboardManager.Enabled = true;
        }

        /// <summary>
        /// Called by host when view should update animation.
        /// </summary>
        public override void Update()
        {
            // Update camera
            UpdateCamera();

            // Test camera collision
            WorldCameraIntersectionTest();

            // Apply camera changes
            ActiveCamera.ApplyChanges();

            // Update components
            skyManager.Update();
            billboardManager.Update();

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
            long startTime = host.Timer.ElapsedMilliseconds;
            visibleBatches = 0;
            visibleTriangles = 0;
            maxBatchArrayLength = 0;
#endif

            // Clear device
            ClearDevice();

            // Draw world geometry
            if (batches.Models != null)
            {
                ClearBatches();
                BatchScene();
                SetRenderStates();
                DrawBatches();
            }

            // Test pointer intersection
            ModelPointerIntersectionTest();

            // Draw billboards
            billboardManager.Draw();

#if DEBUG
            // The String.Format here creates nearly all the garbage collections reported.
            // TODO: Implement a garbage-free means of reporting this information.
            //long drawTime = host.Timer.ElapsedMilliseconds - startTime;
            //string performance = string.Format(
            //    "MaxBillboardBatchLength={0}, DrawTime={1}ms, FPS={2:0.00}",
            //    billboardManager.MaxBatchLength,
            //    drawTime,
            //    host.FPS);
            //host.StatusMessage = performance;
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

        #endregion

        #region Overrides

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
            if (batchMode == BatchModes.Exterior)
                host.TextureManager.Climate = base.Climate;
            else
                host.TextureManager.Climate = DFLocation.ClimateType.None;

            // Clear scroll velocity
            cameraVelocity = Vector3.Zero;

            // Resume view
            host.ModelManager.CacheModelData = true;
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
            ActiveCamera.NextPosition = pos;
            ActiveCamera.ApplyChanges();
        }

        #endregion

        #region Rendering Pipeline

        /// <summary>
        /// Clear graphics device buffer.
        /// </summary>
        private void ClearDevice()
        {
            // All camera modes but free are just cleared
            if (cameraMode != CameraModes.Free)
            {
                host.GraphicsDevice.Clear(backgroundColor);
                return;
            }

            // Free camera dungeons are cleared black
            if (batchMode == BatchModes.Dungeon)
            {
                host.GraphicsDevice.Clear(Color.Black);
                return;
            }

            // Draw sky enabled
            if (BatchOptions.SkyPlane == (batchOptions & BatchOptions.SkyPlane))
            {
                host.GraphicsDevice.Clear(skyManager.ClearColor);
                skyManager.Draw();
                return;
            }

            // If all else fails just clear
            host.GraphicsDevice.Clear(backgroundColor);
        }

        /// <summary>
        /// Clears batches. These are rebuild every frame from
        ///  visible objects.
        /// </summary>
        private void ClearBatches()
        {
            // Clear each batch list
            List<int> keys = new List<int>(batches.Models.Keys);
            foreach (int key in keys)
            {
                // Zero out length of each array
                BatchModelArray batchArray = batches.Models[key];
                batchArray.Length = 0;
                batches.Models[key] = batchArray;
            }

            // Clear billboard batch
            billboardManager.ClearBatch();
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

            // Reset model intersection lists
            modelPointerIntersections.Clear();
            modelPointerIntersections.Capacity = defaultIntersectionCapacity;

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
        /// Step through block to find visible models.
        /// </summary>
        /// <param name="block">BlockManager.Block.</param>
        /// <param name="blockTransform">Block transform.</param>
        /// <param name="mouseInBlock">True if mouse ray in block bounds.</param>
        private void BatchBlock(ref BlockManager.BlockData block, ref Matrix blockTransform, bool mouseInBlock)
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
                        modelPointerIntersections.Add(mi);
                    }
                }

                // Add the model
                ModelManager.ModelData model = host.ModelManager.GetModelData(modelInfo.ModelId);
                BatchModel(ref model, ref modelTransform);

                // Increment index
                modelIndex++;
            }

            // Batch misc flats
            if (cameraMode == CameraModes.Free &&
                BatchOptions.MiscFlats == (batchOptions & BatchOptions.MiscFlats))
            {
                BatchFlats(ref block, blockTransform);
            }

            // Optional exterior batching
            if (batchMode == BatchModes.Exterior)
            {
                // Batch ground plane
                if (BatchOptions.GroundPlane == (batchOptions & BatchOptions.GroundPlane))
                {
                    // Translate ground down a few units to reduce
                    // z-fighting with other ground-aligned planes
                    Matrix groundTransform = blockTransform * Matrix.CreateTranslation(0, groundHeight, 0);

                    // Draw ground plane
                    BatchGroundPlane(ref block, ref groundTransform);
                }
            }
        }

        /// <summary>
        /// Step through a model and add triangles to batch.
        /// </summary>
        /// <param name="model">ModelManager.Model.</param>
        /// <param name="modelTransform">Model transform.</param>
        private void BatchModel(ref ModelManager.ModelData model, ref Matrix modelTransform)
        {
            // Exit if no model loaded
            if (model.Vertices == null)
                return;

            // Batch submeshes
            foreach (var subMesh in model.SubMeshes)
            {
#if DEBUG
                // Increment triangle counter
                visibleTriangles += subMesh.PrimitiveCount;
#endif

                // Add subMesh to batch
                if (batches.Models.ContainsKey(subMesh.TextureKey))
                {
                    // Add reference to vertex and index data to batch
                    BatchModelArray batchArray = batches.Models[subMesh.TextureKey];
                    int index = batchArray.Length;
                    batchArray.BatchItems[index].Indexed = true;
                    batchArray.BatchItems[index].ModelTransform = modelTransform;
                    batchArray.BatchItems[index].VertexBuffer = model.VertexBuffer;
                    batchArray.BatchItems[index].IndexBuffer = model.IndexBuffer;
                    batchArray.BatchItems[index].Vertices = model.Vertices;
                    batchArray.BatchItems[index].Indices = model.Indices;
                    batchArray.BatchItems[index].StartIndex = subMesh.StartIndex;
                    batchArray.BatchItems[index].PrimitiveCount = subMesh.PrimitiveCount;
                    batchArray.Length++;
                    batches.Models[subMesh.TextureKey] = batchArray;
                }
            }
        }

        /// <summary>
        /// Batch block flats.
        /// </summary>
        /// <param name="block">BlockManager.Block.</param>
        /// <param name="blockTransform">Matrix.</param>
        private void BatchFlats(ref BlockManager.BlockData block, Matrix blockTransform)
        {
            // Iterate through flats
            Matrix flatTransform;
            BoundingSphere flatBounds;
            foreach (var flat in block.Flats)
            {
                // Create transformed bounding sphere
                flatTransform = Matrix.CreateTranslation(flat.Position) * blockTransform;
                flatBounds.Center = Vector3.Transform(flat.BoundingSphere.Center, flatTransform);
                flatBounds.Radius = flat.BoundingSphere.Radius;

                // TEST: Draw bounds
                //renderableBoundingSphere.Color = Color.Red;
                //renderableBoundingSphere.Draw(flatBounds, ActiveCamera.View, ActiveCamera.Projection, Matrix.Identity);

                // Test bounds against camera frustum
                if (!viewFrustum.Intersects(flatBounds))
                    continue;

                // Add to batch
                billboardManager.AddToBatch(
                    flat.Origin,
                    flat.Position,
                    flat.Size,
                    flat.TextureKey,
                    blockTransform);
            }
        }

        /// <summary>
        /// Batch ground plane.
        /// </summary>
        /// <param name="block">BlockManager.Block.</param>
        /// <param name="groundTransform">Ground transform.</param>
        private void BatchGroundPlane(ref BlockManager.BlockData block, ref Matrix groundTransform)
        {
            // Exit if no ground plane loaded
            if (block.GroundPlaneVertices == null)
                return;

#if DEBUG
            // Increment triangle counter
            visibleTriangles += block.GroundPlaneVertices.Length / 3;
#endif

            // Add ground plane to model batch
            if (batches.Models.ContainsKey(TextureManager.TerrainAtlasKey))
            {
                // Add reference to vertex and index data to batch
                BatchModelArray batchArray = batches.Models[TextureManager.TerrainAtlasKey];
                int index = batchArray.Length;
                batchArray.BatchItems[index].Indexed = false;
                batchArray.BatchItems[index].ModelTransform = groundTransform;
                batchArray.BatchItems[index].VertexBuffer = block.GroundPlaneVertexBuffer;
                batchArray.BatchItems[index].IndexBuffer = null;
                batchArray.BatchItems[index].Vertices = block.GroundPlaneVertices;
                batchArray.BatchItems[index].Indices = null;
                batchArray.BatchItems[index].StartIndex = 0;
                batchArray.BatchItems[index].PrimitiveCount = block.GroundPlaneVertices.Length / 3;
                batchArray.Length++;
                batches.Models[TextureManager.TerrainAtlasKey] = batchArray;
            }
        }

        /// <summary>
        /// Draw native mesh as wireframe lines.
        /// </summary>
        /// <param name="color">Line color.</param>
        /// <param name="model">ModelManager.Model.</param>
        /// <param name="matrix">Matrix.</param>
        private void DrawNativeMesh(Color color, ref ModelManager.ModelData model, Matrix matrix)
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
        private void DrawNativeFace(Color color, DFMesh.DFPoint[] points, Matrix matrix)
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
            // Set vertex declaration
            host.GraphicsDevice.VertexDeclaration = modelVertexDeclaration;

            // Update view and projection matrices
            modelEffect.View = ActiveCamera.View;
            modelEffect.Projection = ActiveCamera.Projection;

            // Set sampler state
            host.GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            host.GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            // Iterate batch lists
            foreach (var item in batches.Models)
            {
                BatchModelArray batchArray = item.Value;

                // Do nothing if batch empty
                if (batchArray.Length == 0)
                    continue;

#if DEBUG
                // Update max length of batch array
                if (batchArray.Length > maxBatchArrayLength)
                    maxBatchArrayLength = batchArray.Length;
#endif

                modelEffect.Texture = host.TextureManager.GetTexture(item.Key);

                modelEffect.Begin();
                modelEffect.CurrentTechnique.Passes[0].Begin();

                // Iterate batch items
                BatchModelItem batchItem;
                for (int i = 0; i < batchArray.Length; i++)
                {
                    // Get batch item
                    batchItem = batchArray.BatchItems[i];

                    // Set vertex buffer
                    host.GraphicsDevice.Vertices[0].SetSource(
                        batchItem.VertexBuffer,
                        0,
                        VertexPositionNormalTexture.SizeInBytes);

                    modelEffect.World = batchItem.ModelTransform;
                    modelEffect.CommitChanges();

                    // Draw based on indexed flag
                    if (batchItem.Indexed)
                    {
                        // Set index buffer
                        host.GraphicsDevice.Indices = batchItem.IndexBuffer;

                        // Draw indexed primitives
                        host.GraphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        0,
                        0,
                        batchItem.Vertices.Length,
                        batchItem.StartIndex,
                        batchItem.PrimitiveCount);
                    }
                    else
                    {
                        // Draw primitives
                        host.GraphicsDevice.DrawPrimitives(
                            PrimitiveType.TriangleList,
                            batchItem.StartIndex,
                            batchItem.PrimitiveCount);
                    }
                }

                modelEffect.CurrentTechnique.Passes[0].End();
                modelEffect.End();

#if DEBUG
                // Update visible batch count
                visibleBatches++;
#endif

            }
        }

        #endregion

        #region Intersection Tests

        /// <summary>
        /// Tests pointer against model intersections to
        ///  determine actual intersection at face level.
        /// </summary>
        private void ModelPointerIntersectionTest()
        {
            // Nothing to do if no intersections
            if (modelPointerIntersections.Count == 0)
                return;

            // Sort intersections by distance
            modelPointerIntersections.Sort();

            // Iterate intersections
            float? intersection = null;
            float? closestIntersection = null;
            ModelIntersection closestModelIntersection = null;
            foreach (var mi in modelPointerIntersections)
            {
                // Get model
                ModelManager.ModelData model = host.ModelManager.GetModelData(mi.ModelID.Value);

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
                ModelManager.ModelData model = host.ModelManager.GetModelData(closestModelIntersection.ModelID.Value);
                DrawNativeMesh(Color.Gold, ref model, closestModelIntersection.Matrix);

                // Store modelid of model under mouse
                mouseOverModel = closestModelIntersection.ModelID;
            }
        }

        /// <summary>
        /// Step through blocks to find camera-block intersections.
        /// </summary>
        private void WorldCameraIntersectionTest()
        {
            // Get batch layout data            
            int count;
            BlockPosition[] layout;
            GetLayoutArray(out layout, out count);
            if (layout == null)
                return;

            // Reset camera intersection lists
            modelCameraIntersections.Clear();
            modelCameraIntersections.Capacity = defaultIntersectionCapacity;

            // Step through block layout
            Matrix blockTransform;
            BoundingBox blockBounds;
            for (int i = 0; i < count; i++)
            {
                // Create transformed block bounding box
                blockTransform = Matrix.CreateTranslation(layout[i].position);
                blockBounds.Min = Vector3.Transform(layout[i].block.BoundingBox.Min, blockTransform);
                blockBounds.Max = Vector3.Transform(layout[i].block.BoundingBox.Max, blockTransform);

                // Test camera against block bounds.
                if (blockBounds.Intersects(ActiveCamera.BoundingSphere))
                    BlockCameraIntersectionTest(ref layout[i].block, ref blockTransform);
            }

            // We now have a list of all model bounds that intersect
            // with camera bounds. Test these models for face intersections.
            ModelCameraIntersectionTest();
        }

        /// <summary>
        /// Step through block to find camera-model intersections.
        /// </summary>
        /// <param name="block">BlockManager.Block</param>
        /// <param name="blockTransform">Block transform.</param>
        private void BlockCameraIntersectionTest(ref BlockManager.BlockData block, ref Matrix blockTransform)
        {
            // Iterate each model in this block
            Matrix modelTransform;
            BoundingSphere cameraBounds = ActiveCamera.BoundingSphere;
            BoundingSphere modelBounds;
            float intersectDistance;
            foreach (var modelInfo in block.Models)
            {
                // Create transformed model bounding sphere
                modelTransform = modelInfo.Matrix * blockTransform;
                modelBounds.Center = Vector3.Transform(modelInfo.BoundingSphere.Center, modelTransform);
                modelBounds.Radius = modelInfo.BoundingSphere.Radius;

                // Test if camera collides with model sphere
                if (modelBounds.Intersects(cameraBounds))
                {
                    // Get distance between camera and model spheres
                    intersectDistance = Vector3.Distance(cameraBounds.Center, modelBounds.Center);

                    // Add to intersection list
                    ModelIntersection mi = new ModelIntersection(
                        intersectDistance,
                        modelInfo.ModelId,
                        modelTransform);
                    modelCameraIntersections.Add(mi);
                }
            }
        }

        /// <summary>
        /// Determine camera-model intersection at face level.
        /// </summary>
        private void ModelCameraIntersectionTest()
        {
            // Nothing to do if no intersections
            if (modelCameraIntersections.Count == 0)
                return;

            // Sort intersections by distance
            modelCameraIntersections.Sort();

            // Iterate intersections
            Intersection.CollisionResult result;
            foreach (var mi in modelCameraIntersections)
            {
                // Get model
                ModelManager.ModelData model = host.ModelManager.GetModelData(mi.ModelID.Value);

                // Test intersection
                result = intersection.SphereIntersectDFMesh(
                    ActiveCamera.BoundingSphere,
                    mi.Matrix,
                    ref model);

                // TODO: Modify camera position on intersection
                if (result.Hit)
                {
                }
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

            // Reset batches
            batches.Models = new Dictionary<int, BatchModelArray>();
            
            // Build single-block exterior layout
            BuildExteriorLayout(ref blockName);

            // Set status message
            currentStatus = string.Format("Viewing RMB block {0}.", blockName);

            // Store block name
            currentBlockName = blockName;

            // Clear location
            currentLocation = null;

            // Set default sky
            skyManager.SkyIndex = SkyComponent.DefaultSkyIndex;

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

            // Reset batches
            batches.Models = new Dictionary<int, BatchModelArray>();

            // Build single-block layout
            BuildDungeonLayout(ref blockName);

            // Set status message
            currentStatus = string.Format("Viewing RDB block {0}.", blockName);

            // Store block name
            currentBlockName = blockName;

            // Clear location
            currentLocation = null;

            // Set default sky
            skyManager.SkyIndex = SkyComponent.DefaultSkyIndex;

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

            // Reset batches
            batches.Models = new Dictionary<int, BatchModelArray>();

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

            // Store location
            currentLocation = dfLocation;

            // Set climate sky
            skyManager.SkyIndex = dfLocation.SkyArchive;

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

            // Add to layout dictionary
            exteriorLayoutDict.Clear();
            exteriorLayoutDict.Add(key, 0);

            // Bounds are equivalent to block
            BoundingBox bounds = blockPosition.block.BoundingBox;
            bounds.Min.Y = cameraFloorHeight;
            bounds.Max.Y = cameraCeilingHeight;
            exteriorTopDownCamera.MovementBounds = bounds;
            exteriorFreeCamera.MovementBounds = bounds;
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

            // Add to layout dictionary
            dungeonLayoutDict.Clear();
            dungeonLayoutDict.Add(key, 0);

            // Set top down bounds to have a higher ceiling
            BoundingBox topDownBounds = blockPosition.block.BoundingBox;
            topDownBounds.Max.Y = cameraCeilingHeight;
            dungeonTopDownCamera.MovementBounds = topDownBounds;

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
            exteriorTopDownCamera.MovementBounds = bounds;
            exteriorFreeCamera.MovementBounds = bounds;
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
            dungeonTopDownCamera.MovementBounds = topDownBounds;

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
                case BatchModes.Exterior:
                    if (exteriorLayoutCount > 0)
                    {
                        layout = exteriorLayout;
                        count = exteriorLayoutCount;
                    }
                    break;
                case BatchModes.Dungeon:
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
                case BatchModes.Exterior:
                    layoutDict = exteriorLayoutDict;
                    break;
                case BatchModes.Dungeon:
                    layoutDict = dungeonLayoutDict;
                    break;
                default:
                    layoutDict = new Dictionary<int, int>();
                    break;
            }
        }

        #endregion

        #region Resource Loading

        /// <summary>
        /// Ensures block content is loaded and bounding box correctly sized.
        /// </summary>
        /// <param name="block">BlockManager.Block.</param>
        private bool UpdateBlock(ref BlockManager.BlockData block)
        {
            // Load block models
            int textureKey;
            Vector3 min, max;
            float minVertical = float.MaxValue, maxVertical = float.MinValue;
            for (int i = 0; i < block.Models.Count; i++)
            {
                // Get model info
                BlockManager.BlockModel info = block.Models[i];

                // Load model resource
                ModelManager.ModelData model;
                if (!host.ModelManager.GetModelData(info.ModelId, out model))
                    return false;

                // Load texture resources for this model
                for (int sm = 0; sm < model.SubMeshes.Length; sm++)
                {
                    // Load texture for this submesh
                    textureKey = host.TextureManager.LoadTexture(
                        model.DFMesh.SubMeshes[sm].TextureArchive,
                        model.DFMesh.SubMeshes[sm].TextureRecord,
                        TextureManager.TextureCreateFlags.ApplyClimate |
                        TextureManager.TextureCreateFlags.MipMaps |
                        TextureManager.TextureCreateFlags.PowerOfTwo);

                    // Store texture key in model
                    model.SubMeshes[sm].TextureKey = textureKey;

                    // Start a new batch if this is a texture we haven't seen before
                    if (!batches.Models.ContainsKey(textureKey))
                    {
                        // Create new batch array.
                        // These arrays are of a fixed length as new
                        // objects create lots of garbage at run-time.
                        // The Length is simply reset and the array reused
                        // each frame.
                        BatchModelArray batchArray;
                        batchArray.Length = 0;
                        batchArray.BatchItems = new BatchModelItem[batchArrayLength];
                        batches.Models.Add(textureKey, batchArray);
                    }

                    // Start a ground plane batch if not present
                    if (!batches.Models.ContainsKey(TextureManager.TerrainAtlasKey))
                    {
                        // Create a terrain atlas batch.
                        // Sized to 64 items as there can only be one
                        // item per block and never more than 64 blocks.
                        BatchModelArray batchArray;
                        batchArray.Length = 0;
                        batchArray.BatchItems = new BatchModelItem[64];
                        batches.Models.Add(TextureManager.TerrainAtlasKey, batchArray);
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

            // Load block flats
            for (int i = 0; i < block.Flats.Count; i++)
            {
                // Get flat info
                BlockManager.BlockFlat info = block.Flats[i];

                // Set climate ground flats archive if this is a scenery flat
                if (info.FlatType == BlockManager.FlatType.Scenery)
                {
                    // Just use temperate (504) if no location set
                    if (currentLocation == null)
                        info.TextureArchive = 504;
                    else
                        info.TextureArchive = currentLocation.Value.GroundFlatsArchive;
                }

                // Load texture for this flat
                textureKey = host.TextureManager.LoadTexture(
                    info.TextureArchive,
                    info.TextureRecord,
                    TextureManager.TextureCreateFlags.Dilate |
                    TextureManager.TextureCreateFlags.PreMultiplyAlpha);

                // Get dimensions and scale of this texture image
                // We do this as TextureManager may have just pulled texture key from cache.
                // without loading file. We need the following information to create bounds.
                string path = Path.Combine(host.Arena2Path, TextureFile.IndexToFileName(info.TextureArchive));
                System.Drawing.Size size = TextureFile.QuickSize(path, info.TextureRecord);
                System.Drawing.Size scale = TextureFile.QuickScale(path, info.TextureRecord);

                // Store texture key in flat
                info.TextureKey = textureKey;

                info.Size.X = size.Width;
                info.Size.Y = size.Height;

                // Apply scale
                if (info.BlockType == DFBlock.BlockTypes.Rdb && info.TextureArchive > 499)
                {
                    // Foliage (TEXTURE.500 and up) do not seem to use scaling
                    // in dungeons. Disable scaling for now.
                    info.Size.X = size.Width;
                    info.Size.Y = size.Height;
                }
                else
                {
                    // Scale billboard
                    int xChange = (int)(size.Width * (scale.Width / 256.0f));
                    int yChange = (int)(size.Height * (scale.Height / 256.0f));
                    info.Size.X = size.Width + xChange;
                    info.Size.Y = size.Height + yChange;
                }

                // Set origin of outdoor flats to centre-bottom
                if (info.BlockType == DFBlock.BlockTypes.Rmb)
                {
                    info.Origin.Y = groundHeight + info.Size.Y / 2;
                }

                // Set bounding sphere
                info.BoundingSphere.Center = info.Origin;
                if (info.Size.X > info.Size.Y)
                    info.BoundingSphere.Radius = info.Size.X / 2;
                else
                    info.BoundingSphere.Radius = info.Size.Y / 2;

                // Set model info
                block.Flats[i] = info;
            }

            // Ensure vertical limits are not still at test values.
            // This can happen when a block has zero models.
            if (minVertical == float.MaxValue) minVertical = 0;
            if (maxVertical == float.MinValue) maxVertical = 2048f;

            // Ensure block is always of minimum height.
            // This prevents tall trees from being improperly culled.
            if (maxVertical < 2048f) maxVertical = 2048f;

            // Update block vertical bounds
            block.BoundingBox.Min.Y = minVertical;
            block.BoundingBox.Max.Y = maxVertical;

            // Update dungeon camera vertical limits
            if (batchMode == BatchModes.Dungeon)
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
                case BatchModes.Exterior:
                    return (cameraMode == CameraModes.Free) ? exteriorFreeCamera : exteriorTopDownCamera;
                case BatchModes.Dungeon:
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
            exteriorTopDownCamera.Update(Camera.UpdateFlags.None, host.ElapsedTime);
            dungeonTopDownCamera.Update(Camera.UpdateFlags.None, host.ElapsedTime);
        }

        /// <summary>
        /// Sets free camera to starting position.
        /// </summary>
        private void InitFreeCameraPosition()
        {
            // Reset free cameras
            exteriorFreeCamera.ResetReference();
            dungeonFreeCamera.ResetReference();

            // Set position
            Vector3 exteriorPos = new Vector3(
                exteriorFreeCamera.MovementBounds.Max.X / 2,
                cameraFloorHeight,
                exteriorFreeCamera.MovementBounds.Max.Z);
            exteriorFreeCamera.NextPosition = exteriorPos;

            // Set dungeon free camera position
            Vector3 dungeonPos = new Vector3(
                dungeonFreeCamera.MovementBounds.Min.X + (dungeonFreeCamera.MovementBounds.Max.X - dungeonFreeCamera.MovementBounds.Min.X) / 2,
                1024f,
                dungeonFreeCamera.MovementBounds.Max.Z);
            dungeonFreeCamera.NextPosition = dungeonPos;

            // Set reference
            exteriorFreeCamera.Reference = new Vector3(0f, 0f, -1f);
            dungeonFreeCamera.Reference = new Vector3(0f, 0f, -1f);

            // Update
            exteriorFreeCamera.Update(Camera.UpdateFlags.None, host.ElapsedTime);
            dungeonFreeCamera.Update(Camera.UpdateFlags.None, host.ElapsedTime);
        }

        /// <summary>
        /// Conditionally updates active camera.
        /// </summary>
        private void UpdateCamera()
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
                    case BatchModes.Exterior:
                        exteriorFreeCamera.Update(flags, host.ElapsedTime);
                        break;
                    case BatchModes.Dungeon:
                        dungeonFreeCamera.Update(flags, host.ElapsedTime);
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
                    case BatchModes.Exterior:
                        exteriorTopDownCamera.Update(Camera.UpdateFlags.None, host.ElapsedTime);
                        break;
                    case BatchModes.Dungeon:
                        dungeonTopDownCamera.Update(Camera.UpdateFlags.None, host.ElapsedTime);
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
            dungeonFreeCamera.MovementBounds = bounds;
        }

        /// <summary>
        /// Update dungeon free camera bounds.
        ///  This is done after loading block resources.
        /// </summary>
        /// <param name="bounds">BoundingBox.</param>
        private void UpdateDungeonFreeCameraBounds(BoundingBox bounds)
        {
            BoundingBox newBounds = dungeonFreeCamera.MovementBounds;

            if (bounds.Min.Y < newBounds.Min.Y)
                newBounds.Min.Y = bounds.Min.Y - cameraDungeonFreedom;
            if (bounds.Max.Y > newBounds.Max.Y)
                newBounds.Max.Y = bounds.Max.Y + cameraDungeonFreedom;

            dungeonFreeCamera.MovementBounds = newBounds;
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

            // Set billboard camera
            if (batchMode == BatchModes.Exterior)
                billboardManager.Camera = exteriorFreeCamera;
            else if (batchMode == BatchModes.Dungeon)
                billboardManager.Camera = dungeonFreeCamera;
        }

        #endregion

    }

}
