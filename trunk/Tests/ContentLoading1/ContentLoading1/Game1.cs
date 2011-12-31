using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using DeepEngine;
using DeepEngine.World;
using DeepEngine.Components;

namespace ContentLoading1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GameCore core;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        const string arena2Path = @"c:\dosgames\dagger\arena2";

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            this.IsMouseVisible = true;

            // Create and register engine core
            core = new GameCore(arena2Path, this);
            this.Components.Add(core);
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

            // Create entity
            BaseEntity entity = new BaseEntity(core.DeepCore.ActiveScene);
            entity.Matrix = Matrix.CreateTranslation(-555, -1024, 0);

            // Attach drawable component
            GeometricPrimitiveComponent primitiveComponent = new GeometricPrimitiveComponent(entity);
            primitiveComponent.MakeCube(1024f);
            primitiveComponent.Color = Color.White;

            // Attach components
            PhysicsComponent physicsComponent = new PhysicsComponent(entity, 1024f, 1024f, 1024f);
            LightComponent light = new LightComponent(entity, Vector3.Right, Color.White, 0.5f);

            // Create second entity
            BaseEntity entity2 = new BaseEntity(core.DeepCore.ActiveScene);
            GeometricPrimitiveComponent primitiveComponent2 = new GeometricPrimitiveComponent(entity2);
            primitiveComponent2.MakeTorus(64f, 64f, 16);
            primitiveComponent2.Color = Color.White;
            PhysicsComponent physicsComponent2 = new PhysicsComponent(entity2, 128f, 64f, 128f, 1f);
            LightComponent light2 = new LightComponent(entity2, Vector3.Zero, 512f, Color.Green, 2f);
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

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
