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

namespace XNASeries_2
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        BlockManager blockManager;
        string MyArena2Path = "C:\\dosgames\\DAGGER\\ARENA2";

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        BasicEffect effect;
        VertexDeclaration vertexDeclaration;

        int blockIndex = 835;
        float angle = 0f;

        private FPS fps;
        BlocksFile blocksFile = new BlocksFile();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            fps = new FPS(this);
            Components.Add(fps);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Window.Title = "Tutorial_XNASeries_2";
            blockManager = new BlockManager(GraphicsDevice, MyArena2Path);

            Matrix cameraMatrix = Matrix.CreateLookAt(new Vector3(-4000, 2000, -100), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            Matrix projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)Window.ClientBounds.Width / (float)Window.ClientBounds.Height, 1.0f, 10000.0f);
            vertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionNormalTexture.VertexElements);

            effect = new BasicEffect(GraphicsDevice, null);
            effect.World = Matrix.Identity;
            effect.View = cameraMatrix;
            effect.Projection = projectionMatrix;
            effect.TextureEnabled = true;
            effect.PreferPerPixelLighting = true;
            effect.EnableDefaultLighting();
            effect.SpecularColor = new Vector3(0.2f, 0.2f, 0.2f);

            blocksFile.Load(Path.Combine(MyArena2Path, "BLOCKS.BSA"), FileUsage.UseDisk, true);

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

            // TODO: use this.Content to load your game content here
            blockManager.LoadBlock(blockIndex);
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
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            angle += 0.05f;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);

            GraphicsDevice.VertexDeclaration = vertexDeclaration;

            // Translate block by half X and Z so that it rotates around middle
            Matrix matrix = Matrix.CreateTranslation(new Vector3(2048, 0, 2048));
            matrix *= Matrix.CreateRotationY(MathHelper.ToRadians(angle));
            blockManager.DrawBlock(blockIndex, ref effect, ref matrix);

            base.Draw(gameTime);
        }
  
    }
}
