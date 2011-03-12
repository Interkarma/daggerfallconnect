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
    /// Connects to a *.IMG file to enumerate and extract image data. Each IMG file contains a single image.
    /// </summary>
    public class ImgFile : DFImageFile
    {
        #region Class Variables

        /// <summary>
        /// File header.
        /// </summary>
        private ImgFileHeader Header;

        /// <summary>
        /// The image data for this image. Each IMG file only contains a single image.
        /// </summary>
        private DFBitmap ImgRecord;

        /// <summary>
        /// Specifies if this IMG file defines its own palette.
        /// </summary>
        private bool IsPalettizedValue = false;

        /// <summary>
        /// Start of palettized data in file. Only valid when IsPalettized is true;
        /// </summary>
        private long PaletteDataPosition = -1;

        /// <summary>
        /// Start of image data in file.
        /// </summary>
        private long ImageDataPosition = -1;

        #endregion

        #region Class Structures

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ImgFile()
        {
        }

        /// <summary>
        /// Load constructor.
        /// </summary>
        /// <param name="FilePath">Absolute path to *.IMG file.</param>
        /// <param name="Usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        public ImgFile(string FilePath, FileUsage Usage, bool ReadOnly)
        {
            Load(FilePath, Usage, ReadOnly);
        }

        /// <summary>
        /// Load constructor with palette assignment.
        /// Some IMG files contain palette information, this will overwrite the specified palette.
        /// </summary>
        /// <param name="FilePath">Absolute path to *.IMG file.</param>
        /// <param name="Usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="Palette">Palette to use when building images.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        public ImgFile(string FilePath, FileUsage Usage, DFPalette Palette, bool ReadOnly)
        {
            MyPalette = Palette;
            Load(FilePath, Usage, ReadOnly);
        }

        /// <summary>
        /// Load constructor that also loads a palette.
        /// Some IMG files contain palette information, this will overwrite the specified palette.
        /// </summary>
        /// <param name="FilePath">Absolute path to *.IMG file.</param>
        /// <param name="Usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="PaletteFilePath">Absolute path to Daggerfall palette file.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        public ImgFile(string FilePath, FileUsage Usage, string PaletteFilePath, bool ReadOnly)
        {
            LoadPalette(PaletteFilePath);
            Load(FilePath, Usage, ReadOnly);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Specifies if this IMG file defines its own palette.
        /// </summary>
        public bool IsPalettized
        {
            get { return IsPalettizedValue; }
        }

        /// <summary>
        /// IMG files use a variety of palettes. This property returns the correct palette filename to use for this image.
        ///  Palettized images (check IsPalettized flag) will return String.Emtpy for the palette filename.
        /// </summary>
        public override string PaletteName
        {
            get
            {
                // No palette if file is palettized
                if (IsPalettized)
                    return string.Empty;

                // Return based on source filename
                string fn = Path.GetFileName(ManagedFile.FilePath);
                if (fn == "DANK02I0.IMG")
                    return "DANKBMAP.COL";
                else if (fn.Substring(0, 4) == "FMAP")
                    return "FMAP_PAL.COL";
                else if (fn.Substring(0, 4) == "NITE")
                    return "NIGHTSKY.COL";
                else
                    return "ART_PAL.COL";
            }
        }

        /// <summary>
        /// Number of image records in this Img file.
        /// </summary>
        public override int RecordCount
        {
            get
            {
                if (ManagedFile.FilePath == string.Empty)
                    return 0;
                else
                    return 1;
            }

        }

        /// <summary>
        /// Description of this file (always "IMG File" as the game files contain no text descriptions for this file type).
        /// </summary>
        public override string Description
        {
            get { return "IMG File"; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets unsupported filenames. The three IMG filenames returned have no image data or are otherwise invalid.
        /// </summary>
        /// <returns>Array of unsupported filenames.</returns>
        public string[] UnsupportedFilenames()
        {
            string[] names = new string[3];
            names[0] = "FMAP0I00.IMG";
            names[1] = "FMAP0I01.IMG";
            names[2] = "FMAP0I16.IMG";
            return names;
        }

        /// <summary>
        /// Tests if a filename is supported.
        /// </summary>
        /// <param name="Filename">Name of *.IMG file.</param>
        /// <returns>True if supported, otherwise false.</returns>
        public bool IsFilenameSupported(string Filename)
        {
            // Look for filename in list of unsupported filenames
            string[] names = UnsupportedFilenames();
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == Filename)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Loads an IMG file.
        /// </summary>
        /// <param name="FilePath">Absolute path to *.IMG file</param>
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
            if (!fn.EndsWith(".IMG"))
                return false;

            // Handle unsupported files
            if (!IsFilenameSupported(fn))
            {
                Console.WriteLine(string.Format("{0} is unsupported.", fn));
                return false;
            }

            // Load file
            if (!ManagedFile.Load(FilePath, Usage, ReadOnly))
                return false;

            // Read file
            if (!Read())
                return false;

            return true;
        }

        /// <summary>
        /// Gets bitmap data as indexed 8-bit byte array.
        /// </summary>
        /// <returns>DFBitmap object.</returns>
        public DFBitmap GetDFBitmap()
        {
            // Read image data
            if (!ReadImageData())
                return new DFBitmap();

            return ImgRecord;
        }

        /// <summary>
        /// Gets bitmap data as indexed 8-bit byte array for specified record and frame.
        ///  This method is an override for abstract method in parent class DFImageFile.
        ///  As IMG files only have 1 image the Record and Frame indices must always be 0
        ///  when calling this method.
        /// </summary>
        /// <param name="Record">Index of record. Must be 0.</param>
        /// <param name="Frame">Index of frame. Must be 0.</param>
        /// <returns>DFBitmap object.</returns>
        public override DFBitmap GetDFBitmap(int Record, int Frame)
        {
            // Validate
            if (0 != Record || 0 != Frame)
                return new DFBitmap();

            return GetDFBitmap();
        }

        /// <summary>
        /// Gets number of frames in specified record.
        /// </summary>
        /// <param name="Record">Index of record. Must be 0 for Img files.</param>
        /// <returns>Number of frames. Always 1 for loaded Img files.</returns>
        public override int GetFrameCount(int Record)
        {
            // Validate
            if (Record != 0)
                return -1;

            // Read image data
            if (!ReadImageData())
                return -1;

            return 1;
        }

        /// <summary>
        /// Gets width and height of specified record. As IMG files only have 1 record,
        ///  this must be 0.
        /// </summary>
        /// <param name="Record">Index of record. Must be 0.</param>
        /// <returns>Size object.</returns>
        public override Size GetSize(int Record)
        {
            // Validate
            if (Record != 0)
                return new Size(0, 0);

            return new Size(ImgRecord.Width, ImgRecord.Height);
        }

        /// <summary>
        /// Gets managed bitmap from specified record and frame.
        /// </summary>
        /// <param name="Record">Record index. Must be 0 for Img files.</param>
        /// <param name="Frame">Frame index. Must be 0 for Img files.</param>
        /// <param name="IndexedColour">True to maintain idexed colour, false to return RGB bitmap.</param>
        /// <param name="MakeTransparent">True to make colour 0x000000 transparent, otherwise false.</param>
        /// <returns>Bitmap object.</returns>
        public override Bitmap GetManagedBitmap(int Record, int Frame, bool IndexedColour, bool MakeTransparent)
        {
            // Read image data
            if (!ReadImageData())
                return new Bitmap(4,4);

            // Validate
            if (null == ImgRecord.Data || Record != 0 || Frame != 0)
                return new Bitmap(4, 4);

            // Palettized bitmaps do not use transparent index
            if (IsPalettized)
                MakeTransparent = false;

            return base.GetManagedBitmap(ref ImgRecord, IndexedColour, MakeTransparent);
        }

        /// <summary>
        /// Get a preview of the file.
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
            Bitmap preview = new Bitmap(Width, Height);

            // Exit if no image loaded
            if (0 == RecordCount)
                return preview;

            // Get graphics object
            Graphics gr = Graphics.FromImage(preview);
            gr.Clear(Background);

            // Get this record and frame
            DFBitmap dfb = GetDFBitmap();
            Bitmap bmp = GetManagedBitmap(ref dfb, true, false);

            // Copy managed bitmap to preview frame
            Rectangle srcRect = new Rectangle(0, 0, dfb.Width, dfb.Height);
            Rectangle dstRect = new Rectangle(xpos, ypos, dfb.Width, dfb.Height);
            gr.DrawImage(bmp, dstRect, srcRect, GraphicsUnit.Pixel);

            return preview;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// IMG files have a fixed width and height not specified in a header.
        ///  This method returns the correct dimensions of images inside these files.
        /// </summary>
        /// <returns>Dimensions of image.</returns>
        protected Size GetHeaderlessFileImageDimensions()
        {
            // Set image dimensions
            switch (ManagedFile.Length)
            {
                case 44:
                    return new Size(22, 22);
                case 289:
                    return new Size(17, 17);
                case 441:
                    return new Size(49, 9);
                case 512:
                    return new Size(32, 16);
                case 720:
                    return new Size(9, 80);
                case 990:
                    return new Size(45, 22);
                case 1720:
                    return new Size(43, 40);
                case 2140:
                    return new Size(107, 20);
                case 2916:
                    return new Size(81, 36);
                case 3200:
                    return new Size(40, 80);
                case 3938:
                    return new Size(179, 22);
                case 4280:
                    return new Size(107, 40);
                case 4508:
                    return new Size(322, 14);
                case 20480:
                    return new Size(320, 64);
                case 26496:
                    return new Size(184, 144);
                case 64000:
                    return new Size(320, 200);
                case 64768:
                    return new Size(320, 200);
                case 68800:
                    return new Size(320, 215);
                case 112128:
                    return new Size(512, 219);
                default:
                    return new Size(0, 0);
            }
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
                BinaryReader Reader = ManagedFile.GetReader();
                ReadHeader(ref Reader);
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
            // Start header
            Reader.BaseStream.Position = 0;
            Header.Position = 0;

            // Create header based on RCI byte-size test
            Size sz = GetHeaderlessFileImageDimensions();
            if (sz.Width == 0 && sz.Height == 0)
            {
                // This image has a header
                ReadImgFileHeader(ref Reader, ref Header);
            }
            else
            {
                // This is an RCI-style image has no header, so we need to build one
                // Note that RCI-style images are never compressed
                Header.XOffset = 0;
                Header.YOffset = 0;
                Header.Width = (Int16)sz.Width;
                Header.Height = (Int16)sz.Height;
                Header.Compression = CompressionFormats.Uncompressed;
                Header.PixelDataLength = (UInt16)(Header.Width * Header.Height);
                Header.DataPosition = Reader.BaseStream.Position;
            }

            // Store image data position
            ImageDataPosition = Reader.BaseStream.Position;
        }

        /// <summary>
        /// Reads image data.
        /// </summary>
        private bool ReadImageData()
        {
            // Exit if this image already read
            if (ImgRecord.Data != null)
                return true;

            // Setup frame to hold extracted image
            ImgRecord.Width = Header.Width;
            ImgRecord.Height = Header.Height;
            ImgRecord.Stride = Header.Width;
            ImgRecord.Format = DFBitmap.Formats.Indexed;
            ImgRecord.Data = new byte[Header.Width * Header.Height];

            // Create reader
            BinaryReader Reader = ManagedFile.GetReader(ImageDataPosition);

            // Read image data
            ReadImage(ref Reader);

            // Read palette data
            ReadPalette(ref Reader);

            return true;
        }

        /// <summary>
        /// Read uncompressed image data.
        /// </summary>
        /// <param name="Reader">Source reader positioned at start of image data.</param>
        /// <returns>True if succeeded, otherwise false.</returns>
        private bool ReadImage(ref BinaryReader Reader)
        {
            // Read image bytes
            BinaryWriter writer = new BinaryWriter(new MemoryStream(ImgRecord.Data));
            writer.Write(Reader.ReadBytes(ImgRecord.Width * ImgRecord.Height));

            return true;
        }

        /// <summary>
        /// Some IMG files contain palette information following the image data.
        ///  This palette will replace any previosuly specified palette.
        /// </summary>
        /// <param name="Reader">Source reader positioned at end of image data.</param>
        private void ReadPalette(ref BinaryReader Reader)
        {
            // Get filename
            string fn = Path.GetFileName(ManagedFile.FilePath);
            switch (fn)
            {
                case "CHGN00I0.IMG":
                case "DIE_00I0.IMG":
                case "PICK02I0.IMG":
                case "PICK03I0.IMG":
                case "PRIS00I0.IMG":
                case "TITL00I0.IMG":
                    PaletteDataPosition = Reader.BaseStream.Position;
                    MyPalette.Read(ref Reader);
                    IsPalettizedValue = true;
                    break;
                default:
                    IsPalettizedValue = false;
                    return;
            }

            // The palette for palettized images is very dark. Multiplying the RGB values by 4 results in correct-looking colours
            if (IsPalettized)
            {
                for (int i = 0; i < 256; i++)
                {
                    int r = MyPalette.GetRed(i) * 4;
                    int g = MyPalette.GetGreen(i) * 4;
                    int b = MyPalette.GetBlue(i) * 4;
                    MyPalette.Set(i, (byte)r, (byte)g, (byte)b);
                }
            }
        }

        #endregion
    }
}
