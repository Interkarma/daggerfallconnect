﻿// Project:         XNALibrary
// Description:     Simple XNA game library for DaggerfallConnect.
// Copyright:       Copyright (C) 2011 Gavin Clayton
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
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace XNALibrary
{

    /// <summary>
    /// Engine core.
    /// </summary>
    public class Core
    {
        #region Class Variables

        // Daggerfall
        private string arena2Path;

        // XNA
        IServiceProvider serviceProvider;
        GraphicsDevice graphicsDevice;
        ContentManager contentManager;

        // XNALibrary
        bool deferredRendering = false;
        SceneBuilder sceneBuilder;
        DefaultRenderer defaultRenderer;
        DeferredRenderer deferredRenderer;
        Collision collision;
        Gravity gravity;
        Input input;
        Scene scene;
        Camera camera;

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
        /// Gets TextureManager.
        /// </summary>
        public TextureManager TextureManager
        {
            get { return sceneBuilder.TextureManager; }
        }

        /// <summary>
        /// Gets ModelManager.
        /// </summary>
        public ModelManager ModelManager
        {
            get { return sceneBuilder.ModelManager; }
        }

        /// <summary>
        /// Gets SceneBuilder.
        /// </summary>
        public SceneBuilder SceneBuilder
        {
            get { return sceneBuilder; }
        }

        /// <summary>
        /// Gets Renderer.
        /// </summary>
        public DefaultRenderer Renderer
        {
            get { return (deferredRendering) ? deferredRenderer : defaultRenderer; }
        }

        /// <summary>
        /// Gets input.
        /// </summary>
        public Input Input
        {
            get { return input; }
        }

        /// <summary>
        /// Gets or sets Camera.
        /// </summary>
        public Camera Camera
        {
            get { return camera; }
            set { camera = value; }
        }

        /// <summary>
        /// Gets or sets Scene.
        /// </summary>
        public Scene Scene
        {
            get { return scene; }
            set { scene = value; }
        }

        /// <summary>
        /// Gets or sets flag to use deferred renderer.
        /// </summary>
        public bool DeferredRendering
        {
            get { return deferredRendering; }
            set { deferredRendering = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="arena2Folder">Path to Arena2 folder.</param>
        /// <param name="serviceProvider">IServiceProvider.</param>
        public Core(string arena2Path, IServiceProvider serviceProvider)
        {
            // Store values
            this.arena2Path = arena2Path;
            this.serviceProvider = serviceProvider;

            // Get graphics
            IGraphicsDeviceService graphicsDeviceService =
                (IGraphicsDeviceService)serviceProvider.GetService(typeof(IGraphicsDeviceService));
            this.graphicsDevice = graphicsDeviceService.GraphicsDevice;

            // Create content manager
            contentManager = new ContentManager(serviceProvider, "Content");

            // Create engine components
            sceneBuilder = new SceneBuilder(graphicsDevice, arena2Path);
            collision = new Collision();
            gravity = new Gravity();
            scene = new Scene();
            camera = new Camera();

            // Create renderers
            defaultRenderer = new DefaultRenderer(TextureManager);
            deferredRenderer = new DeferredRenderer(TextureManager);
            Resize();

            // Create input
            input = new Input();
            input.ActiveDevices = Input.DeviceFlags.All;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates engine.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time.</param>
        public void Update(TimeSpan elapsedTime)
        {
            // Update scene
            scene.Update(elapsedTime);

            // Update input
            input.Update(elapsedTime);
            input.Apply(camera, true);

            // Update camera
            camera.Update();
        }

        /// <summary>
        /// Draws scene using camera.
        /// </summary>
        public void Draw()
        {
            // Draw
            DefaultRenderer renderer = this.Renderer;
            renderer.Camera = camera;
            renderer.Scene = scene;
            renderer.Draw();
        }

        /// <summary>
        /// Called when viewport size has changed.
        /// </summary>
        public void Resize()
        {
            // Update default renderer
            defaultRenderer.Camera = camera;
            defaultRenderer.UpdateCameraAspectRatio(-1, -1);

            // Update deferred renderer
            deferredRenderer.Camera = camera;
            deferredRenderer.UpdateCameraAspectRatio(-1, -1);
        }

        #endregion
    }

}