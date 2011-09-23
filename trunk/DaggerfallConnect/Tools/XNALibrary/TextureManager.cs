// Project:         XNALibrary
// Description:     Simple XNA game library for DaggerfallConnect.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace XNALibrary
{
    // Differentiate between Color types
    using GDIColor = System.Drawing.Color;
    using XNAColor = Microsoft.Xna.Framework.Graphics.Color;

    /// <summary>
    /// Helper class to load and store Daggerfall textures for XNA. Enables climate substitutions
    ///  (Daggerfall swaps textures based on climate type) and texture atlasing for ground tiles.
    ///  Provides some loading and pre-processing options. See TextureCreateFlags for more details.
    /// </summary>
    public class TextureManager
    {

        #region Class Variables

        // Class
        private string arena2Path;
        private GraphicsDevice graphicsDevice;
        private TextureFile textureFile;

        // Atlas dimensions
        private const int atlasWidth = 1024;
        private const int atlasHeight = 1024;

        // Atlas layout
        private Dictionary<int, RectangleF> terrainAtlasDict;

        // Terrain atlas textures
        private Texture2D desertAtlas;
        private Texture2D mountainAtlas;
        private Texture2D temperateAtlas;
        private Texture2D swampAtlas;

        // Texture dictionaries
        private const int atlasTextureKey = -1000000;
        private Dictionary<int, Texture2D> generalTextureDict;
        private Dictionary<int, Texture2D> winterTextureDict;

        // Climate and weather
        DFLocation.ClimateBaseType climateType = DFLocation.ClimateBaseType.None;
        DFLocation.ClimateWeather climateWeather = DFLocation.ClimateWeather.Normal;

        #endregion

        #region Class Structures

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
            ApplyClimate = 1,

            /// <summary>
            /// Ensures texture will be POW2 by extending right and
            ///  bottom dimensions to nearest POW2 boundary. Texture
            ///  will remain at top-left of total area. It is the
            ///  responsibility of the caller to ensure model UVs are
            ///  modified to correctly address valid space in the
            ///  POW2 texture.
            ///  </summary>
            PowerOfTwo = 2,

            /// <summary>
            /// Blends colour into neighbouring alpha pixels to help
            ///  create softer borders. Most useful when loading
            ///  billboard textures. Also extends image dimensions
            ///  by a couple of pixels around each edge so images do
            ///  not appear to be cut off at the edges.
            /// </summary>
            Dilate = 4,

            /// <summary>
            /// Pre-multiplies alpha with colour channels. It is the
            ///  responsibility of the caller to ensure renderer blend
            ///  states are set to use pre-multiplied textures.
            /// </summary>
            PreMultiplyAlpha = 8,

            /// <summary>
            /// Creates a chain of mipmaps for this texture.
            /// </summary>
            MipMaps = 16,
        }

        /// <summary>
        /// Parameters of climate atlas during build.
        /// </summary>
        private struct AtlasParams
        {
            public DFLocation.ClimateBaseType climate;
            public Dictionary<int, RectangleF> dictionary;
            public int format;
            public int width;
            public int height;
            public int stride;
            public int xpos;
            public int ypos;
            public int gutter;
            public int maxRowHeight;
            public byte[] buffer;
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
        /// Gets terrain atlas texture key.
        /// </summary>
        public static int TerrainAtlasKey
        {
            get { return atlasTextureKey; }
        }

        /// <summary>
        /// Gets TextureFile object.
        /// </summary>
        public TextureFile TextureFile
        {
            get { return textureFile; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="device">Graphics Device.</param>
        /// <param name="arena2Path">Path to Arena2 folder.</param>
        public TextureManager(GraphicsDevice device, string arena2Path)
        {
            // Setup
            graphicsDevice = device;
            this.arena2Path = arena2Path;
            textureFile = new TextureFile();
            textureFile.Palette.Load(Path.Combine(arena2Path, textureFile.PaletteName));

            // Create dictionaries
            terrainAtlasDict = new Dictionary<int, RectangleF>();
            generalTextureDict = new Dictionary<int, Texture2D>();
            winterTextureDict = new Dictionary<int, Texture2D>();

            // Set default climate
            SetClimate(climateType, climateWeather);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads a texture from archive and record index. Animated textures are
        ///  not supported at this time and only first frame will be loaded.
        ///  When using climate swaps the key returned will be stable. This means
        ///  you can store this key with your mesh/scene and TextureManager will
        ///  return the correct climate and weather variant for that key when you
        ///  call GetTexture(key) during rendering.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <param name="flags">TextureCreateFlags.</param>
        /// <returns>Texture key.</returns>
        public int LoadTexture(int archive, int record, TextureCreateFlags flags)
        {
            // Load based on flags
            if (TextureCreateFlags.ApplyClimate == (flags & TextureCreateFlags.ApplyClimate) &&
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
        /// Gets texture by key.
        ///  Use static TerrainAtlasKey property for terrain key.
        ///  Manager will return NULL if texture does not exist.
        /// </summary>
        /// <param name="key">Texture key.</param>
        /// <returns>Texture2D.</returns>
        public Texture2D GetTexture(int key)
        {
            // Return atlas if requested
            if (key == atlasTextureKey)
                return GetTerrainAtlas();

            // Try to return winter texture when required
            if (this.climateWeather == DFLocation.ClimateWeather.Winter)
            {
                if (winterTextureDict.ContainsKey(key))
                    return winterTextureDict[key];
            }

            // Otherwise return general texture
            if (!generalTextureDict.ContainsKey(key))
                return null;
            else
                return generalTextureDict[key];
        }

        /// <summary>
        /// Gets rectangle of specific subtexture inside terrain atlas.
        ///  Returns error subtexture on invalid key.
        /// </summary>
        /// <param name="record">Record index.</param>
        /// <returns>RectangleF of subtexture.</returns>
        public RectangleF GetTerrainSubTextureRect(int record)
        {
            // Get the subtexture rectangle
            int key = GetAtlasTextureKey(DFLocation.ClimateTextureSet.Exterior_Terrain, climateWeather, record);
            if (!terrainAtlasDict.ContainsKey(key))
                return terrainAtlasDict[0];
            else
                return terrainAtlasDict[key];
        }

        /// <summary>
        /// Remove cached texture based on key.
        ///  Cannot remove terrain atlas by key,
        ///  use ClearAtlases() method instead.
        /// </summary>
        /// <param name="key">Texture key.</param>
        public void RemoveTexture(int key)
        {
            // Remove general texture og key
            if (generalTextureDict.ContainsKey(key))
                generalTextureDict.Remove(key);

            // Remove winter texture of key
            if (winterTextureDict.ContainsKey(key))
                winterTextureDict.Remove(key);
        }

        /// <summary>
        /// Clear all cached textures.
        /// </summary>
        public void ClearTextures()
        {
            // Remove general and winter dictionaries
            generalTextureDict.Clear();
            winterTextureDict.Clear();
        }

        /// <summary>
        /// Clear all climate dictionaries.
        /// </summary>
        public void ClearAtlases()
        {
            desertAtlas = null;
            mountainAtlas = null;
            temperateAtlas = null;
            swampAtlas = null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets unique key for a non-climate-aware texture.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <returns>Texture key.</returns>
        private int GetTextureKey(int archive, int record)
        {
            return (archive * 1000) + record;
        }

        /// <summary>
        /// Gets unique key for a climate-aware texture.
        /// </summary>
        /// <param name="climateType">Climate type.</param>
        /// <param name="climateSet">Climate set.</param>
        /// <param name="record">Record index.</param>
        /// <returns>Texture key.</returns>
        private int GetTextureKey(DFLocation.ClimateBaseType climateType, DFLocation.ClimateTextureSet climateSet, int record)
        {
            return 1000000 + ((int)climateType * 100000) + ((int)climateSet * 1000) + record;
        }

        /// <summary>
        /// Gets unique key for atlas sub-textures.
        /// </summary>
        /// <param name="set">Climate set.</param>
        /// <param name="weather">Climate weather.</param>
        /// <param name="record">Record index.</param>
        /// <returns>Climate texture key.</returns>
        private int GetAtlasTextureKey(DFLocation.ClimateTextureSet set, DFLocation.ClimateWeather weather, int record)
        {
            return (int)set * 10000 + (int)weather * 100 + record;
        }

        /// <summary>
        /// Load a texture without climate processing.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <param name="flags">TextureCreateFlags.</param>
        /// <returns>Texture key.</returns>
        private int LoadTextureNoClimate(int archive, int record, TextureCreateFlags flags)
        {
            int key = GetTextureKey(archive, record);
            if (generalTextureDict.ContainsKey(key))
                return key;
            else
                CreateTexture(key, archive, record, false, flags);

            return key;
        }

        /// <summary>
        /// Load a texture with climate processing.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <param name="flags">TextureCreateFlags.</param>
        /// <returns>Texture key.</returns>
        private int LoadTextureWithClimate(int archive, int record, TextureCreateFlags flags)
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

            // Check if key already exists
            int key = GetTextureKey(climateType, climateSet, record);
            if (this.climateWeather == DFLocation.ClimateWeather.Winter)
            {
                if (winterTextureDict.ContainsKey(key))
                    return key;
            }
            else
            {
                if (generalTextureDict.ContainsKey(key))
                    return key;
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
            CreateTexture(key, newArchive, record, supportsWinter, flags);
            return key;
        }

        /// <summary>
        /// Creates texture for the specified archive and record, then
        ///  adds to dictionary against key. Will attempt to load a
        ///  winter variant if specified.
        /// </summary>
        /// <param name="key">Key to associate with texture.</param>
        /// <param name="archive">Archive index to load.</param>
        /// <param name="record">Record index to load.</param>
        /// <param name="supportsWinter">True to load winter set (archive+1).</param>
        /// <param name="flags">TextureCreateFlags.</param>
        private void CreateTexture(int key, int archive, int record, bool supportsWinter, TextureCreateFlags flags)
        {
            // Get normal texture in ARGB format
            textureFile.Load(Path.Combine(arena2Path, TextureFile.IndexToFileName(archive)), FileUsage.UseDisk, true);
            DFBitmap normalBitmap = textureFile.GetBitmapFormat(record, 0, 0, DFBitmap.Formats.ARGB);

            // Perform optional image processing
            ProcessDFBitmap(ref normalBitmap, flags);

            // Create XNA texture
            generalTextureDict.Add(key, CreateTexture2D(ref normalBitmap, flags));

            // Load winter texture
            if (supportsWinter)
            {
                // Get winter texture in ARGB format
                textureFile.Load(Path.Combine(arena2Path, TextureFile.IndexToFileName(archive + 1)), FileUsage.UseDisk, true);
                DFBitmap winterBitmap = textureFile.GetBitmapFormat(record, 0, 0, DFBitmap.Formats.ARGB);

                // Perform optional image processing
                ProcessDFBitmap(ref normalBitmap, flags);
                
                // Create XNA texture
                winterTextureDict.Add(key, CreateTexture2D(ref winterBitmap, flags));
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
            Texture2D texture;
            if (TextureCreateFlags.MipMaps == (flags & TextureCreateFlags.MipMaps))
            {
                // Create XNA texture with mipmaps
                texture = new Texture2D(
                    graphicsDevice,
                    width,
                    height,
                    0,
                    TextureUsage.AutoGenerateMipMap,
                    SurfaceFormat.Color);
            }
            else
            {
                // Create XNA texture without mipmaps
                texture = new Texture2D(
                    graphicsDevice,
                    dfBitmap.Width,
                    dfBitmap.Height, 1,
                    TextureUsage.None,
                    SurfaceFormat.Color);
            }

            // Set data
            texture.SetData<byte>(
                0,
                new Microsoft.Xna.Framework.Rectangle(0, 0, dfBitmap.Width, dfBitmap.Height),
                dfBitmap.Data,
                0,
                dfBitmap.Width * dfBitmap.Height * 4,
                SetDataOptions.None);

            return texture;
        }

        /// <summary>
        /// Gets current terrain atlas based on climate.
        /// </summary>
        /// <returns>Texture2D.</returns>
        private Texture2D GetTerrainAtlas()
        {
            switch (this.climateType)
            {
                case DFLocation.ClimateBaseType.Desert:
                    return desertAtlas;
                case DFLocation.ClimateBaseType.Mountain:
                    return mountainAtlas;
                case DFLocation.ClimateBaseType.Temperate:
                    return temperateAtlas;
                case DFLocation.ClimateBaseType.Swamp:
                    return swampAtlas;
                default:
                    return temperateAtlas;
            }
        }

        /// <summary>
        /// Performs optional processing to DFBitmap.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap.</param>
        /// <param name="flags">TextureCreateFlags.</param>
        private void ProcessDFBitmap(ref DFBitmap dfBitmap, TextureCreateFlags flags)
        {
            // Dilate
            if (TextureCreateFlags.Dilate == (flags & TextureCreateFlags.Dilate))
                DilateDFBitmap(ref dfBitmap);

            // Pre-multiply alpha
            if (TextureCreateFlags.PreMultiplyAlpha == (flags & TextureCreateFlags.PreMultiplyAlpha))
                PreMultiplyAlphaDFBitmap(ref dfBitmap);
        }

        /// <summary>
        /// Pre-multiply bitmap alpha.
        /// </summary>
        /// <param name="dfBitmap">FBitmap.</param>
        private void PreMultiplyAlphaDFBitmap(ref DFBitmap dfBitmap)
        {
            // Format must be ARGB
            if (dfBitmap.Format != DFBitmap.Formats.ARGB)
                throw new Exception("DFBitmap not ARGB.");

            // Constants
            const int formatWidth = 4;

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

        /// <summary>
        /// Dilate bitmap.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap.</param>
        private void DilateDFBitmap(ref DFBitmap dfBitmap)
        {
            // Format must be ARGB
            if (dfBitmap.Format != DFBitmap.Formats.ARGB)
                throw new Exception("DFBitmap not ARGB.");

            // Constants
            const int formatWidth = 4;
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
                dstBuffer[dstPos + 3] = 1;
            }
        }

        #endregion

        #region Climate Swaps

        /// <summary>
        /// Sets climate and weather type to use for texture swaps.
        /// </summary>
        /// <param name="climate">Climate type.</param>
        /// <param name="weather">Weather type.</param>
        private void SetClimate(DFLocation.ClimateBaseType climate, DFLocation.ClimateWeather weather)
        {
            // Load new terrain atlas, using temperate when none specified
            if (climate == DFLocation.ClimateBaseType.None)
                BuildTerrainAtlas(DFLocation.ClimateBaseType.Temperate);
            else
                BuildTerrainAtlas(climate);

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

        #region Atlas Building

        /// <summary>
        /// Builds a texture atlas from terrain ground tiles. This allows ground planes to be
        ///  drawn in a single batch.
        /// </summary>
        /// <param name="climate">Climate type.</param>
        /// <returns>True if successful.</returns>
        private bool BuildTerrainAtlas(DFLocation.ClimateBaseType climate)
        {
            // Return true if atlas already built
            switch (climate)
            {
                case DFLocation.ClimateBaseType.Desert:
                    if (desertAtlas != null) return true;
                    break;
                case DFLocation.ClimateBaseType.Mountain:
                    if (mountainAtlas != null) return true;
                    break;
                case DFLocation.ClimateBaseType.Temperate:
                    if (temperateAtlas != null) return true;
                    break;
                case DFLocation.ClimateBaseType.Swamp:
                    if (swampAtlas != null) return true;
                    break;
            }

            // Init working parameters
            AtlasParams ap = new AtlasParams();
            ap.dictionary = new Dictionary<int, RectangleF>();
            ap.climate = climate;
            ap.format = 4;
            ap.width = atlasWidth;
            ap.height = atlasHeight;
            ap.stride = atlasWidth * ap.format;
            ap.gutter = 0;
            ap.xpos = ap.gutter * ap.format;
            ap.ypos = ap.gutter * ap.stride;
            ap.maxRowHeight = 0;
            ap.buffer = new byte[(atlasWidth * atlasHeight) * ap.format];

            // Add textures to atlas
            //AddErrorBitmap(ref ap);
            AddTextureFile(DFLocation.ClimateTextureSet.Exterior_Terrain, DFLocation.ClimateWeather.Normal, ref ap);
            AddTextureFile(DFLocation.ClimateTextureSet.Exterior_Terrain, DFLocation.ClimateWeather.Winter, ref ap);
            AddTextureFile(DFLocation.ClimateTextureSet.Exterior_Terrain, DFLocation.ClimateWeather.Rain, ref ap);

            // Create texture from atlas buffer
            Texture2D atlasTexture = new Texture2D(graphicsDevice, atlasWidth, atlasHeight, 0, TextureUsage.AutoGenerateMipMap, SurfaceFormat.Color);
            atlasTexture.SetData<byte>(ap.buffer);
            switch (climate)
            {
                case DFLocation.ClimateBaseType.Desert:
                    desertAtlas = atlasTexture;
                    break;
                case DFLocation.ClimateBaseType.Mountain:
                    mountainAtlas = atlasTexture;
                    break;
                case DFLocation.ClimateBaseType.Temperate:
                    temperateAtlas = atlasTexture;
                    break;
                case DFLocation.ClimateBaseType.Swamp:
                    swampAtlas = atlasTexture;
                    break;
            }

            // Only need to store dictionary once, and it's the same
            // for all climate types.
            if (terrainAtlasDict.Count == 0)
                terrainAtlasDict = ap.dictionary;

            // TEST: Save texture for review
            //atlasTexture.Save("C:\\test\\Terrain.png", ImageFileFormat.Png);

            return true;
        }

        /// <summary>
        /// Adds all records from a texture file into the atlas.
        /// </summary>
        /// <param name="set">Climate set to add.</param>
        /// <param name="weather">Weather set to add.</param>
        /// <param name="ap">AtlasParams.</param>
        private void AddTextureFile(DFLocation.ClimateTextureSet set, DFLocation.ClimateWeather weather, ref AtlasParams ap)
        {
            // Resolve climate set and weather to filename
            string filename = ImageFileReader.GetClimateTextureFileName(ap.climate, set, weather);

            // Load Daggerfall texture file
            textureFile.Load(Path.Combine(arena2Path, filename), FileUsage.UseDisk, true);

            // Add each record to atlas (not supporting animation yet)
            for (int r = 0; r < textureFile.RecordCount; r++)
            {
                // Get record bitmap in ARGB format
                DFBitmap dfBitmap = textureFile.GetBitmapFormat(r, 0, 0, DFBitmap.Formats.ARGB);

                // Add bitmap to atlas
                int key = GetAtlasTextureKey(set, weather, r);
                RectangleF rect = AddDFBitmap(key, ref dfBitmap, ref ap);

                // Add key to dictionary
                ap.dictionary.Add(key, rect);
            }
        }

        /// <summary>
        /// Adds an individual bitmap to atlas.
        /// </summary>
        /// <param name="key">Unique texture key.</param>
        /// <param name="dfBitmap">DFBitmap to add.</param>
        /// <param name="ap">AtlasParams.</param>
        private RectangleF AddDFBitmap(int key, ref DFBitmap dfBitmap, ref AtlasParams ap)
        {
            // For now can only handle width <= 64 pixels
            int stepx = 64;
            if (dfBitmap.Width > 64)
            {
                Console.WriteLine("Width > 64.");
                return new RectangleF(0, 0, 0, 0);
            }

            // For now can only handle height <= 128 pixels
            int stepy;
            if (dfBitmap.Height <= 64)
                stepy = 64;
            else if (dfBitmap.Height <= 128)
                stepy = 128;
            else
            {
                Console.WriteLine("Height > 128.");
                return new RectangleF(0, 0, 0, 0);
            }

            // Track max height per row
            if (stepy > ap.maxRowHeight)
                ap.maxRowHeight = stepy;

            // Handle row wrap
            if (ap.xpos + dfBitmap.Stride > ap.stride)
            {
                AddNewRow(ref ap);
                ap.maxRowHeight = dfBitmap.Height;
            }

            // Calculate entry point to buffer
            int bufferPos = ap.ypos + ap.xpos;

            // Copy each row of bitmap data to atlas
            for (int y = 0; y < dfBitmap.Height; y++)
            {
                Buffer.BlockCopy(dfBitmap.Data,
                    y * dfBitmap.Stride,
                    ap.buffer,
                    bufferPos + (y * ap.stride),
                    dfBitmap.Stride);
            }

            // Create texture layout
            RectangleF rect = new RectangleF(
                (float)(ap.xpos / ap.format) / (float)ap.width,
                (float)(ap.ypos / ap.stride) / (float)ap.height,
                (float)dfBitmap.Width / (float)ap.width,
                (float)dfBitmap.Height / (float)ap.height);

            // Increment xpos
            ap.xpos += (stepx * ap.format) + (ap.gutter * ap.format);

            return rect;
        }

        /// <summary>
        /// Starts new row in atlas. Will throw on overflow.
        /// </summary>
        /// <param name="ap">AtlasParams.</param>
        private void AddNewRow(ref AtlasParams ap)
        {
            // Step down to next row
            ap.ypos += (ap.maxRowHeight * ap.stride) + (ap.gutter * ap.stride);
            if (ap.ypos >= ap.height * ap.stride)
                throw new Exception("Atlas buffer overflow.");

            // Reset column and max height
            ap.xpos = (ap.gutter * ap.format);
            ap.maxRowHeight = 0;
        }

        /*
        /// <summary>
        /// Adds a red texture to atlas which will be used if an unknown texture is referenced.
        /// </summary>
        /// <param name="ap">AtlasParams.</param>
        private void AddErrorBitmap(ref AtlasParams ap)
        {
            // Create red error image. Can only have one error per
            // atlas and it will always be key 0.
            if (!ap.dictionary.ContainsKey(0))
            {
                DFManualImage errorImage = new DFManualImage(64, 64, DFBitmap.Formats.ARGB);
                errorImage.Clear(0xff, 0xff, 0, 0);
                DFBitmap dfBitmap = errorImage.DFBitmap;
                RectangleF rect = AddDFBitmap(0, ref dfBitmap, ref ap);
                ap.dictionary.Add(0, rect);
            }
        }
        */

        #endregion

    }

}
