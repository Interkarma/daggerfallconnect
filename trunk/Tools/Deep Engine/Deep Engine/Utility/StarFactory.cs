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
    /// Creates random starfields using simple noise.
    /// </summary>
    public class StarFactory
    {

        #region Fields

        DeepCore core;

        int starMapResolution = 2048;
        float threshold = 0.997f;

        GraphicsDevice graphicsDevice;
        FullScreenQuad quadRenderer;
        Texture2D starMap;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the last star map texture generated.
        /// </summary>
        public Texture2D StarMap
        {
            get { return starMap; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="graphicsDevice">GraphicsDevice.</param>
        /// <param name="contentManager">Content manager for loading PerlinNoise.fx</param>
        public StarFactory(DeepCore core)
        {
            // Setup graphics
            this.core = core;
            this.graphicsDevice = core.GraphicsDevice;
            this.quadRenderer = core.Renderer.FullScreenQuad;

            // Create default star map
            starMap = GetStarMap(0);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a random noise image.
        /// </summary>
        /// <param name="resolution">Resolution of image.</param>
        /// <returns>Noise texture.</returns>
        private Texture2D GetStarMap(int seed)
        {
            // Create random image
            Random rnd = new Random(seed);
            Color[] colors = new Color[starMapResolution * starMapResolution];
            for (int y = 0; y < starMapResolution; y++)
            {
                for (int x = 0; x < starMapResolution; x++)
                {
                    // Only accept values over threshold
                    float value = (float)rnd.Next(1000) / 1000.0f;
                    if (value >= threshold)
                    {
                        colors[y * starMapResolution + x] = new Color(new Vector3(value, value, value));
                    }
                }
            }

            // Create texture
            starMap = new Texture2D(graphicsDevice, starMapResolution, starMapResolution, false, SurfaceFormat.Color);
            starMap.SetData<Color>(colors);

            return starMap;
        }

        #endregion

    }

}
