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
        private ModelManager.Model currentModel;
        private int currentModelIndex = 0;
        private int currentModelId = -1;

        // Appearance
        private Color modelViewBackgroundColor = Color.LightGray;

        // XNA
        private VertexDeclaration modelVertexDeclaration;
        private BasicEffect modelEffect;
        private float nearPlaneDistance = 1.0f;
        private float farPlaneDistance = 5000.0f;
        private Matrix projectionMatrix;
        private Matrix viewMatrix;
        private Vector3 cameraPosition;
        private Vector3 cameraReference = new Vector3(0, 0, -1);
        private Vector3 cameraUpVector = Vector3.Up;

        // Movement
        private Matrix modelRotation = Matrix.Identity;
        private Matrix modelTranslation = Matrix.Identity;
        private float translationStep = 10.0f;

        // Models list
        private bool useFilteredModels = false;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public ModelView(ViewHost host)
            : base(host)
        {
            // Start in normal camera mode
            CameraMode = CameraModes.None;
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
        }

        /// <summary>
        /// Called by host when view should redraw.
        /// </summary>
        public override void Resize()
        {
            // Update projection matrix and refresh
            UpdateProjectionMatrix();
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
                // TODO: Support multiple camera modes

                // Adjust model rotation for normal camera
                float modelYaw = MathHelper.ToRadians((float)host.MousePosDelta.X * 0.5f);
                float modelPitch = MathHelper.ToRadians((float)host.MousePosDelta.Y * 0.5f);
                Matrix rotation = Matrix.CreateRotationY(modelYaw) * Matrix.CreateRotationX(modelPitch);
                modelRotation *= rotation;
            }
        }

        /// <summary>
        /// Called when the mouse wheel is scrolled.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public override void OnMouseWheel(MouseEventArgs e)
        {
            float amount = ((float)e.Delta / 120.0f) * translationStep;
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
        /// Called when user double-clicks mouse.
        /// </summary>
        /// <param name="e">MouseEventArgs.</param>
        public override void OnMouseDoubleClick(MouseEventArgs e)
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

        /// <summary>
        /// Called when filtered models array has been changed.
        /// </summary>
        public override void FilteredModelsChanged()
        {
            if (host.FilteredModelsArray == null)
            {
                useFilteredModels = false;
                currentModelIndex = 0;
                LayoutModel();
                UpdateStatusMessage();
            }
            else
            {
                useFilteredModels = true;
                currentModelIndex = 0;
                LayoutModel();
                UpdateStatusMessage();
            }
        }

        /// <summary>
        /// Called when the view is resumed after being inactive.
        ///  Allows view to perform any layout or other requirements before redraw.
        /// </summary>
        public override void ResumeView()
        {
            UpdateProjectionMatrix();
            FilteredModelsChanged();
            host.Refresh();
        }

        #endregion

        #region Drawing Methods

        private void DrawSingleModel()
        {
            // Exit if no model loaded
            if (currentModel.Vertices == null)
                return;

            // Set render states
            host.GraphicsDevice.RenderState.DepthBufferEnable = true;
            host.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            host.GraphicsDevice.RenderState.AlphaTestEnable = false;
            host.GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            host.GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
            host.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            host.GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Anisotropic;
            host.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Anisotropic;
            host.GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Linear;
            host.GraphicsDevice.SamplerStates[0].MaxAnisotropy = host.GraphicsDevice.GraphicsDeviceCapabilities.MaxAnisotropy;

            // Set vertex declaration
            host.GraphicsDevice.VertexDeclaration = modelVertexDeclaration;

            // Set view and projection matrices
            modelEffect.View = viewMatrix;
            modelEffect.Projection = projectionMatrix;

            // Transform world in normal camera mode
            modelEffect.World = modelRotation * modelTranslation;

            // Draw submeshes
            foreach (var submesh in currentModel.SubMeshes)
            {
                modelEffect.Texture = host.TextureManager.GetTexture(submesh.TextureKey);

                modelEffect.Begin();
                modelEffect.CurrentTechnique.Passes[0].Begin();

                host.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                    currentModel.Vertices, 0, currentModel.Vertices.Length,
                    submesh.Indices, 0, submesh.Indices.Length / 3);

                modelEffect.CurrentTechnique.Passes[0].End();
                modelEffect.End();
            }
        }

        #endregion

        #region Model Management

        /// <summary>
        /// Loads and positions current model.
        /// </summary>
        private void LayoutModel()
        {
            // Load model based on source
            if (useFilteredModels)
            {
                if (host.FilteredModelsArray != null)
                {
                    if (host.FilteredModelsArray.Length > 0)
                        LoadModel(host.FilteredModelsArray[currentModelIndex]);
                }
            }
            else
            {
                LoadModel((int)host.ModelManager.Arch3dFile.GetRecordId(currentModelIndex));
            }

            // Update view matrix
            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraPosition + cameraReference, cameraUpVector);
        }

        /// <summary>
        /// Loads a model for view. Should only be called from LayoutModel.
        /// </summary>
        /// <param name="id">ID of model.</param>
        private void LoadModel(int id)
        {
            // Do nothing if model already loaded
            if (currentModelId == id)
                return;

            // Load the model
            currentModel = host.ModelManager.GetModel(id, false);

            // Load texture for each submesh.
            for (int sm = 0; sm < currentModel.SubMeshes.Length; sm++)
            {
                // Load textures
                currentModel.SubMeshes[sm].TextureKey =
                    host.TextureManager.LoadTexture(
                    currentModel.SubMeshes[sm].TextureArchive,
                    currentModel.SubMeshes[sm].TextureRecord);
            }

            // Centre model
            Vector3 Min = currentModel.BoundingBox.Min;
            Vector3 Max = currentModel.BoundingBox.Max;
            float transX = (float)(Min.X + ((Max.X - Min.X) / 2));
            float transY = (float)(Min.Y + ((Max.Y - Min.Y) / 2));
            float transZ = (float)(Min.Z + ((Max.Z - Min.Z) / 2));
            Matrix matrix = Matrix.CreateTranslation(-transX, -transY, -transZ);

            // Apply matrix to model
            currentModel = host.ModelManager.TransformModel(ref currentModel, matrix);

            // Store current model id
            currentModelId = id;

            // Reset camera position and model rotation
            cameraPosition.X = 0.0f;
            cameraPosition.Y = 0.0f;
            cameraPosition.Z = 600.0f + (Max.Z - Min.Z);
            modelRotation = Matrix.Identity;
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
            Vector3 Min = currentModel.BoundingBox.Min;
            Vector3 Max = currentModel.BoundingBox.Max;
            if (cameraPosition.Z < nearPlaneDistance)
                cameraPosition.Z = nearPlaneDistance;
            if (cameraPosition.Z > farPlaneDistance - (Max.Z - Min.Z))
                cameraPosition.Z = farPlaneDistance - (Max.Z - Min.Z);

            // Update view matrix
            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraPosition + cameraReference, cameraUpVector);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates projection matrix to current view size.
        /// </summary>
        private void UpdateProjectionMatrix()
        {
            // Create projection matrix
            if (host.IsReady)
            {
                float aspectRatio = (float)host.Width / (float)host.Height;
                projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, nearPlaneDistance, farPlaneDistance);
            }
        }

        /// <summary>
        /// Updates status message.
        /// </summary>
        private void UpdateStatusMessage()
        {
            // Set the message
            host.StatusMessage = string.Empty;
        }

        #endregion

    }

}
