// Project:         DaggerfallConnect
// Description:     Read data from Daggerfall's file formats.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using XNALibrary;

namespace XNASeries_1
{
    /// <summary>
    /// This tutorial shows how to load a single Daggerfall model into a scene.
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // XNA
        GraphicsDeviceManager graphics;

        // Daggerfall Connect
        string arena2Path = @"C:\dosgames\dagger\arena2";
        TextureManager textureManager;
        ModelManager modelManager;

        // Scene
        SceneManager sceneManager;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Create Daggerfall Connect objects
            textureManager = new TextureManager(GraphicsDevice, arena2Path);
            modelManager = new ModelManager(GraphicsDevice, arena2Path);

            // Create scene manager
            sceneManager = new SceneManager(GraphicsDevice, arena2Path);
            sceneManager.Initialize();
            sceneManager.TextureManager = textureManager;
            sceneManager.ModelManager = modelManager;

            // Set camera position
            Vector3 cameraPos = new Vector3(0, 350, 1500);
            sceneManager.Camera.NextPosition = cameraPos;
            sceneManager.Camera.ApplyChanges();

            // Set camera updating
            sceneManager.CameraUpdateFlags =
                Camera.UpdateFlags.Keyboard |
                Camera.UpdateFlags.Mouse |
                Camera.UpdateFlags.Controller;

            // Set camera look preferences
            sceneManager.Camera.InvertMouseLook = false;
            sceneManager.Camera.InvertControllerLook = true;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create simple scene
            sceneManager.AddModelNode(null, 456);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            sceneManager.Update(gameTime.ElapsedGameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            sceneManager.Draw(gameTime.ElapsedGameTime);
        }
    }
}
