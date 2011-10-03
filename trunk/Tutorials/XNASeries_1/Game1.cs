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

        // XNALibrary
        SceneBuilder sceneBuilder;
        Renderer renderer;
        Input input;

        // Daggerfall path
        string arena2Path = @"C:\dosgames\dagger\arena2";

        // Camera
        Vector3 cameraPos = new Vector3(0, 300, 4000);

        // Options
        bool invertMouseLook = false;
        bool invertGamePadLook = true;
        bool drawBounds = true;

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
            renderer.Camera.Position = cameraPos;

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
            // Add a model node
            ModelNode node1 = sceneBuilder.CreateModelNode(456);
            node1.Position = new Vector3(1000f, 0f, 0f);
            renderer.Scene.AddNode(null, node1);

            // Add a second model node
            ModelNode node2 = sceneBuilder.CreateModelNode(343);
            node2.Position = new Vector3(-1000f, 0f, 0f);
            renderer.Scene.AddNode(null, node2);
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