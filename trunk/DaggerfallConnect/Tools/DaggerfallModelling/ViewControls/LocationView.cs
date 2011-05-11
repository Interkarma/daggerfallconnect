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
using DaggerfallModelling.Engine;
#endregion

namespace DaggerfallModelling.ViewControls
{

    /// <summary>
    /// Explore a location from a single block to full cities and dungeons.
    ///  This view takes a DFBlock or DFLocation to build scene layout.
    ///  Provides scene culling, state batching, picking, and collisions.
    /// </summary>
    public class LocationView : ViewBase
    {

        #region Class Variables

        // Components
        private Scene sceneManager;
        private Collision collisionManager;
        private Billboards billboardManager;
        private Sky skyManager;

        // Status message
        private string currentStatus = string.Empty;

        // Appearance
        private Color backgroundColor = Color.LightGray;
        private Color modelHighlightColor = Color.Gold;
        private Color doorHighlightColor = Color.Red;
        private Color actionHighlightColor = Color.CornflowerBlue;

        // XNA
        private VertexDeclaration modelVertexDeclaration;
        private VertexDeclaration lineVertexDeclaration;
        private BasicEffect lineEffect;
        private BasicEffect modelEffect;

        // Movement
        private Vector3 cameraVelocity;
        private float cameraStep = 5.0f;
        private float wheelStep = 100.0f;

        // Line drawing
        VertexPositionColor[] planeLines = new VertexPositionColor[64];

        // Bounding volume drawing
        private RenderableBoundingBox renderableBoundingBox;
        private RenderableBoundingSphere renderableBoundingSphere;

        #endregion

        #region Class Structures
        #endregion

        #region Properties

        /// <summary>
        /// Gets sky manager.
        /// </summary>
        public Sky SkyManager
        {
            get { return skyManager; }
        }

        /// <summary>
        /// Gets billboard manager.
        /// </summary>
        public Billboards BillboardManager
        {
            get { return billboardManager; }
        }

        /// <summary>
        /// Gets scene manager.
        /// </summary>
        public Scene SceneManager
        {
            get { return sceneManager; }
        }

        /// <summary>
        /// Gets collision manager.
        /// </summary>
        public Collision CollisionManager
        {
            get { return collisionManager; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public LocationView(ViewHost host)
            : base(host)
        {
            // Start in top-down camera mode
            CameraMode = CameraModes.TopDown;
            renderableBoundingBox = new RenderableBoundingBox(host.GraphicsDevice);
            renderableBoundingSphere = new RenderableBoundingSphere(host.GraphicsDevice);
        }

        #endregion

        #region Abstract Overrides

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

            // Create scene component
            sceneManager = new Scene(host, this);
            sceneManager.Initialize();
            sceneManager.Enabled = true;

            // Create collision component
            collisionManager = new Collision(host);
            collisionManager.Initialize();
            collisionManager.Camera = sceneManager.Camera;
            collisionManager.Enabled = true;

            // Create sky component
            skyManager = new Sky(host);
            skyManager.Initialize();
            skyManager.Camera = sceneManager.Camera;
            skyManager.Enabled = true;

            // Create billboard component
            billboardManager = new Billboards(host);
            billboardManager.Initialize();
            billboardManager.Camera = sceneManager.Camera;
            billboardManager.Enabled = true;
        }

        /// <summary>
        /// Called by host when view should update.
        /// </summary>
        public override void Update()
        {
            // Update scene
            sceneManager.Update();

            // Set component cameras
            collisionManager.Camera = sceneManager.Camera;
            skyManager.Camera = sceneManager.Camera;
            billboardManager.Camera = sceneManager.Camera;

            // Get current layout array
            int count;
            Scene.BlockPosition[] layout;
            sceneManager.GetLayoutArray(out layout, out count);

            // Update camera.
            // This polls input devices for movement.
            sceneManager.UpdateCamera();

            // Handle intersections and collision response
            collisionManager.SetLayout(layout, count);
            collisionManager.Update();

            // Apply camera changes after collision response
            sceneManager.Camera.ApplyChanges();

            // Update components
            skyManager.Update();
            billboardManager.Update();
        }

        /// <summary>
        /// Called by host when view should redraw.
        /// </summary>
        public override void Draw()
        {
            // Just clear device if no models batched.
            // This prevents other scene elements being drawn
            // without an environment.
            if (sceneManager.ModelBatchCount == 0)
            {
                host.GraphicsDevice.Clear(backgroundColor);
                return;
            }

            // Draw background
            DrawBackground();

            // Draw world geometry
            SetRenderStates();
            DrawBatches();

            // Highlight model under pointer
            HighlightModelUnderPointer();

            // Draw billboards
            billboardManager.Draw();
        }

        /// <summary>
        /// Called by host when view should resize.
        /// </summary>
        public override void Resize()
        {
            // Update projection matrix and refresh
            sceneManager.UpdateProjectionMatrix();
            sceneManager.Update();
            host.Refresh();
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Called when view should track mouse movement.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public override void OnMouseMove(MouseEventArgs e)
        {
            // Update mouse ray
            host.UpdateMouseRay(
                e.X, e.Y,
                sceneManager.Camera.View,
                sceneManager.Camera.Projection);

            // Top down camera movement
            if (CameraMode == CameraModes.TopDown)
            {
                // Scene dragging
                if (host.RightMouseDown)
                {
                    sceneManager.Camera.Translate(
                        (float)-host.MousePosDelta.X * cameraStep,
                        0f,
                        (float)-host.MousePosDelta.Y * cameraStep);
                }
            }
        }

        /// <summary>
        /// Called when the mouse wheel is scrolled.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public override void OnMouseWheel(MouseEventArgs e)
        {
            // Top down camera movement
            if (CameraMode == CameraModes.TopDown)
            {
                float amount = ((float)e.Delta / 120.0f) * wheelStep;
                sceneManager.Camera.Translate(0, -amount, 0);
            }
        }

        /// <summary>
        /// Called when a mouse button is pressed.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public override void OnMouseDown(MouseEventArgs e)
        {
            // Clear camera velocity for any mouse down event
            cameraVelocity = Vector3.Zero;
        }

        /// <summary>
        /// Called when a mouse button is released.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public override void OnMouseUp(MouseEventArgs e)
        {
            // Normal camera movement
            if (CameraMode == CameraModes.TopDown)
            {
                // Scene dragging
                if (e.Button == MouseButtons.Right)
                {
                    // Set scroll velocity on right mouse up
                    cameraVelocity = new Vector3(
                        -host.MouseVelocity.X * cameraStep,
                        0.0f,
                        -host.MouseVelocity.Y * cameraStep);

                    // Cap velocity at very small amounts to limit drifting
                    if (cameraVelocity.X > -cameraStep && cameraVelocity.X < cameraStep) cameraVelocity.X = 0.0f;
                    if (cameraVelocity.Z > -cameraStep && cameraVelocity.Z < cameraStep) cameraVelocity.Z = 0.0f;
                }
            }
        }

        /// <summary>
        /// Called when user double-clicks mouse.
        /// </summary>
        /// <param name="e">MouseEventArgs.</param>
        public override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (collisionManager.PointerOverModel != null)
            {
                host.ShowModelView(collisionManager.PointerOverModel.ModelID, Climate);
            }
        }

        /// <summary>
        /// Called when the view is resumed after being inactive.
        ///  Allows view to perform any layout or other requirements before redraw.
        /// </summary>
        public override void ResumeView()
        {
            // Climate swaps in dungeons now implemented yet.
            // Set climate type manually for now to ensure
            // dungeons do not use climate swaps.
            if (sceneManager.BatchMode == Scene.BatchModes.Exterior)
                host.TextureManager.Climate = base.Climate;
            else
                host.TextureManager.Climate = DFLocation.ClimateType.None;

            // Clear scroll velocity
            cameraVelocity = Vector3.Zero;

            // Resume view
            host.ModelManager.CacheModelData = true;
            sceneManager.UpdateProjectionMatrix();
            sceneManager.Update();
            UpdateStatusMessage();
            host.Refresh();
        }

        /// <summary>
        /// Called to change camera mode.
        /// </summary>
        /// <param name="mode">New camera mode.</param>
        protected override void OnChangeCameraMode(CameraModes cameraMode)
        {
            base.OnChangeCameraMode(cameraMode);

            // Clear camera velocity
            cameraVelocity = Vector3.Zero;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Moves active camera to X-Z origin of specified block.
        ///  Nothing happens if block is not found.
        /// </summary>
        /// <param name="name">Name of block.</param>
        public void MoveToBlock(int x, int z)
        {
            /*
            // Search for block in active layout
            int count;
            BlockPosition[] layout;
            Dictionary<int, int> layoutDict;
            GetLayoutArray(out layout, out count);
            GetLayoutDict(out layoutDict);
            BlockPosition foundBlock = new BlockPosition();
            int key = GetBlockKey(x, z);
            if (layoutDict.ContainsKey(key))
                foundBlock = layout[layoutDict[key]];
            else
                return;

            // Move active camera to block position
            Vector3 pos = ActiveCamera.Position;
            pos.X = foundBlock.position.X + foundBlock.block.BoundingBox.Max.X / 2;
            pos.Z = foundBlock.position.Z + foundBlock.block.BoundingBox.Min.Z / 2;
            ActiveCamera.NextPosition = pos;
            ActiveCamera.ApplyChanges();
            */
        }

        #endregion

        #region Rendering Pipeline

        /// <summary>
        /// Clear graphics device buffer and
        /// draws any background images.
        /// </summary>
        private void DrawBackground()
        {
            // All camera modes but free are just cleared
            if (cameraMode != CameraModes.Free)
            {
                host.GraphicsDevice.Clear(backgroundColor);
                return;
            }

            // Free camera dungeons are cleared black
            if (sceneManager.BatchMode == Scene.BatchModes.Dungeon)
            {
                host.GraphicsDevice.Clear(Color.Black);
                return;
            }

            // Draw sky enabled
            if (Scene.SceneOptionFlags.SkyPlane == (sceneManager.SceneOptions & Scene.SceneOptionFlags.SkyPlane))
            {
                host.GraphicsDevice.Clear(skyManager.ClearColor);
                skyManager.Draw();
                return;
            }

            // If all else fails just clear
            host.GraphicsDevice.Clear(backgroundColor);
        }

        /// <summary>
        /// Draw native mesh as wireframe lines.
        /// </summary>
        /// <param name="color">Line color.</param>
        /// <param name="model">ModelManager.Model.</param>
        /// <param name="matrix">Matrix.</param>
        private void DrawNativeMesh(Color color, ref ModelManager.ModelData model, Matrix matrix)
        {
            // Scale up just a little to make outline visually pop
            matrix = Matrix.CreateScale(1.015f) * matrix;

            // Set view and projection matrices
            lineEffect.View = sceneManager.Camera.View;
            lineEffect.Projection = sceneManager.Camera.Projection;
            lineEffect.World = matrix;

            // Set vertex declaration
            host.GraphicsDevice.VertexDeclaration = lineVertexDeclaration;

            // Draw faces
            foreach (var subMesh in model.DFMesh.SubMeshes)
            {
                foreach (var plane in subMesh.Planes)
                {
                    DrawNativeFace(color, plane.Points, matrix);
                }
            }
        }

        /// <summary>
        /// Draw a native face as a line list.
        /// </summary>
        /// <param name="color">Line color.</param>
        /// <param name="points">DFMesh.DFPoint.</param>
        /// <param name="matrix">Matrix.</param>
        private void DrawNativeFace(Color color, DFMesh.DFPoint[] points, Matrix matrix)
        {
            // Build line primitives for this face
            int lineCount = 0;
            Vector3 vertex1, vertex2;
            for (int p = 0; p < points.Length - 1; p++)
            {
                // Add first point
                vertex1.X = points[p].X;
                vertex1.Y = -points[p].Y;
                vertex1.Z = -points[p].Z;
                planeLines[lineCount].Color = color;
                planeLines[lineCount++].Position = vertex1;

                // Add second point
                vertex2.X = points[p + 1].X;
                vertex2.Y = -points[p + 1].Y;
                vertex2.Z = -points[p + 1].Z;
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

            lineEffect.Begin();
            lineEffect.CurrentTechnique.Passes[0].Begin();

            // Draw lines
            host.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, planeLines, 0, lineCount / 2);

            lineEffect.CurrentTechnique.Passes[0].End();
            lineEffect.End();
        }

        /// <summary>
        /// Sets render states prior to drawing.
        /// </summary>
        private void SetRenderStates()
        {
            // Set render states
            host.GraphicsDevice.RenderState.DepthBufferEnable = true;
            host.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            host.GraphicsDevice.RenderState.AlphaTestEnable = false;
            host.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            // Set anisotropy based on camera mode
            if (cameraMode == CameraModes.Free)
            {
                // Set max anisotropy
                host.GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Anisotropic;
                host.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Anisotropic;
                host.GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Linear;
                host.GraphicsDevice.SamplerStates[0].MaxAnisotropy = host.GraphicsDevice.GraphicsDeviceCapabilities.MaxAnisotropy;
            }
            else
            {
                // Set zero anisotropy
                host.GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Linear;
                host.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Linear;
                host.GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Linear;
                host.GraphicsDevice.SamplerStates[0].MaxAnisotropy = 0;
            }
        }

        /// <summary>
        /// Draw batches of visible triangles that have been sorted by texture
        ///  to minimise begin-end blocks.
        /// </summary>
        private void DrawBatches()
        {
            // Set vertex declaration
            host.GraphicsDevice.VertexDeclaration = modelVertexDeclaration;

            // Update view and projection matrices
            modelEffect.View = sceneManager.Camera.View;
            modelEffect.Projection = sceneManager.Camera.Projection;

            // Set sampler state
            host.GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            host.GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            // Iterate batch lists
            foreach (var item in sceneManager.Batches.Models)
            {
                Scene.BatchModelArray batchArray = item.Value;

                // Do nothing if batch empty
                if (batchArray.Length == 0)
                    continue;

                modelEffect.Texture = host.TextureManager.GetTexture(item.Key);

                modelEffect.Begin();
                modelEffect.CurrentTechnique.Passes[0].Begin();

                // Iterate batch items
                Scene.BatchModelItem batchItem;
                for (int i = 0; i < batchArray.Length; i++)
                {
                    // Get batch item
                    batchItem = batchArray.BatchItems[i];

                    // Set vertex buffer
                    host.GraphicsDevice.Vertices[0].SetSource(
                        batchItem.VertexBuffer,
                        0,
                        VertexPositionNormalTexture.SizeInBytes);

                    modelEffect.World = batchItem.ModelTransform;
                    modelEffect.CommitChanges();

                    // Draw based on indexed flag
                    if (batchItem.Indexed)
                    {
                        // Set index buffer
                        host.GraphicsDevice.Indices = batchItem.IndexBuffer;

                        // Draw indexed primitives
                        host.GraphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        0,
                        0,
                        batchItem.Vertices.Length,
                        batchItem.StartIndex,
                        batchItem.PrimitiveCount);
                    }
                    else
                    {
                        // Draw primitives
                        host.GraphicsDevice.DrawPrimitives(
                            PrimitiveType.TriangleList,
                            batchItem.StartIndex,
                            batchItem.PrimitiveCount);
                    }
                }

                modelEffect.CurrentTechnique.Passes[0].End();
                modelEffect.End();
            }
        }

        /// <summary>
        /// Highlights model under pointer.
        ///  Can differentiate between model types enough to
        ///  recognise doors, switches, etc.
        /// </summary>
        private void HighlightModelUnderPointer()
        {
            // Highlight model under mouse/controller
            if (collisionManager.PointerOverModel != null &&
                cameraVelocity == Vector3.Zero &&
                host.MouseInClientArea)
            {
                Collision.ModelIntersection mi = collisionManager.PointerOverModel;
                ModelManager.ModelData model = host.ModelManager.GetModelData(mi.ModelID.Value);
                
                // Highlight action models
                if (mi.BlockModel.HasActionRecord)
                {
                    HighlightActionChain(ref mi, ref model);
                }
                else
                {
                    // Highlight model based on description
                    switch (mi.BlockModel.Description)
                    {
                        case "DOR":     // Door
                            DrawNativeMesh(doorHighlightColor, ref model, mi.ModelMatrix);
                            break;
                        default:
                            DrawNativeMesh(modelHighlightColor, ref model, mi.ModelMatrix);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Highlights a chain of models linked together by action records.
        ///  Only specific action-enabled models are supported as many actions
        ///  (e.g. casting a spell on the player) have no meaning here.
        /// </summary>
        private void HighlightActionChain(ref Collision.ModelIntersection mi, ref ModelManager.ModelData model)
        {
            // Reject model if it does not have an action record
            // or is a child action of another record. Just draw
            // normal highlight instead.
            if (!mi.BlockModel.HasActionRecord ||
                mi.BlockModel.RdbObject.Resources.ModelResource.ActionResource.ParentObjectIndex != -1)
            {
                DrawNativeMesh(modelHighlightColor, ref model, mi.ModelMatrix);
                return;
            }

            // Highlight parent object selected in scene
            DrawNativeMesh(actionHighlightColor, ref model, mi.ModelMatrix);

            // Find and highlight any child objects
            int key = mi.BlockModel.TargetObjectKey;
            BlockManager.BlockData block = mi.BlockModel.Parent;
            ModelManager.ModelData childModel;
            while (key > 0)
            {
                // Check key exists
                if (!block.ModelLookup.ContainsKey(key))
                    break;

                // Get index in model list
                int index = block.ModelLookup[key];

                // Highlight this model
                childModel = host.ModelManager.GetModelData(block.Models[index].ModelId);
                DrawNativeMesh(actionHighlightColor, ref childModel, block.Models[index].Matrix * mi.BlockMatrix);

                // Get next key
                key = block.Models[index].TargetObjectKey;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates status message.
        /// </summary>
        private void UpdateStatusMessage()
        {
            // Set the message
            host.StatusMessage = currentStatus;
        }

        #endregion

    }

}
