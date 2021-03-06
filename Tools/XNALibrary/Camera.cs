﻿// Project:         XNALibrary
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

        // Clipping plane extents
        private float nearPlaneDistance = 1.0f;
        private float farPlaneDistance = 50000.0f;

        // Matrices
        private Matrix projectionMatrix = Matrix.Identity;
        private Matrix viewMatrix = Matrix.Identity;
        private Matrix worldMatrix = Matrix.Identity;

        // Camera
        private bool restrictVertical = false;
        private float eyeHeight = 64f;
        private float bodyRadius = 16f;
        private float cameraYaw = 0.0f;
        private float cameraPitch = 0.0f;
        private BoundingBox cameraMovementBounds;
        private BoundingSphere cameraBoundingSphere;
        private Vector3 cameraPosition;
        private Vector3 cameraReference = Vector3.Forward;
        private Vector3 cameraUpVector = Vector3.Up;
        private Vector3 cameraTransformedReference;
        private Vector3 cameraTarget;
        private BoundingFrustum viewFrustum;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets flag to restrict vertical movement.
        /// </summary>
        public bool RestrictVertical
        {
            get { return restrictVertical; }
            set { restrictVertical = value; }
        }

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
        /// Gets bounds of camera.
        /// </summary>
        public BoundingSphere BoundingSphere
        {
            get
            {
                cameraBoundingSphere.Center = cameraPosition;
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
        /// Gets or sets camera position.
        /// </summary>
        public Vector3 Position
        {
            get { return cameraPosition; }
            set { SetPosition(value); }
        }

        /// <summary>
        /// Gets or sets camera facing.
        /// </summary>
        public Vector3 Reference
        {
            get { return cameraReference; }
            set { SetReference(value); }
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
            cameraBoundingSphere = new BoundingSphere();
            cameraPosition = new Vector3(0, eyeHeight, 0);
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
            dst.restrictVertical = src.restrictVertical;
            dst.nearPlaneDistance = src.nearPlaneDistance;
            dst.farPlaneDistance = src.farPlaneDistance;
            dst.projectionMatrix = src.projectionMatrix;
            dst.viewMatrix = src.viewMatrix;
            dst.worldMatrix = src.worldMatrix;
            dst.eyeHeight = src.eyeHeight;
            dst.bodyRadius = src.bodyRadius;
            dst.cameraYaw = src.cameraYaw;
            dst.cameraPitch = src.cameraPitch;
            dst.cameraMovementBounds = src.cameraMovementBounds;
            dst.cameraBoundingSphere = src.cameraBoundingSphere;
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
        /// <param name="yaw">Amount to change yaw.</param>
        /// <param name="pitch">Amount to change pitch.</param>
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
                if (restrictVertical)
                {
                    // Restrict camera to X-Z dimensions
                    movement.Y = 0f;
                    Matrix moveRotation = Matrix.Identity;
                    Matrix.CreateRotationY(MathHelper.ToRadians(cameraYaw), out moveRotation);
                    Vector3.Transform(ref movement, ref moveRotation, out movement);
                    cameraPosition += movement;
                    EnforceBounds();
                }
                else
                {
                    // Camera can move freely in all dimensions
                    Vector3.Transform(ref movement, ref lookRotation, out movement);
                    cameraPosition += movement;
                    EnforceBounds();
                }
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
        /// Sets new camera reference.
        /// </summary>
        /// <param name="reference">Reference vector.</param>
        private void SetReference(Vector3 reference)
        {
            ResetReference();
            cameraReference = reference;
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
            if (cameraPosition.Y < cameraMovementBounds.Min.Y + eyeHeight)
                cameraPosition.Y = cameraMovementBounds.Min.Y + eyeHeight;
            if (cameraPosition.Z < cameraMovementBounds.Min.Z)
                cameraPosition.Z = cameraMovementBounds.Min.Z;
            if (cameraPosition.X > cameraMovementBounds.Max.X)
                cameraPosition.X = cameraMovementBounds.Max.X;
            if (cameraPosition.Y > cameraMovementBounds.Max.Y - eyeHeight)
                cameraPosition.Y = cameraMovementBounds.Max.Y - eyeHeight;
            if (cameraPosition.Z > cameraMovementBounds.Max.Z)
                cameraPosition.Z = cameraMovementBounds.Max.Z;
        }

        #endregion

    }

}
