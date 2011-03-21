// Project:         XNALibrary
// Description:     Simple XNA game library for DaggerfallConnect.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/using System;

#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Drawing;
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

    /// <summary>
    /// Helper class to load and store Daggerfall textures for XNA. This class will pre-load
    ///  textures for each climate set in a different thread at construct time. Each climate
    ///  set is compiled into a texture atlas, reducing state changes for large scenes (whole
    ///  dungeons and cities). It is also possible to load miscellaneous textures for very
    ///  small scenes or when using a climate atlas is not desired.
    /// </summary>
    public class TextureManager
    {

        #region Class Variables

        private GraphicsDevice graphicsDevice;
        private ImageFileReader imageFileReader;

        // Flag raised when thread loading climate textures completed
        bool climatePreLoadCompleted = false;

        // Atlas layout dictionaries for each climate type
        private Dictionary<int, RectangleF> desertDict;
        private Dictionary<int, RectangleF> mountainDict;
        private Dictionary<int, RectangleF> temperateDict;
        private Dictionary<int, RectangleF> swampDict;

        // Atlas params for each climate type
        private AtlasParams desertParams;
        private AtlasParams mountainParams;
        private AtlasParams temperateParams;
        private AtlasParams swampParams;

        // Texture atlas for each climate type
        private Texture2D desertAtlas;
        private Texture2D mountainAtlas;
        private Texture2D temperateAtlas;
        private Texture2D swampAtlas;

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
            public int width;
            public int height;
            public int format;
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
            get { return imageFileReader.Arena2Path; }
        }

        /// <summary>
        /// Gets the GraphicsDevice set at construction.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get { return graphicsDevice; }
        }

        /// <summary>
        /// True if thread preloading climate textures has finished.
        /// </summary>
        public bool ClimatePreLoadCompleted
        {
            get { return climatePreLoadCompleted; }
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
            imageFileReader = new ImageFileReader(arena2Path);
            imageFileReader.AutoDiscard = true;

            // Create dictionaries
            miscTexturesDict = new Dictionary<int, Texture2D>();
            desertDict = new Dictionary<int, RectangleF>();
            mountainDict = new Dictionary<int, RectangleF>();
            temperateDict = new Dictionary<int, RectangleF>();
            swampDict = new Dictionary<int, RectangleF>();

            // Start reading climate textures in another thread
            Thread thread = new Thread(this.ThreadLoadClimateTextures);
            thread.Start();
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

            // Get DF texture in ARGB format so we can just SetData the byte array into XNA
            DFImageFile textureFile = imageFileReader.LoadFile(TextureFile.IndexToFileName(archive));
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

        #region Threading Methods

        private void ThreadLoadClimateTextures()
        {
            // Build texture atlas for each climate type
            long startTime = DateTime.Now.Ticks;
            BuildClimateAtlas(ClimateBases.Desert, out desertParams, out desertAtlas);
            BuildClimateAtlas(ClimateBases.Mountain, out mountainParams, out mountainAtlas);
            BuildClimateAtlas(ClimateBases.Temperate, out temperateParams, out temperateAtlas);
            BuildClimateAtlas(ClimateBases.Swamp, out swampParams, out swampAtlas);
            long totalTime = DateTime.Now.Ticks - startTime;
            climatePreLoadCompleted = true;
#if DEBUG
            Console.WriteLine("Climate texture atlas build completed in {0} milliseconds.", (float)totalTime / 10000.0f);
#endif
        }

        #endregion

        #region Atlas Building

        /// <summary>
        /// Builds atlas of all textures specific to a climate.
        /// </summary>
        /// <param name="climate">Climate type.</param>
        /// <param name="atlasParams">Params to populated by this build.</param>
        /// <param name="atlasTexture">Texture2D out.</param>
        /// <returns>True if successful, otherwise false.</returns>
        private bool BuildClimateAtlas(ClimateBases climate, out AtlasParams atlasParams, out Texture2D atlasTexture)
        {
            // Define size of buffer to hold image data (ARGB format)
            int width = 2048, height = 2048, format = 4;
            int stride = width * format;

            // Init climate pack
            AtlasParams ap = new AtlasParams();
            ap.climate = climate;
            ap.width = width;
            ap.height = height;
            ap.format = 4;
            ap.stride = stride;
            ap.xpos = 0;
            ap.ypos = 0;
            ap.maxRowHeight = 0;
            ap.buffer = new byte[stride * height];


            // Add custom red bitmap to use for lookup errors
            DFManualImage mi = new DFManualImage(64, 64, DFBitmap.Formats.ARGB);
            mi.Clear(0xff, 0xff, 0, 0);
            DFBitmap dfBitmap = mi.DFBitmap;
            AtlasDFBitmap(0, ref dfBitmap, ref ap);


            // Add terrain tiles (have normal, snow, and rain sets)
            AtlasTextureFile(ClimateSets.Terrain, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.Terrain, ClimateWeather.Snow, ref ap);
            AtlasTextureFile(ClimateSets.Terrain, ClimateWeather.Rain, ref ap);


            // Add exterior textures (have normal and snow sets, but not rain)
            AtlasTextureFile(ClimateSets.Ruins, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.Ruins, ClimateWeather.Snow, ref ap);

            AtlasTextureFile(ClimateSets.Castle, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.Castle, ClimateWeather.Snow, ref ap);

            AtlasTextureFile(ClimateSets.CityA, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.CityA, ClimateWeather.Snow, ref ap);

            AtlasTextureFile(ClimateSets.CityB, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.CityB, ClimateWeather.Snow, ref ap);

            AtlasTextureFile(ClimateSets.CityWalls, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.CityWalls, ClimateWeather.Snow, ref ap);

            AtlasTextureFile(ClimateSets.Farm, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.Farm, ClimateWeather.Snow, ref ap);

            AtlasTextureFile(ClimateSets.Fences, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.Fences, ClimateWeather.Snow, ref ap);

            AtlasTextureFile(ClimateSets.MagesGuild, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.MagesGuild, ClimateWeather.Snow, ref ap);

            AtlasTextureFile(ClimateSets.Manor, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.Manor, ClimateWeather.Snow, ref ap);

            AtlasTextureFile(ClimateSets.MerchantHomes, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.MerchantHomes, ClimateWeather.Snow, ref ap);

            AtlasTextureFile(ClimateSets.TavernExteriors, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.TavernExteriors, ClimateWeather.Snow, ref ap);

            AtlasTextureFile(ClimateSets.TempleExteriors, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.TempleExteriors, ClimateWeather.Snow, ref ap);

            AtlasTextureFile(ClimateSets.Village, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.Village, ClimateWeather.Snow, ref ap);

            AtlasTextureFile(ClimateSets.Roofs, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.Roofs, ClimateWeather.Snow, ref ap);


            // Add interior textures (do not have snow or rain sets)
            AtlasTextureFile(ClimateSets.PalaceInt, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.CityInt, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.CryptA, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.CryptB, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.DungeonsA, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.DungeonsB, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.DungeonsC, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.DungeonsNEWCs, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.FarmInt, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.MagesGuildInt, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.ManorInt, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.MarbleFloors, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.MerchantHomesInt, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.Mines, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.Caves, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.Paintings, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.TavernInt, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.TempleInt, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.VillageInt, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.Sewer, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.Doors, ClimateWeather.Normal, ref ap);


            // Create texture from atlas buffer
            atlasTexture = new Texture2D(graphicsDevice, width, height, 0, TextureUsage.AutoGenerateMipMap, SurfaceFormat.Color);
            atlasTexture.SetData<byte>(ap.buffer);

            // Store params so we can keep adding textures to end of atlas later
            atlasParams = ap;

            // TEST: Save texture for review
            //string filename = string.Format("C:\\test\\{0}.png", climate.ToString());
            //atlasTexture.Save(filename, ImageFileFormat.Png);

            return true;
        }

        private void AtlasTextureFile(ClimateSets set, ClimateWeather weather, ref AtlasParams ap)
        {
            // Resolve climate set and weather to filename
            string filename = ImageFileReader.GetClimateTextureFileName(ap.climate, set, weather);
            AtlasTextureFile(set, weather, ref filename, ref ap);
        }

        private void AtlasTextureFile(ClimateSets set, ClimateWeather weather, ref string filename, ref AtlasParams ap)
        {
            // Load texture file
            DFImageFile imageFile = imageFileReader.LoadFile(filename);
            if (imageFile == null)
            {
#if DEBUG
                Console.WriteLine("Image file `{0}` does not exist.", filename);
#endif
                return;
            }

            // Add each record to atlas (not supporting animation yet)
            for (int r = 0; r < imageFile.RecordCount; r++)
            {
                // Get record bitmap in ARGB format
                DFBitmap dfBitmap = imageFile.GetBitmapFormat(r, 0, 0, DFBitmap.Formats.ARGB);

                // Add bitmap
                int key = TextureKey(set, weather, r);
                AtlasDFBitmap(key, ref dfBitmap, ref ap);
            }
        }

        private void AtlasDFBitmap(int key, ref DFBitmap dfBitmap, ref AtlasParams ap)
        {
            // Check key is not already in dictionary
            if (TextureExists(ap.climate, key))
                throw new Exception("Texture key already exists.");

            // For now can only handle width <= 64 pixels
            int stepx = 64;
            if (dfBitmap.Width > 64)
            {
#if DEBUG
                Console.WriteLine("Width > 64.");
#endif
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
#if DEBUG
                Console.WriteLine("Height > 128.");
#endif
                return;
            }

            // Track max height per row
            if (stepy > ap.maxRowHeight)
                ap.maxRowHeight = stepy;

            // Handle row wrap
            if (ap.xpos + dfBitmap.Stride > ap.stride)
            {
                AtlasNewRow(ref ap);
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
            AddTexture(ap.climate, key, ref textureRect);
        }

        private void AtlasNewRow(ref AtlasParams ap)
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

        #endregion

    }

}
