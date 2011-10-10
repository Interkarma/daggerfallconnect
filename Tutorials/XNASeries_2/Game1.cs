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
using DaggerfallConnect.Arena2;
using XNALibrary;

namespace XNASeries_2
{
    /// <summary>
    /// This tutorial shows how to use the Daggerfall Connect XNALibrary
    ///  to load RMB and RDB blocks into a new scene.
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // XNA
        GraphicsDeviceManager graphics;

        // XNALibrary
        Core core;

        // Daggerfall path
        string arena2Path = @"C:\dosgames\dagger\arena2";

        // Content
        bool showDungeonBlock = false;
        string rmbBlockName = "MAGEAA13.RMB";           // Exterior block
        string rdbBlockName = "S0000040.RDB";           // Dungeon block

        // Camera
        Vector3 rmbCameraPos = new Vector3(2048, 400, 1000);
        Vector3 rdbCameraPos = new Vector3(1024, 512, 1000);

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
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
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
            // Set scene options
            core.Renderer.BackgroundColor = Color.Black;
            core.TextureManager.Daytime = false;

            // Create a block node
            BlockNode node;
            if (showDungeonBlock)
            {
                node = core.SceneBuilder.CreateBlockNode(rdbBlockName, null, false);
                core.Camera.Position = rdbCameraPos;
            }
            else
            {
                node = core.SceneBuilder.CreateBlockNode(rmbBlockName, null, false);
                core.Camera.Position = rmbCameraPos;
            }

            // Add to scene
            core.Scene.AddNode(null, node);
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
            core.Scene.Update(gameTime.ElapsedGameTime);

            // Update input
            core.Input.Update(gameTime.ElapsedGameTime);
            core.Input.Apply(core.Camera, true);

            // Update camera
            core.Camera.Update();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Draw scene
            core.Draw();

            // Draw bounds
            if (drawBounds)
                core.Renderer.DrawBounds();
        }
    }
}
