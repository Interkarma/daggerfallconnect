// Project:         Deep Engine
// Description:     3D game engine for Ruins of Hill Deep and Daggerfall Workshop projects.
// Copyright:       Copyright (C) 2012 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statement
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
#endregion

namespace SceneEditor.UserControls
{

    /// <summary>
    /// Terrain editor control.
    /// </summary>
    public partial class TerrainEditor : UserControl
    {

        #region Fields

        const int previewDimension = 64;

        int mapDimension;
        int leafDimension;
        int gridDivisions;
        int levelCount;

        Bitmap previewImage;

        float[] perlinMapData;
        float[] heightMapData;
        byte[] blendMapData;

        bool manuallyPainted = false;

        public event EventHandler OnHeightMapChanged;
        public event EventHandler OnBlendMapChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets dimension of maps.
        /// </summary>
        public int MapDimension
        {
            get { return mapDimension; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public TerrainEditor()
        {
            InitializeComponent();

            // Set default maps
            InitialiseMaps(512);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set cursor position in maps.
        /// </summary>
        /// <param name="u">U position.</param>
        /// <param name="v">V position.</param>
        public void SetCursorPosition(float u, float v)
        {
            // Get coordinates
            int x = (int)((float)this.HeightMapPreview.Width * u) - 2;
            int y = (int)((float)this.HeightMapPreview.Height * v) - 2;

            // Position crosshair
            CrosshairImage.Location = new Point(x, y);
        }

        /// <summary>
        /// Gets heightmap data.
        ///  Array is equal to MapDimension*MapDimension elements.
        /// </summary>
        public float[] GetHeightMapData()
        {
            return heightMapData;
        }

        /// <summary>
        /// Gets blendmap as an RGBA byte array.
        ///  Array is equal to MapDimension*MapDimension elements long.
        /// </summary>
        public byte[] GetBlendMapData()
        {
            return blendMapData;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates a new set of maps with the specified dimension and levels.
        ///  Any current maps will be lost.
        /// </summary>
        /// <param name="dimension">Dimension of map.</param>
        private void InitialiseMaps(int dimension)
        {
            // Maps must always subdivide into 128x128 or smaller leaf nodes.
            // This is to ensure each leaf tile fits within a single vertex buffer.
            int leafDimension = dimension;
            int levelCount = 0;
            while (leafDimension > 128)
            {
                levelCount++;
                leafDimension /= 2;
            }

            // Store values
            this.mapDimension = dimension;
            this.leafDimension = leafDimension;
            this.gridDivisions = dimension / leafDimension;
            this.levelCount = levelCount;

            // Create map data
            perlinMapData = new float[dimension * dimension];
            heightMapData = new float[dimension * dimension];
            blendMapData = new byte[dimension * dimension * 4];

            // Create preview image
            previewImage = new Bitmap(previewDimension, previewDimension);
            HeightMapPreview.Image = previewImage;

            // Create initial perlin map
            GenerateNoise((int)GlobalSeedUpDown.Value);
            GeneratePerlinMap();

            // Set initial preview
            UpdatePreview();
        }

        /// <summary>
        /// Warn user a change is about to overwrite their manually painted changes.
        /// </summary>
        private bool WarnOverwrite()
        {
            if (manuallyPainted)
            {
                DialogResult result = MessageBox.Show("Your manually painted changes will be overwritten.", "Overwrite Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.OK)
                    return true;
                else
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Called whenever height map has been globally modified.
        /// </summary>
        private void UpdateHeightMapGlobal()
        {
            // Update preview image
            UpdatePreview();

            // Raise event
            if (OnHeightMapChanged != null)
                OnHeightMapChanged(this, null);
        }

        /// <summary>
        /// Called whenever blend map has been globally modified.
        /// </summary>
        private void UpdateBlendMapGlobal()
        {
            // Raise event
            if (OnBlendMapChanged != null)
                OnBlendMapChanged(this, null);
        }

        #endregion

        #region Image Processing Methods

        /// <summary>
        /// Gets averaged perlin data.
        /// </summary>
        /// <param name="x">X position in source data.</param>
        /// <param name="y">Y position in source data.</param>
        /// <param name="dimension">Width*Height dimension of source data.</param>
        /// <param name="data">Source data array.</param>
        /// <returns>Averaged value.</returns>
        private float AverageSample(int x, int y, int dimension, float[] data)
        {
            int x1 = (x + 1 >= dimension) ? dimension - 1 : x + 1;
            int y1 = (y + 1 >= dimension) ? dimension - 1 : y + 1;

            float value00 = data[y * dimension + x];
            float value10 = data[y * dimension + x1];
            float value01 = data[y1 * dimension + x];
            float value11 = data[y1 * dimension + x1];

            float value = (value00 + value10 + value01 + value11) / 4f;

            return value;
        }

        /// <summary>
        /// C# implementation of frac function.
        /// </summary>
        /// <param name="value">Value in.</param>
        /// <returns>Value out.</returns>
        private decimal Frac(decimal value)
        {
            return value - Math.Truncate(value);
        }

        /// <summary>
        /// Repaints blend map based on height.
        /// </summary>
        private void RepaintBlendMap()
        {
            int pos = 0;
            byte r, g, b, a;
            for (int y = 0; y < mapDimension; y++)
            {
                for (int x = 0; x < mapDimension; x++)
                {
                    // Set blend weights
                    float height = (heightMapData[y * mapDimension + x] * 0.5f + 0.5f);
                    float x1 = Microsoft.Xna.Framework.MathHelper.Clamp(1.0f - Math.Abs(height - 0.2f) / 0.2f, 0f, 1f);
                    float y1 = Microsoft.Xna.Framework.MathHelper.Clamp(1.0f - Math.Abs(height - 0.45f) / 0.25f, 0f, 1f);
                    float z1 = Microsoft.Xna.Framework.MathHelper.Clamp(1.0f - Math.Abs(height - 0.7f) / 0.25f, 0f, 1f);
                    float w1 = Microsoft.Xna.Framework.MathHelper.Clamp(1.0f - Math.Abs(height - 0.95f) / 0.25f, 0f, 1f);

                    // Get blend pixel
                    r = (byte)(255f * x1);
                    g = (byte)(255f * y1);
                    b = (byte)(255f * z1);
                    a = (byte)(255f * w1);

                    // Set blend pixel
                    blendMapData[pos++] = r;
                    blendMapData[pos++] = g;
                    blendMapData[pos++] = b;
                    blendMapData[pos++] = a;
                }
            }
        }

        /// <summary>
        /// Regenerates perlin map. This map is used for perlin operations like raise/lower
        ///  and generating a whole terrain from perlin noide.
        /// </summary>
        private void GeneratePerlinMap()
        {
            // Generate perlin map
            for (int y = 0; y < mapDimension; y++)
            {
                for (int x = 0; x < mapDimension; x++)
                {
                    perlinMapData[y * mapDimension + x] =
                        GetRandomHeight(x, y, 1.0f, (float)GlobalFrequencyUpDown.Value, (float)GlobalAmplitudeUpDown.Value, 0.5f, 8);
                }
            }
        }

        /// <summary>
        /// Smooths height map data.
        /// </summary>
        private void SmoothHeightMap()
        {
            for (int y = 0; y < mapDimension; y++)
            {
                for (int x = 0; x < mapDimension; x++)
                {
                    heightMapData[y * mapDimension + x] = AverageSample(x, y, mapDimension, heightMapData);
                }
            }
        }

        /// <summary>
        /// Updates preview image.
        /// </summary>
        private void UpdatePreview()
        {
            // Create grayscale preview of heightmap data
            for (int y = 0; y < mapDimension; y++)
            {
                float v = (float)y / (float)mapDimension;
                for (int x = 0; x < mapDimension; x++)
                {
                    float u = (float)x / (float)mapDimension;

                    // Get source colour value
                    float half = (float)byte.MaxValue / 2f;
                    byte value = (byte)(half * heightMapData[y * mapDimension + x] + half);
                    Color color = Color.FromArgb(value, value, value);

                    // Calculate destination position and set colour
                    int xpos = (int)((float)previewImage.Width * u);
                    int ypos = (int)((float)previewImage.Height * v);
                    previewImage.SetPixel(xpos, ypos, color);
                }
            }

            // Refresh preview picturebox
            HeightMapPreview.Refresh();
        }

        #endregion

        #region TerrainEditor Events

        /// <summary>
        /// Seed value changed.
        /// </summary>
        private void GlobalSeedUpDown_ValueChanged(object sender, EventArgs e)
        {
            GenerateNoise((int)GlobalSeedUpDown.Value);
            GeneratePerlinMap();
        }

        /// <summary>
        /// Frequency changed.
        /// </summary>
        private void GlobalFrequencyUpDown_ValueChanged(object sender, EventArgs e)
        {
            GeneratePerlinMap();
        }

        /// <summary>
        /// Amplitude changed.
        /// </summary>
        private void GlobalAmplitudeUpDown_ValueChanged(object sender, EventArgs e)
        {
            GeneratePerlinMap();
        }

        /// <summary>
        /// Uniform raise clicked.
        /// </summary>
        private void UniformRaiseButton_Click(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Uniform lower clicked. 
        /// </summary>
        private void UniformLowerButton_Click(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Perlin raise clicked.
        /// </summary>
        private void PerlinRaiseButton_Click(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Perlin lower clicked.
        /// </summary>
        private void PerlinLowerButton_Click(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Perlin button clicked.
        /// </summary>
        private void PerlinTerrainButton_Click(object sender, EventArgs e)
        {
            if (WarnOverwrite())
            {
                // Copy map from perlin settings
                heightMapData = (float[])perlinMapData.Clone();
                UpdateHeightMapGlobal();
            }
        }

        /// <summary>
        /// Smooth terrain button clicked.
        /// </summary>
        private void SmoothButton_Click(object sender, EventArgs e)
        {
            if (WarnOverwrite())
            {
                SmoothHeightMap();
                UpdateHeightMapGlobal();
            }
        }

        /// <summary>
        /// Auto paint blend map button clicked.
        /// </summary>
        private void AutoPaintButton_Click(object sender, EventArgs e)
        {
            if (WarnOverwrite())
            {
                RepaintBlendMap();
                UpdateBlendMapGlobal();
            }
        }

        #endregion

        #region Perlin Noise

        // Thanks to James Craig for this Perlin Noise code.
        // http://www.gutgames.com/post/Perlin-Noise.aspx

        private const int MAX_WIDTH = 256;
        private const int MAX_HEIGHT = 256;
        private float[,] noise;

        //Gets the value for a specific X and Y coordinate 
        private float GetRandomHeight(float x, float y, float maxHeight,
            float frequency, float amplitude, float persistance,
            int octaves)
        {
            float finalValue = 0.0f;
            for (int i = 0; i < octaves; ++i)
            {
                finalValue += GetSmoothNoise(x * frequency, y * frequency) * amplitude;
                frequency *= 2.0f;
                amplitude *= persistance;
            }
            if (finalValue < -1.0f)
            {
                finalValue = -1.0f;
            }
            else if (finalValue > 1.0f)
            {
                finalValue = 1.0f;
            }
            return finalValue * maxHeight;
        }

        //This function is a simple bilinear filtering function which is good enough. 
        //You can do cosine or bicubic if you really want though. 
        private float GetSmoothNoise(float x, float y)
        {
            float fractionX = x - (int)x;
            float fractionY = y - (int)y;
            int x1 = ((int)x + MAX_WIDTH) % MAX_WIDTH;
            int y1 = ((int)y + MAX_HEIGHT) % MAX_HEIGHT;

            //for cool art deco looking images, do +1 for X2 and Y2 instead of -1...
            int x2 = ((int)x + MAX_WIDTH - 1) % MAX_WIDTH;
            int y2 = ((int)y + MAX_HEIGHT - 1) % MAX_HEIGHT;

            float finalValue = 0.0f;
            finalValue += fractionX * fractionY * noise[x1, y1];
            finalValue += fractionX * (1 - fractionY) * noise[x1, y2];
            finalValue += (1 - fractionX) * fractionY * noise[x2, y1];
            finalValue += (1 - fractionX) * (1 - fractionY) * noise[x2, y2];

            return finalValue;
        }

        private void GenerateNoise(int seed)
        {
            noise = new float[MAX_WIDTH, MAX_HEIGHT];   //Create the noise table where MAX_WIDTH and MAX_HEIGHT are set to some value>0
            Random randomGenerator = new Random(seed);  //Create the random generator (just using C#'s at the moment)
            for (int x = 0; x < MAX_WIDTH; ++x)
            {
                for (int y = 0; y < MAX_HEIGHT; ++y)
                {
                    noise[x, y] = ((float)(randomGenerator.NextDouble()) - 0.5f) * 2.0f;  //Generate noise between -1 and 1
                }
            }
        }

        #endregion

    }

}
