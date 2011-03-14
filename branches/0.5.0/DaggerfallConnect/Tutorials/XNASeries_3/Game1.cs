#region Imports

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using XNALibrary;

#endregion

namespace XNASeries_3
{
    /// <summary>
    /// This tutorial covers loading and drawing RDB (dungeon) blocks using XNAHelper.
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        string MyArena2Path = "C:\\dosgames\\DAGGER\\ARENA2";
        MapManager mapManager;
        string mapKey;

        FPS fps;
        InputHandler input;
        Camera camera;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        BasicEffect effect;
        VertexDeclaration vertexDeclaration;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            //fps = new FPS(this);
            //Components.Add(fps);

            input = new InputHandler(this);
            Components.Add(input);

            camera = new Camera(this);
            Components.Add(camera);

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Set window title
            Window.Title = "Tutorial_XNASeries_3";

            // Create MapManager
            mapManager = new MapManager(GraphicsDevice, MyArena2Path);

            // Create vertex declaration
            vertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionNormalTexture.VertexElements);

            // Setup basic effect
            effect = new BasicEffect(GraphicsDevice, null);
            effect.World = Matrix.Identity;
            effect.TextureEnabled = true;
            effect.PreferPerPixelLighting = true;
            effect.EnableDefaultLighting();
            effect.SpecularColor = new Vector3(0.2f, 0.2f, 0.2f);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load location
            mapKey = mapManager.LoadMap("Daggerfall", "Privateer's Hold");
            //mapKey = mapManager.LoadMap("High Rock sea coast", "Mantellan Crux");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Clear device
            GraphicsDevice.Clear(Color.Black);

            // Set vertex declaration
            GraphicsDevice.VertexDeclaration = vertexDeclaration;

            // Set view and projection matrices
            effect.View = camera.View;
            effect.Projection = camera.Projection;

            // Draw the block
            Matrix matrix = Matrix.Identity;
            mapManager.DrawMapDungeon(mapKey, ref effect, ref matrix);

            base.Draw(gameTime);
        }
    }
}
