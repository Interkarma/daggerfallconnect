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

        // Daggerfall Connect
        string arena2Path = @"C:\dosgames\dagger\arena2";
        TextureManager textureManager;
        ModelManager modelManager;
        BlockManager blockManager;

        // Content
        //string blockName = "MAGEAA13.RMB";          // Exterior block
        string blockName = "S0000100.RDB";        // Dungeon block

        // Scene
        SceneManager sceneManager;

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
            // Create Daggerfall Connect objects
            textureManager = new TextureManager(GraphicsDevice, arena2Path);
            modelManager = new ModelManager(GraphicsDevice, arena2Path);
            blockManager = new BlockManager(GraphicsDevice, arena2Path);

            // Set climate and weather
            textureManager.Climate = climate;
            textureManager.Weather = weather;

            // Create scene manager
            sceneManager = new SceneManager(GraphicsDevice, arena2Path);
            sceneManager.Initialize();
            sceneManager.TextureManager = textureManager;
            sceneManager.ModelManager = modelManager;
            sceneManager.BlockManager = blockManager;

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

            // Add a block node
            sceneManager.AddBlockNode(null, blockName);
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
