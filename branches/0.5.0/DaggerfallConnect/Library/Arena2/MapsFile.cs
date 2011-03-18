// Project:         DaggerfallConnect
// Description:     Read data from Daggerfall's file formats.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Imports

using System;
using System.Text;
using System.IO;
using DaggerfallConnect.Utility;

#endregion

namespace DaggerfallConnect.Arena2
{
    /// <summary>
    /// Connects to MAPS.BSA to enumerate locations within a specific region and extract their layouts.
    /// </summary>
    public class MapsFile
    {
        #region Class Variables

        /// <summary>
        /// All region names.
        /// </summary>
        private string[] RegionNames = {
            "Alik'r Desert", "Dragontail Mountains", "Glenpoint Foothills", "Daggerfall Bluffs",
            "Yeorth Burrowland", "Dwynnen", "Ravennian Forest", "Devilrock",
            "Malekna Forest", "Isle of Balfiera", "Bantha", "Dak'fron",
            "Islands in the Western Iliac Bay", "Tamarilyn Point", "Lainlyn Cliffs", "Bjoulsae River",
            "Wrothgarian Mountains", "Daggerfall", "Glenpoint", "Betony", "Sentinel", "Anticlere", "Lainlyn", "Wayrest",
            "Gen Tem High Rock village", "Gen Rai Hammerfell village", "Orsinium Area", "Skeffington Wood",
            "Hammerfell bay coast", "Hammerfell sea coast", "High Rock bay coast", "High Rock sea coast",
            "Northmoor", "Menevia", "Alcaire", "Koegria", "Bhoriane", "Kambria", "Phrygias", "Urvaius",
            "Ykalon", "Daenia", "Shalgora", "Abibon-Gora", "Kairou", "Pothago", "Myrkwasa", "Ayasofya",
            "Tigonus", "Kozanset", "Satakalaam", "Totambu", "Mournoth", "Ephesus", "Santaki", "Antiphyllos",
            "Bergama", "Gavaudon", "Tulune", "Glenumbra Moors", "Ilessan Hills", "Cybiades"
        };

        /// <summary>
        /// Block file prefixes.
        /// </summary>
        private string[] RmbBlockPrefixes = {
	        "TVRN", "GENR", "RESI", "WEAP", "ARMR", "ALCH", "BANK", "BOOK",
	        "CLOT", "FURN", "GEMS", "LIBR", "PAWN", "TEMP", "TEMP", "PALA",
	        "FARM", "DUNG", "CAST", "MANR", "SHRI", "RUIN", "SHCK", "GRVE",
	        "FILL", "KRAV", "KDRA", "KOWL", "KMOO", "KCAN", "KFLA", "KHOR",
	        "KROS", "KWHE", "KSCA", "KHAW", "MAGE", "THIE", "DARK", "FIGH",
	        "CUST", "WALL", "MARK", "SHIP", "WITC"
        };

        /// <summary>
        /// Temple number array.
        /// </summary>
        private string[] RmbTempleNumbers = { "A0", "B0", "C0", "D0", "E0", "F0", "G0", "H0" };

        /// <summary>
        /// RMB block letters array.
        /// </summary>
        private string[] RmbBlockLetters = { "AA", "BA", "AL", "BL", "AM", "BM", "AS", "BS", "GA", "GL", "GM", "GS" };

        /// <summary>
        /// RDB block letters array.
        /// </summary>
        private string[] RdbBlockLetters = { "N", "W", "L", "S", "B", "M" };

        /// <summary>
        /// Auto-discard behaviour enabled or disabled.
        /// </summary>
        private bool AutoDiscardValue = true;

        /// <summary>
        /// The last region opened. Used by auto-discard logic.
        /// </summary>
        private int LastRegion = -1;

        /// <summary>
        /// The BsaFile representing MAPS.BSA.
        /// </summary>
        private BsaFile BsaFile = new BsaFile();

        /// <summary>
        /// Array of decomposed region records.
        /// </summary>
        private RegionRecord[] Regions;

        /// <summary>
        /// Flag set when class is loaded and ready.
        /// </summary>
        private bool IsReady = false;

        #endregion

        #region Class Structures

        /// <summary>
        /// Represents a single region record.
        /// </summary>
        private struct RegionRecord
        {
            public string Name;
            public FileProxy MapNames;
            public FileProxy MapTable;
            public FileProxy MapPItem;
            public FileProxy MapDItem;
            public DFRegion DFRegion;
        }

        /// <summary>
        /// Offsets to dungeon records.
        /// </summary>
        public struct DungeonOffset
        {
            /// <summary>Offset in bytes relative to end of offset list.</summary>
            public UInt32 Offset;

            /// <summary>Is TRUE (0x0001) for all elements.</summary>
            public UInt16 IsDungeon;

            /// <summary>The exterior location this dungeon is paired with.</summary>
            public UInt16 ExteriorLocationId;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MapsFile()
        {
        }

        /// <summary>
        /// Load constructor.
        /// </summary>
        /// <param name="FilePath">Absolute path to MAPS.BSA.</param>
        /// <param name="Usage">Determines if the BSA file will read from disk or memory.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        public MapsFile(string FilePath, FileUsage Usage, bool ReadOnly)
        {
            Load(FilePath, Usage, ReadOnly);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// If true then decomposed regions will be destroyed every time a different region is fetched.
        ///  If false then decomposed regions will be maintained until DiscardRegion() or DiscardAllRegions() is called.
        ///  Turning off auto-discard will speed up region retrieval times at the expense of RAM. For best results, disable
        ///  auto-discard and impose your own caching scheme using LoadRecord() and DiscardRecord() based on your application
        ///  needs.
        /// </summary>
        public bool AutoDiscard
        {
            get { return AutoDiscardValue; }
            set { AutoDiscardValue = value; }
        }

        /// <summary>
        /// Number of regions in MAPS.BSA.
        /// </summary>
        public int RegionCount
        {
            get { return BsaFile.Count / 4; }
        }

        /// <summary>
        /// True when ready to load regions and locations, otherwise false.
        /// </summary>
        public bool Ready
        {
            get { return IsReady; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load MAPS.BSA file.
        /// </summary>
        /// <param name="FilePath">Absolute path to MAPS.BSA file.</param>
        /// <param name="Usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool Load(string FilePath, FileUsage Usage, bool ReadOnly)
        {
            // Validate filename
            FilePath = FilePath.ToUpper();
            if (!FilePath.EndsWith("MAPS.BSA"))
                return false;

            // Load file
            IsReady = false;
            if (!BsaFile.Load(FilePath, Usage, ReadOnly))
                return false;

            // Create records array
            Regions = new RegionRecord[RegionCount];

            // Set ready flag
            IsReady = true;

            return true;
        }

        /// <summary>
        /// Gets the name of specified region. Does not change the currently loaded region.
        /// </summary>
        /// <param name="Region">Index of region.</param>
        /// <returns>Name of the region.</returns>
        public string GetRegionName(int Region)
        {
            return RegionNames[Region];
        }

        /// <summary>
        /// Gets an array of region names.
        /// </summary>
        /// <returns>Array of all region names as strings.</returns>
        public string[] GetAllRegionNames()
        {
            return RegionNames;
        }

        /// <summary>
        /// Gets index of region with specified name. Does not change the currently loaded region.
        /// </summary>
        /// <param name="Name">Name of region to search for.</param>
        /// <returns>Index of found region, or -1 if not found.</returns>
        public int GetRegionIndex(string Name)
        {
            // Search for region name
            for (int i = 0; i < RegionCount; i++)
            {
                if (RegionNames[i] == Name)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Load a region into memory and decompose it for use.
        /// </summary>
        /// <param name="Region">Index of region to load.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool LoadRegion(int Region)
        {
            // Validate
            if (Region >= RegionCount)
                return false;

            // Exit if file has already been opened
            if (Regions[Region].MapNames != null &&
                Regions[Region].MapTable != null &&
                Regions[Region].MapPItem != null &&
                Regions[Region].MapDItem != null)
                return true;

            // Auto discard previous record
            if (AutoDiscardValue && LastRegion != -1)
                DiscardRegion(LastRegion);

            // Load record data
            int record = Region * 4;
            Regions[Region].MapPItem = BsaFile.GetRecordProxy(record++);
            Regions[Region].MapDItem = BsaFile.GetRecordProxy(record++);
            Regions[Region].MapTable = BsaFile.GetRecordProxy(record++);
            Regions[Region].MapNames = BsaFile.GetRecordProxy(record);
            if (Regions[Region].MapNames.Length == 0 ||
                Regions[Region].MapTable.Length == 0 ||
                Regions[Region].MapPItem.Length == 0 ||
                Regions[Region].MapDItem.Length == 0)
                return false;

            // Set region name
            Regions[Region].Name = RegionNames[Region];

            // Read region
            if (!ReadRegion(Region))
            {
                DiscardRegion(Region);
                return false;
            }

            // Set previous record
            LastRegion = Region;

            return true;
        }

        /// <summary>
        /// Discard a region from memory.
        /// </summary>
        /// <param name="Region">Index of region to discard.</param>
        public void DiscardRegion(int Region)
        {
            // Validate
            if (Region >= RegionCount)
                return;

            // Discard memory files and other data
            Regions[Region].Name = string.Empty;
            Regions[Region].MapNames = null;
            Regions[Region].MapTable = null;
            Regions[Region].MapPItem = null;
            Regions[Region].MapDItem = null;
            Regions[Region].DFRegion = new DFRegion();
        }

        /// <summary>
        /// Discard all regions.
        /// </summary>
        public void DiscardAllRegions()
        {
            for (int index = 0; index < RegionCount; index++)
            {
                DiscardRegion(index);
            }
        }

        /// <summary>
        /// Gets a DFRegion by index.
        /// </summary>
        /// <param name="Region">Index of region.</param>
        /// <returns>DFRegion.</returns>
        public DFRegion GetRegion(int Region)
        {
            // Load the region
            if (!LoadRegion(Region))
                return new DFRegion();

            return Regions[Region].DFRegion;
        }

        /// <summary>
        /// Gets a DFRegion by name.
        /// </summary>
        /// <param name="Name">Name of region.</param>
        /// <returns>DFRegion.</returns>
        public DFRegion GetRegion(string Name)
        {
            int Region = GetRegionIndex(Name);
            if (-1 == Region)
                return new DFRegion();

            return GetRegion(Region);
        }

        /// <summary>
        /// Gets a DFLocation representation of a location.
        /// </summary>
        /// <param name="Region">Index of region.</param>
        /// <param name="Location">Index of location.</param>
        /// <returns>DFLocation.</returns>
        public DFLocation GetLocation(int Region, int Location)
        {
            // Load the region
            if (!LoadRegion(Region))
                return new DFLocation();

            // Read location
            DFLocation dfLocation = new DFLocation();
            if (!ReadLocation(Region, Location, ref dfLocation))
                return new DFLocation();

            return dfLocation;
        }

        /// <summary>
        /// Gets DFLocation representation of a location.
        /// </summary>
        /// <param name="RegionName">Name of region.</param>
        /// <param name="LocationName">Name of location.</param>
        /// <returns>DFLocation.</returns>
        public DFLocation GetLocation(string RegionName, string LocationName)
        {
            // Load region
            int Region = GetRegionIndex(RegionName);
            if (!LoadRegion(Region))
                return new DFLocation();

            // Check location exists
            if (!Regions[Region].DFRegion.MapNameLookup.ContainsKey(LocationName))
                return new DFLocation();

            // Get location index
            int Location = Regions[Region].DFRegion.MapNameLookup[LocationName];

            return GetLocation(Region, Location);
        }

        /// <summary>
        /// Resolve block name for exterior block.
        /// </summary>
        /// <param name="dfLocation">DFLocation to resolve block name.</param>
        /// <param name="X">Block X coordinate.</param>
        /// <param name="Y">Block Y coordinate.</param>
        /// <returns>Block name.</returns>
        public string GetRmbBlockName(ref DFLocation dfLocation, int X, int Y)
        {
            string letters = string.Empty;
            string numbers = string.Empty;

            // Get indices
            int offset = Y * dfLocation.Exterior.ExteriorData.Width + X;
            byte blockIndex = dfLocation.Exterior.ExteriorData.BlockIndex[offset];
            byte blockNumber = dfLocation.Exterior.ExteriorData.BlockNumber[offset];
            byte blockCharacter = dfLocation.Exterior.ExteriorData.BlockCharacter[offset];

            // Get prefix
            string prefix = RmbBlockPrefixes[blockIndex];

            // Get letters and numbers
            if (blockIndex == 13 || blockIndex == 14)
            {
                // Handle temple logic
                if (7 < blockCharacter) letters = "GA"; else letters = "AA";
                numbers = RmbTempleNumbers[blockCharacter & 7];
            }
            else
            {
                // Numbers are uniform in non-temple blocks
                numbers = string.Format("{0:00}", blockNumber);

                // Letters have some special cases
                byte q = (byte)(blockCharacter / 16);
                if (dfLocation.Name == "Wayrest")
                {
                    // Handle Wayrest exceptions
                    if (prefix == "CUST")
                        q = 0;
                    else
                        if (q > 0) q--;
                }
                else if (dfLocation.Name == "Sentinel")
                {
                    // Handle Sentinel exceptions
                    if (prefix == "CUST")
                        q = 8;
                }
                else
                {
                    // Default
                    if (prefix == "CUST")
                        q = 0;
                }

                // Resolve letters
                letters = RmbBlockLetters[q];
            }

            return prefix + letters + numbers + ".RMB";
        }

        #endregion

        #region Readers

        /// <summary>
        /// Read a region.
        /// </summary>
        /// <param name="Region">The region index to read.</param>
        /// <returns>True if successful, otherwise false.</returns>
        private bool ReadRegion(int Region)
        {
            try
            {
                // Store region name
                Regions[Region].DFRegion.Name = RegionNames[Region];

                // Read map names
                BinaryReader reader = Regions[Region].MapNames.GetReader();
                ReadMapNames(ref reader, Region);

                // Read map table
                reader = Regions[Region].MapTable.GetReader();
                ReadMapTable(ref reader, Region);
            }
            catch (Exception e)
            {
                DiscardRegion(Region);
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Read a location from the currently loaded region.
        /// </summary>
        /// <param name="Region">Region index.</param>
        /// <param name="Location">Location index.</param>
        /// <param name="dfLocation">DFLocation object to receive data.</param>
        /// <returns>True if successful, otherwise false.</returns>
        private bool ReadLocation(int Region, int Location, ref DFLocation dfLocation)
        {
            try
            {
                // Read MapPItem for this location
                BinaryReader reader = Regions[Region].MapPItem.GetReader();
                ReadMapPItem(ref reader, Region, Location, ref dfLocation);

                // Read MapDItem for this location
                reader = Regions[Region].MapDItem.GetReader();
                ReadMapDItem(ref reader, Region, Location, ref dfLocation);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Read map names.
        /// </summary>
        /// <param name="Reader">A binary reader to data.</param>
        /// <param name="Region">Destination region index.</param>
        private void ReadMapNames(ref BinaryReader Reader, int Region)
        {
            // Location count
            Reader.BaseStream.Position = 0;
            Regions[Region].DFRegion.LocationCount = Reader.ReadUInt32();

            // Read names
            Regions[Region].DFRegion.MapNames = new String[Regions[Region].DFRegion.LocationCount];
            Regions[Region].DFRegion.MapNameLookup = new System.Collections.Generic.Dictionary<string, int>();
            for (int i = 0; i < Regions[Region].DFRegion.LocationCount; i++)
            {
                // Read map name data
                Regions[Region].DFRegion.MapNames[i] = Regions[Region].MapNames.ReadCStringSkip(Reader, 0, 32);

                // Add to dictionary
                if (!Regions[Region].DFRegion.MapNameLookup.ContainsKey(Regions[Region].DFRegion.MapNames[i]))
                    Regions[Region].DFRegion.MapNameLookup.Add(Regions[Region].DFRegion.MapNames[i], i);
            }
        }

        /// <summary>
        /// Read map table.
        /// </summary>
        /// <param name="Reader">A binary reader to data.</param>
        /// <param name="Region">Destination region index.</param>
        private void ReadMapTable(ref BinaryReader Reader, int Region)
        {
            // Read map table for each location
            UInt32 bitfield;
            Reader.BaseStream.Position = 0;
            Regions[Region].DFRegion.MapTable = new DFRegion.RegionMapTable[Regions[Region].DFRegion.LocationCount];
            Regions[Region].DFRegion.MapIdLookup = new System.Collections.Generic.Dictionary<int, int>();
            for (int i = 0; i < Regions[Region].DFRegion.LocationCount; i++)
            {
                // Read map table data
                Regions[Region].DFRegion.MapTable[i].MapId = Reader.ReadInt32();
                Regions[Region].DFRegion.MapTable[i].Unknown1 = Reader.ReadByte();
                bitfield = Reader.ReadUInt32();
                Regions[Region].DFRegion.MapTable[i].LongitudeTypeBitfield = bitfield;
                Regions[Region].DFRegion.MapTable[i].Longitude = bitfield & 0x1ffff;
                Regions[Region].DFRegion.MapTable[i].Type = (DFRegion.LocationTypes)(bitfield >> 17);
                Regions[Region].DFRegion.MapTable[i].Latitude = Reader.ReadUInt16();
                Regions[Region].DFRegion.MapTable[i].Unknown2 = Reader.ReadUInt16();
                Regions[Region].DFRegion.MapTable[i].Unknown3 = Reader.ReadUInt32();

                // Add to dictionary
                if (!Regions[Region].DFRegion.MapIdLookup.ContainsKey(Regions[Region].DFRegion.MapTable[i].MapId))
                    Regions[Region].DFRegion.MapIdLookup.Add(Regions[Region].DFRegion.MapTable[i].MapId, i);
            }
        }

        /// <summary>
        /// Reads MapPItem data.
        /// </summary>
        /// <param name="Reader">A binary reader to data.</param>
        /// <param name="Region">Region index.</param>
        /// <param name="Location">Location Index.</param>
        /// <param name="dfLocation">Destination DFLocation.</param>
        private void ReadMapPItem(ref BinaryReader Reader, int Region, int Location, ref DFLocation dfLocation)
        {
            // Position reader at location record by reading offset and adding to end of offset table
            Reader.BaseStream.Position = Location * 4;
            Reader.BaseStream.Position = (Regions[Region].DFRegion.LocationCount * 4) + Reader.ReadUInt32();

            // Store name
            dfLocation.Name = Regions[Region].DFRegion.MapNames[Location];

            // Read LocationRecordElement
            ReadLocationRecordElement(ref Reader, Region, ref dfLocation.Exterior.RecordElement);

            // Read BuildingListHeader
            dfLocation.Exterior.BuildingCount = Reader.ReadUInt16();
            dfLocation.Exterior.Unknown1 = new Byte[5];
            for (int i = 0; i < 5; i++) dfLocation.Exterior.Unknown1[i] = Reader.ReadByte();

            // Read BuildingData
            dfLocation.Exterior.Buildings = new DFLocation.BuildingData[dfLocation.Exterior.BuildingCount];
            for (int building = 0; building < dfLocation.Exterior.BuildingCount; building++)
            {
                dfLocation.Exterior.Buildings[building].NameSeed = Reader.ReadUInt16();
                dfLocation.Exterior.Buildings[building].NullValue1 = Reader.ReadUInt64();
                dfLocation.Exterior.Buildings[building].NullValue2 = Reader.ReadUInt64();
                dfLocation.Exterior.Buildings[building].FactionId = Reader.ReadUInt16();
                dfLocation.Exterior.Buildings[building].Sector = Reader.ReadInt16();
                dfLocation.Exterior.Buildings[building].LocationId = Reader.ReadUInt16();
                dfLocation.Exterior.Buildings[building].BuildingType = Reader.ReadByte();
                dfLocation.Exterior.Buildings[building].Quality = Reader.ReadByte();
            }

            // Read ExteriorData
            dfLocation.Exterior.ExteriorData.AnotherName = Regions[Region].MapPItem.ReadCStringSkip(Reader, 0, 32);
            dfLocation.Exterior.ExteriorData.MapId = Reader.ReadInt32();
            dfLocation.Exterior.ExteriorData.LocationId = Reader.ReadUInt32();
            dfLocation.Exterior.ExteriorData.Width = Reader.ReadByte();
            dfLocation.Exterior.ExteriorData.Height = Reader.ReadByte();
            dfLocation.Exterior.ExteriorData.Unknown2 = Reader.ReadBytes(7);
            dfLocation.Exterior.ExteriorData.BlockIndex = Reader.ReadBytes(64);
            dfLocation.Exterior.ExteriorData.BlockNumber = Reader.ReadBytes(64);
            dfLocation.Exterior.ExteriorData.BlockCharacter = Reader.ReadBytes(64);
            dfLocation.Exterior.ExteriorData.Unknown3 = Reader.ReadBytes(34);
            dfLocation.Exterior.ExteriorData.NullValue1 = Reader.ReadUInt64();
            dfLocation.Exterior.ExteriorData.NullValue2 = Reader.ReadByte();
            dfLocation.Exterior.ExteriorData.Unknown4 = new UInt32[22];
            for (int i = 0; i < 22; i++) dfLocation.Exterior.ExteriorData.Unknown4[i] = Reader.ReadUInt32();
            dfLocation.Exterior.ExteriorData.NullValue3 = Reader.ReadBytes(40);
            dfLocation.Exterior.ExteriorData.Unknown5 = Reader.ReadUInt32();
        }

        /// <summary>
        /// Read LocationRecordElementData common to both MapPItem and MapDItem
        /// </summary>
        /// <param name="Reader">A binary reader to data.</param>
        /// <param name="Region">Region index.</param>
        /// <param name="RecordElement">Destination DFLocation.LocationRecordElement.</param>
        private void ReadLocationRecordElement(ref BinaryReader Reader, int Region, ref DFLocation.LocationRecordElement RecordElement)
        {
            // Read LocationDoorElement
            UInt32 doorCount = Reader.ReadUInt32();
            RecordElement.DoorCount = doorCount;
            RecordElement.Doors = new DFLocation.LocationDoorElement[doorCount];
            for (int door = 0; door < doorCount; door++)
            {
                RecordElement.Doors[door].BuildingDataIndex = Reader.ReadUInt16();
                RecordElement.Doors[door].NullValue = Reader.ReadByte();
                RecordElement.Doors[door].Mask = Reader.ReadByte();
                RecordElement.Doors[door].Unknown1 = Reader.ReadByte();
                RecordElement.Doors[door].Unknown2 = Reader.ReadByte();
            }

            // Read LocationRecordElementHeader
            RecordElement.Header.AlwaysOne1 = Reader.ReadUInt32();
            RecordElement.Header.NullValue1 = Reader.ReadUInt16();
            RecordElement.Header.NullValue2 = Reader.ReadByte();
            RecordElement.Header.X = Reader.ReadInt32();
            RecordElement.Header.NullValue3 = Reader.ReadUInt32();
            RecordElement.Header.Y = Reader.ReadInt32();
            RecordElement.Header.IsExterior = Reader.ReadUInt16();
            RecordElement.Header.NullValue4 = Reader.ReadUInt16();
            RecordElement.Header.Unknown1 = Reader.ReadUInt32();
            RecordElement.Header.Unknown2 = Reader.ReadUInt32();
            RecordElement.Header.AlwaysOne2 = Reader.ReadUInt16();
            RecordElement.Header.LocationId = Reader.ReadUInt16();
            RecordElement.Header.NullValue5 = Reader.ReadUInt32();
            RecordElement.Header.IsInterior = Reader.ReadUInt16();
            RecordElement.Header.ExteriorLocationId = Reader.ReadUInt32();
            RecordElement.Header.NullValue6 = Reader.ReadBytes(26);
            RecordElement.Header.LocationName = Regions[Region].MapPItem.ReadCStringSkip(Reader, 0, 32);
            RecordElement.Header.Unknown3 = Reader.ReadBytes(9);
        }

        /// <summary>
        /// Reads MapDItem data.
        /// </summary>
        /// <param name="Reader">A binary reader to data.</param>
        /// <param name="Region">Region index.</param>
        /// <param name="Location">Location index.</param>
        /// <param name="dfLocation">Destination DFLocation.</param>
        private void ReadMapDItem(ref BinaryReader Reader, int Region, int Location, ref DFLocation dfLocation)
        {
            // Exit if no data
            dfLocation.HasDungeon = false;
            if (Reader.BaseStream.Length == 0)
                return;

            // Find dungeon offset
            bool found = false;
            UInt32 locationId = dfLocation.Exterior.RecordElement.Header.LocationId;
            UInt32 dungeonCount = Reader.ReadUInt32();
            DungeonOffset dungeonOffset = new DungeonOffset();
            for (int i = 0; i < dungeonCount; i++)
            {
                // Search for dungeon offset matching location id
                dungeonOffset.Offset = Reader.ReadUInt32();
                dungeonOffset.IsDungeon = Reader.ReadUInt16();
                dungeonOffset.ExteriorLocationId = Reader.ReadUInt16();
                if (dungeonOffset.ExteriorLocationId == locationId)
                {
                    found = true;
                    break;
                }
            }

            // Exit if correct offset not found
            if (!found)
                return;

            // Position reader at dungeon record by reading offset and adding to end of offset table
            Reader.BaseStream.Position = 4 + dungeonCount * 8 + dungeonOffset.Offset;

            // Read LocationRecordElement
            ReadLocationRecordElement(ref Reader, Region, ref dfLocation.Dungeon.RecordElement);

            // Read DungeonHeader
            dfLocation.Dungeon.Header.NullValue1 = Reader.ReadUInt16();
            dfLocation.Dungeon.Header.Unknown1 = Reader.ReadUInt32();
            dfLocation.Dungeon.Header.Unknown2 = Reader.ReadUInt32();
            dfLocation.Dungeon.Header.BlockCount = Reader.ReadUInt16();
            dfLocation.Dungeon.Header.Unknown3 = Reader.ReadBytes(5);

            // Read DungeonBlock elements
            dfLocation.Dungeon.Blocks = new DFLocation.DungeonBlock[dfLocation.Dungeon.Header.BlockCount];
            for (int i = 0; i < dfLocation.Dungeon.Header.BlockCount; i++)
            {
                // Read data
                dfLocation.Dungeon.Blocks[i].X = Reader.ReadSByte();
                dfLocation.Dungeon.Blocks[i].Z = Reader.ReadSByte();
                dfLocation.Dungeon.Blocks[i].BlockNumberStartIndexBitfield = Reader.ReadUInt16();

                // Decompose bitfield
                UInt16 bitfield = dfLocation.Dungeon.Blocks[i].BlockNumberStartIndexBitfield;
                dfLocation.Dungeon.Blocks[i].BlockNumber = (UInt16)(bitfield & 0x3ff);
                dfLocation.Dungeon.Blocks[i].IsStartingBlock = ((bitfield & 0x400) == 0x400) ? true : false;
                dfLocation.Dungeon.Blocks[i].BlockIndex = (Byte)(bitfield >> 11);

                // Compose block name
                dfLocation.Dungeon.Blocks[i].BlockName = String.Format("{0}{1:0000000}.RDB", RdbBlockLetters[dfLocation.Dungeon.Blocks[i].BlockIndex], dfLocation.Dungeon.Blocks[i].BlockNumber);
            }

            // Set dungeon flag
            dfLocation.HasDungeon = true;
        }

        #endregion
    }
}
