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
        private Dictionary<int, BlockPosition> dungeonLayout = new Dictionary<int, BlockPosition>();

        // Appearance
        private Color backgroundColor = Color.LightGray;

        // XNA
        private VertexDeclaration modelVertexDeclaration;
        private BasicEffect modelEffect;
        private float nearPlaneDistance = 1.0f;
        private float farPlaneDistance = 40000.0f;
        private Matrix projectionMatrix;
        private Matrix viewMatrix;
        private Matrix worldMatrix;
        private BoundingFrustum viewFrustum;

        // Camera
        private BoundingBox exteriorBounds;
        private BoundingBox dungeonBounds;
        private Vector3 cameraPosition = new Vector3(1024, 8192, -1024);
        private Vector3 cameraReference = new Vector3(0, -1.0f, -0.01f);
        private Vector3 cameraUpVector = Vector3.Up;

        // Movement
        private Vector3 cameraVelocity;
        private float cameraStepMuliplier = 5.0f;

        // Batching options
        BatchModes batchMode = BatchModes.SingleExteriorBlock;
        BatchOptions batchOptions = BatchOptions.RmbGroundPlane | BatchOptions.RmbGroundFlats;

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
            set { batchMode = value; }
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
            CameraMode = CameraModes.Normal;
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

            // Setup camera
            float aspectRatio = (float)host.GraphicsDevice.Viewport.Width / (float)host.GraphicsDevice.Viewport.Height;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, nearPlaneDistance, farPlaneDistance);
            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraPosition + cameraReference, cameraUpVector);
            worldMatrix = Matrix.Identity;

            // Create initial view frustum
            viewFrustum = new BoundingFrustum(viewMatrix * projectionMatrix);
        }

        /// <summary>
        /// Called by host when view should update animation.
        /// </summary>
        public override void Tick()
        {
            // Apply camera velocity in normal mode
            if (CameraMode == CameraModes.Normal)
            {
                TranslateCameraXZ(cameraVelocity.X, cameraVelocity.Z);
            }
        }

        /// <summary>
        /// Called by host when view should redraw.
        /// </summary>
        public override void Draw()
        {
            // Clear display
            host.GraphicsDevice.Clear(backgroundColor);

            // Get appropriate layout data
            Dictionary<int, BlockPosition> layout;
            switch (batchMode)
            {
                case BatchModes.SingleExteriorBlock:
                case BatchModes.FullExterior:
                    layout = exteriorLayout;
                    break;
                case BatchModes.SingleDungeonBlock:
                case BatchModes.FullDungeon:
                    layout = dungeonLayout;
                    break;
                default:
                    return;
            }

            // Nothing to do if layout is empty
            if (layout.Count == 0)
                return;

            // Set render states
            host.GraphicsDevice.RenderState.DepthBufferEnable = true;
            host.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            host.GraphicsDevice.RenderState.AlphaTestEnable = false;
            host.GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            host.GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
            host.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            host.GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Anisotropic;
            host.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Anisotropic;
            host.GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Linear;
            host.GraphicsDevice.SamplerStates[0].MaxAnisotropy = host.GraphicsDevice.GraphicsDeviceCapabilities.MaxAnisotropy;

            // Set vertex declaration
            host.GraphicsDevice.VertexDeclaration = modelVertexDeclaration;

            // Set view and projection matrices
            modelEffect.View = viewMatrix;
            modelEffect.Projection = projectionMatrix;

            // Update frustum matrix
            viewFrustum.Matrix = viewMatrix * projectionMatrix;

            // Used to translate ground down a few units to reduce
            // z-fighting with other ground-aligned planes
            Matrix ground = Matrix.CreateTranslation(0, -10, 0);

            // Draw visible blocks
            Matrix world;
            foreach (var layoutItem in layout)
            {
                // Create translation matrix for this block
                world = Matrix.CreateTranslation(layout[layoutItem.Key].position);

                // Create transformed block bounding box
                BoundingBox blockBox = new BoundingBox(
                    Vector3.Transform(layoutItem.Value.block.BoundingBox.Min, world),
                    Vector3.Transform(layoutItem.Value.block.BoundingBox.Max, world));

                // Test block bounding box against frustum
                if (!viewFrustum.Intersects(blockBox))
                    continue;

                // Draw each model in this block
                foreach (var modelItem in layoutItem.Value.block.Models)
                {
                    modelEffect.World = modelItem.Matrix * world;
                    DrawSingleModel((int)modelItem.ModelId);
                }

                // Optionally draw gound plane for this item
                if (batchMode == BatchModes.SingleExteriorBlock ||
                    batchMode == BatchModes.FullExterior &&
                    BatchOptions.RmbGroundPlane == (batchOptions & BatchOptions.RmbGroundPlane))
                {
                    modelEffect.World = world * ground;
                    DrawGroundPlane(layoutItem.Key);
                }
            }
        }

        /// <summary>
        /// Called by host when view should redraw.
        /// </summary>
        public override void Resize()
        {
            // Host must be ready as projection matrix depends on host control dimensions
            if (!host.IsReady)
                return;

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
            // Normal camera movement
            if (CameraMode == CameraModes.Normal)
            {
                // Scene dragging
                if (host.RightMouseDown)
                {
                    TranslateCameraXZ(
                        (float)-host.MousePosDelta.X * cameraStepMuliplier,
                        (float)-host.MousePosDelta.Y * cameraStepMuliplier);
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
            if (CameraMode == CameraModes.Normal)
            {
                // Scene dragging
                if (e.Button == MouseButtons.Right)
                {
                    // Set scroll velocity on right mouse up
                    cameraVelocity = new Vector3(
                        -host.MouseVelocity.X * cameraStepMuliplier,
                        0.0f,
                        -host.MouseVelocity.Y * cameraStepMuliplier);

                    // Cap velocity at very small amounts to prevent sliding
                    if (cameraVelocity.X > -cameraStepMuliplier && cameraVelocity.X < cameraStepMuliplier) cameraVelocity.X = 0.0f;
                    if (cameraVelocity.Z > -cameraStepMuliplier && cameraVelocity.Z < cameraStepMuliplier) cameraVelocity.Z = 0.0f;
                }
            }
        }

        /// <summary>
        /// Called when mouse enters client area.
        /// </summary>
        /// <param name="e">EventArgs</param>
        public override void OnMouseEnter(EventArgs e)
        {
        }

        /// <summary>
        /// Called when mouse leaves client area.
        /// </summary>
        /// <param name="e">EventArgs</param>
        public override void OnMouseLeave(EventArgs e)
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
            // Clear scroll velocity
            cameraVelocity = Vector3.Zero;

            // Resume view
            UpdateProjectionMatrix();
            UpdateStatusMessage();
            host.Refresh();
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

            modelEffect.Begin();
            modelEffect.CurrentTechnique.Passes[0].Begin();

            host.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, exteriorLayout[key].block.GroundPlaneVertices, 0, 512);

            modelEffect.CurrentTechnique.Passes[0].End();
            modelEffect.End();
        }

        #endregion

        #region Content Loading

        /// <summary>
        /// Loads a single block as an exterior location.
        ///  This will replace existing exterior layout.
        /// </summary>
        /// <param name="blockName">Block name.</param>
        public void LoadExterior(string blockName)
        {
            // There is no climate context for standalone blocks
            host.TextureManager.Climate = DFLocation.ClimateType.None;
            
            // Build layout
            BuildExteriorLayout(ref blockName);
        }

        /// <summary>
        /// Loads a single block as a dungeon location.
        ///  This will replace existing dungeon layout.
        /// </summary>
        /// <param name="blockName">Block name.</param>
        public void LoadDungeon(string blockName)
        {
            // There is no climate context for standalone blocks
            host.TextureManager.Climate = DFLocation.ClimateType.None;

            // Build layout
            BuildDungeonLayout(ref blockName);
        }

        /// <summary>
        /// Loads a full exterior location.
        ///  This will replace existing exterior layout.
        /// </summary>
        /// <param name="dfLocation">DFLocation.</param>
        public void LoadExterior(ref DFLocation dfLocation)
        {
            // Set climate for texture swaps
            host.TextureManager.Climate = dfLocation.Climate;

            // Build layout
            BuildExteriorLayout(ref dfLocation);
        }

        /// <summary>
        /// Loads a full dungeon layout.
        ///  This will replace existing dungeon layout.
        /// </summary>
        /// <param name="dfLocation">DFLocation.</param>
        public void LoadDungeon(ref DFLocation dfLocation)
        {
            // No climate support in dungeons at this time
            host.TextureManager.Climate = DFLocation.ClimateType.None;

            // Dungeon layout must be present in location
            if (dfLocation.HasDungeon)
                BuildDungeonLayout(ref dfLocation);
            else
                throw new Exception("Specified DFLocation does not contain a dungeon layout.");
        }

        #endregion

        #region Layout Methods

        /// <summary>
        /// Build exterior layout from a single block.
        /// </summary>
        /// <param name="blockName">Block name.</param>
        private void BuildExteriorLayout(ref string blockName)
        {
            // Creat exterior layout for one block
            exteriorLayout = new Dictionary<int, BlockPosition>(1);

            // Get block key and name
            int key = GetBlockKey(0, 0);
            string name = host.BlockManager.BlocksFile.CheckName(blockName);

            // Create block position data
            BlockPosition blockPosition = new BlockPosition();
            blockPosition.name = name;
            blockPosition.block = host.BlockManager.LoadBlock(name);

            // Set block position
            blockPosition.position = new Vector3(0, 0, 0);

            // Build ground plane
            host.BlockManager.BuildRmbGroundPlane(host.TextureManager, ref blockPosition.block);

            // Load block models and textures
            LoadBlockResources(ref blockPosition.block);

            // Add to layout dictionary
            exteriorLayout.Add(key, blockPosition);

            // Bounds are equivalent to block
            exteriorBounds = blockPosition.block.BoundingBox;

            // Clear scroll velocity
            cameraVelocity = Vector3.Zero;

            // Centre camera in layout
            cameraPosition.X = (exteriorBounds.Max.X - exteriorBounds.Min.X) / 2;
            cameraPosition.Z = -(exteriorBounds.Max.Z - exteriorBounds.Min.Z) / 2;
        }

        /// <summary>
        /// Build dungeon layout from a single block.
        /// </summary>
        /// <param name="blockName">Block name.</param>
        private void BuildDungeonLayout(ref string blockName)
        {
            // Create dungeon layout
            dungeonLayout = new Dictionary<int, BlockPosition>();

            // Get block key
            int key = GetBlockKey(0, 0);

            // Create block position data
            BlockPosition blockPosition = new BlockPosition();
            blockPosition.name = blockName;
            blockPosition.block = host.BlockManager.LoadBlock(blockName);

            // Set block position
            blockPosition.position = new Vector3(0, 0, 0);

            // Load block models and textures
            LoadBlockResources(ref blockPosition.block);

            // Add to layout dictionary
            dungeonLayout.Add(key, blockPosition);

            // Bounds are equivalent to block
            dungeonBounds = blockPosition.block.BoundingBox;

            // Clear scroll velocity
            cameraVelocity = Vector3.Zero;

            // Centre camera in layout
            cameraPosition.X = (dungeonBounds.Max.X - dungeonBounds.Min.X) / 2;
            cameraPosition.Z = -(dungeonBounds.Max.Z - dungeonBounds.Min.Z) / 2;
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
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Get block key and name
                    int key = GetBlockKey(x, y);
                    string name = host.BlockManager.BlocksFile.CheckName(host.MapsFile.GetRmbBlockName(ref dfLocation, x, y));

                    // Create block position data
                    BlockPosition blockPosition = new BlockPosition();
                    blockPosition.name = name;
                    blockPosition.block = host.BlockManager.LoadBlock(name);
                    
                    // Set block position
                    blockPosition.position = new Vector3(x * rmbBlockSide, 0, -(y * rmbBlockSide));

                    // Build ground plane
                    host.BlockManager.BuildRmbGroundPlane(host.TextureManager, ref blockPosition.block);

                    // Load block models and textures
                    LoadBlockResources(ref blockPosition.block);

                    // Add to layout dictionary
                    exteriorLayout.Add(key, blockPosition);

                    // Merge bounding boxes
                    Vector3 min = blockPosition.block.BoundingBox.Min + blockPosition.position;
                    Vector3 max = blockPosition.block.BoundingBox.Max + blockPosition.position;
                    bounds = BoundingBox.CreateMerged(bounds, new BoundingBox(min, max));
                }
            }

            // Set bounds
            exteriorBounds = bounds;

            // Clear scroll velocity
            cameraVelocity = Vector3.Zero;

            // Centre camera in layout
            cameraPosition.X = (exteriorBounds.Max.X - exteriorBounds.Min.X) / 2;
            cameraPosition.Z = -(exteriorBounds.Max.Z - exteriorBounds.Min.Z) / 2;
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
            foreach (var block in dfLocation.Dungeon.Blocks)
            {
                // Get block key
                int key = GetBlockKey(block.X, block.Z);

                // Create block position data
                BlockPosition blockPosition = new BlockPosition();
                blockPosition.name = block.BlockName;
                blockPosition.block = host.BlockManager.LoadBlock(block.BlockName);

                // Set block position
                blockPosition.position = new Vector3(block.X * rdbBlockSide, 0, -(block.Z * rdbBlockSide));

                // Load block models and textures
                LoadBlockResources(ref blockPosition.block);

                // Add to layout dictionary
                dungeonLayout.Add(key, blockPosition);

                // Merge bounding boxes
                Vector3 min = blockPosition.block.BoundingBox.Min + blockPosition.position;
                Vector3 max = blockPosition.block.BoundingBox.Max + blockPosition.position;
                bounds = BoundingBox.CreateMerged(bounds, new BoundingBox(min, max));
            }

            // Set bounds
            dungeonBounds = bounds;

            // Clear scroll velocity
            cameraVelocity = Vector3.Zero;

            // Centre camera in layout
            cameraPosition.X = (dungeonBounds.Max.X - dungeonBounds.Min.X) / 2;
            cameraPosition.Z = -(dungeonBounds.Max.Z - dungeonBounds.Min.Z) / 2;
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
        private void LoadBlockResources(ref BlockManager.Block block)
        {
            // Load block models
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
        }

        #endregion

        #region Camera Methods

        private void TranslateCameraXZ(float x, float z)
        {
            // Translate camera vector
            cameraPosition.X += x;
            cameraPosition.Z += z;

            // Keep camera within exterior bounds
            if (batchMode == BatchModes.SingleExteriorBlock ||
                batchMode == BatchModes.FullExterior)
            {
                if (cameraPosition.X < exteriorBounds.Min.X) cameraPosition.X = exteriorBounds.Min.X;
                if (cameraPosition.Z < exteriorBounds.Min.Z) cameraPosition.Z = exteriorBounds.Min.Z;
                if (cameraPosition.X > exteriorBounds.Max.X) cameraPosition.X = exteriorBounds.Max.X;
                if (cameraPosition.Z > exteriorBounds.Max.Z) cameraPosition.Z = exteriorBounds.Max.Z;
            }

            // Keep camera within dungeon bounds
            if (batchMode == BatchModes.SingleDungeonBlock ||
                batchMode == BatchModes.FullDungeon)
            {
                if (cameraPosition.X < dungeonBounds.Min.X) cameraPosition.X = dungeonBounds.Min.X;
                if (cameraPosition.Z < dungeonBounds.Min.Z) cameraPosition.Z = dungeonBounds.Min.Z;
                if (cameraPosition.X > dungeonBounds.Max.X) cameraPosition.X = dungeonBounds.Max.X;
                if (cameraPosition.Z > dungeonBounds.Max.Z) cameraPosition.Z = dungeonBounds.Max.Z;
            }

            // Update view matrix
            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraPosition + cameraReference, cameraUpVector);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates projection matrix to current view size.
        /// </summary>
        private void UpdateProjectionMatrix()
        {
            // Create projection matrix
            if (host.IsReady)
            {
                float aspectRatio = (float)host.Width / (float)host.Height;
                projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, nearPlaneDistance, farPlaneDistance);
            }
        }

        /// <summary>
        /// Updates status message.
        /// </summary>
        private void UpdateStatusMessage()
        {
            // Set the message
            host.StatusMessage = string.Empty;
        }

        #endregion

    }

}
