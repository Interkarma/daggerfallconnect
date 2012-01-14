// Project:         Deep Engine
// Description:     3D game engine for Ruins of Hill Deep and Daggerfall Workshop projects.
// Copyright:       Copyright (C) 2012 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DeepEngine.Core;
using DeepEngine.Daggerfall;
using DeepEngine.Rendering;
using DeepEngine.Utility;
using DeepEngine.World;
#endregion

namespace DeepEngine.Components
{

    /// <summary>
    /// Component for drawing batched billboard sprites from Daggerfall.
    /// </summary>
    public class DaggerfallBillboardBatchComponent : DrawableComponent
    {

        #region Fields

        // Billboard geometry template
        private VertexPositionNormalTextureBump[] billboardVertices;
        private int[] billboardIndices;

        // Billboard static batches
        StaticGeometryBuilder staticGeometry;

        // Effects
        Effect renderBillboards;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        public DaggerfallBillboardBatchComponent(DeepCore core)
            : base(core)
        {
            // Setup
            CreateBillboard();
            staticGeometry = new StaticGeometryBuilder(core.GraphicsDevice);
            renderBillboards = core.ContentManager.Load<Effect>("Effects/RenderBillboards");
        }

        #endregion

        #region DrawableComponent Overrides

        /// <summary>
        /// Called when component should draw itself.
        ///  First pass renders opaque part of texture, which is sorted by depth buffer.
        ///  Second pass renders unsorted alpha fringe of texture.
        ///  This yields decent results and is much less expensive than a CPU sort.
        /// </summary>
        /// <param name="caller"></param>
        public override void Draw(BaseEntity caller)
        {
            // Do nothing if disabled
            if (!enabled)
                return;

            // Calculate world matrix
            Matrix worldMatrix = caller.Matrix * matrix;

            // Set render states
            core.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            core.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            core.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            // Set transforms
            renderBillboards.Parameters["World"].SetValue(worldMatrix);
            renderBillboards.Parameters["View"].SetValue(core.ActiveScene.Camera.View);
            renderBillboards.Parameters["Projection"].SetValue(core.ActiveScene.Camera.Projection);

            // Set buffers
            core.GraphicsDevice.SetVertexBuffer(staticGeometry.VertexBuffer);
            core.GraphicsDevice.Indices = staticGeometry.IndexBuffer;

            // Draw opaque pass
            core.GraphicsDevice.BlendState = BlendState.Opaque;
            core.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            renderBillboards.Parameters["AlphaTestDirection"].SetValue(1f);
            DrawBatches();

            // Draw blended fringe pixels
            //core.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            //core.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            //renderBillboards.Parameters["AlphaTestDirection"].SetValue(-1f);
            //DrawBatches();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add billboard to collection.
        /// </summary>
        /// <param name="archive">Texture archive.</param>
        /// <param name="record">Texture index.</param>
        /// <param name="position">Position relative to parent entity.</param>
        public void AddBillboard(int archive, int record, Vector3 position)
        {
            MaterialManager.TextureCreateFlags flags = 
                MaterialManager.TextureCreateFlags.Dilate |
                MaterialManager.TextureCreateFlags.PreMultiplyAlpha |
                MaterialManager.TextureCreateFlags.MipMaps;

            // Load flat
            int textureKey;
            Vector2 startSize, finalSize;
            LoadDaggerfallFlat(archive, record, flags, out textureKey, out startSize, out finalSize);

            // Calcuate final position
            Vector3 finalPosition = new Vector3(position.X, -position.Y + (finalSize.Y / 2) - 4, -position.Z);

            // Information about the billboard is packed into unused parts of the vertex format.
            // This allows us to send a huge static batch of billboards and correctly position, rotate, and scale
            // each one for the camera.
            for (int i = 0; i < 4; i++)
            {
                billboardVertices[i].Tangent = finalPosition;
                billboardVertices[i].Binormal = new Vector3(finalSize.X, finalSize.Y, 0);
            }

            // Add to batch
            staticGeometry.AddToBuilder((uint)textureKey, billboardVertices, billboardIndices, Matrix.Identity);
        }

        /// <summary>
        /// Seal billboard static geometry so it can be rendered.
        ///  Only do this once all static billboards have been added.
        /// </summary>
        public void Seal()
        {
            // Seal static geometry
            staticGeometry.ApplyBuilder();
            staticGeometry.Seal();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates billboard template.
        /// </summary>
        private void CreateBillboard()
        {
            // Set dimensions of billboard 
            const float w = 0.5f;
            const float h = 0.5f;

            // Create vertex array
            billboardVertices = new VertexPositionNormalTextureBump[4];
            billboardVertices[0] = new VertexPositionNormalTextureBump(
                new Vector3(-w, h, 0),
                Vector3.Up,
                new Vector2(0, 0),
                Vector3.Zero,
                Vector3.Zero);
            billboardVertices[1] = new VertexPositionNormalTextureBump(
                new Vector3(w, h, 0),
                Vector3.Up,
                new Vector2(1, 0),
                Vector3.Zero,
                Vector3.Zero);
            billboardVertices[2] = new VertexPositionNormalTextureBump(
                new Vector3(-w, -h, 0),
                Vector3.Up,
                new Vector2(0, 1),
                Vector3.Zero,
                Vector3.Zero);
            billboardVertices[3] = new VertexPositionNormalTextureBump(
                new Vector3(w, -h, 0),
                Vector3.Up,
                new Vector2(1, 1),
                Vector3.Zero,
                Vector3.Zero);

            // Create index array
            billboardIndices = new int[6]
            {
                0, 1, 2,
                1, 3, 2,
            };
        }

        /// <summary>
        /// Loads a Daggerfall flat (billboard).
        /// </summary>
        /// <param name="textureArchive">Texture archive index.</param>
        /// <param name="textureRecord">Texture record index.</param>
        /// <param name="textureFlags">Texture create flags.</param>
        /// <param name="textureKey">Texture key.</param>
        /// <param name="startSize">Start size before scaling.</param>
        /// <param name="finalSize">Final size after scaling.</param>
        private void LoadDaggerfallFlat(
            int textureArchive,
            int textureRecord,
            MaterialManager.TextureCreateFlags textureFlags,
            out int textureKey,
            out Vector2 startSize,
            out Vector2 finalSize)
        {
            // Get path to texture file
            string path = Path.Combine(
                core.MaterialManager.Arena2Path,
                TextureFile.IndexToFileName(textureArchive));

            // Get size and scale of this texture
            System.Drawing.Size size = TextureFile.QuickSize(path, textureRecord);
            System.Drawing.Size scale = TextureFile.QuickScale(path, textureRecord);

            // Set start size
            startSize.X = size.Width;
            startSize.Y = size.Height;

            // Apply scale
            int xChange = (int)(size.Width * (scale.Width / BlocksFile.ScaleDivisor));
            int yChange = (int)(size.Height * (scale.Height / BlocksFile.ScaleDivisor));
            finalSize.X = size.Width + xChange;
            finalSize.Y = size.Height + yChange;

            // Load texture
            textureKey = core.MaterialManager.LoadTexture(
                textureArchive,
                textureRecord,
                textureFlags);
        }

        /// <summary>
        /// Draw static batches with current settings.
        /// </summary>
        private void DrawBatches()
        {
            // Draw batches
            foreach (var item in staticGeometry.StaticBatches)
            {
                // Apply texture
                renderBillboards.Parameters["Texture"].SetValue(core.MaterialManager.GetTexture((int)item.Key));

                // Render geometry
                foreach (EffectPass pass in renderBillboards.CurrentTechnique.Passes)
                {
                    // Apply effect pass
                    pass.Apply();

                    // Draw primitives
                    core.GraphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        0,
                        0,
                        staticGeometry.VertexBuffer.VertexCount,
                        item.Value.StartIndex,
                        item.Value.PrimitiveCount);
                }
            }
        }

        #endregion

    }

}
