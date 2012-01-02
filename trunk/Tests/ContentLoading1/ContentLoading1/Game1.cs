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

            LoadBlockScene();
            //LoadModelScene();
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
        /// Loads a block scene.
        /// </summary>
        private void LoadBlockScene()
        {
            // Create level entity
            DynamicWorldEntity level = new DynamicWorldEntity(core.ActiveScene);

            // Create block component
            NativeBlockComponent block = new NativeBlockComponent(core.DeepCore, "MAGEAA13.RMB");
            level.Components.Add(block);

            // Create directional light
            DynamicWorldEntity directionalLight = new DynamicWorldEntity(core.ActiveScene);
            directionalLight.Components.Add(new LightComponent(core.DeepCore, Vector3.Down + Vector3.Right, Color.White, 1));
        }

        /// <summary>
        /// Loads a simple Daggerfall model scene.
        /// </summary>
        private void LoadModelScene()
        {
            // Create directional light
            DynamicWorldEntity lightEntity = new DynamicWorldEntity(core.ActiveScene);
            lightEntity.Components.Add(new LightComponent(core.DeepCore, Vector3.Right, Color.White, 1.0f));

            // Create model entity
            DynamicWorldEntity modelEntity = new DynamicWorldEntity(core.ActiveScene);
            modelEntity.Matrix = Matrix.CreateTranslation(0, -400, 0);
            modelEntity.Components.Add(new NativeModelComponent(core.DeepCore, 456));

            // Create point light
            DynamicWorldEntity pointLightEntity = new DynamicWorldEntity(core.ActiveScene);
            pointLightEntity.Matrix = Matrix.CreateTranslation(500, 0, 0);
            pointLightEntity.Components.Add(new LightComponent(core.DeepCore, Vector3.Zero, 512f, Color.Red, 1f));
        }

        /// <summary>
        /// Loads a simple physics test scene.
        /// </summary>
        private void LoadPhysicsScene()
        {
            // Create cube entity
            DynamicWorldEntity cubeEntity = new DynamicWorldEntity(core.ActiveScene);
            cubeEntity.Matrix = Matrix.CreateTranslation(-555, -1024, 0);

            // Create torus entity
            DynamicWorldEntity torusEntity = new DynamicWorldEntity(core.ActiveScene);

            // Create sphere entity
            DynamicWorldEntity sphereEntity = new DynamicWorldEntity(core.ActiveScene);
            sphereEntity.Matrix = Matrix.CreateTranslation(-555, 0, 0);

            // Attach cube geometry
            GeometricPrimitiveComponent cubeGeometry = new GeometricPrimitiveComponent(core.DeepCore);
            cubeGeometry.MakeCube(1024f);
            cubeGeometry.Color = Color.White;
            cubeEntity.Components.Add(cubeGeometry);

            // Attach cube physics and a directional light
            PhysicsColliderComponent cubePhysics = new PhysicsColliderComponent(core.DeepCore, cubeEntity.Matrix, 1024f, 1024f, 1024f);
            LightComponent cubeLight = new LightComponent(core.DeepCore, Vector3.Right, Color.White, 0.5f);
            cubeEntity.Components.Add(cubePhysics);
            cubeEntity.Components.Add(cubeLight);

            // Attach torus geometry
            GeometricPrimitiveComponent torusGeometry = new GeometricPrimitiveComponent(core.DeepCore);
            torusGeometry.MakeTorus(64f, 64f, 16);
            torusGeometry.Color = Color.Red;
            torusEntity.Components.Add(torusGeometry);

            // Attach torus physics and a point light
            PhysicsColliderComponent torusPhysics = new PhysicsColliderComponent(core.DeepCore, torusEntity.Matrix, 128f, 64f, 128f, 1f);
            LightComponent torusLight = new LightComponent(core.DeepCore, Vector3.Zero, 512f, Color.Red, 1f);
            torusEntity.Components.Add(torusPhysics);
            torusEntity.Components.Add(torusLight);

            // Attach sphere geometry
            GeometricPrimitiveComponent sphereGeometry = new GeometricPrimitiveComponent(core.DeepCore);
            sphereGeometry.MakeSphere(64f, 16);
            sphereGeometry.Color = Color.Red;
            sphereEntity.Components.Add(sphereGeometry);
            
            // Attach sphere physics
            PhysicsColliderComponent spherePhysics = new PhysicsColliderComponent(core.DeepCore, sphereEntity.Matrix, 64f, 1f);
            spherePhysics.PhysicsEntity.Material.Bounciness = 0.0f;
            sphereEntity.Components.Add(spherePhysics);

            // Share torus light with sphere
            sphereEntity.Components.Add(torusLight);
        }

        #endregion
    }
}
