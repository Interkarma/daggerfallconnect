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
        const float rmbBlockSide = 4096.0f;
        const float rdbBlockSide = 2048.0f;
        private Dictionary<int, BlockPosition> exteriorLayout = new Dictionary<int,BlockPosition>();
        private Dictionary<int, bool> exteriorResources = new Dictionary<int, bool>();
        private Dictionary<int, BlockPosition> dungeonLayout = new Dictionary<int, BlockPosition>();
        private Dictionary<int, bool> dungeonResources = new Dictionary<int, bool>();

        // Location
        string currentBlockName = string.Empty;
        int currentLatitude = -1;
        int currentLongitude = -1;

        // Status message
        string currentStatus = string.Empty;

        // Appearance
        private Color backgroundColor = Color.LightGray;

        // XNA
        private VertexDeclaration modelVertexDeclaration;
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

        // Batching options
        BatchModes batchMode = BatchModes.SingleExteriorBlock;
        BatchOptions batchOptions = BatchOptions.RmbGroundPlane | BatchOptions.RmbGroundFlats;

        // Ray testing
        Ray mouseRay;
        RenderableBoundingBox renderableBounds;

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
        [Flags] public enum BatchOptions
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
            renderableBounds = new RenderableBoundingBox(host.GraphicsDevice);
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
        }

        /// <summary>
        /// Called by host when view should redraw.
        /// </summary>
        public override void Draw()
        {
            // Clear display
            host.GraphicsDevice.Clear(backgroundColor);

            #region Get Layout
            // Get appropriate layout data
            Dictionary<int, BlockPosition> layout;
            Dictionary<int, bool> resources;
            switch (batchMode)
            {
                case BatchModes.SingleExteriorBlock:
                case BatchModes.FullExterior:
                    layout = exteriorLayout;
                    resources = exteriorResources;
                    break;
                case BatchModes.SingleDungeonBlock:
                case BatchModes.FullDungeon:
                    layout = dungeonLayout;
                    resources = dungeonResources;
                    break;
                default:
                    return;
            }

            // Nothing to do if layout is empty
            if (layout.Count == 0)
                return;
            #endregion

            #region Set Render States
            // Set render states
            host.GraphicsDevice.RenderState.DepthBufferEnable = true;
            host.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            host.GraphicsDevice.RenderState.AlphaTestEnable = false;
            host.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            // Set anisotropy based on camera mode
            if (cameraMode == CameraModes.Free)
            {
                host.GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Anisotropic;
                host.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Anisotropic;
                host.GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Linear;
                host.GraphicsDevice.SamplerStates[0].MaxAnisotropy = host.GraphicsDevice.GraphicsDeviceCapabilities.MaxAnisotropy;
            }
            else
            {
                host.GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Linear;
                host.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Linear;
                host.GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Linear;
                host.GraphicsDevice.SamplerStates[0].MaxAnisotropy = 0;
            }
            
            #endregion

            #region Setup
            // Set vertex declaration
            host.GraphicsDevice.VertexDeclaration = modelVertexDeclaration;

            // Get view and projection matrices based on camera mode
            Matrix view = ActiveCamera.View;
            Matrix projection = ActiveCamera.Projection;

            // Update matrices
            modelEffect.View = view;
            modelEffect.Projection = projection;
            viewFrustum.Matrix = view * projection;

            // Init variables used for tracking mouse ray and drawing bounding box
            float? distance = null;
            float minDistance = 999999999f;
            bool mouseInBlock = false;
            BlockManager.ModelInfo? closestModelInfo = null;
            Matrix closestModelMatrix = Matrix.Identity;
            #endregion

            #region Step Layout
            // Draw visible blocks
            Matrix world;
            foreach (var layoutItem in layout)
            {
                // Create transformed block bounding box
                world = Matrix.CreateTranslation(layout[layoutItem.Key].position);
                BoundingBox blockBox = new BoundingBox(
                    Vector3.Transform(layoutItem.Value.block.BoundingBox.Min, world),
                    Vector3.Transform(layoutItem.Value.block.BoundingBox.Max, world));

                // Test block bounding box against frustum
                if (!viewFrustum.Intersects(blockBox))
                    continue;

                // Late-load resources if not present
                if (!resources[layoutItem.Key])
                    resources[layoutItem.Key] = LoadBlockResources(layoutItem.Value.block);

                // Test ray against block bounds.
                // Is only performed when not scrolling.
                if (cameraVelocity == Vector3.Zero)
                {
                    distance = mouseRay.Intersects(blockBox);
                    if (distance != null)
                        mouseInBlock = true;
                }

                // Draw each model in this block
                foreach (var modelItem in layoutItem.Value.block.Models)
                {
                    // Create transformed model bounding box
                    modelEffect.World = modelItem.Matrix * world;
                    BoundingBox modelBox = new BoundingBox(
                            Vector3.Transform(modelItem.BoundingBox.Min, modelEffect.World),
                            Vector3.Transform(modelItem.BoundingBox.Max, modelEffect.World));

                    // Test model bounding box against frustum
                    //if (!viewFrustum.Intersects(modelBox))
                    //    continue;

                    // Draw the model
                    DrawSingleModel((int)modelItem.ModelId);

                    // Test ray against model if ray also in this block
                    if (mouseInBlock)
                    {
                        // TODO: Place intersected models in sorted array and test against face data
                        
                        distance = mouseRay.Intersects(modelBox);
                        if (distance != null)
                        {
                            if (distance < minDistance)
                            {
                                minDistance = distance.Value;
                                closestModelInfo = modelItem;
                                closestModelMatrix = modelEffect.World;
                            }
                        }
                    }
                }

                // Optionally draw gound plane for this item
                if (batchMode == BatchModes.SingleExteriorBlock ||
                    batchMode == BatchModes.FullExterior &&
                    BatchOptions.RmbGroundPlane == (batchOptions & BatchOptions.RmbGroundPlane))
                {
                    // Used to translate ground down a few units to reduce
                    // z-fighting with other ground-aligned planes
                    Matrix ground = Matrix.CreateTranslation(0, -7, 0);

                    // Draw ground
                    modelEffect.World = world * ground;
                    DrawGroundPlane(layoutItem.Key);
                }
            }
            #endregion

            #region Bounding Box
            // Draw bounding box if mouse over model
            if (closestModelInfo != null && host.MouseInClientArea)
            {
                renderableBounds.Draw(
                    closestModelInfo.Value.BoundingBox,
                    view,
                    projection,
                    closestModelMatrix);
            }
            #endregion
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
            UpdateMouseRay(e.X, e.Y);

            // Normal camera movement
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

                    // Cap velocity at very small amounts to prevent sliding
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
        }

        /// <summary>
        /// Called when filtered models array has been changed.
        /// </summary>
        public override void FilteredModelsChanged()
        {
        }

        /// <summary>
        /// Called when the view is resumed after being inactive.
        ///  Allows view to perform any layout or other requirements before redraw.
        /// </summary>
        public override void ResumeView()
        {
            //base.ResumeView();

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

        #region Drawing Methods

        private void DrawSingleModel(int key)
        {
            // Get model
            ModelManager.Model model = host.ModelManager.GetModel(key);

            // Exit if no model loaded
            if (model.Vertices == null)
                return;

            // Set wrap mode
            host.GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            host.GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            foreach (var submesh in model.SubMeshes)
            {
                modelEffect.Texture = host.TextureManager.GetTexture(submesh.TextureKey);

                modelEffect.Begin();
                modelEffect.CurrentTechnique.Passes[0].Begin();

                host.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                    model.Vertices, 0, model.Vertices.Length,
                    submesh.Indices, 0, submesh.Indices.Length / 3);

                modelEffect.CurrentTechnique.Passes[0].End();
                modelEffect.End();
            }
        }

        private void DrawGroundPlane(int key)
        {
            // Set terrain texture atlas
            modelEffect.Texture = host.TextureManager.TerrainAtlas;

            // Set clamp mode
            host.GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
            host.GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Clamp;

            modelEffect.Begin();
            modelEffect.CurrentTechnique.Passes[0].Begin();

            host.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, exteriorLayout[key].block.GroundPlaneVertices, 0, 512);

            modelEffect.CurrentTechnique.Passes[0].End();
            modelEffect.End();
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
        }

        #endregion

        #region Layout Methods

        /// <summary>
        /// Build exterior layout from a single block.
        /// </summary>
        /// <param name="blockName">Block name.</param>
        private void BuildExteriorLayout(ref string blockName)
        {
            // Create exterior layout for one block
            exteriorLayout = new Dictionary<int, BlockPosition>(1);
            exteriorResources = new Dictionary<int, bool>(1);

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

            // Add to layout dictionary
            exteriorLayout.Add(key, blockPosition);
            exteriorResources.Add(key, false);

            // Bounds are equivalent to block
            BoundingBox bounds = blockPosition.block.BoundingBox;
            bounds.Min.Y = cameraFloorHeight;
            bounds.Max.Y = cameraCeilingHeight;
            exteriorTopDownCamera.Bounds = bounds;
            exteriorFreeCamera.Bounds = bounds;

            // Clear scroll velocity
            cameraVelocity = Vector3.Zero;

            // Init camera pos
            InitCameraPosition();
        }

        /// <summary>
        /// Build dungeon layout from a single block.
        /// </summary>
        /// <param name="blockName">Block name.</param>
        private void BuildDungeonLayout(ref string blockName)
        {
            // Create dungeon layout
            dungeonLayout = new Dictionary<int, BlockPosition>();
            dungeonResources = new Dictionary<int, bool>(1);

            // Get block key
            int key = GetBlockKey(0, 0);

            // Create block position data
            BlockPosition blockPosition = new BlockPosition();
            blockPosition.name = blockName;
            blockPosition.block = host.BlockManager.LoadBlock(blockName);

            // Set block position
            blockPosition.position = new Vector3(0, 0, 0);

            // Add to layout dictionary
            dungeonLayout.Add(key, blockPosition);
            dungeonResources.Add(key, false);

            // Set top down bounds to have a higher ceiling
            BoundingBox topDownBounds = blockPosition.block.BoundingBox;
            topDownBounds.Max.Y = cameraCeilingHeight;
            dungeonTopDownCamera.Bounds = topDownBounds;

            // Set free camera to have more camera movement around the outside
            BoundingBox freeBounds = blockPosition.block.BoundingBox;
            freeBounds.Min.X -= cameraDungeonFreedom;
            freeBounds.Max.X += cameraDungeonFreedom;
            freeBounds.Min.Y -= cameraDungeonFreedom;
            freeBounds.Max.Y = cameraDungeonFreedom;
            freeBounds.Min.Z -= cameraDungeonFreedom;
            freeBounds.Max.Z += cameraDungeonFreedom;
            dungeonFreeCamera.Bounds = freeBounds;

            // Clear scroll velocity
            cameraVelocity = Vector3.Zero;

            // Init camera pos
            InitCameraPosition();
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

            // Create exterior layout
            exteriorLayout = new Dictionary<int, BlockPosition>(width * height);
            exteriorResources = new Dictionary<int, bool>(width * height);
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

                    // Add to layout dictionary
                    exteriorLayout.Add(key, blockPosition);
                    exteriorResources.Add(key, false);

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

            // Clear scroll velocity
            cameraVelocity = Vector3.Zero;

            // Init camera pos
            InitCameraPosition();
        }

        /// <summary>
        /// Build exterior layout from a full location.
        /// </summary>
        /// <param name="dfLocation">DFLocation.</param>
        private void BuildDungeonLayout(ref DFLocation dfLocation)
        {
            // Bounding box for layout
            BoundingBox bounds = new BoundingBox();

            // Create dungeon layout
            dungeonLayout = new Dictionary<int, BlockPosition>();
            dungeonResources = new Dictionary<int, bool>();
            foreach (var block in dfLocation.Dungeon.Blocks)
            {
                // Get block key
                int key = GetBlockKey(block.X, block.Z);

                // Some dungeons (e.g. Orsinium) encode more than one block with identical coordinates.
                // It is not yet known if Daggerfall uses the first or subsequent blocks.
                // We are using the first instance here until research shows otherwise.
                if (dungeonLayout.ContainsKey(key))
                    continue;

                // Create block position data
                BlockPosition blockPosition = new BlockPosition();
                blockPosition.name = block.BlockName;
                blockPosition.block = host.BlockManager.LoadBlock(block.BlockName);

                // Set block position
                blockPosition.position = new Vector3(block.X * rdbBlockSide, 0, -(block.Z * rdbBlockSide));

                // Add to layout dictionary
                dungeonLayout.Add(key, blockPosition);
                dungeonResources.Add(key, false);

                // Merge bounding boxes
                Vector3 min = blockPosition.block.BoundingBox.Min + blockPosition.position;
                Vector3 max = blockPosition.block.BoundingBox.Max + blockPosition.position;
                bounds = BoundingBox.CreateMerged(bounds, new BoundingBox(min, max));
            }

            // Set top down bounds to have a higher ceiling
            BoundingBox topDownBounds = bounds;
            topDownBounds.Max.Y = cameraCeilingHeight;
            dungeonTopDownCamera.Bounds = topDownBounds;
            
            // Set free camera to have more camera movement around the outside
            BoundingBox freeBounds = bounds;
            freeBounds.Min.X -= cameraDungeonFreedom;
            freeBounds.Max.X += cameraDungeonFreedom;
            freeBounds.Min.Y -= cameraDungeonFreedom;
            freeBounds.Max.Y = cameraDungeonFreedom;
            freeBounds.Min.Z -= cameraDungeonFreedom;
            freeBounds.Max.Z += cameraDungeonFreedom;
            dungeonFreeCamera.Bounds = freeBounds;

            // Clear scroll velocity
            cameraVelocity = Vector3.Zero;
            InitCameraPosition();
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

        #endregion

        #region Resource Loading

        /// <summary>
        /// Loads model and texture resources for specified block.
        ///  This method also updates block bounding box based on the models it contains.
        ///  Resources cached by TextureManager and ModelManager are recovered quickly.
        /// </summary>
        /// <param name="block">BlockManager.Block.</param>
        private bool LoadBlockResources(BlockManager.Block block)
        {
            // Load model textures
            float minVertical = 0f, maxVertical = 0f;
            for (int i = 0; i < block.Models.Count; i++)
            {
                // Get model info
                BlockManager.ModelInfo info = block.Models[i];

                // Load model resource
                ModelManager.Model model;
                host.ModelManager.LoadModel((int)info.ModelId, out model);

                // Load texture resources for this model
                for (int sm = 0; sm < model.SubMeshes.Length; sm++)
                {
                    model.SubMeshes[sm].TextureKey = host.TextureManager.LoadTexture(
                        model.SubMeshes[sm].TextureArchive,
                        model.SubMeshes[sm].TextureRecord);
                }

                // Set model bounding box
                info.BoundingBox = model.BoundingBox;

                // Track vertical extents using model bounds
                if (model.BoundingBox.Min.Y < minVertical)
                    minVertical = model.BoundingBox.Min.Y;
                if (model.BoundingBox.Max.Y > maxVertical)
                    maxVertical = model.BoundingBox.Max.Y;

                // Set model info
                block.Models[i] = info;
            }

            // Update block bounding box
            Vector3 min = new Vector3(block.BoundingBox.Min.X, minVertical, block.BoundingBox.Min.Z);
            Vector3 max = new Vector3(block.BoundingBox.Max.X, maxVertical, block.BoundingBox.Max.Z);
            block.BoundingBox = new BoundingBox(min, max);

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

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets appropriate layout dictionary to use based on batch mode.
        /// </summary>
        /// <returns>Layout dictionary.</returns>
        private Dictionary<int, BlockPosition> GetLayoutDict()
        {
            switch (batchMode)
            {
                case BatchModes.SingleExteriorBlock:
                case BatchModes.FullExterior:
                    return exteriorLayout;
                case BatchModes.SingleDungeonBlock:
                case BatchModes.FullDungeon:
                    return dungeonLayout;
                default:
                    return null;
            }
        }

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

        /// <summary>
        /// Conditionally updates cameras.
        /// </summary>
        private void UpdateCameras()
        {
            // Update based on camera mode
            if (CameraMode == CameraModes.Free)
            {
                // Host must be focused
                if (!host.Focused)
                    return;

                // Update based on batch mode
                switch (batchMode)
                {
                    case BatchModes.SingleExteriorBlock:
                    case BatchModes.FullExterior:
                        exteriorFreeCamera.Update(Camera.UpdateFlags.Keyboard);
                        break;
                    case BatchModes.SingleDungeonBlock:
                    case BatchModes.FullDungeon:
                        dungeonFreeCamera.Update(Camera.UpdateFlags.Keyboard);
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

        #endregion

        #region Mouse Picking Methods

        /// <summary>
        /// Update mouse ray for picking.
        /// </summary>
        private void UpdateMouseRay(int x, int y)
        {
            // Get appropriate view matrix
            Matrix view = ActiveCamera.View;
            Matrix projection = ActiveCamera.Projection;

            // Unproject vectors into view area
            Viewport vp = host.GraphicsDevice.Viewport;
            Matrix world = Matrix.CreateTranslation(0, 0, 0);
            Vector3 near = vp.Unproject(new Vector3(x, y, 0), projection, view, world);
            Vector3 far = vp.Unproject(new Vector3(x, y, 1), projection, view, world);

            // Create ray
            Vector3 direction = far - near;
            direction.Normalize();
            mouseRay.Position = near;
            mouseRay.Direction = direction;
        }

        #endregion

    }

}
