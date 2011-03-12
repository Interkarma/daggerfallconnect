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
    /// Connects to a *.CIF or *.RCI file to enumerate and extract image data.
    ///  Each CIF file may contain one or more images, including animated records with multiple frames.
    ///  Each RCI file may contain one or more images, but never animated records.
    /// </summary>
    public class CifRciFile : DFImageFile
    {
        #region Class Variables

        /// <summary>
        /// Record array. This is pre-sized to 64 objects as CIF files do not contain a master header and
        ///  it's necessary to read through the file to count the number of image records.
        /// </summary>
        private Record[] Records = new Record[64];

        /// <summary>
        /// Total number of records in this file. Any records past this count in Records array are not valid.
        /// </summary>
        private int TotalRecords = 0;

        #endregion

        #region Class Structures

        /// <summary>
        /// Types of records found in this file.
        /// </summary>
        private enum RecordType
        {
            MultiImage,
            WeaponAnim,
        }

        /// <summary>
        /// Record data.
        /// </summary>
        private struct Record
        {
            public ImgFileHeader Header;
            public AnimationHeader AnimHeader;
            public RecordType FileType;
            public long AnimPixelDataPosition;
            public DFBitmap[] Frames;
        }

        /// <summary>
        /// Animation header for weapon files.
        /// </summary>
        private struct AnimationHeader
        {
            public long Position;
            public UInt16 Width;
            public UInt16 Height;
            public UInt16 LastFrameWidth;
            public Int16 XOffset;
            public Int16 LastFrameYOffset;
            public Int16 DataLength;
            public UInt16[] FrameDataOffsetList;
            public UInt16 TotalSize;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CifRciFile()
        {
        }

        /// <summary>
        /// Load constructor.
        /// </summary>
        /// <param name="FilePath">Absolute path to *.CIF or *.RCI file.</param>
        /// <param name="Usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        public CifRciFile(string FilePath, FileUsage Usage, bool ReadOnly)
        {
            Load(FilePath, Usage, ReadOnly);
        }

        /// <summary>
        /// Load constructor with palette assignment.
        /// </summary>
        /// <param name="FilePath">Absolute path to *.CIF or *.RCI file.</param>
        /// <param name="Usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="Palette">Palette to use when building images.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        public CifRciFile(string FilePath, FileUsage Usage, DFPalette Palette, bool ReadOnly)
        {
            MyPalette = Palette;
            Load(FilePath, Usage, ReadOnly);
        }

        /// <summary>
        /// Load constructor that also loads a palette.
        /// </summary>
        /// <param name="FilePath">Absolute path to *.CIF or *.RCI file.</param>
        /// <param name="Usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="PaletteFilePath">Absolute path to Daggerfall palette file.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        public CifRciFile(string FilePath, FileUsage Usage, string PaletteFilePath, bool ReadOnly)
        {
            LoadPalette(PaletteFilePath);
            Load(FilePath, Usage, ReadOnly);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Return correct palette name for this file (always ART_PAL.COL for CIF and RCI files).
        /// </summary>
        public override string PaletteName
        {
            get { return "ART_PAL.COL"; }
        }

        /// <summary>
        /// Number of image records in this Cif or Rci file.
        /// </summary>
        public override int RecordCount
        {
            get { return TotalRecords; }
        }

        /// <summary>
        /// Description of this file.
        /// </summary>
        public override string Description
        {
            get
            {
                if (FilePath.EndsWith(".CIF"))
                    return "CIF File";
                else if (FilePath.EndsWith(".RCI"))
                    return "RCI File";
                else
                    return "Unknown";
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads a CIF or RCI file.
        /// </summary>
        /// <param name="FilePath">Absolute path to *.CIF or *.RCI file</param>
        /// <param name="Usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public override bool Load(string FilePath, FileUsage Usage, bool ReadOnly)
        {
            // Exit if this file already loaded
            if (ManagedFile.FilePath == FilePath)
                return true;

            // Validate filename
            FilePath = FilePath.ToUpper();
            if (!FilePath.EndsWith(".CIF") && !FilePath.EndsWith(".RCI"))
                return false;

            // Load file
            if (!ManagedFile.Load(FilePath, Usage, ReadOnly))
                return false;

            // Read file
            if (!Read())
                return false;

            return true;
        }

        /// <summary>
        /// Gets number of frames in specified record.
        /// </summary>
        /// <param name="Record">Index of record.</param>
        /// <returns>Number of frames.</returns>
        public override int GetFrameCount(int Record)
        {
            // Validate
            if (Record < 0 || Record >= RecordCount)
                return 0;

            return Records[Record].Header.FrameCount;
        }

        /// <summary>
        /// Gets width and height of specified record. All frames of this record are the same dimensions.
        /// </summary>
        /// <param name="Record">Index of record.</param>
        /// <returns>Size object.</returns>
        public override Size GetSize(int Record)
        {
            // Validate
            if (Record < 0 || Record >= RecordCount)
                return new Size(0, 0);

            return new Size(Records[Record].Header.Width, Records[Record].Header.Height);
        }

        /// <summary>
        /// Gets bitmap data as indexed 8-bit byte array for specified record and frame.
        /// </summary>
        /// <param name="Record">Index of record.</param>
        /// <param name="Frame">Index of frame.</param>
        /// <returns>DFBitmap object.</returns>
        public override DFBitmap GetDFBitmap(int Record, int Frame)
        {
            // Validate
            if (Record < 0 || Record >= RecordCount || Frame >= GetFrameCount(Record))
                return new DFBitmap();

            // Read raw data from file
            if (!ReadImageData(Record, Frame))
                return new DFBitmap();

            return Records[Record].Frames[Frame];
        }

        /// <summary>
        /// Gets managed bitmap from specified record and frame.
        /// </summary>
        /// <param name="Record">Record index.</param>
        /// <param name="Frame">Frame index.</param>
        /// <param name="IndexedColour">True to maintain idexed colour, false to return RGB bitmap.</param>
        /// <param name="MakeTransparent">True to make colour 0x000000 transparent, otherwise false.</param>
        /// <returns>Bitmap object.</returns>
        public override Bitmap GetManagedBitmap(int Record, int Frame, bool IndexedColour, bool MakeTransparent)
        {
            // Validate
            if (Record < 0 || Record >= RecordCount || Frame >= GetFrameCount(Record))
                return new Bitmap(4, 4);

            // Read raw data from file
            if (!ReadImageData(Record, Frame))
                return new Bitmap(4, 4);

            // Call base method
            return base.GetManagedBitmap(ref Records[Record].Frames[Frame], IndexedColour, MakeTransparent);
        }

        /// <summary>
        /// Get a preview of the file. As many images as possible will be laid out onto the preview surface.
        /// </summary>
        /// <param name="Width">Width of preview surface.</param>
        /// <param name="Height">Height of preview surface.</param>
        /// <param name="Background">Colour of background.</param>
        /// <returns>Bitmap object.</returns>
        public override Bitmap GetPreview(int Width, int Height, Color Background)
        {
            // Setup
            int xpos = 0;
            int ypos = 0;
            int Record = 0;
            int RowMaxHeight = 0;
            Bitmap preview = new Bitmap(Width, Height);

            // Get graphics object
            Graphics gr = Graphics.FromImage(preview);
            gr.Clear(Background);

            do
            {
                // Exit if no more records
                if (Record >= RecordCount)
                    break;

                // Get this record and frame
                DFBitmap dfb = GetDFBitmap(Record++, 0);
                Bitmap bmp = GetManagedBitmap(ref dfb, true, false);

                // Update max height for this row
                if (dfb.Height > RowMaxHeight)
                    RowMaxHeight = dfb.Height;

                // Copy managed bitmap to preview frame
                Rectangle srcRect = new Rectangle(0, 0, dfb.Width, dfb.Height);
                Rectangle dstRect = new Rectangle(xpos, ypos, dfb.Width, dfb.Height);
                gr.DrawImage(bmp, dstRect, srcRect, GraphicsUnit.Pixel);

                // Increment X position with wrap around
                xpos += (dfb.Width + 1);
                if (xpos >= Width)
                {
                    xpos = 0;
                    ypos += (RowMaxHeight + 1);
                    RowMaxHeight = 0;
                }
            } while (xpos < Width && ypos < Height);

            return preview;
        }

        #endregion

        #region Readers

        /// <summary>
        /// Read file.
        /// </summary>
        /// <returns>True if succeeded, otherwise false.</returns>
        private bool Read()
        {
            try
            {
                // Step through file
                BinaryReader reader = ManagedFile.GetReader();
                ReadRecords(ref reader);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Reads records and formats Records array.
        /// </summary>
        /// <param name="Reader">Source reader.</param>
        private void ReadRecords(ref BinaryReader Reader)
        {
            // Go to start of file
            Reader.BaseStream.Position = 0;

            // Handle RCI files (faces.cif is actually an rci file)
            string fn = Path.GetFileName(ManagedFile.FilePath);
            if (fn.EndsWith(".RCI") || fn == "FACES.CIF")
            {
                ReadRci(ref Reader, fn);
                return;
            }

            // Handle WEAPON files
            if (fn.Contains("WEAPO"))
            {
                ReadWeaponCif(ref Reader, fn);
                return;
            }

            // Count number of single-frame records. This is just a contiguous array of single-frame IMG files
            int count = 0;
            do
            {
                // Read header data
                ReadImgFileHeader(ref Reader, ref Records[count].Header);

                // Set file type
                Records[count].FileType = RecordType.MultiImage;

                // Create empty frame object
                Records[count].Frames = new DFBitmap[1];

                // Increment past image data for now
                Reader.BaseStream.Position += Records[count].Header.PixelDataLength;

                // Increment count
                count++;
            } while (Reader.BaseStream.Position < Reader.BaseStream.Length);

            // Store count
            TotalRecords = count;
        }

        /// <summary>
        /// Special handling for RCI files.
        /// </summary>
        /// <param name="Reader">Source reader.</param>
        /// <param name="Filename">Name of this file without path.</param>
        private void ReadRci(ref BinaryReader Reader, string Filename)
        {
            // Set dimensions based on filename
            Size sz;
            switch (Filename)
            {
                case "FACES.CIF":
                case "CHLD00I0.RCI":
                    sz = new Size(64, 64);
                    break;
                case "TFAC00I0.RCI":
                    Records = new Record[503];  // Extend array as this file has hundreds of images
                    sz = new Size(64, 64);
                    break;
                case "BUTTONS.RCI":
                    sz = new Size(32, 16);
                    break;
                case "MPOP.RCI":
                    sz = new Size(17, 17);
                    break;
                case "NOTE.RCI":
                    sz = new Size(44, 9);
                    break;
                case "SPOP.RCI":
                    sz = new Size(22, 22);
                    break;
                default:
                    return;
            }

            // Get image count
            int count = ManagedFile.Length / (sz.Width * sz.Height);

            // Read count image records
            for (int i = 0; i < count; i++)
            {
                // Create empty frame object
                Records[i].Header.Position = Reader.BaseStream.Position;
                Records[i].Header.XOffset = 0;
                Records[i].Header.YOffset = 0;
                Records[i].Header.Width = (Int16)sz.Width;
                Records[i].Header.Height = (Int16)sz.Height;
                Records[i].Header.Compression = CompressionFormats.Uncompressed;
                Records[i].Header.PixelDataLength = (UInt16)(sz.Width * sz.Height);
                Records[i].Header.FrameCount = 1;
                Records[i].Header.DataPosition = Reader.BaseStream.Position;

                // Set record type
                Records[i].FileType = RecordType.MultiImage;

                // Increment past image data for now
                Reader.BaseStream.Position += Records[i].Header.PixelDataLength;

                // Create empty frame object
                Records[i].Frames = new DFBitmap[1];
            }

            // Store count
            TotalRecords = count;
        }

        /// <summary>
        /// Special handling for WEAPON CIF files.
        /// </summary>
        /// <param name="Reader">Source reader.</param>
        /// <param name="Filename">Name of this file without path.</param>
        private void ReadWeaponCif(ref BinaryReader Reader, string Filename)
        {
            // Read "wielding" image, except for WEAPO09.CIF (the bow) which has no such image
            int count = 0;
            if (Filename != "WEAPON09.CIF")
            {
                // Read header data
                ReadImgFileHeader(ref Reader, ref Records[count].Header);

                // Set record type (first image is just a standard IMG image)
                Records[count].FileType = RecordType.MultiImage;

                // Create empty frame object
                Records[count].Frames = new DFBitmap[1];

                // Increment past image data for now
                Reader.BaseStream.Position += Records[count].Header.PixelDataLength;

                // Increment count
                count++;
            }

            do
            {
                // Read animation header
                Records[count].AnimHeader.Position = Reader.BaseStream.Position;
                Records[count].AnimHeader.Width = Reader.ReadUInt16();
                Records[count].AnimHeader.Height = Reader.ReadUInt16();
                Records[count].AnimHeader.LastFrameWidth = Reader.ReadUInt16();
                Records[count].AnimHeader.XOffset = Reader.ReadInt16();
                Records[count].AnimHeader.LastFrameYOffset = Reader.ReadInt16();
                Records[count].AnimHeader.DataLength = Reader.ReadInt16();

                // Set record type
                Records[count].FileType = RecordType.WeaponAnim;

                // Read frame data offset list
                int FrameCount = 0;
                Records[count].AnimHeader.FrameDataOffsetList = new UInt16[31];
                for (int i = 0; i < 31; i++)
                {
                    Records[count].AnimHeader.FrameDataOffsetList[i] = Reader.ReadUInt16();
                    if (Records[count].AnimHeader.FrameDataOffsetList[i] != 0)
                        FrameCount++;
                }

                // Create empty frame objects
                Records[count].Header.FrameCount = FrameCount;
                Records[count].Frames = new DFBitmap[FrameCount];

                // Read total size
                Records[count].AnimHeader.TotalSize = Reader.ReadUInt16();

                // Get position of pixel data
                Records[count].AnimPixelDataPosition = Reader.BaseStream.Position;

                // Skip over pixel data for now
                Reader.BaseStream.Position = Records[count].AnimHeader.Position + Records[count].AnimHeader.TotalSize;

                // Increment count
                count++;
            } while (Reader.BaseStream.Position < Reader.BaseStream.Length);

            // Store count
            TotalRecords = count;
        }

        /// <summary>
        /// Reads image data for specified record and frame.
        /// </summary>
        /// <param name="Record">Record index.</param>
        /// <param name="Frame">Frame index.</param>
        /// <returns>True if succeeded, otherwise false.</returns>
        private bool ReadImageData(int Record, int Frame)
        {
            // Exit if this image already read
            if (Records[Record].Frames[Frame].Data != null)
                return true;

            // Handle weapon-type records
            if (Records[Record].FileType == RecordType.WeaponAnim)
                return ReadWeaponImage(Record, Frame);

            // Read based on compression type
            switch (Records[Record].Header.Compression)
            {
                case CompressionFormats.RleCompressed:
                    return ReadRleImage(Record, Frame);
                case CompressionFormats.Uncompressed:
                    return ReadImage(Record, Frame);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Read uncompressed record.
        /// </summary>
        /// <param name="Record">Record index.</param>
        /// <param name="Frame">Frame index.</param>
        /// <returns>True if succeeded, otherwise false.</returns>
        private bool ReadImage(int Record, int Frame)
        {
            // Setup frame to hold extracted image
            Records[Record].Frames[Frame].Width = Records[Record].Header.Width;
            Records[Record].Frames[Frame].Height = Records[Record].Header.Height;
            Records[Record].Frames[Frame].Stride = Records[Record].Header.Width;
            Records[Record].Frames[Frame].Format = DFBitmap.Formats.Indexed;
            Records[Record].Frames[Frame].Data = new byte[Records[Record].Header.PixelDataLength];

            // Read image bytes
            long position = Records[Record].Header.DataPosition;
            BinaryReader reader = ManagedFile.GetReader(position);
            BinaryWriter writer = new BinaryWriter(new MemoryStream(Records[Record].Frames[Frame].Data));
            writer.Write(reader.ReadBytes(Records[Record].Header.PixelDataLength));

            return true;
        }

        /// <summary>
        /// Reads image data for specified weapon-type record and frame.
        /// </summary>
        /// <param name="Record">Record index.</param>
        /// <param name="Frame">Frame index.</param>
        /// <returns>True if succeeded, otherwise false.</returns>
        private bool ReadWeaponImage(int Record, int Frame)
        {
            // Setup frame to hold extracted image
            int length = Records[Record].AnimHeader.Width * Records[Record].AnimHeader.Height;
            Records[Record].Frames[Frame].Width = Records[Record].AnimHeader.Width;
            Records[Record].Frames[Frame].Height = Records[Record].AnimHeader.Height;
            Records[Record].Frames[Frame].Stride = Records[Record].AnimHeader.Width;
            Records[Record].Frames[Frame].Format = DFBitmap.Formats.Indexed;
            Records[Record].Frames[Frame].Data = new byte[length];

            // Extract image data from frame RLE
            long position = Records[Record].AnimHeader.Position + Records[Record].AnimHeader.FrameDataOffsetList[Frame];
            BinaryReader reader = ManagedFile.GetReader(position);
            BinaryWriter writer = new BinaryWriter(new MemoryStream(Records[Record].Frames[Frame].Data));
            ReadRleData(ref reader, length, ref writer);

            return true;
        }

        /// <summary>
        /// Read a RLE record.
        /// </summary>
        /// <param name="Record">Record index.</param>
        /// <param name="Frame">Frame index.</param>
        /// <returns>True if succeeded, otherwise false.</returns>
        private bool ReadRleImage(int Record, int Frame)
        {
            // Setup frame to hold extracted image
            int length = Records[Record].Header.Width * Records[Record].Header.Height;
            Records[Record].Frames[Frame].Width = Records[Record].Header.Width;
            Records[Record].Frames[Frame].Height = Records[Record].Header.Height;
            Records[Record].Frames[Frame].Stride = Records[Record].Header.Width;
            Records[Record].Frames[Frame].Format = DFBitmap.Formats.Indexed;
            Records[Record].Frames[Frame].Data = new byte[length];

            // Extract image data from RLE
            long position = Records[Record].Header.DataPosition;
            BinaryReader reader = ManagedFile.GetReader(position);
            BinaryWriter writer = new BinaryWriter(new MemoryStream(Records[Record].Frames[Frame].Data));
            ReadRleData(ref reader, length, ref writer);

            return true;
        }

        /// <summary>
        /// Reads RLE compressed data from source reader to destination writer.
        /// </summary>
        /// <param name="Reader">Source reader positioned at start of input data.</param>
        /// <param name="Length">Length of source data.</param>
        /// <param name="Writer">Destination writer positioned at start of output data.</param>
        /// <returns>True if succeeded, otherwise false.</returns>
        private void ReadRleData(ref BinaryReader Reader, int Length, ref BinaryWriter Writer)
        {
            // Read image bytes
            byte pixel = 0;
            byte code = 0;
            int pos = 0;
            do
            {
                code = Reader.ReadByte();
                if (code > 127)
                {
                    pixel = Reader.ReadByte();
                    for (int i = 0; i < code - 127; i++)
                    {
                        Writer.Write(pixel);
                        pos++;
                    }
                }
                else
                {
                    Writer.Write(Reader.ReadBytes(code + 1));
                    pos += (code + 1);
                }
            } while (pos < Length);
        }

        #endregion
    }
}
