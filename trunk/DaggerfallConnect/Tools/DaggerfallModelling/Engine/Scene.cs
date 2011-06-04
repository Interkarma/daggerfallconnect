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
using DaggerfallModelling.Engine;
using DaggerfallModelling.ViewControls;
#endregion

namespace DaggerfallModelling.Engine
{

    /// <summary>
    /// Scene construction and storage for LocationView.
    ///  Models are batched and sorted by texture for the
    ///  renderer. Designed around Daggerfall's map layouts
    ///  and is not intended to be a general-purpose scene graph.
    /// </summary>
    public class Scene : ComponentBase
    {

        #region Class Variables

        // Views
        LocationView view;

        // Daggerfall location
        private DFLocation? currentLocation = null;
        private string currentBlockName = string.Empty;
        private int currentLatitude = -1;
        private int currentLongitude = -1;

        // Scene layout
        private const float rmbBlockSide = 4096.0f;
        private const float rdbBlockSide = 2048.0f;
        private BlockPosition[] exteriorLayout = new BlockPosition[64];
        private BlockPosition[] dungeonLayout = new BlockPosition[32];
        private int exteriorLayoutCount = 0;
        private int dungeonLayoutCount = 0;

        // Scene options
        private SceneOptionFlags sceneOptions =
            SceneOptionFlags.SkyPlane | SceneOptionFlags.GroundPlane |
            SceneOptionFlags.DecorativeFlats | SceneOptionFlags.MousePicking;

        // Scene block lookup
        private Dictionary<int, int> exteriorLayoutDict = new Dictionary<int, int>();
        private Dictionary<int, int> dungeonLayoutDict = new Dictionary<int, int>();

        // Batching
        private const int batchArrayLength = 1024;
        private BatchArray batches = new BatchArray();
        private BatchModes batchMode = BatchModes.Exterior;

        // Ground height
        private int groundHeight = -1;

        // Cameras
        private Camera exteriorTopDownCamera = new Camera();
        private Camera exteriorFreeCamera = new Camera();
        private Camera dungeonTopDownCamera = new Camera();
        private Camera dungeonFreeCamera = new Camera();
        private static float cameraFloorHeight = 0.0f;
        private static float cameraCeilingHeight = 10000.0f;
        private static float cameraStartHeight = 6000.0f;
        private static float cameraDungeonFreedom = 1000.0f;

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
        /// Contains batches of visible items for renderer.
        /// </summary>
        public struct BatchArray
        {
            public Dictionary<int, BatchModelArray> Models;
        }

        /// <summary>
        /// Stores a fixed array of submeshes which is allocated
        ///  once to minimise garbage collections later.
        /// </summary>
        public struct BatchModelArray
        {
            public int Length;
            public BatchModelItem[] BatchItems;
        }

        /// <summary>
        /// Represents a visible submesh.
        /// </summary>
        public struct BatchModelItem
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
        public struct BlockPosition
        {
            public string name;
            public Vector3 position;
            public BlockManager.BlockData block;
        }

        /// <summary>
        /// Flags for scene options.
        /// </summary>
        [Flags]
        public enum SceneOptionFlags
        {
            /// <summary>No flags set.</summary>
            None = 0,

            /// <summary>Render sky behind exterior free-camera scenes.</summary>
            SkyPlane = 1,

            /// <summary>Render ground plane below exterior scenes.</summary>
            GroundPlane = 2,

            /// <summary>
            /// Render decorative flats (e.g. trees, rocks, animals)
            ///  in free-camera scenes.
            /// </summary>
            DecorativeFlats = 4,

            /// <summary>
            /// Render editor flats (e.g. monster spawn points, quest markers)
            ///  in free-camera scenes.
            /// </summary>
            EditorFlats = 8,

            /// <summary>Mouse-model picking.</summary>
            MousePicking = 16,

            /// <summary>Controller-model picking.</summary>
            ControllerPicking = 32,
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets scene options flags.
        /// </summary>
        public SceneOptionFlags SceneOptions
        {
            get { return sceneOptions; }
            set { sceneOptions = value; }
        }

        /// <summary>
        /// Gets or sets batch mode for drawing operations.
        /// </summary>
        public BatchModes BatchMode
        {
            get { return batchMode; }
            set { batchMode = value; }
        }

        /// <summary>
        /// Gets batch array of visible items.
        /// </summary>
        public BatchArray Batches
        {
            get { return batches; }
        }

        public BlockPosition[] Layout
        {
            get
            {
                int count;
                BlockPosition[] layout;
                GetLayoutArray(out layout, out count);
                return layout;
            }
        }

        /// <summary>
        /// Gets layout count.
        /// </summary>
        public int LayoutCount
        {
            get { return GetLayoutCount(); }
        }

        /// <summary>
        /// Gets number of model batches in scene.
        ///  Each batch is a collection of submeshes
        ///  grouped by material properties.
        /// </summary>
        public int ModelBatchCount
        {
            get { return (batches.Models == null) ? 0 : batches.Models.Count; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public Scene(ViewHost host, LocationView view)
            : base (host)
        {
            this.view = view;
        }

        #endregion

        #region Abstract Overrides

        /// <summary>
        /// Called when component must initialise.
        /// </summary>
        public override void Initialize()
        {
            // Setup active camera
            camera = GetActiveCamera();

            // Setup initial camera positions
            InitCameraPosition();

            // Setup camera projection matrices
            UpdateProjectionMatrix();
        }

        /// <summary>
        /// Called when component should update.
        /// </summary>
        public override void Update()
        {
            // Set camera
            camera = GetActiveCamera();

            // Batch visible scene elements for renderer
            if (batches.Models != null)
            {
                ClearBatches();
                BuildBatches();
            }
        }

        /// <summary>
        /// Called when component should redraw.
        /// </summary>
        public override void Draw()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates projection matrix to current view size.
        /// </summary>
        public void UpdateProjectionMatrix()
        {
            // Update aspect ratio for all cameras
            float aspectRatio = (float)host.ClientRectangle.Width / (float)host.ClientRectangle.Height;
            exteriorTopDownCamera.SetAspectRatio(aspectRatio);
            exteriorFreeCamera.SetAspectRatio(aspectRatio);
            dungeonTopDownCamera.SetAspectRatio(aspectRatio);
            dungeonFreeCamera.SetAspectRatio(aspectRatio);
        }

        /// <summary>
        /// Determines if specified scene option flags are set.
        /// </summary>
        /// <param name="flags">SceneOptionFlags.</param>
        /// <returns>True if flags set.</returns>
        public bool HasSceneOptionFlags(SceneOptionFlags flags)
        {
            return flags == (sceneOptions & flags);
        }

        /// <summary>
        /// Gets appropriate layout array to use based on batch mode.
        /// </summary>
        /// <param name="layout">Layout array output.</param>
        /// <param name="count">Layout count output.</param>
        public void GetLayoutArray(out Scene.BlockPosition[] layout, out int count)
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
        /// Conditionally updates active camera.
        /// </summary>
        public void UpdateCamera()
        {
            // Update based on camera mode
            if (view.CameraMode == ViewBase.CameraModes.Free)
            {
                // Set input flags
                Camera.InputFlags flags = Camera.InputFlags.Controller;
                if (host.Focused)
                {
                    flags |= Camera.InputFlags.Keyboard;
                }
                if (host.MouseInClientArea)
                {
                    flags |= Camera.InputFlags.Mouse;
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
            else if (view.CameraMode == ViewBase.CameraModes.TopDown)
            {
                // Apply camera velocity
                //ActiveCamera.Translate(cameraVelocity.X, 0f, cameraVelocity.Z);

                // Update based on batch mode
                switch (batchMode)
                {
                    case BatchModes.Exterior:
                        exteriorTopDownCamera.Update(Camera.InputFlags.None, host.ElapsedTime);
                        break;
                    case BatchModes.Dungeon:
                        dungeonTopDownCamera.Update(Camera.InputFlags.None, host.ElapsedTime);
                        break;
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
            view.Climate = climate;

            // Reset batches
            batches.Models = new Dictionary<int, BatchModelArray>();

            // Build single-block exterior layout
            BuildExteriorLayout(ref blockName);

            // Set status message
            //currentStatus = string.Format("Viewing RMB block {0}.", blockName);

            // Store block name
            currentBlockName = blockName;

            // Clear location
            currentLocation = null;

            // Set default sky
            view.SkyManager.SkyIndex = Sky.DefaultSkyIndex;

            // Init camera
            //cameraVelocity = Vector3.Zero;
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
            view.Climate = DFLocation.ClimateType.None;

            // Reset batches
            batches.Models = new Dictionary<int, BatchModelArray>();

            // Build single-block layout
            BuildDungeonLayout(ref blockName);

            // Set status message
            //currentStatus = string.Format("Viewing RDB block {0}.", blockName);

            // Store block name
            currentBlockName = blockName;

            // Clear location
            currentLocation = null;

            // Set default sky
            view.SkyManager.SkyIndex = Sky.DefaultSkyIndex;

            // Init camera
            //cameraVelocity = Vector3.Zero;
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
            view.Climate = dfLocation.Climate;

            // Reset batches
            batches.Models = new Dictionary<int, BatchModelArray>();

            // Build layout
            BuildExteriorLayout(ref dfLocation);

            // Optionally build dungeon layout
            if (dfLocation.HasDungeon)
                BuildDungeonLayout(ref dfLocation);

            // Set status message
            //currentStatus = string.Format("Exploring {0}.", dfLocation.Name);

            // Store location coordinates
            currentLatitude = (int)dfLocation.MapTableData.Latitude;
            currentLongitude = (int)dfLocation.MapTableData.Longitude;

            // Store location
            currentLocation = dfLocation;

            // Set climate sky
            view.SkyManager.SkyIndex = dfLocation.SkyArchive;

            // Init camera
            //cameraVelocity = Vector3.Zero;
            InitCameraPosition();
        }

        /// <summary>
        /// Gets specific block from scene layout data.
        /// </summary>
        /// <param name="X">X position of block in layout grid.</param>
        /// <param name="Y">Y position of block in layout grid.</param>
        /// <param name="blockPosition">BlockPosition scene data.</param>
        /// <returns>True if successful.</returns>
        public bool GetSceneBlock(int X, int Y, out BlockPosition blockPosition)
        {
            // Get layout
            int count;
            BlockPosition[] layout;
            GetLayoutArray(out layout, out count);

            // Get layout dictionary
            Dictionary<int, int> layoutDict;
            GetLayoutDict(out layoutDict);

            // Get block key
            int key = GetBlockKey(X, Y);
            if (!layoutDict.ContainsKey(key))
            {
                blockPosition = new BlockPosition();
                return false;
            }

            // Get scene block
            blockPosition = layout[layoutDict[key]];
            return false;
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

        /// <summary>
        /// Gets number of elements in active layout.
        /// </summary>
        private int GetLayoutCount()
        {
            switch (batchMode)
            {
                case BatchModes.Exterior:
                    return exteriorLayoutCount;
                case BatchModes.Dungeon:
                    return dungeonLayoutCount;
                default:
                    return 0;
            }
        }

        #endregion

        #region Batching

        /// <summary>
        /// Clears batches. These are rebuilt every frame from
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
            view.BillboardManager.ClearBatch();
        }

        /// <summary>
        /// Step through scene batching visible triangles.
        /// </summary>
        private void BuildBatches()
        {
            // Get batch layout data            
            int count;
            BlockPosition[] layout;
            GetLayoutArray(out layout, out count);
            if (layout == null)
                return;

            // Step through block layout
            Matrix blockTransform;
            BoundingBox blockBounds;
            for (int i = 0; i < count; i++)
            {
                // Create transformed block bounding box
                blockTransform = Matrix.CreateTranslation(layout[i].position);
                blockBounds.Min = Vector3.Transform(layout[i].block.BoundingBox.Min, blockTransform);
                blockBounds.Max = Vector3.Transform(layout[i].block.BoundingBox.Max, blockTransform);

                // Do nothing further if block is not visible
                if (!camera.BoundingFrustum.Intersects(blockBounds))
                    continue;

                // Update block if required
                if (layout[i].block.UpdateRequired)
                    UpdateBlock(ref layout[i].block);

                // Batch block
                BatchBlock(ref layout[i].block, ref blockTransform);
            }
        }

        /// <summary>
        /// Step through block to find visible models.
        /// </summary>
        /// <param name="block">BlockManager.Block.</param>
        /// <param name="blockTransform">Block transform.</param>
        private void BatchBlock(ref BlockManager.BlockData block, ref Matrix blockTransform)
        {
            // Iterate each model in this block
            int modelIndex = 0;
            Matrix modelTransform;
            BoundingSphere modelBounds;
            foreach (var modelInfo in block.Models)
            {
                // Create transformed model bounding sphere
                modelTransform = modelInfo.Matrix * blockTransform;
                modelBounds.Center = Vector3.Transform(modelInfo.BoundingSphere.Center, modelTransform);
                modelBounds.Radius = modelInfo.BoundingSphere.Radius;

                // Do nothing further if model not visible
                if (!camera.BoundingFrustum.Intersects(modelBounds))
                    continue;

                // Add the model
                ModelManager.ModelData model = host.ModelManager.GetModelData(modelInfo.ModelId);
                BatchModel(ref model, ref modelTransform);

                // Increment index
                modelIndex++;
            }

            // Batch misc flats
            if (view.CameraMode == ViewBase.CameraModes.Free &&
                HasSceneOptionFlags(SceneOptionFlags.DecorativeFlats))
            {
                BatchFlats(ref block, blockTransform);
            }

            // Optional exterior batching
            if (batchMode == BatchModes.Exterior)
            {
                // Batch ground plane
                if (HasSceneOptionFlags(SceneOptionFlags.GroundPlane))
                {
                    // Translate ground down to reduce z-fighting
                    // with other ground-aligned planes
                    Matrix groundTransform = blockTransform * Matrix.CreateTranslation(0, groundHeight, 0);

                    // Batch ground plane
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

                // Test bounds against camera frustum
                if (!camera.BoundingFrustum.Intersects(flatBounds))
                    continue;

                // Add to batch
                view.BillboardManager.AddToBatch(
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
                BlockManager.ModelItem info = block.Models[i];

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
                BlockManager.FlatItem info = block.Flats[i];

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

                // Set origin of outdoor flats to centre-bottom.
                // Sink them just a little so they don't look too floaty.
                if (info.BlockType == DFBlock.BlockTypes.Rmb)
                {
                    info.Origin.Y = (groundHeight + info.Size.Y / 2) - 4;
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
                    return (view.CameraMode == ViewBase.CameraModes.Free) ? exteriorFreeCamera : exteriorTopDownCamera;
                case BatchModes.Dungeon:
                default:
                    return (view.CameraMode == ViewBase.CameraModes.Free) ? dungeonFreeCamera : dungeonTopDownCamera;
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
            exteriorTopDownCamera.Update(Camera.InputFlags.None, host.ElapsedTime);
            dungeonTopDownCamera.Update(Camera.InputFlags.None, host.ElapsedTime);
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
            exteriorFreeCamera.Update(Camera.InputFlags.None, host.ElapsedTime);
            dungeonFreeCamera.Update(Camera.InputFlags.None, host.ElapsedTime);
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

    }

}
