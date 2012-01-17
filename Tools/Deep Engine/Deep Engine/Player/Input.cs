// Project:         Deep Engine
// Description:     3D game engine for Ruins of Hill Deep and Daggerfall Workshop projects.
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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DeepEngine.Daggerfall;
#endregion

namespace DeepEngine.Player
{
    /// <summary>
    /// Collects input from various devices and applies to a camera.
    /// </summary>
    public class Input
    {
        #region Class Variables

        // Keyboard movement
        float keyboardSpinRate = 200f;
        float keyboardMoveRate = 800f;
        float keyboardShiftKeyMultiplier = 2.5f;

        // Mouse movement
        float mouseSpinRate = 0.002f;
        float middleButtonMoveRate = 50f;

        // GamePad movement
        bool gamePadConnected = false;
        bool gamePadInputReceived = false;
        float gamePadSpinRate = 180f;
        float gamePadMoveRate = 1100f;

        // Look options
        bool invertMouseLookY = false;
        bool invertGamePadLookY = false;

        // Mouse
        Point lastMousePos = Point.Zero;
        Point mousePos = Point.Zero;
        Point mouseDelta = Point.Zero;

        // Changes
        float pitch = 0.0f;
        float yaw = 0.0f;
        Vector3 movement = Vector3.Zero;

        // Input flags
        DeviceFlags activeDevices = DeviceFlags.None;

        // Input state
        GamePadState gamePadState;
        GamePadState previousGamePadState;
        KeyboardState keyboardState;
        KeyboardState previousKeyboardState;
        MouseState mouseState;
        MouseState previousMouseState;

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
        /// Gets current GamePad state.
        /// </summary>
        public GamePadState GamePadState
        {
            get { return gamePadState; }
        }

        /// <summary>
        /// Gets current keyboard state.
        /// </summary>
        public KeyboardState KeyboardState
        {
            get { return keyboardState; }
        }

        /// <summary>
        /// Gets current mouse state.
        /// </summary>
        public MouseState MouseState
        {
            get { return mouseState; }
        }

        /// <summary>
        /// Gets previous GamePad state.
        /// </summary>
        public GamePadState PreviousGamePadState
        {
            get { return previousGamePadState; }
        }

        /// <summary>
        /// Gets previous keyboard state.
        /// </summary>
        public KeyboardState PreviousKeyboardState
        {
            get { return previousKeyboardState; }
        }

        /// <summary>
        /// Gets previous mouse state.
        /// </summary>
        public MouseState PreviousMouseState
        {
            get { return previousMouseState; }
        }

        /// <summary>
        /// Gets mouse movement delta.
        /// </summary>
        public Point MouseDelta
        {
            get { return mouseDelta; }
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
        /// <param name="elapsedTime">Elapsed time since last update.</param>
        public void Update(TimeSpan elapsedTime)
        {
            // Calculate delta time
            float dt = (float)elapsedTime.TotalSeconds;

            // Get keyboard state
            previousKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();

            /*
            // Keyboard input
            if ((activeDevices & DeviceFlags.Keyboard) == DeviceFlags.Keyboard)
            {
                // Movement
                if (keyboardState.IsKeyDown(Keys.Q))                                            // Look left
                    yaw += keyboardSpinRate * dt;
                if (keyboardState.IsKeyDown(Keys.E))                                            // Look right
                    yaw -= keyboardSpinRate * dt;
                if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))        // Move forwards
                    movement.Z -= keyboardMoveRate * dt;
                if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))      // Move backwards
                    movement.Z += keyboardMoveRate * dt;
                if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))      // Move left
                    movement.X -= keyboardMoveRate * dt;
                if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))     // Move right
                    movement.X += keyboardMoveRate * dt;

                // Multiply keyboard movement when shift is down
                if (keyboardState.IsKeyDown(Keys.LeftShift) ||
                    keyboardState.IsKeyDown(Keys.RightShift))
                {
                    movement *= keyboardShiftKeyMultiplier;
                }
            }

            // Get mouse state
            previousMouseState = mouseState;
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
                    float yawDegrees = MathHelper.ToDegrees(mouseDelta.X) * mouseSpinRate;
                    float pitchDegrees = MathHelper.ToDegrees(mouseDelta.Y) * mouseSpinRate;

                    yaw -= yawDegrees;
                    pitch -= (invertMouseLookY) ? -pitchDegrees : pitchDegrees;
                }

                // Movement with middle-button pressed
                if (mouseState.MiddleButton == ButtonState.Pressed)
                {
                    movement.X += (mouseDelta.X * middleButtonMoveRate) * dt;
                    movement.Y -= (mouseDelta.Y * middleButtonMoveRate) * dt;
                }
            }

            // Get player 1 gamepad state
            previousGamePadState = gamePadState;
            gamePadState = GamePad.GetState(0);
            if (gamePadState.IsConnected)
                gamePadConnected = true;
            else
                gamePadConnected = false;

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
                        yaw += (gamePadSpinRate * -gamePadState.ThumbSticks.Right.X) * dt;
                    if (gamePadState.ThumbSticks.Right.X > 0)
                        yaw -= (gamePadSpinRate * gamePadState.ThumbSticks.Right.X) * dt;

                    // Look up and down
                    if (invertGamePadLookY)
                    {
                        if (gamePadState.ThumbSticks.Right.Y < 0)
                            pitch += (gamePadSpinRate * -gamePadState.ThumbSticks.Right.Y) * dt;
                        if (gamePadState.ThumbSticks.Right.Y > 0)
                            pitch -= (gamePadSpinRate * gamePadState.ThumbSticks.Right.Y) * dt;
                    }
                    else
                    {
                        if (gamePadState.ThumbSticks.Right.Y < 0)
                            pitch -= (gamePadSpinRate * -gamePadState.ThumbSticks.Right.Y) * dt;
                        if (gamePadState.ThumbSticks.Right.Y > 0)
                            pitch += (gamePadSpinRate * gamePadState.ThumbSticks.Right.Y) * dt;
                    }

                    // Move forward and backward
                    if (gamePadState.ThumbSticks.Left.Y > 0)
                        movement.Z -= (gamePadMoveRate * gamePadState.ThumbSticks.Left.Y) * dt;
                    if (gamePadState.ThumbSticks.Left.Y < 0)
                        movement.Z += (gamePadMoveRate * -gamePadState.ThumbSticks.Left.Y) * dt;

                    // Move left and right
                    if (gamePadState.ThumbSticks.Left.X > 0)
                        movement.X -= (gamePadMoveRate * -gamePadState.ThumbSticks.Left.X) * dt;
                    if (gamePadState.ThumbSticks.Left.X < 0)
                        movement.X += (gamePadMoveRate * gamePadState.ThumbSticks.Left.X) * dt;

                    // Move up, down, left, and right with d-pad
                    if (gamePadState.DPad.Up == ButtonState.Pressed)
                        movement.Y += gamePadMoveRate * dt;
                    if (gamePadState.DPad.Down == ButtonState.Pressed)
                        movement.Y -= gamePadMoveRate * dt;
                    if (gamePadState.DPad.Left == ButtonState.Pressed)
                        movement.X -= gamePadMoveRate * dt;
                    if (gamePadState.DPad.Right == ButtonState.Pressed)
                        movement.X += gamePadMoveRate * dt;

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
            */
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
                if (camera.UseMovementControls)
                {
                    camera.Transform(yaw, pitch, movement);
                    if (reset) Reset();
                }
                
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
    }
}
