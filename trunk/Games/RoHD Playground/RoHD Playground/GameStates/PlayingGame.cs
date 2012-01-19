// Project:         Ruins of Hill Deep - Playground Build
// Description:     Test environment for Ruins of Hill Deep development.
// Copyright:       Copyright (C) 2012 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DeepEngine.Core;
using DeepEngine.Components;
using DeepEngine.GameStates;
using DeepEngine.World;
using DeepEngine.Player;
#endregion

namespace RoHD_Playground.GameStates
{

    /// <summary>
    /// Playing game interface.
    /// </summary>
    public interface IPlayingGameState : IGameState { }

    /// <summary>
    /// Playing game.
    /// </summary>
    class PlayingGame : GameState, IPlayingGameState
    {

        #region Fields

        Scene scene;
        CharacterControllerInput playerInput;

        MouseState startMouseState, mouseState;
        float mouseLookSpeed = 0.001f;

        long nextObjectTime = 0;
        long minObjectTime = 200;

        SpriteFont consoleFont;

        int colorIndex = 0;
        int physicsObjectIndex = 0;
        string physicsObjectText;
        List<PhysicsObjects> physicsObjects;

        Song song1;

        // Alpha value < 0.5 is specular intensity.
        Vector4[] specularColors = new Vector4[]
        {
            new Vector4(Color.Red.ToVector3(), 0.49f),
            new Vector4(Color.Green.ToVector3(), 0.49f),
            new Vector4(Color.Blue.ToVector3(), 0.49f),
            new Vector4(Color.Gold.ToVector3(), 0.49f),
            new Vector4(Color.Purple.ToVector3(), 0.49f),
            new Vector4(Color.YellowGreen.ToVector3(), 0.49f),
        };

        // Alpha value > 0.5 is emissive intensity.
        Vector4[] emissiveColors = new Vector4[]
        {
            new Vector4(Color.Red.ToVector3(), 0.99f),
            new Vector4(Color.Green.ToVector3(), 0.99f),
            new Vector4(Color.Blue.ToVector3(), 0.99f),
            new Vector4(Color.Gold.ToVector3(), 0.99f),
            new Vector4(Color.Purple.ToVector3(), 0.99f),
            new Vector4(Color.YellowGreen.ToVector3(), 0.99f),
        };

        #endregion

        #region Structures

        /// <summary>
        /// Various physic objects the player can fire.
        /// </summary>
        private enum PhysicsObjects
        {
            SpecularSphere,
            EmissiveSphere,
            SpecularCube,
            EmissiveCube,
            Anvil = 74212,
            Arrow = 99800,
            Octahedron = 74231,
            Branch = 41737,
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="game">Game object.</param>
        public PlayingGame(DeepCore core, Game game)
            : base(core, game)
        {
            // Build list of physics objects we're going to use
            physicsObjects = new List<PhysicsObjects>();
            physicsObjects.Add(PhysicsObjects.SpecularSphere);
            physicsObjects.Add(PhysicsObjects.EmissiveSphere);
            physicsObjects.Add(PhysicsObjects.SpecularCube);
            physicsObjects.Add(PhysicsObjects.EmissiveCube);
            physicsObjects.Add(PhysicsObjects.Anvil);
            physicsObjects.Add(PhysicsObjects.Arrow);
            physicsObjects.Add(PhysicsObjects.Octahedron);
            physicsObjects.Add(PhysicsObjects.Branch);
        }

        #endregion

        #region GameState Overrides

        protected override void LoadContent()
        {
            // Load fonts
            consoleFont = Game.Content.Load<SpriteFont>("Fonts/MenuFont");

            core.Renderer.ShowDebugBuffers = false;

            // Create scene
            scene = new Scene(core);
            scene.Camera.Position = new Vector3(22, 27, -20);
            scene.Camera.Update();
            core.ActiveScene = scene;

            // Set day/night mode for window textures
            core.MaterialManager.Daytime = false;

            // Create level entity
            WorldEntity level = new WorldEntity(core.ActiveScene);

            // Create block component
            DaggerfallBlockComponent block = new DaggerfallBlockComponent(core, core.ActiveScene);
            block.LoadBlock("S0000181.RDB", MapsFile.DefaultClimateSettings);
            level.Components.Add(block);

            // Attach block flats
            AddBlockFlats(level, block);

            // Attach block lights
            AddBlockLights(level, block);

            // Disable core input handling
            core.Input.ActiveDevices = Input.DeviceFlags.None;

            // Create player controller
            playerInput = new CharacterControllerInput(core.ActiveScene.Space, core.ActiveScene.Camera);
            playerInput.UseCameraSmoothing = true;
            playerInput.CharacterController.JumpSpeed = 9;
            playerInput.CharacterController.HorizontalMotionConstraint.Speed = 8;
            playerInput.StandingCameraOffset = 1.4f;
            playerInput.Activate();

            // Initialise mouse state
            Game.IsMouseVisible = false;
            Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            startMouseState = mouseState = Mouse.GetState();

            // Load songs
            MediaPlayer.Stop();
            song1 = Game.Content.Load<Song>("Songs/DanGoodale_DF-D2");
            //MediaPlayer.Play(song1);

            UpdatePhysicsObjectText();
        }

        public override void Update(GameTime gameTime)
        {
            if (playerInput != null)
            {
                playerInput.Update(
                    gameTime.ElapsedGameTime.Seconds,
                    core.Input.PreviousKeyboardState,
                    core.Input.KeyboardState,
                    core.Input.PreviousGamePadState,
                    core.Input.GamePadState);
            }

            // Mouse look
            mouseState = core.Input.MouseState;
            if (mouseState != startMouseState)
            {
                mouseState = core.Input.MouseState;
                int mouseChangeX = mouseState.X - startMouseState.X;
                int mouseChangeY = mouseState.Y - startMouseState.Y;

                float yawDegrees = -MathHelper.ToDegrees(mouseChangeX) * mouseLookSpeed;
                float pitchDegrees = -MathHelper.ToDegrees(mouseChangeY) * mouseLookSpeed;

                if (core.Input.InvertMouseLook) pitchDegrees = -pitchDegrees;

                scene.Camera.Transform(yawDegrees, pitchDegrees, Vector3.Zero);

                Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            }

            KeyboardState keyState = core.Input.KeyboardState;
            KeyboardState lastKeyState = core.Input.PreviousKeyboardState;
            if (keyState.IsKeyDown(Keys.F) && core.Stopwatch.ElapsedMilliseconds > nextObjectTime)
            {
                FireCurrentObject();
                nextObjectTime = core.Stopwatch.ElapsedMilliseconds + minObjectTime;
            }

            // Next physics object
            if (keyState.IsKeyDown(Keys.Up) && !lastKeyState.IsKeyDown(Keys.Up))
            {
                physicsObjectIndex++;
                if (physicsObjectIndex >= physicsObjects.Count)
                    physicsObjectIndex = 0;
                UpdatePhysicsObjectText();
            }

            // Previous physics object
            if (keyState.IsKeyDown(Keys.Down) && !lastKeyState.IsKeyDown(Keys.Down))
            {
                physicsObjectIndex--;
                if (physicsObjectIndex < 0)
                    physicsObjectIndex = physicsObjects.Count - 1;
                UpdatePhysicsObjectText();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            core.Draw(false);
            GraphicsDevice.Clear(Color.Black);
            core.Present();

            core.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            core.SpriteBatch.DrawString(consoleFont, physicsObjectText, Vector2.Zero, Color.White);
            core.SpriteBatch.End();
        }

        #endregion

        #region Private Methods

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

            // Add flats to entity
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
            }
        }

        /// <summary>
        /// Attach light components to an entity.
        /// </summary>
        /// <param name="entity">Entity to attach lights.</param>
        /// <param name="block">Block to get lights from</param>
        private void AddBlockLights(BaseEntity entity, DaggerfallBlockComponent block)
        {
            // Exit if no lights
            if (block.BlockLights.Count == 0)
                return;

            // Add lights to entity
            foreach (var light in block.BlockLights)
            {
                // Get position
                Vector3 position = new Vector3(light.Position.X, light.Position.Y, light.Position.Z);

                // Add light
                LightComponent lightComponent = new LightComponent(
                    core,
                    position,
                    light.Radius,
                    Color.White,
                    1.0f);
                entity.Components.Add(lightComponent);
            }
        }

        private void UpdatePhysicsObjectText()
        {
            physicsObjectText = string.Format("Physics Object: {0}", physicsObjects[physicsObjectIndex].ToString());
        }

        #endregion

        #region Physics Objects

        /// <summary>
        /// Fires current physics object
        /// </summary>
        private void FireCurrentObject()
        {
            switch (physicsObjects[physicsObjectIndex])
            {
                case PhysicsObjects.SpecularSphere:
                    FireSphere(specularColors);
                    break;
                case PhysicsObjects.EmissiveSphere:
                    FireSphere(emissiveColors);
                    break;
                case PhysicsObjects.SpecularCube:
                    FireCube(specularColors);
                    break;
                case PhysicsObjects.EmissiveCube:
                    FireCube(emissiveColors);
                    break;
                case PhysicsObjects.Anvil:
                    FireAnvil();
                    break;
                case PhysicsObjects.Arrow:
                    FireArrow();
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Fires a coloured sphere from player's position into world.
        /// </summary>
        private void FireSphere(Vector4[] colors)
        {
            float radius = 0.70f;

            // Get next colour for sphere
            Vector4 sphereColor = colors[colorIndex++];
            Color lightColor = new Color(sphereColor.X, sphereColor.Y, sphereColor.Z);
            if (colorIndex >= colors.Length)
                colorIndex = 0;

            // Get camera facing
            Vector3 cameraFacing = core.ActiveScene.Camera.TransformedReference;

            // Get start position
            Vector3 position = core.ActiveScene.Camera.Position;
            position += cameraFacing * radius;

            // Create sphere entity
            WorldEntity sphereEntity = new WorldEntity(core.ActiveScene);
            sphereEntity.Matrix = Matrix.CreateTranslation(position);

            // Attach sphere geometry
            GeometricPrimitiveComponent sphereGeometry = new GeometricPrimitiveComponent(core);
            sphereGeometry.MakeSphere(radius, 16);
            sphereGeometry.Color = sphereColor;
            sphereEntity.Components.Add(sphereGeometry);

            // Attach sphere physics
            PhysicsColliderComponent spherePhysics = new PhysicsColliderComponent(core, core.ActiveScene, sphereEntity.Matrix, radius, 1f);
            spherePhysics.PhysicsEntity.LinearVelocity = cameraFacing * 15f;
            spherePhysics.PhysicsEntity.Material.Bounciness = 0.2f;
            sphereEntity.Components.Add(spherePhysics);

            // Attach sphere light
            LightComponent sphereLight = new LightComponent(core, Vector3.Zero, radius * 2.5f, lightColor, 2.0f);
            sphereEntity.Components.Add(sphereLight);

            // Set entity to expire after 5 minutes
            sphereEntity.Components.Add(new ReaperComponent(core, sphereEntity, 300000));
        }

        /// <summary>
        /// Fires a coloured cube from player's position into world.
        /// </summary>
        private void FireCube(Vector4[] colors)
        {
            float size = 1.5f;

            // Get next colour for cube
            Vector4 cubeColor = colors[colorIndex++];
            Color lightColor = new Color(cubeColor.X, cubeColor.Y, cubeColor.Z);
            if (colorIndex >= colors.Length)
                colorIndex = 0;

            // Get camera facing
            Vector3 cameraFacing = core.ActiveScene.Camera.TransformedReference;

            // Get start position
            Vector3 position = core.ActiveScene.Camera.Position;
            position += cameraFacing * size;

            // Create cube entity
            WorldEntity cubeEntity = new WorldEntity(core.ActiveScene);
            cubeEntity.Matrix = Matrix.CreateTranslation(position);

            // Attach cube geometry
            GeometricPrimitiveComponent cubeGeometry = new GeometricPrimitiveComponent(core);
            cubeGeometry.MakeCube(size);
            cubeGeometry.Color = cubeColor;
            cubeEntity.Components.Add(cubeGeometry);

            // Attach cube physics
            PhysicsColliderComponent cubePhysics = new PhysicsColliderComponent(core, core.ActiveScene, cubeEntity.Matrix, size, size, size, 1f);
            cubePhysics.PhysicsEntity.LinearVelocity = cameraFacing * 15f;
            cubePhysics.PhysicsEntity.Material.Bounciness = 0.0f;
            cubeEntity.Components.Add(cubePhysics);

            // Attach cube light
            LightComponent cubeLight = new LightComponent(core, Vector3.Zero, size * 2.5f, lightColor, 2.0f);
            cubeEntity.Components.Add(cubeLight);

            // Set entity to expire after 5 minutes
            cubeEntity.Components.Add(new ReaperComponent(core, cubeEntity, 300000));
        }

        /// <summary>
        /// Drops a heavy anvil.
        /// </summary>
        private void FireAnvil()
        {
            // Get anvil model
            DaggerfallModelComponent anvilModel = new DaggerfallModelComponent(core, (int)PhysicsObjects.Anvil);
            anvilModel.Matrix = Matrix.CreateScale(0.04f);

            // Get camera facing
            Vector3 cameraFacing = core.ActiveScene.Camera.TransformedReference;

            // Get start position
            Vector3 position = core.ActiveScene.Camera.Position;
            position += cameraFacing * (anvilModel.BoundingSphere.Radius * 2);

            // Create anvil entity
            WorldEntity anvilEntity = new WorldEntity(core.ActiveScene);
            anvilEntity.Matrix = Matrix.CreateTranslation(position);

            // Attach anvil geometry
            anvilEntity.Components.Add(anvilModel);

            // Attach anvil physics
            BoundingBox box = anvilModel.BoundingBox;
            float width = box.Max.X - box.Min.X;
            float height = box.Max.Y - box.Min.Y;
            float depth = box.Max.Z - box.Min.Z;
            PhysicsColliderComponent anvilPhysics = new PhysicsColliderComponent(core, core.ActiveScene, anvilEntity.Matrix, width, height, depth, 200f);
            anvilPhysics.PhysicsEntity.Material.Bounciness = 0.0f;
            anvilEntity.Components.Add(anvilPhysics);

            // Set entity to expire after 5 minutes
            anvilEntity.Components.Add(new ReaperComponent(core, anvilEntity, 300000));
        }

        /// <summary>
        /// Fires an arrow projectile.
        /// </summary>
        private void FireArrow()
        {
            // Get arrow model
            DaggerfallModelComponent arrowModel = new DaggerfallModelComponent(core, (int)PhysicsObjects.Arrow);
            arrowModel.Matrix = Matrix.CreateScale(1.0f);

            // Get camera facing
            Vector3 cameraFacing = core.ActiveScene.Camera.TransformedReference;

            // Get start position
            Vector3 position = core.ActiveScene.Camera.Position;
            position += cameraFacing * (arrowModel.BoundingSphere.Radius);

            // Create arrow entity
            WorldEntity arrowEntity = new WorldEntity(core.ActiveScene);
            arrowEntity.Matrix = Matrix.CreateTranslation(position);

            // Attach arrow geometry
            arrowEntity.Components.Add(arrowModel);

            // Attach arrow physics
            BoundingBox box = arrowModel.BoundingBox;
            float width = box.Max.X - box.Min.X;
            float height = box.Max.Y - box.Min.Y;
            float depth = box.Max.Z - box.Min.Z;
            PhysicsColliderComponent arrowPhysics = new PhysicsColliderComponent(core, core.ActiveScene, arrowEntity.Matrix, width, height, depth, 1f);
            arrowPhysics.PhysicsEntity.LinearVelocity = cameraFacing * 40f;
            arrowPhysics.PhysicsEntity.Material.Bounciness = 0.1f;
            arrowEntity.Components.Add(arrowPhysics);

            // Set entity to expire after 2 seconds
            arrowEntity.Components.Add(new ReaperComponent(core, arrowEntity, 2000));
        }

        #endregion
    }

}
