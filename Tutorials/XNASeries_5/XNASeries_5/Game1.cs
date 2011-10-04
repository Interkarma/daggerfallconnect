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

namespace XNASeries_5
{
    /// <summary>
    /// Currently a deferred renderering testbed.
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // XNA
        GraphicsDeviceManager graphics;

        // XNALibrary
        Core core;

        // Daggerfall path
        string arena2Path = @"C:\dosgames\dagger\arena2";

        // Camera
        Vector3 cameraPos = new Vector3(0, 300, 1500);

        // Options
        bool invertMouseLook = false;
        bool invertGamePadLook = true;
        bool drawBounds = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.ApplyChanges();
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
            // Create engine
            core = new Core(arena2Path, this.Services);

            // Set camera position
            core.Camera.Position = cameraPos;

            // Set input preferences
            core.Input.InvertMouseLook = invertMouseLook;
            core.Input.InvertGamePadLook = invertGamePadLook;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Add a model node
            ModelNode node1 = core.SceneBuilder.CreateModelNode(456);
            node1.Position = new Vector3(0f, 0f, 0f);
            core.Scene.AddNode(null, node1);
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
            // Update scene
            core.Update(gameTime.ElapsedGameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Draw
            core.Draw();

            // Draw bounds
            if (drawBounds)
                core.Renderer.DrawBounds();
        }
    }
}
