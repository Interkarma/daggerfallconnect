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
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace DeepEngine.Player
{

    /// <summary>
    /// Camera class.
    /// </summary>
    public class Camera
    {

        #region Fields

        // Clipping plane extents
        float nearPlaneDistance = 0.02f;
        float farPlaneDistance = 1000f;

        // Matrices
        Matrix projectionMatrix = Matrix.Identity;
        Matrix viewMatrix = Matrix.Identity;
        Matrix worldMatrix = Matrix.Identity;

        // Camera
        float cameraYaw = 0.0f;
        float cameraPitch = 0.0f;
        Vector3 cameraPosition;
        Vector3 cameraReference = Vector3.Forward;
        Vector3 cameraUpVector = Vector3.Up;
        Vector3 cameraTransformedReference;
        Vector3 cameraTarget;
        BoundingFrustum viewFrustum;

        // Movement
        bool useMovementControls = true;
        BoundingBox cameraMovementBounds;
        float cameraSpeed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets camera movement bounds.
        /// </summary>
        public BoundingBox MovementBounds
        {
            get { return cameraMovementBounds; }
            set { cameraMovementBounds = value; }
        }

        /// <summary>
        /// Gets or sets camera speed.
        /// </summary>
        public float Speed
        {
            get { return cameraSpeed; }
            set { cameraSpeed = value; }
        }

        /// <summary>
        /// Gets or sets projection matrix.
        /// </summary>
        public Matrix ProjectionMatrix
        {
            get { return projectionMatrix; }
            set { projectionMatrix = value; }
        }

        /// <summary>
        /// Gets or sets view matrix.
        /// </summary>
        public Matrix ViewMatrix
        {
            get { return viewMatrix; }
            set { viewMatrix = value; }
        }

        /// <summary>
        /// Gets or sets world matrix.
        /// </summary>
        public Matrix WorldMatrix
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
            set { SetPosition(value); }
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
        /// Gets or sets the yaw rotation of the camera.
        /// </summary>
        public float Yaw
        {
            get { return cameraYaw; }
            set { cameraYaw = MathHelper.WrapAngle(value); }
        }

        /// <summary>
        /// Gets or sets the pitch rotation of the camera.
        /// </summary>
        public float Pitch
        {
            get { return cameraPitch; }
            set
            {
                cameraPitch = value;
                if (cameraPitch > MathHelper.PiOver2 * .99f)
                    cameraPitch = MathHelper.PiOver2 * .99f;
                else if (cameraPitch < -MathHelper.PiOver2 * .99f)
                    cameraPitch = -MathHelper.PiOver2 * .99f;
            }
        }

        /// <summary>
        /// Gets or sets control flag.
        /// </summary>
        public bool UseMovementControls
        {
            get { return useMovementControls; }
            set { useMovementControls = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Camera()
        {
            viewFrustum = new BoundingFrustum(Matrix.Identity);
            cameraMovementBounds.Min = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            cameraMovementBounds.Max = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            cameraPosition = Vector3.Zero;
            ResetReference();
        }

        /// <summary>
        /// Copy constructor (shallow).
        /// </summary>
        /// <param name="camera">Source Camera.</param>
        public Camera(Camera camera)
        {
            Copy(camera, this);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copy the source camera to the destination camera.
        ///  This is a shallow copy. Class-based members will be
        ///  copied by reference. Other members will be copied
        ///  by value.
        /// </summary>
        /// <param name="src">Source Camera.</param>
        /// <param name="dst">Destination Camera.</param>
        static public void Copy(Camera src, Camera dst)
        {
            // Copy all variables from source camera.
            dst.nearPlaneDistance = src.nearPlaneDistance;
            dst.farPlaneDistance = src.farPlaneDistance;
            dst.projectionMatrix = src.projectionMatrix;
            dst.viewMatrix = src.viewMatrix;
            dst.worldMatrix = src.worldMatrix;
            dst.cameraYaw = src.cameraYaw;
            dst.cameraPitch = src.cameraPitch;
            dst.cameraMovementBounds = src.cameraMovementBounds;
            dst.cameraPosition = src.cameraPosition;
            dst.cameraReference = src.cameraReference;
            dst.cameraUpVector = src.cameraUpVector;
            dst.cameraTransformedReference = src.cameraTransformedReference;
            dst.cameraTarget = src.cameraTarget;
            dst.viewFrustum = src.viewFrustum;
        }

        /// <summary>
        /// Updates view and frustum matrices.
        /// </summary>
        public void Update()
        {
            // Update view matrix
            Matrix.CreateLookAt(ref cameraPosition, ref cameraTarget, ref cameraUpVector, out viewMatrix);

            // Update frustum
            viewFrustum.Matrix = viewMatrix * projectionMatrix;

            // Create world matrix for camera
            worldMatrix = Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(cameraYaw), MathHelper.ToRadians(cameraPitch), 0);
        }

        /// <summary>
        /// Resets camera yaw and pitch.
        /// </summary>
        public void ResetReference()
        {
            cameraYaw = 0f;
            cameraPitch = 0f;
            cameraReference = Vector3.Forward;
            cameraTransformedReference = cameraReference;
        }

        /// <summary>
        /// Translate camera by coordinates.
        /// </summary>
        /// <param name="X">X amount.</param>
        /// <param name="Y">Y amount.</param>
        /// <param name="Z">Z amount.</param>
        public void Translate(float X, float Y, float Z)
        {
            cameraPosition.X = cameraPosition.X + X;
            cameraPosition.Y = cameraPosition.Y + Y;
            cameraPosition.Z = cameraPosition.Z + Z;
            EnforceBounds();
            UpdateTarget();
        }

        /// <summary>
        /// Translate camera by vector.
        /// </summary>
        /// <param name="translation">Translation vector.</param>
        public void Translate(Vector3 translation)
        {
            cameraPosition += translation;
            EnforceBounds();
            UpdateTarget();
        }

        /// <summary>
        /// Transform camera.
        /// </summary>
        /// <param name="yaw">Amount to change yaw in degrees.</param>
        /// <param name="pitch">Amount to change pitch in degrees.</param>
        /// <param name="movement">Distance to move from current position.</param>
        public void Transform(float yaw, float pitch, Vector3 movement)
        {
            //  Apply yaw
            cameraYaw += yaw;
            if (cameraYaw > 360)
                cameraYaw -= 360;
            else if (cameraYaw < 0)
                cameraYaw += 360;

            // Apply pitch
            cameraPitch += pitch;
            if (cameraPitch > 89)
                cameraPitch = 89;
            if (cameraPitch < -89)
                cameraPitch = -89;

            // Look rotation
            Matrix lookRotation = Matrix.Identity;
            Matrix.CreateRotationY(MathHelper.ToRadians(cameraYaw), out lookRotation);
            lookRotation = Matrix.CreateRotationX(MathHelper.ToRadians(cameraPitch)) * lookRotation;

            // Apply movement
            if (movement != Vector3.Zero)
            {
                Vector3.Transform(ref movement, ref lookRotation, out movement);
                cameraPosition += movement;
                EnforceBounds();
            }

            // Transform camera target
            Vector3.Transform(ref cameraReference, ref lookRotation, out cameraTransformedReference);
            UpdateTarget();
        }

        /// <summary>
        /// Centres camera within X-Z bounds, with a variable height.
        /// </summary>
        public void CentreInBounds(float height)
        {
            cameraPosition.X = cameraMovementBounds.Min.X + (cameraMovementBounds.Max.X - cameraMovementBounds.Min.X) / 2;
            cameraPosition.Y = height;
            cameraPosition.Z = cameraMovementBounds.Min.Z + (cameraMovementBounds.Max.Z - cameraMovementBounds.Min.Z) / 2;
            EnforceBounds();
            UpdateTarget();
        }

        /// <summary>
        /// Sets new aspect ratio and updates projection matrix.
        /// </summary>
        /// <param name="aspectRatio">New aspect ratio.</param>
        public void SetAspectRatio(float aspectRatio)
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                aspectRatio,
                nearPlaneDistance,
                farPlaneDistance);

            // Update
            Update();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets new camera position.
        /// </summary>
        /// <param name="position">Position vector.</param>
        private void SetPosition(Vector3 position)
        {
            cameraPosition.X = position.X;
            cameraPosition.Y = position.Y;
            cameraPosition.Z = position.Z;
            EnforceBounds();
            UpdateTarget();
        }

        /// <summary>
        /// Sets new camera target.
        /// </summary>
        private void UpdateTarget()
        {
            Vector3.Add(ref cameraPosition, ref cameraTransformedReference, out cameraTarget);
        }

        /// <summary>
        /// Ensures camera stays within movement bounds.
        /// </summary>
        private void EnforceBounds()
        {
            // Keep camera position within defined movement bounds
            if (cameraPosition.X < cameraMovementBounds.Min.X)
                cameraPosition.X = cameraMovementBounds.Min.X;
            if (cameraPosition.Y < cameraMovementBounds.Min.Y)
                cameraPosition.Y = cameraMovementBounds.Min.Y;
            if (cameraPosition.Z < cameraMovementBounds.Min.Z)
                cameraPosition.Z = cameraMovementBounds.Min.Z;
            if (cameraPosition.X > cameraMovementBounds.Max.X)
                cameraPosition.X = cameraMovementBounds.Max.X;
            if (cameraPosition.Y > cameraMovementBounds.Max.Y)
                cameraPosition.Y = cameraMovementBounds.Max.Y;
            if (cameraPosition.Z > cameraMovementBounds.Max.Z)
                cameraPosition.Z = cameraMovementBounds.Max.Z;
        }

        #endregion

    }

}
