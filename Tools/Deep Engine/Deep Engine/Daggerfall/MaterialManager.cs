﻿// Project:         Deep Engine
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
using System.Threading;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DeepEngine.Core;
using DeepEngine.Utility;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace DeepEngine.Daggerfall
{
    /// <summary>
    /// Loads texture data from Daggerfall and creates materials for use by engine.
    /// </summary>
    public class MaterialManager
    {

        #region Fields

        // Strings
        const string maxMaterialsError = "Maximum number of materials has been reached.";
        const string noFramesError = "No texture frames found while creating material.";

        // XNA
        SpriteBatch spriteBatch = null;

        // Class
        DeepCore core;
        string arena2Path;
        GraphicsDevice graphicsDevice;
        TextureFile textureFile;

        // Texture dictionaries
        const int nullTextureKey = int.MinValue;
        Dictionary<int, Texture2D> colorTextureDict;
        Dictionary<int, Texture2D> normalTextureDict;
        Dictionary<int, uint> textureMaterialDict;

        // Material array
        const int maxMaterials = 512;
        BaseMaterialEffect[] materialEffects;

        // Climate and weather
        DFLocation.ClimateBaseType climateType = DFLocation.ClimateBaseType.None;
        DFLocation.ClimateWeather climateWeather = DFLocation.ClimateWeather.Normal;
        bool daytime = true;

        // Constants
        const int defaultWorldClimate = 231;
        const int formatWidth = 4;
        const int tileSide = 64;
        const string formatError = "DFBitmap not RGBA.";
        const float bumpSize = 1f;
        Color dayWindowColor = new Color(89, 154, 178, 0x80);
        Color nightWindowColor = new Color(255, 182, 56, 0xff);

        #endregion

        #region Structures

        /// <summary>
        /// Flags to modify how texture is created.
        /// </summary>
        [Flags]
        public enum TextureCreateFlags
        {
            /// <summary>
            /// No flags set.
            /// </summary>
            None = 0,

            /// <summary>
            /// Attempts to process texture into current Climate and
            ///  Weather. TextureManager will atempt to ignore invalid
            ///  climate swaps.
            /// </summary>
            ApplyClimate = 0x01,

            /// <summary>
            /// Ensures texture will be POW2 by extending right and
            ///  bottom dimensions to nearest POW2 boundary. Texture
            ///  will remain at top-left of total area. It is the
            ///  responsibility of the caller to ensure model UVs are
            ///  modified to correctly address valid space in the
            ///  POW2 texture.
            ///  </summary>
            PowerOfTwo = 0x02,

            /// <summary>
            /// Blends colour into neighbouring alpha pixels to help
            ///  create softer borders. Most useful when loading
            ///  billboard textures. Also extends image dimensions
            ///  by a couple of pixels around each edge so images do
            ///  not appear to be cut off at the edges.
            /// </summary>
            Dilate = 0x04,

            /// <summary>
            /// Pre-multiplies alpha with colour channels. It is the
            ///  responsibility of the caller to ensure renderer blend
            ///  states are set to use pre-multiplied textures.
            /// </summary>
            PreMultiplyAlpha = 0x08,

            /// <summary>
            /// Creates a chain of mipmaps for this texture.
            /// </summary>
            MipMaps = 0x10,

            /// <summary>
            /// Makes image grayscale.
            /// </summary>
            Grayscale = 0x20,

            /// <summary>
            /// Creates a separate normal map for this texture.
            ///  This is achieved by using a sobel emboss filter to
            ///  create a bump map, which is then converted into a
            ///  normal map. The results are not very good.
            ///  Overall it enhances just how low-res Daggerfall's
            ///  textures are, and detracts from the painterly pixel
            ///  art style. This process is considered
            ///  to be experimental only.
            /// </summary>
            NormalMap = 0x40,

            /// <summary>
            /// Rotates texture 90 degrees counter-clockwise.
            ///  Can be used to build ground textures manually.
            /// </summary>
            Rotate = 0x80,

            /// <summary>
            /// Flips texture horizontally and vertically.
            ///  Can be used to build ground textures manually.
            /// </summary>
            Flip = 0x100,

            /// <summary>
            /// Uses special alpha values for transparent and
            ///  emissive texture properties.
            /// </summary>
            ExtendedAlpha = 0x200,
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets GraphicsDevice set at construction.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get { return graphicsDevice; }
        }

        /// <summary>
        /// Gets Arena2 path set at construction.
        /// </summary>
        public string Arena2Path
        {
            get { return arena2Path; }
        }

        /// <summary>
        /// Gets or sets current climate type for swaps.
        /// </summary>
        public DFLocation.ClimateBaseType ClimateType
        {
            get { return climateType; }
            set { SetClimate(value, climateWeather); }
        }

        /// <summary>
        /// Gets or sets current weather for swaps.
        /// </summary>
        public DFLocation.ClimateWeather Weather
        {
            get { return climateWeather; }
            set { SetClimate(climateType, value); }
        }

        /// <summary>
        /// Gets or sets daytime flag.
        ///  Primarily controls colour of building windows.
        ///  This is a temporary solution until support is added to shaders.
        /// </summary>
        public bool Daytime
        {
            get { return daytime; }
            set { daytime = value; }
        }

        /// <summary>
        /// Gets reserved key for null textures.
        /// </summary>
        public static int NullTextureKey
        {
            get { return nullTextureKey; }
        }

        /// <summary>
        /// Gets TextureFile object.
        /// </summary>
        public TextureFile TextureFile
        {
            get { return textureFile; }
        }

        /// <summary>
        /// Gets default flags for model materials.
        /// </summary>
        public static TextureCreateFlags DefaultModelFlags
        {
            get
            {
                return MaterialManager.TextureCreateFlags.MipMaps |
                    MaterialManager.TextureCreateFlags.PowerOfTwo |
                    MaterialManager.TextureCreateFlags.ExtendedAlpha |
                    MaterialManager.TextureCreateFlags.ApplyClimate;
            }
        }

        /// <summary>
        /// Gets default flags for billboard materials.
        /// </summary>
        public static TextureCreateFlags DefaultBillboardFlags
        {
            get
            {
                return MaterialManager.TextureCreateFlags.MipMaps |
                    MaterialManager.TextureCreateFlags.Dilate |
                    MaterialManager.TextureCreateFlags.PreMultiplyAlpha;
            }
        }

        /// <summary>
        /// Gets default flags for terrain tile materials.
        /// </summary>
        public static TextureCreateFlags DefaultTerrainFlags
        {
            get
            {
                return MaterialManager.TextureCreateFlags.MipMaps |
                    MaterialManager.TextureCreateFlags.PowerOfTwo;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="device">Engine core.</param>
        public MaterialManager(DeepCore core)
        {
            // Save references
            this.core = core;
            this.graphicsDevice = core.GraphicsDevice;
            this.arena2Path = core.Arena2Path;

            // Setup
            textureFile = new TextureFile();
            textureFile.Palette.Load(Path.Combine(arena2Path, textureFile.PaletteName));
            spriteBatch = new SpriteBatch(graphicsDevice);

            // Create dictionaries
            colorTextureDict = new Dictionary<int, Texture2D>();
            normalTextureDict = new Dictionary<int, Texture2D>();
            textureMaterialDict = new Dictionary<int, uint>();

            // Create material array
            materialEffects = new BaseMaterialEffect[maxMaterials];

            // Set default climate
            SetClimate(climateType, climateWeather);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a new material effect linked to a Daggerfall texture.
        /// </summary>
        /// <param name="archive">Texture archive.</param>
        /// <param name="record">Texture record.</param>
        /// <param name="effect">Effect</param>
        /// <returns>BaseMaterialEffect.</returns>
        public BaseMaterialEffect CreateDaggerfallMaterialEffect(int archive, int record, Effect effect, MaterialManager.TextureCreateFlags flags)
        {
            // Create group key
            int groupKey = GetTextureKey(archive, record, 0);

            // Only allow one material per Daggerfall texture archive & record index.
            // The material will group frames under this key when loading material for first time.
            if (textureMaterialDict.ContainsKey(groupKey))
                return GetMaterialEffect(textureMaterialDict[groupKey]);

            // Load texture
            List<int> frameKeys = LoadTexture(archive, record, flags);

            // Create empty material effect
            BaseMaterialEffect material = CreateMaterialEffect(effect, null, null, null, null, null);

            // Build material based on number of frames
            if (frameKeys.Count == 1)
            {
                // Single frame
                material.IsAnimated = false;
                material.DiffuseTexture = GetTexture(frameKeys[0]);
            }
            else if (frameKeys.Count > 1)
            {
                // Setup animation in material
                material.IsAnimated = true;
                material.DiffuseTextureFrames = new List<Texture2D>(frameKeys.Count);

                // Multiple frames
                foreach (var key in frameKeys)
                {
                    material.DiffuseTextureFrames.Add(GetTexture(key));
                }
            }
            else
            {
                // No frames found
                throw new Exception(noFramesError);
            }

            // Assign to dictionary
            textureMaterialDict.Add(groupKey, material.ID);

            return material;
        }

        /// <summary>
        /// Creates a new material effect.
        /// </summary>
        /// <param name="effect">Effect to use.</param>
        /// <param name="technique">Technique to use.</param>
        /// <param name="diffuseTextureParam">Diffuse texture parameter name. Can be null.</param>
        /// <param name="normalTextureParam">Normals texture parameter name. Can be null.</param>
        /// <param name="worldMatrixParam">World matrix parameter name.</param>
        /// <param name="viewMatrixParam">View matrix parameter name.</param>
        /// <param name="projectionMatrixParam">Projections matrix parameter name.</param>
        /// <param name="diffuseTexture">Diffuse texture to use. Can be null.</param>
        /// <param name="normalTexture">Normal texture to use. Can be null.</param>
        /// <returns>BaseMaterialEffect.</returns>
        private BaseMaterialEffect CreateMaterialEffect(
            Effect effect,
            EffectTechnique technique,
            string diffuseTextureParam,
            string normalTextureParam,
            Texture2D diffuseTexture,
            Texture2D normalTexture)
        {
            // Create material
            BaseMaterialEffect material = new BaseMaterialEffect(
                core,
                effect,
                technique,
                diffuseTextureParam,
                normalTextureParam);

            // Save texture references
            material.DiffuseTexture = diffuseTexture;
            material.NormalTexture = normalTexture;

            // Check for array overflow
            if (material.ID > maxMaterials)
            {
                throw new Exception(maxMaterialsError);
            }

            // Add to array
            materialEffects[material.ID] = material;

            return material;
        }

        /// <summary>
        /// Gets material effect by ID.
        /// </summary>
        /// <param name="id">ID of material to get.</param>
        /// <returns>BaseMaterialEffect.</returns>
        public BaseMaterialEffect GetMaterialEffect(uint id)
        {
            return materialEffects[id];
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets color map texture by key.
        ///  Manager will return NULL if texture does not exist.
        /// </summary>
        /// <param name="key">Texture key.</param>
        /// <returns>Texture2D.</returns>
        private Texture2D GetTexture(int key)
        {
            // Otherwise return general texture
            if (!colorTextureDict.ContainsKey(key))
                return null;
            else
                return colorTextureDict[key];
        }

        /// <summary>
        /// Gets normal map texture by key.
        ///  Manager will return NULL if texture does not exist.
        /// </summary>
        /// <param name="key">Texture key.</param>
        /// <returns>Texture2D.</returns>
        private Texture2D GetNormalTexture(int key)
        {
            if (!normalTextureDict.ContainsKey(key))
                return null;
            else
                return normalTextureDict[key];
        }

        /// <summary>
        /// Remove cached texture based on key.
        ///  Cannot remove terrain atlas by key,
        ///  use ClearAtlases() method instead.
        /// </summary>
        /// <param name="key">Texture key.</param>
        private void RemoveTexture(int key)
        {
            // Remove color texture by key
            if (colorTextureDict.ContainsKey(key))
                colorTextureDict.Remove(key);

            // Remove normal texture by key
            if (normalTextureDict.ContainsKey(key))
                normalTextureDict.Remove(key);

            // Remove material link by key
            if (textureMaterialDict.ContainsKey(key))
                textureMaterialDict.Remove(key);
        }

        /// <summary>
        /// Clear all cached textures.
        /// </summary>
        private void ClearTextures()
        {
            // Remove general and winter dictionaries
            colorTextureDict.Clear();
            normalTextureDict.Clear();
            textureMaterialDict.Clear();
            BaseMaterialEffect.ResetID();
        }

        /// <summary>
        /// Determine if texture with specified key exists in color map texture dictionary.
        /// </summary>
        /// <param name="key">Texture key.</param>
        /// <returns>True if texture exists.</returns>
        private bool HasColorTexture(int key)
        {
            return colorTextureDict.ContainsKey(key);
        }

        /// <summary>
        /// Determine if texture with specified key exists in normal map texture dictionary.
        /// </summary>
        /// <param name="key">Texture key.</param>
        /// <returns>True if texture exists.</returns>
        private bool HasNormalTexture(int key)
        {
            return normalTextureDict.ContainsKey(key);
        }

        /// <summary>
        /// Loads a texture from archive, record, and frame indices.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <param name="flags">TextureCreateFlags.</param>
        /// <returns>Texture key.</returns>
        private List<int> LoadTexture(int archive, int record, TextureCreateFlags flags)
        {
            // Load based on flags
            if (flags.HasFlag(TextureCreateFlags.ApplyClimate) &&
                climateType != DFLocation.ClimateBaseType.None)
            {
                return LoadTextureWithClimate(archive, record, flags);
            }
            else
            {
                return LoadTextureNoClimate(archive, record, flags);
            }
        }

        /// <summary>
        /// Gets unique key for a non-climate-aware texture.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <param name="frame">Frame index.</param>
        /// <returns>Texture key.</returns>
        private int GetTextureKey(int archive, int record, int frame)
        {
            return (archive * -10000) - (record * -10) - frame;
        }

        /// <summary>
        /// Gets unique key for a climate-aware texture.
        ///  These textures never have animation frames.
        /// </summary>
        /// <param name="climateType">Climate type.</param>
        /// <param name="climateSet">Climate set.</param>
        /// <param name="record">Record index.</param>
        /// <returns>Texture key.</returns>
        private int GetTextureKey(DFLocation.ClimateBaseType climateType, DFLocation.ClimateTextureSet climateSet, int record)
        {
            return ((int)climateType * 1000000) + ((int)climateSet * 1000) + (record * 10);
        }

        /// <summary>
        /// Load a texture without climate processing.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <param name="frame">Frame index.</param>
        /// <param name="flags">TextureCreateFlags.</param>
        /// <returns>List of texture keys.</returns>
        private List<int> LoadTextureNoClimate(int archive, int record, TextureCreateFlags flags)
        {
            List<int> frameKeys;
            CreateTexture(archive, record, flags, out frameKeys);

            return frameKeys;
        }

        /// <summary>
        /// Load a texture with climate processing.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <param name="flags">TextureCreateFlags.</param>
        /// <returns>Texture key.</returns>
        private List<int> LoadTextureWithClimate(int archive, int record, TextureCreateFlags flags)
        {
            // Get the base set this archive belongs to regardless of climate
            bool supportsWinter, supportsRain;
            DFLocation.ClimateTextureSet climateSet = GetClimateSet(archive, out supportsWinter, out supportsRain);

            // Load non-climate-aware textures without climate processing
            if (DFLocation.ClimateTextureSet.None == climateSet)
            {
                return LoadTextureNoClimate(archive, record, flags);
            }

            // Handle missing Swamp textures
            if (climateType == DFLocation.ClimateBaseType.Swamp)
            {
                switch (climateSet)
                {
                    case DFLocation.ClimateTextureSet.Interior_TempleInt:
                    case DFLocation.ClimateTextureSet.Interior_MarbleFloors:
                        return LoadTextureNoClimate(archive, record, flags);
                }
            }

            // Handle specific climate sets with missing winter textures
            if (climateType == DFLocation.ClimateBaseType.Desert ||
                climateType == DFLocation.ClimateBaseType.Swamp)
            {
                switch (climateSet)
                {
                    case DFLocation.ClimateTextureSet.Exterior_Castle:
                    case DFLocation.ClimateTextureSet.Exterior_MagesGuild:
                        supportsWinter = false;
                        break;
                }
            }

            // Load climate-aware texture
            int newArchive = (int)climateType + (int)climateSet;

            // Increment winter textures
            List<int> frameKeys;
            if (supportsWinter && climateWeather == DFLocation.ClimateWeather.Winter)
                CreateTexture(newArchive + 1, record, flags, out frameKeys);
            else
                CreateTexture(newArchive, record, flags, out frameKeys);

            // Climate textures only ever have a single frame
            return frameKeys;
        }

        /// <summary>
        /// Loads all texture frames for the specified archive and record.
        /// </summary>
        /// <param name="archive">Archive index to load.</param>
        /// <param name="record">Record index to load.</param>
        /// <param name="flags">TextureCreateFlags.</param>
        /// <param name="frameKeys">List of frame keys loaded.</param>
        private void CreateTexture(int archive, int record, TextureCreateFlags flags, out List<int> frameKeys)
        {
            // Load texture file
            textureFile.Load(Path.Combine(arena2Path, TextureFile.IndexToFileName(archive)), FileUsage.UseDisk, true);

            // Get frame count
            int frameCount = textureFile.GetFrameCount(record);

            // Create list of keys to reference each frame
            frameKeys = new List<int>(frameCount);

            // Process each frame
            for (int frame = 0; frame < frameCount; frame++)
            {
                // Get key
                int key = GetTextureKey(archive, record, frame);

                // Just add key if it already exists so we're not converting textures twice
                if (colorTextureDict.ContainsKey(key))
                {
                    frameKeys.Add(key);
                    continue;
                }

                // Get RGBA format
                DFBitmap colorBitmap;
                if (flags.HasFlag(TextureCreateFlags.ExtendedAlpha))
                    colorBitmap = GetEngineRGBA(archive, record, frame, flags);
                else
                    colorBitmap = textureFile.GetBitmapFormat(record, frame, 0, DFBitmap.Formats.RGBA);

                // Perform optional image processing
                ProcessDFBitmap(ref colorBitmap, flags);

                // Create and store XNA texture
                colorTextureDict.Add(key, CreateTexture2D(ref colorBitmap, flags));

                // Create XNA normal texture
                //if (flags.HasFlag(TextureCreateFlags.NormalMap))
                //    normalTextureDict.Add(key, CreateNormalTexture2D(ref colorBitmap, flags));

                // Add frame to list
                frameKeys.Add(key);
            }
        }

        /// <summary>
        /// Creates Texture2D from DFBitmap.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap source. Must be in ARGB format.</param>
        /// <param name="flags">TextureCreateFlags.</param>
        /// <returns>Texture2D.</returns>
        private Texture2D CreateTexture2D(ref DFBitmap dfBitmap, TextureCreateFlags flags)
        {
            // Get dimensions of new texture
            int width, height;
            if (TextureCreateFlags.PowerOfTwo == (flags & TextureCreateFlags.PowerOfTwo))
            {
                width = (PowerOfTwo.IsPowerOfTwo(dfBitmap.Width)) ? dfBitmap.Width : PowerOfTwo.NextPowerOfTwo(dfBitmap.Width);
                height = (PowerOfTwo.IsPowerOfTwo(dfBitmap.Height)) ? dfBitmap.Height : PowerOfTwo.NextPowerOfTwo(dfBitmap.Height);
            }
            else
            {
                width = dfBitmap.Width;
                height = dfBitmap.Height;
            }

            // Create new texture
            Texture2D texture = new Texture2D(
                    graphicsDevice,
                    width,
                    height,
                    flags.HasFlag(TextureCreateFlags.MipMaps),
                    SurfaceFormat.Color);

            // Set texture data
            for (int level = 0; level < texture.LevelCount; level++)
            {
                // Level 0 is always full size
                if (level == 0)
                {
                    texture.SetData<byte>(
                            0,
                            new Microsoft.Xna.Framework.Rectangle(0, 0, dfBitmap.Width, dfBitmap.Height),
                            dfBitmap.Data,
                            0,
                            dfBitmap.Width * dfBitmap.Height * formatWidth);
                    continue;
                }

                // Create mipmap
                DFBitmap newBitmap = CreateMipMap(ref dfBitmap, level);
                texture.SetData<byte>(
                            level,
                            new Microsoft.Xna.Framework.Rectangle(0, 0, newBitmap.Width, newBitmap.Height),
                            newBitmap.Data,
                            0,
                            newBitmap.Width * newBitmap.Height * formatWidth);
            }

            return texture;
        }

        /// <summary>
        /// Creates a normal map texture from DFBitmap.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap.</param>
        /// <param name="flags">TextureCreateFlags.</param>
        private Texture2D CreateNormalTexture2D(ref DFBitmap dfBitmap, TextureCreateFlags flags)
        {
            // Get bump map
            DFBitmap normalBitmap = GetBumpMap(ref dfBitmap);

            // Convert bump map to normal map
            ConvertToNormalMap(ref normalBitmap);

            return CreateTexture2D(ref normalBitmap, flags);
        }

        /// <summary>
        /// Gets RGBA image with alpha packed for use with hidef renderer.
        ///  Alpha 0x00 - 0x7f is specular intensity.
        ///  Alpha 0x80 - 0xff is emissive intensity.
        ///  In Detail:
        ///  * 0 is matte and 127 is glossy.
        ///  * 128 is not emissive and 255 is fullbright.
        ///  * A material cannot be specular and emissive at the same time.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <param name="frame">Frame index.</param>
        /// <param name="flags">Texture flags.</param>
        /// <returns>DFBitmap.</returns>
        private DFBitmap GetEngineRGBA(int archive, int record, int frame, TextureCreateFlags flags)
        {
            // Daggerfall uses this index to represent a transparent colour
            const int dfChromaIndex = 0x00;

            // Daggerfall uses this index to represent the glass in a window
            const int dfWindowIndex = 0xff;

            // Create new bitmap
            DFBitmap srcBitmap = textureFile.GetDFBitmap(record, frame);
            DFBitmap dstBitmap = new DFBitmap();
            dstBitmap.Format = DFBitmap.Formats.RGBA;
            dstBitmap.Width = srcBitmap.Width;
            dstBitmap.Height = srcBitmap.Height;
            dstBitmap.Stride = dstBitmap.Width * formatWidth;
            dstBitmap.Data = new byte[dstBitmap.Stride * dstBitmap.Height];

            // Get source palette
            DFPalette palette = textureFile.Palette;

            // Write pixel data to array
            byte a, r, g, b;
            int srcPos = 0, dstPos = 0;
            for (int i = 0; i < dstBitmap.Width * dstBitmap.Height; i++)
            {
                // Get index of this pixel
                byte index = srcBitmap.Data[srcPos++];

                // Get initial colour and alpha values
                r = palette.GetRed(index);
                g = palette.GetGreen(index);
                b = palette.GetBlue(index);
                a = 0;

                // Write colour and alpha values
                if (index == dfChromaIndex)
                {
                    a = 0;
                }
                else if (index == dfWindowIndex)
                {
                    if (daytime)
                    {
                        r = dayWindowColor.R;
                        g = dayWindowColor.G;
                        b = dayWindowColor.B;
                        a = dayWindowColor.A;
                    }
                    else
                    {
                        r = nightWindowColor.R;
                        g = nightWindowColor.G;
                        b = nightWindowColor.B;
                        a = nightWindowColor.A;
                    }
                }

                // TODO: Handle light textures

                // Write colour and alpha values
                dstBitmap.Data[dstPos++] = r;
                dstBitmap.Data[dstPos++] = g;
                dstBitmap.Data[dstPos++] = b;
                dstBitmap.Data[dstPos++] = a;
            }

            return dstBitmap;
        }

        #endregion

        #region Image Processing

        #region Processing

        /// <summary>
        /// Performs optional processing to DFBitmap.
        ///  Only pre-process options are handled here.
        ///  Other options are handled at texture creation time.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap.</param>
        /// <param name="flags">TextureCreateFlags.</param>
        private void ProcessDFBitmap(ref DFBitmap dfBitmap, TextureCreateFlags flags)
        {
            // Format must be RGBA
            if (dfBitmap.Format != DFBitmap.Formats.RGBA)
                throw new Exception(formatError);

            // Grayscale
            if (flags.HasFlag(TextureCreateFlags.Grayscale))
                MakeGrayscale(ref dfBitmap);

            // Rotate
            if (flags.HasFlag(TextureCreateFlags.Rotate))
                RotateDFBitmap(ref dfBitmap);

            // Flip
            if (flags.HasFlag(TextureCreateFlags.Flip))
                FlipDFBitmap(ref dfBitmap);

            // Dilate
            if (flags.HasFlag(TextureCreateFlags.Dilate))
                DilateDFBitmap(ref dfBitmap);

            // Pre-multiply alpha
            if (flags.HasFlag(TextureCreateFlags.PreMultiplyAlpha))
                PreMultiplyAlphaDFBitmap(ref dfBitmap);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Sets RGBA pixel in DFBitmap.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap.</param>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="color">Color.</param>
        static public void SetPixel(ref DFBitmap dfBitmap, int x, int y, Color color)
        {
            int pos = y * dfBitmap.Stride + x * formatWidth;
            dfBitmap.Data[pos++] = color.R;
            dfBitmap.Data[pos++] = color.G;
            dfBitmap.Data[pos++] = color.B;
            dfBitmap.Data[pos] = color.A;
        }

        /// <summary>
        /// Sets RGBA pixel in DFBitmap.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap.</param>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="r">Red value.</param>
        /// <param name="g">Green value.</param>
        /// <param name="b">Blue value.</param>
        /// <param name="a">Alpha value.</param>
        static public void SetPixel(ref DFBitmap dfBitmap, int x, int y, byte r, byte g, byte b, byte a)
        {
            int pos = y * dfBitmap.Stride + x * formatWidth;
            dfBitmap.Data[pos++] = r;
            dfBitmap.Data[pos++] = g;
            dfBitmap.Data[pos++] = b;
            dfBitmap.Data[pos] = a;
        }

        /// <summary>
        /// Gets pixel Color in DFBitmap.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap.</param>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <returns>Color.</returns>
        static public Color GetPixel(ref DFBitmap dfBitmap, int x, int y)
        {
            int pos = y * dfBitmap.Stride + x * formatWidth;
            return Color.FromNonPremultiplied(
                dfBitmap.Data[pos++],
                dfBitmap.Data[pos++],
                dfBitmap.Data[pos++],
                dfBitmap.Data[pos]);
        }

        /// <summary>
        /// Creates a clone of a DFBitmap.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap.</param>
        /// <param name="copyData">True to copy data contents.</param>
        /// <returns>DFBitmap.</returns>
        static public DFBitmap CloneDFBitmap(ref DFBitmap dfBitmap, bool copyData)
        {
            // Create destination bitmap to receive normal image
            DFBitmap newBitmap = new DFBitmap();
            newBitmap.Format = dfBitmap.Format;
            newBitmap.Width = dfBitmap.Width;
            newBitmap.Height = dfBitmap.Height;
            newBitmap.Stride = dfBitmap.Stride;
            newBitmap.Data = new byte[dfBitmap.Data.Length];

            if (copyData)
                Buffer.BlockCopy(dfBitmap.Data, 0, newBitmap.Data, 0, dfBitmap.Data.Length);

            return newBitmap;
        }

        /// <summary>
        /// Clones a RenderTarget2D to Texture2D.
        /// </summary>
        /// <param name="renderTarget">RenderTarget2D.</param>
        /// <returns>Texture2D</returns>
        static public Texture2D CloneRenderTarget2DToTexture2D(GraphicsDevice graphicsDevice, RenderTarget2D renderTarget)
        {
            // Create target texture
            Texture2D clone = new Texture2D(
                graphicsDevice,
                renderTarget.Width,
                renderTarget.Height,
                (renderTarget.LevelCount > 1) ? true : false,
                SurfaceFormat.Color);

            // Clone each level
            for (int level = 0; level < renderTarget.LevelCount; level++)
            {
                // Get source data
                int width = PowerOfTwo.MipMapSize(renderTarget.Width, level);
                int height = PowerOfTwo.MipMapSize(renderTarget.Height, level);
                Color[] srcData = new Color[width * height];
                renderTarget.GetData<Color>(level, null, srcData, 0, width * height);

                // Set destination data
                clone.SetData<Color>(level, null, srcData, 0, width * height);
            }

            return clone;
        }

        #endregion

        #region Grayscale

        /// <summary>
        /// Makes image grayscale.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap.</param>
        private void MakeGrayscale(ref DFBitmap dfBitmap)
        {
            // Make each pixel grayscale
            int srcPos = 0, dstPos = 0;
            for (int i = 0; i < dfBitmap.Width * dfBitmap.Height; i++)
            {
                // Get source color
                byte r = dfBitmap.Data[srcPos++];
                byte g = dfBitmap.Data[srcPos++];
                byte b = dfBitmap.Data[srcPos++];
                byte a = dfBitmap.Data[srcPos++];
                System.Drawing.Color srcColor = System.Drawing.Color.FromArgb(
                    a, r, g, b);

                // Create grayscale color
                int grayscale = (int)(r * 0.3f + g * 0.59f + b * 0.11f);
                System.Drawing.Color dstColor = System.Drawing.Color.FromArgb(
                    grayscale, grayscale, grayscale);

                // Write destination pixel
                dfBitmap.Data[dstPos++] = dstColor.R;
                dfBitmap.Data[dstPos++] = dstColor.G;
                dfBitmap.Data[dstPos++] = dstColor.B;
                dfBitmap.Data[dstPos++] = a;
            }
        }

        #endregion

        #region PreMultiply Alpha

        /// <summary>
        /// Pre-multiply bitmap alpha.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap.</param>
        private void PreMultiplyAlphaDFBitmap(ref DFBitmap dfBitmap)
        {
            // Pre-multiply alpha for each pixel
            int pos;
            float multiplier;
            for (int y = 0; y < dfBitmap.Height; y++)
            {
                pos = y * dfBitmap.Stride;
                for (int x = 0; x < dfBitmap.Width; x++)
                {
                    multiplier = dfBitmap.Data[pos + 3] / 256f;
                    dfBitmap.Data[pos] = (byte)(dfBitmap.Data[pos] * multiplier);
                    dfBitmap.Data[pos + 1] = (byte)(dfBitmap.Data[pos + 1] * multiplier);
                    dfBitmap.Data[pos + 2] = (byte)(dfBitmap.Data[pos + 2] * multiplier);
                    pos += formatWidth;
                }
            }
        }

        #endregion

        #region Rotate and Flip

        /// <summary>
        /// Rotates a DFBitmap 90 degrees counter-clockwise.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap.</param>
        private void RotateDFBitmap(ref DFBitmap dfBitmap)
        {
            // Create destination bitmap to receive rotated image
            DFBitmap dstBitmap = new DFBitmap();
            dstBitmap.Format = dfBitmap.Format;
            dstBitmap.Width = dfBitmap.Width;
            dstBitmap.Height = dfBitmap.Height;
            dstBitmap.Stride = dfBitmap.Stride;
            dstBitmap.Data = new byte[dfBitmap.Data.Length];

            // Rotate image
            int srcPos = 0;
            int dstPos = 0;
            for (int x = 0; x < dstBitmap.Width; x++)
            {
                for (int y = dstBitmap.Height - 1; y >= 0; y--)
                {
                    // Get source pixel
                    byte r = dfBitmap.Data[srcPos++];
                    byte g = dfBitmap.Data[srcPos++];
                    byte b = dfBitmap.Data[srcPos++];
                    byte a = dfBitmap.Data[srcPos++];

                    // Write destination pixel
                    dstPos = y * dstBitmap.Stride + x * formatWidth;
                    dstBitmap.Data[dstPos++] = r;
                    dstBitmap.Data[dstPos++] = g;
                    dstBitmap.Data[dstPos++] = b;
                    dstBitmap.Data[dstPos++] = a;
                }
            }

            // Assign processed image
            dfBitmap = dstBitmap;
        }

        /// <summary>
        /// Flips a DFBitmap horizontally and vertically.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap.</param>
        private void FlipDFBitmap(ref DFBitmap dfBitmap)
        {
            // Create destination bitmap to receive flipped image
            DFBitmap dstBitmap = new DFBitmap();
            dstBitmap.Format = dfBitmap.Format;
            dstBitmap.Width = dfBitmap.Width;
            dstBitmap.Height = dfBitmap.Height;
            dstBitmap.Stride = dfBitmap.Stride;
            dstBitmap.Data = new byte[dfBitmap.Data.Length];

            // Flip image
            int srcPos = 0;
            int dstPos = dstBitmap.Data.Length - 1;
            for (int i = 0; i < dfBitmap.Width * dfBitmap.Height; i++)
            {
                // Get source pixel
                byte r = dfBitmap.Data[srcPos++];
                byte g = dfBitmap.Data[srcPos++];
                byte b = dfBitmap.Data[srcPos++];
                byte a = dfBitmap.Data[srcPos++];

                // Write destination pixel
                dstBitmap.Data[dstPos--] = a;
                dstBitmap.Data[dstPos--] = b;
                dstBitmap.Data[dstPos--] = g;
                dstBitmap.Data[dstPos--] = r;
            }

            // Assign processed image
            dfBitmap = dstBitmap;
        }

        #endregion

        #region Dilate

        /// <summary>
        /// Dilate bitmap.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap.</param>
        private void DilateDFBitmap(ref DFBitmap dfBitmap)
        {
            // Format must be RGBA
            if (dfBitmap.Format != DFBitmap.Formats.RGBA)
                throw new Exception(formatError);

            // Constants
            const int paddingPixels = 1;

            // Create larger bitmap to receive dilated image
            DFBitmap dstBitmap = new DFBitmap();
            dstBitmap.Format = dfBitmap.Format;
            dstBitmap.Width = dfBitmap.Width + (paddingPixels * 2);
            dstBitmap.Height = dfBitmap.Height + (paddingPixels * 2);
            dstBitmap.Stride = dstBitmap.Width * formatWidth;
            dstBitmap.Data = new byte[dstBitmap.Stride * dstBitmap.Height];

            // Process image
            int srcPos, dstPos;
            for (int y = 0; y < dfBitmap.Height; y++)
            {
                srcPos = y * dfBitmap.Stride;
                dstPos = (y * dstBitmap.Stride) + (paddingPixels * dstBitmap.Stride) + (paddingPixels * formatWidth);
                for (int x = 0; x < dfBitmap.Width; x++)
                {
                    // Copy pixel to top, bottom, left, right
                    TestCopyPixel(ref dfBitmap.Data, ref dstBitmap.Data, srcPos, dstPos - dstBitmap.Stride);
                    TestCopyPixel(ref dfBitmap.Data, ref dstBitmap.Data, srcPos, dstPos + dstBitmap.Stride);
                    TestCopyPixel(ref dfBitmap.Data, ref dstBitmap.Data, srcPos, dstPos - formatWidth);
                    TestCopyPixel(ref dfBitmap.Data, ref dstBitmap.Data, srcPos, dstPos + formatWidth);

                    // Copy pixel to four corners
                    TestCopyPixel(ref dfBitmap.Data, ref dstBitmap.Data, srcPos, dstPos - dstBitmap.Stride - formatWidth);
                    TestCopyPixel(ref dfBitmap.Data, ref dstBitmap.Data, srcPos, dstPos - dstBitmap.Stride + formatWidth);
                    TestCopyPixel(ref dfBitmap.Data, ref dstBitmap.Data, srcPos, dstPos + dstBitmap.Stride - formatWidth);
                    TestCopyPixel(ref dfBitmap.Data, ref dstBitmap.Data, srcPos, dstPos + dstBitmap.Stride + formatWidth);

                    // Copy central colour pixel
                    dstBitmap.Data[dstPos] = dfBitmap.Data[srcPos];
                    dstBitmap.Data[dstPos + 1] = dfBitmap.Data[srcPos + 1];
                    dstBitmap.Data[dstPos + 2] = dfBitmap.Data[srcPos + 2];
                    dstBitmap.Data[dstPos + 3] = dfBitmap.Data[srcPos + 3];

                    srcPos += formatWidth;
                    dstPos += formatWidth;
                }
            }

            // Assign processed image
            dfBitmap = dstBitmap;
        }

        /// <summary>
        /// Tests a pixel for transparancy and clones colour information.
        /// </summary>
        /// <param name="srcBuffer">Source buffer.</param>
        /// <param name="dstBuffer">Destination buffer.</param>
        /// <param name="srcPos">Source position.</param>
        /// <param name="dstPos">Destination position.</param>
        private void TestCopyPixel(ref byte[] srcBuffer, ref byte[] dstBuffer, int srcPos, int dstPos)
        {
            // Do nothing if source pixel is transparant
            if (srcBuffer[srcPos + 3] == 0)
                return;

            // Copy source colour if destination pixel is transparant
            if (dstBuffer[dstPos + 3] == 0)
            {
                dstBuffer[dstPos] = srcBuffer[srcPos];
                dstBuffer[dstPos + 1] = srcBuffer[srcPos + 1];
                dstBuffer[dstPos + 2] = srcBuffer[srcPos + 2];
                dstBuffer[dstPos + 3] = 0x01;
            }
        }

        #endregion

        #region MipMap

        /// <summary>
        /// Creates a new mipmap from the source bitmap.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap source.</param>
        /// <param name="level">MipMap level.</param>
        /// <returns>DFBitmap mipmap.</returns>
        private DFBitmap CreateMipMap(ref DFBitmap dfBitmap, int level)
        {
            // Get start size
            int width = dfBitmap.Width;
            int height = dfBitmap.Height;

            // Get new size for level
            int newWidth = PowerOfTwo.MipMapSize(dfBitmap.Width, level);
            int newHeight = PowerOfTwo.MipMapSize(dfBitmap.Height, level);

            // Create new bitmap
            DFBitmap newBitmap = new DFBitmap();
            newBitmap.Format = DFBitmap.Formats.RGBA;
            newBitmap.Width = newWidth;
            newBitmap.Height = newHeight;
            newBitmap.Stride = newWidth * formatWidth;
            newBitmap.Data = new byte[newBitmap.Stride * newBitmap.Height];

            // Scale factors
            double xFactor = (double)width / newWidth;
            double yFactor = (double)height / newHeight;

            // Coordinates of source points and coefficients
            double ox, oy, dx, dy, k1, k2;
            int ox1, oy1, ox2, oy2;

            // Destination pixel values
            double r, g, b, a;

            // Width and height decreased by 1
            int ymax = height - 1;
            int xmax = width - 1;

            // Bicubic resize
            for (int y = 0; y < newHeight; y++)
            {
                // Y coordinates
                oy = (double)y * yFactor - 0.5f;
                oy1 = (int)oy;
                dy = oy - (double)oy1;

                for (int x = 0; x < newWidth; x++)
                {
                    // X coordinates
                    ox = (double)x * xFactor - 0.5f;
                    ox1 = (int)ox;
                    dx = ox - (double)ox1;

                    // Initial pixel value
                    r = g = b = a = 0;

                    for (int n = -1; n < 3; n++)
                    {
                        // Get Y coefficient
                        k1 = BiCubicKernel(dy - (double)n);

                        oy2 = oy1 + n;
                        if (oy2 < 0)
                            oy2 = 0;
                        if (oy2 > ymax)
                            oy2 = ymax;

                        for (int m = -1; m < 3; m++)
                        {
                            // Get X coefficient
                            k2 = k1 * BiCubicKernel((double)m - dx);

                            ox2 = ox1 + m;
                            if (ox2 < 0)
                                ox2 = 0;
                            if (ox2 > xmax)
                                ox2 = xmax;

                            // Get pixel of original image
                            int srcPos = (ox2 * formatWidth) + (dfBitmap.Stride * oy2);
                            r += k2 * dfBitmap.Data[srcPos++];
                            g += k2 * dfBitmap.Data[srcPos++];
                            b += k2 * dfBitmap.Data[srcPos++];
                            a += k2 * dfBitmap.Data[srcPos];
                        }
                    }

                    // Set destination pixel
                    int dstPos = (x * formatWidth) + (newBitmap.Stride * y);
                    newBitmap.Data[dstPos++] = (byte)r;
                    newBitmap.Data[dstPos++] = (byte)g;
                    newBitmap.Data[dstPos++] = (byte)b;
                    newBitmap.Data[dstPos] = (byte)a;
                }
            }

            return newBitmap;
        }

        /// <summary>
        /// BiCubic calculations.
        /// </summary>
        /// <param name="x">Value.</param>
        /// <returns>Double.</returns>
        private double BiCubicKernel(double x)
        {
            if (x > 2.0)
                return 0.0;

            double a, b, c, d;
            double xm1 = x - 1.0;
            double xp1 = x + 1.0;
            double xp2 = x + 2.0;

            a = (xp2 <= 0.0) ? 0.0 : xp2 * xp2 * xp2;
            b = (xp1 <= 0.0) ? 0.0 : xp1 * xp1 * xp1;
            c = (x <= 0.0) ? 0.0 : x * x * x;
            d = (xm1 <= 0.0) ? 0.0 : xm1 * xm1 * xm1;

            return (0.16666666666666666667 * (a - (4.0 * b) + (6.0 * c) - (4.0 * d)));
        }

        #endregion

        #region Normal Map

        /// <summary>
        /// Converts a grayscale bump bitmap into normal map format.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap.</param>
        private void ConvertToNormalMap(ref DFBitmap dfBitmap)
        {
            // Calculate normal map vectors
            ConvertGrayToAlpha(ref dfBitmap);
            ConvertAlphaToNormals(ref dfBitmap);
        }

        /// <summary>
        /// Copies grayscale color information into the alpha channel.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap.</param>
        private void ConvertGrayToAlpha(ref DFBitmap dfBitmap)
        {
            for (int y = 0; y < dfBitmap.Height; y++)
            {
                for (int x = 0; x < dfBitmap.Width; x++)
                {
                    // Get position
                    int pos = y * dfBitmap.Stride + x * formatWidth;

                    // Get average of grayscale RGB values
                    float average =
                        (dfBitmap.Data[pos++] + dfBitmap.Data[pos++] + dfBitmap.Data[pos++]) / 3;

                    // Copy to alpha
                    dfBitmap.Data[pos] = (byte)average;
                }
            }
        }

        /// <summary>
        /// Using height data stored in the alpha channel, computes normalmap
        ///  vectors and stores them in the RGB portion of the bitmap.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap.</param>
        private void ConvertAlphaToNormals(ref DFBitmap dfBitmap)
        {
            for (int y = 0; y < dfBitmap.Height; y++)
            {
                for (int x = 0; x < dfBitmap.Width; x++)
                {
                    // Look up the heights to either side of this pixel
                    float left = GetHeightFromAlpha(ref dfBitmap, x - 1, y);
                    float right = GetHeightFromAlpha(ref dfBitmap, x + 1, y);
                    float top = GetHeightFromAlpha(ref dfBitmap, x, y - 1);
                    float bottom = GetHeightFromAlpha(ref dfBitmap, x, y + 1);

                    // Compute gradient vectors, then cross them to get the normal
                    Vector3 dx = new Vector3(1, 0, (right - left) * bumpSize);
                    Vector3 dy = new Vector3(0, 1, (bottom - top) * bumpSize);
                    Vector3 normal = Vector3.Cross(dx, dy);
                    normal.Normalize();

                    // Create final colour
                    float alpha = GetHeightFromAlpha(ref dfBitmap, x, y);
                    Vector4 vector = new Vector4(normal, alpha);

                    // Store result
                    int pos = y * dfBitmap.Stride + x * formatWidth;
                    dfBitmap.Data[pos++] = (byte)(255f * vector.X);
                    dfBitmap.Data[pos++] = (byte)(255f * vector.Y);
                    dfBitmap.Data[pos++] = (byte)(255f * vector.Z);
                    dfBitmap.Data[pos] = (byte)vector.W;
                }
            }
        }

        /// <summary>
        /// Helper for looking up height values from the bitmap alpha channel,
        ///  clamping if the specified position is off the edge of the bitmap.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap.</param>
        /// <param name="x">X coord.</param>
        /// <param name="y">Y coord.</param>
        private float GetHeightFromAlpha(ref DFBitmap dfBitmap, int x, int y)
        {
            // Clamp X
            if (x < 0)
                x = 0;
            else if (x >= dfBitmap.Width)
                x = dfBitmap.Width - 1;

            // Clamp Y
            if (y < 0)
                y = 0;
            else if (y >= dfBitmap.Height)
                y = dfBitmap.Height - 1;

            // Get position
            int pos = y * dfBitmap.Stride + x * formatWidth;

            return (float)dfBitmap.Data[pos + 3];
        }

        #endregion

        #region Bump Map

        /// <summary>
        /// Gets a bump map from the source DFBitmap.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap source image.</param>
        /// <returns>Bump map image.</returns>
        private DFBitmap GetBumpMap(ref DFBitmap dfBitmap)
        {
            // Create sobel emboss filter
            Filter edgeDetectionFilter = new Filter(3, 3);
            edgeDetectionFilter.MyFilter[0, 0] = 1;
            edgeDetectionFilter.MyFilter[1, 0] = 2;
            edgeDetectionFilter.MyFilter[2, 0] = 1;
            edgeDetectionFilter.MyFilter[0, 1] = 0;
            edgeDetectionFilter.MyFilter[1, 1] = 0;
            edgeDetectionFilter.MyFilter[2, 1] = 0;
            edgeDetectionFilter.MyFilter[0, 2] = -1;
            edgeDetectionFilter.MyFilter[1, 2] = -2;
            edgeDetectionFilter.MyFilter[2, 2] = -1;

            // Get filtered image
            DFBitmap newBitmap = edgeDetectionFilter.ApplyFilter(ref dfBitmap);

            // Convert to grayscale
            MakeGrayscale(ref newBitmap);

            return newBitmap;
        }

        #endregion

        #region Edge Detection

        /*
         * Based on Craig's Utility Library (CUL) by James Craig.
         * http://www.gutgames.com/post/Edge-detection-in-C.aspx
         * MIT License (http://www.opensource.org/licenses/mit-license.php)
         */

        /// <summary>
        /// Gets a new bitmap containing edges detected in source bitmap.
        ///  The source bitmap is unchanged.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap source.</param>
        /// <param name="threshold">Edge detection threshold.</param>
        /// <param name="edgeColor">Edge colour to write.</param>
        /// <returns>DFBitmap containing edges.</returns>
        private DFBitmap FindEdges(ref DFBitmap dfBitmap, float threshold, Color edgeColor)
        {
            // Clone bitmap settings
            DFBitmap newBitmap = CloneDFBitmap(ref dfBitmap, false);

            for (int x = 0; x < dfBitmap.Width; x++)
            {
                for (int y = 0; y < dfBitmap.Height; y++)
                {
                    Color currentColor = GetPixel(ref dfBitmap, x, y);
                    if (y < newBitmap.Height - 1 && x < newBitmap.Width - 1)
                    {
                        Color tempColor = GetPixel(ref dfBitmap, x + 1, y + 1);
                        if (ColorDistance(ref currentColor, ref tempColor) > threshold)
                            SetPixel(ref newBitmap, x, y, edgeColor);
                    }
                    else if (y < newBitmap.Height - 1)
                    {
                        Color tempColor = GetPixel(ref dfBitmap, x, y + 1);
                        if (ColorDistance(ref currentColor, ref tempColor) > threshold)
                            SetPixel(ref newBitmap, x, y, edgeColor);
                    }
                    else if (x < newBitmap.Width - 1)
                    {
                        Color tempColor = GetPixel(ref dfBitmap, x + 1, y);
                        if (ColorDistance(ref currentColor, ref tempColor) > threshold)
                            SetPixel(ref newBitmap, x, y, edgeColor);
                    }
                }
            }

            return newBitmap;
        }

        /// <summary>
        /// Gets distance between two colours using Euclidean distance function.
        ///  Distance = SQRT( (R1-R2)2 + (G1-G2)2 + (B1-B2)2 )
        /// </summary>
        /// <param name="color1">First Color.</param>
        /// <param name="color2">Second Color.</param>
        /// <returns>Distance between colours.</returns>
        private float ColorDistance(ref Color color1, ref Color color2)
        {
            float r = color1.R - color2.R;
            float g = color1.G - color2.G;
            float b = color1.B - color2.B;

            return (float)Math.Sqrt((r * r) + (g * g) + (b * b));
        }

        #endregion

        #region Matrix Convolution Filter

        /*
         * Based on Craig's Utility Library (CUL) by James Craig.
         * http://www.gutgames.com/post/Matrix-Convolution-Filters-in-C.aspx
         * MIT License (http://www.opensource.org/licenses/mit-license.php)
         */

        /// <summary>
        /// Used when applying convolution filters to an image.
        /// </summary>
        public class Filter
        {
            #region Constructors
            /// <summary>
            /// Constructor
            /// </summary>
            public Filter()
            {
                MyFilter = new int[3, 3];
                Width = 3;
                Height = 3;
                Offset = 0;
                Absolute = false;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="Width">Width</param>
            /// <param name="Height">Height</param>
            public Filter(int Width, int Height)
            {
                MyFilter = new int[Width, Height];
                this.Width = Width;
                this.Height = Height;
                Offset = 0;
                Absolute = false;
            }
            #endregion

            #region Public Properties
            /// <summary>
            /// The actual filter array
            /// </summary>
            public int[,] MyFilter { get; set; }

            /// <summary>
            /// Width of the filter box
            /// </summary>
            public int Width { get; set; }

            /// <summary>
            /// Height of the filter box
            /// </summary>
            public int Height { get; set; }

            /// <summary>
            /// Amount to add to the red, blue, and green values
            /// </summary>
            public int Offset { get; set; }

            /// <summary>
            /// Determines if we should take the absolute value prior to clamping
            /// </summary>
            public bool Absolute { get; set; }
            #endregion

            #region Public Methods
            /// <summary>
            /// Applies the filter to the input image
            /// </summary>
            /// <param name="Input">input image</param>
            /// <returns>Returns a separate image with the filter applied</returns>
            public DFBitmap ApplyFilter(ref DFBitmap Input)
            {
                DFBitmap NewBitmap = CloneDFBitmap(ref Input, false);
                for (int x = 0; x < Input.Width; ++x)
                {
                    for (int y = 0; y < Input.Height; ++y)
                    {
                        int RValue = 0;
                        int GValue = 0;
                        int BValue = 0;
                        int Weight = 0;
                        int XCurrent = -Width / 2;
                        for (int x2 = 0; x2 < Width; ++x2)
                        {
                            if (XCurrent + x < Input.Width && XCurrent + x >= 0)
                            {
                                int YCurrent = -Height / 2;
                                for (int y2 = 0; y2 < Height; ++y2)
                                {
                                    if (YCurrent + y < Input.Height && YCurrent + y >= 0)
                                    {
                                        Color Pixel = GetPixel(ref Input, XCurrent + x, YCurrent + y);
                                        RValue += MyFilter[x2, y2] * Pixel.R;
                                        GValue += MyFilter[x2, y2] * Pixel.G;
                                        BValue += MyFilter[x2, y2] * Pixel.B;
                                        Weight += MyFilter[x2, y2];
                                    }
                                    ++YCurrent;
                                }
                            }
                            ++XCurrent;
                        }

                        Color MeanPixel = GetPixel(ref Input, x, y);
                        if (Weight == 0)
                            Weight = 1;
                        if (Weight > 0)
                        {
                            if (Absolute)
                            {
                                RValue = System.Math.Abs(RValue);
                                GValue = System.Math.Abs(GValue);
                                BValue = System.Math.Abs(BValue);
                            }

                            RValue = (RValue / Weight) + Offset;
                            RValue = (int)MathHelper.Clamp(RValue, 0, 255);
                            GValue = (GValue / Weight) + Offset;
                            GValue = (int)MathHelper.Clamp(GValue, 0, 255);
                            BValue = (BValue / Weight) + Offset;
                            BValue = (int)MathHelper.Clamp(BValue, 0, 255);
                            MeanPixel = Color.FromNonPremultiplied(RValue, GValue, BValue, 0xff);
                        }

                        SetPixel(ref NewBitmap, x, y, MeanPixel);
                    }
                }

                return NewBitmap;
            }
            #endregion
        }

        #endregion

        #endregion

        #region Climate Swaps

        /// <summary>
        /// Sets climate and weather type to use for texture swaps.
        /// </summary>
        /// <param name="climate">Climate type.</param>
        /// <param name="weather">Weather type.</param>
        private void SetClimate(DFLocation.ClimateBaseType climate, DFLocation.ClimateWeather weather)
        {
            // Store new climate settings
            this.climateType = climate;
            this.climateWeather = weather;
        }

        /// <summary>
        /// Gets the base climate set the specified archive belongs to.
        /// </summary>
        /// <param name="archive">Archive from which to derive set.</param>
        /// <param name="supportsWinter">True if there is a winter version of this set.</param>
        /// <param name="supportsRain">True if there is a rain version of this set.</param>
        /// <returns>Derived ClimateSet.</returns>
        private DFLocation.ClimateTextureSet GetClimateSet(int archive, out bool supportsWinter, out bool supportsRain)
        {
            // Get climate set
            supportsWinter = false;
            supportsRain = false;
            DFLocation.ClimateTextureSet set = (DFLocation.ClimateTextureSet)(archive - (archive / 100) * 100);
            switch (set)
            {
                //
                // Terrain sets
                //
                case DFLocation.ClimateTextureSet.Exterior_Terrain:
                    supportsWinter = true;
                    supportsRain = true;
                    break;
                //
                // Exterior sets
                //
                case DFLocation.ClimateTextureSet.Exterior_Castle:
                case DFLocation.ClimateTextureSet.Exterior_CityA:
                case DFLocation.ClimateTextureSet.Exterior_CityB:
                case DFLocation.ClimateTextureSet.Exterior_CityWalls:
                case DFLocation.ClimateTextureSet.Exterior_Farm:
                case DFLocation.ClimateTextureSet.Exterior_Fences:
                case DFLocation.ClimateTextureSet.Exterior_MagesGuild:
                case DFLocation.ClimateTextureSet.Exterior_Manor:
                case DFLocation.ClimateTextureSet.Exterior_MerchantHomes:
                case DFLocation.ClimateTextureSet.Exterior_Roofs:
                case DFLocation.ClimateTextureSet.Exterior_Ruins:
                case DFLocation.ClimateTextureSet.Exterior_TavernExteriors:
                case DFLocation.ClimateTextureSet.Exterior_TempleExteriors:
                case DFLocation.ClimateTextureSet.Exterior_Village:
                    supportsWinter = true;
                    break;
                //
                // Interior sets
                //
                case DFLocation.ClimateTextureSet.Interior_Caves:
                case DFLocation.ClimateTextureSet.Interior_CityInt:
                case DFLocation.ClimateTextureSet.Interior_CryptA:
                case DFLocation.ClimateTextureSet.Interior_CryptB:
                case DFLocation.ClimateTextureSet.Interior_Doors:
                case DFLocation.ClimateTextureSet.Interior_DungeonsA:
                case DFLocation.ClimateTextureSet.Interior_DungeonsB:
                case DFLocation.ClimateTextureSet.Interior_DungeonsC:
                case DFLocation.ClimateTextureSet.Interior_DungeonsNEWCs:
                case DFLocation.ClimateTextureSet.Interior_FarmInt:
                case DFLocation.ClimateTextureSet.Interior_MagesGuildInt:
                case DFLocation.ClimateTextureSet.Interior_ManorInt:
                case DFLocation.ClimateTextureSet.Interior_MarbleFloors:
                case DFLocation.ClimateTextureSet.Interior_MerchantHomesInt:
                case DFLocation.ClimateTextureSet.Interior_Mines:
                case DFLocation.ClimateTextureSet.Interior_Paintings:
                case DFLocation.ClimateTextureSet.Interior_PalaceInt:
                case DFLocation.ClimateTextureSet.Interior_Sewer:
                case DFLocation.ClimateTextureSet.Interior_TavernInt:
                case DFLocation.ClimateTextureSet.Interior_TempleInt:
                case DFLocation.ClimateTextureSet.Interior_VillageInt:
                    break;
                //
                // All other results
                //
                default:
                    return DFLocation.ClimateTextureSet.None;
            }

            // Confirmed valid set
            return set;
        }

        #endregion

    }

}
