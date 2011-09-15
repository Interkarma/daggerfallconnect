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

namespace XNALibrary
{
    /// <summary>
    /// Collects input from various devices and applies to a camera.
    /// </summary>
    public class Input
    {
        #region Class Variables

        // Keyboard movement
        private const float keyboardSpinRate = 200f;
        private const float keyboardMoveRate = 400f;
        private const float keyboardShiftKeyMultiplier = 3f;

        // Mouse movement
        private const float mouseSpinRate = 10f;
        private const float mouseMoveRate = 100f;
        private const float middleButonMoveRate = 100f;

        // Controller movement
        private bool controllerConnected = false;
        private const float controllerSpinRate = 180f;
        private const float controllerMoveRate = 1100f;

        // Look options
        private bool invertMouseLookY = false;
        private bool invertControllerLookY = true;

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

        #endregion

        #region Class Structures

        [Flags]
        public enum DeviceFlags
        {
            None = 0,
            Keyboard = 1,
            Mouse = 2,
            Controller = 4,
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
        /// Gets connection state of player 1 controller.
        /// </summary>
        public bool ControllerConnected
        {
            get { return controllerConnected; }
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
        /// Gets or sets flag to invert controller look.
        /// </summary>
        public bool InvertControllerLook
        {
            get { return invertControllerLookY; }
            set { invertControllerLookY = value; }
        }

        /// <summary>
        /// Gets current mouse position.
        /// </summary>
        public Point MousePos
        {
            get { return mousePos; }
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
            KeyboardState ks = Keyboard.GetState();

            // Keyboard input
            if ((activeDevices & DeviceFlags.Keyboard) == DeviceFlags.Keyboard)
            {
                // Movement
                if (ks.IsKeyDown(Keys.Q))                               // Look left
                    yaw += keyboardSpinRate * timeDelta;
                if (ks.IsKeyDown(Keys.E))                               // Look right
                    yaw -= keyboardSpinRate * timeDelta;
                if (ks.IsKeyDown(Keys.W) || ks.IsKeyDown(Keys.Up))      // Move forwards
                    movement.Z -= keyboardMoveRate * timeDelta;
                if (ks.IsKeyDown(Keys.S) || ks.IsKeyDown(Keys.Down))    // Move backwards
                    movement.Z += keyboardMoveRate * timeDelta;
                if (ks.IsKeyDown(Keys.A) || ks.IsKeyDown(Keys.Left))    // Move left
                    movement.X -= keyboardMoveRate * timeDelta;
                if (ks.IsKeyDown(Keys.D) || ks.IsKeyDown(Keys.Right))   // Move right
                    movement.X += keyboardMoveRate * timeDelta;

                // Multiply keyboard movement when shift is down
                if (ks.IsKeyDown(Keys.LeftShift) || ks.IsKeyDown(Keys.RightShift))
                {
                    movement *= keyboardShiftKeyMultiplier;
                }
            }

            // Get mouse state
            MouseState ms = Mouse.GetState();
            lastMousePos = mousePos;
            mousePos.X = ms.X;
            mousePos.Y = ms.Y;
            mouseDelta.X = mousePos.X - lastMousePos.X;
            mouseDelta.Y = mousePos.Y - lastMousePos.Y;

            // Mouse input
            if ((activeDevices & DeviceFlags.Mouse) == DeviceFlags.Mouse)
            {
                // Mouse-look with left-button pressed
                if (ms.LeftButton == ButtonState.Pressed)
                {
                    yaw -= (mouseDelta.X * mouseSpinRate) * timeDelta;
                    pitch -= ((invertMouseLookY) ? -mouseDelta.Y : mouseDelta.Y) * mouseSpinRate * timeDelta;
                }

                // Movement with right-button pressed
                if (ms.RightButton == ButtonState.Pressed)
                {
                    movement.Z += (mouseDelta.Y * mouseMoveRate) * timeDelta;
                }

                // Movement with middle-button pressed
                if (ms.MiddleButton == ButtonState.Pressed)
                {
                    movement.X += (mouseDelta.X * middleButonMoveRate) * timeDelta;
                    movement.Y -= (mouseDelta.Y * middleButonMoveRate) * timeDelta;
                }
            }

            /*
            // Controller input
            if ((flags & Flags.Controller) == Flags.Controller)
            {
                GamePadState cs = GamePad.GetState(0);
                if (cs.IsConnected)
                {
                    controllerConnected = true;

                    // Look left and right
                    if (cs.ThumbSticks.Right.X < 0)
                        cameraYaw += (controllerSpinRate * -cs.ThumbSticks.Right.X) * timeDelta;
                    if (cs.ThumbSticks.Right.X > 0)
                        cameraYaw -= (controllerSpinRate * cs.ThumbSticks.Right.X) * timeDelta;

                    // Look up and down
                    if (invertControllerLookY)
                    {
                        if (cs.ThumbSticks.Right.Y < 0)
                            cameraPitch += (controllerSpinRate * -cs.ThumbSticks.Right.Y) * timeDelta;
                        if (cs.ThumbSticks.Right.Y > 0)
                            cameraPitch -= (controllerSpinRate * cs.ThumbSticks.Right.Y) * timeDelta;
                    }
                    else
                    {
                        if (cs.ThumbSticks.Right.Y < 0)
                            cameraPitch -= (controllerSpinRate * -cs.ThumbSticks.Right.Y) * timeDelta;
                        if (cs.ThumbSticks.Right.Y > 0)
                            cameraPitch += (controllerSpinRate * cs.ThumbSticks.Right.Y) * timeDelta;
                    }

                    // Move forward and backward
                    if (cs.ThumbSticks.Left.Y > 0)
                        movement.Z -= (controllerMoveRate * cs.ThumbSticks.Left.Y) * timeDelta;
                    if (cs.ThumbSticks.Left.Y < 0)
                        movement.Z += (controllerMoveRate * -cs.ThumbSticks.Left.Y) * timeDelta;

                    // Move left and right
                    if (cs.ThumbSticks.Left.X > 0)
                        movement.X -= (controllerMoveRate * -cs.ThumbSticks.Left.X) * timeDelta;
                    if (cs.ThumbSticks.Left.X < 0)
                        movement.X += (controllerMoveRate * cs.ThumbSticks.Left.X) * timeDelta;
                }
                else
                {
                    controllerConnected = false;
                }
            }
            */
        }

        /// <summary>
        /// Apply pending changes to specified camera.
        /// </summary>
        /// <param name="camera">Camera to receive input.</param>
        public void Apply(Camera camera)
        {
            // Transform camera by changes
            if (camera != null)
            {
                camera.Transform(yaw, pitch, movement);
                camera.Update();
                yaw = 0.0f;
                pitch = 0.0f;
                movement = Vector3.Zero;
            }
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
