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
using System.Drawing.Imaging;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;
using DeepEngine.Core;
using DeepEngine.Components;
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
        const int formatWidth = 4;

        string arena2Path;
        ImageFileReader imageReader = new ImageFileReader();

        int mapDimension;
        int leafDimension;
        int gridDivisions;
        int levelCount;

        Bitmap previewImage;
        byte[] previewImageData;

        QuadTerrainComponent terrain = null;

        //float[] perlinMapData;
        float[] heightMapData;
        byte[] blendMap0Data;
        byte[] blendMap1Data;

        bool manuallyPainted = false;

        PointF? cursorPosition = null;
        CursorEditAction currentEditAction = CursorEditAction.DeformUpDown;
        int brushRadius = 32;
        float brushStrength = 0.5f;
        bool deformInProgress = false;
        float deformStartValue;

        float[] heightMapUndo = null;
        float[] radialBrush = null;

        public event EventHandler OnHeightMapChanged;
        public event EventHandler OnBlendMapChanged;

        #endregion

        #region Structures

        /// <summary>
        /// Edit action to perform under the cursor.
        /// </summary>
        public enum CursorEditAction
        {
            DeformUpDown,
            DeformSmooth,
            DeformBumps,
            Paint,
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets dimension of maps.
        /// </summary>
        public int MapDimension
        {
            get { return mapDimension; }
        }

        /// <summary>
        /// Gets flag stating if a deform is in progress.
        /// </summary>
        public bool DeformInProgress
        {
            get { return deformInProgress; }
        }

        /// <summary>
        /// Gets current edit action.
        /// </summary>
        public CursorEditAction CurrentEditAction
        {
            get { return currentEditAction; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public TerrainEditor()
        {
            InitializeComponent();
            
            // Set default values
            BrushRadiusUpDown.Value = brushRadius;
            BrushStrengthUpDown.Value = (decimal)brushStrength;

            // Create initial brush
            radialBrush = CreateRadialBrush(brushRadius);

            // Create preview image
            previewImage = new Bitmap(previewDimension, previewDimension, PixelFormat.Format32bppArgb);
            previewImageData = new byte[previewDimension * previewDimension * formatWidth];
            HeightMapPreview.Image = previewImage;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets terrain component to edit.
        /// </summary>
        /// <param name="terrain">Terrain component.</param>
        public void SetTerrain(QuadTerrainComponent terrain)
        {
            // Store reference to terrain
            this.terrain = terrain;

            // Get arena2 path from active core
            this.arena2Path = terrain.Core.Arena2Path;
            this.imageReader.Arena2Path = this.arena2Path;

            // Load terrain maps
            InitialiseMaps(terrain);

            // Load texture previews
            InitialiseTexturePreviews(terrain);
        }

        /// <summary>
        /// Stop editing a terrain.
        /// </summary>
        public void ClearTerrain()
        {
            this.terrain = null;
        }

        /// <summary>
        /// Set cursor position in maps.
        /// </summary>
        /// <param name="u">U position.</param>
        /// <param name="v">V position.</param>
        public void SetCursorPosition(float u, float v)
        {
            // Do nothing if no terrain set
            if (terrain == null)
                return;

            // Cannot change while deforming up and down
            if (deformInProgress)
                return;

            // Get coordinates
            cursorPosition = new PointF(u, v);
            int x = (int)((float)this.HeightMapPreview.Width * u) - 2;
            int y = (int)((float)this.HeightMapPreview.Height * v) - 2;

            // Position crosshair
            CrosshairImage.Location = new Point(x, y);
            CrosshairImage.Visible = true;
        }

        /// <summary>
        /// Clears the cursor position.
        /// </summary>
        public void ClearCursorPosition()
        {
            // Do nothing if no terrain set
            if (terrain == null)
                return;

            // Cannot change while deforming up and down
            if (deformInProgress)
                return;

            cursorPosition = null;
            CrosshairImage.Visible = false;
        }

        /// <summary>
        /// Begins deforming terrain at current cursor position.
        /// </summary>
        /// <param name="startValue">Starting value for relative deformation.</param>
        public void BeginDeformUpDown(float startValue)
        {
            // Do nothing if no terrain set
            if (terrain == null)
                return;

            // Cursor position must be valid
            if (cursorPosition == null)
                return;

            // Must be in deform up/down mode
            if (currentEditAction != CursorEditAction.DeformUpDown)
                return;

            // Store height map data under cursor
            SaveHeightMapUndo();

            // Start deform
            deformInProgress = true;
            deformStartValue = startValue;
        }

        /// <summary>
        /// Sets terrain deformation at cursor position.
        ///  Must call BeginDeformUpDown() first.
        /// </summary>
        /// <param name="currentValue">Current value for deformation relative to start value.</param>
        public void SetDeformUpDown(float currentValue)
        {
            // Do nothing if no terrain set
            if (terrain == null)
                return;

            // Must be in correct edit action
            if (currentEditAction == CursorEditAction.DeformUpDown && deformInProgress)
            {
                DeformHeightMap(currentValue);
            }
        }

        /// <summary>
        /// Completes deformation process.
        /// </summary>
        public void EndDeformUpDown()
        {
            // Do nothing if no terrain set
            if (terrain == null)
                return;

            // Must be in correct edit action
            if (currentEditAction == CursorEditAction.DeformUpDown && deformInProgress)
            {
                deformInProgress = false;

                // Update preview
                UpdatePreview();
            }
        }

        /// <summary>
        /// Cancels a deformation process and restores starting height pixels.
        /// </summary>
        public void CancelDeformUpDown()
        {
            // Do nothing if no terrain set
            if (terrain == null)
                return;

            // Must be in correct edit action
            if (currentEditAction == CursorEditAction.DeformUpDown && deformInProgress)
            {
            }
        }

        /// <summary>
        /// Paints the terrain at cursor using brush and texture.
        /// </summary>
        public void PaintTerrain()
        {
            // Do nothing if no terrain set
            if (terrain == null)
                return;

            // Must be in correct edit action
            if (currentEditAction == CursorEditAction.Paint)
            {
                PaintBlendMap();
            }
        }

        /// <summary>
        /// Gets heightmap data.
        ///  Array is equal to MapDimension*MapDimension elements.
        /// </summary>
        /// <returns>Height map array, or NULL if no terrain set.</returns>
        public float[] GetHeightMapData()
        {
            // Do nothing if no terrain set
            if (terrain == null)
                return null;

            return heightMapData;
        }

        /// <summary>
        /// Gets blend map data based on selected texture.
        /// </summary>
        /// <param name="data">Blend data array out.</param>
        /// <param name="channel">Blend channel offset out.</param>
        /// <param name="texture">Texture index out.</param>
        public void GetBlendMapData(out byte[] data, out int channel, out int texture)
        {
            data = null;
            channel = -1;
            texture = -1;

            // Do nothing if no terrain set
            if (terrain == null)
                return;

            // Get based on texture index
            if (Texture0RadioButton.Checked)
            {
                texture = 0;
            }
            else if (Texture1RadioButton.Checked)
            {
                data = GetBlendMap0Data();
                channel = 0;
                texture = 1;
            }
            else if (Texture2RadioButton.Checked)
            {
                data = GetBlendMap0Data();
                channel = 1;
                texture = 2;
            }
            else if (Texture3RadioButton.Checked)
            {
                data = GetBlendMap0Data();
                channel = 2;
                texture = 3;
            }
            else if (Texture4RadioButton.Checked)
            {
                data = GetBlendMap0Data();
                channel = 3;
                texture = 4;
            }
            else if (Texture5RadioButton.Checked)
            {
                data = GetBlendMap1Data();
                channel = 0;
                texture = 5;
            }
            else if (Texture6RadioButton.Checked)
            {
                data = GetBlendMap1Data();
                channel = 1;
                texture = 6;
            }
            else if (Texture7RadioButton.Checked)
            {
                data = GetBlendMap1Data();
                channel = 2;
                texture = 7;
            }
            else if (Texture8RadioButton.Checked)
            {
                data = GetBlendMap1Data();
                channel = 3;
                texture = 8;
            }
        }

        /// <summary>
        /// Gets blendmap0 as an RGBA byte array.
        ///  Array is equal to MapDimension*MapDimension elements long.
        /// </summary>
        /// <returns>Blend map array, or NULL if no terrain set.</returns>
        public byte[] GetBlendMap0Data()
        {
            // Do nothing if no terrain set
            if (terrain == null)
                return null;

            return blendMap0Data;
        }

        /// <summary>
        /// Gets blendmap1 as an RGBA byte array.
        ///  Array is equal to MapDimension*MapDimension elements long.
        /// </summary>
        /// <returns>Blend map array, or NULL if no terrain set.</returns>
        public byte[] GetBlendMap1Data()
        {
            // Do nothing if no terrain set
            if (terrain == null)
                return null;

            return blendMap1Data;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sync editor maps with terrain maps.
        /// </summary>
        /// <param name="terrain">Terrain to sync maps from.</param>
        private void InitialiseMaps(QuadTerrainComponent terrain)
        {
            // Store values
            this.mapDimension = terrain.MapDimension;
            this.leafDimension = terrain.LeafDimension;
            this.gridDivisions = mapDimension / leafDimension;
            this.levelCount = terrain.LevelCount;

            // Create perlin map data
            //perlinMapData = new float[mapDimension * mapDimension];

            // Get map data
            heightMapData = terrain.GetHeight();
            blendMap0Data = terrain.GetBlend(0);
            blendMap1Data = terrain.GetBlend(1);

            // Create initial perlin map
            //GenerateNoise((int)GlobalSeedUpDown.Value);
            //GeneratePerlinMap();

            // Set initial preview
            UpdatePreview();
        }

        /// <summary>
        /// Sync editor textures with terrain.
        /// </summary>
        /// <param name="terrain">Terrain to sync textures from.</param>
        private void InitialiseTexturePreviews(QuadTerrainComponent terrain)
        {
            // Set each texture preview
            SetTexturePreview(terrain, 0, Texture0PictureBox);
            SetTexturePreview(terrain, 1, Texture1PictureBox);
            SetTexturePreview(terrain, 2, Texture2PictureBox);
            SetTexturePreview(terrain, 3, Texture3PictureBox);
            SetTexturePreview(terrain, 4, Texture4PictureBox);
            SetTexturePreview(terrain, 5, Texture5PictureBox);
            SetTexturePreview(terrain, 6, Texture6PictureBox);
            SetTexturePreview(terrain, 7, Texture7PictureBox);
            SetTexturePreview(terrain, 8, Texture8PictureBox);
        }

        /// <summary>
        /// Sets the texture preview from specified index.
        /// </summary>
        /// <param name="terrain">Terrain component.</param>
        /// <param name="index">Texture index.</param>
        /// <param name="pictureBox">Picture box to receive image.</param>
        private void SetTexturePreview(QuadTerrainComponent terrain, int index, PictureBox pictureBox)
        {
            // Get texture description from index
            QuadTerrainComponent.DaggerfallTerrainTexture textureDesc = terrain.GetDaggerfallTexture(index);

            // Get Daggerfall image file
            imageReader.LibraryType = LibraryTypes.Texture;
            DFImageFile imageFile = imageReader.GetImageFile(TextureFile.IndexToFileName(textureDesc.Archive));

            // Get managed bitmap and set on picture box
            Bitmap textureBitmap = imageFile.GetManagedBitmap(textureDesc.Record, 0, true, false);
            pictureBox.Image = textureBitmap;
            pictureBox.Refresh();
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
                {
                    manuallyPainted = false;
                    return true;
                }
                else
                {
                    return false;
                }
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
            // Clamp position
            int x1 = (x + 1 >= dimension) ? dimension - 1 : x + 1;
            int y1 = (y + 1 >= dimension) ? dimension - 1 : y + 1;

            // Get values
            float value00 = data[y * dimension + x];
            float value10 = data[y * dimension + x1];
            float value01 = data[y1 * dimension + x];
            float value11 = data[y1 * dimension + x1];

            // Average values
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
                    blendMap0Data[pos++] = r;
                    blendMap0Data[pos++] = g;
                    blendMap0Data[pos++] = b;
                    blendMap0Data[pos++] = a;
                }
            }
        }

        /// <summary>
        /// Regenerates perlin map. This map is used for perlin operations like raise/lower
        ///  and generating a whole terrain from perlin noise.
        /// </summary>
        private void GeneratePerlinMap()
        {
            /*
            // Generate perlin map
            for (int y = 0; y < mapDimension; y++)
            {
                for (int x = 0; x < mapDimension; x++)
                {
                    perlinMapData[y * mapDimension + x] =
                        GetRandomHeight(x, y, 1.0f, (float)GlobalFrequencyUpDown.Value, (float)GlobalAmplitudeUpDown.Value, 0.5f, 8);
                }
            }
            */
        }

        /// <summary>
        /// Smooth height map data.
        /// </summary>
        private void SmoothHeightMap()
        {
            // Make every height pixel an average of its surrounding height pixels
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
            const float halfByte = 127.5f;

            // Get bitmap rectangle
            Rectangle rect = new Rectangle(0, 0, previewImage.Width, previewImage.Height);

            // Create preview data
            for (int y = 0; y < rect.Height; y++)
            {
                float v = (float)y / (float)rect.Height;

                for (int x = 0; x < rect.Width; x++)
                {
                    float u = (float)x / (float)rect.Width;

                    // Get source data value
                    int xpos = (int)(mapDimension * u);
                    int ypos = (int)(mapDimension * v);
                    byte value = (byte)(halfByte * heightMapData[ypos * mapDimension + xpos] + halfByte);

                    // Set preview pixel
                    previewImage.SetPixel(x, y, Color.FromArgb(value, value, value));
                }
            }

            // Refresh preview picturebox
            HeightMapPreview.Refresh();
        }

        #endregion

        #region Global Toolbox Events

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
            /*
            if (WarnOverwrite())
            {
                // Copy map from perlin settings
                heightMapData = (float[])perlinMapData.Clone();
                UpdateHeightMapGlobal();
            }
            */
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

        #region Brush Toolbox Events

        /// <summary>
        /// Called when brush radius changed.
        /// </summary>
        private void BrushRadiusUpDown_ValueChanged(object sender, EventArgs e)
        {
            brushRadius = (int)BrushRadiusUpDown.Value;
            radialBrush = CreateRadialBrush(brushRadius);
        }

        /// <summary>
        /// Called when brush strength changed.
        /// </summary>
        private void BrushStrengthUpDown_ValueChanged(object sender, EventArgs e)
        {
            brushStrength = (float)BrushStrengthUpDown.Value;
        }

        #endregion

        #region Action Toolbox Events

        /// <summary>
        /// Clear existing check from action buttons.
        /// </summary>
        private void ClearActionButtonChecks()
        {
            DeformUpDownButton.Checked = false;
            DeformSmoothButton.Checked = false;
            DeformBumpsButton.Checked = false;
            ManualPaintButton.Checked = false;
        }

        /// <summary>
        /// Deform up/down button clicked.
        /// </summary>
        private void DeformUpDownButton_Click(object sender, EventArgs e)
        {
            ClearActionButtonChecks();
            currentEditAction = CursorEditAction.DeformUpDown;
            DeformUpDownButton.Checked = true;
        }

        /// <summary>
        /// Deform smooth button clicked.
        /// </summary>
        private void DeformSmoothButton_Click(object sender, EventArgs e)
        {
            ClearActionButtonChecks();
            currentEditAction = CursorEditAction.DeformSmooth;
            DeformSmoothButton.Checked = true;
        }

        /// <summary>
        /// Deform bumps button clicked.
        /// </summary>
        private void DeformBumpsButton_Click(object sender, EventArgs e)
        {
            ClearActionButtonChecks();
            currentEditAction = CursorEditAction.DeformBumps;
            DeformBumpsButton.Checked = true;
        }

        /// <summary>
        /// Deform paint button clicked.
        /// </summary>
        private void ManualPaintButton_Click(object sender, EventArgs e)
        {
            ClearActionButtonChecks();
            currentEditAction = CursorEditAction.Paint;
            ManualPaintButton.Checked = true;
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

        #region Brush Operations

        /// <summary>
        /// Creates radial brush.
        /// </summary>
        /// <param name="radius">Radius of new brush.</param>
        /// <param name="intensity">Intensity for attenuation equation.</param>
        /// <returns>New radial brush.</returns>
        private float[] CreateRadialBrush(int radius, float intensity = 1.05f)
        {
            // Create brush array
            int dimension = radius * 2;
            float[] brush = new float[dimension * dimension];
            Microsoft.Xna.Framework.Vector2 centre = new Microsoft.Xna.Framework.Vector2(radius, radius);

            // Populate brush array
            float distance;
            Microsoft.Xna.Framework.Vector2 pos;
            for (int y = 0; y < dimension; y++)
            {
                for (int x = 0; x < dimension; x++)
                {
                    // Get distance to centre pixel
                    pos.X = x;
                    pos.Y = y;
                    distance = Microsoft.Xna.Framework.Vector2.Distance(pos, centre);

                    // Compute attenuation based on distance
                    float value = (1.0f - distance / radius) * intensity;
                    value = Microsoft.Xna.Framework.MathHelper.Clamp(value, 0.0f, 1.0f);

                    // Set value
                    brush[y * dimension + x] = value;
                }
            }

            return brush;
        }

        /// <summary>
        /// Removes blend evenly across all channels at index position.
        /// </summary>
        /// <param name="mapPos">Position in blend map to descale.</param>
        /// <param name="excludedChannel">Channel to exclude. Use -1 to descale all channels.</param>
        /// <param name="amount">Amount to descale by.</param>
        private void DescaleBlendMaps(int mapPos, int excludedTexture, float amount)
        {
            // Amount to change in byte form
            byte change = (byte)(255.0f * amount);

            // Descale blend map 0
            for (int channel = 0; channel < 4; channel++)
            {
                // Skip excluded channel.
                if ((channel + 1) == excludedTexture)
                    continue;

                // Descale channel at position
                int value = blendMap0Data[mapPos + channel] - change;
                if (value < 0) value = 0;
                blendMap0Data[mapPos + channel] = (byte)value;
            }

            // Descale blend map 1
            for (int channel = 0; channel < 4; channel++)
            {
                // Skip excluded channel.
                if ((channel + 5) == excludedTexture)
                    continue;

                // Descale channel at position
                int value = blendMap1Data[mapPos + channel] - change;
                if (value < 0) value = 0;
                blendMap1Data[mapPos + channel] = (byte)value;
            }
        }

        /// <summary>
        /// Paints the terrain at cursor using selected texture.
        /// </summary>
        public void PaintBlendMap()
        {
            // Cannot do anything if cursor position invalid
            if (cursorPosition == null)
                return;

            // Get target blend map
            byte[] blendMapData;
            int channel = 0;
            int texture = 0;
            GetBlendMapData(out blendMapData, out channel, out texture);

            // Get start position of brush rectangle
            int brushDimension = brushRadius * 2;
            int brushStride = brushDimension * formatWidth;
            int xs = (int)(mapDimension * cursorPosition.Value.X - brushRadius);
            int ys = (int)(mapDimension * cursorPosition.Value.Y - brushRadius);

            // Get stride of blend map
            int blendMapStride = mapDimension * formatWidth;
            
            // Paint all valid blend pixels
            int brushx, brushy, brushPos, mapPos;
            float brushValue, startValue, newValue;
            for (int y = ys; y < ys + brushDimension; y++)
            {
                // Cannot go out of bounds
                if (y < 0 || y >= mapDimension)
                    continue;

                for (int x = xs; x < xs + brushDimension; x++)
                {
                    // Cannot go out of bounds
                    if (x < 0 || x >= mapDimension)
                        continue;

                    // Get map position
                    mapPos = (y * blendMapStride) + (x * formatWidth);

                    // Get brush value
                    brushx = (x - xs);
                    brushy = (y - ys);
                    if (brushx < 0 || brushx >= brushDimension) continue;
                    if (brushy < 0 || brushy >= brushDimension) continue;
                    brushPos = brushy * brushDimension + brushx;
                    brushValue = radialBrush[brushPos] * brushStrength * 0.05f;

                    if (blendMapData == null)
                    {
                        // Just descale everything when no blend map specified
                        DescaleBlendMaps(mapPos, -1, brushValue);
                    }
                    else
                    {
                        // Descale other channels to make room for new colour
                        DescaleBlendMaps(mapPos, texture, brushValue);

                        // Get start value
                        startValue = (float)blendMapData[mapPos + channel] / 255.0f;

                        // Get new value
                        newValue = Microsoft.Xna.Framework.MathHelper.Clamp(startValue + brushValue, 0.0f, 1.0f);

                        // Set new value
                        blendMapData[mapPos + channel] = (byte)(newValue * 255.0f);
                    }
                }
            }

            // Set manually painted flag
            manuallyPainted = true;

            // Raise update event
            OnBlendMapChanged(this, null);
        }

        /// <summary>
        /// Deforms the height map at cursor using brush.
        /// </summary>
        /// <param name="scale">Scale of deform operation.</param>
        private void DeformHeightMap(float scale)
        {
            // Cannot do anything if cursor position invalid
            if (cursorPosition == null)
                return;

            // Get start position of brush rectangle
            int dimension = brushRadius * 2;
            int xs = (int)(mapDimension * cursorPosition.Value.X - brushRadius);
            int ys = (int)(mapDimension * cursorPosition.Value.Y - brushRadius);

            // Deform all valid height pixels
            for (int y = ys; y < ys + dimension; y++)
            {
                // Cannot go out of bounds
                if (y < 0 || y >= mapDimension)
                    continue;

                for (int x = xs; x < xs + dimension; x++)
                {
                    // Cannot go out of bounds
                    if (x < 0 || x >= mapDimension)
                        continue;

                    // Get brush value
                    int brushx = (x - xs);
                    int brushy = (y - ys);
                    if (brushx < 0 || brushx >= dimension) continue;
                    if (brushy < 0 || brushy >= dimension) continue;
                    float brushValue = radialBrush[brushy * dimension + brushx] * brushStrength;

                    // Get start value
                    float startValue = heightMapUndo[brushy * dimension + brushx];

                    // Get new value
                    float newValue = Microsoft.Xna.Framework.MathHelper.Clamp(startValue + brushValue * scale, -1.0f, 1.0f);

                    // Set new value
                    heightMapData[y * mapDimension + x] = newValue;
                }
            }

            // Set manually painted flag
            manuallyPainted = true;

            // Raise update event
            OnHeightMapChanged(this, null);
        }

        /// <summary>
        /// Saves the height map data under the brush.
        /// </summary>
        private void SaveHeightMapUndo()
        {
            // Cannot do anything if cursor position invalid
            if (cursorPosition == null)
                return;

            // Create undo array
            int dimension = brushRadius * 2;
            float[] undo = new float[dimension * dimension];

            // Get start position of rectangle
            int xs = (int)(mapDimension * cursorPosition.Value.X - brushRadius);
            int ys = (int)(mapDimension * cursorPosition.Value.Y - brushRadius);

            // Capture all valid height pixels
            for (int y = ys; y < ys + dimension; y++)
            {
                // Cannot go out of bounds
                if (y < 0 || y >= mapDimension)
                    continue;

                for (int x = xs; x < xs + dimension; x++)
                {
                    // Cannot go out of bounds
                    if (x < 0 || x >= mapDimension)
                        continue;

                    // Get height pixel
                    float value = heightMapData[y * mapDimension + x];

                    // Save height pixel
                    int xd = (x - xs);
                    int yd = (y - ys);
                    if (xd < 0 || xd >= dimension) continue;
                    if (yd < 0 || yd >= dimension) continue;
                    undo[yd * dimension + xd] = value;
                }
            }

            // Assign undo array
            heightMapUndo = undo;
        }

        #endregion

    }

}
