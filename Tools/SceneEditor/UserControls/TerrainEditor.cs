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

        int mapDimension;
        int leafDimension;
        int gridDivisions;
        int levelCount;

        Bitmap heightMap;
        Bitmap blendMap;
        Bitmap foliageMap;
        Bitmap previewImage;

        byte[] perlinMapData;
        byte[] heightMapData;
        byte[] blendMapData;

        const int formatWidth = 4;

        bool manuallyModified = false;

        public event EventHandler OnHeightMapChanged;

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
        /// Gets heightmap as a color array.
        ///  Array is equal to MapDimension*MapDimension elements.
        /// </summary>
        public Color[] GetHeightMapColorArray()
        {
            // Create array
            Color[] colors = new Color[mapDimension * mapDimension];

            // Get colors
            for (int y = 0; y < heightMap.Height; y++)
            {
                for (int x = 0; x < heightMap.Width; x++)
                {
                    colors[y * heightMap.Width + x] = heightMap.GetPixel(x, y);
                }
            }

            return colors;
        }

        /// <summary>
        /// Gets blendmap as a color array.
        ///  Array is equal to MapDimension*MapDimension elements.
        /// </summary>
        public Color[] GetBlendMapColorArray()
        {
            // Create array
            Color[] colors = new Color[mapDimension * mapDimension];

            // Get colors
            for (int y = 0; y < blendMap.Height; y++)
            {
                for (int x = 0; x < blendMap.Width; x++)
                {
                    colors[y * blendMap.Width + x] = blendMap.GetPixel(x, y);
                }
            }

            return colors;
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

            // Create maps
            heightMap = new Bitmap(dimension, dimension);
            blendMap = new Bitmap(dimension, dimension);
            foliageMap = new Bitmap(dimension, dimension);

            // Create preview image
            previewImage = new Bitmap(HeightMapPreview.Width, HeightMapPreview.Height);
            HeightMapPreview.Image = previewImage;

            // Create initial perlin map
            GenerateNoise((int)GlobalSeedUpDown.Value);
            perlinMapData = new byte[dimension * dimension];
            GeneratePerlinMap();
        }

        /// <summary>
        /// Warn user a change is about to overwrite their manual changes.
        /// </summary>
        private bool WarnOverwrite()
        {
            if (manuallyModified)
            {
                DialogResult result = MessageBox.Show("Your manual changes will be overwritten.", "Overwrite Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.OK)
                    return true;
                else
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Regenerates perlin map.
        /// </summary>
        private void GeneratePerlinMap()
        {
            for (int y = 0; y < heightMap.Height; y++)
            {
                for (int x = 0; x < heightMap.Width; x++)
                {
                    float value = GetRandomHeight(x, y, 127.5f, (float)GlobalFrequencyUpDown.Value, (float)GlobalAmplitudeUpDown.Value, 0.5f, 16) + 127.5f;
                    perlinMapData[y * heightMap.Width + x] = (byte)value;
                }
            }
        }

        /// <summary>
        /// Repaints blend map after height changes.
        /// </summary>
        private void RepaintBlendMap()
        {
            for (int y = 0; y < heightMap.Height; y++)
            {
                for (int x = 0; x < heightMap.Width; x++)
                {
                    // Set blend weights
                    float height = (float)heightMap.GetPixel(x, y).R / 255f;
                    float x1 = Microsoft.Xna.Framework.MathHelper.Clamp(1.0f - Math.Abs(height - 0.2f) / 0.2f, 0f, 1f);
                    float y1 = Microsoft.Xna.Framework.MathHelper.Clamp(1.0f - Math.Abs(height - 0.45f) / 0.25f, 0f, 1f);
                    float z1 = Microsoft.Xna.Framework.MathHelper.Clamp(1.0f - Math.Abs(height - 0.7f) / 0.25f, 0f, 1f);
                    float w1 = Microsoft.Xna.Framework.MathHelper.Clamp(1.0f - Math.Abs(height - 0.95f) / 0.25f, 0f, 1f);
                    
                    // Set blend pixel
                    int r = (int)(255f * x1);
                    int g = (int)(255f * y1);
                    int b = (int)(255f * z1);
                    int a = (int)(255f * w1);
                    blendMap.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                }
            }
        }

        /// <summary>
        /// Updates preview map after height change.
        /// </summary>
        private void UpdatePreview()
        {
            // Get handle to destination image
            Graphics gr = Graphics.FromImage(HeightMapPreview.Image);

            // Draw preview
            Rectangle dstRect = new Rectangle(0, 0, previewImage.Width, previewImage.Height);
            gr.DrawImage(heightMap, dstRect);
            HeightMapPreview.Refresh();

            // Raise event
            if (OnHeightMapChanged != null)
                OnHeightMapChanged(this, null);
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
        /// Min terrain clicked.
        /// </summary>
        private void MinTerrainButton_Click(object sender, EventArgs e)
        {
            if (WarnOverwrite())
            {
                // Set entire terrain to min
                Graphics gr = Graphics.FromImage(heightMap);
                gr.Clear(Color.Black);
                RepaintBlendMap();
                UpdatePreview();
            }
        }

        /// <summary>
        /// Max terrain clicked.
        /// </summary>
        private void MaxTerrainButton_Click(object sender, EventArgs e)
        {
            if (WarnOverwrite())
            {
                // Set entire terrain to max
                Graphics gr = Graphics.FromImage(heightMap);
                gr.Clear(Color.White);
                RepaintBlendMap();
                UpdatePreview();
            }
        }

        /// <summary>
        /// Perlin button clicked.
        /// </summary>
        private void PerlinTerrainButton_Click(object sender, EventArgs e)
        {
            if (WarnOverwrite())
            {
                // Generate new map from perlin settings
                for (int y = 0; y < heightMap.Height; y++)
                {
                    for (int x = 0; x < heightMap.Width; x++)
                    {
                        byte value = perlinMapData[y * heightMap.Width + x];
                        heightMap.SetPixel(x, y, Color.FromArgb(value, value, value));
                    }
                }
                RepaintBlendMap();
                UpdatePreview();
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
