﻿// Project:         XNALibrary
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

    // Use climate enums in this namespace
    using ClimateType = DFLocation.ClimateType;
    using ClimateSet = DFLocation.ClimateSet;
    using ClimateWeather = DFLocation.ClimateWeather;

    // Differentiate between Color types
    using GDIColor = System.Drawing.Color;
    using XNAColor = Microsoft.Xna.Framework.Graphics.Color;

    /// <summary>
    /// Helper class to load and store Daggerfall textures for XNA. Enables climate substitutions
    ///  (Daggerfall swaps textures based on climate type) and texture atlasing for ground tiles.
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
        private const int billboardTextureKey = -1000001;
        private Dictionary<int, Texture2D> generalTextureDict;
        private Dictionary<int, Texture2D> winterTextureDict;

        // Climate and weather
        ClimateType climateType = ClimateType.None;
        ClimateWeather climateWeather = ClimateWeather.Normal;

        #endregion

        #region Class Structures

        /// <summary>
        /// Parameters of climate atlas during build.
        /// </summary>
        private struct AtlasParams
        {
            public ClimateType climate;
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
        /// Gets Arena2 path set at construction.
        /// </summary>
        public string Arena2Path
        {
            get { return arena2Path; }
        }

        /// <summary>
        /// Gets or sets current climate for swaps. Setting this to 
        ///  None will use default textures assigned to models in ARCH3D.BSA.
        ///  Temperate ground tiles will be used when Climate is None.
        /// </summary>
        public ClimateType Climate
        {
            get { return climateType; }
            set { SetClimate(value, climateWeather); }
        }

        /// <summary>
        /// Gets or sets current weather for swaps.
        /// </summary>
        public ClimateWeather Weather
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
        /// Gets rectangle of specific subtexture inside terrain atlas.
        ///  Returns error subtexture on invalid key.
        /// </summary>
        /// <param name="record">Record index.</param>
        /// <returns>RectangleF of subtexture.</returns>
        public RectangleF GetTerrainSubTextureRect(int record)
        {
            // Get the subtexture rectangle
            int key = GetAtlasTextureKey(ClimateSet.Exterior_Terrain, climateWeather, record);
            if (!terrainAtlasDict.ContainsKey(key))
                return terrainAtlasDict[0];
            else
                return terrainAtlasDict[key];
        }

        /// <summary>
        /// Loads a texture based on indices. Only first frame of animated textures
        ///  will be loaded, as animated textures are not supported at this time.
        ///  If a ClimateType is set, and the specified texture archive is climate-
        ///  specific, the appropriate climate and weather texture will be loaded instead.
        ///  The key will be the same no matter the climate or weather, allowing you to
        ///  perform swaps without needing to rebuild texture keys on model submeshes.
        ///  Textures loaded without climate processing will not be given mipmaps and
        ///  will not be forced to POW2 dimensions.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <param name="allowClimate">True to allow climate processing.</param>
        /// <returns>Texture key.</returns>
        public int LoadTexture(int archive, int record, bool allowClimate)
        {
            // Load without climate processing
            int key;
            if (!allowClimate)
            {
                key = GetTextureKey(archive, record);
                if (generalTextureDict.ContainsKey(key))
                {
                    return key;
                }
                else
                {
                    LoadTexture(key, archive, record, false, false);
                    return key;
                }
            }

            // Get the base set this archive belongs to regardless of climate
            bool supportsWinter, supportsRain;
            ClimateSet climateSet = GetClimateSet(archive, out supportsWinter, out supportsRain);

            // Load non climate aware textures, or no climate type specified
            if (this.climateType == ClimateType.None || climateSet == ClimateSet.None)
            {
                key = GetTextureKey(archive, record);
                if (generalTextureDict.ContainsKey(key))
                {
                    return key;
                }
                else
                {
                    LoadTexture(key, archive, record, false, true);
                    return key;
                }
            }

            // Check if key already exists
            key = GetTextureKey(climateType, climateSet, record);
            if (this.climateWeather == ClimateWeather.Winter)
            {
                if (winterTextureDict.ContainsKey(key))
                    return key;
            }
            else
            {
                if (generalTextureDict.ContainsKey(key))
                    return key;
            }

            // Swap sets are missing a winter file in certain climates.
            if (climateType == ClimateType.Desert || climateType == ClimateType.Swamp)
            {
                switch ((int)climateSet)
                {
                    case 9:
                    case 35:
                        supportsWinter = false;
                        break;
                }
            }

            // Load climate aware texture
            int newArchive = (int)climateType + (int)climateSet;
            LoadTexture(key, newArchive, record, supportsWinter, true);
            return key;
        }

        /// <summary>
        /// Get texture for current climate, based on key.
        ///  Use TerrainAtlasKey property for terrain key.
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
            if (this.climateWeather == ClimateWeather.Winter)
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
        /// Gets unique key for a non climate aware texture.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <returns>Texture key.</returns>
        private int GetTextureKey(int archive, int record)
        {
            return (archive * 1000) + record;
        }

        /// <summary>
        /// Gets unique key for a climate aware texture.
        /// </summary>
        /// <param name="climateType">Climate type.</param>
        /// <param name="climateSet">Climate set.</param>
        /// <param name="record">Record index.</param>
        /// <returns>Texture key.</returns>
        private int GetTextureKey(ClimateType climateType, ClimateSet climateSet, int record)
        {
            return 1000000 + ((int)climateType * 100000) + ((int)climateSet * 1000) + record;
        }

        /// <summary>
        /// Gets unique key for a atlas textures.
        /// </summary>
        /// <param name="set">Climate set.</param>
        /// <param name="weather">Climate weather.</param>
        /// <param name="record">Record index.</param>
        /// <returns>Climate texture key.</returns>
        private int GetAtlasTextureKey(ClimateSet set, ClimateWeather weather, int record)
        {
            return (int)set * 10000 + (int)weather * 100 + record;
        }

        /// <summary>
        /// Loads both normal and winter textures for the specified archive.
        ///  Must always specify base archive index, never winter or rain indices.
        /// </summary>
        /// <param name="key">Key to associate with texture.</param>
        /// <param name="archive">Archive index to load.</param>
        /// <param name="record">Record index to load.</param>
        /// /// <param name="createMipMaps">True to create mip-maps for this texture.</param>
        private void LoadTexture(int key, int archive, int record, bool supportsWinter, bool createMipMaps)
        {
            // Get normal texture in ARGB format
            textureFile.Load(Path.Combine(arena2Path, TextureFile.IndexToFileName(archive)), FileUsage.UseDisk, true);
            DFBitmap normalBitmap = textureFile.GetBitmapFormat(record, 0, 0, DFBitmap.Formats.ARGB);

            // Get winter texture in ARGB format
            textureFile.Load(Path.Combine(arena2Path, TextureFile.IndexToFileName(archive + 1)), FileUsage.UseDisk, true);
            DFBitmap winterBitmap = textureFile.GetBitmapFormat(record, 0, 0, DFBitmap.Formats.ARGB);

            // Create XNA textures
            generalTextureDict.Add(key, CreateTexture(ref normalBitmap, createMipMaps));
            if (supportsWinter)
                winterTextureDict.Add(key, CreateTexture(ref winterBitmap, createMipMaps));
        }

        /// <summary>
        /// Creates Texture2D from DFBitmap.
        /// </summary>
        /// <param name="dfBitmap">DFBitmap source. Must be in ARGB format.</param>
        /// <param name="createMipMaps">True to create mip-maps for this texture.</param>
        /// <returns>Texture2D.</returns>
        private Texture2D CreateTexture(ref DFBitmap dfBitmap, bool createMipMaps)
        {
            Texture2D texture;
            if (createMipMaps)
            {
                // Get width and height as power of 2
                int width = (PowerOfTwo.IsPowerOfTwo(dfBitmap.Width)) ? dfBitmap.Width : PowerOfTwo.NextPowerOfTwo(dfBitmap.Width);
                int height = (PowerOfTwo.IsPowerOfTwo(dfBitmap.Height)) ? dfBitmap.Height : PowerOfTwo.NextPowerOfTwo(dfBitmap.Height);

                // Create XNA texture
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
                // Create XNA texture
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
                case ClimateType.Desert:
                    return desertAtlas;
                case ClimateType.Mountain:
                    return mountainAtlas;
                case ClimateType.Temperate:
                    return temperateAtlas;
                case ClimateType.Swamp:
                    return swampAtlas;
                default:
                    return temperateAtlas;
            }
        }

        #endregion

        #region Climate Swaps

        /// <summary>
        /// Sets climate and weather type to use for texture swaps.
        /// </summary>
        /// <param name="climate">Climate type.</param>
        /// <param name="weather">Weather type.</param>
        private void SetClimate(ClimateType climate, ClimateWeather weather)
        {
            // Load new terrain atlas, using temperate when none specified
            if (climate == ClimateType.None)
                BuildTerrainAtlas(ClimateType.Temperate);
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
        private ClimateSet GetClimateSet(int archive, out bool supportsWinter, out bool supportsRain)
        {
            // Get climate set
            supportsWinter = false;
            supportsRain = false;
            ClimateSet set = (ClimateSet)(archive - (archive / 100) * 100);
            switch (set)
            {
                //
                // Terrain sets
                //
                case ClimateSet.Exterior_Terrain:
                    supportsWinter = true;
                    supportsRain = true;
                    break;
                //
                // Exterior sets
                //
                case ClimateSet.Exterior_Castle:
                case ClimateSet.Exterior_CityA:
                case ClimateSet.Exterior_CityB:
                case ClimateSet.Exterior_CityWalls:
                case ClimateSet.Exterior_Farm:
                case ClimateSet.Exterior_Fences:
                case ClimateSet.Exterior_MagesGuild:
                case ClimateSet.Exterior_Manor:
                case ClimateSet.Exterior_MerchantHomes:
                case ClimateSet.Exterior_Roofs:
                case ClimateSet.Exterior_Ruins:
                case ClimateSet.Exterior_TavernExteriors:
                case ClimateSet.Exterior_TempleExteriors:
                case ClimateSet.Exterior_Village:
                    supportsWinter = true;
                    break;
                //
                // Interior sets
                //
                case ClimateSet.Interior_Caves:
                case ClimateSet.Interior_CityInt:
                case ClimateSet.Interior_CryptA:
                case ClimateSet.Interior_CryptB:
                case ClimateSet.Interior_Doors:
                case ClimateSet.Interior_DungeonsA:
                case ClimateSet.Interior_DungeonsB:
                case ClimateSet.Interior_DungeonsC:
                case ClimateSet.Interior_DungeonsNEWCs:
                case ClimateSet.Interior_FarmInt:
                case ClimateSet.Interior_MagesGuildInt:
                case ClimateSet.Interior_ManorInt:
                case ClimateSet.Interior_MarbleFloors:
                case ClimateSet.Interior_MerchantHomesInt:
                case ClimateSet.Interior_Mines:
                case ClimateSet.Interior_Paintings:
                case ClimateSet.Interior_PalaceInt:
                case ClimateSet.Interior_Sewer:
                case ClimateSet.Interior_TavernInt:
                case ClimateSet.Interior_TempleInt:
                case ClimateSet.Interior_VillageInt:
                    break;
                //
                // All other results
                //
                default:
                    return ClimateSet.None;
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
        private bool BuildTerrainAtlas(ClimateType climate)
        {
            // Return true if atlas already built
            switch (climate)
            {
                case ClimateType.Desert:
                    if (desertAtlas != null) return true;
                    break;
                case ClimateType.Mountain:
                    if (mountainAtlas != null) return true;
                    break;
                case ClimateType.Temperate:
                    if (temperateAtlas != null) return true;
                    break;
                case ClimateType.Swamp:
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
            AddTextureFile(ClimateSet.Exterior_Terrain, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSet.Exterior_Terrain, ClimateWeather.Winter, ref ap);
            AddTextureFile(ClimateSet.Exterior_Terrain, ClimateWeather.Rain, ref ap);

            // Create texture from atlas buffer
            Texture2D atlasTexture = new Texture2D(graphicsDevice, atlasWidth, atlasHeight, 0, TextureUsage.AutoGenerateMipMap, SurfaceFormat.Color);
            atlasTexture.SetData<byte>(ap.buffer);
            switch (climate)
            {
                case ClimateType.Desert:
                    desertAtlas = atlasTexture;
                    break;
                case ClimateType.Mountain:
                    mountainAtlas = atlasTexture;
                    break;
                case ClimateType.Temperate:
                    temperateAtlas = atlasTexture;
                    break;
                case ClimateType.Swamp:
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
        private void AddTextureFile(ClimateSet set, ClimateWeather weather, ref AtlasParams ap)
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
