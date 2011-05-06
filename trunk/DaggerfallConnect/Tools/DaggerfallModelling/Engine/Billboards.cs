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
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using XNALibrary;
using DaggerfallModelling.ViewControls;
#endregion

namespace DaggerfallModelling.Engine
{

    // Differentiate between Color types
    using GDIColor = System.Drawing.Color;
    using XNAColor = Microsoft.Xna.Framework.Graphics.Color;

    /// <summary>
    /// Component to draw Daggerfall flats.
    ///  These billboards will rotate around Y axis to face camera.
    ///  Origin is always at bottom centre.
    /// </summary>
    class Billboards : ComponentBase
    {

        #region Class Variables

        // XNA
        private BasicEffect billboardEffect;
        private VertexDeclaration billboardVertexDeclaration;
        private Camera camera;

        // Billboard
        private VertexPositionNormalTexture[] billboardVertices;
        private short[] billboardIndices;

        // Batching
        private const int batchArrayLength = 1024;
        private List<BatchFlat> batch = new List<BatchFlat>(batchArrayLength);

#if DEBUG
        // Performance
        private int maxBatchLength = 0;
#endif

        #endregion

        #region Class Structures
        #endregion

        #region SubClasses

        /// <summary>
        /// Describes a batched billboard for rendering.
        ///  Used when sorting billboards for correct draw order..
        /// </summary>
        private class BatchFlat : IComparable<BatchFlat>
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
            public BatchFlat()
            {
                this.distance = null;
                this.textureKey = -1;
                this.matrix = Matrix.Identity;
            }
            public BatchFlat(float? distance, int textureKey, Matrix matrix)
            {
                this.distance = distance;
                this.textureKey = textureKey;
                this.matrix = matrix;
            }

            // IComparable
            public int CompareTo(BatchFlat other)
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

        #region Properties

        /// <summary>
        /// Gets or sets camera used when rendering billboard.
        /// </summary>
        public Camera Camera
        {
            get { return camera; }
            set { camera = value; }
        }

#if DEBUG
        /// <summary>
        /// Gets max batch length.
        /// </summary>
        public int MaxBatchLength
        {
            get { return maxBatchLength; }
        }
#endif

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public Billboards(ViewHost host)
            : base(host)
        {
        }

        #endregion

        #region Abstract Overrides

        /// <summary>
        /// Called when component must initialise.
        /// </summary>
        public override void Initialize()
        {
            // Create vertex declaration
            billboardVertexDeclaration = new VertexDeclaration(host.GraphicsDevice, VertexPositionNormalTexture.VertexElements);

            // Create billboard effect
            billboardEffect = new BasicEffect(host.GraphicsDevice, null);
            billboardEffect.TextureEnabled = true;
            billboardEffect.LightingEnabled = false;
            billboardEffect.AmbientLightColor = new Vector3(1f, 1f, 1f);

            // Build billboard
            BuildBillboard();
        }

        /// <summary>
        /// Called when view component should update animation.
        /// </summary>
        public override void Update()
        {
        }

        /// <summary>
        /// Called when view component should redraw.
        /// </summary>
        public override void Draw()
        {
            if (host.IsReady && this.Enabled)
            {
                BeginDraw();
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
            batch.Capacity = batchArrayLength;

#if DEBUG
            maxBatchLength = 0;
#endif
        }

        /// <summary>
        /// Submit a flat to be drawn. This must be done
        ///  every frame or Draw() will do nothing.
        /// </summary>
        /// <param name="position">Position.</param>
        /// <param name="size">Size.</param>
        /// <param name="textureKey">Texture key.</param>
        /// <param name="blockTransform">Matrix of parent block.</param>
        public void AddToBatch(Vector3 origin, Vector3 position, Vector2 size, int textureKey, Matrix blockTransform)
        {
            // Create billboard matrix
            Matrix constrainedBillboard = Matrix.CreateConstrainedBillboard(
                position,
                position - camera.TransformedReference,
                Vector3.Up,
                null,
                null);

            // Create flat transform
            Matrix flatTransform =
                Matrix.CreateScale(size.X, size.Y, 1f) *
                Matrix.CreateTranslation(origin) *
                constrainedBillboard * 
                blockTransform;

            // Calc distance between batch and camera
            float distance = Vector3.Distance(
                Vector3.Transform(position, blockTransform),
                Camera.Position);

            // Add to batch
            BatchFlat flat = new BatchFlat(
                distance,
                textureKey,
                flatTransform);
            batch.Add(flat);

#if DEBUG
            // Track max batch length
            if (batch.Count > maxBatchLength)
                maxBatchLength = batch.Count;
#endif
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Begin drawing billboards. This must be called before the first
        ///  billboard is drawn.
        /// </summary>
        private void BeginDraw()
        {
            // Set render states
            host.GraphicsDevice.RenderState.DepthBufferEnable = true;
            host.GraphicsDevice.RenderState.DepthBufferWriteEnable = false;
            host.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            host.GraphicsDevice.RenderState.AlphaTestEnable = false;
            host.GraphicsDevice.RenderState.CullMode = CullMode.None;
            host.GraphicsDevice.RenderState.SourceBlend = Blend.One;
            host.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;

            // Set sampler state
            host.GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
            host.GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Clamp;

            // Set zero anisotropy
            host.GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Linear;
            host.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Linear;
            host.GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Linear;
            host.GraphicsDevice.SamplerStates[0].MaxAnisotropy = 0;

            // Set vertex declaration
            host.GraphicsDevice.VertexDeclaration = billboardVertexDeclaration;

            // Set matrices
            billboardEffect.View = camera.View;
            billboardEffect.Projection = camera.Projection;

            // Sort batch
            batch.Sort();

            // Begin effect
            billboardEffect.Begin();
            billboardEffect.CurrentTechnique.Passes[0].Begin();
        }

        /// <summary>
        /// Draw all batched billboards.
        /// </summary>
        private void DrawBatch()
        {
            // Draw all flats in batch
            foreach (var flat in batch)
            {
                // Update effect
                billboardEffect.World = flat.Matrix;
                billboardEffect.Texture = host.TextureManager.GetTexture(flat.TextureKey);
                billboardEffect.CommitChanges();

                // Draw billboard
                host.GraphicsDevice.DrawUserIndexedPrimitives(
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
            billboardEffect.CurrentTechnique.Passes[0].End();
            billboardEffect.End();

            // Restore uncommon render states
            host.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
        }

        /// <summary>
        /// Build billboard.
        /// </summary>
        private void BuildBillboard()
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
