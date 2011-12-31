// Project:         XNALibrary
// Description:     Simple XNA game library for DaggerfallConnect.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace DeepEngine.Deprecated
{
    /// <summary>
    /// Collects input from various devices and applies to a camera.
    /// </summary>
    public class Input
    {
        #region Class Variables

        // Keyboard movement
        private const float keyboardSpinRate = 100f;
        private const float keyboardMoveRate = 200f;
        private const float keyboardShiftKeyMultiplier = 3f;

        // Mouse movement
        private const float mouseSpinRate = 5f;
        private const float mouseMoveRate = 50f;
        private const float middleButonMoveRate = 10f;

        // GamePad movement
        private bool gamePadConnected = false;
        private bool gamePadInputReceived = false;
        private const float gamePadSpinRate = 180f;
        private const float gamePadMoveRate = 1100f;

        // Look options
        private bool invertMouseLookY = false;
        private bool invertGamePadLookY = false;

        // Mouse
        private Point lastMousePos = Point.Zero;
        private Point mousePos = Point.Zero;
        private Point mouseDelta = Point.Zero;

        // Changes
        private float pitch = 0.0f;
        private float yaw = 0.0f;
        private Vector3 movement = Vector3.Zero;

        // Input flags
        private DeviceFlags activeDevices = DeviceFlags.None;

        // Input state
        private GamePadState gamePadState;
        private KeyboardState keyboardState;
        private MouseState mouseState;

        #endregion

        #region Class Structures

        [Flags]
        public enum DeviceFlags
        {
            None = 0,
            Keyboard = 1,
            Mouse = 2,
            GamePad = 4,
            All = 7,
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets active input device flags.
        /// </summary>
        public DeviceFlags ActiveDevices
        {
            get { return activeDevices; }
            set { activeDevices = value; }
        }

        /// <summary>
        /// Gets connection state of player 1 gamepad.
        /// </summary>
        public bool GamePadConnected
        {
            get { return gamePadConnected; }
        }

        /// <summary>
        /// Gets flag set if GamePad input received last update.
        /// </summary>
        public bool GamePadInputReceived
        {
            get { return gamePadInputReceived; }
        }

        /// <summary>
        /// Gets or sets flag to invert mouse look.
        /// </summary>
        public bool InvertMouseLook
        {
            get { return invertMouseLookY; }
            set { invertMouseLookY = value; }
        }

        /// <summary>
        /// Gets or sets flag to invert gamepad look.
        /// </summary>
        public bool InvertGamePadLook
        {
            get { return invertGamePadLookY; }
            set { invertGamePadLookY = value; }
        }

        /// <summary>
        /// Gets current mouse position.
        /// </summary>
        public Point MousePos
        {
            get { return mousePos; }
        }

        /// <summary>
        /// Gets keyboard state at time of last update.
        /// </summary>
        public KeyboardState KeyboardState
        {
            get { return keyboardState; }
        }

        /// <summary>
        /// Gets mouse state at time of last update.
        /// </summary>
        public MouseState MouseState
        {
            get { return mouseState; }
        }

        /// <summary>
        /// Gets GamePad state at time of last update.
        /// </summary>
        public GamePadState GamePadState
        {
            get { return gamePadState; }
        }

        /// <summary>
        /// Gets movement delta.
        /// </summary>
        public Vector3 MovementDelta
        {
            get { return movement; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Input()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Called when input system should poll active devices.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last frame.</param>
        public void Update(TimeSpan elapsedTime)
        {
            // Calculate time delta
            float timeDelta = (float)elapsedTime.TotalSeconds;

            // Get keyboard state
            keyboardState = Keyboard.GetState();

            // Keyboard input
            if ((activeDevices & DeviceFlags.Keyboard) == DeviceFlags.Keyboard)
            {
                // Movement
                if (keyboardState.IsKeyDown(Keys.Q))                                            // Look left
                    yaw += keyboardSpinRate * timeDelta;
                if (keyboardState.IsKeyDown(Keys.E))                                            // Look right
                    yaw -= keyboardSpinRate * timeDelta;
                if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))        // Move forwards
                    movement.Z -= keyboardMoveRate * timeDelta;
                if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))      // Move backwards
                    movement.Z += keyboardMoveRate * timeDelta;
                if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))      // Move left
                    movement.X -= keyboardMoveRate * timeDelta;
                if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))     // Move right
                    movement.X += keyboardMoveRate * timeDelta;

                // Multiply keyboard movement when shift is down
                if (keyboardState.IsKeyDown(Keys.LeftShift) ||
                    keyboardState.IsKeyDown(Keys.RightShift))
                {
                    movement *= keyboardShiftKeyMultiplier;
                }
            }

            // Get mouse state
            mouseState = Mouse.GetState();
            lastMousePos = mousePos;
            mousePos.X = mouseState.X;
            mousePos.Y = mouseState.Y;
            mouseDelta.X = mousePos.X - lastMousePos.X;
            mouseDelta.Y = mousePos.Y - lastMousePos.Y;

            // Mouse input
            if ((activeDevices & DeviceFlags.Mouse) == DeviceFlags.Mouse)
            {
                // Mouse-look with right-button pressed
                if (mouseState.RightButton == ButtonState.Pressed)
                {
                    yaw -= (mouseDelta.X * mouseSpinRate) * timeDelta;
                    pitch -= ((invertMouseLookY) ? -mouseDelta.Y : mouseDelta.Y) * mouseSpinRate * timeDelta;
                }

                // Movement with middle-button pressed
                if (mouseState.MiddleButton == ButtonState.Pressed)
                {
                    movement.X += (mouseDelta.X * middleButonMoveRate) * timeDelta;
                    movement.Y -= (mouseDelta.Y * middleButonMoveRate) * timeDelta;
                }
            }

            // Get player 1 gamepad state
            gamePadState = GamePad.GetState(0);
            if (gamePadState.IsConnected)
            {
                gamePadConnected = true;
            }
            else
            {
                gamePadConnected = false;
            }

            // GamePad input
            if ((activeDevices & DeviceFlags.GamePad) == DeviceFlags.GamePad)
            {
                if (gamePadState.IsConnected)
                {
                    // Get start states
                    float startYaw = yaw;
                    float startPitch = pitch;
                    Vector3 startMovement = movement;

                    // Look left and right
                    if (gamePadState.ThumbSticks.Right.X < 0)
                        yaw += (gamePadSpinRate * -gamePadState.ThumbSticks.Right.X) * timeDelta;
                    if (gamePadState.ThumbSticks.Right.X > 0)
                        yaw -= (gamePadSpinRate * gamePadState.ThumbSticks.Right.X) * timeDelta;

                    // Look up and down
                    if (invertGamePadLookY)
                    {
                        if (gamePadState.ThumbSticks.Right.Y < 0)
                            pitch += (gamePadSpinRate * -gamePadState.ThumbSticks.Right.Y) * timeDelta;
                        if (gamePadState.ThumbSticks.Right.Y > 0)
                            pitch -= (gamePadSpinRate * gamePadState.ThumbSticks.Right.Y) * timeDelta;
                    }
                    else
                    {
                        if (gamePadState.ThumbSticks.Right.Y < 0)
                            pitch -= (gamePadSpinRate * -gamePadState.ThumbSticks.Right.Y) * timeDelta;
                        if (gamePadState.ThumbSticks.Right.Y > 0)
                            pitch += (gamePadSpinRate * gamePadState.ThumbSticks.Right.Y) * timeDelta;
                    }

                    // Move forward and backward
                    if (gamePadState.ThumbSticks.Left.Y > 0)
                        movement.Z -= (gamePadMoveRate * gamePadState.ThumbSticks.Left.Y) * timeDelta;
                    if (gamePadState.ThumbSticks.Left.Y < 0)
                        movement.Z += (gamePadMoveRate * -gamePadState.ThumbSticks.Left.Y) * timeDelta;

                    // Move left and right
                    if (gamePadState.ThumbSticks.Left.X > 0)
                        movement.X -= (gamePadMoveRate * -gamePadState.ThumbSticks.Left.X) * timeDelta;
                    if (gamePadState.ThumbSticks.Left.X < 0)
                        movement.X += (gamePadMoveRate * gamePadState.ThumbSticks.Left.X) * timeDelta;

                    // Move up, down, left, and right with d-pad
                    if (gamePadState.DPad.Up == ButtonState.Pressed)
                        movement.Y += gamePadMoveRate * timeDelta;
                    if (gamePadState.DPad.Down == ButtonState.Pressed)
                        movement.Y -= gamePadMoveRate * timeDelta;
                    if (gamePadState.DPad.Left == ButtonState.Pressed)
                        movement.X -= gamePadMoveRate * timeDelta;
                    if (gamePadState.DPad.Right == ButtonState.Pressed)
                        movement.X += gamePadMoveRate * timeDelta;

                    // Determine if gamepad used
                    if (yaw != startYaw ||
                        pitch != startPitch ||
                        movement != startMovement)
                    {
                        gamePadInputReceived = true;
                    }
                    else
                    {
                        gamePadInputReceived = false;
                    }
                }
            }
        }

        /// <summary>
        /// Apply pending changes to specified camera.
        /// </summary>
        /// <param name="camera">Camera to receive input.</param>
        /// <param name="reset">True to reset changes after being applied to camera.</param>
        public void Apply(Camera camera, bool reset)
        {
            // Transform camera by changes
            if (camera != null)
            {
                camera.Transform(yaw, pitch, movement);
                if (reset)
                    Reset();
            }
        }

        /// <summary>
        /// Clear any pending input.
        /// </summary>
        public void Reset()
        {
            yaw = 0.0f;
            pitch = 0.0f;
            movement = Vector3.Zero;
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
