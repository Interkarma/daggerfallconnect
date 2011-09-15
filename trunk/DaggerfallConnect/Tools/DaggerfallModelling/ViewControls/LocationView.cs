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

        // XNALibrary
        private Input input;
        private Renderer renderer;

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

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public LocationView(ViewHost host)
            : base(host)
        {
            // Create XNALibrary subsystems
            input = new Input();
            renderer = new Renderer(host.TextureManager);
        }

        #endregion

        #region Abstract Overrides

        /// <summary>
        /// Called by host when view should initialise.
        /// </summary>
        public override void Initialize()
        {
            // Initialise renderer sky
            renderer.InitialiseSky(host.Arena2Path);

            // Initialise input subsystem
            input.ActiveDevices = Input.DeviceFlags.None;
            input.InvertMouseLook = false;
            input.InvertControllerLook = true;

            // Start in top-down camera mode
            CameraMode = CameraModes.TopDown;

            // Set top-down camera reference
            topDownCamera.Reference = new Vector3(0f, -1.0f, -0.01f);
        }

        /// <summary>
        /// Called by host when view should update.
        /// </summary>
        public override void Update()
        {
            // Determine input flags
            Input.DeviceFlags flags = Input.DeviceFlags.None;
            if (CameraMode == CameraModes.Free)
            {
                // Always enable controller
                flags |= Input.DeviceFlags.Controller;

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
            input.ActiveDevices = flags;
            input.Update(host.ElapsedTime);

            // Apply input to camera and update scene
            input.Apply(renderer.Camera);
            renderer.Scene.Update(host.ElapsedTime);
        }

        /// <summary>
        /// Called by host when view should redraw.
        /// </summary>
        public override void Draw()
        {
            // Render scene
            renderer.Draw();
        }

        /// <summary>
        /// Called by host when view should resize.
        /// </summary>
        public override void Resize()
        {
            renderer.UpdateCameraAspectRatio(host.Width, host.Height);
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
            renderer.UpdatePointerRay(e.X, e.Y);

            // Top down camera movement
            if (CameraMode == CameraModes.TopDown)
            {
                // Scene dragging
                if (host.RightMouseDown)
                {
                    renderer.Camera.Translate(
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
        }

        /// <summary>
        /// Called when the view is resumed after being inactive.
        ///  Allows view to perform any layout or other requirements before redraw.
        /// </summary>
        public override void ResumeView()
        {
            // Set texture manager climate
            host.TextureManager.ClimateType = base.Climate;

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

            // Set camera
            switch (cameraMode)
            {
                case CameraModes.TopDown:
                    renderer.Camera = topDownCamera;
                    renderer.Options = Renderer.RendererOptions.Picking;
                    SetTopDownCameraBackground();
                    break;
                case CameraModes.Free:
                    renderer.Camera = freeCamera;
                    renderer.Options = Renderer.RendererOptions.Flats | Renderer.RendererOptions.Picking;
                    if (sceneType == SceneTypes.Exterior)
                        renderer.Options |= Renderer.RendererOptions.SkyPlane;
                    SetFreeCameraBackground();
                    break;
            }

            // Upadate camera
            renderer.UpdateCameraAspectRatio(-1, -1);

            // Clear camera velocity
            cameraVelocity = Vector3.Zero;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Builds a new scene containing a single RMB or RDB block.
        /// </summary>
        /// <param name="blockName">Block name.</param>
        public void CreateBlockScene(string blockName)
        {
            // Check if resulting scene will be the same
            if (this.block.Name == blockName &&
                this.sceneType == SceneTypes.Block)
            {
                return;
            }

            // Create block node
            renderer.Scene.ResetScene();
            BlockNode node = host.SceneBuilder.CreateBlockNode(blockName, null);
            if (node == null)
                return;

            // Store data
            this.block = node.Block;
            this.sceneType = SceneTypes.Block;

            // Add node to scene
            renderer.Scene.AddNode(null, node);

            // Update scene so bounds are correct
            renderer.Scene.Update(TimeSpan.MinValue);

            // Get centre position for this block.
            // This is worked out using static block dimensions to get a
            // nice "centred" feeling when scrolling through blocks.
            float side = (block.Type == DFBlock.BlockTypes.Rmb) ?
                SceneBuilder.RMBSide : SceneBuilder.RDBSide;
            Vector3 center = new Vector3(side / 2, 0, -side / 2);

            // Set custom movement bounds
            float radius = renderer.Scene.Root.TransformedBounds.Radius;
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
            SetTopDownCameraBackground();
            SetFreeCameraBackground();
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
            renderer.Scene.ResetScene();
            renderer.BackgroundColor = generalBackgroundColor;
            LocationNode node = host.SceneBuilder.CreateExteriorLocationNode(regionName, locationName);
            if (node == null)
                return;

            // Store data
            this.location = node.Location;
            this.sceneType = SceneTypes.Exterior;

            // Add node to scene
            renderer.Scene.AddNode(null, node);

            // Update scene so bounds are correct
            renderer.Scene.Update(TimeSpan.MinValue);

            // Set custom movement bounds
            Vector3 center = renderer.Scene.Root.TransformedBounds.Center;
            float radius = renderer.Scene.Root.TransformedBounds.Radius;
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

            // Set background
            SetTopDownCameraBackground();
            SetFreeCameraBackground();

            // Set sky
            if (renderer.Sky != null)
            {
                renderer.Sky.SkyIndex = node.Location.SkyArchive;
            }
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
            renderer.Scene.ResetScene();
            renderer.BackgroundColor = generalBackgroundColor;
            LocationNode node = host.SceneBuilder.CreateDungeonLocationNode(regionName, locationName);
            if (node == null)
                return;

            // Store data
            this.location = node.Location;
            this.sceneType = SceneTypes.Dungeon;

            // Add node to scene
            renderer.Scene.AddNode(null, node);

            // Update scene so bounds are correct
            renderer.Scene.Update(TimeSpan.MinValue);

            // Set custom movement bounds
            Vector3 center = renderer.Scene.Root.TransformedBounds.Center;
            float radius = renderer.Scene.Root.TransformedBounds.Radius;
            BoundingBox movementBounds = new BoundingBox(
                new Vector3(center.X - radius, cameraFloorHeight, center.Z - radius),
                new Vector3(center.X + radius, cameraCeilingHeight, center.Z + radius));
            topDownCamera.MovementBounds = movementBounds;
            freeCamera.MovementBounds = movementBounds;

            // Position top-down camera
            topDownCamera.CentreInBounds(topDownCameraStartHeight);

            // Position free camera
            freeCamera.CentreInBounds(freeCamera.EyeHeight);
            freeCamera.Position = new Vector3(
                    freeCamera.Position.X, freeCamera.Position.Y, 0f);

            // Set background
            SetTopDownCameraBackground();
            SetFreeCameraBackground();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets background colours for top down camera.
        /// </summary>
        private void SetTopDownCameraBackground()
        {
            // Always use general in top down mode
            renderer.BackgroundColor = generalBackgroundColor;
        }

        /// <summary>
        /// Sets background sky/colours for free camera.
        /// </summary>
        private void SetFreeCameraBackground()
        {
            if (sceneType == SceneTypes.Dungeon)
                renderer.BackgroundColor = dungeonBackgroundColor;
            else
                renderer.BackgroundColor = generalBackgroundColor;
        }

        /// <summary>
        /// Executes the action record of a parent action.
        /// </summary>
        private void RunActionRecord()
        {
            // Exit if nothing under pointer
            SceneNode node = renderer.PointerOverNode;
            if (node == null)
                return;

            // Exit if no action present on node
            if (!node.Action.Enabled)
                return;

            // Link back to start node if there is a parent
            while (node.Action.PreviousNode != null)
            {
                node = (ModelNode)node.Action.PreviousNode;
            }

            // TEST: Run action chain directly to end state
            do
            {
                float degreesX, degreesY, degreesZ;
                Matrix rotationX, rotationY, rotationZ;

                // Get rotation matrix for each axis
                degreesX = node.Action.Rotation.X;
                degreesY = node.Action.Rotation.Y;
                degreesZ = -node.Action.Rotation.Z;
                rotationX = Matrix.CreateRotationX(MathHelper.ToRadians(degreesX));
                rotationY = Matrix.CreateRotationY(MathHelper.ToRadians(degreesY));
                rotationZ = Matrix.CreateRotationZ(MathHelper.ToRadians(degreesZ));

                // Create final matrix
                Matrix matrix = Matrix.Identity;
                matrix *= rotationX;
                matrix *= rotationY;
                matrix *= rotationZ;
                matrix *= Matrix.CreateTranslation(node.Action.Translation);

                if (node.Action.ActionState == SceneNode.ActionState.Start)
                {
                    // Apply matrix
                    node.Action.Matrix = matrix;
                    node.Action.ActionState = SceneNode.ActionState.End;
                }
                else if (node.Action.ActionState == SceneNode.ActionState.End)
                {
                    // Apply inverse matrix
                    Matrix.Invert(ref matrix, out matrix);
                    node.Action.Matrix = matrix;
                    node.Action.ActionState = SceneNode.ActionState.Start;
                }

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

        #endregion

    }

}
