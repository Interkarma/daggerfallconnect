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
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;

#endregion

namespace XNALibrary
{

    // Use climate enums locally
    using ClimateBases = DFLocation.ClimateBases;
    using ClimateSets = DFLocation.ClimateSets;
    using ClimateWeather = DFLocation.ClimateWeather;

    /// <summary>
    /// Helper class to load and store Daggerfall textures for XNA.
    /// </summary>
    public class TextureManager
    {

        #region Class Variables

        private GraphicsDevice graphicsDevice;
        private ImageFileReader imageFileReader;

        // Flag raised when thread loading climate textures completed
        bool threadLoadCompleted = false;

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

        #endregion

        #region Properties

        /// <summary>
        /// Gets ImageFileReader.
        /// </summary>
        public ImageFileReader ImageFileReader
        {
            get { return imageFileReader; }
        }

        /// <summary>
        /// True if thread preloading textures has finished.
        /// </summary>
        public bool ThreadLoadCompleted
        {
            get { return threadLoadCompleted; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="device">Graphics Device.</param>
        /// <param name="arena2Folder">Path to Arena2 folder.</param>
        public TextureManager(GraphicsDevice device, string arena2Folder)
        {
            // Setup
            graphicsDevice = device;
            imageFileReader = new ImageFileReader(arena2Folder);

            // Start reading climate textures in another thread
            Thread thread = new Thread(this.ThreadLoadTextures);
            thread.Start();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get unique key for specified texture.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <param name="frame">Frame index.</param>
        /// <returns>Texture key.</returns>
        //public int GetTextureKey(int archive, int record, int frame)
        //{
        //    return (archive * 10000) + (record * 100) + frame;
        //}

        /// <summary>
        /// Loads texture based on index.
        /// </summary>
        /// <param name="archive">Archive index.</param>
        /// <param name="record">Record index.</param>
        /// <param name="frame">Frame index.</param>
        /// <returns>Texture key.</returns>
        //public int LoadTexture(int archive, int record, int frame)
        //{
        //    // Just return key if already in dictionary
        //    int textureKey = GetTextureKey(archive, record, frame);
        //    if (textureDictionary.ContainsKey(textureKey))
        //        return textureKey;

        //    // Get DF texture in ARGB format so we can just SetData the byte array into XNA
        //    DFImageFile textureFile = imageFileReader.LoadFile(TextureFile.IndexToFileName(archive));
        //    DFBitmap dfbitmap = textureFile.GetBitmapFormat(record, frame, 0, DFBitmap.Formats.ARGB);

        //    // Create XNA texture
        //    Texture2D texture = new Texture2D(graphicsDevice, dfbitmap.Width, dfbitmap.Height, 0, TextureUsage.AutoGenerateMipMap, SurfaceFormat.Color);
        //    texture.SetData<byte>(dfbitmap.Data);

        //    // Store texture in dictionary
        //    textureDictionary.Add(textureKey, texture);

        //    return textureKey;
        //}

        /// <summary>
        /// Get texture based on key. The manager will return NULL if texture does not exist.
        /// </summary>
        /// <param name="textureKey"></param>
        /// <returns></returns>
        //public Texture2D GetTexture(int textureKey)
        //{
        //    if (!textureDictionary.ContainsKey(textureKey))
        //        return null;
        //    else
        //        return textureDictionary[textureKey];
        //}

        #endregion

        #region Threading Methods

        private void ThreadLoadTextures()
        {
            // Build texture atlas for each climate type
            long startTime = DateTime.Now.Ticks;
            BuildClimateAtlas(ClimateBases.Desert, out desertParams, out desertAtlas);
            BuildClimateAtlas(ClimateBases.Mountain, out mountainParams, out mountainAtlas);
            BuildClimateAtlas(ClimateBases.Temperate, out temperateParams, out temperateAtlas);
            BuildClimateAtlas(ClimateBases.Swamp, out swampParams, out swampAtlas);
            long totalTime = DateTime.Now.Ticks - startTime;
            threadLoadCompleted = true;
#if DEBUG
            Console.WriteLine("Texture build thread completed in {0} milliseconds.", (float)totalTime / 10000.0f);
#endif
        }

        #endregion

        #region Atlas Building

        /// <summary>
        /// Parameters of atlas during build.
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
            AtlasDFBitmap(ref dfBitmap, ref ap);


            // Add terrain tiles (have normal, snow, and rain sets)
            AtlasTextureFile(ClimateSets.Terrain, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.Terrain, ClimateWeather.Snow, ref ap);
            AtlasTextureFile(ClimateSets.Terrain, ClimateWeather.Rain, ref ap);


            // Add exterior textures (have normal and snow sets, but not rain)
            AtlasTextureFile(ClimateSets.Ruins, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.Ruins, ClimateWeather.Snow, ref ap);

            AtlasTextureFile(ClimateSets.Castle, ClimateWeather.Normal, ref ap);
            AtlasTextureFile(ClimateSets.Castle, ClimateWeather.Snow, ref ap);

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
            AtlasTextureFile(ClimateSets.PalaceInt, ClimateWeather.Normal, ref ap);
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
            string filename = imageFileReader.GetClimateTextureFileName(ap.climate, set, weather);
            AtlasTextureFile(ref filename, ref ap);
        }

        private void AtlasTextureFile(ref string filename, ref AtlasParams ap)
        {
            // Load texture file
            DFImageFile imageFile = imageFileReader.LoadFile(filename);
            if (imageFile == null)
            {
                Console.WriteLine("Image file `{0}` does not exist.", filename);
                return;
            }

            // Add each record to atlas (not supporting animation yet)
            for (int r = 0; r < imageFile.RecordCount; r++)
            {
                // Get record bitmap in ARGB format
                DFBitmap dfBitmap = imageFile.GetBitmapFormat(r, 0, 0, DFBitmap.Formats.ARGB);

                // Add bitmap
                AtlasDFBitmap(ref dfBitmap, ref ap);
            }
        }

        private void AtlasDFBitmap(ref DFBitmap dfBitmap, ref AtlasParams ap)
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

    }

}
