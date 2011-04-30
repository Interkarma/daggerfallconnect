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

namespace DaggerfallModelling.ViewComponents
{
    // Differentiate between Color types
    using GDIColor = System.Drawing.Color;
    using XNAColor = Microsoft.Xna.Framework.Graphics.Color;

    /// <summary>
    /// Component to draw a Daggerfall sky in outsoor scenes.
    /// </summary>
    public class SkyComponent : ComponentBase
    {

        #region Class Variables

        // Sky images
        private SkyFile skyFile;
        private Texture2D westTexture;
        private Texture2D eastTexture;
        private XNAColor clearColor;

        // XNA
        private BasicEffect skyEffect;
        private VertexDeclaration skyVertexDeclaration;
        private Camera camera;

        // Sky
        private VertexPositionNormalTexture[] skyVertices;
        private short[] skyIndices;

        #endregion

        #region Properties

        /// <summary>
        /// Gets clear colour.
        /// </summary>
        public XNAColor ClearColor
        {
            get { return clearColor; }
        }

        /// <summary>
        /// Gets or sets camera used when rendering sky.
        /// </summary>
        public Camera Camera
        {
            get { return camera; }
            set { camera = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public SkyComponent(ViewHost host)
            : base(host)
        {
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Called when component mnust initialise.
        /// </summary>
        public override void Initialize()
        {
            // Create vertex declaration
            skyVertexDeclaration = new VertexDeclaration(host.GraphicsDevice, VertexPositionNormalTexture.VertexElements);

            // Create sky effect
            skyEffect = new BasicEffect(host.GraphicsDevice, null);
            skyEffect.World = Matrix.Identity;
            skyEffect.TextureEnabled = true;
            skyEffect.LightingEnabled = false;
            skyEffect.AmbientLightColor = new Vector3(1f, 1f, 1f);
            skyEffect.View = Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);

            // Build sky
            BuildSky();

            // TEST: Load a sample sky file
            skyFile = new SkyFile(
                Path.Combine(host.Arena2Path, "SKY09.DAT"),
                FileUsage.UseDisk,
                true);

            // TEST: Load a sample sky
            LoadTextures();
        }

        /// <summary>
        /// Called when view component should tick animation.
        /// </summary>
        public override void Tick()
        {
        }

        /// <summary>
        /// Called when view component should redraw.
        /// </summary>
        public override void Draw()
        {
            if (host.IsReady && this.Enabled)
            {
                DrawSky();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Build sky.
        /// </summary>
        private void BuildSky()
        {
            // Set dimensions of sky "plane" segment
            const float w = 256f;
            const float h = 110f;

            // Create vertex array
            skyVertices = new VertexPositionNormalTexture[4];
            skyVertices[0] = new VertexPositionNormalTexture(
                new Vector3(-w, h, -w),
                new Vector3(0, 0, 0),
                new Vector2(0, 0));
            skyVertices[1] = new VertexPositionNormalTexture(
                new Vector3(w, h, -w),
                new Vector3(0, 0, 0),
                new Vector2(1f, 0));
            skyVertices[2] = new VertexPositionNormalTexture(
                new Vector3(-w, -h, -w),
                new Vector3(0, 0, 0),
                new Vector2(0, 1f));
            skyVertices[3] = new VertexPositionNormalTexture(
                new Vector3(w, -h, -w),
                new Vector3(0, 0, 0),
                new Vector2(1f, 1f));

            // Create index array
            skyIndices = new short[6]
            {
                0, 1, 2,
                2, 1, 3,
            };
        }

        /// <summary>
        /// Draw sky.
        /// </summary>
        private void DrawSky()
        {
            // Set render states
            host.GraphicsDevice.RenderState.DepthBufferEnable = false;
            host.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            host.GraphicsDevice.RenderState.AlphaTestEnable = false;
            host.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            // Set sampler state
            host.GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
            host.GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Clamp;

            // Set zero anisotropy
            host.GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Linear;
            host.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Linear;
            host.GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Linear;
            host.GraphicsDevice.SamplerStates[0].MaxAnisotropy = 0;

            // Set vertex declaration
            host.GraphicsDevice.VertexDeclaration = skyVertexDeclaration;

            // Set matrices
            skyEffect.Projection = camera.Projection;

            // Translate planes left and right based on camera yaw
            float percent;
            Matrix scroll;
            Texture2D fillerTexture;
            float fillerX;
            if (camera.Yaw > 179f && camera.Yaw < 360f)
            {
                percent = (360f - camera.Yaw) / 180f;
                scroll = Matrix.CreateTranslation(-512f * percent, 0f, 0f);
                fillerX = 768f;
                fillerTexture = westTexture;
            }
            else
            {
                percent = camera.Yaw / 180f;
                scroll = Matrix.CreateTranslation(512f * percent, 0f, 0f);
                fillerX = -768f;
                fillerTexture = eastTexture;
            }

            // Translate planes up and down based on camera pitch
            percent = camera.Pitch / 90f;
            float adjustY = -(256f * percent * 1.5f);
            if (adjustY < -110f)
                adjustY = -110f;
            scroll *= Matrix.CreateTranslation(0f, adjustY, 0f);

            // Begin effect
            skyEffect.Begin();
            skyEffect.CurrentTechnique.Passes[0].Begin();

            // Draw west half of sky
            DrawSkyPlane(
                scroll * Matrix.CreateTranslation(-256, 110f, 0f),
                westTexture);

            // Draw east half of sky
            DrawSkyPlane(
                scroll * Matrix.CreateTranslation(256, 110f, 0f),
                eastTexture);

            // Draw filler texture to bridge between scrolling hemispheres
            DrawSkyPlane(
                scroll * Matrix.CreateTranslation(fillerX, 110f, 0f),
                fillerTexture);

            // End effect
            skyEffect.CurrentTechnique.Passes[0].End();
            skyEffect.End();
        }

        /// <summary>
        /// Draws sky plane using specified matrix and texture.
        /// </summary>
        /// <param name="matrix">Matrix.</param>
        /// <param name="texture">Texture2D.</param>
        private void DrawSkyPlane(Matrix matrix, Texture2D texture)
        {
            skyEffect.World = matrix;
            skyEffect.Texture = texture;
            skyEffect.CommitChanges();
            host.GraphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList,
                skyVertices,
                0,
                skyVertices.Length,
                skyIndices,
                0,
                skyIndices.Length / 3);
        }

        /// <summary>
        /// Loads textures for current time of day.
        ///  Will exchange east and west and reverse animation for afternoon times.
        /// </summary>
        private void LoadTextures()
        {
            int index = 22;

            // Get images for both sides of sky
            skyFile.Palette = skyFile.GetDFPalette(index);
            DFBitmap west = skyFile.GetBitmapFormat(0, index, 0, DFBitmap.Formats.ARGB);
            DFBitmap east = skyFile.GetBitmapFormat(1, index, 0, DFBitmap.Formats.ARGB);

            // Create textures
            westTexture = new Texture2D(host.GraphicsDevice, west.Width, west.Height, 1, TextureUsage.None, SurfaceFormat.Color);
            eastTexture = new Texture2D(host.GraphicsDevice, east.Width, east.Height, 1, TextureUsage.None, SurfaceFormat.Color);

            // Set data
            westTexture.SetData<byte>(0, null, west.Data, 0, west.Width * west.Height * 4, SetDataOptions.None);
            eastTexture.SetData<byte>(0, null, east.Data, 0, west.Width * west.Height * 4, SetDataOptions.None);

            // Sample clear colour from bottom of sky image
            int pos = west.Data.Length - 4;
            byte a = west.Data[pos+3];
            byte r = west.Data[pos+2];
            byte g = west.Data[pos+1];
            byte b = west.Data[pos];
            clearColor.A = a;
            clearColor.R = r;
            clearColor.G = g;
            clearColor.B = b;
        }

        #endregion

    }

}
