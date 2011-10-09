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
#endregion

namespace DaggerfallModelling.ViewControls
{

    /// <summary>
    /// Explore blocks, cities, and dungeons.
    /// </summary>
    public class LocationView : ViewBase
    {

        #region Class Variables

        // Scene data
        private DFBlock block;
        private DFLocation location;
        private SceneTypes sceneType = SceneTypes.None;

        // Cameras
        private Camera topDownCamera = new Camera();
        private Camera freeCamera = new Camera();

        // Status message
        private string currentStatus = string.Empty;

        // Movement
        private Vector3 cameraVelocity;
        private float cameraStep = 5.0f;
        private float wheelStep = 100.0f;
        private static float topDownCameraStartHeight = 6000.0f;
        private static float cameraFloorHeight = 0.0f;
        private static float cameraCeilingHeight = 10000.0f;

        // Appearance
        private Color generalBackgroundColor = Color.LightGray;
        private Color dungeonBackgroundColor = Color.Black;

        // Textures
        private Texture2D crosshairTexture = null;

        #endregion

        #region Class Structures

        /// <summary>
        /// Used to track what kind of scene has been set.
        /// </summary>
        public enum SceneTypes
        {
            None,
            Block,
            Exterior,
            Dungeon,
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets current scene type based on scene data
        ///  previously passed in.
        /// </summary>
        public SceneTypes SceneType
        {
            get { return sceneType; }
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
        public Sky Sky
        {
            get { return host.Core.Renderer.Sky; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public LocationView(ViewHost host)
            : base(host)
        {
        }

        #endregion

        #region Abstract Overrides

        /// <summary>
        /// Called by host when view should initialise.
        /// </summary>
        public override void Initialize()
        {
            // Initialise renderer sky and compass
            host.Core.Renderer.InitialiseSky(host.Arena2Path);
            host.Core.Renderer.InitialiseCompass(host.Arena2Path);

            // Initialise input subsystem
            host.Core.Input.ActiveDevices = Input.DeviceFlags.None;

            // Start in top-down camera mode
            CameraMode = CameraModes.TopDown;

            // Set top-down camera reference
            topDownCamera.Reference = new Vector3(0f, -1.0f, -0.01f);

            // Load crosshair texture
            LoadCrosshairTexture();

            // TEST: Force on night time and winter
            host.Core.TextureManager.Daytime = false;
        }

        /// <summary>
        /// Called by host when view should update.
        /// </summary>
        public override void Update()
        {
            // Update input
            UpdateInput();

            // Update scene
            host.Core.Scene.Update(host.ElapsedTime);

            // Update gravity
            if (host.AppSettings.EnableGravity)
            {
                freeCamera.RestrictVertical = true;
                host.Core.Gravity.Update(host.ElapsedTime, freeCamera, host.Core.Collision);
            }
            else
            {
                freeCamera.RestrictVertical = false;
            }

            // Update collision
            if (host.AppSettings.EnableCollision)
            {
                host.Core.Collision.Update(
                    host.Core.Camera,
                    host.Core.Scene,
                    host.Core.Input);
            }
            else
            {
                host.Core.Input.Apply(
                    host.Core.Camera,
                    true);
            }

            // Update camera
            host.Core.Camera.Update();
        }

        /// <summary>
        /// Called by host when view should redraw.
        /// </summary>
        public override void Draw()
        {
            // Render scene
            host.Core.Draw();

            // Draw crosshair if gamepad connected
            if (host.Core.Input.GamePadConnected &&
                CameraMode == CameraModes.Free)
            {
                DrawCrosshair();
            }

#if DEBUG
            // Draw performance text
            DrawPerformanceString();
#endif
        }

        /// <summary>
        /// Called by host when view should resize.
        /// </summary>
        public override void Resize()
        {
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
            host.Core.Renderer.UpdatePointerRay(e.X, e.Y);

            // Top down camera movement
            if (CameraMode == CameraModes.TopDown)
            {
                // Scene dragging
                if (host.RightMouseDown)
                {
                    host.Core.Camera.Translate(
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
                topDownCamera.Translate(0, -amount, 0);
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
            // Top-down camera movement
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
            if (host.Core.Renderer.PointerOverNode != null)
            {
                if (host.Core.Renderer.PointerOverNode is ModelNode)
                {
                    ModelNode node = (ModelNode)host.Core.Renderer.PointerOverNode;
                    host.ShowModelView(node.Model.DFMesh.ObjectId, Climate);
                }
            }
        }

        /// <summary>
        /// Called when a key is pressed.
        /// </summary>
        /// <param name="e">KeyEventArgs.</param>
        public override void OnKeyDown(KeyEventArgs e)
        {
            // Run action record when user hits activate key
            if (e.KeyCode == Keys.R ||
                e.KeyCode == Keys.Enter)
            {
                RunActionRecord();
            }

            // Jump in free camera mode
            if (e.KeyCode == Keys.Space &&
                CameraMode == CameraModes.Free)
            {
                host.Core.Gravity.Jump();
            }
        }

        /// <summary>
        /// Called when the view is resumed after being inactive.
        ///  Allows view to perform any layout or other requirements before redraw.
        /// </summary>
        public override void ResumeView()
        {
            // Set texture manager climate
            if (this.sceneType == SceneTypes.Exterior ||
                this.SceneType == SceneTypes.Block)
                host.TextureManager.ClimateType = ClimateType;
            else
                host.TextureManager.ClimateType = DFLocation.ClimateBaseType.None;

            // Clear scroll velocity
            cameraVelocity = Vector3.Zero;

            // Resume view
            Resize();
            host.ModelManager.CacheModelData = true;
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

            // Update camera settings
            UpdateCamera();
            cameraVelocity = Vector3.Zero;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Builds a new scene containing a single RMB or RDB block.
        /// </summary>
        /// <param name="blockName">Block name.</param>
        public void CreateBlockScene(string blockName, DFLocation.ClimateSettings? climate)
        {
            // Check if resulting scene will be the same
            if (this.block.Name == blockName &&
                this.sceneType == SceneTypes.Block)
            {
                return;
            }

            // Create block node
            host.Core.Scene.ResetScene();
            BlockNode node = host.Core.SceneBuilder.CreateBlockNode(blockName, climate, true);
            if (node == null)
                return;

            // Store data
            this.block = node.Block;
            this.sceneType = SceneTypes.Block;
            base.Climate = climate;

            // Add node to scene
            host.Core.Scene.AddNode(null, node);

            // Update scene so bounds are correct
            host.Core.Scene.Update(TimeSpan.MinValue);

            // Get centre position for this block.
            // This is worked out using static block dimensions to get a
            // nice "centred" feeling when scrolling through blocks.
            float side = (block.Type == DFBlock.BlockTypes.Rmb) ?
                SceneBuilder.RMBSide : SceneBuilder.RDBSide;
            Vector3 center = new Vector3(side / 2, 0, -side / 2);

            // Set custom movement bounds
            float radius = host.Core.Scene.Root.TransformedBounds.Radius;
            BoundingBox movementBounds = new BoundingBox(
                new Vector3(center.X - radius, cameraFloorHeight, center.Z - radius),
                new Vector3(center.X + radius, cameraCeilingHeight, center.Z + radius));
            topDownCamera.MovementBounds = movementBounds;
            freeCamera.MovementBounds = movementBounds;

            // Position top-down camera
            topDownCamera.CentreInBounds(topDownCameraStartHeight);

            // Position free camera
            freeCamera.Reference = Vector3.Forward;
            if (block.Type == DFBlock.BlockTypes.Rmb)
            {
                freeCamera.CentreInBounds(freeCamera.EyeHeight);
                freeCamera.Position = new Vector3(
                    freeCamera.Position.X, freeCamera.Position.Y, 0f);
            }
            else
            {
                freeCamera.CentreInBounds(radius / 2);
                freeCamera.Position = new Vector3(
                    freeCamera.Position.X, freeCamera.Position.Y, movementBounds.Max.Z);
            }

            // Set background
            UpdateCamera();

            // Set status message
            currentStatus = string.Format("Exploring block {0}.", blockName);
        }

        /// <summary>
        /// Builds a new scene containing a location exterior.
        /// </summary>
        /// <param name="regionName">Region name.</param>
        /// <param name="locationName">Location name.</param>
        public void CreateExteriorLocationScene(string regionName, string locationName)
        {
            // Check if resulting scene will be the same
            if (this.location.RegionName == regionName &&
                this.location.Name == locationName &&                
                this.sceneType == SceneTypes.Exterior)
            {
                return;
            }

            // Create location node
            host.Core.Scene.ResetScene();
            host.Core.Renderer.BackgroundColor = generalBackgroundColor;
            LocationNode node = host.Core.SceneBuilder.CreateExteriorLocationNode(regionName, locationName);
            if (node == null)
                return;

            // Store data
            this.location = node.Location;
            this.sceneType = SceneTypes.Exterior;
            base.Climate = location.Climate;

            // Add node to scene
            host.Core.Scene.AddNode(null, node);

            // Update scene so bounds are correct
            host.Core.Scene.Update(TimeSpan.MinValue);

            // Set custom movement bounds
            Vector3 center = host.Core.Scene.Root.TransformedBounds.Center;
            float radius = host.Core.Scene.Root.TransformedBounds.Radius;
            BoundingBox movementBounds = new BoundingBox(
                new Vector3(center.X - radius, cameraFloorHeight, center.Z - radius),
                new Vector3(center.X + radius, cameraCeilingHeight, center.Z + radius));
            topDownCamera.MovementBounds = movementBounds;
            freeCamera.MovementBounds = movementBounds;

            // Position top-down camera
            topDownCamera.CentreInBounds(topDownCameraStartHeight);

            // Position free camera
            freeCamera.Reference = Vector3.Forward;
            freeCamera.CentreInBounds(freeCamera.EyeHeight);
            freeCamera.Position = new Vector3(
                    freeCamera.Position.X, freeCamera.Position.Y, 0f);

            // Update camera
            UpdateCamera();

            // Set sky
            if (host.Core.Renderer.Sky != null)
                host.Core.Renderer.Sky.SkyIndex = node.Location.Climate.SkyArchive;

            // Set status message
            currentStatus = string.Format("Exploring {0} (Exterior).", locationName);
        }

        /// <summary>
        /// Builds a new scene containing a location dungeon.
        /// </summary>
        /// <param name="regionName">Region name.</param>
        /// <param name="locationName">Location name.</param>
        public void CreateDungeonLocationScene(string regionName, string locationName)
        {
            // Check if resulting scene will be the same
            if (this.location.RegionName == regionName &&
                this.location.Name == locationName &&
                this.sceneType == SceneTypes.Dungeon)
            {
                return;
            }

            // Create location node
            host.Core.Scene.ResetScene();
            host.Core.Renderer.BackgroundColor = generalBackgroundColor;
            LocationNode node = host.Core.SceneBuilder.CreateDungeonLocationNode(regionName, locationName);
            if (node == null)
                return;

            // Store data
            this.location = node.Location;
            this.sceneType = SceneTypes.Dungeon;
            base.Climate = null;

            // Add node to scene
            host.Core.Scene.AddNode(null, node);

            // Update scene so bounds are correct
            host.Core.Scene.Update(TimeSpan.MinValue);

            // Set custom movement bounds
            Vector3 center = host.Core.Scene.Root.TransformedBounds.Center;
            float radius = host.Core.Scene.Root.TransformedBounds.Radius;
            BoundingBox movementBounds = new BoundingBox(
                new Vector3(center.X - radius, center.Y - radius, center.Z - radius),
                new Vector3(center.X + radius, center.Y + radius, center.Z + radius));
            topDownCamera.MovementBounds = movementBounds;
            freeCamera.MovementBounds = movementBounds;

            // Position top-down camera
            topDownCamera.CentreInBounds(topDownCameraStartHeight);

            // Position free camera
            freeCamera.Reference = Vector3.Forward;
            freeCamera.Position = new Vector3(
                    center.X + SceneBuilder.RDBSide / 2,
                    center.Y,
                    movementBounds.Max.Z - SceneBuilder.RDBSide / 2);

            // Update camera
            UpdateCamera();

            // Set status message
            currentStatus = string.Format("Exploring {0} (Dungeon).", locationName);
        }

        /// <summary>
        /// Moves active camera to X-Z origin of specified block.
        /// </summary>
        /// <param name="name">Name of block.</param>
        public void MoveToBlock(int x, int z)
        {
            cameraVelocity = Vector3.Zero;
            Vector3 pos = host.Core.Camera.Position;
            if (sceneType == SceneTypes.Exterior)
            {
                pos.X = x * SceneBuilder.RMBSide + SceneBuilder.RMBSide / 2;
                pos.Z = -z * SceneBuilder.RMBSide - SceneBuilder.RMBSide / 2;
                host.Core.Camera.Position = pos;
            }
            else if (sceneType == SceneTypes.Dungeon)
            {
                pos.X = x * SceneBuilder.RDBSide + SceneBuilder.RDBSide / 2;
                pos.Z = -z * SceneBuilder.RDBSide - SceneBuilder.RDBSide / 2;
                host.Core.Camera.Position = pos;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Update the camera.
        /// </summary>
        private void UpdateCamera()
        {
            // Set camera background
            SetCameraBackground();

            // Update camera settings
            switch (base.CameraMode)
            {
                case CameraModes.TopDown:
                    host.Core.Camera = topDownCamera;
                    host.Core.Renderer.Options = DefaultRenderer.RendererOptions.Picking;
                    break;
                case CameraModes.Free:
                    host.Core.Camera = freeCamera;
                    host.Core.Renderer.Options =
                        DefaultRenderer.RendererOptions.Flats |
                        DefaultRenderer.RendererOptions.Picking |
                        DefaultRenderer.RendererOptions.Compass;
                    if (sceneType == SceneTypes.Exterior)
                        host.Core.Renderer.Options |= DefaultRenderer.RendererOptions.SkyPlane;
                    break;
            }

            // Update viewport
            host.Core.Resize(0, 0);
        }

        /// <summary>
        /// Gets currently active camera.
        /// </summary>
        /// <returns></returns>
        private Camera GetActiveCamera()
        {
            switch (base.CameraMode)
            {
                case CameraModes.TopDown:
                    return topDownCamera;
                case CameraModes.Free:
                    return freeCamera;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Sets background colours.
        /// </summary>
        private void SetCameraBackground()
        {
            if (sceneType == SceneTypes.Dungeon)
                host.Core.Renderer.BackgroundColor = dungeonBackgroundColor;
            else
                host.Core.Renderer.BackgroundColor = generalBackgroundColor;
        }

        /// <summary>
        /// Executes the action record of a parent action.
        /// </summary>
        private void RunActionRecord()
        {
            // Exit if nothing under pointer
            SceneNode node = host.Core.Renderer.PointerOverNode;
            if (node == null)
                return;

            // Exit if no action present on node
            if (!node.Action.Enabled)
                return;

            // Link back to start node if there is a parent
            while (node.Action.PreviousNode != null)
            {
                node = node.Action.PreviousNode;
            }

            // Run action chain
            do
            {
                if (node.Action.ActionState == SceneNode.ActionState.Start)
                    node.Action.ActionState = SceneNode.ActionState.RunningForwards;
                else if (node.Action.ActionState == SceneNode.ActionState.End)
                    node.Action.ActionState = SceneNode.ActionState.RunningBackwards;

                // Get next node in chain
                node = node.Action.NextNode;
            } while (node != null);
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
        /// Load crosshair texture.
        /// </summary>
        private void LoadCrosshairTexture()
        {
            // Load crosshair image
            ImageFileReader reader = new ImageFileReader(host.Arena2Path);
            reader.LibraryType = LibraryTypes.Cif;
            DFImageFile crosshair = reader.GetImageFile("PNTER.CIF");
            DFBitmap crosshairBitmap = crosshair.GetBitmapFormat(4, 0, 0, DFBitmap.Formats.RGBA);

            // Create texture
            crosshairTexture = new Texture2D(
                host.GraphicsDevice,
                crosshairBitmap.Width,
                crosshairBitmap.Height,
                false,
                SurfaceFormat.Color);

            // Set data
            crosshairTexture.SetData<byte>(
                0,
                null,
                crosshairBitmap.Data,
                0,
                crosshairBitmap.Width * crosshairBitmap.Height * 4);
        }

        /// <summary>
        /// Draws crosshair in centre of viewport.
        /// </summary>
        private void DrawCrosshair()
        {
            // Get centre of viewport
            Vector2 pos;
            pos.X = host.GraphicsDevice.Viewport.Width / 2 - 6;
            pos.Y = host.GraphicsDevice.Viewport.Height / 2 - 6;

            // Begin drawing
            host.Core.SpriteBatch.Begin();

            // Draw crosshair
            host.Core.SpriteBatch.Draw(crosshairTexture, pos, Color.White);

            // End drawing
            host.Core.SpriteBatch.End();
        }

        /// <summary>
        /// Collect input.
        /// </summary>
        private void UpdateInput()
        {
            // Determine input flags
            Input.DeviceFlags flags = Input.DeviceFlags.None;
            if (CameraMode == CameraModes.Free)
            {
                // Always enable controller
                flags |= Input.DeviceFlags.GamePad;

                // Only enable keyboard and mouse if host has focus
                if (host.Focused)
                {
                    flags |= Input.DeviceFlags.Keyboard;
                    flags |= Input.DeviceFlags.Mouse;
                }
            }

            // Apply top-down camera velocity
            topDownCamera.Translate(cameraVelocity.X, 0f, cameraVelocity.Z);

            // Gather input
            host.Core.Input.ActiveDevices = flags;
            host.Core.Input.InvertMouseLook = host.AppSettings.InvertMouseY;
            host.Core.Input.InvertGamePadLook = host.AppSettings.InvertGamePadY;
            host.Core.Input.Update(host.ElapsedTime);

            // GamePad stuff
            if (host.Core.Input.GamePadConnected)
            {
                // Update gamepad ray
                if (host.Core.Input.GamePadInputReceived)
                {
                    Point pos;
                    pos.X = host.GraphicsDevice.Viewport.Width / 2;
                    pos.Y = host.GraphicsDevice.Viewport.Height / 2;
                    host.Core.Renderer.UpdatePointerRay(pos.X, pos.Y);
                }

                // Handle triggering action record from controller
                if (host.Core.Input.GamePadState.Buttons.A == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                {
                    RunActionRecord();
                }

                // Handle jumping from controller
                if (host.Core.Input.GamePadState.Buttons.Y == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                {
                    host.Core.Gravity.Jump();
                }
            }
        }

#if DEBUG
        /// <summary>
        /// Draws performance string
        /// </summary>
        private void DrawPerformanceString()
        {
            string performance = string.Format(
                "Scene: {0}ms, Renderer: {1}ms, Collision: {2}ms, FPS: {3}, GCPS: {4:0.00}",
                host.Core.Scene.UpdateTime,
                host.Core.Renderer.DrawTime,
                host.Core.Collision.UpdateTime,
                host.FPS,
                (float)host.GarbageCollectionCount / (float)host.Timer.Elapsed.Seconds);

            host.Core.SpriteBatch.Begin();
            host.Core.SpriteBatch.DrawString(host.Core.SmallFont, performance, Vector2.One, Color.Black);
            host.Core.SpriteBatch.DrawString(host.Core.SmallFont, performance, Vector2.Zero, Color.Yellow);
            host.Core.SpriteBatch.End();
        }
#endif

        #endregion

    }

}
