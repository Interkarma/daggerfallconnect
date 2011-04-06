// Project:         XNALibrary
// Description:     Simple XNA game library for DaggerfallConnect.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using System.IO;
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
        private const int atlasWidth = 512;
        private const int atlasHeight = 512;

        // Atlas texture
        private Texture2D terrainAtlas;

        // Atlas layout
        private Dictionary<int, RectangleF> terrainAtlasDict;

        // Texture dictonary
        private Dictionary<int, Texture2D> texturesDict;

        // Climate and weather
        ClimateType climateType = ClimateType.None;
        ClimateWeather climateWeather = ClimateWeather.Normal;

        #endregion

        #region Class Structures

        /// <summary>
        /// Stores a Texture2D resource along with information
        /// need for texture swaps.
        /// </summary>
        private struct DFTexture
        {
            public Texture2D texture;
            public ClimateSet set;
            public bool winterValid;
            public bool rainValid;
        }

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
        /// Gets the GraphicsDevice set at construction.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get { return graphicsDevice; }
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
        /// Gets terrain atlas texture for current climate type.
        /// </summary>
        public Texture2D TerrainAtlas
        {
            get { return terrainAtlas; }
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

            // Create empty dictionaries
            texturesDict = new Dictionary<int, Texture2D>();
            terrainAtlasDict = new Dictionary<int, RectangleF>();

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
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <param name="frame">Frame index.</param>
        /// <returns>Texture key.</returns>
        public int LoadTexture(int archive, int record)
        {
            // Just return key if already in dictionary
            int key = GetTextureKey(archive, record, 0);
            if (texturesDict.ContainsKey(key))
                return key;

            // Load texture file
            if (climateType == ClimateType.None)
            {
                // Load without modifying to climate
                textureFile.Load(Path.Combine(arena2Path, TextureFile.IndexToFileName(archive)), FileUsage.UseDisk, true);
            }
            else
            {
                // Modify archive to climate
                int climateArchive = GetClimateArchive(archive);
                textureFile.Load(Path.Combine(arena2Path, TextureFile.IndexToFileName(climateArchive)), FileUsage.UseDisk, true);
            }

            // Get DF texture in ARGB format so we can just SetData the byte array into XNA
            DFBitmap dfbitmap = textureFile.GetBitmapFormat(record, 0, 0, DFBitmap.Formats.ARGB);

            // Create XNA texture
            Texture2D texture = new Texture2D(graphicsDevice, dfbitmap.Width, dfbitmap.Height, 0, TextureUsage.AutoGenerateMipMap, SurfaceFormat.Color);
            texture.SetData<byte>(dfbitmap.Data);

            // Store texture in dictionary
            texturesDict.Add(key, texture);

            return key;
        }

        /// <summary>
        /// Get texture based on key. The manager will return NULL if texture does not exist.
        /// </summary>
        /// <param name="key">Texture key.</param>
        /// <returns>Texture2D.</returns>
        public Texture2D GetTexture(int key)
        {
            if (!texturesDict.ContainsKey(key))
                return null;
            else
                return texturesDict[key];
        }

        /// <summary>
        /// Removes texture based on key.
        /// </summary>
        /// <param name="key">Texture key.</param>
        public void RemoveTexture(int key)
        {
            if (texturesDict.ContainsKey(key))
                texturesDict.Remove(key);
        }

        /// <summary>
        /// Clear all textures.
        /// </summary>
        public void ClearTextures()
        {
            texturesDict.Clear();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets unique key for a texture.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <param name="frame">Frame index.</param>
        /// <returns>Texture key.</returns>
        private int GetTextureKey(int archive, int record, int frame)
        {
            return (archive * 10000) + (record * 100) + frame;
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

        #endregion

        #region Climate Swaps

        /// <summary>
        /// Sets climate and weather type to use for texture swaps.
        /// </summary>
        /// <param name="climate">Climate type.</param>
        /// <param name="weather">Weather type.</param>
        private void SetClimate(ClimateType climate, ClimateWeather weather)
        {
            // Do nothing if new climate is equal to old climate
            if (climate != ClimateType.None && climate == climateType)
                return;

            // Load new terrain atlas, using temperate when none specified
            if (climate == ClimateType.None)
                BuildTerrainAtlas(ClimateType.Temperate, weather);
            else
                BuildTerrainAtlas(climate, weather);

            // TODO: Modify loaded textures to new archive

            // Store new climate settings
            this.climateType = climate;
            this.climateWeather = weather;
        }

        /// <summary>
        /// Modifies archive index to suit current climate and weather settings.
        ///  Only valid archive swaps are performed.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <returns>Modified archive index.</returns>
        private int GetClimateArchive(int archive)
        {
            // Get climate set
            int newArchive = archive;
            bool winterValid = false;
            bool rainValid = false;
            ClimateSet set = (ClimateSet)(archive - (archive / 100) * 100);
            switch (set)
            {
                //
                // Terrain sets
                //
                case ClimateSet.Exterior_Terrain:
                    winterValid = true;
                    rainValid = true;
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
                    winterValid = true;
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

                default:
                    return archive;     // Not a valid set, just return unmodified.
            }

            // We now know the set is a valid type and which weather combinations are valid.
            // Start by getting the base archive + set.
            newArchive = (int)climateType + (int)set;

            // Adjust for weather. Nothing to do for normal weather.
            switch (climateWeather)
            {
                case ClimateWeather.Winter:
                    if (winterValid)
                        newArchive += 1;        // Winter archive is base + set + 1
                    break;
                case ClimateWeather.Rain:
                    if (rainValid)
                        newArchive += 2;        // Rain archive is base + set + 2
                    break;
                default:
                    break;
            }

            return newArchive;
        }

        #endregion

        #region Atlas Building

        /// <summary>
        /// Builds a texture atlas from terrain ground tiles. This allows ground planes to be
        ///  drawn in a single batch.
        /// </summary>
        /// <param name="climate">Climate type.</param>
        /// <param name="weather">Weather type.</param>
        /// <returns>True if successful.</returns>
        private bool BuildTerrainAtlas(ClimateType climate, ClimateWeather weather)
        {
            // Init working parameters
            AtlasParams ap = new AtlasParams();
            ap.dictionary = terrainAtlasDict;
            ap.climate = climate;
            ap.format = 4;
            ap.width = atlasWidth;
            ap.height = atlasHeight;
            ap.stride = atlasWidth * ap.format;
            ap.xpos = 0;
            ap.ypos = 0;
            ap.maxRowHeight = 0;
            ap.buffer = new byte[(atlasWidth * atlasHeight) * ap.format];

            // Clear existing dictionary
            terrainAtlasDict.Clear();

            // Add textures to atlas
            AddErrorBitmap(ref ap);
            AddTextureFile(ClimateSet.Exterior_Terrain, weather, ref ap);

            // Create texture from atlas buffer
            terrainAtlas = new Texture2D(graphicsDevice, atlasWidth, atlasHeight, 0, TextureUsage.AutoGenerateMipMap, SurfaceFormat.Color);
            terrainAtlas.SetData<byte>(ap.buffer);

            // TEST: Save texture for review
            //terrainAtlas.Save("C:\\test\\Terrain.png", ImageFileFormat.Png);

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
                terrainAtlasDict.Add(key, rect);
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
                    bufferPos + y * ap.stride,
                    dfBitmap.Stride);
            }

            // Create texture layout
            RectangleF rect = new RectangleF(
                (float)(ap.xpos / ap.format) / (float)ap.width,
                (float)(ap.ypos / ap.stride) / (float)ap.height,
                (float)dfBitmap.Width / (float)ap.width,
                (float)dfBitmap.Height / (float)ap.height);

            // Increment xpos
            ap.xpos += stepx * ap.format;

            return rect;
        }

        /// <summary>
        /// Adds a red texture to atlas which will be used if an unknown texture is referenced.
        /// </summary>
        /// <param name="ap">AtlasParams.</param>
        private void AddErrorBitmap(ref AtlasParams ap)
        {
            // Create red error image. Can only have one error per
            // atlas and it will always be key 0.
            if (!terrainAtlasDict.ContainsKey(0))
            {
                DFManualImage errorImage = new DFManualImage(64, 64, DFBitmap.Formats.ARGB);
                errorImage.Clear(0xff, 0xff, 0, 0);
                DFBitmap dfBitmap = errorImage.DFBitmap;
                RectangleF rect = AddDFBitmap(0, ref dfBitmap, ref ap);
                terrainAtlasDict.Add(0, rect);
            }
        }

        /// <summary>
        /// Starts new row in atlas. Will throw on overflow.
        /// </summary>
        /// <param name="ap">AtlasParams.</param>
        private void AddNewRow(ref AtlasParams ap)
        {
            // Step down to next row
            ap.ypos += ap.maxRowHeight * ap.stride;
            if (ap.ypos >= ap.height * ap.stride)
                throw new Exception("Atlas buffer overflow.");

            // Reset column and max height
            ap.xpos = 0;
            ap.maxRowHeight = 0;
        }

        #endregion

    }

}
