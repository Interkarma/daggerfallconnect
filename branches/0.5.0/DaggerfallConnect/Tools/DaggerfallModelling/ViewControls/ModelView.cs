// Project:         DaggerfallModelling
// Description:     Explore and export 3D models from Daggerfall.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using XNALibrary;

#endregion

namespace DaggerfallModelling.ViewControls
{

    /// <summary>
    /// Explore a single model.
    /// </summary>
    public class ModelView : ViewBase
    {

        #region Class Variables

        // Model
        ModelManager.Model model;
        RenderableBoundingBox renderableBoundingBox;

        // Appearance
        private Color modelViewBackgroundColor = Color.LightGray;

        // XNA
        private VertexDeclaration modelVertexDeclaration;
        private BasicEffect modelEffect;
        private float nearPlaneDistance = 1.0f;
        private float farPlaneDistance = 10000.0f;
        private Matrix projectionMatrix;
        private Matrix viewMatrix;
        private Vector3 cameraPosition = new Vector3(0, 0, 2000);
        private Vector3 cameraReference = new Vector3(0, 0, -1);
        private Vector3 cameraUpVector = new Vector3(0, 1, 0);

        // Movement
        float rotationStep = 0.005f;
        float translationStep = 2500.0f;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public ModelView(ViewHost host)
            : base(host)
        {
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Called by host when view should initialise.
        /// </summary>
        public override void Initialize()
        {
            // Create vertex declaration
            modelVertexDeclaration = new VertexDeclaration(host.GraphicsDevice, VertexPositionNormalTexture.VertexElements);

            // Setup model basic effect
            modelEffect = new BasicEffect(host.GraphicsDevice, null);
            modelEffect.World = Matrix.Identity;
            modelEffect.TextureEnabled = true;
            modelEffect.PreferPerPixelLighting = true;
            modelEffect.EnableDefaultLighting();
            modelEffect.AmbientLightColor = new Vector3(0.4f, 0.4f, 0.4f);
            modelEffect.SpecularColor = new Vector3(0.2f, 0.2f, 0.2f);

            // Setup camera
            float aspectRatio = (float)host.GraphicsDevice.Viewport.Width / (float)host.GraphicsDevice.Viewport.Height;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, nearPlaneDistance, farPlaneDistance);
            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraPosition + cameraReference, cameraUpVector);

            // TEST: Load a model
            //LoadModel(455);
            LoadModel(58051);
        }

        /// <summary>
        /// Called by host when view should update animation.
        /// </summary>
        public override void Tick()
        {
        }

        /// <summary>
        /// Called by host when view should redraw.
        /// </summary>
        public override void Draw()
        {
            // Clear display
            host.GraphicsDevice.Clear(modelViewBackgroundColor);

            // Draw model
            DrawSingleModel();

            // Draw bounding box
            DrawBoundingBox();
        }

        /// <summary>
        /// Called by host when view should redraw.
        /// </summary>
        public override void Resize()
        {
            // Host must be ready as matrix depends on host control dimensions
            if (!host.IsReady)
                return;

            // Create projection matrix
            float aspectRatio = (float)host.Width / (float)host.Height;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, nearPlaneDistance, farPlaneDistance);

            // Request redraw now
            host.Refresh();
        }

        /// <summary>
        /// Called when view should track mouse movement.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public override void OnMouseMove(MouseEventArgs e)
        {
            if (host.LeftMouseDown)
            {
                float amountX = (MathHelper.ToDegrees(host.MousePosDelta.X) * rotationStep) * host.TimeDelta;
                float amountY = (MathHelper.ToDegrees(host.MousePosDelta.Y) * rotationStep) * host.TimeDelta;
                Matrix x = Matrix.CreateRotationX(amountY);
                Matrix y = Matrix.CreateRotationY(amountX);
                modelEffect.World *= (x * y);
            }
        }

        /// <summary>
        /// Called when the mouse wheel is scrolled.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public override void OnMouseWheel(MouseEventArgs e)
        {
            float amount = (((float)e.Delta / 120.0f) * translationStep) * host.TimeDelta;
            TranslateCamera(0, 0, -amount);
        }

        /// <summary>
        /// Called when a mouse button is pressed.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public override void OnMouseDown(MouseEventArgs e)
        {
        }

        /// <summary>
        /// Called when a mouse button is released.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public override void OnMouseUp(MouseEventArgs e)
        {
        }

        /// <summary>
        /// Called when mouse enters client area.
        /// </summary>
        /// <param name="e">EventArgs</param>
        public override void OnMouseEnter(EventArgs e)
        {
        }

        /// <summary>
        /// Called when mouse leaves client area.
        /// </summary>
        /// <param name="e">EventArgs</param>
        public override void OnMouseLeave(EventArgs e)
        {
        }

        #endregion

        #region Drawing Methods

        private void DrawSingleModel()
        {
            // Exit if no model loaded
            if (model.Vertices == null)
                return;

            // Set vertex declaration
            host.GraphicsDevice.VertexDeclaration = modelVertexDeclaration;

            // Set view and projection matrices
            modelEffect.View = viewMatrix;
            modelEffect.Projection = projectionMatrix;

            foreach (var submesh in model.SubMeshes)
            {
                modelEffect.Texture = host.TextureManager.GetTexture(submesh.TextureKey);

                modelEffect.Begin();
                modelEffect.CurrentTechnique.Passes[0].Begin();

                host.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                    model.Vertices, 0, model.Vertices.Length,
                    submesh.Indices, 0, submesh.Indices.Length / 3);

                modelEffect.CurrentTechnique.Passes[0].End();
                modelEffect.End();
            }
        }

        private void DrawBoundingBox()
        {
            renderableBoundingBox.Draw(viewMatrix, projectionMatrix, modelEffect.World);
        }

        #endregion

        #region Model Management

        /// <summary>
        /// Loads a model to view.
        /// </summary>
        /// <param name="id">ID of model.</param>
        private void LoadModel(int id)
        {
            // Load the model
            model = host.ModelManager.GetModel(id, false);

            // Load texture for each submesh.
            for (int sm = 0; sm < model.SubMeshes.Length; sm++)
            {
                // Load textures
                model.SubMeshes[sm].TextureKey =
                    host.TextureManager.LoadTexture(
                    model.SubMeshes[sm].TextureArchive,
                    model.SubMeshes[sm].TextureRecord);
            }

            // Centre model
            Vector3 Min = model.BoundingBox.Min;
            Vector3 Max = model.BoundingBox.Max;
            float transX = (float)(Min.X + ((Max.X - Min.X) / 2));
            float transY = (float)(Min.Y + ((Max.Y - Min.Y) / 2));
            float transZ = (float)(Min.Z + ((Max.Z - Min.Z) / 2));
            Matrix matrix = Matrix.CreateTranslation(-transX, -transY, -transZ);

            // Apply matrix to model
            model = host.ModelManager.TransformModel(ref model, matrix);

            // Create renderable bounding box
            renderableBoundingBox = new RenderableBoundingBox(host.GraphicsDevice, model.BoundingBox);
        }

        #endregion

        #region Camera Methods

        private void TranslateCamera(float X, float Y, float Z)
        {
            // Translate camera vector
            cameraPosition.X += X;
            cameraPosition.Y += Y;
            cameraPosition.Z += Z;

            // Cap Z
            if (cameraPosition.Z < nearPlaneDistance)
                cameraPosition.Z = nearPlaneDistance;
            if (cameraPosition.Z > farPlaneDistance)
                cameraPosition.Z = farPlaneDistance;

            // Update view matrix
            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraPosition + cameraReference, cameraUpVector);
        }

        #endregion

    }

}
