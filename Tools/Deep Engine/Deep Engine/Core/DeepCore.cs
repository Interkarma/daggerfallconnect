// Project:         Deep Engine
// Description:     3D game engine for Ruins of Hill Deep and Daggerfall Workshop projects.
// Copyright:       Copyright (C) 2012 Gavin Clayton
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;
using DeepEngine.Daggerfall;
using DeepEngine.Rendering;
using DeepEngine.World;
#endregion

namespace DeepEngine.Core
{

    /// <summary>
    /// Core engine implementation.
    /// </summary>
    public class DeepCore
    {

        #region Fields

        // Constant strings
        const string contentRootDirectory = "Deep Engine Content";

        // Daggerfall
        string arena2Path;
        MaterialManager materialManager;
        ModelManager modelManager;
        BlocksFile blockManager;
        MapsFile mapManager;

        // XNA
        IServiceProvider serviceProvider;
        GraphicsDevice graphicsDevice;
        ContentManager contentManager;
        SpriteBatch spriteBatch;

        // Engine
        Renderer renderer;

        // Engine statistics
        Stopwatch stopwatch = Stopwatch.StartNew();
        long lastUpdateTime = 0;
        long lastDrawTime = 0;

        // Temporary
        Scene scene;
        Deprecated.Input deprecatedInput;

        #endregion

        #region Properties

        /// <summary>
        /// Gets Arena2 path.
        /// </summary>
        public string Arena2Path
        {
            get { return arena2Path; }
        }

        /// <summary>
        /// Gets IServiceProvider.
        /// </summary>
        public IServiceProvider Services
        {
            get { return serviceProvider; }
        }

        /// <summary>
        /// Gets GraphicsDevice.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get { return graphicsDevice; }
        }

        /// <summary>
        /// Gets SpriteBatch.
        /// </summary>
        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
        }

        /// <summary>
        /// Gets ContentManager.
        /// </summary>
        public ContentManager ContentManager
        {
            get { return contentManager; }
        }

        /// <summary>
        /// Gets Daggerfall material manager.
        /// </summary>
        public MaterialManager MaterialManager
        {
            get { return materialManager; }
        }

        /// <summary>
        /// Gets Daggerfall model manager.
        /// </summary>
        public ModelManager ModelManager
        {
            get { return modelManager; }
        }

        /// <summary>
        /// Gets Daggerfall block manager.
        /// </summary>
        public BlocksFile BlockManager
        {
            get { return blockManager; }
        }

        /// <summary>
        /// Gets Daggerfall map manager.
        /// </summary>
        public MapsFile MapManager
        {
            get { return mapManager; }
        }

        /// <summary>
        /// Gets or sets active scene.
        /// </summary>
        public Scene ActiveScene
        {
            get { return scene; }
            set { scene = value; }
        }

        /// <summary>
        /// Gets deferred renderer.
        /// </summary>
        public Renderer Renderer
        {
            get { return renderer; }
        }

        /// <summary>
        /// Gets last update time in milliseconds.
        /// </summary>
        public long LastUpdateTime
        {
            get { return lastUpdateTime; }
        }

        /// <summary>
        /// Gets last draw time in milliseconds.
        /// </summary>
        public long LastDrawTime
        {
            get { return lastDrawTime; }
        }

        /// <summary>
        /// Gets number of visible lights submitted to renderer.
        /// </summary>
        public int VisibleLightsCount
        {
            get { return renderer.VisibleLightsCount; }
        }

        /// <summary>
        /// Gets number of visible billboards submitted to renderer.
        /// </summary>
        public int VisibleBillboardsCount
        {
            get { return renderer.VisibleBillboardsCount; }
        }

        /// <summary>
        /// Gets the number of milliseconds elapsed since core started.
        /// </summary>
        public long ElapsedMilliseconds
        {
            get { return stopwatch.ElapsedMilliseconds; }
        }

        /// <summary>
        /// Gets engine stopwatch.
        /// </summary>
        public Stopwatch Stopwatch
        {
            get { return stopwatch; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="arena2Path">Path to Arena2 folder.</param>
        /// <param name="renderTargetSize">Size of internal render target.</param>
        /// <param name="serviceProvider">IServiceProvider.</param>
        public DeepCore(string arena2Path, Vector2 renderTargetSize, IServiceProvider serviceProvider)
        {
            // Store values
            this.arena2Path = arena2Path;
            this.serviceProvider = serviceProvider;

            // Create engine objects
            this.renderer = new Renderer(this, renderTargetSize);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Called to initialise core.
        /// </summary>
        public void Initialize()
        {
            // Create content manager
            contentManager = new ContentManager(serviceProvider, contentRootDirectory);

            // Create input
            deprecatedInput = new Deprecated.Input();
            deprecatedInput.ActiveDevices = Deprecated.Input.DeviceFlags.All;

            // Create empty scene
            scene = new Scene(this);

            // Initialise engine objects
            renderer.Initialize();

            // Get GraphicsDevice
            IGraphicsDeviceService graphicsDeviceService =
                (IGraphicsDeviceService)serviceProvider.GetService(typeof(IGraphicsDeviceService));
            this.graphicsDevice = graphicsDeviceService.GraphicsDevice;

            // Create sprite batch for rendering console and other overlays
            spriteBatch = new SpriteBatch(graphicsDevice);

            // Create Daggerfall managers.
            // MaterialManager must be created before ModelManager due to dependencies.
            this.materialManager = new MaterialManager(this);
            this.modelManager = new ModelManager(this);
            this.blockManager = new BlocksFile(Path.Combine(arena2Path, BlocksFile.Filename), FileUsage.UseDisk, true);
            this.mapManager = new MapsFile(Path.Combine(arena2Path, MapsFile.Filename), FileUsage.UseDisk, true);

            // Load engine objects content
            renderer.LoadContent();
        }

        /// <summary>
        /// Called when core should update.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Start timer
            long startTime = stopwatch.ElapsedMilliseconds;

            // Update scene
            scene.Update(gameTime);

            // Update renderer
            renderer.Update();

            // Update input
            deprecatedInput.Update(gameTime.ElapsedGameTime);
            deprecatedInput.Apply(scene.DeprecatedCamera, true);

            // Get time
            lastUpdateTime = stopwatch.ElapsedMilliseconds - startTime;
        }

        /// <summary>
        /// Called when core should draw.
        /// </summary>
        /// <param name="drawDebugBuffers">Flag to draw debug buffers.</param>
        public void Draw()
        {
            // Start timer
            long startTime = stopwatch.ElapsedMilliseconds;

            // Draw scene
            renderer.Draw(scene);

            // Get time
            lastDrawTime = stopwatch.ElapsedMilliseconds - startTime;
        }

        #endregion

    }

}
