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
using System.Collections.Generic;
using System.IO;
using DaggerfallConnect.Utility;

#endregion

namespace DaggerfallConnect.Arena2
{
    /// <summary>
    /// Connects to BLOCKS.BSA to enumerate and extract city and dungeon blocks.
    /// </summary>
    public class BlocksFile
    {
        #region Class Variables

        /// <summary>
        /// Auto-discard behaviour enabled or disabled.
        /// </summary>
        private bool AutoDiscardValue = true;

        /// <summary>
        /// The last record opened. Used by auto-discard logic.
        /// </summary>
        private int LastBlock = -1;

        /// <summary>
        /// The BsaFile representing BLOCKS.BSA.
        /// </summary>
        private BsaFile BsaFile = new BsaFile();

        /// <summary>
        /// Array of decomposed block records.
        /// </summary>
        private BlockRecord[] Blocks;

        /// <summary>
        /// Name to index lookup dictionary.
        /// </summary>
        private Dictionary<String, int> BlockNameLookup = new Dictionary<String, int>();

        #endregion

        #region Class Structures

        /// <summary>
        /// Represents a single block record.
        /// </summary>
        private struct BlockRecord
        {
            public string Name;
            public FileProxy MemoryFile;
            public DFBlock DFBlock;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BlocksFile()
        {
        }

        /// <summary>
        /// Load constructor.
        /// </summary>
        /// <param name="FilePath">Absolute path to BLOCKS.BSA.</param>
        /// <param name="Usage">Determines if the BSA file will read from disk or memory.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        public BlocksFile(string FilePath, FileUsage Usage, bool ReadOnly)
        {
            Load(FilePath, Usage, ReadOnly);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// If true then decomposed block records will be destroyed every time a different block is fetched.
        ///  If false then decomposed block records will be maintained until DiscardRecord() or DiscardAllRecords() is called.
        ///  Turning off auto-discard will speed up block retrieval times at the expense of RAM. For best results, disable
        ///  auto-discard and impose your own caching scheme using LoadBlock() and DiscardBlock() based on your application
        ///  needs.
        /// </summary>
        public bool AutoDiscard
        {
            get { return AutoDiscardValue; }
            set { AutoDiscardValue = value; }
        }

        /// <summary>
        /// Number of BSA records in ARCH3D.BSA.
        /// </summary>
        public int Count
        {
            get { return BsaFile.Count; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load BLOCKS.BSA file.
        /// </summary>
        /// <param name="FilePath">Absolute path to BLOCKS.BSA file.</param>
        /// <param name="Usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool Load(string FilePath, FileUsage Usage, bool ReadOnly)
        {
            // Validate filename
            FilePath = FilePath.ToUpper();
            if (!FilePath.EndsWith("BLOCKS.BSA"))
                return false;

            // Load file
            if (!BsaFile.Load(FilePath, Usage, ReadOnly))
                return false;

            // Create records array
            Blocks = new BlockRecord[BsaFile.Count];

            return true;
        }

        /// <summary>
        /// Gets name of specified block. Does not change the currently loaded block.
        /// </summary>
        /// <param name="Block">Index of block.</param>
        /// <returns>Name of the block.</returns>
        public string GetBlockName(int Block)
        {
            return BsaFile.GetRecordName(Block);
        }

        /// <summary>
        /// Gets the type of specified block. Does not change the currently loaded block.
        /// </summary>
        /// <param name="Block">Index of block.</param>
        /// <returns>DFBlock.blockTypes object.</returns>
        public DFBlock.BlockTypes GetBlockType(int Block)
        {
            // Determine record type from extension of name
            string name = GetBlockName(Block);
            if (name.EndsWith(".RMB"))
                return DFBlock.BlockTypes.Rmb;
            else if (name.EndsWith(".RDB"))
                return DFBlock.BlockTypes.Rdb;
            else if (name.EndsWith(".RDI"))
                return DFBlock.BlockTypes.Rdi;
            else
                return DFBlock.BlockTypes.Unknown;
        }

        /// <summary>
        /// Get RDB block type (quest, normal, wet, etc.)
        ///  Does not return RdbTypes.Start as this can only be derived from
        ///  map data.
        /// </summary>
        /// <param name="BlockName">Name of RDB block.</param>
        /// <returns>DFBlock.RdbTypes object.</returns>
        public DFBlock.RdbTypes GetRdbType(string BlockName)
        {
            // Determine block type
            if (BlockName.StartsWith("B"))
                return DFBlock.RdbTypes.Border;
            else if (BlockName.StartsWith("W"))
                return DFBlock.RdbTypes.Wet;
            else if (BlockName.StartsWith("S"))
                return DFBlock.RdbTypes.Quest;
            else if (BlockName.StartsWith("M"))
                return DFBlock.RdbTypes.Mausoleum;
            else if (BlockName.StartsWith("N"))
                return DFBlock.RdbTypes.Normal;
            else
                return DFBlock.RdbTypes.Unknown;
        }

        /// <summary>
        /// Gets index of block with specified name. Does not change the currently loaded block.
        ///  Uses a dictionary to map name to index so this method will be faster on subsequent calls.
        /// </summary>
        /// <param name="Name">Name of block.</param>
        /// <returns>Index of found block, or -1 if not found.</returns>
        public int GetBlockIndex(string Name)
        {
            // Return known value if already indexed
            if (BlockNameLookup.ContainsKey(Name))
                return BlockNameLookup[Name];

            // Otherwise find and store index by searching for name
            for (int i = 0; i < Count; i++)
            {
                if (GetBlockName(i) == Name)
                {
                    // Found the block, add to dictionary and return
                    BlockNameLookup.Add(Name, i);
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Load a block into memory and decompose it for use.
        /// </summary>
        /// <param name="Block">Index of block to load.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool LoadBlock(int Block)
        {
            // Validate
            if (Block < 0 || Block >= BsaFile.Count)
                return false;

            // Exit if file has already been opened
            if (Blocks[Block].MemoryFile != null )
                return true;

            // Auto discard previous record
            if (AutoDiscardValue && LastBlock != -1)
                DiscardBlock(LastBlock);

            // Load record data
            Blocks[Block].MemoryFile = BsaFile.GetRecordProxy(Block);
            if (Blocks[Block].MemoryFile == null)
                return false;

            // Set record name
            Blocks[Block].Name = BsaFile.GetRecordName(Block);
            Blocks[Block].DFBlock.Name = BsaFile.GetRecordName(Block);

            // Set record type
            Blocks[Block].DFBlock.Type = GetBlockType(Block);

            // Read record
            if (!Read(Block))
            {
                DiscardBlock(Block);
                return false;
            }

            // Store in lookup dictionary
            if (!BlockNameLookup.ContainsKey(Blocks[Block].Name))
                BlockNameLookup.Add(Blocks[Block].Name, Block);

            // Set previous record
            LastBlock = Block;

            return true;
        }

        /// <summary>
        /// Discard a block from memory.
        /// </summary>
        /// <param name="Block">Index of block to discard.</param>
        public void DiscardBlock(int Block)
        {
            // Validate
            if (Block >= BsaFile.Count)
                return;

            // Discard memory file and other data
            Blocks[Block].Name = string.Empty;
            Blocks[Block].DFBlock.Type = DFBlock.BlockTypes.Unknown;
            Blocks[Block].MemoryFile = null;
            Blocks[Block].DFBlock.RmbBlock.Misc3dObjectRecords = null;
            Blocks[Block].DFBlock.RmbBlock.MiscFlatObjectRecords = null;
            Blocks[Block].DFBlock.RmbBlock.SubRecords = null;
            Blocks[Block].DFBlock.RdbBlock.ModelDataList = null;
            Blocks[Block].DFBlock.RdbBlock.ModelReferenceList = null;
            Blocks[Block].DFBlock.RdbBlock.ObjectRootList = null;
        }

        /// <summary>
        /// Discard all block records.
        /// </summary>
        public void DiscardAllBlocks()
        {
            for (int block = 0; block < BsaFile.Count; block++)
            {
                DiscardBlock(block);
            }
        }

        /// <summary>
        /// Gets a DFBlock representation of a record.
        /// </summary>
        /// <param name="Block">Index of block to load.</param>
        /// <returns>DFBlock object.</returns>
        public DFBlock GetBlock(int Block)
        {
            // Load the record
            if (!LoadBlock(Block))
                return new DFBlock();

            return Blocks[Block].DFBlock;
        }

        /// <summary>
        /// Gets a DFBlock by name.
        /// </summary>
        /// <param name="Name">Name of block.</param>
        /// <returns>DFBlock object.</returns>
        public DFBlock GetBlock(string Name)
        {
            // Look for block index
            int index = GetBlockIndex(Name);
            if (index == -1)
            {
                // Not found, search for alternate name
                string alternateName = SearchAlternateRMBName(ref Name);
                if (!string.IsNullOrEmpty(alternateName))
                    index = GetBlockIndex(alternateName);
            }

            return GetBlock(index);
        }

        /// <summary>
        /// Gets block AutoMap by name.
        /// </summary>
        /// <param name="Name">Name of block.</param>
        /// <param name="RemoveGroundFlats">Filters ground flat "speckles" from the AutoMap.</param>
        /// <returns>DFBitmap object.</returns>
        public DFBitmap GetBlockAutoMap(string Name, bool RemoveGroundFlats)
        {
            // Test block is valid
            DFBlock dfBlock = GetBlock(Name);
            if (string.IsNullOrEmpty(dfBlock.Name))
                return new DFBitmap();

            // Create DFBitmap and copy data
            DFBitmap dfBitmap = new DFBitmap();
            dfBitmap.Data = dfBlock.RmbBlock.FldHeader.AutoMapData;
            dfBitmap.Width = 64;
            dfBitmap.Height = 64;
            dfBitmap.Stride = 64;
            dfBitmap.Format = DFBitmap.Formats.Indexed;

            // Filter ground flats if specified
            if (RemoveGroundFlats)
            {
                for (int i = 0; i < dfBitmap.Data.Length; i++)
                {
                    if (dfBitmap.Data[i] == 0xfb)
                        dfBitmap.Data[i] = 0x00;
                }
            }

            return dfBitmap;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Not all RMB block names can be resolved.
        ///  This method attempts to find a suitable match for these cases
        ///  by matching prefix and suffix of failed block name to a 
        ///  valid block name.
        /// </summary>
        /// <param name="Name">Name of invalid block.</param>
        /// <returns>Valid block name with same prefix and suffix, or string.Empty if no match found.</returns>
        private string SearchAlternateRMBName(ref string Name)
        {
            string found = string.Empty;
            string prefix = Name.Substring(0, 4);
            string suffix = Name.Substring(Name.Length - 6, 6);
            for (int block = 0; block < Count; block++)
            {
                string test = GetBlockName(block);
                if (test.StartsWith(prefix) && test.EndsWith(suffix))
                    found = test;
            }

            return found;
        }

        #endregion

        #region Readers

        /// <summary>
        /// Read a block record.
        /// </summary>
        /// <param name="Block">The block index to read.</param>
        /// <returns>True if successful, otherwise false.</returns>
        private bool Read(int Block)
        {
            try
            {
                // Read memory file
                BinaryReader reader = Blocks[Block].MemoryFile.GetReader();
                ReadBlock(ref reader, Block);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Choose to read an RMB, RDB, or RDI block record. Other block types will be discarded.
        /// </summary>
        /// <param name="Reader">A binary reader to file.</param>
        /// <param name="Block">Destination block index.</param>
        private void ReadBlock(ref BinaryReader Reader, int Block)
        {
            // Step through file based on type
            Reader.BaseStream.Position = 0;
            if (Blocks[Block].DFBlock.Type == DFBlock.BlockTypes.Rmb)
            {
                // Read RMB data
                ReadRmbFldHeader(ref Reader, Block);
                ReadRmbBlockData(ref Reader, Block);
                ReadRmbMisc3dObjects(ref Reader, Block);
                ReadRmbMiscFlatObjectRecords(ref Reader, Block);
            }
            else if (Blocks[Block].DFBlock.Type == DFBlock.BlockTypes.Rdb)
            {
                // Read RDB data
                ReadRdbHeader(ref Reader, Block);
                ReadRdbModelReferenceList(ref Reader, Block);
                ReadRdbModelDataList(ref Reader, Block);
                ReadRdbObjectSectionHeader(ref Reader, Block);
                ReadRdbObjectSectionRootList(ref Reader, Block);
                ReadRdbObjectLists(ref Reader, Block);
            }
            else if (Blocks[Block].DFBlock.Type == DFBlock.BlockTypes.Rdi)
            {
                // Read RDI data
                ReadRdiRecord(ref Reader, Block);
            }
            else
            {
                DiscardBlock(Block);
                return;
            }
        }

        #endregion

        #region RMB Readers

        /// <summary>
        /// Read the fixed length data (FLD) header of an RMB record
        /// </summary>
        /// <param name="Reader">A binary reader to file</param>
        /// <param name="Block">Destination block index</param>
        private void ReadRmbFldHeader(ref BinaryReader Reader, int Block)
        {
            // Record counts
            Blocks[Block].DFBlock.RmbBlock.FldHeader.NumBlockDataRecords = Reader.ReadByte();
            Blocks[Block].DFBlock.RmbBlock.FldHeader.NumMisc3dObjectRecords = Reader.ReadByte();
            Blocks[Block].DFBlock.RmbBlock.FldHeader.NumMiscFlatObjectRecords = Reader.ReadByte();

            // Block positions
            Blocks[Block].DFBlock.RmbBlock.FldHeader.BlockPositions = new DFBlock.RmbFldBlockPositions[32];
            for (int i = 0; i < 32; i++)
            {
                Blocks[Block].DFBlock.RmbBlock.FldHeader.BlockPositions[i].Unknown1 = Reader.ReadUInt32();
                Blocks[Block].DFBlock.RmbBlock.FldHeader.BlockPositions[i].Unknown2 = Reader.ReadUInt32();
                Blocks[Block].DFBlock.RmbBlock.FldHeader.BlockPositions[i].XPos = Reader.ReadInt32();
                Blocks[Block].DFBlock.RmbBlock.FldHeader.BlockPositions[i].ZPos = Reader.ReadInt32();
                Blocks[Block].DFBlock.RmbBlock.FldHeader.BlockPositions[i].YRotation = Reader.ReadInt32();
            }

            // Section1 and Section2 unknown data
            Blocks[Block].DFBlock.RmbBlock.FldHeader.Section1UnknownData = Reader.ReadBytes(832);
            Blocks[Block].DFBlock.RmbBlock.FldHeader.Section2UnknownData = Reader.ReadBytes(128);

            // Block data sizes
            Blocks[Block].DFBlock.RmbBlock.FldHeader.BlockDataSizes = new Int32[32];
            for (int i = 0; i < 32; i++)
            {
                Blocks[Block].DFBlock.RmbBlock.FldHeader.BlockDataSizes[i] = Reader.ReadInt32();
            }

            // Ground data
            Blocks[Block].DFBlock.RmbBlock.FldHeader.GroundData.Header = Reader.ReadBytes(8);
            ReadRmbGroundTilesData(ref Reader, Block);
            ReadRmbGroundSceneryData(ref Reader, Block);

            // Automap
            Blocks[Block].DFBlock.RmbBlock.FldHeader.AutoMapData = Reader.ReadBytes(64 * 64);

            // Filenames
            Blocks[Block].DFBlock.RmbBlock.FldHeader.Name = Blocks[Block].MemoryFile.ReadCString(Reader, 13);
            Blocks[Block].DFBlock.RmbBlock.FldHeader.OtherNames = new string[32];
            for (int i = 0; i < 32; i++)
            {
                Blocks[Block].DFBlock.RmbBlock.FldHeader.OtherNames[i] = Blocks[Block].MemoryFile.ReadCString(Reader, 13);
            }
        }

        /// <summary>
        /// Read ground tile data for this block.
        /// </summary>
        /// <param name="Reader">A binary reader to file.</param>
        /// <param name="Block">Destination block index.</param>
        private void ReadRmbGroundTilesData(ref BinaryReader Reader, int Block)
        {
            // Create new array
            Blocks[Block].DFBlock.RmbBlock.FldHeader.GroundData.GroundTiles = new DFBlock.RmbGroundTiles[16, 16];

            // Read in data
            Byte bitfield;
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    // Read source bitfield
                    bitfield = Reader.ReadByte();

                    // Store data
                    Blocks[Block].DFBlock.RmbBlock.FldHeader.GroundData.GroundTiles[x, y].TileBitfield = bitfield;
                    Blocks[Block].DFBlock.RmbBlock.FldHeader.GroundData.GroundTiles[x, y].TextureRecord = bitfield & 0x3f;
                    Blocks[Block].DFBlock.RmbBlock.FldHeader.GroundData.GroundTiles[x, y].IsRotated = ((bitfield & 0x40) == 0x40) ? true : false;
                    Blocks[Block].DFBlock.RmbBlock.FldHeader.GroundData.GroundTiles[x, y].IsFlipped = ((bitfield & 0x80) == 0x80) ? true : false;
                }
            }
        }


        /// <summary>
        /// Read ground scenery data for this block.
        /// </summary>
        /// <param name="Reader">A binary reader to file.</param>
        /// <param name="Block">Destination block index.</param>
        private void ReadRmbGroundSceneryData(ref BinaryReader Reader, int Block)
        {
            // Create new array
            Blocks[Block].DFBlock.RmbBlock.FldHeader.GroundData.GroundScenery = new DFBlock.RmbGroundScenery[16, 16];

            // Read in data
            Byte bitfield;
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    // Read source bitfield
                    bitfield = Reader.ReadByte();

                    // Store data
                    Blocks[Block].DFBlock.RmbBlock.FldHeader.GroundData.GroundScenery[x, y].TileBitfield = bitfield;
                    if (bitfield < 255)
                    {
                        Blocks[Block].DFBlock.RmbBlock.FldHeader.GroundData.GroundScenery[x, y].Unknown1 = bitfield & 0x03;
                        Blocks[Block].DFBlock.RmbBlock.FldHeader.GroundData.GroundScenery[x, y].TextureRecord = bitfield / 0x04 - 1;
                    }
                    else
                    {
                        Blocks[Block].DFBlock.RmbBlock.FldHeader.GroundData.GroundScenery[x, y].Unknown1 = 0;
                        Blocks[Block].DFBlock.RmbBlock.FldHeader.GroundData.GroundScenery[x, y].TextureRecord = -1;
                    }
                }
            }
        }

        /// <summary>
        /// Read RMB block data, i.e. the outside and inside repeating sections.
        /// </summary>
        /// <param name="Reader">A binary reader to file.</param>
        /// <param name="Block">Destination block index.</param>
        private void ReadRmbBlockData(ref BinaryReader Reader, int Block)
        {
            // Read block data
            int recordCount = Blocks[Block].DFBlock.RmbBlock.FldHeader.NumBlockDataRecords;
            Blocks[Block].DFBlock.RmbBlock.SubRecords = new DFBlock.RmbSubRecord[recordCount];
            long position = Reader.BaseStream.Position;
            for (int i = 0; i < recordCount; i++)
            {
                // Copy XZ position and Y rotation into subrecord data for convenience
                Blocks[Block].DFBlock.RmbBlock.SubRecords[i].XPos = Blocks[Block].DFBlock.RmbBlock.FldHeader.BlockPositions[i].XPos;
                Blocks[Block].DFBlock.RmbBlock.SubRecords[i].ZPos = Blocks[Block].DFBlock.RmbBlock.FldHeader.BlockPositions[i].ZPos;
                Blocks[Block].DFBlock.RmbBlock.SubRecords[i].YRotation = Blocks[Block].DFBlock.RmbBlock.FldHeader.BlockPositions[i].YRotation;

                // Read outside and inside block data
                ReadRmbBlockSubRecord(ref Reader, ref Blocks[Block].DFBlock.RmbBlock.SubRecords[i].Exterior);
                ReadRmbBlockSubRecord(ref Reader, ref Blocks[Block].DFBlock.RmbBlock.SubRecords[i].Interior);

                // Offset to next position (this ignores padding byte and ensures block reading is correctly stepped)
                position += Blocks[Block].DFBlock.RmbBlock.FldHeader.BlockDataSizes[i];
                Reader.BaseStream.Position = position;
            }
        }

        /// <summary>
        /// Read miscellaneous block 3D objects.
        /// </summary>
        /// <param name="Reader">A binary reader to file</param>
        /// <param name="Block">Destination block index</param>
        private void ReadRmbMisc3dObjects(ref BinaryReader Reader, int Block)
        {
            // Read misc block 3d objects
            Blocks[Block].DFBlock.RmbBlock.Misc3dObjectRecords = new DFBlock.RmbBlock3dObjectRecord[Blocks[Block].DFBlock.RmbBlock.FldHeader.NumMisc3dObjectRecords];
            ReadRmbModelRecords(ref Reader, ref Blocks[Block].DFBlock.RmbBlock.Misc3dObjectRecords);
        }

        /// <summary>
        /// Read miscellaneous block flat objects.
        /// </summary>
        /// <param name="Reader">A binary reader to file</param>
        /// <param name="Block">Destination block index</param>
        private void ReadRmbMiscFlatObjectRecords(ref BinaryReader Reader, int Block)
        {
            // Read misc flat object records
            Blocks[Block].DFBlock.RmbBlock.MiscFlatObjectRecords = new DFBlock.RmbBlockFlatObjectRecord[Blocks[Block].DFBlock.RmbBlock.FldHeader.NumMiscFlatObjectRecords];
            ReadRmbFlatObjectRecords(ref Reader, ref Blocks[Block].DFBlock.RmbBlock.MiscFlatObjectRecords);
        }

        /// <summary>
        /// Read block subrecords, i.e. the resources embedded in block data.
        /// </summary>
        /// <param name="Reader">A binary reader to file</param>
        /// <param name="BlockData">Destination record index</param>
        private void ReadRmbBlockSubRecord(ref BinaryReader Reader, ref DFBlock.RmbBlockData BlockData)
        {
            // Header
            BlockData.Header.Position = Reader.BaseStream.Position;
            BlockData.Header.Num3dObjectRecords = Reader.ReadByte();
            BlockData.Header.NumFlatObjectRecords = Reader.ReadByte();
            BlockData.Header.NumSection3Records = Reader.ReadByte();
            BlockData.Header.NumPeopleRecords = Reader.ReadByte();
            BlockData.Header.NumDoorRecords = Reader.ReadByte();
            BlockData.Header.Unknown1 = Reader.ReadInt16();
            BlockData.Header.Unknown2 = Reader.ReadInt16();
            BlockData.Header.Unknown3 = Reader.ReadInt16();
            BlockData.Header.Unknown4 = Reader.ReadInt16();
            BlockData.Header.Unknown5 = Reader.ReadInt16();
            BlockData.Header.Unknown6 = Reader.ReadInt16();

            // 3D object records
            BlockData.Block3dObjectRecords = new DFBlock.RmbBlock3dObjectRecord[BlockData.Header.Num3dObjectRecords];
            ReadRmbModelRecords(ref Reader, ref BlockData.Block3dObjectRecords);

            // Flat object record
            BlockData.BlockFlatObjectRecords = new DFBlock.RmbBlockFlatObjectRecord[BlockData.Header.NumFlatObjectRecords];
            ReadRmbFlatObjectRecords(ref Reader, ref BlockData.BlockFlatObjectRecords);

            // Section3 records
            int numSection3Records = BlockData.Header.NumSection3Records;
            BlockData.BlockSection3Records = new DFBlock.RmbBlockSection3Record[numSection3Records];
            for (int i = 0; i < numSection3Records; i++)
            {
                BlockData.BlockSection3Records[i].XPos = Reader.ReadInt32();
                BlockData.BlockSection3Records[i].YPos = Reader.ReadInt32();
                BlockData.BlockSection3Records[i].ZPos = Reader.ReadInt32();
                BlockData.BlockSection3Records[i].Unknown1 = Reader.ReadByte();
                BlockData.BlockSection3Records[i].Unknown2 = Reader.ReadByte();
                BlockData.BlockSection3Records[i].Unknown3 = Reader.ReadInt16();
            }

            // People records
            int numPeopleRecords = BlockData.Header.NumPeopleRecords;
            BlockData.BlockPeopleRecords = new DFBlock.RmbBlockPeopleRecord[numPeopleRecords];
            for (int i = 0; i < numPeopleRecords; i++)
            {
                BlockData.BlockPeopleRecords[i].XPos = Reader.ReadInt32();
                BlockData.BlockPeopleRecords[i].YPos = Reader.ReadInt32();
                BlockData.BlockPeopleRecords[i].ZPos = Reader.ReadInt32();
                BlockData.BlockPeopleRecords[i].TextureBitfield = Reader.ReadUInt16();
                BlockData.BlockPeopleRecords[i].TextureArchive = BlockData.BlockPeopleRecords[i].TextureBitfield >> 7;
                BlockData.BlockPeopleRecords[i].TextureRecord = BlockData.BlockPeopleRecords[i].TextureBitfield & 0x7f;
                BlockData.BlockPeopleRecords[i].NpcType = Reader.ReadInt16();
                BlockData.BlockPeopleRecords[i].Unknown1 = Reader.ReadByte();
            }

            // Door records
            int numDoorRecords = BlockData.Header.NumDoorRecords;
            BlockData.BlockDoorRecords = new DFBlock.RmbBlockDoorRecord[numDoorRecords];
            for (int i = 0; i < numDoorRecords; i++)
            {
                BlockData.BlockDoorRecords[i].XPos = Reader.ReadInt32();
                BlockData.BlockDoorRecords[i].YPos = Reader.ReadInt32();
                BlockData.BlockDoorRecords[i].ZPos = Reader.ReadInt32();
                BlockData.BlockDoorRecords[i].Unknown1 = Reader.ReadInt16();
                BlockData.BlockDoorRecords[i].Unknown2 = Reader.ReadInt16();
                BlockData.BlockDoorRecords[i].Unknown3 = Reader.ReadInt16();
                BlockData.BlockDoorRecords[i].NullValue1 = Reader.ReadByte();
            }
        }

        /// <summary>
        /// Read a 3D object subrecord.
        /// </summary>
        /// <param name="Reader">A binary reader to file.</param>
        /// <param name="RecordsOut">Destination object.</param>
        private void ReadRmbModelRecords(ref BinaryReader Reader, ref DFBlock.RmbBlock3dObjectRecord[] RecordsOut)
        {
            // Read all 3d object records into array
            for (int i = 0; i < RecordsOut.Length; i++)
            {
                Int16 objectId1 = Reader.ReadInt16();
                Byte objectId2 = Reader.ReadByte();
                RecordsOut[i].ObjectId1 = objectId1;
                RecordsOut[i].ObjectId2 = objectId2;
                RecordsOut[i].ModelId = ((objectId1 * 100) + objectId2).ToString();
                RecordsOut[i].ModelIdNum = UInt32.Parse(RecordsOut[i].ModelId);
                RecordsOut[i].ObjectType = Reader.ReadByte();
                RecordsOut[i].Unknown1 = Reader.ReadUInt32();
                RecordsOut[i].Unknown2 = Reader.ReadUInt32();
                RecordsOut[i].Unknown3 = Reader.ReadUInt32();
                RecordsOut[i].NullValue1 = Reader.ReadUInt64();
                RecordsOut[i].XPos1 = Reader.ReadInt32();
                RecordsOut[i].YPos1 = Reader.ReadInt32();
                RecordsOut[i].ZPos1 = Reader.ReadInt32();
                RecordsOut[i].XPos = Reader.ReadInt32();
                RecordsOut[i].YPos = Reader.ReadInt32();
                RecordsOut[i].ZPos = Reader.ReadInt32();
                RecordsOut[i].NullValue2 = Reader.ReadUInt32();
                RecordsOut[i].YRotation = Reader.ReadInt16();
                RecordsOut[i].Unknown4 = Reader.ReadUInt16();
                RecordsOut[i].NullValue3 = Reader.ReadUInt32();
                RecordsOut[i].Unknown5 = Reader.ReadUInt32();
                RecordsOut[i].NullValue4 = Reader.ReadUInt16();
            }
        }

        /// <summary>
        /// Read a flat object subrecord.
        /// </summary>
        /// <param name="Reader">A binary reader to file.</param>
        /// <param name="RecordsOut">Destination object.</param>
        private void ReadRmbFlatObjectRecords(ref BinaryReader Reader, ref DFBlock.RmbBlockFlatObjectRecord[] RecordsOut)
        {
            // Read all flat object records into array
            for (int i = 0; i < RecordsOut.Length; i++)
            {
                RecordsOut[i].XPos = Reader.ReadInt32();
                RecordsOut[i].YPos = Reader.ReadInt32();
                RecordsOut[i].ZPos = Reader.ReadInt32();
                RecordsOut[i].TextureBitfield = Reader.ReadUInt16();
                RecordsOut[i].TextureArchive = RecordsOut[i].TextureBitfield >> 7;
                RecordsOut[i].TextureRecord = RecordsOut[i].TextureBitfield & 0x7f;
                RecordsOut[i].Unknown1 = Reader.ReadInt16();
                RecordsOut[i].Unknown2 = Reader.ReadByte();
            }
        }

        #endregion

        #region RDB Readers

        private void ReadRdbHeader(ref BinaryReader Reader, int Block)
        {
            // Read header
            Blocks[Block].DFBlock.RdbBlock.Header.Unknown1 = Reader.ReadUInt32();
            Blocks[Block].DFBlock.RdbBlock.Header.Width = Reader.ReadUInt32();
            Blocks[Block].DFBlock.RdbBlock.Header.Height = Reader.ReadUInt32();
            Blocks[Block].DFBlock.RdbBlock.Header.ObjectRootOffset = Reader.ReadUInt32();
            Blocks[Block].DFBlock.RdbBlock.Header.Unknown2 = Reader.ReadUInt32();
        }

        private void ReadRdbModelReferenceList(ref BinaryReader Reader, int Block)
        {
            // Read model reference list
            Blocks[Block].DFBlock.RdbBlock.ModelReferenceList = new DFBlock.RdbModelReference[750];
            for (int i = 0; i < 750; i++)
            {
                Blocks[Block].DFBlock.RdbBlock.ModelReferenceList[i].ModelId = Blocks[Block].MemoryFile.ReadCString(Reader, 5);
                UInt32.TryParse(Blocks[Block].DFBlock.RdbBlock.ModelReferenceList[i].ModelId, out Blocks[Block].DFBlock.RdbBlock.ModelReferenceList[i].ModelIdNum);
                Blocks[Block].DFBlock.RdbBlock.ModelReferenceList[i].Description = Blocks[Block].MemoryFile.ReadCString(Reader, 3);
            }
        }

        private void ReadRdbModelDataList(ref BinaryReader Reader, int Block)
        {
            // Read unknown model data list
            Blocks[Block].DFBlock.RdbBlock.ModelDataList = new DFBlock.RdbModelData[750];
            for (int i = 0; i < 750; i++)
            {
                Blocks[Block].DFBlock.RdbBlock.ModelDataList[i].Unknown1 = Reader.ReadUInt32();
            }
        }

        private void ReadRdbObjectSectionHeader(ref BinaryReader Reader, int Block)
        {
            // Read object section header
            Blocks[Block].DFBlock.RdbBlock.ObjectHeader.UnknownOffset = Reader.ReadUInt32();
            Blocks[Block].DFBlock.RdbBlock.ObjectHeader.Unknown1 = Reader.ReadUInt32();
            Blocks[Block].DFBlock.RdbBlock.ObjectHeader.Unknown2 = Reader.ReadUInt32();
            Blocks[Block].DFBlock.RdbBlock.ObjectHeader.Unknown3 = Reader.ReadUInt32();
            Blocks[Block].DFBlock.RdbBlock.ObjectHeader.Length = Reader.ReadUInt32();
            Blocks[Block].DFBlock.RdbBlock.ObjectHeader.Unknown4 = Reader.ReadBytes(32);
            Blocks[Block].DFBlock.RdbBlock.ObjectHeader.Dagr = Blocks[Block].MemoryFile.ReadCString(Reader, 4);
            Blocks[Block].DFBlock.RdbBlock.ObjectHeader.Unknown5 = Reader.ReadBytes(456);
        }

        private void ReadRdbObjectSectionRootList(ref BinaryReader Reader, int Block)
        {
            // Handle improper position in stream
            if (Reader.BaseStream.Position != Blocks[Block].DFBlock.RdbBlock.Header.ObjectRootOffset)
                throw(new Exception("Start of ObjectRoot section does not match header offset."));

            // Read object section root list
            UInt32 width = Blocks[Block].DFBlock.RdbBlock.Header.Width;
            UInt32 height = Blocks[Block].DFBlock.RdbBlock.Header.Height;
            Blocks[Block].DFBlock.RdbBlock.ObjectRootList = new DFBlock.RdbObjectRoot[width * height];
            for (int i = 0; i < width * height; i++)
            {
                Blocks[Block].DFBlock.RdbBlock.ObjectRootList[i].RootOffset = Reader.ReadInt32();
            }
        }

        private void ReadRdbObjectLists(ref BinaryReader Reader, int Block)
        {
            // Read all objects starting from each root position
            for (int i = 0; i < Blocks[Block].DFBlock.RdbBlock.ObjectRootList.Length; i++)
            {
                // Skip if no data present
                if (Blocks[Block].DFBlock.RdbBlock.ObjectRootList[i].RootOffset < 0)
                    continue;

                // Pre-count number of objects in linked list before allocating array, skip if no objects
                int objectCount = CountRdbObjects(ref Reader, ref Blocks[Block].DFBlock.RdbBlock.ObjectRootList[i]);
                if (objectCount == 0)
                    continue;

                // Create object array
                Blocks[Block].DFBlock.RdbBlock.ObjectRootList[i].RdbObjects = new DFBlock.RdbObject[objectCount];

                // Read object array
                ReadRdbObjects(ref Reader, ref Blocks[Block].DFBlock.RdbBlock.ObjectRootList[i]);
            }
        }

        private int CountRdbObjects(ref BinaryReader Reader, ref DFBlock.RdbObjectRoot ObjectRoot)
        {
            // Go to root of object linked list
            Reader.BaseStream.Position = ObjectRoot.RootOffset;

            // Count objects in list
            int objectCount = 0;
            while(true)
            {
                // Increment object count
                objectCount++;

                // Get next position and exit if finished
                long next = Reader.ReadInt32();
                if (next < 0) break;

                // Go to next offset in list
                Reader.BaseStream.Position = next;
            }

            return objectCount;
        }

        private void ReadRdbObjects(ref BinaryReader Reader, ref DFBlock.RdbObjectRoot ObjectRoot)
        {
            // Go to root of object linked list
            Reader.BaseStream.Position = ObjectRoot.RootOffset;

            // Read objects in list
            int index = 0;
            while (true)
            {
                // Read object data
                ObjectRoot.RdbObjects[index].Next = Reader.ReadInt32();
                ObjectRoot.RdbObjects[index].Previous = Reader.ReadInt32();
                ObjectRoot.RdbObjects[index].XPos = Reader.ReadInt32();
                ObjectRoot.RdbObjects[index].YPos = Reader.ReadInt32();
                ObjectRoot.RdbObjects[index].ZPos = Reader.ReadInt32();
                ObjectRoot.RdbObjects[index].Type = (DFBlock.RdbResourceTypes)Reader.ReadByte();
                ObjectRoot.RdbObjects[index].ResourceOffset = Reader.ReadUInt32();

                // Read resource-specific data
                switch (ObjectRoot.RdbObjects[index].Type)
                {
                    case DFBlock.RdbResourceTypes.Model:
                        ReadRdbModelResource(ref Reader, ref ObjectRoot.RdbObjects[index]);
                        break;

                    case DFBlock.RdbResourceTypes.Flat:
                        ReadRdbFlatResource(ref Reader, ref ObjectRoot.RdbObjects[index]);
                        break;

                    case DFBlock.RdbResourceTypes.Light:
                        ReadRdbLightResource(ref Reader, ref ObjectRoot.RdbObjects[index]);
                        break;

                    default:
                        throw (new Exception("Unknown RDB resource type encountered."));
                }

                // Exit if finished
                if (ObjectRoot.RdbObjects[index].Next < 0)
                    break;

                // Go to next offset in list
                Reader.BaseStream.Position = ObjectRoot.RdbObjects[index].Next;

                // Increment index
                index++;
            }
        }

        private void ReadRdbModelResource(ref BinaryReader Reader, ref DFBlock.RdbObject RdbObject)
        {
            // Go to resource offset
            Reader.BaseStream.Position = RdbObject.ResourceOffset;

            // Read model data
            RdbObject.Resources.ModelResource.XRotation = Reader.ReadInt32();
            RdbObject.Resources.ModelResource.YRotation = Reader.ReadInt32();
            RdbObject.Resources.ModelResource.ZRotation = Reader.ReadInt32();
            RdbObject.Resources.ModelResource.ModelIndex = Reader.ReadUInt16();
            RdbObject.Resources.ModelResource.Unknown1 = Reader.ReadUInt32();
            RdbObject.Resources.ModelResource.Unknown2 = Reader.ReadByte();
            RdbObject.Resources.ModelResource.ActionOffset = Reader.ReadInt32();

            // Read action data
            if (RdbObject.Resources.ModelResource.ActionOffset > 0)
                ReadRdbModelActionRecords(ref Reader, ref RdbObject);
        }

        private void ReadRdbModelActionRecords(ref BinaryReader Reader, ref DFBlock.RdbObject RdbObject)
        {
            // Go to action offset
            Reader.BaseStream.Position = RdbObject.Resources.ModelResource.ActionOffset;

            // Read action data
            RdbObject.Resources.ModelResource.ActionResource.Axis = (DFBlock.RdbActionAxes)Reader.ReadByte();
            RdbObject.Resources.ModelResource.ActionResource.Duration = Reader.ReadUInt16();
            RdbObject.Resources.ModelResource.ActionResource.Magnitude = Reader.ReadUInt16();
            RdbObject.Resources.ModelResource.ActionResource.TargetObjectOffset = Reader.ReadInt32();
            RdbObject.Resources.ModelResource.ActionResource.ActionType = (DFBlock.RdbActionType)Reader.ReadByte();
        }

        private void ReadRdbFlatResource(ref BinaryReader Reader, ref DFBlock.RdbObject RdbObject)
        {
            // Go to resource offset
            Reader.BaseStream.Position = RdbObject.ResourceOffset;

            // Read flat data
            RdbObject.Resources.FlatResource.TextureBitfield = Reader.ReadUInt16();
            RdbObject.Resources.FlatResource.TextureArchive = RdbObject.Resources.FlatResource.TextureBitfield >> 7;
            RdbObject.Resources.FlatResource.TextureRecord = RdbObject.Resources.FlatResource.TextureBitfield & 0x7f;
            RdbObject.Resources.FlatResource.Gender = (DFBlock.RdbFlatGenders)Reader.ReadUInt16();
            RdbObject.Resources.FlatResource.FactionId = Reader.ReadUInt16();
            RdbObject.Resources.FlatResource.Unknown1 = Reader.ReadBytes(5);
        }

        private void ReadRdbLightResource(ref BinaryReader Reader, ref DFBlock.RdbObject RdbObject)
        {
            // Go to resource offset
            Reader.BaseStream.Position = RdbObject.ResourceOffset;

            // Read light data
            RdbObject.Resources.LightResource.Unknown1 = Reader.ReadUInt32();
            RdbObject.Resources.LightResource.Unknown2 = Reader.ReadUInt32();
            RdbObject.Resources.LightResource.Unknown3 = Reader.ReadUInt16();
        }

        #endregion

        #region RDI Readers

        /// <summary>
        /// RDI data is currently an unknown format of 512 bytes in length.
        /// </summary>
        /// <param name="Reader">BinaryReader to start of data.</param>
        /// <param name="Block">Block index.</param>
        private void ReadRdiRecord(ref BinaryReader Reader, int Block)
        {
            // Each RDI block is 512 bytes of unknown data
            Blocks[Block].DFBlock.RdiBlock.Data = Reader.ReadBytes(512);
        }

        #endregion
    }
}
