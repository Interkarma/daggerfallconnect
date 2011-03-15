// Project:         DaggerfallConnect
// Description:     Read data from Daggerfall's file formats.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Imports

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace DaggerfallConnect
{
    /// <summary>
    /// Terrain base enumeration for regional texture swaps.
    /// </summary>
    public enum TerrainBases
    {
        /// <summary>Desert terrain base.</summary>
        Desert = 0,
        /// <summary>Mountain terrain base.</summary>
        Mountain = 100,
        /// <summary>Temperate terrain base.</summary>
        Temperate = 300,
        /// <summary>Swamp terrain base.</summary>
        Swamp = 400,
    }

    /// <summary>
    /// Weather base enumeration for regional texture swaps.
    /// </summary>
    public enum WeatherBases
    {
        /// <summary>Sunny weather base.</summary>
        Sunny = 0,
        /// <summary>Winter weather base.</summary>
        Winter = 1,
        /// <summary>Rain weather base.</summary>
        Rain = 2,
    }

    /// <summary>
    /// These are the building types used for the automap (as value+1), quest subsystem, and
    ///  location building database.
    /// </summary>
    public enum BuildingTypes
    {
        Alchemist = 0x00,
        HouseForSale = 0x01,
        Amorer = 0x02,
        Bank = 0x03,
        Town4 = 0x04,
        Bookseller = 0x05,
        ClothingStore = 0x06,
        FurnitureStore = 0x07,
        GemStore = 0x08,
        GeneralStore = 0x09,
        Library = 0x0a,
        Guildhall = 0x0b,
        PawnShop = 0x0c,
        WeaponSmith = 0x0d,
        Temple = 0x0e,
        Tavern = 0x0f,
        Palace = 0x10,
        House1 = 0x11,
        House2 = 0x12,
        House3 = 0x13,
        House4 = 0x14,
        House5 = 0x15,
        House6 = 0x16,
        Town23 = 0x17,
        Ship = 0x18,
        Special1 = 0x74,
        Special2 = 0xdf,
        Special3 = 0xf9,
        Special4 = 0xfa,
    }

    /// <summary>
    /// Helper class to read all world map information. This includes political alignment, climate data,
    ///  heightmaps, regional texture calculations, landscape texture calculations, and town and dungeon layouts.
    /// </summary>
    public class MapReader
    {
        #region Class Variables
        #endregion

        #region Class Structures

        /// <summary>
        /// Offsets from base to specified texture set.
        /// </summary>
        private enum TextureOffsets
        {
            /// <summary>Terrain</summary>
            Terrain = 2,
            /// <summary>Ruins</summary>
            Ruins = 7,
            /// <summary>Castle</summary>
            Castle = 9,
            /// <summary>CityA</summary>
            CityA = 12,
            /// <summary>CityB</summary>
            CityB = 14,
            /// <summary>CityWalls</summary>
            CityWalls = 17,
            /// <summary>DungeonsA</summary>
            DungeonsA = 22,
            /// <summary>DungeonsB</summary>
            DungeonsB = 23,
            /// <summary>DungeonsC</summary>
            DungeonsC = 24,
            /// <summary>DungeonsNEWCs</summary>
            DungeonsNEWCs = 25,
            /// <summary>Farm</summary>
            Farm = 26,
            /// <summary>Fences</summary>
            Fences = 29,
            /// <summary>MagesGuild</summary>
            MagesGuild = 35,
            /// <summary>Manor</summary>
            Manor = 38,
            /// <summary>MerchantHomes</summary>
            MerchantHomes = 42,
            /// <summary>TavernExteriors</summary>
            TavernExteriors = 58,
            /// <summary>TempleExteriors</summary>
            TempleExteriors = 61,
            /// <summary>Village</summary>
            Village = 64,
            /// <summary>Roofs</summary>
            Roofs = 69,
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MapReader()
        {
        }

        #endregion

        #region Public Properties
        #endregion

        #region Public Methods

        /// <summary>
        /// Gets filename of the texture archive for each base terrain type.
        /// </summary>
        /// <param name="Type">Terrain type.</param>
        /// <returns>File name of texture archive for specified terrain type.</returns>
        public string GetTerrainFileName(TerrainBases Type)
        {
            switch (Type)
            {
                case TerrainBases.Desert:
                    return "TEXTURE.002";
                case TerrainBases.Mountain:
                    return "TEXTURE.102";
                case TerrainBases.Temperate:
                    return "TEXTURE.302";
                case TerrainBases.Swamp:
                    return "TEXTURE.402";
                default:
                    return string.Empty;
            }
        }

        #endregion

        #region Private Methods
        #endregion

        #region Unused For Now

        // This code has not been checked for correctness.
        // It will be moved and reworked down the track.

        /*
        /// <summary>
        /// Modify a texture archive based on region and weather combinations. Only textures that support terrain changing
        ///  will be returned modified. Otherwise, sourceArchive will be returned unchanged.
        /// </summary>
        /// <param name="SourceArchive">The source archive to modify.</param>
        /// <param name="TerrainBase">Desired terrain base.</param>
        /// <param name="WeatherBase">Desired weather base.</param>
        /// <returns>Texture archive index modified to specified terrain and weather base.</returns>
        private int GetRegionalTextureArchive(int SourceArchive, TerrainBases TerrainBase, WeatherBases WeatherBase)
        {
            // Handle no terrain base
            if (TerrainBase == TerrainBases.None)
                return SourceArchive;

            // Get base and offset for this archive
            int baseTexture = SourceArchive / 100;
            int textureOffset = SourceArchive - (baseTexture * 100);
            int terrainOffset = (int)TerrainBase;
            int weatherOffset = (int)WeatherBase;

            // Process supported offsets
            TextureOffsets sto = (TextureOffsets)textureOffset;
            int result = SourceArchive;
            switch (sto)
            {
                // Case with weather offsets
                case TextureOffsets.Terrain:
                case TextureOffsets.Ruins:
                case TextureOffsets.Castle:
                case TextureOffsets.CityA:
                case TextureOffsets.CityB:
                case TextureOffsets.CityWalls:
                case TextureOffsets.Farm:
                case TextureOffsets.Fences:
                case TextureOffsets.MagesGuild:
                case TextureOffsets.Manor:
                case TextureOffsets.MerchantHomes:
                case TextureOffsets.TavernExteriors:
                case TextureOffsets.TempleExteriors:
                case TextureOffsets.Village:
                case TextureOffsets.Roofs:
                    result = terrainOffset + textureOffset + weatherOffset;
                    break;

                // Cases without weather offsets
                case TextureOffsets.DungeonsA:
                case TextureOffsets.DungeonsB:
                case TextureOffsets.DungeonsC:
                case TextureOffsets.DungeonsNEWCs:
                    result = terrainOffset + textureOffset;
                    break;

                default:
                    break;
            };

            return result;
        }

        /// <summary>
        /// Obtain the correct scenery texture archive (plants, rocks, etc.) for use in the specified climate.
        /// </summary>
        /// <param name="Climate">The climate value. Must be between in the range 223-232.</param>
        /// <returns>The climate texture archive if successful, otherwise -1.</returns>
        private int GetClimateSceneryArchive(int Climate)
        {
            switch (Climate)
            {
                case 223:
                    return 502;
                case 224:
                    return 503;
                case 225:
                    return 503;
                case 226:
                    return 510;
                case 227:
                    return 500;
                case 228:
                    return 502;
                case 229:
                    return 501;
                case 230:
                    return 504;
                case 231:
                    return 504;
                case 232:
                    return 508;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Obtain the correct base texture archive for the specified climate. This texture base is used to
        ///  determine which series of textures are used in different regions. For example, the 100 series
        ///  textures is Mountainous, the 300 series is Temperate.
        /// </summary>
        /// <param name="Climate">The climate value. Must be between in the range 223-232.</param>
        /// <returns>The terrain texture archive if successful, otherwise -1.</returns>
        private int GetClimateTerrainBase(int Climate)
        {
            switch (Climate)
            {
                case 223:
                    return 400;
                case 224:
                    return 0;
                case 225:
                    return 0;
                case 226:
                    return 100;
                case 227:
                    return 400;
                case 228:
                    return 400;
                case 229:
                    return 0;
                case 230:
                    return 300;
                case 231:
                    return 300;
                case 232:
                    return 300;
                default:
                    return -1;
            }
        }
        */

        #endregion
    }
}
