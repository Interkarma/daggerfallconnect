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
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using DaggerfallConnect.Utility;

#endregion

namespace DaggerfallConnect.Arena2
{
    /// <summary>
    /// Reads data from WOODS.WLD.
    /// </summary>
    public class WoodsFile
    {
        #region Class Variables

        /// <summary>Width of heightmap in bytes.</summary>
        private const int MapWidthValue = 1000;

        /// <summary>Height of heightmap in bytes.</summary>
        private const int MapHeightValue = 500;

        /// <summary>Memory length of heightmap in bytes.</summary>
        private const int MapBufferLengthValue = MapWidthValue * MapHeightValue;

        /// <summary>
        /// Abstracts WOODS.WLD file to a managed disk or memory stream.
        /// </summary>
        private FileProxy ManagedFile = new FileProxy();

        /// <summary>
        /// Contains the WOODS.WLD file header data.
        /// </summary>
        private FileHeader Header;

        /// <summary>
        /// Offsets to CellData structures. There are 1000*500 offsets that correspond to
        ///  the standard 1000x500 world map structure.
        /// </summary>
        private UInt32[] DataOffsets;

        /// <summary>
        /// UNUSED.
        ///  Unknown data.
        /// </summary>
        private DataSection1 DataSection1Data;

        /// <summary>
        /// Height map data.
        /// </summary>
        private Byte[] HeightMapBuffer = new Byte[MapBufferLengthValue];

        #endregion

        #region Class Structures

        /// <summary>
        /// Represents WOODS.WLD file header.
        /// </summary>
        private struct FileHeader
        {
            public long Position;
            public UInt32 OffsetSize;
            public UInt32 Width;
            public UInt32 Height;
            public UInt32 NullValue1;
            public UInt32 DataSection1Offset;
            public UInt32 Unknown1;
            public UInt32 Unknown2;
            public UInt32 HeightMapOffset;
            public UInt32[] NullValue2;
        }

        /// <summary>
        /// UNUSED.
        ///  Represents DataSection1 data.
        ///  The purpose of this data is currently unknown.
        /// </summary>
        private struct DataSection1
        {
            public UInt32[] Unknown1;
        }

        /// <summary>
        /// NOT IMPLEMENTED.
        ///  Extended information per world cell.
        /// </summary>
        //private struct CellData
        //{
        //    public UInt16 Unknown1;
        //    public UInt32 NullValue1;
        //    public UInt16 FileIndex;
        //    public Byte Climate;
        //    public Byte ClimateNoise;
        //    public UInt32[] NullValue2;
        //    public Byte[][] ElevationNoise;
        //}

        #endregion

        #region Public Properties

        /// <summary>
        /// Width of heightmap data (always 1000).
        /// </summary>
        public int MapWidth
        {
            get { return MapWidthValue; }
        }

        /// <summary>
        /// Height of heightmap data (always 500).
        /// </summary>
        public int MapHeight
        {
            get { return MapHeightValue; }
        }

        /// <summary>
        /// Gets a copy of extracted heightmap data.
        /// </summary>
        public Byte[] Buffer
        {
            get { return HeightMapBuffer; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public WoodsFile()
        {
        }

        /// <summary>
        /// Load constructor.
        /// </summary>
        /// <param name="FilePath">Absolute path to WOODS.WLD.</param>
        /// <param name="Usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        public WoodsFile(string FilePath, FileUsage Usage, bool ReadOnly)
        {
            Load(FilePath, Usage, ReadOnly);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load WOODS.WLD file.
        /// </summary>
        /// <param name="FilePath">Absolute path to WOODS.WLD file.</param>
        /// <param name="Usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool Load(string FilePath, FileUsage Usage, bool ReadOnly)
        {
            // Validate filename
            FilePath = FilePath.ToUpper();
            if (!FilePath.EndsWith("WOODS.WLD"))
                return false;

            // Load file into memory
            if (!ManagedFile.Load(FilePath, Usage, ReadOnly))
                return false;

            // Read file
            if (!Read())
                return false;

            return true;
        }

        /// <summary>
        /// Get extracted heightmap data as an indexed image.
        /// </summary>
        /// <returns>DFBitmap object.</returns>
        public DFBitmap GetHeightMapDFBitmap()
        {
            DFBitmap DFBitmap = new DFBitmap();
            DFBitmap.Format = DFBitmap.Formats.Indexed;
            DFBitmap.Width = MapWidthValue;
            DFBitmap.Height = MapHeightValue;
            DFBitmap.Stride = MapWidthValue;
            DFBitmap.Data = HeightMapBuffer;
            return DFBitmap;
        }

        /// <summary>
        /// Get extracted heightmap data as a managed bitmap.
        /// </summary>
        /// <returns>Bitmap object.</returns>
        public Bitmap GetHeightMapManagedBitmap()
        {
            DFBitmap bmp = GetHeightMapDFBitmap();
            DFImageFile img = new ImgFile();
            DFPalette pal = new DFPalette();
            pal.MakeGrayscale();
            img.Palette = pal;
            return img.GetManagedBitmap(ref bmp, true, false);
        }

        /// <summary>
        /// Gets value for specified position in heightmap.
        /// </summary>
        /// <param name="x">X position in heightmap. 0 to MapWidth-1.</param>
        /// <param name="y">Y position in heightmap. 0 to MapHeight-1.</param>
        /// <returns>Value of heightmap data if valid, -1 if invalid.</returns>
        public int GetHeightMapValue(int x, int y)
        {
            // Validate
            if (x < 0 || x >= MapWidth) return -1;
            if (y < 0 || y >= MapHeight) return -1;

            return Buffer[(y * MapWidthValue) + x];
        }

        #endregion

        #region Private Methods
        #endregion

        #region Readers

        /// <summary>
        /// Read file.
        /// </summary>
        private bool Read()
        {
            try
            {
                // Step through file
                BinaryReader Reader = ManagedFile.GetReader();
                ReadHeader(Reader);
                ReadDataOffsets(Reader);
                ReadDataSection1(Reader);
                ReadHeightMap(Reader);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Read header data.
        /// </summary>
        /// <param name="Reader">Reader to stream.</param>
        private void ReadHeader(BinaryReader Reader)
        {
            // Read header
            Reader.BaseStream.Position = 0;
            Header.Position = 0;
            Header.OffsetSize = Reader.ReadUInt32();
            Header.Width = Reader.ReadUInt32();
            Header.Height = Reader.ReadUInt32();
            Header.NullValue1 = Reader.ReadUInt32();
            Header.DataSection1Offset = Reader.ReadUInt32();
            Header.Unknown1 = Reader.ReadUInt32();
            Header.Unknown2 = Reader.ReadUInt32();
            Header.HeightMapOffset = Reader.ReadUInt32();
            Header.NullValue2 = new UInt32[28];
            for (int i = 0; i < 28; i++)
                Header.NullValue2[i] = Reader.ReadUInt32();
        }

        /// <summary>
        /// Read data offsets.
        /// </summary>
        /// <param name="Reader">Reader to stream, positioned at start of offset data.</param>
        private void ReadDataOffsets(BinaryReader Reader)
        {
            // Validate
            if (Header.Width * Header.Height != MapBufferLengthValue)
                throw new Exception("Invalid WOODS.WLD Width*Height result from Header.");

            // Create offset array
            DataOffsets = new UInt32[MapBufferLengthValue];

            // Read offsets 
            for (int i = 0; i < MapBufferLengthValue; i++)
                DataOffsets[i] = Reader.ReadUInt32();
        }

        /// <summary>
        /// Read DataSection1 data.
        ///  The purpose of this data is currently unknown.
        /// </summary>
        /// <param name="Reader">Reader to stream.</param>
        private void ReadDataSection1(BinaryReader Reader)
        {
            // Position reader
            Reader.BaseStream.Position = Header.DataSection1Offset;

            // Read data
            DataSection1Data.Unknown1 = new UInt32[256];
            for (int i = 0; i < 256; i++)
                DataSection1Data.Unknown1[i] = Reader.ReadUInt32();
        }

        /// <summary>
        /// Read heightmap data.
        /// </summary>
        /// <param name="Reader">Reader to stream.</param>
        private void ReadHeightMap(BinaryReader Reader)
        {
            // Read heightmap data
            Reader.BaseStream.Position = Header.HeightMapOffset;
            HeightMapBuffer = Reader.ReadBytes(MapBufferLengthValue);
        }

        #endregion
    }
}
