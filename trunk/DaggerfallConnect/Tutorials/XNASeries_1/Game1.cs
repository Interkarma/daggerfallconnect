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
    /// This tutorial shows how to use the Daggerfall Connect XNALibrary
    ///  to load Daggerfall models into a scene and render bounding volumes.
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

        // Camera
        Vector3 cameraPos = new Vector3(0, 300, 4000);

        // Options
        bool drawBounds = true;
        bool invertMouseLook = false;
        bool invertControllerLook = true;

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
            sceneManager.Camera.NextPosition = cameraPos;
            sceneManager.Camera.ApplyChanges();

            // Set camera updating
            sceneManager.CameraInputFlags =
                Camera.InputFlags.Keyboard |
                Camera.InputFlags.Mouse |
                Camera.InputFlags.Controller;

            // Set camera look preferences
            sceneManager.Camera.InvertMouseLook = invertMouseLook;
            sceneManager.Camera.InvertControllerLook = invertControllerLook;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Set root node to draw bounds
            sceneManager.Root.Matrix = Matrix.CreateTranslation(0f, 0f, 0f);
            sceneManager.Root.DrawBounds = drawBounds;
            sceneManager.Root.DrawBoundsColor = Color.Red;

            // Add a model node
            ModelNode node1 = sceneManager.AddModelNode(null, 456);
            node1.Matrix = Matrix.CreateTranslation(1000f, 0f, 0f);
            node1.DrawBounds = drawBounds;

            // Add a second model node
            ModelNode node2 = sceneManager.AddModelNode(null, 343);
            node2.Matrix = Matrix.CreateTranslation(-1000f, 0f, 0f);
            node2.DrawBounds = drawBounds;
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
            sceneManager.Draw();
            if (drawBounds)
                sceneManager.DrawBounds();
        }
    }
}
