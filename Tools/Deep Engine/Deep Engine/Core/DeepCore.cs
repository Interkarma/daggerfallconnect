﻿// Project:         Deep Engine
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
using DeepEngine.Player;
using DeepEngine.Utility;
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
        const string errorMultipleDrawCalls = "Draw() called more than once before present.";

        // Daggerfall
        string arena2Path;
        MaterialManager materialManager;
        ModelManager modelManager;
        BlocksFile blockManager;
        MapsFile mapManager;
        SndFile soundManager;

        // XNA
        IServiceProvider serviceProvider;
        GraphicsDevice graphicsDevice;
        ContentManager contentManager;
        SpriteBatch spriteBatch;

        // Engine
        float deltaTime;
        Renderer renderer;

        // Engine statistics
        Stopwatch stopwatch = Stopwatch.StartNew();
        long lastUpdateTime = 0;
        long lastDrawTime = 0;
        int drawCount = 0;

        // Temporary
        Scene scene;
        Input input;

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
        /// Gets Daggerfall sound effect manager.
        /// </summary>
        public SndFile SoundManager
        {
            get { return soundManager; }
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
        /// Gets deltra time from last update.
        /// </summary>
        public float DeltaTime
        {
            get { return deltaTime; }
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
        /// <param name="arena2Path">Path to Arena2 folder.</param>
        /// <param name="serviceProvider">IServiceProvider.</param>
        public DeepCore(string arena2Path, IServiceProvider serviceProvider)
        {
            // Store values
            this.arena2Path = arena2Path;
            this.serviceProvider = serviceProvider;

            // Create engine objects
            this.renderer = new Renderer(this);
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
            input = new Input();
            input.ActiveDevices = Input.DeviceFlags.All;

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
            this.soundManager = new SndFile(Path.Combine(arena2Path, SndFile.Filename), FileUsage.UseDisk, true);

            // Load engine objects content
            renderer.LoadContent();
        }

        /// <summary>
        /// Called when core should update.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last update.</param>
        public void Update(TimeSpan elapsedTime)
        {
            // Calculate delta time
            deltaTime = (float)elapsedTime.TotalSeconds;

            // Start timer
            long startTime = stopwatch.ElapsedMilliseconds;

            // Update scene
            if (scene != null)
                scene.Update(elapsedTime);

            // Update renderer
            renderer.Update();

            // Update input
            input.Update(elapsedTime);

            // Get time
            lastUpdateTime = stopwatch.ElapsedMilliseconds - startTime;
        }

        /// <summary>
        /// Called when core should draw.
        /// </summary>
        /// <param name="present">Optionally present render target after draw operation.</param>
        public void Draw(bool present)
        {
            // Start timer
            long startTime = stopwatch.ElapsedMilliseconds;

            // Draw scene
            renderer.Draw(scene);

            // Get time
            lastDrawTime = stopwatch.ElapsedMilliseconds - startTime;

            // Increment drawcount
            drawCount++;

            // Present render target over frame buffer
            if (present)
                Present();
        }

        /// <summary>
        /// Called when core should present render.
        ///  Scene will be alpha blended over anything already in place.
        ///  This allows caller to clear, draw sky effects, etc.
        /// </summary>
        public void Present()
        {
            // Throw if draw called multiple times before present
            if (drawCount > 1)
            {
                throw new Exception(errorMultipleDrawCalls);
            }

            // Present render
            renderer.Present();

            // Reset drawcount
            drawCount = 0;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Descibes a single display mode.
        /// </summary>
        public struct DisplayModeDesc
        {
            public int Index;
            public DisplayMode Mode;
            public string ResolutionText;
        }

        /// <summary>
        /// Enumerates supported display modes.
        /// </summary>
        /// <returns>List of supported display modes.</returns>
        public static List<DisplayModeDesc> EnumerateDisplayModes()
        {
            List<DisplayModeDesc> enumeratedDisplayModes = new List<DisplayModeDesc>(10);

            int index = 0;
            foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                // Exclude unsupported surface formats
                if (mode.Format != SurfaceFormat.Color ||
                    mode.Height < 720)
                {
                    continue;
                }

                // Describe mode
                DisplayModeDesc desc = new DisplayModeDesc
                {
                    Mode = mode,
                    ResolutionText = string.Format("{0}x{1}", mode.Width, mode.Height),
                };

                // Add resolution to list
                desc.Index = index++;
                enumeratedDisplayModes.Add(desc);
            }

            return enumeratedDisplayModes;
        }

        #endregion

    }

}
