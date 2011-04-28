// Project:         DaggerfallConnect
// Description:     Read data from Daggerfall's file formats.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
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
    /// Connects to a TEXTURE.??? file to enumerate and extract image data.
    ///  Each texture file may contain one or more images, including animated records with multiple frames.
    ///  Textures will only be converted from the source binary file when needed. This allows you to extract
    ///  individual records and frames without the overhead of converting unwanted images.
    ///  Combine this with a texture caching scheme when loading large 3D scenes to avoid unnecessary load time.
    /// </summary>
    public class TextureFile : DFImageFile
    {
        #region Class Variables

        /// <summary>
        /// Width and height of solid images.
        /// </summary>
        private const int SolidSize = 32;

        /// <summary>
        /// Type of solid image.
        /// </summary>
        private SolidTypes SolidType = SolidTypes.None;

        /// <summary>
        /// File header.
        /// </summary>
        private FileHeader Header;

        /// <summary>
        /// Record header array
        /// </summary>
        private RecordHeader[] RecordHeaders;

        /// <summary>
        /// Record array.
        /// </summary>
        private Record[] Records;

        #endregion

        #region Class Structures

        /// <summary>
        /// Solid types enumeration.
        /// </summary>
        private enum SolidTypes
        {
            None,
            SolidColoursA,
            SolidColoursB,
        }

        /// <summary>
        /// Row encoding used for the SpecialFrameHeader of RecordRle archives.
        /// </summary>
        private enum RowEncoding
        {
            IsRleEncoded = 0x8000,
            NotRleEncoded = 0,
        }

        /// <summary>
        /// File header.
        /// </summary>
        private struct FileHeader
        {
            public long Position;
            public Int16 RecordCount;
            public String Name;
        }

        /// <summary>
        /// Record header.
        /// </summary>
        private struct RecordHeader
        {
            public long Position;
            public Int16 Type1;
            public Int32 RecordPosition;
            public Int16 Type2;
            public Int32 Unknown1;
            public Int64 NullValue1;
        }

        /// <summary>
        /// Record data.
        /// </summary>
        private struct Record
        {
            public long Position;
            public Int16 OffsetX;
            public Int16 OffsetY;
            public Int16 Width;
            public Int16 Height;
            public CompressionFormats Compression;
            public UInt32 RecordSize;
            public UInt32 DataOffset;
            public Boolean IsNormal;
            public UInt16 FrameCount;
            public Int16 Unknown1;
            public Int16 ScaleX;
            public Int16 ScaleY;
            public DFBitmap[] Frames;
        }

        /// <summary>
        /// Used to decode RecordRle archives.
        /// </summary>
        private struct SpecialRowHeader
        {
            public Int16 RowOffset;
            public RowEncoding RowEncoding;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TextureFile()
        {
        }

        /// <summary>
        /// Load constructor.
        /// </summary>
        /// <param name="FilePath">Absolute path to TEXTURE.* file.</param>
        /// <param name="Usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        public TextureFile(string FilePath, FileUsage Usage, bool ReadOnly)
        {
            Load(FilePath, Usage, ReadOnly);
        }

        /// <summary>
        /// Load constructor with palette assignment.
        /// </summary>
        /// <param name="FilePath">Absolute path to TEXTURE.* file.</param>
        /// <param name="Usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="Palette">Palette to use when building images.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        public TextureFile(string FilePath, FileUsage Usage, DFPalette Palette, bool ReadOnly)
        {
            MyPalette = Palette;
            Load(FilePath, Usage, ReadOnly);
        }

        /// <summary>
        /// Load constructor that also loads a palette.
        /// </summary>
        /// <param name="FilePath">Absolute path to TEXTURE.* file.</param>
        /// <param name="Usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="PaletteFilePath">Absolute path to Daggerfall palette file.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        public TextureFile(string FilePath, FileUsage Usage, string PaletteFilePath, bool ReadOnly)
        {
            Load(FilePath, Usage, ReadOnly);
            LoadPalette(PaletteFilePath);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets description of texture file.
        /// </summary>
        public override string Description
        {
            get {return Header.Name;}
        }

        /// <summary>
        /// Gets unsupported filenames. The three texture filenames returned have no image data or are otherwise invalid.
        /// </summary>
        /// <returns>Array of unsupported filenames.</returns>
        public static string[] UnsupportedFilenames
        {
            get
            {
                string[] names = new string[3];
                names[0] = "TEXTURE.215";
                names[1] = "TEXTURE.217";
                names[2] = "TEXTURE.436";
                return names;
            }
        }

        /// <summary>
        /// Gets correct palette name for this file (always ART_PAL.COL for texture files).
        /// </summary>
        public override string PaletteName
        {
            get { return "ART_PAL.COL"; }
        }

        /// <summary>
        /// Number of image records in this file.
        /// </summary>
        public override int RecordCount
        {
            get { return Header.RecordCount; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Tests if a filename is supported.
        /// </summary>
        /// <param name="Filename">Name of TEXTURE.* file.</param>
        /// <returns>True if supported, otherwise false.</returns>
        public bool IsFilenameSupported(string Filename)
        {
            // Look for filename in list of unsupported filenames
            string[] names = UnsupportedFilenames;
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == Filename)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Loads a texture file.
        /// </summary>
        /// <param name="FilePath">Absolute path to TEXTURE.* file</param>
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
            string fn = Path.GetFileName(FilePath);
            if (!fn.StartsWith("TEXTURE."))
                return false;
            
            // Handle unsupported files
            if (!IsFilenameSupported(fn))
            {
                Console.WriteLine(string.Format("{0} is unsupported.", fn));
                return false;
            }

            // Handle solid types
            if (fn == "TEXTURE.000")
                SolidType = SolidTypes.SolidColoursA;
            else if (fn == "TEXTURE.001")
                SolidType = SolidTypes.SolidColoursB;
            else
                SolidType = SolidTypes.None;

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
            if (Record < 0 || Record >= Header.RecordCount || Records == null)
                return -1;

            // Handle solid types
            if (SolidType != SolidTypes.None)
                return 1;

            return Records[Record].FrameCount;
        }

        /// <summary>
        /// Gets width and height of specified record. All frames of this record are the same dimensions.
        /// </summary>
        /// <param name="Record">Index of record.</param>
        /// <returns>Size object.</returns>
        public override Size GetSize(int Record)
        {
            // Validate
            if (Record < 0 || Record >= Header.RecordCount || Records == null)
                return new Size(0, 0);

            // Handle solid types
            if (SolidType != SolidTypes.None)
                return new Size(SolidSize, SolidSize);

            return new Size(Records[Record].Width, Records[Record].Height);
        }

        /// <summary>
        /// Get the width and height scale to apply to image in scene. These values are divided by 256
        ///  to obtain a value between -1.0 - 0.0, and presumably 0.0 - 1.0. This is the scale of pixels
        ///  for enlarging or shrinking the image.
        /// </summary>
        /// <param name="Record">Index of record.</param>
        /// <returns>Scale values for X and Y in Size object.</returns>
        public Size GetScale(int Record)
        {
            // Validate
            if (Record < 0 || Record >= Header.RecordCount || Records == null)
                return new Size(0, 0);

            return new Size(Records[Record].ScaleX, Records[Record].ScaleX);
        }

        /// <summary>
        /// Gets width of this image.
        /// </summary>
        /// <param name="Record">Index of record.</param>
        /// <returns>Width of image in pixels.</returns>
        public int GetWidth(int Record)
        {
            // Validate
            if (Record < 0 || Record >= Header.RecordCount || Records == null)
                return 0;

            // Handle solid types
            if (SolidType != SolidTypes.None)
                return SolidSize;

            return Records[Record].Width;
        }

        /// <summary>
        /// Gets the height of this image.
        /// </summary>
        /// <param name="Record">Index of record.</param>
        /// <returns>Height of image in pixels.</returns>
        public int GetHeight(int Record)
        {
            // Validate
            if (Record < 0 || Record >= Header.RecordCount || Records == null)
                return 0;

            // Handle solid types
            if (SolidType != SolidTypes.None)
                return SolidSize;

            return Records[Record].Height;
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
            if (Record < 0 || Record >= Header.RecordCount || Records == null || Frame >= GetFrameCount(Record))
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
            if (Record < 0 || Record >= Header.RecordCount || Records == null || Frame >= GetFrameCount(Record))
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

        #region Static Methods

        /// <summary>
        /// Returns a TEXTURE.nnn filename based on index.
        ///  This is needed when loading textures for 3D objects that reference textures by index rather than filename.
        ///  If the index is not valid, the returned filename will also be invalid.
        /// </summary>
        /// <param name="ArchiveIndex">Index of texture archive.</param>
        /// <returns>Texture filename in the format TEXTURE.nnn.</returns>
        public static string IndexToFileName(int ArchiveIndex)
        {
            return string.Format("TEXTURE.{0:000}", ArchiveIndex);
        }

        /// <summary>
        /// Gets size of an unloaded texture quickly with minimum overhead.
        ///  This is useful for mesh loading where the texture dimensions need to be known,
        ///  but you may not need to load the texture at that time.
        /// </summary>
        /// <param name="FilePath">Absolute path to TEXTURE.* file</param>
        /// <param name="Record">Index of record.</param>
        /// <returns>Size.</returns>
        public static Size QuickSize(string FilePath, int Record)
        {
            FileStream fs;
            try
            {
                fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new Size(0, 0);
            }

            // Read record count and check range
            BinaryReader reader = new BinaryReader(fs);
            int recordCount = reader.ReadInt16();
            if (Record < 0 || Record >= recordCount)
                return new Size(0, 0);

            // Offset to width and height
            reader.BaseStream.Position = 26 + 20 * Record + 2;
            reader.BaseStream.Position = reader.ReadInt32() + 4;

            // Read width and height
            int width = reader.ReadInt16();
            int height = reader.ReadInt16();

            // Close reader and stream
            reader.Close();

            // Return size
            return new Size(width, height);
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
                ReadHeader(ref reader);
                ReadRecordHeaders(ref reader);
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
        /// Reads file header.
        /// </summary>
        /// <param name="Reader">Source reader.</param>
        private void ReadHeader(ref BinaryReader Reader)
        {
            Reader.BaseStream.Position = 0;
            Header.Position = 0;
            Header.RecordCount = Reader.ReadInt16();
            Header.Name = ManagedFile.ReadCString(Reader, 0).Trim();
        }

        /// <summary>
        /// Reads record headers.
        /// </summary>
        /// <param name="Reader">Source reader.</param>
        private void ReadRecordHeaders(ref BinaryReader Reader)
        {
            Reader.BaseStream.Position = 26;
            RecordHeaders = new RecordHeader[Header.RecordCount];
            for (int r = 0; r < Header.RecordCount; r++)
            {
                RecordHeaders[r].Position = Reader.BaseStream.Position;
                RecordHeaders[r].Type1 = Reader.ReadInt16();
                RecordHeaders[r].RecordPosition = Reader.ReadInt32();
                RecordHeaders[r].Type2 = Reader.ReadInt16();
                RecordHeaders[r].Unknown1 = Reader.ReadInt32();
                RecordHeaders[r].NullValue1 = Reader.ReadInt64();
            }
        }

        /// <summary>
        /// Reads records.
        /// </summary>
        /// <param name="Reader">Source reader.</param>
        private void ReadRecords(ref BinaryReader Reader)
        {
            Records = new Record[Header.RecordCount];
            for (int r = 0; r < Header.RecordCount; r++)
            {
                Reader.BaseStream.Position = RecordHeaders[r].RecordPosition;
                Records[r].Position = Reader.BaseStream.Position;
                Records[r].OffsetX = Reader.ReadInt16();
                Records[r].OffsetY = Reader.ReadInt16();
                Records[r].Width = Reader.ReadInt16();
                Records[r].Height = Reader.ReadInt16();
                Records[r].Compression = (CompressionFormats)Reader.ReadInt16();
                Records[r].RecordSize = (UInt32)Reader.ReadInt32();
                Records[r].DataOffset = (UInt32)Reader.ReadInt32();
                Records[r].IsNormal = Convert.ToBoolean(Reader.ReadInt16());
                Records[r].FrameCount = (UInt16)Reader.ReadInt16();
                Records[r].Unknown1 = Reader.ReadInt16();
                Records[r].ScaleX = Reader.ReadInt16();
                Records[r].ScaleY = Reader.ReadInt16();

                // Create frame array
                if (SolidTypes.None == SolidType)
                {
                    Records[r].Frames = new DFBitmap[Records[r].FrameCount];
                }
                else
                {
                    Records[r].Width = SolidSize;
                    Records[r].Height = SolidSize;
                    Records[r].Frames = new DFBitmap[1];
                }
            }
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

            // Handle solid types
            if (SolidType != SolidTypes.None)
                return ReadSolid(Record);

            // Read based on compression type
            switch (Records[Record].Compression)
            {
                case CompressionFormats.RecordRle:
                case CompressionFormats.ImageRle:
                    return ReadRle(Record, Frame);
                case CompressionFormats.Uncompressed:
                default:
                    return ReadImage(Record, Frame);
            }
        }

        /// <summary>
        /// Create a solid image type.
        /// </summary>
        /// <param name="Record">Record index.</param>
        /// <returns>True if succeeded, otherwise false.</returns>
        private bool ReadSolid(int Record)
        {
            // Set start colour index
            byte colourIndex;
            if (SolidType == SolidTypes.SolidColoursA)
                colourIndex = (byte)Record;
            else
                colourIndex = (byte)(128 + Record);

            // Create buffer to hold extracted image
            Records[Record].Frames[0].Width = SolidSize;
            Records[Record].Frames[0].Height = SolidSize;
            Records[Record].Frames[0].Data = new byte[SolidSize * SolidSize];

            // Write image bytes
            int srcPos = 0;
            byte[] srcData = Records[Record].Frames[0].Data;
            for (int i = 0; i < SolidSize * SolidSize; i++)
            {
                srcData[srcPos++] = colourIndex;
            }

            return true;
        }

        /// <summary>
        /// Read uncompressed record.
        /// </summary>
        /// <param name="Record">Record index.</param>
        /// <param name="Frame">Frame index.</param>
        /// <returns>True if succeeded, otherwise false.</returns>
        private bool ReadImage(int Record, int Frame)
        {
            // Create buffer to hold extracted image
            Records[Record].Frames[Frame].Width = Records[Record].Width;
            Records[Record].Frames[Frame].Height = Records[Record].Height;
            Records[Record].Frames[Frame].Stride = Records[Record].Width;
            Records[Record].Frames[Frame].Format = DFBitmap.Formats.Indexed;
            Records[Record].Frames[Frame].Data = new byte[Records[Record].Width * Records[Record].Height];

            if (Records[Record].FrameCount == 1)
            {
                // Extract image bytes
                long position = Records[Record].Position + Records[Record].DataOffset;
                BinaryReader reader = ManagedFile.GetReader(position);
                BinaryWriter writer = new BinaryWriter(new MemoryStream(Records[Record].Frames[Frame].Data));
                for (int y = 0; y < Records[Record].Height; y++)
                {
                    writer.Write(reader.ReadBytes(Records[Record].Width));
                    reader.BaseStream.Position += (256 - Records[Record].Width);
                }
            }
            else if (Records[Record].FrameCount > 1)
            {
                // Get frame offset list
                Int32[] offsets = new Int32[Records[Record].FrameCount];
                long position = Records[Record].Position + Records[Record].DataOffset;
                BinaryReader reader = ManagedFile.GetReader(position);
                for (int offset = 0; offset < Records[Record].FrameCount; offset++)
                    offsets[offset] = reader.ReadInt32();

                // Offset to desired frame
                reader.BaseStream.Position = position + offsets[Frame];
                int cx = reader.ReadInt16();
                int cy = reader.ReadInt16();

                // Extract image bytes
                BinaryWriter writer = new BinaryWriter(new MemoryStream(Records[Record].Frames[Frame].Data));
                for (int y = 0; y < cy; y++)
                {
                    int x = 0;
                    while (x < cx)
                    {
                        // Write transparant bytes
                        byte pixel = reader.ReadByte();
                        int run = x + pixel;
                        for (; x < run; x++)
                        {
                            writer.Write((byte)0);
                        }

                        // Write image bytes
                        pixel = reader.ReadByte();
                        run = x + pixel;
                        for (; x < run; x++)
                        {
                            pixel = reader.ReadByte();
                            writer.Write(pixel);
                        }
                    }
                }
            }
            else
            {
                // No frames
                return false;
            }

            return true;
        }

        /// <summary>
        /// Read a RecordRle record.
        /// </summary>
        /// <param name="Record">Record index.</param>
        /// <param name="Frame">Frame index.</param>
        /// <returns>True if succeeded, otherwise false.</returns>
        private bool ReadRle(int Record, int Frame)
        {
            // Create buffer to hold extracted image
            Records[Record].Frames[Frame].Width = Records[Record].Width;
            Records[Record].Frames[Frame].Height = Records[Record].Height;
            Records[Record].Frames[Frame].Stride = Records[Record].Width;
            Records[Record].Frames[Frame].Format = DFBitmap.Formats.Indexed;
            Records[Record].Frames[Frame].Data = new byte[Records[Record].Width * Records[Record].Height];

            // Find offset to special row headers for this frame
            long position = Records[Record].Position + Records[Record].DataOffset;
            position += (Records[Record].Height * Frame) * 4;
            BinaryReader Reader = ManagedFile.GetReader(position);

            // Read special row headers for this frame
            SpecialRowHeader[] SpecialRowHeaders = new SpecialRowHeader[Records[Record].Height];
            for (int i = 0; i < Records[Record].Height; i++)
            {
                SpecialRowHeaders[i].RowOffset = Reader.ReadInt16();
                SpecialRowHeaders[i].RowEncoding = (RowEncoding)Reader.ReadUInt16();
            }

            // Create row memory writer
            BinaryWriter writer = new BinaryWriter(new MemoryStream(Records[Record].Frames[Frame].Data));

            // Extract all rows of image
            foreach(SpecialRowHeader header in SpecialRowHeaders)
            {
                // Get offset to row relative to record data offset
                position = Records[Record].Position + header.RowOffset;
                Reader.BaseStream.Position = position;

                // Handle row data based on compression
                if (RowEncoding.IsRleEncoded == header.RowEncoding)
                {
                    // Extract RLE row
                    byte pixel = 0;
                    int probe = 0;
                    int rowPos = 0;
                    int rowWidth = Reader.ReadUInt16();
                    do
                    {
                        probe = Reader.ReadInt16();
                        if (probe < 0)
                        {
                            probe = -probe;
                            pixel = Reader.ReadByte();
                            for (int i = 0; i < probe; i++)
                            {
                                writer.Write(pixel);
                                rowPos++;
                            }
                        }
                        else if (0 < probe)
                        {
                            writer.Write(Reader.ReadBytes(probe));
                            rowPos += probe;
                        }
                    } while (rowPos < rowWidth);
                }
                else
                {
                    // Just copy bytes
                    writer.Write(Reader.ReadBytes(Records[Record].Width));
                }
            }

            return true;
        }

        #endregion
    }
}
