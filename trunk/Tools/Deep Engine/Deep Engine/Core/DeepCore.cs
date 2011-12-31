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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
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

        // XNA
        IServiceProvider serviceProvider;
        GraphicsDevice graphicsDevice;
        ContentManager contentManager;
        SpriteBatch spriteBatch;

        // Engine
        Renderer renderer;

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

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="arena2Folder">Path to Arena2 folder.</param>
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
            deprecatedInput = new Deprecated.Input();
            deprecatedInput.ActiveDevices = Deprecated.Input.DeviceFlags.All;

            // Create empty scene
            scene = new Scene(this);

            // Initialise engine objects
            renderer.Initialize();
        }

        /// <summary>
        /// Called when core should load content.
        /// </summary>
        public void LoadContent()
        {
            // Get GraphicsDevice
            IGraphicsDeviceService graphicsDeviceService =
                (IGraphicsDeviceService)serviceProvider.GetService(typeof(IGraphicsDeviceService));
            this.graphicsDevice = graphicsDeviceService.GraphicsDevice;

            // Create sprite batch for rendering console and other overlays
            spriteBatch = new SpriteBatch(graphicsDevice);

            // Create Daggerfall managers
            this.materialManager = new MaterialManager(graphicsDevice, arena2Path);
            this.modelManager = new ModelManager(graphicsDevice, arena2Path);

            // Load engine objects content
            renderer.LoadContent();
        }

        /// <summary>
        /// Called when core should update.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Update scene
            scene.Update(gameTime);

            // Update renderer
            renderer.Update();

            // Update input
            deprecatedInput.Update(gameTime.ElapsedGameTime);
            deprecatedInput.Apply(scene.DeprecatedCamera, true);
        }

        /// <summary>
        /// Called when core should draw.
        /// </summary>
        public void Draw()
        {
            renderer.Draw(scene);
        }

        #endregion

    }

}
