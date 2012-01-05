using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
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
        SpriteFont font;

        Stopwatch stopwatch = Stopwatch.StartNew();
        long lastLoadTime = 0;

        long nextBallTime = 0;
        long minBallTime = 200;

        int colorIndex = 0;
        Color[] colors = new Color[]
        {
            Color.Red,
            Color.Green,
            Color.Blue,
            Color.Gold,
            Color.Purple,
            Color.YellowGreen,
        };


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

            // Load a sprite font
            font = Content.Load<SpriteFont>("SpriteFont1");

            long startTime = stopwatch.ElapsedMilliseconds;

            // Load a test scene
            LoadExteriorMapScene();
            //LoadBlockScene();
            //LoadModelScene();
            //LoadPhysicsScene();

            lastLoadTime = stopwatch.ElapsedMilliseconds - startTime;
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

            KeyboardState ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.Space) && stopwatch.ElapsedMilliseconds > nextBallTime)
            {
                ShootBall();
                nextBallTime = stopwatch.ElapsedMilliseconds + minBallTime;
            }

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

            // Draw debug buffers
            core.DeepCore.DrawDebugBuffers();

            // Compose engine statistics
            string status = string.Format(
                "Update: {0}ms, Draw: {1}ms, Lights: {2}",
                core.DeepCore.LastUpdateTime,
                core.DeepCore.LastDrawTime,
                core.DeepCore.VisibleLightsCount);

            // Draw engine statistics
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            spriteBatch.DrawString(font, status, Vector2.Zero, Color.Gold);
            spriteBatch.End();
        }

        #region Content Loading Methods

        /// <summary>
        /// Loads a map scene.
        /// </summary>
        private void LoadExteriorMapScene()
        {
            // Set clear colour to black
            core.DeepCore.Renderer.ClearColor = Color.Black;

            // Set camera position
            core.ActiveScene.DeprecatedCamera.Position = new Vector3(2048, 500, 4096);

            // Create level entity
            WorldEntity level = new WorldEntity(core.ActiveScene);

            // Get location
            DFLocation location = core.DeepCore.MapManager.GetLocation("Wayrest", "Wayrest");
            int width = location.Exterior.ExteriorData.Width;
            int height = location.Exterior.ExteriorData.Height;

            // Add block components
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Get block name
                    string name = core.DeepCore.BlockManager.CheckName(
                        core.DeepCore.MapManager.GetRmbBlockName(ref location, x, y));

                    // Create block component
                    DaggerfallBlockComponent block = new DaggerfallBlockComponent(core.DeepCore, core.DeepCore.ActiveScene);
                    block.LoadBlock(name, location.Climate);
                    block.Matrix = Matrix.CreateTranslation(x * BlocksFile.RMBDimension, 0f, -(y * BlocksFile.RMBDimension));

                    // Attach component
                    level.Components.Add(block);
                }
            }

            // Clear model cache to release some memory
            core.DeepCore.ModelManager.ClearModelData();

            // Create directional light
            WorldEntity directionalLight = new WorldEntity(core.ActiveScene);
            directionalLight.Components.Add(new LightComponent(core.DeepCore, Vector3.Down + Vector3.Right, Color.White, 0.2f));
        }

        private void LoadBlockScene()
        {
            // Set camera position
            core.ActiveScene.DeprecatedCamera.Position = new Vector3(2048, 500, 4096);

            // Create level entity
            WorldEntity level = new WorldEntity(core.ActiveScene);

            // Create block component
            DaggerfallBlockComponent block = new DaggerfallBlockComponent(core.DeepCore, core.DeepCore.ActiveScene);
            block.LoadBlock("MAGEAA13.RMB", MapsFile.DefaultClimateSettings);

            // Attach component
            level.Components.Add(block);

            // Create directional light
            WorldEntity directionalLight = new WorldEntity(core.ActiveScene);
            directionalLight.Components.Add(new LightComponent(core.DeepCore, Vector3.Down + Vector3.Right, Color.White, 1));
        }

        /// <summary>
        /// Loads a simple Daggerfall model scene.
        /// </summary>
        private void LoadModelScene()
        {
            // Set camera position
            core.ActiveScene.DeprecatedCamera.Position = new Vector3(0, 0, 1000);

            // Create directional light
            WorldEntity lightEntity = new WorldEntity(core.ActiveScene);
            lightEntity.Components.Add(new LightComponent(core.DeepCore, Vector3.Right, Color.White, 1.0f));

            // Create model entity
            WorldEntity modelEntity = new WorldEntity(core.ActiveScene);
            modelEntity.Matrix = Matrix.CreateTranslation(0, -400, 0);
            modelEntity.Components.Add(new DaggerfallModelComponent(core.DeepCore, 456));

            // Create point light
            WorldEntity pointLightEntity = new WorldEntity(core.ActiveScene);
            pointLightEntity.Matrix = Matrix.CreateTranslation(500, 0, 0);
            pointLightEntity.Components.Add(new LightComponent(core.DeepCore, Vector3.Zero, 512f, Color.Red, 1f));
        }

        /// <summary>
        /// Loads a simple physics test scene.
        /// </summary>
        private void LoadPhysicsScene()
        {
            // Set camera position
            core.ActiveScene.DeprecatedCamera.Position = new Vector3(-100, -300, 800);

            // Create cube entity
            WorldEntity cubeEntity = new WorldEntity(core.ActiveScene);
            cubeEntity.Matrix = Matrix.CreateTranslation(-555, -1024, 0);

            // Create torus entity
            WorldEntity torusEntity = new WorldEntity(core.ActiveScene);

            // Create sphere entity
            WorldEntity sphereEntity = new WorldEntity(core.ActiveScene);
            sphereEntity.Matrix = Matrix.CreateTranslation(-555, 0, 0);

            // Attach cube geometry
            GeometricPrimitiveComponent cubeGeometry = new GeometricPrimitiveComponent(core.DeepCore);
            cubeGeometry.MakeCube(1024f);
            cubeGeometry.Color = Color.White;
            cubeEntity.Components.Add(cubeGeometry);

            // Attach cube physics and a directional light
            PhysicsColliderComponent cubePhysics = new PhysicsColliderComponent(core.DeepCore, core.DeepCore.ActiveScene, cubeEntity.Matrix, 1024f, 1024f, 1024f);
            LightComponent cubeLight = new LightComponent(core.DeepCore, Vector3.Right, Color.White, 0.5f);
            cubeEntity.Components.Add(cubePhysics);
            cubeEntity.Components.Add(cubeLight);

            // Attach torus geometry
            GeometricPrimitiveComponent torusGeometry = new GeometricPrimitiveComponent(core.DeepCore);
            torusGeometry.MakeTorus(64f, 64f, 16);
            torusGeometry.Color = Color.Red;
            torusEntity.Components.Add(torusGeometry);

            // Attach torus physics and a point light
            PhysicsColliderComponent torusPhysics = new PhysicsColliderComponent(core.DeepCore, core.DeepCore.ActiveScene, torusEntity.Matrix, 128f, 64f, 128f, 1f);
            LightComponent torusLight = new LightComponent(core.DeepCore, Vector3.Zero, 512f, Color.Red, 1f);
            torusEntity.Components.Add(torusPhysics);
            torusEntity.Components.Add(torusLight);

            // Attach sphere geometry
            GeometricPrimitiveComponent sphereGeometry = new GeometricPrimitiveComponent(core.DeepCore);
            sphereGeometry.MakeSphere(64f, 16);
            sphereGeometry.Color = Color.Red;
            sphereEntity.Components.Add(sphereGeometry);
            
            // Attach sphere physics
            PhysicsColliderComponent spherePhysics = new PhysicsColliderComponent(core.DeepCore, core.DeepCore.ActiveScene, sphereEntity.Matrix, 64f, 1f);
            spherePhysics.PhysicsEntity.Material.Bounciness = 0.0f;
            sphereEntity.Components.Add(spherePhysics);

            // Share torus light with sphere
            sphereEntity.Components.Add(torusLight);
        }

        #endregion

        #region Fun Stuff

        /// <summary>
        /// Shoots a coloured ball from player's position into world.
        /// </summary>
        public void ShootBall()
        {
            // Get next colour for ball
            Color sphereColor = colors[colorIndex++];
            if (colorIndex >= colors.Length)
                colorIndex = 0;

            // Get camera facing
            Vector3 cameraFacing = core.ActiveScene.DeprecatedCamera.TransformedReference;

            // Get start position
            Vector3 position = core.ActiveScene.DeprecatedCamera.Position;
            position += cameraFacing * 128;

            // Create sphere entity
            WorldEntity sphereEntity = new WorldEntity(core.ActiveScene);
            sphereEntity.Matrix = Matrix.CreateTranslation(position);

            // Attach sphere geometry
            GeometricPrimitiveComponent sphereGeometry = new GeometricPrimitiveComponent(core.DeepCore);
            sphereGeometry.MakeSphere(64f, 16);
            sphereGeometry.Color = sphereColor;
            sphereEntity.Components.Add(sphereGeometry);

            // Attach sphere physics
            PhysicsColliderComponent spherePhysics = new PhysicsColliderComponent(core.DeepCore, core.DeepCore.ActiveScene, sphereEntity.Matrix, 64f, 1f);
            spherePhysics.PhysicsEntity.LinearVelocity = cameraFacing * 500f;
            spherePhysics.PhysicsEntity.Material.Bounciness = 0.2f;
            sphereEntity.Components.Add(spherePhysics);

            // Attach sphere light
            LightComponent sphereLight = new LightComponent(core.DeepCore, Vector3.Zero, 512f, sphereColor, 1f);
            sphereEntity.Components.Add(sphereLight);

            // Set entity to expire after 5 minutes
            sphereEntity.Components.Add(new ReaperComponent(core.DeepCore, sphereEntity, 300000));
        }

        #endregion
    }
}
