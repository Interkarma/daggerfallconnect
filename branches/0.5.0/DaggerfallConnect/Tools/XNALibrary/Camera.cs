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
        private const float spinRate = 300.0f;
        private const float moveRate = 1200.0f;

        // Clipping plane extents
        private float nearPlaneDistance = 1.0f;
        private float farPlaneDistance = 30000.0f;

        // Matrices
        private Matrix projectionMatrix = Matrix.Identity;
        private Matrix viewMatrix = Matrix.Identity;
        private Matrix worldMatrix = Matrix.Identity;

        // Camera
        private Vector3 cameraPosition = new Vector3(0, 0, 0);
        private Vector3 cameraReference = Vector3.Forward;
        private Vector3 cameraUpVector = Vector3.Up;
        private float cameraYaw = 0.0f;
        private float cameraPitch = 0.0f;
        private Vector3 movement;

        #endregion

        #region Public Properties

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
            set { cameraPosition = value; }
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
        /// Called when the camera should poll input methods and update.
        /// </summary>
        public void Update()
        {
            // Update view matrix
            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraPosition + cameraReference, cameraUpVector);

            // Get state from input devices
            //GamePadState gamePadState = GamePad.GetState(0);
            //MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();
        }

        #endregion

    }

}
