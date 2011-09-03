﻿// Project:         DaggerfallModelling
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
        private static float cameraFloorHeight = 0.0f;
        private static float cameraCeilingHeight = 10000.0f;
        private static float cameraStartHeight = 6000.0f;
        //private static float cameraDungeonFreedom = 1000.0f;

        // Appearance
        private Color generalBackgroundColor = Color.LightGray;
        //private Color dungeonBackgroundColor = Color.Black;
        //private Color modelHighlightColor = Color.Gold;
        //private Color doorHighlightColor = Color.Red;
        //private Color actionHighlightColor = Color.CornflowerBlue;

        #endregion

        #region Class Structures
        #endregion

        #region Properties
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
            renderer = new Renderer(host.GraphicsDevice);
        }

        #endregion

        #region Abstract Overrides

        /// <summary>
        /// Called by host when view should initialise.
        /// </summary>
        public override void Initialize()
        {
            // Initialise renderer subsystem
            renderer.Scene.TextureManager = host.TextureManager;
            renderer.Scene.ModelManager = host.ModelManager;
            renderer.Scene.BlockManager = host.BlockManager;

            // Initialise input subsystem
            input.ActiveDevices = Input.DeviceFlags.None;
            input.InvertMouseLook = false;
            input.InvertControllerLook = true;

            // Start in top-down camera mode
            CameraMode = CameraModes.TopDown;

            // Initialise camera positions
            ResetCameras();
        }

        /// <summary>
        /// Called by host when view should update.
        /// </summary>
        public override void Update()
        {
            input.Update(host.ElapsedTime);
            input.Apply(renderer.Camera);
            renderer.Scene.Update(host.ElapsedTime);
        }

        /// <summary>
        /// Called by host when view should redraw.
        /// </summary>
        public override void Draw()
        {
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
        }

        /// <summary>
        /// Called when a mouse button is pressed.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public override void OnMouseDown(MouseEventArgs e)
        {
        }

        /// <summary>
        /// Called when a mouse button is released.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public override void OnMouseUp(MouseEventArgs e)
        {
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
        }

        /// <summary>
        /// Called when the view is resumed after being inactive.
        ///  Allows view to perform any layout or other requirements before redraw.
        /// </summary>
        public override void ResumeView()
        {
            Resize();
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
                    input.ActiveDevices = Input.DeviceFlags.None;
                    break;
                case CameraModes.Free:
                    renderer.Camera = freeCamera;
                    input.ActiveDevices = Input.DeviceFlags.All;
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
        /// Shows a block scene.
        /// </summary>
        /// <param name="name">Block name.</param>
        public void ShowBlockScene(string name)
        {
            // Create scene node
            DFBlock.BlockTypes type;
            renderer.Scene.ResetScene();
            renderer.BackgroundColor = generalBackgroundColor;
            SceneNode node = renderer.Scene.AddBlockNode(null, name, out type);
            if (node == null)
                return;

            // Set camera movement bounds
            switch (type)
            {
                case DFBlock.BlockTypes.Rmb:
                    topDownCamera.MovementBounds = BlockManager.RMBBoundingBox;
                    freeCamera.MovementBounds = BlockManager.RMBBoundingBox;
                    break;
                case DFBlock.BlockTypes.Rdb:
                    topDownCamera.MovementBounds = BlockManager.RDBBoundingBox;
                    freeCamera.MovementBounds = BlockManager.RDBBoundingBox;
                    break;
                default:
                    return;
            }

            // Reset cameras
            ResetCameras();
        }

        /// <summary>
        /// Initialise camera position.
        /// </summary>
        public void ResetCameras()
        {
            // Set top-down camera position and reference
            topDownCamera.Position = new Vector3(0f, cameraStartHeight, 0f);
            topDownCamera.Reference = new Vector3(0f, -1.0f, -0.01f);
            topDownCamera.CentreInBounds(cameraStartHeight);

            // Set free camera position and reference
            freeCamera.Position = new Vector3(0f, 0f, 0f);
            freeCamera.Reference = new Vector3(0f, 0f, -1f);
            //freeCamera.CentreInBounds(cameraStartHeight);
        }

        #endregion

        #region Private Methods

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
