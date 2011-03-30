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
    /// Serves different ModelViewClient implementations to the users.
    /// </summary>
    public class ContentViewHost : WinFormsGraphicsDevice.GraphicsDeviceControl
    {

        #region Class Variables

        // Resources
        private bool isReady = false;
        private string arena2Path = string.Empty;

        // Managers
        private TextureManager textureManager;
        private ModelManager modelManager;

        // Mouse
        private Point mousePos;
        private long mouseTime;
        private Point mousePosDelta;
        private long mouseTimeDelta;
        private bool leftMouseDown = false;
        private bool rightMouseDown = false;
        private float mouseVelocity;
        private const float mouseVelocityMultiplier = 2.0f;

        // Timer
        private Timer animTimer = new Timer();
        private long startTime = 0;
        private long timeInSeconds;
        private uint frameCount = 0;
        private float timeDelta;

        // Views
        private ViewModes viewMode = ViewModes.SingleModelView;
        private Dictionary<ViewModes, ContentViewClient> viewClients;

        #endregion

        #region Class Structures

        /// <summary>
        /// Content modes supported.
        /// </summary>
        public enum ViewModes
        {
            /// <summary>Viewing model thumbnails.</summary>
            ThumbnailView,
            /// <summary>Viewing single model.</summary>
            SingleModelView,
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets ready flag indicating host is operating.
        ///  Must set Arena2 path before host can be ready.
        /// </summary>
        public bool IsReady
        {
            get { return isReady; }
        }

        /// <summary>
        /// Gets host TextureManager.
        /// </summary>
        public TextureManager TextureManager
        {
            get { return textureManager; }
        }

        /// <summary>
        /// Gets host ModelManager.
        /// </summary>
        public ModelManager ModelManager
        {
            get { return modelManager; }
        }

        /// <summary>
        /// Gets average time between frames.
        /// </summary>
        public float TimeDelta
        {
            get { return timeDelta; }
        }

        /// <summary>
        /// Gets current mouse position.
        /// </summary>
        public Point MousePos
        {
            get { return mousePos; }
        }

        /// <summary>
        /// Gets change from last mouse position.
        /// </summary>
        public Point MousePosDelta
        {
            get { return mousePosDelta; }
        }

        /// <summary>
        /// Gets mouse velocity.
        /// </summary>
        public float MouseVelocity
        {
            get { return mouseVelocity; }
        }

        /// <summary>
        /// Gets state of left mouse button.
        /// </summary>
        public bool LeftMouseDown
        {
            get { return leftMouseDown; }
        }

        /// <summary>
        /// Gets state of right mouse button.
        /// </summary>
        public bool RightMouseDown
        {
            get { return rightMouseDown; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ContentViewHost()
        {
            // Create view client dictionary
            viewClients = new Dictionary<ViewModes, ContentViewClient>();

            // Measure start time of control
            startTime = DateTime.Now.Ticks;
        }

        public ContentViewHost(string arena2Path)
            : this()
        {
            SetArena2Path(arena2Path);
        }

        #endregion

        #region Abstract Implementations

        /// <summary>
        /// Initialise the control.
        /// </summary>
        protected override void Initialize()
        {
            // Handle device reset event
            GraphicsDevice.DeviceReset += new EventHandler(GraphicsDevice_DeviceReset);
        }

        /// <summary>
        /// Draw control.
        /// </summary>
        protected override void Draw()
        {
            // Do nothing if not visible
            if (!Visible)
                return;

            // Exit if not ready to draw
            if (!isReady)
            {
                // Keep trying to initialise view
                if (!InitialiseView())
                {
                    // Just clear the display until ready
                    GraphicsDevice.Clear(Color.Gray);
                    return;
                }
            }

            // Draw current view mode
            viewClients[viewMode].Draw();

            // Increment frame count
            frameCount++;
        }

        #endregion

        #region Events

        /// <summary>
        /// GraphicsDevice reset.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        private void GraphicsDevice_DeviceReset(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Handles animations.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        void AnimTimer_Tick(object sender, EventArgs e)
        {
            // Update time in seconds
            timeInSeconds = (DateTime.Now.Ticks - startTime) / 10000000;

            // Calculate time delta
            timeDelta = (float)timeInSeconds / (float)frameCount;

            // Tick current view mode
            viewClients[viewMode].Tick();

            // Redraw
            this.Refresh();
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Resize event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // Resize current view mode
            if (isReady)
                viewClients[viewMode].Resize();
        }

        /// <summary>
        /// Mouse movement, delta, and time.
        /// </summary>
        /// <param name="e">MouseEventArgs.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Capture deltas from last position and time
            mousePosDelta = new Point(e.Location.X - mousePos.X, e.Location.Y - mousePos.Y);
            mouseTimeDelta = DateTime.Now.Ticks - mouseTime;

            // Ensure mouse time delta is never 0 (possible, and screws with distance/time calcs obviously)
            if (mouseTimeDelta == 0)
                mouseTimeDelta = 1;

            // Update position and time
            mousePos = new Point(e.Location.X, e.Location.Y);
            mouseTime = DateTime.Now.Ticks;

            // Calc and cap velocity
            mouseVelocity = ((float)mousePosDelta.Y / (float)mouseTimeDelta) * 100000.0f;
            mouseVelocity *= mouseVelocityMultiplier;
            if (mouseVelocity >= -2.5f && mouseVelocity <= 2.5f) mouseVelocity = 0.0f;

            // Move mouse in current view mode
            if (isReady)
                viewClients[viewMode].OnMouseMove(e);
        }

        /// <summary>
        /// Mouse wheel movement.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            // Send to client
            if (isReady)
                viewClients[viewMode].OnMouseWheel(e);
        }

        /// <summary>
        /// Mouse buttons down.
        /// </summary>
        /// <param name="e">MouseEventArgs.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // Set focus to this control
            this.Focus();

            // Store button state
            switch (e.Button)
            {
                case MouseButtons.Left:
                    leftMouseDown = true;
                    break;
                case MouseButtons.Right:
                    rightMouseDown = true;
                    break;
            }

            // Send to client
            if (isReady)
                viewClients[viewMode].OnMouseDown(e);
        }

        /// <summary>
        /// Mouse buttons up.
        /// </summary>
        /// <param name="e">MouseEventArgs.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            // Store button state
            switch (e.Button)
            {
                case MouseButtons.Left:
                    leftMouseDown = false;
                    break;
                case MouseButtons.Right:
                    rightMouseDown = false;
                    break;
            }

            // Send to client
            if (isReady)
                viewClients[viewMode].OnMouseUp(e);
        }

        /// <summary>
        /// Visible state of control has changed.
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            // Start and stop anim timer based on visible flag
            if (this.Visible)
                animTimer.Enabled = true;
            else
                animTimer.Enabled = false;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Allows view to set up managers for content loading. Must be called post control creation.
        ///  of control.
        /// </summary>
        /// <param name="arena2Path"></param>
        /// <returns></returns>
        public void SetArena2Path(string arena2Path)
        {
            if (this.Created)
            {
                // Save path and initialise view
                this.arena2Path = arena2Path;
                InitialiseView();
            }
            else
            {
                // Just save path and exit.
                // Will keep trying to initialise using this path later.
                this.arena2Path = arena2Path;
            }
        }

        /// <summary>
        /// Enable or disable animation timer.
        ///  Can only perform this operation when control is visible.
        /// </summary>
        /// <param name="suspend">True to suspend, false to resume.</param>
        public void EnableAnimTimer(bool enable)
        {
            // Exit if control not visible
            if (!this.Visible)
                return;

            // Suspend and resume timer
            animTimer.Enabled = enable;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Attempts to initialise view based on Arena2Path
        /// </summary>
        /// <returns></returns>
        private bool InitialiseView()
        {
            // Exit if not created or Arena2 is not set
            if (!this.Created || string.IsNullOrEmpty(arena2Path))
                return false;

            // Create managers
            try
            {
                textureManager = new TextureManager(GraphicsDevice, arena2Path);
                modelManager = new ModelManager(GraphicsDevice, arena2Path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            // Set ready flag
            isReady = true;

            // Initialise client views
            AttachViewClient(ViewModes.ThumbnailView, new ThumbnailViewClient(this));
            AttachViewClient(ViewModes.SingleModelView, new SingleModelViewClient(this));

            // Start anim timer
            animTimer.Interval = 8;
            animTimer.Enabled = true;
            animTimer.Tick += new EventHandler(AnimTimer_Tick);

            return true;
        }

        /// <summary>
        /// Attach a view client to the specified mode.
        /// </summary>
        /// <param name="mode">Mode to attach client with.</param>
        /// <param name="viewClient">ViewClient object.</param>
        private void AttachViewClient(ViewModes viewMode, ContentViewClient viewClient)
        {
            // Can only attach one view to a mode
            if (viewClients.ContainsKey(viewMode))
                return;

            // Add client to dictionary
            viewClients.Add(viewMode, viewClient);

            // Initialise client
            viewClient.Initialize();
        }

        #endregion

    }

}
