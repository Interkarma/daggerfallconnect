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

        // Block layout
        Dictionary<int, BlockPosition> exteriorLayout = new Dictionary<int,BlockPosition>();
        Dictionary<int, BlockPosition> dungeonLayout = new Dictionary<int, BlockPosition>();

        // Appearance
        private Color backgroundColor = Color.LightGray;

        // XNA
        private VertexDeclaration modelVertexDeclaration;
        private BasicEffect modelEffect;
        private float nearPlaneDistance = 1.0f;
        private float farPlaneDistance = 40000.0f;
        private Matrix projectionMatrix;
        private Matrix viewMatrix;
        private Matrix worldMatrix;
        private BoundingFrustum viewFrustum;
        private Vector3 cameraPosition = new Vector3(0, 1000, 0);
        private Vector3 cameraReference = new Vector3(0, 0, -1);
        private Vector3 cameraUpVector = new Vector3(0, 1, 0);
        private float cameraYaw = 0.0f;
        private float cameraPitch = 0.0f;
        private Vector3 movement;

        // Drawing
        RenderableBoundingBox renderableBounds;

        // Movement
        float rotationStep = 10.0f;
        float translationStep = 200.0f;

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

        #region Class Structures

        /// <summary>
        /// Describes how a block is positioned in world space.
        /// </summary>
        private struct BlockPosition
        {
            public string name;
            public Vector3 position;
            public BlockManager.Block block;
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

            // Create initial view frustum
            viewFrustum = new BoundingFrustum(viewMatrix * projectionMatrix);

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

            // Set vertex declaration
            host.GraphicsDevice.VertexDeclaration = modelVertexDeclaration;

            // Set view and projection matrices
            modelEffect.View = viewMatrix;
            modelEffect.Projection = projectionMatrix;

            // Update frustum matrix
            viewFrustum.Matrix = viewMatrix * projectionMatrix;

            // Draw visible blocks
            foreach (var layoutItem in exteriorLayout)
            {
                // Create translation matrix for this block
                Matrix world = Matrix.CreateTranslation(exteriorLayout[layoutItem.Key].position);

                // Create transformed block bounding box
                BoundingBox blockBox = new BoundingBox(
                    Vector3.Transform(layoutItem.Value.block.BoundingBox.Min, world),
                    Vector3.Transform(layoutItem.Value.block.BoundingBox.Max, world));

                // Test block bounding box against frustum
                if (!viewFrustum.Intersects(blockBox))
                    continue;

                // Draw each model in this block
                foreach (var modelItem in layoutItem.Value.block.Models)
                {
                    modelEffect.World = modelItem.Matrix * world;
                    DrawModel((int)modelItem.ModelId);
                }

                // Draw gound plane in this block
                modelEffect.World = world;
                DrawGroundPlane(layoutItem.Key);
            }
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
            movement = Vector3.Zero;

            // Change camera facing or position based on mouse button down
            if (host.LeftMouseDown)
            {
                // Update yaw and pitch
                cameraYaw += (-host.MousePosDelta.X * rotationStep) * host.TimeDelta;
                cameraPitch += (-host.MousePosDelta.Y * rotationStep) * host.TimeDelta;
            }
            else if (host.RightMouseDown)
            {
                // Update movement
                movement.Z += (-host.MousePosDelta.Y * translationStep) * host.TimeDelta;
            }

            // Create rotation matrix from yaw and pitch
            Matrix rotation;
            Matrix.CreateRotationY(MathHelper.ToRadians(cameraYaw), out rotation);
            rotation = Matrix.CreateRotationX(MathHelper.ToRadians(cameraPitch)) * rotation;

            // Adjust position
            if (movement != Vector3.Zero)
            {
                Vector3.Transform(ref movement, ref rotation, out movement);
                cameraPosition -= (movement * translationStep) * host.TimeDelta;
            }

            // Transform camera
            Vector3 transformedReference;
            Vector3.Transform(ref cameraReference, ref rotation, out transformedReference);
            Vector3 cameraTarget;
            Vector3.Add(ref cameraPosition, ref transformedReference, out cameraTarget);
            Matrix.CreateLookAt(ref cameraPosition, ref cameraTarget, ref cameraUpVector, out viewMatrix);
        }

        /// <summary>
        /// Called when the mouse wheel is scrolled.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public override void OnMouseWheel(MouseEventArgs e)
        {
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

        private void DrawModel(int key)
        {
            // Get model
            ModelManager.Model model = host.ModelManager.GetModel(key);

            // Exit if no model loaded
            if (model.Vertices == null)
                return;

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

        private void DrawGroundPlane(int key)
        {
            // Set terrain texture atlas
            modelEffect.Texture = host.TextureManager.TerrainAtlas;

            modelEffect.Begin();
            modelEffect.CurrentTechnique.Passes[0].Begin();

            // Draw ground plane
            host.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, exteriorLayout[key].block.GroundPlaneVertices, 0, 512);

            modelEffect.CurrentTechnique.Passes[0].End();
            modelEffect.End();
        }

        #endregion

        #region Content Loading

        /// <summary>
        /// Sets location currently in view.
        /// </summary>
        /// <param name="dfLocation">DFLocation.</param>
        public void SetLocation(ref DFLocation dfLocation)
        {
            // Build exterior layout
            BuildExteriorLayout(ref dfLocation);

            // Optionally build dungeon layout
            if (dfLocation.HasDungeon)
                BuildDungeonLayout(ref dfLocation);

            // Set climate for texture swaps
            //host.TextureManager.Climate = dfLocation.Climate;
        }

        #endregion

        #region Private Methods

        private void BuildExteriorLayout(ref DFLocation dfLocation)
        {
            // All exterior blocks are 4096x4096 in X-Z space.
            const float blockSide = 4096.0f;

            // Get dimensions of exterior location array
            int width = dfLocation.Exterior.ExteriorData.Width;
            int height = dfLocation.Exterior.ExteriorData.Height;

            // Create exterior layout
            exteriorLayout = new Dictionary<int, BlockPosition>(width * height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Get block key and name
                    int key = GetBlockKey(x, y);
                    string name = host.BlockManager.BlocksFile.CheckName(host.MapsFile.GetRmbBlockName(ref dfLocation, x, y));

                    // Create block position data
                    BlockPosition blockPosition = new BlockPosition();
                    blockPosition.name = name;
                    blockPosition.block = host.BlockManager.LoadBlock(name);
                    
                    // Set block position
                    blockPosition.position = new Vector3(x * blockSide, 0, -(y * blockSide));

                    // Build ground plane
                    host.BlockManager.BuildRmbGroundPlane(host.TextureManager, ref blockPosition.block);

                    // Load block models and textures
                    LoadBlockResources(ref blockPosition.block);

                    // Add to layout dictionary
                    exteriorLayout.Add(key, blockPosition);
                }
            }
        }

        private void BuildDungeonLayout(ref DFLocation dfLocation)
        {
        }

        private int GetBlockKey(int x, int y)
        {
            return y * 100 + x;
        }

        #endregion

        #region Block Management

        private void LoadBlockResources(ref BlockManager.Block block)
        {
            // Load block models
            float maxHeight = 0;
            for (int i = 0; i < block.Models.Count; i++)
            {
                // Get model info
                BlockManager.ModelInfo info = block.Models[i];

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
                block.Models[i] = info;
            }

            // Save correct max height in block
            Vector3 max = new Vector3(
                block.BoundingBox.Max.X,
                maxHeight,
                block.BoundingBox.Max.Z);
            block.BoundingBox.Max = max;
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
