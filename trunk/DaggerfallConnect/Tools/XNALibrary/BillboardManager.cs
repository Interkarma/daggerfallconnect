// Project:         XNALibrary
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
using System.Threading;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace XNALibrary
{

    /// <summary>
    /// Component to draw Daggerfall billboards (flats).
    ///  These billboards rotate around Y axis to always face camera.
    /// </summary>
    public class BillboardManager : Component
    {

        #region Class Variables

        // Daggerfall resources
        private TextureManager textureManager = null;

        // XNA
        private BasicEffect effect;
        private VertexDeclaration vertexDeclaration;

        // Billboard
        private VertexPositionNormalTexture[] billboardVertices;
        private short[] billboardIndices;

        // Batching
        private List<BillboardBatchItem> batch = new List<BillboardBatchItem>(1024);

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets TextureManager for resource loading.
        /// </summary>
        public TextureManager TextureManager
        {
            get { return textureManager; }
            set { textureManager = value; }
        }

        #endregion

        #region SubClasses

        /// <summary>
        /// Describes a batched billboard for rendering.
        ///  Used when sorting billboards for correct draw order.
        /// </summary>
        private class BillboardBatchItem : IComparable<BillboardBatchItem>
        {
            // Variables
            private float? distance;
            private int textureKey;
            private Matrix matrix;

            // Properties
            public float? Distance
            {
                get { return distance; }
                set { distance = value; }
            }
            public int TextureKey
            {
                get { return textureKey; }
                set { textureKey = value; }
            }
            public Matrix Matrix
            {
                get { return matrix; }
                set { matrix = value; }
            }

            // Constructors
            public BillboardBatchItem()
            {
                this.distance = null;
                this.textureKey = -1;
                this.matrix = Matrix.Identity;
            }
            public BillboardBatchItem(float? distance, int textureKey, Matrix matrix)
            {
                this.distance = distance;
                this.textureKey = textureKey;
                this.matrix = matrix;
            }

            // IComparable
            public int CompareTo(BillboardBatchItem other)
            {
                int returnValue = 1;
                if (other.Distance < this.Distance)
                    returnValue = -1;
                else if (other.Distance == this.Distance)
                    returnValue = 0;
                return returnValue;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="graphicsDevice">Graphics device used in rendering.</param>
        public BillboardManager(GraphicsDevice graphicsDevice)
            : base(graphicsDevice)
        {
            // Create vertex declaration
            vertexDeclaration = new VertexDeclaration(graphicsDevice, VertexPositionNormalTexture.VertexElements);

            // Create billboard effect
            effect = new BasicEffect(graphicsDevice, null);
            effect.TextureEnabled = true;
            effect.LightingEnabled = false;
            effect.AmbientLightColor = new Vector3(1f, 1f, 1f);

            // Create billboard template
            CreateBillboard();
        }

        #endregion

        #region Abstract Overrides

        /// <summary>
        /// Called when view component should redraw.
        /// </summary>
        /// <param name="camera">Camera looking into scene.</param>
        public override void Draw(Camera camera)
        {
            if (this.Enabled && camera != null)
            {
                BeginDraw(camera);
                DrawBatch();
                EndDraw();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets batch prior to being rebuilt.
        /// </summary>
        public void ClearBatch()
        {
            // Reset batch
            batch.Clear();
        }

        /// <summary>
        /// Submit a billboard to be drawn. This must be done
        ///  every frame or Draw() will do nothing.
        /// </summary>
        /// <param name="camera">Camera looking into scene.</param>
        /// <param name="origin">Origin of billboard.</param>
        /// <param name="position">Position of billboard relative to origin.</param>
        /// <param name="size">Size.</param>
        /// <param name="textureKey">Texture key.</param>
        /// <param name="blockTransform">Matrix of parent block.</param>
        public void AddBatch(Camera camera, Vector3 origin, Vector3 position, Vector2 size, int textureKey, Matrix blockTransform)
        {
            // Create billboard matrix
            Matrix constrainedBillboard = Matrix.CreateConstrainedBillboard(
                position,
                position - camera.TransformedReference,
                Vector3.Up,
                null,
                null);

            // Create billboard transform
            Matrix transform =
                Matrix.CreateScale(size.X, size.Y, 1f) *
                Matrix.CreateTranslation(origin) *
                constrainedBillboard *
                blockTransform;

            // Calc distance between batch and camera
            float distance = Vector3.Distance(
                Vector3.Transform(position, blockTransform),
                camera.Position);

            // Add to batch
            BillboardBatchItem batchItem = new BillboardBatchItem(
                distance,
                textureKey,
                transform);
            batch.Add(batchItem);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Begin drawing billboards. This must be called before the first
        ///  billboard is drawn.
        /// </summary>
        /// <param name="camera">Camera looking into scene.</param>
        private void BeginDraw(Camera camera)
        {
            // Set render states
            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = false;
            GraphicsDevice.RenderState.AlphaBlendEnable = true;
            GraphicsDevice.RenderState.AlphaTestEnable = false;
            GraphicsDevice.RenderState.CullMode = CullMode.None;
            GraphicsDevice.RenderState.SourceBlend = Blend.One;
            GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;

            // Set sampler state
            GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
            GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Clamp;

            // Set zero anisotropy
            GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Linear;
            GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Linear;
            GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Linear;
            GraphicsDevice.SamplerStates[0].MaxAnisotropy = 0;

            // Set vertex declaration
            GraphicsDevice.VertexDeclaration = vertexDeclaration;

            // Set matrices
            effect.View = camera.View;
            effect.Projection = camera.Projection;

            // Sort batch
            batch.Sort();

            // Begin effect
            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();
        }

        /// <summary>
        /// Draw all batched billboards.
        /// </summary>
        private void DrawBatch()
        {
            // Exit if TextureManager not set
            if (textureManager == null)
                return;

            // Draw all billboards in batch
            foreach (var item in batch)
            {
                // Update effect
                effect.World = item.Matrix;
                effect.Texture = textureManager.GetTexture(item.TextureKey);
                effect.CommitChanges();

                // Draw billboard
                GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    billboardVertices,
                    0,
                    billboardVertices.Length,
                    billboardIndices,
                    0,
                    billboardIndices.Length / 3);
            }
        }

        /// <summary>
        /// End drawing billboards. This must be called after the last billboard
        ///  has been drawn.
        /// </summary>
        private void EndDraw()
        {
            // End effect
            effect.CurrentTechnique.Passes[0].End();
            effect.End();

            // Restore uncommon render states
            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
        }

        /// <summary>
        /// Create billboard.
        /// </summary>
        private void CreateBillboard()
        {
            // Set dimensions of billboard 
            const float w = 0.5f;
            const float h = 0.5f;

            // Create vertex array
            billboardVertices = new VertexPositionNormalTexture[4];
            billboardVertices[0] = new VertexPositionNormalTexture(
                new Vector3(-w, h, 0),
                new Vector3(0, 0, 0),
                new Vector2(1, 0));
            billboardVertices[1] = new VertexPositionNormalTexture(
                new Vector3(w, h, 0),
                new Vector3(0, 0, 0),
                new Vector2(0, 0));
            billboardVertices[2] = new VertexPositionNormalTexture(
                new Vector3(-w, -h, 0),
                new Vector3(0, 0, 0),
                new Vector2(1, 1));
            billboardVertices[3] = new VertexPositionNormalTexture(
                new Vector3(w, -h, 0),
                new Vector3(0, 0, 0),
                new Vector2(0, 1));

            // Create index array
            billboardIndices = new short[6]
            {
                0, 1, 2,
                2, 1, 3,
            };
        }

        #endregion

    }

}
