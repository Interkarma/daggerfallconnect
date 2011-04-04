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
    /// Explore a location from a single block to full cities and dungeons.
    /// </summary>
    public class LocationView : ContentViewBase
    {

        #region Class Variables

        // Appearance
        private Color backgroundColor = Color.LightGray;

        // XNA
        private VertexDeclaration modelVertexDeclaration;
        private BasicEffect modelEffect;
        private float nearPlaneDistance = 1.0f;
        private float farPlaneDistance = 50000.0f;
        private Matrix projectionMatrix;
        private Matrix viewMatrix;
        private Matrix worldMatrix;
        private Vector3 cameraPosition = new Vector3(2048, 1024, 6000);
        private Vector3 cameraReference = new Vector3(0, 0, -1);
        private Vector3 cameraUpVector = new Vector3(0, 1, 0);

        // Drawing
        RenderableBoundingBox renderableBounds;

        // Movement
        float rotationStep = 0.005f;
        float translationStep = 2500.0f;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public LocationView(ContentViewHost host)
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
            worldMatrix = Matrix.Identity;

            // Setup bounding box renderer
            renderableBounds = new RenderableBoundingBox(host.GraphicsDevice);
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
            host.GraphicsDevice.Clear(backgroundColor);

            /*
            // Draw ground plane
            DrawRmbGroundPlane(ref testBlock, ref modelEffect);

            // Render block bounding box
            renderableBounds.Draw(testBlock.BoundingBox, viewMatrix, projectionMatrix, worldMatrix);

            // Render models and bounding boxes
            foreach (var item in testBlock.Models)
            {
                ModelManager.Model model = host.ModelManager.GetModel((int)item.ModelId);
                DrawModel(ref model, item.Matrix);
                renderableBounds.Draw(model.BoundingBox, viewMatrix, projectionMatrix, item.Matrix * worldMatrix);
            }
            */
        }

        /// <summary>
        /// Called by host when view should redraw.
        /// </summary>
        public override void Resize()
        {
            // Host must be ready as projection matrix depends on host control dimensions
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
                worldMatrix *= (x * y);
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

        #endregion

        #region Drawing Methods

        private void DrawModel(ref ModelManager.Model model, Matrix matrix)
        {
            // Exit if no model loaded
            if (model.Vertices == null)
                return;

            // Set vertex declaration
            host.GraphicsDevice.VertexDeclaration = modelVertexDeclaration;

            // Set view and projection matrices
            modelEffect.View = viewMatrix;
            modelEffect.Projection = projectionMatrix;
            modelEffect.World = matrix * worldMatrix;

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

        private void DrawRmbGroundPlane(ref BlockManager.Block block, ref BasicEffect effect)
        {
            // Build ground plane if not already built
            if (block.GroundPlaneVertices == null)
                host.BlockManager.BuildRmbGroundPlane(host.TextureManager, ref block);

            // Set vertex declaration
            host.GraphicsDevice.VertexDeclaration = modelVertexDeclaration;

            // Set view and projection matrices
            modelEffect.View = viewMatrix;
            modelEffect.Projection = projectionMatrix;
            modelEffect.World = worldMatrix;

            // Set test texture
            effect.Texture = host.TextureManager.TerrainAtlas;

            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();

            host.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, block.GroundPlaneVertices, 0, 512);

            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }

        #endregion

        #region Block Management

        /*
        private void LoadTestBlock(string name)
        {
            // Load block
            testBlock = host.BlockManager.LoadBlock(name);

            // Load block models
            float maxHeight = 0;
            for (int i = 0; i < testBlock.Models.Count; i++)
            {
                // Get model info
                BlockManager.ModelInfo info = testBlock.Models[i];

                // Load model resource
                ModelManager.Model model;
                host.ModelManager.LoadModel((int)info.ModelId, out model);

                // Load texture resources for this model
                for (int sm = 0; sm < model.SubMeshes.Length; sm++)
                {
                    model.SubMeshes[sm].TextureKey = host.TextureManager.LoadTexture(
                        model.SubMeshes[sm].TextureArchive,
                        model.SubMeshes[sm].TextureRecord);
                }

                // Set model bounding box
                info.BoundingBox = model.BoundingBox;

                // Track max height
                float height = model.BoundingBox.Max.Y - model.BoundingBox.Min.Y;
                if (height > maxHeight)
                    maxHeight = height;

                // Set model info
                testBlock.Models[i] = info;
            }

            // Save correct max height in block
            Vector3 max = new Vector3(
                testBlock.BoundingBox.Max.X,
                maxHeight,
                testBlock.BoundingBox.Max.Z);
            testBlock.BoundingBox.Max = max;
        }
        */

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

        #region Map Management
        #endregion

    }

}
