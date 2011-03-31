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
    using ClimateBases = DFLocation.ClimateBases;
    using ClimateSets = DFLocation.ClimateSets;
    using ClimateWeather = DFLocation.ClimateWeather;

    // Differentiate between Color types
    using GDIColor = System.Drawing.Color;
    using XNAColor = Microsoft.Xna.Framework.Graphics.Color;

    /// <summary>
    /// Helper class to load and store Daggerfall textures for XNA. Textures can be stored in
    ///  a texture atlas (one per climate type) for large scenes or as individual textures for
    ///  small scenes.
    /// </summary>
    public class TextureManager
    {

        #region Class Variables

        // Class
        private string arena2Path;
        private GraphicsDevice graphicsDevice;
        private TextureFile textureFile;

        // Atlas setup
        private const int atlasWidth = 1024;
        private const int atlasHeight = 1024;

        // Atlas for each texture group
        private Texture2D terrainAtlas;
        private Texture2D exteriorAtlas;
        private Texture2D interiorAtlas;

        // Atlas layout dictionaries for each texture group
        private Dictionary<int, RectangleF> terrainAtlasDict;
        private Dictionary<int, RectangleF> interiorAtlasDict;
        private Dictionary<int, RectangleF> exteriorAtlasDict;

        // Dictionary for misc textures
        private Dictionary<int, Texture2D> miscTexturesDict;

        #endregion

        #region Class Structures

        /// <summary>
        /// Parameters of climate atlas during build.
        /// </summary>
        private struct AtlasParams
        {
            public ClimateBases climate;
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

            // Create empty climate dictionaries
            miscTexturesDict = new Dictionary<int, Texture2D>();
            terrainAtlasDict = new Dictionary<int, RectangleF>();
            exteriorAtlasDict = new Dictionary<int, RectangleF>();
            interiorAtlasDict = new Dictionary<int, RectangleF>();

            // Load default climate atlas
            LoadClimate(ClimateBases.Temperate);
        }

        #endregion

        #region Misc Textures

        /// <summary>
        /// Get unique key for misc texture.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <param name="frame">Frame index.</param>
        /// <returns>Misc texture key.</returns>
        public int GetMiscTextureKey(int archive, int record, int frame)
        {
            return (archive * 10000) + (record * 100) + frame;
        }

        /// <summary>
        /// Loads a misc texture based on indices.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <param name="frame">Frame index.</param>
        /// <returns>Misc texture key.</returns>
        public int LoadMiscTexture(int archive, int record, int frame)
        {
            // Just return key if already in dictionary
            int key = GetMiscTextureKey(archive, record, frame);
            if (miscTexturesDict.ContainsKey(key))
                return key;

            // Load texture file
            textureFile.Load(Path.Combine(arena2Path, TextureFile.IndexToFileName(archive)), FileUsage.UseDisk, true);

            // Get DF texture in ARGB format so we can just SetData the byte array into XNA
            DFBitmap dfbitmap = textureFile.GetBitmapFormat(record, frame, 0, DFBitmap.Formats.ARGB);

            // Create XNA texture
            Texture2D texture = new Texture2D(graphicsDevice, dfbitmap.Width, dfbitmap.Height, 0, TextureUsage.AutoGenerateMipMap, SurfaceFormat.Color);
            texture.SetData<byte>(dfbitmap.Data);

            // Store texture in dictionary
            miscTexturesDict.Add(key, texture);

            return key;
        }

        /// <summary>
        /// Get misc texture based on key. The manager will return NULL if texture does not exist.
        /// </summary>
        /// <param name="key">Misc texture key.</param>
        /// <returns>Texture2D.</returns>
        public Texture2D GetMiscTexture(int key)
        {
            if (!miscTexturesDict.ContainsKey(key))
                return null;
            else
                return miscTexturesDict[key];
        }

        /// <summary>
        /// Removes misc texture based on key.
        /// </summary>
        /// <param name="key"></param>
        public void RemoveMiscTexture(int key)
        {
            if (miscTexturesDict.ContainsKey(key))
                miscTexturesDict.Remove(key);
        }

        /// <summary>
        /// Clear all misc textures.
        /// </summary>
        public void ClearMiscTextures()
        {
            miscTexturesDict.Clear();
        }

        #endregion

        #region Public Methods

        public void LoadClimate(ClimateBases climate)
        {
            // Build texture atlas for each climate type
            long startTime = DateTime.Now.Ticks;
            BuildTerrainAtlas(climate);
            BuildExteriorAtlas(climate);
            BuildInteriorAtlas(climate);
            long totalTime = DateTime.Now.Ticks - startTime;
            Console.WriteLine("Climate texture atlas build completed in {0} milliseconds.", (float)totalTime / 10000.0f);
        }

        #endregion

        #region Atlas Building

        private bool BuildTerrainAtlas(ClimateBases climate)
        {
            // Init working parameters
            AtlasParams ap = new AtlasParams();
            ap.climate = climate;
            ap.format = 4;
            ap.width = atlasWidth;
            ap.height = atlasHeight;
            ap.stride = atlasWidth * ap.format;
            ap.xpos = 0;
            ap.ypos = 0;
            ap.maxRowHeight = 0;
            ap.buffer = new byte[(atlasWidth * atlasHeight) * 4];

            // Add terrain texture files for this climate
            AddErrorTexture(ref ap);
            AddTextureFile(ClimateSets.Terrain, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.Terrain, ClimateWeather.Winter, ref ap);
            AddTextureFile(ClimateSets.Terrain, ClimateWeather.Rain, ref ap);

            // Create texture from atlas buffer
            terrainAtlas = new Texture2D(graphicsDevice, atlasWidth, atlasHeight, 0, TextureUsage.AutoGenerateMipMap, SurfaceFormat.Color);
            terrainAtlas.SetData<byte>(ap.buffer);

            // TEST: Save texture for review
            //terrainAtlas.Save("C:\\test\\Terrain.png", ImageFileFormat.Png);

            return true;
        }

        private bool BuildExteriorAtlas(ClimateBases climate)
        {
            // Init working parameters
            AtlasParams ap = new AtlasParams();
            ap.climate = climate;
            ap.format = 4;
            ap.width = atlasWidth;
            ap.height = atlasHeight;
            ap.stride = atlasWidth * ap.format;
            ap.xpos = 0;
            ap.ypos = 0;
            ap.maxRowHeight = 0;
            ap.buffer = new byte[(atlasWidth * atlasHeight) * ap.format];

            // Add exterior textures (have normal and winter sets, but not rain)
            AddErrorTexture(ref ap);
            AddTextureFile(ClimateSets.Ruins, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.Ruins, ClimateWeather.Winter, ref ap);
            AddTextureFile(ClimateSets.Castle, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.Castle, ClimateWeather.Winter, ref ap);
            AddTextureFile(ClimateSets.CityA, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.CityA, ClimateWeather.Winter, ref ap);
            AddTextureFile(ClimateSets.CityB, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.CityB, ClimateWeather.Winter, ref ap);
            AddTextureFile(ClimateSets.CityWalls, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.CityWalls, ClimateWeather.Winter, ref ap);
            AddTextureFile(ClimateSets.Farm, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.Farm, ClimateWeather.Winter, ref ap);
            AddTextureFile(ClimateSets.Fences, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.Fences, ClimateWeather.Winter, ref ap);
            AddTextureFile(ClimateSets.MagesGuild, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.MagesGuild, ClimateWeather.Winter, ref ap);
            AddTextureFile(ClimateSets.Manor, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.Manor, ClimateWeather.Winter, ref ap);
            AddTextureFile(ClimateSets.MerchantHomes, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.MerchantHomes, ClimateWeather.Winter, ref ap);
            AddTextureFile(ClimateSets.TavernExteriors, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.TavernExteriors, ClimateWeather.Winter, ref ap);
            AddTextureFile(ClimateSets.TempleExteriors, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.TempleExteriors, ClimateWeather.Winter, ref ap);
            AddTextureFile(ClimateSets.Village, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.Village, ClimateWeather.Winter, ref ap);
            AddTextureFile(ClimateSets.Roofs, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.Roofs, ClimateWeather.Winter, ref ap);

            // Create texture from atlas buffer
            exteriorAtlas = new Texture2D(graphicsDevice, atlasWidth, atlasHeight, 0, TextureUsage.AutoGenerateMipMap, SurfaceFormat.Color);
            exteriorAtlas.SetData<byte>(ap.buffer);

            // TEST: Save texture for review
            //exteriorAtlas.Save("C:\\test\\Exterior.png", ImageFileFormat.Png);

            return true;
        }

        private bool BuildInteriorAtlas(ClimateBases climate)
        {
            // Init working parameters
            AtlasParams ap = new AtlasParams();
            ap.climate = climate;
            ap.format = 4;
            ap.width = atlasWidth;
            ap.height = atlasHeight;
            ap.stride = atlasWidth * ap.format;
            ap.xpos = 0;
            ap.ypos = 0;
            ap.maxRowHeight = 0;
            ap.buffer = new byte[(atlasWidth * atlasHeight) * ap.format];

            // Add interior textures (do not have winter or rain sets)
            AddErrorTexture(ref ap);
            AddTextureFile(ClimateSets.PalaceInt, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.CityInt, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.CryptA, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.CryptB, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.DungeonsA, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.DungeonsB, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.DungeonsC, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.DungeonsNEWCs, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.FarmInt, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.MagesGuildInt, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.ManorInt, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.MarbleFloors, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.MerchantHomesInt, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.Mines, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.Caves, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.Paintings, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.TavernInt, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.TempleInt, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.VillageInt, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.Sewer, ClimateWeather.Normal, ref ap);
            AddTextureFile(ClimateSets.Doors, ClimateWeather.Normal, ref ap);

            // Create texture from atlas buffer
            interiorAtlas = new Texture2D(graphicsDevice, atlasWidth, atlasHeight, 0, TextureUsage.AutoGenerateMipMap, SurfaceFormat.Color);
            interiorAtlas.SetData<byte>(ap.buffer);

            // TEST: Save texture for review
            //interiorAtlas.Save("C:\\test\\Interior.png", ImageFileFormat.Png);

            return true;
        }

        private void AddErrorTexture(ref AtlasParams ap)
        {
            DFManualImage errorImage = new DFManualImage(64, 64, DFBitmap.Formats.ARGB);
            errorImage.Clear(0xff, 0xff, 0, 0);
            DFBitmap dfBitmap = errorImage.DFBitmap;
            AddDFBitmap(0, ref dfBitmap, ref ap);
        }

        private void AddTextureFile(ClimateSets set, ClimateWeather weather, ref AtlasParams ap)
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

                // Add bitmap
                int key = TextureKey(set, weather, r);
                AddDFBitmap(key, ref dfBitmap, ref ap);
            }
        }

        private void AddDFBitmap(int key, ref DFBitmap dfBitmap, ref AtlasParams ap)
        {
            // For now can only handle width <= 64 pixels
            int stepx = 64;
            if (dfBitmap.Width > 64)
            {
                Console.WriteLine("Width > 64.");
                return;
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
                return;
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

            // Increment xpos
            ap.xpos += stepx * ap.format;

            // Create texture layout
            RectangleF textureRect = new RectangleF(
                (float)(ap.xpos - dfBitmap.Width * ap.format) / (float)ap.stride,
                (float)ap.ypos / (float)ap.height,
                (float)(dfBitmap.Width) / (float)ap.stride,
                (float)(dfBitmap.Height) / (float)ap.height);

            // Add key to dictionary
            //AddTexture(ap.climate, key, ref textureRect);
        }

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

        #region Atlas Dictionary Management

        private int TextureKey(ClimateSets set, ClimateWeather weather, int record)
        {
            return (int)set * 1000 + (int)weather * 100 + record;
        }

        /*
        private bool TextureExists(ClimateBases climate, int key)
        {
            switch (climate)
            {
                case ClimateBases.Desert:
                    return desertDict.ContainsKey(key);
                case ClimateBases.Mountain:
                    return mountainDict.ContainsKey(key);
                case ClimateBases.Temperate:
                    return temperateDict.ContainsKey(key);
                case ClimateBases.Swamp:
                    return swampDict.ContainsKey(key);
                default:
                    throw new Exception("Invalid climate.");
            }
        }

        private void AddTexture(ClimateBases climate, int key, ref RectangleF texture)
        {
            switch (climate)
            {
                case ClimateBases.Desert:
                    desertDict.Add(key, texture);
                    break;
                case ClimateBases.Mountain:
                    mountainDict.Add(key, texture);
                    break;
                case ClimateBases.Temperate:
                    temperateDict.Add(key, texture);
                    break;
                case ClimateBases.Swamp:
                    swampDict.Add(key, texture);
                    break;
                default:
                    throw new Exception("Invalid climate.");
            }
        }
        */

        #endregion

    }

}
