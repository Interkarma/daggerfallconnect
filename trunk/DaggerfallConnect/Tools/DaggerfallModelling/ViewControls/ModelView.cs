// Project:         DaggerfallModelling
// Description:     Explore and export 3D models from Daggerfall.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
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
        private ModelManager.ModelData currentModel;
        private int currentModelIndex = 0;

        // Appearance
        private Color modelViewBackgroundColor = Color.LightGray;

        // XNA
        private VertexDeclaration modelVertexDeclaration;
        private VertexDeclaration lineVertexDeclaration;
        private BasicEffect modelEffect;
        private BasicEffect lineEffect;
        private float nearPlaneDistance = 1.0f;
        private float farPlaneDistance = 5000.0f;
        private Matrix projectionMatrix;
        private Matrix viewMatrix;
        private Vector3 cameraPosition;
        private Vector3 cameraReference = new Vector3(0, 0, -1);
        private Vector3 cameraUpVector = Vector3.Up;

        // Movement
        private const float spinRate = 0.2f;
        private Matrix modelRotation = Matrix.Identity;
        private Matrix modelTranslation = Matrix.Identity;
        private float cameraStep = 0.5f;
        private float wheelStep = 10.0f;

        // Bounds
        VertexPositionColor[] planeLines = new VertexPositionColor[64];
        RenderableBoundingBox renderableBounds;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets current model in view.
        /// </summary>
        public uint? ModelID
        {
            get { return GetModel(); }
            set { SetModel(value); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public ModelView(ViewHost host)
            : base(host)
        {
            currentModel = new ModelManager.ModelData();

            // Start in normal camera mode
            CameraMode = CameraModes.None;
            renderableBounds = new RenderableBoundingBox(host.GraphicsDevice);
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

            // Create vertex declaration
            lineVertexDeclaration = new VertexDeclaration(host.GraphicsDevice, VertexPositionColor.VertexElements);

            // Setup line BasicEffect
            lineEffect = new BasicEffect(host.GraphicsDevice, null);
            lineEffect.LightingEnabled = false;
            lineEffect.TextureEnabled = false;
            lineEffect.VertexColorEnabled = true;

            // Setup camera
            float aspectRatio = (float)host.GraphicsDevice.Viewport.Width / (float)host.GraphicsDevice.Viewport.Height;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, nearPlaneDistance, farPlaneDistance);
            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraPosition + cameraReference, cameraUpVector);

            // Load default model
            ModelID = host.ModelManager.Arch3dFile.GetRecordId(0);
        }

        /// <summary>
        /// Called by host when view should update animation.
        /// </summary>
        public override void Update()
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
        /// Called by host when view should resize.
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
            // Update mouse ray
            host.UpdateMouseRay(e.X, e.Y, viewMatrix, projectionMatrix);

            if (host.RightMouseDown)
            {
                // Adjust model rotation
                float modelYaw = MathHelper.ToRadians((float)host.MousePosDelta.X * spinRate);
                float modelPitch = MathHelper.ToRadians((float)host.MousePosDelta.Y * spinRate);
                Matrix rotation = Matrix.CreateRotationY(modelYaw) * Matrix.CreateRotationX(modelPitch);
                modelRotation *= rotation;
            }
            else if (host.MiddleMouseDown)
            {
                // Adjust camera X-Y translation
                TranslateCamera(
                        (float)-host.MousePosDelta.X * cameraStep,
                        (float)host.MousePosDelta.Y * cameraStep,
                        0.0f);
            }
        }

        /// <summary>
        /// Called when the mouse wheel is scrolled.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public override void OnMouseWheel(MouseEventArgs e)
        {
            // Adjust camera Z translation
            float amount = ((float)e.Delta / 120.0f) * wheelStep;
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
        /// Called when filtered models array has been changed.
        /// </summary>
        public override void FilteredModelsChanged()
        {
            if (host.FilteredModelsArray == null)
            {
                currentModelIndex = 0;
                SetModel(host.ModelManager.Arch3dFile.GetRecordId(currentModelIndex));
                UpdateStatusMessage();
            }
            else
            {
                currentModelIndex = 0;
                SetModel(host.FilteredModelsArray[currentModelIndex]);
                UpdateStatusMessage();
            }
        }

        /// <summary>
        /// Called when the view is resumed after being inactive.
        ///  Allows view to perform any layout or other requirements before redraw.
        /// </summary>
        public override void ResumeView()
        {
            base.ResumeView();
            host.ModelManager.CacheModelData = false;
            UpdateStatusMessage();
            UpdateProjectionMatrix();
            LayoutModel();
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

            // Set anisotropy
            if (host.GraphicsDevice.GraphicsDeviceCapabilities.MaxAnisotropy > 0)
            {
                host.GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Anisotropic;
                host.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Anisotropic;
                host.GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Linear;
                host.GraphicsDevice.SamplerStates[0].MaxAnisotropy = host.GraphicsDevice.GraphicsDeviceCapabilities.MaxAnisotropy;
            }
            else
            {
                host.GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Linear;
                host.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Linear;
                host.GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Linear;
                host.GraphicsDevice.SamplerStates[0].MaxAnisotropy = 0;
            }

            // Set vertex declaration
            host.GraphicsDevice.VertexDeclaration = modelVertexDeclaration;

            // Set view and projection matrices
            modelEffect.View = viewMatrix;
            modelEffect.Projection = projectionMatrix;

            // Transform world
            modelEffect.World = modelRotation * modelTranslation;

            // Set vertex buffer
            host.GraphicsDevice.Vertices[0].SetSource(
                currentModel.VertexBuffer,
                0,
                VertexPositionNormalTexture.SizeInBytes);

            // Set index buffer
            host.GraphicsDevice.Indices = currentModel.IndexBuffer;

            // Test for intersection
            bool insideBoundingSphere;
            int subMeshResult, planeResult;
            Intersection.RayIntersectsDFMesh(
                host.MouseRay,
                modelEffect.World,
                ref currentModel,
                out insideBoundingSphere,
                out subMeshResult,
                out planeResult);

            modelEffect.Begin();
            modelEffect.CurrentTechnique.Passes[0].Begin();

            // Draw submeshes
            foreach (var subMesh in currentModel.SubMeshes)
            {
                modelEffect.Texture = host.TextureManager.GetTexture(subMesh.TextureKey);
                modelEffect.CommitChanges();

                host.GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    0,
                    currentModel.Vertices.Length,
                    subMesh.StartIndex,
                    subMesh.PrimitiveCount);
            }

            modelEffect.CurrentTechnique.Passes[0].End();
            modelEffect.End();

            // Only do this when mouse inside bounding sphere
            if (insideBoundingSphere)
            {
                if (subMeshResult != -1)
                {
                    DrawNativeFace(Color.White, subMeshResult, planeResult, modelEffect.World);

                    uint id = currentModel.DFMesh.ObjectId;
                    int index = host.ModelManager.Arch3dFile.GetRecordIndex(id);

                    host.StatusMessage = string.Format(
                        "ModelIndex={0}, ModelID={1}, TextureArchive={2}, TextureRecord={3}",
                        index,
                        id,
                        currentModel.DFMesh.SubMeshes[subMeshResult].TextureArchive,
                        currentModel.DFMesh.SubMeshes[subMeshResult].TextureRecord);
                }
            }
        }

        /// <summary>
        /// Draw a native face as a line list.
        /// </summary>
        /// <param name="color">Line color.</param>
        /// <param name="subMesh">SubMesh index.</param>
        /// <param name="plane">Plane index.</param>
        /// <param name="matrix">World transform.</param>
        private void DrawNativeFace(Color color, int subMesh, int plane, Matrix matrix)
        {
            // Exit if indices not set
            if (subMesh == -1 || plane == -1)
                return;

            // Build line primitives for this face
            int lineCount = 0;
            Vector3 vertex1, vertex2;
            DFMesh.DFPoint[] points = currentModel.DFMesh.SubMeshes[subMesh].Planes[plane].Points;
            for (int p = 0; p < points.Length - 1; p++)
            {
                // Add first point
                vertex1.X = points[p].X;
                vertex1.Y = -points[p].Y;
                vertex1.Z = -points[p].Z;
                planeLines[lineCount].Color = color;
                planeLines[lineCount++].Position = vertex1;

                // Add second point
                vertex2.X = points[p+1].X;
                vertex2.Y = -points[p+1].Y;
                vertex2.Z = -points[p+1].Z;
                planeLines[lineCount].Color = color;
                planeLines[lineCount++].Position = vertex2;
            }

            // Join final point to first point
            vertex1.X = points[0].X;
            vertex1.Y = -points[0].Y;
            vertex1.Z = -points[0].Z;
            planeLines[lineCount].Color = color;
            planeLines[lineCount++].Position = vertex1;
            vertex2.X = points[points.Length - 1].X;
            vertex2.Y = -points[points.Length - 1].Y;
            vertex2.Z = -points[points.Length - 1].Z;
            planeLines[lineCount].Color = color;
            planeLines[lineCount++].Position = vertex2;

            // Set vertex declaration
            host.GraphicsDevice.VertexDeclaration = lineVertexDeclaration;

            // Set view and projection matrices
            lineEffect.View = viewMatrix;
            lineEffect.Projection = projectionMatrix;
            lineEffect.World = matrix;

            // Set render states
            host.GraphicsDevice.RenderState.DepthBufferEnable = false;
            host.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            host.GraphicsDevice.RenderState.AlphaTestEnable = false;

            lineEffect.Begin();
            lineEffect.CurrentTechnique.Passes[0].Begin();

            // Draw lines
            host.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, planeLines, 0, lineCount / 2);

            lineEffect.CurrentTechnique.Passes[0].End();
            lineEffect.End();
        }

        #endregion

        #region Model Management

        /// <summary>
        /// Gets the current model ID.
        /// </summary>
        /// <returns>ModelID.</returns>
        private uint? GetModel()
        {
            if (host.FilteredModelsArray != null)
            {
                if (host.FilteredModelsArray.Length > 0)
                    return host.FilteredModelsArray[currentModelIndex];
            }
            else
            {
                return host.ModelManager.Arch3dFile.GetRecordId(currentModelIndex);
            }

            return null;
        }

        /// <summary>
        /// View the specified model.
        /// </summary>
        /// <param name="id"></param>
        private void SetModel(uint? id)
        {
            // Do nothing if id is null
            if (id == null)
                return;

            // Sync index to id from filtered list or entire database
            currentModelIndex = -1;
            if (host.FilteredModelsArray != null)
            {
                // Find index in filtered list
                for (int i = 0; i < host.FilteredModelsArray.Length; i++)
                {
                    if (host.FilteredModelsArray[i] == id)
                    {
                        currentModelIndex = i;
                        break;
                    }
                }
            }
            else
            {
                currentModelIndex = host.ModelManager.Arch3dFile.GetRecordIndex((uint)id);
            }

            // Display model
            LoadModel(id);
            LayoutModel();
        }

        /// <summary>
        /// Positions current model.
        /// </summary>
        private void LayoutModel()
        {
            // Update view matrix
            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraPosition + cameraReference, cameraUpVector);
        }

        /// <summary>
        /// Loads a model for view.
        /// </summary>
        /// <param name="id">ModelID.</param>
        private void LoadModel(uint? id)
        {
            // Load the model
            currentModel = host.ModelManager.GetModelData(id.Value);

            // Load texture for each submesh.
            for (int sm = 0; sm < currentModel.SubMeshes.Length; sm++)
            {
                // Load textures
                currentModel.SubMeshes[sm].TextureKey =
                    host.TextureManager.LoadTexture(
                    currentModel.DFMesh.SubMeshes[sm].TextureArchive,
                    currentModel.DFMesh.SubMeshes[sm].TextureRecord,
                    TextureManager.TextureCreateFlags.ApplyClimate |
                    TextureManager.TextureCreateFlags.MipMaps |
                    TextureManager.TextureCreateFlags.PowerOfTwo);
            }

            // Centre camera and reset model rotation
            Vector3 Min = currentModel.BoundingBox.Min;
            Vector3 Max = currentModel.BoundingBox.Max;
            cameraPosition.X = (float)(Min.X + ((Max.X - Min.X) / 2));
            cameraPosition.Y = (float)(Min.Y + ((Max.Y - Min.Y) / 2));
            cameraPosition.Z = 700.0f + (Max.Z - Min.Z);
            modelRotation = Matrix.Identity;
        }

        #endregion

        #region Camera Methods

        private void TranslateCamera(float X, float Y, float Z)
        {
            Vector3 Min = currentModel.BoundingBox.Min;
            Vector3 Max = currentModel.BoundingBox.Max;

            // Translate camera vector
            cameraPosition.X += X;
            cameraPosition.Y += Y;
            cameraPosition.Z += Z;

            // Cap X
            float xAmount = (Max.X - Min.X) * 4;
            if (cameraPosition.X < -xAmount)
                cameraPosition.X = -xAmount;
            if (cameraPosition.X > xAmount)
                cameraPosition.X = xAmount;

            // Cap Y
            float yAmount = (Max.Y - Min.Y) * 4;
            if (cameraPosition.Y < -yAmount)
                cameraPosition.Y = -yAmount;
            if (cameraPosition.Y > yAmount)
                cameraPosition.Y = yAmount;

            // Cap Z
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
            int count;
            string message;

            // State how many models we are viewing
            if (host.FilteredModelsArray == null)
            {
                count = host.ModelManager.Arch3dFile.Count;
                message = string.Format("Exploring all {0} models in ARCH3D.BSA.", count);
            }
            else
            {
                count = host.FilteredModelsArray.Length;
                message = string.Format("Exploring filtered list of {0} models.", count);
            }

            // Add position in list
            message += string.Format(" You are viewing model {0}, which has an ID of {1}.", currentModelIndex, currentModel.DFMesh.ObjectId);

            // Set the message
            host.StatusMessage = message;
        }

        #endregion

    }

}
