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

        // XNALibrary
        private Renderer renderer;
        private Input input;

        // Status message
        private string currentStatus = string.Empty;

        // Movement
        private Vector3 cameraVelocity;
        private static float cameraStartHeight = 6000.0f;
        //private static float cameraFloorHeight = 0.0f;
        //private static float cameraCeilingHeight = 10000.0f;
        //private static float cameraDungeonFreedom = 1000.0f;

        #endregion

        #region Class Structures
        #endregion

        #region Properties

        /// <summary>
        /// Gets scene manager.
        /// </summary>
        public SceneManager Scene
        {
            get { return renderer.Scene; }
        }

        /// <summary>
        /// Gets camera.
        /// </summary>
        public Camera Camera
        {
            get { return renderer.Camera; }
        }

        /// <summary>
        /// Gets input manager.
        /// </summary>
        public Input Input
        {
            get { return input; }
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
            renderer = new Renderer(host.GraphicsDevice);

            // Start in top-down camera mode
            CameraMode = CameraModes.TopDown;
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
            renderer.UpdateCameraAspectRatio(-1, -1);

            // Initialise input subsystem
            input.ActiveDevices = Input.DeviceFlags.None;
            input.InvertMouseLook = false;
            input.InvertControllerLook = true;

            // Initialise camera positions
            ResetCamera();
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

            // Clear camera velocity
            cameraVelocity = Vector3.Zero;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialise camera position.
        /// </summary>
        public void ResetCamera()
        {
            // Reset based on camera mode
            switch (cameraMode)
            {
                case CameraModes.TopDown:
                    ResetTopDownCamera();
                    break;
                case CameraModes.Free:
                    ResetFreeCamera();
                    break;
            }
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

        /// <summary>
        /// Resets top-down camera to starting position.
        /// </summary>
        private void ResetTopDownCamera()
        {
            renderer.Camera.Position = new Vector3(0, cameraStartHeight, 0);
            renderer.Camera.Reference = new Vector3(0f, -1.0f, -0.01f);
            // TODO: Set bounds and centre in bounds
        }

        /// <summary>
        /// Resets free camera to starting position.
        /// </summary>
        private void ResetFreeCamera()
        {
            renderer.Camera.Position = new Vector3(0, cameraStartHeight, 0);
            renderer.Camera.Reference = new Vector3(0f, 0f, -1f);
            // TODO: Set bounds and centre on southern edge
        }

        #endregion

    }

}
