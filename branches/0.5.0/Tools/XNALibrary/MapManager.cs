// Project:         DaggerfallConnect
// Description:     Read data from Daggerfall's file formats.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/using System;

#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;

#endregion

namespace XNALibrary
{
    /// <summary>
    /// Helper class to load and draw location and dungeon maps.
    /// </summary>
    public class MapManager
    {
        #region Class Variables

        private GraphicsDevice graphicsDevice;
        private BlockManager blockManager;
        private MapsFile mapsFile;
        private DFPalette dfAutomapPalette;
        private Dictionary<string, Map> mapDictionary;

        #endregion

        #region Class Structures

        public struct Map
        {
            public string mapKey;
            public DFLocation dfLocation;
            public int width;
            public int height;
            public string[,] rmbBlocks;
            //public DFBitmap[,] rmbAutoMapBitmaps;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// MeshManager.
        /// </summary>
        public MeshManager MeshManager
        {
            get { return blockManager.MeshManager; }
        }

        /// <summary>
        /// BlockManager.
        /// </summary>
        public BlockManager BlockManager
        {
            get { return blockManager; }
        }

        /// <summary>
        /// TextureManager.
        /// </summary>
        public TextureManager TextureManager
        {
            get { return blockManager.TextureManager; }
        }

        /// <summary>
        /// MapsFile.
        /// </summary>
        public MapsFile MapsFile
        {
            get { return mapsFile; }
        }

        /// <summary>
        /// BlocksFile.
        /// </summary>
        public BlocksFile BlocksFile
        {
            get { return blockManager.BlocksFile; }
        }

        /// <summary>
        /// ImageFileReader.
        /// </summary>
        public ImageFileReader ImageFileReader
        {
            get { return blockManager.ImageFileReader; }
        }

        #endregion

        #region Constructors

        public MapManager(GraphicsDevice device, string arena2Folder)
        {
            // Instantiate blockManager
            blockManager = new BlockManager(device, arena2Folder);

            // Load MAPS.BSA
            mapsFile = new MapsFile(Path.Combine(arena2Folder, "MAPS.BSA"), FileUsage.UseDisk, true);

            // Load automap palette
            dfAutomapPalette = new DFPalette(Path.Combine(arena2Folder, "PAL.PAL"));

            // Create map dictionary 
            mapDictionary = new Dictionary<string, Map>();

            // Store GraphicsDevice for drawing operations
            graphicsDevice = device;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load map by index.
        /// </summary>
        /// <param name="regionIndex">Region index.</param>
        /// <param name="locationIndex">Location index.</param>
        /// <returns>Map key.</returns>
        public string LoadMap(int regionIndex, int locationIndex)
        {
            return LoadMapData(regionIndex, locationIndex);
        }

        /// <summary>
        /// Load map by name.
        /// </summary>
        /// <param name="regionName">Region name.</param>
        /// <param name="locationName">Index name.</param>
        /// <returns>Map key.</returns>
        public string LoadMap(string regionName, string locationName)
        {
            DFRegion dfRegion = mapsFile.GetRegion(regionName);
            return LoadMapData(mapsFile.GetRegionIndex(regionName), dfRegion.MapNameLookup[locationName]);
        }

        /// <summary>
        /// Gets unique map key for specified region and location names.
        /// </summary>
        /// <param name="regionName">Region name.</param>
        /// <param name="locationName">Location name.</param>
        /// <returns>Map key.</returns>
        private string GetMapKey(string regionName, string locationName)
        {
            return string.Format("{0}_{1}", regionName, locationName);
        }

        /// <summary>
        /// Gets unique map key for specified region and location indices.
        /// </summary>
        /// <param name="regionName">Region index.</param>
        /// <param name="locationName">Location index.</param>
        /// <returns>Map key.</returns>
        public string GetMapKey(int regionIndex, int locationIndex)
        {
            DFRegion dfRegion = mapsFile.GetRegion(regionIndex);
            return GetMapKey(mapsFile.GetRegionName(regionIndex), dfRegion.MapNames.LocationNames[locationIndex]);
        }

        /// <summary>
        /// Check map key exists.
        /// </summary>
        /// <param name="mapKey">Map key.</param>
        /// <returns>True if key exists (map loaded), otherwise false.</returns>
        public bool MapKeyExists(string mapKey)
        {
            return mapDictionary.ContainsKey(mapKey);
        }

        /// <summary>
        /// Gets map object. Must already be loaded.
        /// </summary>
        /// <param name="mapKey">Map key.</param>
        /// <returns>Map object.</returns>
        public Map GetMap(string mapKey)
        {
            // Check key exists
            if (!mapDictionary.ContainsKey(mapKey))
                throw new Exception("Invalid key.");

            return mapDictionary[mapKey];
        }

        /// <summary>
        /// Draw map using specified BasicEffect and Matrix.
        /// </summary>
        /// <param name="mapKey">Key of map to draw. Must be loaded.</param>
        /// <param name="effect">BasicEffect to draw with.</param>
        /// <param name="matrix">Matrix to transform with.</param>
        public void DrawMapExterior(string mapKey, ref BasicEffect effect, ref Matrix matrix)
        {
            int blocksDrawn = 0;
            const float blockSide = 4096.0f;
            Map map = mapDictionary[mapKey];
            for (int x = 0; x < map.width; x++)
            {
                for (int y = map.height-1; y >= 0; y--)
                {
                    Matrix translate = Matrix.CreateTranslation(x * blockSide, 0, -(y * blockSide));
                    Matrix newMatrix = translate * matrix;
                    if (blockManager.DrawBlock(map.rmbBlocks[x, y], ref effect, ref newMatrix) == true)
                        blocksDrawn++;
                }
            }

            //Console.WriteLine("Blocks drawn {0}", blocksDrawn);
        }

        public void DrawMapDungeon(string mapKey, ref BasicEffect effect, ref Matrix matrix)
        {
            Map map = mapDictionary[mapKey];
            for (int i = 0; i < map.dfLocation.Dungeon.Blocks.Length; i++)
            {
                DFLocation.DungeonBlock block = map.dfLocation.Dungeon.Blocks[i];

                Matrix translate = Matrix.CreateTranslation(-block.X * 2048, 0, -block.Z * 2048);
                Matrix newMatrix = translate * matrix;
                blockManager.DrawBlock(block.BlockName, ref effect, ref newMatrix);
            }
        }

        ///// <summary>
        ///// Gets location minimap.
        ///// </summary>
        ///// <param name="mapKey">Map key.</param>
        ///// <returns>Bitmap.</returns>
        //public Bitmap GetMinimap(string mapKey)
        //{
        //    // Check key exists
        //    if (!mapDictionary.ContainsKey(mapKey))
        //        throw new Exception("Invalid key.");

        //    Map map = mapDictionary[mapKey];

        //    return new Bitmap(256, 256);
        //}

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads map data for drawing.
        /// </summary>
        /// <param name="regionIndex">Region index.</param>
        /// <param name="locationIndex">Location index.</param>
        /// <returns>Map key.</returns>
        private string LoadMapData(int regionIndex, int locationIndex)
        {
            // Get region. Some regions have zero locations and we just ignore these.
            DFRegion dfRegion = mapsFile.GetRegion(regionIndex);
            if (dfRegion.MapNames.LocationCount == 0)
                return string.Empty;

            // Get names
            string regionName = mapsFile.GetRegionName(regionIndex);
            string locationName = mapsFile.GetRegion(regionIndex).MapNames.LocationNames[locationIndex];

            // Look for existing map data
            string mapKey = GetMapKey(regionName, locationName);
            if (mapDictionary.ContainsKey(mapKey))
                return mapKey;

            // Load location data
            Map map = new Map();
            map.mapKey = mapKey;
            map.dfLocation = mapsFile.GetLocation(regionIndex, locationIndex);
            //map.isExterior = (map.dfLocation.RecordElement.Header.IsExterior == 0x8000) ? true : false;
            map.rmbBlocks = new string[8, 8];
            //map.rmbAutoMapBitmaps = new DFBitmap[8, 8];

            // Build map layout
            BuildRmbLayout(ref map);
            BuildRdbLayout(ref map);

            // Store in dictionary
            mapDictionary.Add(mapKey, map);

            return mapKey;
        }

        /// <summary>
        /// Build layout of RMB blocks for map.
        /// </summary>
        /// <param name="map">Map.</param>
        private void BuildRmbLayout(ref Map map)
        {
            // Get map dimensions
            map.width = map.dfLocation.Exterior.ExteriorData.Width;
            map.height = map.dfLocation.Exterior.ExteriorData.Height;

            // Load required RMB blocks
            for (int y = 0; y < map.height; y++)
            {
                for (int x = 0; x < map.width; x++)
                {
                    // Load block content
                    string blockName = mapsFile.GetRmbBlockName(ref map.dfLocation, x, y);
                    BlockManager.Block block = blockManager.LoadBlock(blockName);

                    // Store block name
                    map.rmbBlocks[x, y] = blockName;

                    // Set automap bitmap
                    //GetAutoMapDFBitmap(ref block.dfBlock, x, y);
                    //map.rmbAutoMapBitmaps[x, y] = GetAutoMapDFBitmap(ref block.dfBlock);
                }
            }
        }

        private void BuildRdbLayout(ref Map map)
        {
            // Load required RDB blocks
            for (int i = 0; i < map.dfLocation.Dungeon.Blocks.Length; i++)
            {
                blockManager.LoadBlock(map.dfLocation.Dungeon.Blocks[i].BlockName);
            }
        }

        /// <summary>
        /// Acquires the automap image as a DFBitmap.
        /// </summary>
        /// <param name="dfBlock">Block object to work with.</param>
        /// <returns>DFBitmap object.</returns>
        private DFBitmap GetAutoMapDFBitmap(ref DFBlock dfBlock, int x, int y)
        {
            // Create DFBitmap
            DFBitmap dfBitmap = new DFBitmap();
            dfBitmap.Width = 64;
            dfBitmap.Height = 64;
            dfBitmap.Stride = 64;
            dfBitmap.Format = DFBitmap.Formats.Indexed;
            dfBitmap.Data = dfBlock.RmbBlock.FldHeader.AutoMapData;

            // Test saving bitmap
            //DFManualImage dfManualImage = new DFManualImage(dfBitmap);
            //dfManualImage.Palette.MakeAutomap();
            //Bitmap managedBitmap = dfManualImage.GetManagedBitmap(0, 0, false, false);
            //string filename = string.Format("C:\\Test\\{0} [{1},{2}].png", dfBlock.Name, x, y);
            //managedBitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Png);

            return dfBitmap;
        }

        #endregion

    }
}
