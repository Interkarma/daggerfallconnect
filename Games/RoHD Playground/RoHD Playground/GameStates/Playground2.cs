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
using DeepEngine.UserInterface;
#endregion

namespace RoHD_Playground.GameStates
{

    /// <summary>
    /// Playing game interface.
    /// </summary>
    public interface IPlayground2State : IGameState { }

    /// <summary>
    /// Playground2.
    /// </summary>
    class Playground2 : GameState, IPlayground2State
    {

        #region Fields

        Scene scene;
        CharacterControllerInput playerInput;

        MouseState startMouseState, mouseState;
        float mouseLookScale = 0.001f;
        float mouseLookSpeed = 1.0f;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets user mouse look speed multiplier.
        /// </summary>
        public float MouseLookSpeed
        {
            get { return mouseLookSpeed; }
            set { mouseLookSpeed = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="game">Game object.</param>
        public Playground2(DeepCore core, Game game)
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

            // Create level entity
            WorldEntity level = new WorldEntity(core.ActiveScene);

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
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(GameTime gameTime)
        {
            core.Draw(false);
            core.Present();
        }

        #endregion
    }

}
