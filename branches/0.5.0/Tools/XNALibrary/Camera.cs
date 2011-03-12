// Project:         DaggerfallConnect
// Description:     Read data from Daggerfall's file formats.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/using System;

#region Imports

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

#endregion


namespace XNALibrary
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Camera : Microsoft.Xna.Framework.GameComponent
    {
        #region Class Variables

        private IInputHandler input;

        private const float spinRate = 300.0f;
        private const float moveRate = 1200.0f;

        private float nearPlaneDistance = 1.0f;
        private float farPlaneDistance = 50000.0f;

        private Matrix projectionMatrix;
        private Matrix viewMatrix;

        private Vector3 cameraPosition = new Vector3(0, 1000, 3000);
        private Vector3 cameraReference = new Vector3(0, 0, -1);
        private Vector3 cameraUpVector = new Vector3(0, 1, 0);
        private float cameraYaw = 0.0f;
        private float cameraPitch = 0.0f;
        private Vector3 movement;

        #endregion

        #region Public Properties

        public Matrix Projection
        {
            get { return projectionMatrix; }
            set { projectionMatrix = value; }
        }

        public Matrix View
        {
            get { return viewMatrix; }
            set { viewMatrix = value; }
        }

        public Vector3 Position
        {
            get { return cameraPosition; }
            set { cameraPosition = value; }
        }

        public Vector3 Facing
        {
            get { return cameraReference; }
            set { cameraReference = value; }
        }

        public Vector3 Up
        {
            get { return cameraUpVector; }
            set { cameraUpVector = value; }
        }

        public float NearPlane
        {
            get { return nearPlaneDistance; }
            set { nearPlaneDistance = value; }
        }

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
        /// <param name="game"></param>
        public Camera(Game game)
            : base(game)
        {
            input = (IInputHandler)game.Services.GetService(typeof(IInputHandler));
        }

        #endregion

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            InitialiseCamera();
        }

        private void InitialiseCamera()
        {
            float aspectRatio = (float)Game.GraphicsDevice.Viewport.Width / (float)Game.GraphicsDevice.Viewport.Height;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, nearPlaneDistance, farPlaneDistance);
            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraPosition + cameraReference, cameraUpVector);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            movement = Vector3.Zero;

            //if (input.KeyboardState.IsKeyDown(Keys.Left))
            //    cameraYaw += spinRate * timeDelta;
            //if (input.KeyboardState.IsKeyDown(Keys.Right))
            //    cameraYaw -= spinRate * timeDelta;

            // Look left and right
            if (input.GamePads[0].ThumbSticks.Right.X < 0)
                cameraYaw += (spinRate * -input.GamePads[0].ThumbSticks.Right.X) * timeDelta;
            if (input.GamePads[0].ThumbSticks.Right.X > 0)
                cameraYaw -= (spinRate * input.GamePads[0].ThumbSticks.Right.X) * timeDelta;

            // Look up and down
            if (input.GamePads[0].ThumbSticks.Right.Y < 0)
                cameraPitch += (spinRate * -input.GamePads[0].ThumbSticks.Right.Y) * timeDelta;
            if (input.GamePads[0].ThumbSticks.Right.Y > 0)
                cameraPitch -= (spinRate * input.GamePads[0].ThumbSticks.Right.Y) * timeDelta;

            // Move forward and backward
            if (input.GamePads[0].ThumbSticks.Left.Y > 0)
                movement.Z -= input.GamePads[0].ThumbSticks.Left.Y;
            if (input.GamePads[0].ThumbSticks.Left.Y < 0)
                movement.Z += -input.GamePads[0].ThumbSticks.Left.Y;

            // Move left and right
            if (input.GamePads[0].ThumbSticks.Left.X > 0)
                movement.X -= -input.GamePads[0].ThumbSticks.Left.X;
            if (input.GamePads[0].ThumbSticks.Left.X < 0)
                movement.X += input.GamePads[0].ThumbSticks.Left.X;

            if (cameraYaw > 360)
                cameraYaw -= 360;
            else if (cameraYaw < 0)
                cameraYaw += 360;

            if (cameraPitch > 89)
                cameraPitch = 89;
            if (cameraPitch < -89)
                cameraPitch = -89;

            movement *= (moveRate * timeDelta);

            Matrix rotation;
            Matrix.CreateRotationY(MathHelper.ToRadians(cameraYaw), out rotation);
            rotation = Matrix.CreateRotationX(MathHelper.ToRadians(cameraPitch)) * rotation;

            if (movement != Vector3.Zero)
            {
                Vector3.Transform(ref movement, ref rotation, out movement);
                cameraPosition += movement;
            }

            Vector3 transformedReference;
            Vector3.Transform(ref cameraReference, ref rotation, out transformedReference);
            Vector3 cameraTarget;
            Vector3.Add(ref cameraPosition, ref transformedReference, out cameraTarget);
            Matrix.CreateLookAt(ref cameraPosition, ref cameraTarget, ref cameraUpVector, out viewMatrix);

            base.Update(gameTime);
        }
    }
}