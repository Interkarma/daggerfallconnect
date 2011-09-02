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

namespace XNASeries_2
{
    /// <summary>
    /// This tutorial shows how to use the Daggerfall Connect XNALibrary
    ///  to load an exterior Daggerfall block into a scene with variable
    ///  climate and weather settings.
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // XNA
        GraphicsDeviceManager graphics;

        // XNALibrary
        Renderer renderer;
        Input input;

        // Daggerfall path
        string arena2Path = @"C:\dosgames\dagger\arena2";

        // Content (Only uncomment one of these)
        string blockName = "MAGEAA13.RMB";          // Exterior block
        //string blockName = "S0000100.RDB";        // Dungeon block

        // Camera
        Vector3 cameraPos = new Vector3(2048, 400, 1000);

        // Options
        bool drawBounds = true;
        bool invertMouseLook = false;
        bool invertControllerLook = true;

        // Climate
        DFLocation.ClimateType climate = DFLocation.ClimateType.Temperate;
        DFLocation.ClimateWeather weather = DFLocation.ClimateWeather.Normal;

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
            // Create renderer
            renderer = new Renderer(GraphicsDevice);
            renderer.Scene.TextureManager = new TextureManager(GraphicsDevice, arena2Path);
            renderer.Scene.ModelManager = new ModelManager(GraphicsDevice, arena2Path);
            renderer.Scene.BlockManager = new BlockManager(GraphicsDevice, arena2Path);
            renderer.Camera.Position = cameraPos;

            // Create input
            input = new Input();
            input.ActiveDevices = Input.DeviceFlags.All;
            input.InvertMouseLook = invertMouseLook;
            input.InvertControllerLook = invertControllerLook;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Set root node to draw bounds
            renderer.Scene.Root.DrawBounds = drawBounds;
            renderer.Scene.Root.DrawBoundsColor = Color.Red;

            // Add a block node
            renderer.Scene.AddBlockNode(blockName);
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
            // Update input
            input.Update(gameTime.ElapsedGameTime);
            input.Apply(renderer.Camera);

            // Update scene
            renderer.Scene.Update(gameTime.ElapsedGameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Draw scene
            renderer.Draw();
            if (drawBounds)
                renderer.DrawBounds();
        }
    }
}
