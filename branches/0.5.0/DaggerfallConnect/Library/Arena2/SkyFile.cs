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
    /// Connects to a SKY??.DAT file to enumerate and extract image data.
    /// </summary>
    public class SkyFile : DFImageFile
    {
        #region Class Variables

        // Class constants
        private const int FrameWidth = 512;
        private const int FrameHeight = 220;
        private const int FrameDataLength = FrameWidth * FrameHeight;
        private const int PaletteDataLength = 776;
        private const long PaletteDataPosition = 0;
        private const long ImageDataPosition = 549120;

        /// <summary>
        /// Palette array.
        /// </summary>
        private DFPalette[] Palettes = new DFPalette[32];

        /// <summary>
        /// Bitmap array.
        /// </summary>
        private DFBitmap[] Bitmaps = new DFBitmap[64];

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SkyFile()
        {
        }

        /// <summary>
        /// Load constructor.
        /// </summary>
        /// <param name="FilePath">Absolute path to SKY??.DAT file.</param>
        /// <param name="Usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        public SkyFile(string FilePath, FileUsage Usage, bool ReadOnly)
        {
            Load(FilePath, Usage, ReadOnly);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Number of records in this Sky file. Always 2 for a SKY file (one animation each for east and west sky).
        /// </summary>
        public override int RecordCount
        {
            get
            {
                if (ManagedFile.FilePath == string.Empty)
                    return 0;
                else
                    return 2;
            }

        }

        /// <summary>
        /// SKY files are fully palettized per frame.
        ///  This method always returns string.Empty and is implemented only to satisfy abstract base class DFImage.
        ///  Use GetDFPalette(Frame) instead.
        /// </summary>
        public override string PaletteName
        {
            get { return string.Empty;  }
        }

        /// <summary>
        /// Description of this file (always "SKY File" as the game files contain no text descriptions for this file type).
        /// </summary>
        public override string Description
        {
            get { return "SKY File"; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads a SKY file.
        /// </summary>
        /// <param name="FilePath">Absolute path to SKY??.DAT file</param>
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
            if (!fn.StartsWith("SKY") && !fn.EndsWith(".DAT"))
                return false;

            // Load file
            if (!ManagedFile.Load(FilePath, Usage, ReadOnly))
                return false;

            return true;
        }

        /// <summary>
        /// Gets palette data for specified record and frame.
        /// </summary>
        /// <param name="Frame">Index of frame.</param>
        /// <returns>DFPalette object or null.</returns>
        public DFPalette GetDFPalette(int Frame)
        {
            // Validate
            if (Frame < 0 || Frame >= 32)
                return null;

            // Read palette data
            ReadPalette(Frame);

            return Palettes[Frame];
        }

        /// <summary>
        /// Gets bitmap data as indexed 8-bit byte array.
        /// </summary>
        /// <param name="Record">Index of record.</param>
        /// <param name="Frame">Index of frame.</param>
        /// <returns>DFBitmap object. Check DFBitmap.Data for null on failure.</returns>
        public override DFBitmap GetDFBitmap(int Record, int Frame)
        {
            // Validate
            if (Record < 0 || Record >= 2 || Frame < 0 || Frame >= 32)
                return new DFBitmap();

            // Calculate index
            int Index = Record * 32 + Frame;

            // Read image data
            ReadImageData(Index);

            return Bitmaps[Index];
        }

        /// <summary>
        /// Gets number of frames in specified record. Always 32 for SKY files.
        /// </summary>
        /// <param name="Record">Index of record.</param>
        /// <returns>Number of frames or -1 on error.</returns>
        public override int GetFrameCount(int Record)
        {
            // Validate
            if (string.IsNullOrEmpty(ManagedFile.FileName) || Record < 0 || Record >= 2)
                return -1;

            return 32;
        }

        /// <summary>
        /// Gets width and height of specified record. All frames of this record are the same dimensions.
        /// </summary>
        /// <param name="Record">Index of record.</param>
        /// <returns>Size object.</returns>
        public override Size GetSize(int Record)
        {
            // Validate
            if (string.IsNullOrEmpty(ManagedFile.FileName) || Record < 0 || Record >= 2)
                return new Size(0, 0);

            return new Size(FrameWidth, FrameHeight);
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
            // Get image
            DFBitmap frame = GetDFBitmap(Record, Frame);
            if (null == frame.Data)
                return new Bitmap(4, 4);

            // Set base palette
            base.MyPalette = GetDFPalette(Frame);

            return base.GetManagedBitmap(ref frame, IndexedColour, MakeTransparent);
        }

        /// <summary>
        /// Get a preview of the file. Unlike other image previews, this method will only render frame 20 of record 1.
        ///  This is because all sky images look virtually identical at dawn and only look truly unique around half way through daytime on the brightest side.
        ///  The goal is to create a snapshot unique to each sky palette.
        /// </summary>
        /// <param name="Width">Width of preview surface.</param>
        /// <param name="Height">Height of preview surface.</param>
        /// <param name="Background">Colour of background.</param>
        /// <returns>Bitmap object.</returns>
        public override Bitmap GetPreview(int Width, int Height, Color Background)
        {
            // Use record 1 frame 20
            Bitmap bmp = GetManagedBitmap(1, 20, true, false);

            // Get graphics object
            Bitmap preview = new Bitmap(Width, Height);
            Graphics gr = Graphics.FromImage(preview);
            gr.Clear(Background);

            // Copy managed bitmap to preview frame
            Rectangle srcRect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            Rectangle dstRect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            gr.DrawImage(bmp, dstRect, srcRect, GraphicsUnit.Pixel);

            return preview;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Read palette for specified record.
        /// </summary>
        /// <param name="Index">Index of palette.</param>
        private void ReadPalette(int Index)
        {
            // Read palette data if not already stored
            if (null == Palettes[Index])
            {
                BinaryReader Reader = ManagedFile.GetReader(PaletteDataPosition + (776 * Index) + 8);
                Palettes[Index] = new DFPalette();
                Palettes[Index].Read(ref Reader);
            }
        }

        /// <summary>
        /// Reads image data.
        /// </summary>
        /// <param name="Index">Index of image.</param>
        private bool ReadImageData(int Index)
        {
            // Read image if not already stored
            if (null == Bitmaps[Index].Data)
            {
                BinaryReader Reader = ManagedFile.GetReader(ImageDataPosition + (FrameDataLength * Index));
                Bitmaps[Index].Width = FrameWidth;
                Bitmaps[Index].Height = FrameHeight;
                Bitmaps[Index].Stride = FrameWidth;
                Bitmaps[Index].Format = DFBitmap.Formats.Indexed;
                Bitmaps[Index].Data = Reader.ReadBytes(FrameDataLength);
            }

            return true;
        }

        #endregion
    }
}
