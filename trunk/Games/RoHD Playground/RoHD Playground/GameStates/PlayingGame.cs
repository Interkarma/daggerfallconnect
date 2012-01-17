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
        }

        #endregion

        #region GameState Overrides

        protected override void LoadContent()
        {
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
            playerInput.CharacterController.JumpSpeed = 6;
            playerInput.CharacterController.HorizontalMotionConstraint.Speed = 6;
            playerInput.StandingCameraOffset = 1.4f;
            playerInput.Activate();

            // Initialise mouse state
            Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            startMouseState = mouseState = Mouse.GetState();
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

            core.Update(gameTime.ElapsedGameTime);

            // Mouse look
            mouseState = Mouse.GetState();
            if (mouseState != startMouseState)
            {
                mouseState = Mouse.GetState();
                int mouseChangeX = mouseState.X - startMouseState.X;
                int mouseChangeY = mouseState.Y - startMouseState.Y;

                float yawDegrees = -MathHelper.ToDegrees(mouseChangeX) * mouseLookSpeed;
                float pitchDegrees = MathHelper.ToDegrees(mouseChangeY) * mouseLookSpeed;

                scene.Camera.Transform(yawDegrees, pitchDegrees, Vector3.Zero);

                Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            core.Draw(false);
            GraphicsDevice.Clear(Color.Black);
            core.Present();
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

        #endregion
    }

}
