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
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace XNALibrary
{

    /// <summary>
    /// Camera class.
    /// </summary>
    public class Camera
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

        // Clipping plane extents
        private float nearPlaneDistance = 1.0f;
        private float farPlaneDistance = 50000.0f;

        // Matrices
        private Matrix projectionMatrix = Matrix.Identity;
        private Matrix viewMatrix = Matrix.Identity;
        private Matrix worldMatrix = Matrix.Identity;

        // Camera
        private float eyeHeight = 70f;
        private float bodyRadius = 24f;
        private BoundingBox cameraMovementBounds;
        private BoundingSphere cameraBoundingSphere;
        private Vector3 cameraPosition;
        private Vector3 cameraPreviousPosition;
        private Vector3 cameraNextPosition;
        private Vector3 cameraReference = Vector3.Forward;
        private Vector3 cameraUpVector = Vector3.Up;
        private Vector3 cameraTransformedReference;
        private Vector3 cameraTarget;
        private float cameraYaw = 0.0f;
        private float cameraPitch = 0.0f;
        private Vector3 movement = Vector3.Zero;
        private Matrix rotation = Matrix.Identity;
        private BoundingFrustum viewFrustum;

        // Mouse
        private Point lastMousePos = Point.Zero;
        private Point mousePos = Point.Zero;
        private Point mouseDelta = Point.Zero;

        // Gravity
        private const float gravityRate = 40f;
        private float gravityAmount = 0;

        #endregion

        #region Class Structures

        [Flags]
        public enum UpdateFlags
        {
            None = 0,
            Keyboard = 1,
            Mouse = 2,
            Controller = 4,
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets camera movement bounds.
        /// </summary>
        public BoundingBox MovementBounds
        {
            get { return cameraMovementBounds; }
            set { cameraMovementBounds = value; }
        }

        /// <summary>
        /// Gets or sets eye height.
        /// </summary>
        public float EyeHeight
        {
            get { return eyeHeight; }
            set { eyeHeight = value; }
        }

        /// <summary>
        /// Gets or sets body radius.
        /// </summary>
        public float BodyRadius
        {
            get { return bodyRadius; }
            set { bodyRadius = value; }
        }

        /// <summary>
        /// Gets bounds of camera (based on next position).
        /// </summary>
        public BoundingSphere BoundingSphere
        {
            get
            {
                cameraBoundingSphere.Center = cameraNextPosition;
                cameraBoundingSphere.Radius = bodyRadius;
                return cameraBoundingSphere;
            }
        }

        /// <summary>
        /// Gets or sets projection matrix.
        /// </summary>
        public Matrix Projection
        {
            get { return projectionMatrix; }
            set { projectionMatrix = value; }
        }

        /// <summary>
        /// Gets or sets view matrix.
        /// </summary>
        public Matrix View
        {
            get { return viewMatrix; }
            set { viewMatrix = value; }
        }

        /// <summary>
        /// Gets or sets world matrix.
        /// </summary>
        public Matrix World
        {
            get { return worldMatrix; }
            set { worldMatrix = value; }
        }

        /// <summary>
        /// Gets camera position. Camera position
        ///  can be changed using NextPosition, which is
        ///  applied every time ApplyChanges() is called.
        /// </summary>
        public Vector3 Position
        {
            get { return cameraPosition; }
        }

        /// <summary>
        /// Geta previous camera position.
        /// </summary>
        public Vector3 PreviousPosition
        {
            get { return cameraPreviousPosition; }
        }

        /// <summary>
        /// Gets or sets next camera position, which
        ///  is applied when ApplyChanges() is called.
        /// </summary>
        public Vector3 NextPosition
        {
            get { return cameraNextPosition; }
            set
            {
                cameraNextPosition = value;
                EnforceBounds();
            }
        }

        /// <summary>
        /// Gets or sets camera facing.
        /// </summary>
        public Vector3 Reference
        {
            get { return cameraReference; }
            set { cameraReference = value; }
        }

        /// <summary>
        /// Gets transformed camera facing.
        /// </summary>
        public Vector3 TransformedReference
        {
            get { return cameraTransformedReference; }
        }

        /// <summary>
        /// Gets or sets up vector.
        /// </summary>
        public Vector3 Up
        {
            get { return cameraUpVector; }
            set { cameraUpVector = value; }
        }

        /// <summary>
        /// Gets or sets near plane value.
        /// </summary>
        public float NearPlane
        {
            get { return nearPlaneDistance; }
            set { nearPlaneDistance = value; }
        }

        /// <summary>
        /// Gets or sets far plane value.
        /// </summary>
        public float FarPlane
        {
            get { return farPlaneDistance; }
            set { farPlaneDistance = value; }
        }

        /// <summary>
        /// Gets current BoundingFrustum.
        /// </summary>
        public BoundingFrustum BoundingFrustum
        {
            get { return viewFrustum; }
        }

        /// <summary>
        /// Gets camera yaw in degrees.
        /// </summary>
        public float Yaw
        {
            get { return cameraYaw; }
        }

        /// <summary>
        /// Gets camera pitch in degrees.
        /// </summary>
        public float Pitch
        {
            get { return cameraPitch; }
        }

        /// <summary>
        /// Gets connection state of player 1 controller.
        /// </summary>
        public bool ControllerConnected
        {
            get { return controllerConnected; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Camera()
        {
            viewFrustum = new BoundingFrustum(Matrix.Identity);
            cameraMovementBounds = new BoundingBox();
            cameraBoundingSphere = new BoundingSphere();
            cameraPosition = new Vector3(0, eyeHeight, 0);
            cameraNextPosition = cameraPosition;
            cameraPreviousPosition = cameraPosition;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets camera yaw and pitch.
        /// </summary>
        public void ResetReference()
        {
            cameraYaw = 0f;
            cameraPitch = 0f;
        }

        /// <summary>
        /// Translate camera by coordinates.
        /// </summary>
        /// <param name="X">X amount.</param>
        /// <param name="Y">Y amount.</param>
        /// <param name="Z">Z amount.</param>
        public void Translate(float X, float Y, float Z)
        {
            cameraNextPosition.X = cameraPosition.X + X;
            cameraNextPosition.Y = cameraPosition.Y + Y;
            cameraNextPosition.Z = cameraPosition.Z + Z;
            EnforceBounds();
            ApplyChanges();
        }

        /// <summary>
        /// Called when the camera should poll input methods and update.
        /// </summary>
        /// <param name="flags">Update flags.</param>
        /// <param name="elapsedTime">Elapsed time last frame.</param>
        public void Update(UpdateFlags flags, TimeSpan elapsedTime)
        {
            // Calculate time delta
            float timeDelta = (float)elapsedTime.TotalSeconds;

            // Init movement and rotation
            movement = Vector3.Zero;
            rotation = Matrix.Identity;

            // Keyboard input
            if ((flags & UpdateFlags.Keyboard) == UpdateFlags.Keyboard)
            {
                // Get movement
                KeyboardState ks = Keyboard.GetState();
                if (ks.IsKeyDown(Keys.Q))                               // Look left
                    cameraYaw += keyboardSpinRate * timeDelta;
                if (ks.IsKeyDown(Keys.E))                               // Look right
                    cameraYaw -= keyboardSpinRate * timeDelta;
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

            // Mouse input
            if ((flags & UpdateFlags.Mouse) == UpdateFlags.Mouse)
            {
                // Update mouse state
                MouseState ms = Mouse.GetState();
                lastMousePos = mousePos;
                mousePos.X = ms.X;
                mousePos.Y = ms.Y;
                mouseDelta.X = mousePos.X - lastMousePos.X;
                mouseDelta.Y = mousePos.Y - lastMousePos.Y;

                // Mouse-look with left-button pressed
                if (ms.LeftButton == ButtonState.Pressed)
                {
                    cameraYaw -= (mouseDelta.X * mouseSpinRate) * timeDelta;
                    cameraPitch -= (mouseDelta.Y * mouseSpinRate) * timeDelta;
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

            // Controller input
            if ((flags & UpdateFlags.Controller) == UpdateFlags.Controller)
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
                    if (cs.ThumbSticks.Right.Y < 0)
                        cameraPitch += (controllerSpinRate * -cs.ThumbSticks.Right.Y) * timeDelta;
                    if (cs.ThumbSticks.Right.Y > 0)
                        cameraPitch -= (controllerSpinRate * cs.ThumbSticks.Right.Y) * timeDelta;

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

            // Apply gravity
            movement.Y -= gravityAmount * timeDelta;

            // Fix yaw
            if (cameraYaw > 360)
                cameraYaw -= 360;
            else if (cameraYaw < 0)
                cameraYaw += 360;

            // Fix pitch
            if (cameraPitch > 89)
                cameraPitch = 89;
            if (cameraPitch < -89)
                cameraPitch = -89;

            // Apply rotation
            Matrix.CreateRotationY(MathHelper.ToRadians(cameraYaw), out rotation);
            rotation = Matrix.CreateRotationX(MathHelper.ToRadians(cameraPitch)) * rotation;

            // Apply movement
            if (movement != Vector3.Zero)
            {
                Vector3.Transform(ref movement, ref rotation, out movement);
                cameraNextPosition = cameraPosition + movement;
                EnforceBounds();
            }
        }

        /// <summary>
        /// Apply pending changes to camera movement and rotation.
        ///  Updates View matrix.
        /// </summary>
        public void ApplyChanges()
        {
            // Update position
            cameraPreviousPosition = cameraPosition;
            cameraPosition = cameraNextPosition;
            
            // Transform camera
            Vector3.Transform(ref cameraReference, ref rotation, out cameraTransformedReference);
            Vector3.Add(ref cameraPosition, ref cameraTransformedReference, out cameraTarget);

            // Update view matrix
            Matrix.CreateLookAt(ref cameraPosition, ref cameraTarget, ref cameraUpVector, out viewMatrix);

            // Update frustum
            viewFrustum.Matrix = viewMatrix * projectionMatrix;
        }

        /// <summary>
        /// Centres camera within X-Z bounds, with a variable height.
        /// </summary>
        public void CentreInBounds(float height)
        {
            cameraNextPosition = new Vector3(
                    cameraMovementBounds.Min.X + (cameraMovementBounds.Max.X - cameraMovementBounds.Min.X) / 2,
                    height,
                    cameraMovementBounds.Min.Z + (cameraMovementBounds.Max.Z - cameraMovementBounds.Min.Z) / 2);
            ApplyChanges();
        }

        /// <summary>
        /// Sets new aspect ratio.
        ///  Updates Projections matrix.
        /// </summary>
        /// <param name="aspectRatio">New aspect ratio.</param>
        public void SetAspectRatio(float aspectRatio)
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                aspectRatio,
                nearPlaneDistance,
                farPlaneDistance);

            // Update frustum
            viewFrustum.Matrix = viewMatrix * projectionMatrix;
        }

        /// <summary>
        /// Add gravity acceleration to camera.
        ///  Each subsequent call will make camera fall faster.
        /// </summary>
        public void AddGravity()
        {
            gravityAmount += gravityRate;
        }

        /// <summary>
        /// Clears gravity acceleration.
        /// </summary>
        public void ClearGravity()
        {
            gravityAmount = 0f;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks and enforces camera bounds.
        /// </summary>
        private void EnforceBounds()
        {
            // Keep camera position within defined movement bounds
            if (cameraNextPosition.X < cameraMovementBounds.Min.X)
                cameraNextPosition.X = cameraMovementBounds.Min.X;

            if (cameraNextPosition.Y < cameraMovementBounds.Min.Y + eyeHeight)
            {
                ClearGravity();
                cameraNextPosition.Y = cameraMovementBounds.Min.Y + eyeHeight;
            }

            if (cameraNextPosition.Z < cameraMovementBounds.Min.Z)
                cameraNextPosition.Z = cameraMovementBounds.Min.Z;

            if (cameraNextPosition.X > cameraMovementBounds.Max.X)
                cameraNextPosition.X = cameraMovementBounds.Max.X;

            if (cameraNextPosition.Y > cameraMovementBounds.Max.Y)
                cameraNextPosition.Y = cameraMovementBounds.Max.Y;

            if (cameraNextPosition.Z > cameraMovementBounds.Max.Z)
                cameraNextPosition.Z = cameraMovementBounds.Max.Z;
        }

        #endregion

    }

}
