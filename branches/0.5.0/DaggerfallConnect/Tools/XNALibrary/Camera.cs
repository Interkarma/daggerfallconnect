// Project:         XNALibrary
// Description:     Simple XNA game library for DaggerfallConnect.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Imports

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
    /// Camera class. Does not inherit from GameComponent so it can be used with WinForms.
    /// </summary>
    public class Camera
    {

        #region Class Variables

        // Movement
        private const float keyboardSpinRate = 1.0f;
        private const float mouseSpinRate = 0.1f;
        private const float moveRate = 10.0f;
        private const float mouseMoveRate = 2.0f; 

        // Clipping plane extents
        private float nearPlaneDistance = 1.0f;
        private float farPlaneDistance = 30000.0f;

        // Matrices
        private Matrix projectionMatrix = Matrix.Identity;
        private Matrix viewMatrix = Matrix.Identity;
        private Matrix worldMatrix = Matrix.Identity;

        // Camera
        private BoundingBox cameraBounds = new BoundingBox();
        private Vector3 cameraPosition = new Vector3(0, 64f, 0);
        private Vector3 cameraReference = Vector3.Forward;
        private Vector3 cameraUpVector = Vector3.Up;
        private Vector3 cameraTransformedReference;
        private Vector3 cameraTarget;
        private float cameraYaw = 0.0f;
        private float cameraPitch = 0.0f;
        private Vector3 movement = Vector3.Zero;
        private Matrix rotation = Matrix.Identity;

        // Mouse
        private Point lastMousePos = Point.Zero;
        private Point mousePos = Point.Zero;
        private Point mouseDelta = Point.Zero;

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
        /// Gets or sets camera bounds.
        /// </summary>
        public BoundingBox Bounds
        {
            get { return cameraBounds; }
            set { cameraBounds = value; EnforceBounds(); }
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
        /// Gets or sets camera position.
        /// </summary>
        public Vector3 Position
        {
            get { return cameraPosition; }
            set { cameraPosition = value; EnforceBounds(); }
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

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Camera()
        {
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
            cameraPosition.X += X;
            cameraPosition.Y += Y;
            cameraPosition.Z += Z;
            EnforceBounds();
        }

        /// <summary>
        /// Called when the camera should poll input methods and update.
        /// </summary>
        public void Update(UpdateFlags flags)
        {
            // Init movement and rotation
            movement = Vector3.Zero;
            rotation = Matrix.Identity;

            // Keyboard input
            if ((flags & UpdateFlags.Keyboard) == UpdateFlags.Keyboard)
            {
                KeyboardState ks = Keyboard.GetState();
                if (ks.IsKeyDown(Keys.Q))                               // Look left
                    cameraYaw += keyboardSpinRate;
                if (ks.IsKeyDown(Keys.E))                               // Look right
                    cameraYaw -= keyboardSpinRate;
                if (ks.IsKeyDown(Keys.W) || ks.IsKeyDown(Keys.Up))      // Move forwards
                    movement.Z -= moveRate;
                if (ks.IsKeyDown(Keys.S) || ks.IsKeyDown(Keys.Down))    // Move backwards
                    movement.Z += moveRate;
                if (ks.IsKeyDown(Keys.A) || ks.IsKeyDown(Keys.Left))    // Move left
                    movement.X -= moveRate;
                if (ks.IsKeyDown(Keys.D) || ks.IsKeyDown(Keys.Right))   // Move right
                    movement.X += moveRate;
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
                    cameraYaw -= mouseDelta.X * mouseSpinRate;
                    cameraPitch -= mouseDelta.Y * mouseSpinRate;
                }

                // Movement with right-button pressed
                if (ms.RightButton == ButtonState.Pressed)
                {
                    movement.Z += mouseDelta.Y * mouseMoveRate;
                }

                // Movement with middle-button pressed
                if (ms.MiddleButton == ButtonState.Pressed)
                {
                    movement.X += mouseDelta.X * moveRate;
                    movement.Y -= mouseDelta.Y * moveRate;
                }
            }

            // Controller input
            if ((flags & UpdateFlags.Controller) == UpdateFlags.Keyboard)
            {
                GamePadState cs = GamePad.GetState(0);
            }

            // Limit yaw
            if (cameraYaw > 360)
                cameraYaw -= 360;
            else if (cameraYaw < 0)
                cameraYaw += 360;
            // Limit pitch
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
                cameraPosition += movement;
                EnforceBounds();
            }
            
            // Transform camera
            Vector3.Transform(ref cameraReference, ref rotation, out cameraTransformedReference);
            Vector3.Add(ref cameraPosition, ref cameraTransformedReference, out cameraTarget);

            // Update view matrix
            Matrix.CreateLookAt(ref cameraPosition, ref cameraTarget, ref cameraUpVector, out viewMatrix);
        }

        /// <summary>
        /// Centres camera within X-Z bounds, with a specified height.
        /// </summary>
        public void CentreInBounds(float height)
        {
            cameraPosition = new Vector3(
                    cameraBounds.Min.X + (cameraBounds.Max.X - cameraBounds.Min.X) / 2,
                    height,
                    cameraBounds.Min.Z + (cameraBounds.Max.Z - cameraBounds.Min.Z) / 2);
        }

        /// <summary>
        /// Sets new aspect ratio and recreates projection matrix.
        /// </summary>
        /// <param name="aspectRatio">New aspect ratio.</param>
        public void SetAspectRatio(float aspectRatio)
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                aspectRatio,
                nearPlaneDistance,
                farPlaneDistance);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks and enforces camera bounds.
        /// </summary>
        private void EnforceBounds()
        {
            // Keep camera position within defined bounds
            if (cameraPosition.X < cameraBounds.Min.X) cameraPosition.X = cameraBounds.Min.X;
            if (cameraPosition.Y < cameraBounds.Min.Y) cameraPosition.Y = cameraBounds.Min.Y;
            if (cameraPosition.Z < cameraBounds.Min.Z) cameraPosition.Z = cameraBounds.Min.Z;
            if (cameraPosition.X > cameraBounds.Max.X) cameraPosition.X = cameraBounds.Max.X;
            if (cameraPosition.Y > cameraBounds.Max.Y) cameraPosition.Y = cameraBounds.Max.Y;
            if (cameraPosition.Z > cameraBounds.Max.Z) cameraPosition.Z = cameraBounds.Max.Z;
        }

        #endregion

    }

}
