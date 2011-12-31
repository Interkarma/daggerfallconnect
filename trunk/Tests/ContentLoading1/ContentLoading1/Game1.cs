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

            LoadModelScene();
            //LoadPhysicsScene();
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
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        #region Content Loading Methods

        /// <summary>
        /// Loads a simple Daggerfall model scene.
        /// </summary>
        private void LoadModelScene()
        {
            // Create directional light
            BaseEntity lightEntity = new BaseEntity(core.ActiveScene);
            new LightComponent(lightEntity, Vector3.Right, Color.White, 1.0f);

            // Create model entity
            BaseEntity modelEntity = new BaseEntity(core.ActiveScene);
            modelEntity.Matrix = Matrix.CreateTranslation(0, -400, 0);
            new NativeModelComponent(modelEntity, 456);
        }

        /// <summary>
        /// Loads a simple physics test scene.
        /// </summary>
        private void LoadPhysicsScene()
        {
            // Create cube entity
            BaseEntity cubeEntity = new BaseEntity(core.ActiveScene);
            cubeEntity.Matrix = Matrix.CreateTranslation(-555, -1024, 0);

            // Attach cube primitive component
            GeometricPrimitiveComponent cubeGeometry = new GeometricPrimitiveComponent(cubeEntity);
            cubeGeometry.MakeCube(1024f);
            cubeGeometry.Color = Color.White;

            // Attach cube physics and a directional light
            PhysicsColliderComponent cubePhysics = new PhysicsColliderComponent(cubeEntity, 1024f, 1024f, 1024f);
            LightComponent cubeLight = new LightComponent(cubeEntity, Vector3.Right, Color.White, 0.5f);

            // Create torus entity
            BaseEntity torusEntity = new BaseEntity(core.ActiveScene);

            // Attach torus primitive component
            GeometricPrimitiveComponent torusGeometry = new GeometricPrimitiveComponent(torusEntity);
            torusGeometry.MakeTorus(64f, 64f, 16);
            torusGeometry.Color = Color.White;

            // Attach torus physics and a point light
            PhysicsColliderComponent torusPhysics = new PhysicsColliderComponent(torusEntity, 128f, 64f, 128f, 1f);
            LightComponent torusLight = new LightComponent(torusEntity, Vector3.Zero, 512f, Color.Gold, 2f);
        }

        #endregion
    }
}
