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
        SceneBuilder sceneBuilder;
        Renderer renderer;
        Input input;

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
            // Create scene builder
            sceneBuilder = new SceneBuilder(GraphicsDevice, arena2Path);

            // Create renderer
            renderer = new Renderer(sceneBuilder.TextureManager);

            // Create input
            input = new Input();
            input.ActiveDevices = Input.DeviceFlags.All;
            input.InvertMouseLook = invertMouseLook;
            input.InvertGamePadLook = invertGamePadLook;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a block node
            BlockNode node;
            if (showDungeonBlock)
            {
                node = sceneBuilder.CreateBlockNode(rdbBlockName, null, false);
                renderer.Camera.Position = rdbCameraPos;
            }
            else
            {
                node = sceneBuilder.CreateBlockNode(rmbBlockName, null, false);
                renderer.Camera.Position = rmbCameraPos;
            }

            // Add to scene
            renderer.Scene.AddNode(null, node);
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
            renderer.Scene.Update(gameTime.ElapsedGameTime);

            // Update input
            input.Update(gameTime.ElapsedGameTime);
            input.Apply(renderer.Camera, true);

            // Update camera
            renderer.Camera.Update();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Draw scene
            renderer.Draw();

            // Draw bounds
            if (drawBounds)
                renderer.DrawBounds();
        }
    }
}
