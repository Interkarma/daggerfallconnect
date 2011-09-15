// Project:         DaggerfallConnect
// Description:     Read data from Daggerfall's file formats.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
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
        /// Climate of this location (range is 223-232).
        /// </summary>
        public int Climate;

        /// <summary>
        /// Political alignment of this location (equal to region index + 128).
        /// </summary>
        public int Politic;

        #endregion

        #region Climate Enumerations

        /// <summary>
        /// Climate type enumeration for climate-swapping textures.
        /// </summary>
        public enum ClimateType
        {
            /// <summary>No climate type.</summary>
            None = -1,
            /// <summary>Desert climate type.</summary>
            Desert = 0,
            /// <summary>Mountain climate type.</summary>
            Mountain = 100,
            /// <summary>Temperate/Woodland climate type.</summary>
            Temperate = 300,
            /// <summary>Swamp climate type.</summary>
            Swamp = 400,
        }

        /// <summary>
        /// Climate texture sets for climate-swapping textures.
        /// </summary>
        public enum ClimateSet
        {
            //
            // General sets
            //
            /// <summary>None.</summary>
            None = -1,
            /// <summary>Misc.</summary>
            Misc = 46,
            //
            // Exterior sets
            //
            /// <summary>Terrain.</summary>
            Exterior_Terrain = 2,
            /// <summary>Ruins.</summary>
            Exterior_Ruins = 7,
            /// <summary>Castle.</summary>
            Exterior_Castle = 9,
            /// <summary>CityA.</summary>
            Exterior_CityA = 12,
            /// <summary>CityB.</summary>
            Exterior_CityB = 14,
            /// <summary>CityWalls.</summary>
            Exterior_CityWalls = 17,
            /// <summary>Farm.</summary>
            Exterior_Farm = 26,
            /// <summary>Fences.</summary>
            Exterior_Fences = 29,
            /// <summary>MagesGuild.</summary>
            Exterior_MagesGuild = 35,
            /// <summary>Manor.</summary>
            Exterior_Manor = 38,
            /// <summary>MerchantHomes.</summary>
            Exterior_MerchantHomes = 42,
            /// <summary>TavernExteriors.</summary>
            Exterior_TavernExteriors = 58,
            /// <summary>TempleExteriors.</summary>
            Exterior_TempleExteriors = 61,
            /// <summary>Village.</summary>
            Exterior_Village = 64,
            /// <summary>Roofs.</summary>
            Exterior_Roofs = 69,
            //
            // Interior sets
            //
            /// <summary>PalaceInt.</summary>
            Interior_PalaceInt = 11,
            /// <summary>CityInt.</summary>
            Interior_CityInt = 16,
            /// <summary>CryptA.</summary>
            Interior_CryptA = 19,
            /// <summary>CryptB.</summary>
            Interior_CryptB = 20,
            /// <summary>DungeonsA.</summary>
            Interior_DungeonsA = 22,
            /// <summary>DungeonsB.</summary>
            Interior_DungeonsB = 23,
            /// <summary>DungeonsC.</summary>
            Interior_DungeonsC = 24,
            /// <summary>DungeonsNEWCs.</summary>
            Interior_DungeonsNEWCs = 25,
            /// <summary>FarmInt.</summary>
            Interior_FarmInt = 28,
            /// <summary>MagesGuildInt.</summary>
            Interior_MagesGuildInt = 37,
            /// <summary>ManorInt.</summary>
            Interior_ManorInt = 40,
            /// <summary>MarbleFloors.</summary>
            Interior_MarbleFloors = 41,
            /// <summary>MerchantHomesInt.</summary>
            Interior_MerchantHomesInt = 44,
            /// <summary>Mines.</summary>
            Interior_Mines = 45,
            /// <summary>Caves.</summary>
            Interior_Caves = 47,
            /// <summary>Paintings.</summary>
            Interior_Paintings = 48,
            /// <summary>TavernInt.</summary>
            Interior_TavernInt = 60,
            /// <summary>TempleInt.</summary>
            Interior_TempleInt = 63,
            /// <summary>VillageInt.</summary>
            Interior_VillageInt = 66,
            /// <summary>Sewer.</summary>
            Interior_Sewer = 68,
            /// <summary>Doors.</summary>
            Interior_Doors = 74,
        }

        /// <summary>
        /// Weather variations of climate sets.
        /// </summary>
        public enum ClimateWeather
        {
            /// <summary>Dry summer weather. Use with any climate set.</summary>
            Normal = 0,
            /// <summary>Buildings and ground in winter (only valid with exterior climate sets).</summary>
            Winter = 1,
            /// <summary>Ground wet from rain (only valid with ClimateSet.Terrain).</summary>
            Rain = 2,
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

        #region Exterior Child Structures

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
