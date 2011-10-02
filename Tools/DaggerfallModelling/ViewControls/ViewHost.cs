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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DaggerfallModelling.Classes;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using XNALibrary;
#endregion

namespace DaggerfallModelling.ViewControls
{

    /// <summary>
    /// Serves different views to the user from a single control.
    /// </summary>
    public class ViewHost : WinFormsGraphicsDevice.GraphicsDeviceControl
    {

        #region Class Variables

        // Resources
        private bool isReady = false;
        private string arena2Path = string.Empty;
        private AppSettings appSettings;

        // Mouse
        private Point mousePos;
        private long mouseTime;
        private Point mousePosDelta;
        private long mouseTimeDelta;
        private bool mouseInClientArea = false;
        private bool leftMouseDown = false;
        private bool rightMouseDown = false;
        private bool middleMouseDown = false;
        private Vector2 mouseVelocity;

        // Timing
        private bool update = false;
        private Stopwatch stopwatch = Stopwatch.StartNew();
        private TimeSpan lastTime;
        private TimeSpan elapsedTime;

        // FPS
        private int frameCount = 0;
        private long timeSinceLastFPSUpdate = 0;
        private int fps = 0;

#if DEBUG
        // Garbage counter
        WeakReference gcTracker = new WeakReference(new object());
        private int garbageCount = 0;
#endif

        // XNA
        private SpriteBatch spriteBatch;

        // XNA Content
        private ContentHelper contentHelper;
        private SpriteFont arialSmallFont;

        // XNALibrary
        private SceneBuilder sceneBuilder = null;

        // Appearance
        private Color backgroundColour = Color.Gray;

        // Views
        private ViewModes lastViewMode = ViewModes.None;
        private ViewModes viewMode = ViewModes.ThumbnailView;
        private Dictionary<ViewModes, ViewBase> viewClients;

        // Filtered model array consumed by thumbnail and single model views
        private uint[] filteredModelsArray;

        // Ray testing
        Ray mouseRay;

        // Status message
        private string statusMessage;

        #endregion

        #region Class Structures

        /// <summary>
        /// Content modes supported.
        /// </summary>
        public enum ViewModes
        {
            /// <summary>No view mode.</summary>
            None,
            /// <summary>Viewing model thumbnails.</summary>
            ThumbnailView,
            /// <summary>Viewing single model.</summary>
            ModelView,
            /// <summary>Viewing single block.</summary>
            BlockView,
            /// <summary>Viewing a location.</summary>
            LocationView,
            /// <summary>Viewing a dungeon.</summary>
            DungeonView,
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets current Arena2Path.
        /// </summary>
        public string Arena2Path
        {
            get { return arena2Path; }
        }

        /// <summary>
        /// Gets or sets application settings helper.
        /// </summary>
        public AppSettings AppSettings
        {
            get { return appSettings; }
            set { appSettings = value; }
        }

        /// <summary>
        /// Gets or sets status message.
        /// </summary>
        public string StatusMessage
        {
            get { return statusMessage; }
            set { SetStatusMessage(value); }
        }

        /// <summary>
        /// Gets ready flag indicating host is operating.
        ///  Must set Arena2 path before host can be ready.
        /// </summary>
        public bool IsReady
        {
            get { return isReady; }
        }

        /// <summary>
        /// Gets current view mode.
        /// </summary>
        public ViewModes ViewMode
        {
            get { return viewMode; }
        }

        /// <summary>
        /// Gets or sets camera mode.
        /// </summary>
        public ViewBase.CameraModes CameraMode
        {
            get
            {
                if (isReady)
                    return viewClients[viewMode].CameraMode;
                else
                    return ViewBase.CameraModes.None;
            }
            set
            {
                if (isReady)
                    viewClients[viewMode].CameraMode = value;
            }
        }

        /// <summary>
        /// Gets host ContentHelper.
        /// </summary>
        public ContentHelper ContentHelper
        {
            get { return contentHelper; }
        }

        /// <summary>
        /// Gets host SceneBuilder.
        /// </summary>
        public SceneBuilder SceneBuilder
        {
            get { return sceneBuilder; }
        }

        /// <summary>
        /// Gets host TextureManager.
        /// </summary>
        public TextureManager TextureManager
        {
            get { return sceneBuilder.TextureManager; }
        }

        /// <summary>
        /// Gets host ModelManager.
        /// </summary>
        public ModelManager ModelManager
        {
            get { return sceneBuilder.ModelManager; }
        }

        /// <summary>
        /// Gets host SpriteBatch.
        /// </summary>
        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
        }

        /// <summary>
        /// Gets engine timer.
        /// </summary>
        public Stopwatch Timer
        {
            get { return stopwatch; }
        }

        /// <summary>
        /// Gets elapsed time last frame.
        /// </summary>
        public TimeSpan ElapsedTime
        {
            get { return elapsedTime; }
        }

        /// <summary>
        /// Gets frames-per-second.
        /// </summary>
        public float FPS
        {
            get { return fps; }
        }

#if DEBUG
        /// <summary>
        /// Gets garbage collection count.
        /// </summary>
        public float GarbageCollectionCount
        {
            get { return garbageCount; }
        }
#endif

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
        public Vector2 MouseVelocity
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

        /// <summary>
        /// Gets state of middle mouse button.
        /// </summary>
        public bool MiddleMouseDown
        {
            get { return middleMouseDown; }
        }

        /// <summary>
        /// Gets flag stating if mouse is in client area of control.
        /// </summary>
        public bool MouseInClientArea
        {
            get { return mouseInClientArea; }
        }

        /// <summary>
        /// Gets or sets filtered model ID list for views.
        ///  Set to null to use full model database.
        /// </summary>
        public uint[] FilteredModelsArray
        {
            get { return filteredModelsArray; }
            set { AssignFilteredModels(value); }
        }

        /// <summary>
        /// Gets location climate, or null if no location loaded.
        /// </summary>
        public DFLocation.ClimateSettings? LocationClimate
        {
            get { return GetLocationClimate(); }
        }

        /// <summary>
        /// Gets small sprite font.
        /// </summary>
        public SpriteFont SmallFont
        {
            get { return arialSmallFont; }
        }

        /// <summary>
        /// Gets current mouse ray.
        ///  Client view must call UpdateMouseRay() with matrices
        ///  for this to be current.
        /// </summary>
        public Ray MouseRay
        {
            get { return mouseRay; }
        }

        /// <summary>
        /// Gets model view or NULL if not ready.
        /// </summary>
        public ModelView ModelView
        {
            get { return (isReady) ? (ModelView)viewClients[ViewModes.ModelView] : null; }
        }

        /// <summary>
        /// Gets location view or NULL if not ready.
        /// </summary>
        public LocationView LocationView
        {
            get { return (isReady) ? (LocationView)viewClients[ViewModes.LocationView] : null; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ViewHost()
        {
            // Create view dictionaries
            viewClients = new Dictionary<ViewModes, ViewBase>();
        }

        #endregion

        #region Event Arguments

        public class StatusMessageEventArgs
        {
            public string Message;
        }

        public class ViewModeChangedEventArgs
        {
            public ViewModes ViewMode;
        }

        #endregion

        #region StatusMessageChanged Event

        public delegate void StatusMessageChangedEventHandler(object sender, StatusMessageEventArgs e);
        public event StatusMessageChangedEventHandler StatusMessageChanged;

        protected virtual void RaiseStatusMessageChangedEvent()
        {
            // Do nothing when not created
            if (StatusMessageChanged == null)
                return;

            // Assign arguments
            StatusMessageEventArgs e = new StatusMessageEventArgs();
            e.Message = statusMessage;

            // Raise event
            StatusMessageChanged(this, e);
        }

        #endregion

        #region ViewModeChanged Event

        public delegate void ViewModeChangedEventHandler(object sender, ViewModeChangedEventArgs e);
        public event ViewModeChangedEventHandler ViewModeChanged;

        protected virtual void RaiseViewModeChangedEvent()
        {
            // Do nothing when not created
            if (ViewModeChanged == null)
                return;

            // Assign arguments
            ViewModeChangedEventArgs e = new ViewModeChangedEventArgs();
            e.ViewMode = viewMode;

            // Raise event
            ViewModeChanged(this, e);
        }

        #endregion

        #region Abstract Implementations

        /// <summary>
        /// Initialise the control.
        /// </summary>
        protected override void Initialize()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        /// <summary>
        /// Draw control.
        /// </summary>
        protected override void Draw()
        {
            // Do nothing if not visible or status other than normal
            if (!Visible ||
                GraphicsDevice.GraphicsDeviceStatus != GraphicsDeviceStatus.Normal)
                return;

            // Just clear display if no view mode set
            if (viewMode == ViewModes.None)
            {
                GraphicsDevice.Clear(backgroundColour);
                return;
            }

            // Handle control not ready
            if (!isReady)
            {
                // Keep trying to initialise view
                if (!InitialiseView())
                {
                    // Just clear the display until ready
                    GraphicsDevice.Clear(backgroundColour);
                    return;
                }
            }

            // Draw current view mode
            viewClients[viewMode].Draw();
        }

        #endregion

        #region Events

        /// <summary>
        /// Tick while idle.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">EventArgs.</param>
        void TickWhileIdle(object sender, EventArgs e)
        {
            NativeMethods.Message message;
            while (!NativeMethods.PeekMessage(out message, IntPtr.Zero, 0, 0, 0))
            {
                Tick(sender, e);
            }
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
            if (isReady && viewMode != ViewModes.None)
                viewClients[viewMode].Resize();

            // Invalidate control
            this.Invalidate();
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
            mouseTimeDelta = stopwatch.ElapsedTicks - mouseTime;

            // Ensure mouse time delta is never 0 (avoids potential divide by zero)
            if (mouseTimeDelta == 0)
                mouseTimeDelta = 1;

            // Update position and time
            mousePos = new Point(e.Location.X, e.Location.Y);
            mouseTime = stopwatch.ElapsedTicks;

            // Set mouse velocity
            mouseVelocity.X = (float)mousePosDelta.X / (float)mouseTimeDelta * 10000.0f;
            mouseVelocity.Y = (float)mousePosDelta.Y / (float)mouseTimeDelta * 10000.0f;

            // Move mouse in current view mode
            if (isReady && viewMode != ViewModes.None)
                viewClients[viewMode].OnMouseMove(e);
        }

        /// <summary>
        /// Mouse wheel movement.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            // Send to view
            if (isReady && viewMode != ViewModes.None)
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
                case MouseButtons.Middle:
                    middleMouseDown = true;
                    break;
            }

            // Send to view
            if (isReady && viewMode != ViewModes.None)
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
                case MouseButtons.Middle:
                    middleMouseDown = false;
                    break;
            }

            // Send to view
            if (isReady && viewMode != ViewModes.None)
                viewClients[viewMode].OnMouseUp(e);
        }

        /// <summary>
        /// Mouse button pressed and release.
        /// </summary>
        /// <param name="e">MouseEventArgs.</param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            // Swaps between two views using "back" button on mouse if available
            if (e.Button == MouseButtons.XButton1)
            {
                SwapViewModes();
            }

            // Send to view
            if (isReady && viewMode != ViewModes.None)
                viewClients[viewMode].OnMouseClick(e);
        }

        /// <summary>
        /// Called when user double-clicks mouse.
        /// </summary>
        /// <param name="e">MouseEventArgs.</param>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            // Only handle left mouse double-clicks
            if (e.Button != MouseButtons.Left)
                return;

            // Send to view
            if (isReady && viewMode != ViewModes.None)
                viewClients[viewMode].OnMouseDoubleClick(e);
        }

        /// <summary>
        /// Mouse enters client area.
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            mouseInClientArea = true;
        }

        /// <summary>
        /// Mouse leaves client area.
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            mouseInClientArea = false;
        }

        /// <summary>
        /// Key down.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Swap between view modes on backspace
            if (e.KeyCode == System.Windows.Forms.Keys.Back)
            {
                SwapViewModes();
            }

            // Send to view
            if (isReady && viewMode != ViewModes.None)
                viewClients[viewMode].OnKeyDown(e);
        }

        /// <summary>
        /// Visible state of control has changed.
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            // Start and stop updates based on visible flag
            if (this.Visible)
                update = true;
            else
                update = false;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Allows view to set up managers for content loading. Must be called post control creation.
        /// </summary>
        /// <param name="arena2Path"></param>
        /// <returns></returns>
        public void SetArena2Path(string arena2Path)
        {
            if (this.Created)
            {   
                // Initialise content
                this.arena2Path = arena2Path;
                contentHelper = new ContentHelper(
                    Services,
                    Path.Combine(Application.StartupPath, "Content"));

                // Initialise scene builder
                sceneBuilder = new SceneBuilder(GraphicsDevice, arena2Path);

                // Initialise view
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
        /// Enable or disable updates.
        /// </summary>
        /// <param name="suspend">True to suspend, false to resume.</param>
        public void EnableUpdates(bool enable)
        {
            // Exit if control not ready
            if (!isReady)
                return;

            // Suspend and resume timer
            update = enable;
        }

        #endregion

        #region View Methods

        /// <summary>
        /// Shows thumbnails.
        /// </summary>
        public void ShowThumbnailsView()
        {
            // Exit if not ready
            if (!isReady)
                return;

            // Set view mode
            lastViewMode = viewMode;
            viewMode = ViewModes.ThumbnailView;
            viewClients[viewMode].ResumeView();
            RaiseViewModeChangedEvent();
        }

        /// <summary>
        /// Shows a single model.
        /// </summary>
        /// <param name="id">ModelID of model to show.</param>
        /// <param name="climate">ClimateType.</param>
        public void ShowModelView(uint? id, DFLocation.ClimateSettings? climate)
        {
            // Exit if not ready
            if (!isReady)
                return;

            // Set view mode
            lastViewMode = viewMode;
            viewMode = ViewModes.ModelView;
            ModelView view = (ModelView)viewClients[ViewModes.ModelView];
            view.Climate = climate;
            view.ModelID = id;
            view.ResumeView();
            RaiseViewModeChangedEvent();
        }

        /// <summary>
        /// Shows a single block view.
        /// </summary>
        /// <param name="name">Block name.</param>
        /// <param name="climate">Climate of block.</param>
        public void ShowBlockView(string name, DFLocation.ClimateSettings? climate)
        {
            // Exit if not ready
            if (!isReady)
                return;

            // Show block scene
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToUpper();
                LocationView view = (LocationView)viewClients[ViewModes.BlockView];
                view.CreateBlockScene(name, climate);
            }

            // Set view mode
            lastViewMode = viewMode;
            viewMode = ViewModes.BlockView;
            viewClients[viewMode].ResumeView();
            RaiseViewModeChangedEvent();
        }

        /// <summary>
        /// Shows location exterior for specified location.
        /// </summary>
        /// <param name="dfLocation">DFLocation.</param>
        public void ShowLocationExterior(DFLocation dfLocation)
        {
            // Exit if not ready
            if (!isReady)
                return;

            // Show exterior scene
            LocationView view = (LocationView)viewClients[ViewModes.LocationView];
            view.CreateExteriorLocationScene(dfLocation.RegionName, dfLocation.Name);

            // Set view mode
            lastViewMode = viewMode;
            viewMode = ViewModes.LocationView;
            viewClients[viewMode].ResumeView();
            RaiseViewModeChangedEvent();
        }

        /// <summary>
        /// Shows location dungeon blocks.
        /// </summary>
        /// <param name="dfLocation">DFLocation.</param>
        public void ShowLocationDungeon(DFLocation dfLocation)
        {
            // Exit if not ready
            if (!isReady)
                return;

            // Load dungeon
            LocationView view = (LocationView)viewClients[ViewModes.DungeonView];
            view.CreateDungeonLocationScene(dfLocation.RegionName, dfLocation.Name);

            // Set view mode
            lastViewMode = viewMode;
            viewMode = ViewModes.DungeonView;
            viewClients[viewMode].ResumeView();
            RaiseViewModeChangedEvent();
        }

        /// <summary>
        /// Moves active camera to origin of specified block.
        /// </summary>
        public void MoveToBlock(int x, int z)
        {
            // Exit if not ready
            if (!isReady)
                return;

            // Move to block
            if (viewMode == ViewModes.LocationView)
            {
                LocationView view = (LocationView)viewClients[ViewModes.LocationView];
                view.MoveToBlock(x, z);
            }
            else if (viewMode == ViewModes.DungeonView)
            {
                LocationView view = (LocationView)viewClients[ViewModes.DungeonView];
                view.MoveToBlock(x, z);
            }
        }

        /// <summary>
        /// Requests each view to reset, release resources, destroy layouts, etc.
        /// </summary>
        public void ResetViews()
        {
            if (!isReady)
                return;

            viewClients[ViewModes.ThumbnailView].ResetView();
            viewClients[ViewModes.ModelView].ResetView();
            viewClients[ViewModes.BlockView].ResetView();
            viewClients[ViewModes.LocationView].ResetView();
            viewClients[ViewModes.DungeonView].ResetView();
        }

        #endregion

        #region Mouse Picking Methods

        /// <summary>
        /// Update mouse ray for picking.
        /// </summary>
        /// <param name="x">Mouse X in viewport.</param>
        /// <param name="y">Mouse Y in viewport.</param>
        /// <param name="view">View matrix.</param>
        /// <param name="projection">Projection matrix.</param>
        public void UpdateMouseRay(int x, int y, Matrix view, Matrix projection)
        {
            // Unproject vectors into view area
            Viewport vp = this.GraphicsDevice.Viewport;
            Vector3 near = vp.Unproject(new Vector3(x, y, 0), projection, view, Matrix.Identity);
            Vector3 far = vp.Unproject(new Vector3(x, y, 1), projection, view, Matrix.Identity);

            // Create ray
            Vector3 direction = far - near;
            direction.Normalize();
            mouseRay.Position = near;
            mouseRay.Direction = direction;
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

            // Hook idle event to run as fast as possible
            Application.Idle += TickWhileIdle;
            update = true;

            // Bind view clients
            BindViewClient(ViewModes.ThumbnailView, new ThumbnailView(this));
            BindViewClient(ViewModes.ModelView, new ModelView(this));
            BindViewClient(ViewModes.BlockView, new LocationView(this));
            BindViewClient(ViewModes.LocationView, new LocationView(this));
            BindViewClient(ViewModes.DungeonView, new LocationView(this));

            // Load fonts
            arialSmallFont = contentHelper.ContentManager.Load<SpriteFont>(@"Fonts\ArialSmall");

            // Set ready flag
            isReady = true;

            return true;
        }

        /// <summary>
        /// Tick to update views.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        private void Tick(object sender, EventArgs e)
        {
            // Measure elapsed time
            TimeSpan currentTime = stopwatch.Elapsed;
            elapsedTime = currentTime - lastTime;

            // This is to work around a reported bug with ATI 58xx cards
            // (and possibly others) where GraphicsDevice.Present will
            // fail to wait for vertical sync every other frame. This results
            // in alternating short-long frame times and very stuttery movement.
            // The following attempts to ensure a minimum of 60 fps which
            // smooths out movement on these short frames.
            // The bug has been reproduced and shown to be improved with
            // this workaround.
            while (elapsedTime.Milliseconds < 16)
            {
                currentTime = stopwatch.Elapsed;
                elapsedTime = currentTime - lastTime;
            }

            // Store last time
            lastTime = currentTime;

            // Tick current view
            if (isReady && viewMode != ViewModes.None)
                viewClients[viewMode].Update();

            // Redraw
            CustomRefresh();

            // Calculate frames per second
            frameCount++;
            timeSinceLastFPSUpdate += elapsedTime.Milliseconds;
            if (timeSinceLastFPSUpdate > 1000)
            {
                fps = frameCount;
                frameCount = 0;
                timeSinceLastFPSUpdate -= 1000;
            }

#if DEBUG
            // Calculate garbage collections per second
            if (!gcTracker.IsAlive)
            {
                garbageCount++;
                gcTracker = new WeakReference(new object());
            }
#endif
        }

        /// <summary>
        /// Redraw form.
        /// </summary>
        public void CustomRefresh()
        {
            // Draw form directly rather than hand to event queue
            string beginDrawError = BeginDraw();
            if (string.IsNullOrEmpty(beginDrawError))
            {
                Draw();
                EndDraw();
            }
        }

        /// <summary>
        /// Bind a view to the specified view mode.
        /// </summary>
        /// <param name="mode">Mode to bind with view.</param>
        /// <param name="viewClient">ContentViewBase implementation.</param>
        private void BindViewClient(ViewModes viewMode, ViewBase viewClient)
        {
            // Can only bind one view client to a mode
            if (viewClients.ContainsKey(viewMode))
                return;

            // Add view to dictionary
            viewClients.Add(viewMode, viewClient);

            // Initialise view
            viewClient.Initialize();
        }

        /// <summary>
        /// Set filtered model array and notify views to update.
        /// </summary>
        /// <param name="array"></param>
        private void AssignFilteredModels(uint[] array)
        {
            // Do nothing if not initialised
            if (!isReady)
                return;

            // Do nothing if going from null to null
            if (filteredModelsArray == null && array == null)
                return;

            // Assign new filter array to views.
            // Views have been designed not to perform any visible operation
            // when updating this list so they can all be notified at the same time.
            filteredModelsArray = array;
            viewClients[ViewModes.ThumbnailView].FilteredModelsChanged();
            viewClients[ViewModes.ModelView].FilteredModelsChanged();
            viewClients[ViewModes.BlockView].FilteredModelsChanged();
            viewClients[ViewModes.LocationView].FilteredModelsChanged();
            viewClients[ViewModes.DungeonView].FilteredModelsChanged();
        }

        /// <summary>
        /// Set status message.
        /// </summary>
        /// <param name="statusMessage"></param>
        private void SetStatusMessage(string statusMessage)
        {
            this.statusMessage = statusMessage;
            RaiseStatusMessageChangedEvent();
        }

        /// <summary>
        /// Swaps between current and previous view modes.
        /// </summary>
        private void SwapViewModes()
        {
            // Do nothing if no previous view mode
            if (lastViewMode == ViewModes.None)
                return;

            // Swap current view mode with previous view mode
            ViewModes tempViewMode;
            tempViewMode = viewMode;
            viewMode = lastViewMode;
            lastViewMode = tempViewMode;
            if (isReady && viewMode != ViewModes.None)
                viewClients[viewMode].ResumeView();
            RaiseViewModeChangedEvent();
        }

        /// <summary>
        /// Gets climate of current location.
        /// </summary>
        /// <returns>Location climate, or None if no location loaded.</returns>
        private DFLocation.ClimateSettings? GetLocationClimate()
        {
            if (!isReady)
                return null;

            return viewClients[ViewModes.LocationView].Climate;
        }

        #endregion

    }

    #region Native Methods

    /// <summary>
    /// Native methods.
    /// </summary>
    static class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Message
        {
            public IntPtr hWnd;
            public uint Msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint Time;
            public System.Drawing.Point Point;
        }

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PeekMessage(out Message message, IntPtr hWnd, uint filterMin, uint filterMax, uint flags);
    }

    #endregion

}
