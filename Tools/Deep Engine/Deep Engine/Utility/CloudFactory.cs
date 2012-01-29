// Project:         Deep Engine
// Description:     3D game engine for Ruins of Hill Deep and Daggerfall Workshop projects.
// Copyright:       Copyright (C) 2012 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using DeepEngine.Core;
using DeepEngine.Rendering;
#endregion

namespace DeepEngine.Utility
{

    /// <summary>
    /// Creates variable and animated cloud textures using Perlin noise.
    /// </summary>
    public class CloudFactory
    {

        #region Fields

        DeepCore core;

        int staticMapResolution = 32;
        Vector2 cloudMapResolution = new Vector2(2048, 2048);

        GraphicsDevice graphicsDevice;
        FullScreenQuad quadRenderer;
        RenderTarget2D cloudMap;
        Texture2D staticMap;
        Effect perlinEffect;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="graphicsDevice">GraphicsDevice.</param>
        /// <param name="contentManager">Content manager for loading PerlinNoise.fx</param>
        public CloudFactory(DeepCore core)
        {
            // Setup graphics
            this.core = core;
            this.graphicsDevice = core.GraphicsDevice;
            this.quadRenderer = new FullScreenQuad(graphicsDevice);

            // Create static map
            staticMap = CreateStaticMap(staticMapResolution);

            // Create clouds render target
            cloudMap = new RenderTarget2D(
                graphicsDevice,
                (int)cloudMapResolution.X,
                (int)cloudMapResolution.Y,
                false,
                SurfaceFormat.Color,
                DepthFormat.None);

            // Load effects
            perlinEffect = core.ContentManager.Load<Effect>("Effects/PerlinNoiseEffect");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets clouds updated by time.
        /// </summary>
        /// <param name="time">Time value.</param>
        /// <param name="brightness">Brightness of clouds.</param>
        /// <returns>Generated cloud texture.</returns>
        public Texture2D GetClouds(float time, float brightness)
        {
            GeneratePerlinNoise(time, brightness);
            return cloudMap;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Draws Perlin noise to render target.
        /// </summary>
        /// <param name="time">Time value.</param>
        /// <param name="brightness">Brightness of clouds.</param>
        private void GeneratePerlinNoise(float time, float brightness)
        {
            if (cloudMap.IsContentLost)
            {
                // Re-create clouds render target
                cloudMap = new RenderTarget2D(
                    graphicsDevice,
                    (int)cloudMapResolution.X,
                    (int)cloudMapResolution.Y,
                    false,
                    SurfaceFormat.Color,
                    DepthFormat.None);
            }

            // Save viewport
            Viewport viewport = graphicsDevice.Viewport;

            // Set render target
            graphicsDevice.SetRenderTarget(cloudMap);
            graphicsDevice.Clear(Color.Transparent);

            // Setup effect
            perlinEffect.CurrentTechnique = perlinEffect.Techniques["PerlinNoise"];
            perlinEffect.Parameters["Texture"].SetValue(staticMap);
            perlinEffect.Parameters["Overcast"].SetValue(1.4f);
            perlinEffect.Parameters["Brightness"].SetValue(brightness);
            perlinEffect.Parameters["Time"].SetValue(time / 1000.0f);

            // Draw effect
            perlinEffect.CurrentTechnique.Passes[0].Apply();
            quadRenderer.Draw(graphicsDevice);

            // Reset render target
            graphicsDevice.SetRenderTarget(null);

            // Restore viewport
            graphicsDevice.Viewport = viewport;
        }

        /// <summary>
        /// Creates a random noise image.
        /// </summary>
        /// <param name="resolution">Resolution of image.</param>
        /// <returns>Noise texture.</returns>
        private Texture2D CreateStaticMap(int resolution)
        {
            // Create random image
            Random rand = new Random(0);
            Color[] noisyColors = new Color[resolution * resolution];
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    noisyColors[y * resolution + x] = 
                        new Color(new Vector3((float)rand.Next(1000) / 1000.0f, 0, 0));
                }
            }

            // Create texture
            Texture2D noiseImage = new Texture2D(
                graphicsDevice, resolution, resolution, false, SurfaceFormat.Color);
            noiseImage.SetData<Color>(noisyColors);

            return noiseImage;
        }

        #endregion

    }

}
