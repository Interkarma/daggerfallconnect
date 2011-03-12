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
using System.Drawing;
using System.Drawing.Imaging;
using DaggerfallConnect.Utility;

#endregion

namespace DaggerfallConnect.Arena2
{
    /// <summary>
    /// Connects to CLIMATE.PAK or POLITIC.PAK to extract and read meta-data about the Daggerfall world map.
    /// </summary>
    public class PakFile
    {
        #region Class Variables

        /// <summary>Number of PAK rows.</summary>
        const int PakRowCountValue = 500;

        /// <summary>Length of each PAK row.</summary>
        const int PakRowLengthValue = 1001;

        /// <summary>Memory length of extracted PAK file.</summary>
        const int PakBufferLengthValue = PakRowLengthValue * PakRowCountValue;

        /// <summary>Abstracts PAK file to a managed disk or memory stream.</summary>
        private FileProxy ManagedFile = new FileProxy();

        /// <summary>Extracted PAK file buffer.</summary>
        private Byte[] PakExtractedBuffer = new Byte[PakBufferLengthValue];

        #endregion

        #region Public Properties

        /// <summary>
        /// Obtain a copy of extracted PAK data.
        /// </summary>
        public Byte[] Buffer
        {
            get { return PakExtractedBuffer; }
        }

        /// <summary>
        /// Number of rows in PAK file (always 500).
        /// </summary>
        public int PakRowCount
        {
            get { return PakRowCountValue; }
        }

        /// <summary>
        /// Number of bytes per PAK row (always 1001).
        /// </summary>
        public int PakRowLength
        {
            get { return PakRowLengthValue; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PakFile()
        {
        }

        /// <summary>
        /// Load constructor.
        /// </summary>
        /// <param name="FilePath">Absolute path to PAK file.</param>
        public PakFile(string FilePath)
        {
            Load(FilePath);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load PAK file.
        /// </summary>
        /// <param name="FilePath">Absolute path to PAK file.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool Load(string FilePath)
        {
            // Validate filename
            FilePath = FilePath.ToUpper();
            if (!FilePath.EndsWith("CLIMATE.PAK") && !FilePath.EndsWith("POLITIC.PAK"))
                return false;

            // Load file
            if (!ManagedFile.Load(FilePath, FileUsage.UseMemory, true))
                return false;

            // Expand each row of PAK file into buffer
            BinaryReader offsetReader = ManagedFile.GetReader(0);
            BinaryReader rowReader = ManagedFile.GetReader();
            for (int row = 0; row < PakRowCountValue; row++)
            {
                // Get offsets
                UInt32 offset = offsetReader.ReadUInt32();
                int bufferPos = PakRowLengthValue * row;
                rowReader.BaseStream.Position = offset;

                // Unroll PAK row into buffer
                int rowPos = 0;
                while (rowPos < PakRowLengthValue)
                {
                    // Get PakRun data
                    UInt16 count = rowReader.ReadUInt16();
                    Byte value = rowReader.ReadByte();

                    // Do PakRun
                    for (int c = 0; c < count; c++)
                    {
                        PakExtractedBuffer[bufferPos + rowPos++] = value;
                    }
                }
            }

            // Managed file is no longer needed
            ManagedFile.Close();

            return true;
        }

        /// <summary>
        /// Get extracted PAK data as an indexed image.
        /// </summary>
        /// <returns>DFBitmap object.</returns>
        public DFBitmap GetDFBitmap()
        {
            DFBitmap DFBitmap = new DFBitmap();
            DFBitmap.Format = DFBitmap.Formats.Indexed;
            DFBitmap.Width = PakRowLengthValue;
            DFBitmap.Height = PakRowCount;
            DFBitmap.Stride = PakRowLengthValue;
            DFBitmap.Data = PakExtractedBuffer;
            return DFBitmap;
        }

        /// <summary>
        /// Get extracted PAK data as a managed bitmap.
        /// </summary>
        /// <returns>Bitmap object.</returns>
        public Bitmap GetManagedBitmap()
        {
            DFBitmap bmp = GetDFBitmap();
            DFImageFile img = new ImgFile();
            DFPalette pal = new DFPalette();
            pal.MakeGrayscale();
            img.Palette = pal;
            return img.GetManagedBitmap(ref bmp, true, false);
        }

        /// <summary>
        /// Gets value for specified position in world map.
        /// </summary>
        /// <param name="x">X position in world map. 0 to PakRowLength-1.</param>
        /// <param name="y">Y position in world map. 0 to PakRowCount-1.</param>
        /// <returns>Value of pak data if valid, -1 if invalid.</returns>
        public int GetValue(int x, int y)
        {
            // Validate
            if (x < 0 || x >= PakRowLength) return -1;
            if (y < 0 || y >= PakRowCount) return -1;

            return Buffer[(y * PakRowLength) + x];
        }

        #endregion
    }
}
