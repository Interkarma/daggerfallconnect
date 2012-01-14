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
using DeepEngine.Core;
using DeepEngine.World;
using DeepEngine.Components;

namespace ContentLoading1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        DeepCore core;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        Stopwatch stopwatch = Stopwatch.StartNew();
        long lastLoadTime = 0;

        long nextBallTime = 0;
        long minBallTime = 200;

        // Ball colours.
        // Alpha value < 0.5 is specular intensity.
        // Alpha value > 0.5 is emissive intensity.
        int colorIndex = 0;
        Vector4[] colors = new Vector4[]
        {
            new Vector4(Color.Red.ToVector3(), 0.9f),
            new Vector4(Color.Green.ToVector3(), 0.9f),
            new Vector4(Color.Blue.ToVector3(), 0.9f),
            new Vector4(Color.Gold.ToVector3(), 0.9f),
            new Vector4(Color.Purple.ToVector3(), 0.9f),
            new Vector4(Color.YellowGreen.ToVector3(), 0.9f),
        };


        const string arena2Path = @"c:\dosgames\dagger\arena2";

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            //graphics.PreferredBackBufferWidth = 1920;
            //graphics.PreferredBackBufferHeight = 1080;
            this.IsMouseVisible = true;

            // Create engine core
            core = new DeepCore(arena2Path, this.Services);
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

            core.Initialize();

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
            //LoadExteriorMapScene();
            LoadBlockScene();
            //LoadModelScene();
            //LoadPhysicsScene();

            // Show or hide debug buffers
            core.Renderer.ShowDebugBuffers = true;

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

            core.Update(gameTime);

            KeyboardState ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.Space) && stopwatch.ElapsedMilliseconds > nextBallTime)
            {
                ShootBall();
                nextBallTime = stopwatch.ElapsedMilliseconds + minBallTime;
            }

            if (ks.IsKeyDown(Keys.Escape))
            {
                Exit();
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

            core.Draw();

            // Compose engine statistics
            string status = string.Format(
                "Update: {0:00}ms, Draw: {1:00}ms, Lights: {2:000}, Billboards: {3:0000}",
                core.LastUpdateTime,
                core.LastDrawTime,
                core.VisibleLightsCount,
                core.VisibleBillboardsCount);

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
            // Set clear colour
            core.Renderer.ClearColor = Color.Black;

            // Set day/night mode for window textures
            core.MaterialManager.Daytime = false;

            // Set camera position
            core.ActiveScene.DeprecatedCamera.Position = new Vector3(2048, 500, 4096);

            // Create level entity
            WorldEntity level = new WorldEntity(core.ActiveScene);

            // Get location
            DFLocation location = core.MapManager.GetLocation("Wayrest", "Wayrest");
            int width = location.Exterior.ExteriorData.Width;
            int height = location.Exterior.ExteriorData.Height;

            // Add block components
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Get block name
                    string name = core.BlockManager.CheckName(
                        core.MapManager.GetRmbBlockName(ref location, x, y));

                    // Get block translation
                    Vector3 blockPosition = new Vector3(x * BlocksFile.RMBDimension, 0f, -(y * BlocksFile.RMBDimension));

                    // Attach block component
                    DaggerfallBlockComponent block = new DaggerfallBlockComponent(core, core.ActiveScene);
                    block.LoadBlock(name, location.Climate);
                    block.Matrix = Matrix.CreateTranslation(blockPosition);
                    level.Components.Add(block);

                    // Attach block flats
                    AddBlockFlats(level, block);
                }
            }

            // Clear model cache to release some memory
            core.ModelManager.ClearModelData();

            // Create directional light
            float lightIntensity = 0.25f;
            WorldEntity directionalLight = new WorldEntity(core.ActiveScene);
            directionalLight.Components.Add(new LightComponent(core, Vector3.Normalize(Vector3.Down + Vector3.Right), Color.White, lightIntensity));
            directionalLight.Components.Add(new LightComponent(core, Vector3.Normalize(Vector3.Forward + Vector3.Left), Color.White, lightIntensity));
        }

        private void LoadBlockScene()
        {
            // Set clear colour
            core.Renderer.ClearColor = Color.Black;

            // Set day/night mode for window textures
            core.MaterialManager.Daytime = false;

            // Set camera position
            core.ActiveScene.DeprecatedCamera.Position = new Vector3(2048, 500, 4096);

            // Create level entity
            WorldEntity level = new WorldEntity(core.ActiveScene);

            // Create block component
            DaggerfallBlockComponent block = new DaggerfallBlockComponent(core, core.ActiveScene);
            //block.LoadBlock("BOOKAL02.RMB", MapsFile.DefaultClimateSettings);
            //block.LoadBlock("S0000040.RDB", MapsFile.DefaultClimateSettings);
            block.LoadBlock("N0000000.RDB", MapsFile.DefaultClimateSettings);
            level.Components.Add(block);

            // Attach block flats
            AddBlockFlats(level, block);

            // Create directional light
            float lightIntensity = 1f;
            WorldEntity directionalLight = new WorldEntity(core.ActiveScene);
            directionalLight.Components.Add(new LightComponent(core, Vector3.Normalize(Vector3.Down + Vector3.Right), Color.White, lightIntensity));
            directionalLight.Components.Add(new LightComponent(core, Vector3.Normalize(Vector3.Forward + Vector3.Left), Color.White, lightIntensity));
        }

        /// <summary>
        /// Attach flats component to an entity.
        /// </summary>
        /// <param name="entity">Entity to attach billboards.</param>
        /// <param name="block">Block to get flats from.</param>
        private void AddBlockFlats(BaseEntity entity, DaggerfallBlockComponent block)
        {
            // Exit if no flats
            if (block.BlockFlats.Count == 0)
                return;

            // Add flats to component
            foreach (var flat in block.BlockFlats)
            {
                // Filter editor flats
                if (flat.Type == DaggerfallBlockComponent.FlatTypes.Editor)
                    continue;

                // Get position
                Vector3 position = new Vector3(flat.Position.X, flat.Position.Y, flat.Position.Z);

                // Add billboard component
                DaggerfallBillboardComponent billboard = new DaggerfallBillboardComponent(core, flat);
                billboard.Matrix = block.Matrix * Matrix.CreateTranslation(position);
                entity.Components.Add(billboard);

                // Add a light component for each billboard light source outside of dungeons
                if (flat.Archive == 210 && flat.Dungeon == false)
                {
                    position.Y += billboard.Size.Y;
                    LightComponent lightComponent = new LightComponent(core, block.Matrix.Translation + position, 750f, Color.White, 1.1f);
                    entity.Components.Add(lightComponent);
                }
            }
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
            lightEntity.Components.Add(new LightComponent(core, Vector3.Right, Color.White, 1.0f));

            // Create model entity
            WorldEntity modelEntity = new WorldEntity(core.ActiveScene);
            modelEntity.Matrix = Matrix.CreateTranslation(0, -400, 0);
            modelEntity.Components.Add(new DaggerfallModelComponent(core, 456));

            // Create point light
            WorldEntity pointLightEntity = new WorldEntity(core.ActiveScene);
            pointLightEntity.Matrix = Matrix.CreateTranslation(500, 0, 0);
            pointLightEntity.Components.Add(new LightComponent(core, Vector3.Zero, 512f, Color.Red, 1f));
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
            GeometricPrimitiveComponent cubeGeometry = new GeometricPrimitiveComponent(core);
            cubeGeometry.MakeCube(1024f);
            cubeGeometry.Color = Vector4.One;
            cubeEntity.Components.Add(cubeGeometry);

            // Attach cube physics and a directional light
            PhysicsColliderComponent cubePhysics = new PhysicsColliderComponent(core, core.ActiveScene, cubeEntity.Matrix, 1024f, 1024f, 1024f);
            LightComponent cubeLight = new LightComponent(core, Vector3.Right, Color.White, 0.5f);
            cubeEntity.Components.Add(cubePhysics);
            cubeEntity.Components.Add(cubeLight);

            // Attach torus geometry
            GeometricPrimitiveComponent torusGeometry = new GeometricPrimitiveComponent(core);
            torusGeometry.MakeTorus(64f, 64f, 16);
            torusGeometry.Color = new Vector4(Color.Red.ToVector3(), 1);
            torusEntity.Components.Add(torusGeometry);

            // Attach torus physics and a point light
            PhysicsColliderComponent torusPhysics = new PhysicsColliderComponent(core, core.ActiveScene, torusEntity.Matrix, 128f, 64f, 128f, 1f);
            LightComponent torusLight = new LightComponent(core, Vector3.Zero, 512f, Color.Red, 1f);
            torusEntity.Components.Add(torusPhysics);
            torusEntity.Components.Add(torusLight);

            // Attach sphere geometry
            GeometricPrimitiveComponent sphereGeometry = new GeometricPrimitiveComponent(core);
            sphereGeometry.MakeSphere(64f, 16);
            sphereGeometry.Color = new Vector4(Color.Red.ToVector3(), 1);
            sphereEntity.Components.Add(sphereGeometry);
            
            // Attach sphere physics
            PhysicsColliderComponent spherePhysics = new PhysicsColliderComponent(core, core.ActiveScene, sphereEntity.Matrix, 64f, 1f);
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
            Vector4 sphereColor = colors[colorIndex++];
            Color lightColor = new Color(sphereColor.X, sphereColor.Y, sphereColor.Z);
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
            GeometricPrimitiveComponent sphereGeometry = new GeometricPrimitiveComponent(core);
            sphereGeometry.MakeSphere(64f, 16);
            sphereGeometry.Color = sphereColor;
            sphereEntity.Components.Add(sphereGeometry);

            // Attach sphere physics
            PhysicsColliderComponent spherePhysics = new PhysicsColliderComponent(core, core.ActiveScene, sphereEntity.Matrix, 64f, 1f);
            spherePhysics.PhysicsEntity.LinearVelocity = cameraFacing * 500f;
            spherePhysics.PhysicsEntity.Material.Bounciness = 0.2f;
            sphereEntity.Components.Add(spherePhysics);

            // Attach sphere light
            LightComponent sphereLight = new LightComponent(core, Vector3.Zero, 750f, lightColor, 2.0f);
            sphereEntity.Components.Add(sphereLight);

            // Set entity to expire after 5 minutes
            sphereEntity.Components.Add(new ReaperComponent(core, sphereEntity, 300000));
        }

        #endregion
    }
}
