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
using System.Diagnostics;
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
    /// Serves different views to the user.
    /// </summary>
    public class ViewHost : WinFormsGraphicsDevice.GraphicsDeviceControl
    {

        #region Class Variables

        // Resources
        private bool isReady = false;
        private string arena2Path = string.Empty;

        // Managers
        private TextureManager textureManager;
        private ModelManager modelManager;
        private BlockManager blockManager;

        // Connect objects
        MapsFile mapsFile;

        // Mouse
        private Point mousePos;
        private long mouseTime;
        private Point mousePosDelta;
        private long mouseTimeDelta;
        private bool leftMouseDown = false;
        private bool rightMouseDown = false;
        private Vector2 mouseVelocity;

        // Timing
        private Stopwatch stopwatch = Stopwatch.StartNew();
        private Timer updateTimer = new Timer();
        readonly TimeSpan TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 120);
        readonly TimeSpan MaxElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 10);
        private TimeSpan accumulatedTime;
        private TimeSpan lastTime;

        // XNA
        private SpriteBatch spriteBatch;

        // Appearance
        Color backgroundColour = Color.Gray;

        // Views
        private ViewModes viewMode = ViewModes.ThumbnailView;
        private Dictionary<ViewModes, ViewBase> viewClients;

        // Content
        ContentHelper contentHelper;
        SpriteFont arialSmallFont;

        // Filtered model array consumed by thumbnail and single model views
        int[] filteredModelsArray;

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
            /// <summary>Viewing a location.</summary>
            LocationView,
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
        /// Gets current view mode
        /// </summary>
        public ViewModes ViewMode
        {
            get { return viewMode; }
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
        /// Gets host BlocksManager.
        /// </summary>
        public BlockManager BlockManager
        {
            get { return blockManager; }
        }

        /// <summary>
        /// Gets host MapsFile.
        /// </summary>
        public MapsFile MapsFile
        {
            get { return mapsFile; }
        }

        /// <summary>
        /// Gets host SpriteBatch.
        /// </summary>
        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
        }

        /// <summary>
        /// Gets total elapsed time in milliseconds.
        /// </summary>
        public long Timer
        {
            get { return stopwatch.ElapsedTicks; }
        }

        /// <summary>
        /// Gets accumulated engine time (not clock time) in seconds.
        /// </summary>
        public float TimeDelta
        {
            get { return (float)accumulatedTime.TotalSeconds; }
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
        /// Gets or sets filtered model ID list for views.
        ///  Set to null to use full model database.
        /// </summary>
        public int[] FilteredModelsArray
        {
            get { return filteredModelsArray; }
            set { AssignFilteredModels(value); }
        }

        public SpriteFont SmallFont
        {
            get { return arialSmallFont; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ViewHost()
        {
            // Create view dictionary
            viewClients = new Dictionary<ViewModes, ViewBase>();

            // Create content helper
            contentHelper = new ContentHelper(
                Services,
                Path.Combine(Application.StartupPath, "Content"));
        }

        public ViewHost(string arena2Path)
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
        /// GraphicsDevice reset.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        private void GraphicsDevice_DeviceReset(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Ticks to update views.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            // Measure time
            TimeSpan currentTime = stopwatch.Elapsed;
            TimeSpan elapsedTime = currentTime - lastTime;
            lastTime = currentTime;
            if (elapsedTime > MaxElapsedTime)
                elapsedTime = MaxElapsedTime;

            // Check to see if update needed
            bool updated = false;
            accumulatedTime += elapsedTime;
            while (accumulatedTime >= TargetElapsedTime)
            {
                // Tick current view
                if (isReady && viewMode != ViewModes.None)
                    viewClients[viewMode].Tick();

                // Update control
                Update();
                accumulatedTime -= TargetElapsedTime;
                updated = true;
            }

            // Redraw when updated
            if (updated)
            {
                this.Refresh();
            }
        }

        #endregion

        #region Overrides

        public override void Refresh()
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

            // Ensure mouse time delta is never 0 (possible, and screws with distance/time calcs obviously)
            if (mouseTimeDelta == 0)
                mouseTimeDelta = 1;

            // Update position and time
            mousePos = new Point(e.Location.X, e.Location.Y);
            mouseTime = stopwatch.ElapsedTicks;

            // Set mouse velocity
            mouseVelocity.X = (float)mousePosDelta.X / (float)mouseTimeDelta * 1000000.0f;
            mouseVelocity.Y = (float)mousePosDelta.Y / (float)mouseTimeDelta * 1000000.0f;

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
            }

            // Send to view
            if (isReady && viewMode != ViewModes.None)
                viewClients[viewMode].OnMouseUp(e);
        }

        /// <summary>
        /// Mouse enters client area.
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            // Send to view
            if (isReady && viewMode != ViewModes.None)
                viewClients[viewMode].OnMouseEnter(e);
        }

        /// <summary>
        /// Mouse leaves client area.
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            // Send to view
            if (isReady && viewMode != ViewModes.None)
                viewClients[viewMode].OnMouseLeave(e);
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
                updateTimer.Enabled = true;
            else
                updateTimer.Enabled = false;
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
        /// </summary>
        /// <param name="suspend">True to suspend, false to resume.</param>
        public void EnableAnimTimer(bool enable)
        {
            // Exit if control not ready
            if (!isReady)
                return;

            // Suspend and resume timer
            updateTimer.Enabled = enable;
        }

        #endregion

        #region View Methods

        /// <summary>
        /// Browse all Daggerfall's models as thumbnails.
        /// </summary>
        public void ShowAllThumbnails()
        {
            // Not implemented
        }

        /// <summary>
        /// Shows a list of thumbnails.
        /// </summary>
        public void ShowThumbnailsList()
        {
            // Not implemented
        }

        /// <summary>
        /// Shows a single model.
        /// </summary>
        /// <param name="modelId">Model ID.</param>
        public void ShowModel(int modelId)
        {
            // Not implemented
        }

        /// <summary>
        /// Shows a single block.
        /// </summary>
        /// <param name="name">Block name.</param>
        public void ShowBlock(string name)
        {
            // Not implemented
        }

        /// <summary>
        /// Shows location exterior blocks.
        /// </summary>
        /// <param name="dfLocation">DFLocation.</param>
        public void ShowLocationExterior(DFLocation dfLocation)
        {
            // Set location in view
            LocationView locationView = (LocationView)viewClients[ViewModes.LocationView];
            locationView.SetLocation(ref dfLocation);
        }

        /// <summary>
        /// Shows location dungeon blocks.
        /// </summary>
        /// <param name="dfLocation">DFLocation.</param>
        public void ShowLocationDungeon(DFLocation dfLocation)
        {
            // Set location in view
            LocationView locationView = (LocationView)viewClients[ViewModes.LocationView];
            locationView.SetLocation(ref dfLocation);
        }

        /// <summary>
        /// Centres location view on a specific block.
        /// </summary>
        public void CentreOnBlock()
        {
            // Not implemented
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
                blockManager = new BlockManager(arena2Path);
                mapsFile = new MapsFile(Path.Combine(arena2Path, "MAPS.BSA"), FileUsage.UseDisk, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            // Start update timer with short interval so we update frequently
            updateTimer.Interval = (int)TargetElapsedTime.TotalMilliseconds;
            updateTimer.Enabled = true;
            updateTimer.Tick += new EventHandler(UpdateTimer_Tick);
            updateTimer.Start();

            // Bind views
            BindViewClient(ViewModes.ThumbnailView, new ThumbnailView(this));
            BindViewClient(ViewModes.ModelView, new ModelView(this));
            BindViewClient(ViewModes.LocationView, new LocationView(this));

            // Load fonts
            arialSmallFont = contentHelper.ContentManager.Load<SpriteFont>(@"Fonts\ArialSmall");


            // Set ready flag
            isReady = true;

            return true;
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
        private void AssignFilteredModels(int[] array)
        {
            filteredModelsArray = array;
            if (isReady)
            {
                viewClients[ViewModes.ThumbnailView].FilteredModelsChanged();
                viewClients[ViewModes.ModelView].FilteredModelsChanged();
                viewClients[ViewModes.LocationView].FilteredModelsChanged();
            }
        }

        #endregion

    }

}
