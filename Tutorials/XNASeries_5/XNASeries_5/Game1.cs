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

namespace XNASeries_5
{
    /// <summary>
    /// Currently a deferred rendrering testbed.
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // XNA
        GraphicsDeviceManager graphics;

        // XNALibrary
        SceneBuilder sceneBuilder;
        DeferredRenderer renderer;
        Input input;

        // Daggerfall path
        string arena2Path = @"C:\dosgames\dagger\arena2";

        // Content
        string regionName = "Daggerfall";
        string locationName = "Daggerfall";

        // Camera
        Vector3 cameraPos = new Vector3(22450, 230, -21350);

        // Options
        bool invertMouseLook = false;
        bool invertControllerLook = true;
        bool drawBounds = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1440;
            graphics.PreferredBackBufferHeight = 900;
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
            renderer = new DeferredRenderer(sceneBuilder.TextureManager);
            renderer.Camera.Position = cameraPos;

            // Create input
            input = new Input();
            input.ActiveDevices = Input.DeviceFlags.All;
            input.InvertMouseLook = invertMouseLook;
            input.InvertGamePadLook = invertControllerLook;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create location node
            SceneNode node =
                sceneBuilder.CreateExteriorLocationNode(
                regionName,
                locationName);

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

            // Update pointer ray with mouse position
            Point mousePos = input.MousePos;
            renderer.UpdatePointerRay(mousePos.X, mousePos.Y);

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
