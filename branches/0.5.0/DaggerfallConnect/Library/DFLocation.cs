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
    /// Stores information about locations, such as cities and dungeons.
    /// </summary>
    public struct DFLocation
    {
        #region Structure Variables

        /// <summary>
        /// Name of this location.
        /// </summary>
        public string Name;

        /// <summary>
        /// True if location has a dungeon, otherwise false.
        /// </summary>
        public bool HasDungeon;

        /// <summary>
        /// RegionMapTable data for this location.
        /// </summary>
        public DFRegion.RegionMapTable MapTableData;

        /// <summary>
        /// Exterior location.
        /// </summary>
        public LocationExterior Exterior;

        /// <summary>
        /// Dungeon attached to location (if HasDungeon = true).
        /// </summary>
        public LocationDungeon Dungeon;

        /// <summary>
        /// Climate of this location.
        /// </summary>
        public ClimateBases Climate;

        /// <summary>
        /// Political alignment of this location (equal to region index).
        /// </summary>
        public int Politic;

        #endregion

        #region Climate Enumerations

        /// <summary>
        /// Climate base enumeration for climate-swapping textures.
        /// </summary>
        public enum ClimateBases
        {
            /// <summary>Unknown climate.</summary>
            Unknown = -1,
            /// <summary>Desert climate base.</summary>
            Desert = 0,
            /// <summary>Mountain climate base.</summary>
            Mountain = 100,
            /// <summary>Temperate/Woodland climate base.</summary>
            Temperate = 300,
            /// <summary>Swamp climate base.</summary>
            Swamp = 400,
        }

        /// <summary>
        /// Offsets from climate base to texture set for climate-swapping textures.
        /// </summary>
        private enum ClimateOffsets
        {
            /// <summary>Unknown</summary>
            Unknown = -1,
            /// <summary>Terrain</summary>
            Terrain = 2,
            /// <summary>Ruins</summary>
            Ruins = 7,
            /// <summary>Castle</summary>
            Castle = 9,
            /// <summary>PalaceInt</summary>
            PalaceInt = 11,
            /// <summary>CityA</summary>
            CityA = 12,
            /// <summary>CityB</summary>
            CityB = 14,
            /// <summary>CityInt</summary>
            CityInt = 16,
            /// <summary>CityWalls</summary>
            CityWalls = 17,
            /// <summary>CryptA</summary>
            CryptA = 19,
            /// <summary>CryptB</summary>
            CryptB = 20,
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
            /// <summary>FarmInt</summary>
            FarmInt = 28,
            /// <summary>Fences</summary>
            Fences = 29,
            /// <summary>MagesGuild</summary>
            MagesGuild = 35,
            /// <summary>MagesGuildInt</summary>
            MagesGuildInt = 37,
            /// <summary>Manor</summary>
            Manor = 38,
            /// <summary>ManorInt</summary>
            ManorInt = 40,
            /// <summary>MarbleFloors</summary>
            MarbleFloors = 41,
            /// <summary>MerchantHomes</summary>
            MerchantHomes = 42,
            /// <summary>MerchantHomesInt</summary>
            MerchantHomesInt = 44,
            /// <summary>Mines</summary>
            Mines = 45,
            /// <summary>Misc</summary>
            Misc = 46,
            /// <summary>Caves</summary>
            Caves = 47,
            /// <summary>Paintings</summary>
            Paintings = 48,
            /// <summary>TavernExteriors</summary>
            TavernExteriors = 58,
            /// <summary>TavernInt</summary>
            TavernInt = 60,
            /// <summary>TempleExteriors</summary>
            TempleExteriors = 61,
            /// <summary>TempleInt</summary>
            TempleInt = 63,
            /// <summary>Village</summary>
            Village = 64,
            /// <summary>VillageInt</summary>
            VillageInt = 66,
            /// <summary>Sewer</summary>
            Sewer = 68,
            /// <summary>Roofs</summary>
            Roofs = 69,
            /// <summary>Doors</summary>
            Doors = 74,
        }

        #endregion

        #region Shared Child Structures

        /// <summary>
        /// Initial data common to both dungeons and exterior locations.
        /// </summary>
        public struct LocationRecordElement
        {
            /// <summary>Number of doors in this location.</summary>
            public UInt32 DoorCount;

            /// <summary>Door data array with DoorCount members.</summary>
            public LocationDoorElement[] Doors;

            /// <summary>Header for location data.</summary>
            public LocationRecordElementHeader Header;
        }

        /// <summary>
        /// Describes a door element.
        /// </summary>
        public struct LocationDoorElement
        {
            /// <summary>Index to element within LocationExterior.BuildingData array.
            ///  Or 0xffff when door is not associated with BuildingData array.</summary>
            public UInt16 BuildingDataIndex;

            /// <summary>Always 0.</summary>
            public Byte NullValue;

            /// <summary>Unknown mask;</summary>
            public Byte Mask;

            /// <summary>Unknown.</summary>
            public Byte Unknown1;

            /// <summary>Unknown.</summary>
            public Byte Unknown2;
        }

        /// <summary>
        /// Header used for each location record element.
        /// </summary>
        public struct LocationRecordElementHeader
        {
            /// <summary>Always 1.</summary>
            public UInt32 AlwaysOne1;

            /// <summary>Always 0.</summary>
            public UInt16 NullValue1;

            /// <summary>Always 0.</summary>
            public Byte NullValue2;

            /// <summary>X coordinate for location in world units.</summary>
            public Int32 X;

            /// <summary>Always 0.</summary>
            public UInt32 NullValue3;

            /// <summary>X coordinate for location in world units.</summary>
            public Int32 Y;

            /// <summary>Set to 0x0000 for dungeon interior data, or 0x8000 for location exterior data.</summary>
            public UInt16 IsExterior;

            /// <summary>Always 0.</summary>
            public UInt16 NullValue4;

            /// <summary>Unknown.</summary>
            public UInt32 Unknown1;

            /// <summary>Unknown.</summary>
            public UInt32 Unknown2;

            /// <summary>Always 1.</summary>
            public UInt16 AlwaysOne2;

            /// <summary>LocationID used by the quest subsystem.</summary>
            public UInt16 LocationId;

            /// <summary>Always 0.</summary>
            public UInt32 NullValue5;

            /// <summary>Set to 0x0000 for exterior data, or 0x0001 for interior data.</summary>
            public UInt16 IsInterior;

            /// <summary>Set to 0x0000 when no exterior data present, or ID of exterior location</summary>
            public UInt32 ExteriorLocationId;

            /// <summary>Array of 26 0-value bytes.</summary>
            public Byte[] NullValue6;

            /// <summary>Name of location used when entering it.</summary>
            public String LocationName;

            /// <summary>Array of 9 unknown bytes.</summary>
            public Byte[] Unknown3;
        }

        #endregion

        #region External Child Structures

        /// <summary>
        /// Describes exterior location.
        /// </summary>
        public struct LocationExterior
        {
            /// <summary>Each location is described by a LocationRecordElement.</summary>
            public LocationRecordElement RecordElement;

            /// <summary>The number of building structures present.</summary>
            public UInt16 BuildingCount;

            /// <summary>Unknown.</summary>
            public Byte[] Unknown1;

            /// <summary>Data associated with each building.</summary>
            public BuildingData[] Buildings;

            /// <summary>Exterior map data including layout information.</summary>
            public ExteriorData ExteriorData;
        }

        /// <summary>
        /// Data relating to a building.
        /// </summary>
        public struct BuildingData
        {
            /// <summary>Used to generate building name.</summary>
            public UInt16 NameSeed;

            /// <summary>Always 0.</summary>
            public UInt64 NullValue1;

            /// <summary>Always 0.</summary>
            public UInt64 NullValue2;

            /// <summary>FactionId associated with building, or 0 if no faction.</summary>
            public UInt16 FactionId;

            /// <summary>Generally increases with each building. Otherwise unknown.</summary>
            public Int16 Sector;

            /// <summary>Should always be the same as LocationRecordElementHeader.LocationId.</summary>
            public UInt16 LocationId;

            /// <summary>Type of building.</summary>
            public Byte BuildingType;

            /// <summary>Specifies quality of building from 1-20.</summary>
            public Byte Quality;
        }

        /// <summary>
        /// Layout data for exterior location.
        /// </summary>
        public struct ExteriorData
        {
            /// <summary>Another name for this location. Changing seems to have no effect in game.</summary>
            public String AnotherName;

            /// <summary>This (value and 0x000fffff) matches (MapTable.MapId and 0x000fffff).</summary>
            public Int32 MapId;

            /// <summary>Location ID.</summary>
            public UInt32 LocationId;

            /// <summary>Width of exterior map grid from 1-8.</summary>
            public Byte Width;

            /// <summary>Height of exterior map grid from 1-8.</summary>
            public Byte Height;

            /// <summary>Unknown.</summary>
            public Byte[] Unknown2;

            /// <summary>Only first Width*Height elements will have any meaning.</summary>
            public Byte[] BlockIndex;

            /// <summary>Only first Width*Height elements will have any meaning.</summary>
            public Byte[] BlockNumber;

            /// <summary>Only first Width*Height elements will have any meaning.</summary>
            public Byte[] BlockCharacter;
            
            /// <summary>Unknown.</summary>
            public Byte[] Unknown3;

            /// <summary>Always 0.</summary>
            public UInt64 NullValue1;

            /// <summary>Always 0.</summary>
            public Byte NullValue2;

            /// <summary>Unknown.</summary>
            public UInt32[] Unknown4;

            /// <summary>Always 0.</summary>
            public Byte[] NullValue3;

            /// <summary>Unknown.</summary>
            public UInt32 Unknown5;
        }

        #endregion

        #region Dungeon Child Structures

        /// <summary>
        /// Describes a dungeon map.
        /// </summary>
        public struct LocationDungeon
        {
            /// <summary>Each dungeon is described by a LocationRecordElement.</summary>
            public LocationRecordElement RecordElement;

            /// <summary>Header for dungeon layout.</summary>
            public DungeonHeader Header;

            /// <summary>Layout of dungeon.</summary>
            public DungeonBlock[] Blocks;
        }

        /// <summary>
        /// Header preceding dungeon layout elements.
        /// </summary>
        public struct DungeonHeader
        {
            /// <summary>Always 0.</summary>
            public UInt16 NullValue1;

            /// <summary>Unknown.</summary>
            public UInt32 Unknown1;

            /// <summary>Unknown.</summary>
            public UInt32 Unknown2;

            /// <summary>Count of DungeonBlock elements.</summary>
            public UInt16 BlockCount;

            /// <summary>Unknown.</summary>
            public Byte[] Unknown3;
        }

        /// <summary>
        /// Describes a dungeon block element in a dungeon map layout.
        /// </summary>
        public struct DungeonBlock
        {
            /// <summary>X position of block.</summary>
            public SByte X;
            
            /// <summary>Y position of block.</summary>
            public SByte Z;

            /// <summary>Bitfield containing BlockNumber, start bit, and BlockIndex.</summary>
            public UInt16 BlockNumberStartIndexBitfield;

            /// <summary>BlockNumber read from bitfield.</summary>
            public UInt16 BlockNumber;

            /// <summary>IsStartingBlock read from bitfield.</summary>
            public Boolean IsStartingBlock;

            /// <summary>BlockIndex read from bitfield.</summary>
            public Byte BlockIndex;

            /// <summary>Name of RDB block.</summary>
            public String BlockName;
        }

        #endregion
    }
}
