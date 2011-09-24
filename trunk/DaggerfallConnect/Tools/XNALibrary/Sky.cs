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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using XNALibrary;
#endregion

namespace XNALibrary
{
    // Differentiate between Color types
    using GDIColor = System.Drawing.Color;
    using XNAColor = Microsoft.Xna.Framework.Graphics.Color;

    /// <summary>
    /// Component to draw a Daggerfall sky in outsoor scenes.
    /// </summary>
    public class Sky : Component
    {

        #region Class Variables

        // Daggerfall
        private string arena2Path = string.Empty;

        // Sky images
        private SkyFile skyFile;
        private Texture2D westTexture;
        private Texture2D eastTexture;
        private XNAColor clearColor;

        // Sky animation
        private const int defaultSkyIndex = 9;
        private const int defaultSkyFrame = 22;
        private VertexPositionNormalTexture[] skyVertices;
        private short[] skyIndices;
        private int skyIndex = defaultSkyIndex;
        private int skyFrame = defaultSkyFrame;

        // XNA
        private BasicEffect skyEffect;
        private VertexDeclaration skyVertexDeclaration;

        #endregion

        #region Properties

        /// <summary>
        /// Gets Arena2 path set at construction.
        /// </summary>
        public string Arena2Path
        {
            get { return arena2Path; }
        }

        /// <summary>
        /// Gets clear colour.
        /// </summary>
        public XNAColor ClearColor
        {
            get { return clearColor; }
        }

        /// <summary>
        /// Gets default sky index.
        /// </summary>
        static public int DefaultSkyIndex
        {
            get { return defaultSkyIndex; }
        }

        /// <summary>
        /// Gets default sky frame.
        /// </summary>
        static public int DefaultSkyFrame
        {
            get { return defaultSkyFrame; }
        }

        /// <summary>
        /// Gets or sets sky index. For example 9 will
        ///  load "SKY09.DAT". Valid range is 0-31.
        ///  All other values are ignored.
        /// </summary>
        public int SkyIndex
        {
            get { return skyIndex; }
            set { SetSkyIndex(value); }
        }

        /// <summary>
        /// Gets or sets sky frame.
        ///  0-31 is midnight to midday (morning).
        ///  32-63 is midday to midnight (afternoon).
        ///  All other values are ignored.
        /// </summary>
        public int SkyFrame
        {
            get { return skyFrame; }
            set { SetSkyFrame(value); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="graphicsDevice">Graphics device used in rendering.</param>
        /// <param name="arena2Path">Arena2 path to locate sky image files.</param>
        public Sky(GraphicsDevice graphicsDevice, string arena2Path)
            : base(graphicsDevice)
        {
            // Create vertex declaration
            skyVertexDeclaration = new VertexDeclaration(graphicsDevice,
                VertexPositionNormalTexture.VertexElements);

            // Create sky effect
            skyEffect = new BasicEffect(graphicsDevice, null);
            skyEffect.World = Matrix.Identity;
            skyEffect.TextureEnabled = true;
            skyEffect.LightingEnabled = false;
            skyEffect.AmbientLightColor = new Vector3(1f, 1f, 1f);
            skyEffect.View = Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);

            // Build sky quad
            BuildSky();

            // Store path
            this.arena2Path = arena2Path;

            // Set default index and frame
            SetSkyIndex(defaultSkyIndex);
            SetSkyFrame(defaultSkyFrame);
        }

        #endregion

        #region Abstract Overrides

        /// <summary>
        /// Called when view component should redraw.
        /// </summary>
        public override void Draw(Camera camera)
        {
            if (this.Enabled && camera != null)
            {
                DrawSky(camera);
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
        private void DrawSky(Camera camera)
        {
            // Set render states
            GraphicsDevice.RenderState.DepthBufferEnable = false;
            GraphicsDevice.RenderState.AlphaBlendEnable = false;
            GraphicsDevice.RenderState.AlphaTestEnable = false;
            GraphicsDevice.RenderState.CullMode = CullMode.None;

            // Set sampler state
            GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
            GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Clamp;

            // Set zero anisotropy
            GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Linear;
            GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Linear;
            GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Linear;
            GraphicsDevice.SamplerStates[0].MaxAnisotropy = 0;

            // Set vertex declaration
            GraphicsDevice.VertexDeclaration = skyVertexDeclaration;

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
            GraphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList,
                skyVertices,
                0,
                skyVertices.Length,
                skyIndices,
                0,
                skyIndices.Length / 3);
        }

        /// <summary>
        /// Set texture set of current sky.
        /// </summary>
        /// <param name="skyIndex">Index.</param>
        private void SetSkyIndex(int skyIndex)
        {
            // Validate path
            if (string.IsNullOrEmpty(arena2Path))
                return;

            // Validate index
            if (skyIndex < 0 || skyIndex > 31)
                return;

            // Store new value
            this.skyIndex = skyIndex;

            // Load a sample sky file
            string filename = string.Format("SKY{0:00}.DAT", skyIndex);
            skyFile = new SkyFile(
                Path.Combine(arena2Path, filename),
                FileUsage.UseDisk,
                true);

            // Update textures
            UpdateTextures();
        }

        /// <summary>
        /// Set frame of sky to draw.
        /// </summary>
        /// <param name="skyFrame">Frame.</param>
        private void SetSkyFrame(int skyFrame)
        {
            // Validate path
            if (string.IsNullOrEmpty(arena2Path))
                return;

            // Validate frame
            if (skyFrame < 0 || skyFrame > 63)
                return;

            // Store new value
            this.skyFrame = skyFrame;

            // Update textures
            UpdateTextures();
        }

        /// <summary>
        /// Loads textures for current time of day.
        ///  Will exchange east and west and reverse animation for afternoon times.
        /// </summary>
        private void UpdateTextures()
        {
            // Get images for both sides of sky
            DFBitmap west, east;
            if (skyFrame < 32)
            {
                skyFile.Palette = skyFile.GetDFPalette(skyFrame);
                west = skyFile.GetBitmapFormat(0, skyFrame, 0, DFBitmap.Formats.ARGB);
                east = skyFile.GetBitmapFormat(1, skyFrame, 0, DFBitmap.Formats.ARGB);
            }
            else
            {
                skyFile.Palette = skyFile.GetDFPalette(63 - skyFrame);
                east = skyFile.GetBitmapFormat(0, 63 - skyFrame, 0, DFBitmap.Formats.ARGB);
                west = skyFile.GetBitmapFormat(1, 63 - skyFrame, 0, DFBitmap.Formats.ARGB);
            }

            // Create textures
            westTexture = new Texture2D(GraphicsDevice, west.Width, west.Height, 1, TextureUsage.None, SurfaceFormat.Color);
            eastTexture = new Texture2D(GraphicsDevice, east.Width, east.Height, 1, TextureUsage.None, SurfaceFormat.Color);

            // Set data
            westTexture.SetData<byte>(0, null, west.Data, 0, west.Width * west.Height * 4, SetDataOptions.None);
            eastTexture.SetData<byte>(0, null, east.Data, 0, west.Width * west.Height * 4, SetDataOptions.None);

            // Sample clear colour from bottom of sky image
            int pos = west.Data.Length - 4;
            byte a = west.Data[pos + 3];
            byte r = west.Data[pos + 2];
            byte g = west.Data[pos + 1];
            byte b = west.Data[pos];
            clearColor.A = a;
            clearColor.R = r;
            clearColor.G = g;
            clearColor.B = b;
        }

        #endregion

    }

}
