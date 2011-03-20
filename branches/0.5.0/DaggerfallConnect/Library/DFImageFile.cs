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
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;

#endregion

namespace DaggerfallConnect
{
    /// <summary>
    /// Provides base image handling for all Daggerfall image files.
    ///  This class is inherited from and extended by Arena2.TextureFile, Arena2.ImgFile, etc.
    /// </summary>
    public abstract class DFImageFile
    {
        #region Class Variables

        /// <summary>
        /// Palette for building image data
        /// </summary>
        protected DFPalette MyPalette = new DFPalette();

        /// <summary>
        /// Managed file.
        /// </summary>
        internal FileProxy ManagedFile = new FileProxy();

        #endregion

        #region Class Structures

        /// <summary>
        /// Compression formats enumeration. This is shared as internal as most image formats use some kind of compression.
        /// </summary>
        internal enum CompressionFormats
        {
            Uncompressed = 0x0000,
            RleCompressed = 0x0002,
            ImageRle = 0x0108,
            RecordRle = 0x1108,
        }

        /// <summary>
        /// IMG File header. This is shared as internal as the IMG structure is also used in CIF files.
        ///  The CifFile class will use this file header while reading most records.
        /// </summary>
        internal struct ImgFileHeader
        {
            public long Position;
            public Int16 XOffset;
            public Int16 YOffset;
            public Int16 Width;
            public Int16 Height;
            public CompressionFormats Compression;
            public UInt16 PixelDataLength;
            public int FrameCount;
            public long DataPosition;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DFImageFile()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets full path of managed file.
        /// </summary>
        public string FilePath
        {
            get { return ManagedFile.FilePath; }
        }

        /// <summary>
        /// Gets file name only of managed file.
        /// </summary>
        public string FileName
        {
            get { return Path.GetFileName(FilePath); }
        }

        /// <summary>
        /// Gets or sets palette for building images.
        /// </summary>
        public DFPalette Palette
        {
            get { return MyPalette; }
            set { MyPalette = value; }
        }

        /// <summary>
        /// Gets total number of records in this file.
        /// </summary>
        public abstract int RecordCount
        {
            get;
        }

        /// <summary>
        /// Gets description of this file.
        /// </summary>
        public abstract string Description
        {
            get;
        }

        /// <summary>
        /// Gets correct palette name for this file.
        /// </summary>
        public abstract string PaletteName
        {
            get;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads an image file.
        /// </summary>
        /// <param name="FilePath">Absolute path to file</param>
        /// <param name="Usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="ReadOnly">File will be read-only if true, read-write if false.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public abstract bool Load(string FilePath, FileUsage Usage, bool ReadOnly);

        /// <summary>
        /// Get a preview of the file. As many images as possible will be laid out onto the preview surface.
        /// </summary>
        /// <param name="Width">Width of preview surface.</param>
        /// <param name="Height">Height of preview surface.</param>
        /// <param name="Background">Colour of background.</param>
        /// <returns>Bitmap object.</returns>
        public abstract Bitmap GetPreview(int Width, int Height, Color Background);

        /// <summary>
        /// Gets number of frames in specified record.
        /// </summary>
        /// <param name="Record">Index of record.</param>
        /// <returns>Number of frames.</returns>
        public abstract int GetFrameCount(int Record);

        /// <summary>
        /// Gets width and height of specified record. All frames of this record are the same dimensions.
        /// </summary>
        /// <param name="Record">Index of record.</param>
        /// <returns>Size object.</returns>
        public abstract Size GetSize(int Record);

        /// <summary>
        /// Gets bitmap data as indexed 8-bit byte array for specified record and frame.
        /// </summary>
        /// <param name="Record">Index of record.</param>
        /// <param name="Frame">Index of frame.</param>
        /// <returns>DFBitmap object.</returns>
        public abstract DFBitmap GetDFBitmap(int Record, int Frame);

        /// <summary>
        /// Gets managed bitmap from specified record and frame.
        /// </summary>
        /// <param name="Record">Record index.</param>
        /// <param name="Frame">Frame index.</param>
        /// <param name="IndexedColour">True to maintain idexed colour, false to return RGB bitmap.</param>
        /// <param name="MakeTransparent">True to make colour 0x000000 transparent, otherwise false.</param>
        /// <returns>Bitmap object.</returns>
        public abstract Bitmap GetManagedBitmap(int Record, int Frame, bool IndexedColour, bool MakeTransparent);

        /// <summary>
        /// Loads a Daggerfall palette that will be used for building images.
        /// </summary>
        /// <param name="FilePath">Absolute path to Daggerfall palette.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool LoadPalette(string FilePath)
        {
            return MyPalette.Load(FilePath);
        }

        /// <summary>
        /// Get raw bytes for specified record and frame using a custom pixel format.
        /// </summary>
        /// <param name="Record">Index of record.</param>
        /// <param name="Frame">Index of frame.</param>
        /// <param name="AlphaIndex">Index of alpha colour.</param>
        /// <param name="Format">Specified pixel format to use.</param>
        /// <returns>DFBitmap object.</returns>
        public DFBitmap GetBitmapFormat(int Record, int Frame, byte AlphaIndex, DFBitmap.Formats Format)
        {
            // Get as indexed image
            if (Format == DFBitmap.Formats.Indexed)
                return GetDFBitmap(Record, Frame);

            // Create new bitmap
            const int format = 4;
            DFBitmap srcBitmap = GetDFBitmap(Record, Frame);
            DFBitmap dstBitmap = new DFBitmap();
            dstBitmap.Format = Format;
            dstBitmap.Width = srcBitmap.Width;
            dstBitmap.Height = srcBitmap.Height;
            dstBitmap.Stride = dstBitmap.Width * format;
            dstBitmap.Data = new byte[dstBitmap.Stride * dstBitmap.Height];

            // Write pixel data to array
            byte a, r, g, b;
            int srcPos = 0, dstPos = 0;
            for (int i = 0; i < dstBitmap.Width * dstBitmap.Height; i++)
            {
                // Write colour values
                byte index = srcBitmap.Data[srcPos++];
                if (index != AlphaIndex)
                {
                    // Get colour values
                    a = 0xff;
                    r = MyPalette.GetRed(index);
                    g = MyPalette.GetGreen(index);
                    b = MyPalette.GetBlue(index);

                    // Write colour values
                    switch (Format)
                    {
                        case DFBitmap.Formats.ARGB:
                            dstBitmap.Data[dstPos++] = b;
                            dstBitmap.Data[dstPos++] = g;
                            dstBitmap.Data[dstPos++] = r;
                            dstBitmap.Data[dstPos++] = a;
                            break;
                        case DFBitmap.Formats.RGBA:
                            dstBitmap.Data[dstPos++] = a;
                            dstBitmap.Data[dstPos++] = b;
                            dstBitmap.Data[dstPos++] = g;
                            dstBitmap.Data[dstPos++] = r;
                            break;
                        case DFBitmap.Formats.ABGR:
                            dstBitmap.Data[dstPos++] = r;
                            dstBitmap.Data[dstPos++] = g;
                            dstBitmap.Data[dstPos++] = b;
                            dstBitmap.Data[dstPos++] = a;
                            break;
                        case DFBitmap.Formats.BGRA:
                            dstBitmap.Data[dstPos++] = a;
                            dstBitmap.Data[dstPos++] = r;
                            dstBitmap.Data[dstPos++] = g;
                            dstBitmap.Data[dstPos++] = b;
                            break;
                        default:
                            throw new Exception("Unknown output format.");
                    }
                }
                else
                {
                    // Step over alpha pixels
                    dstPos += format;
                }
            }

            return dstBitmap;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Gets managed bitmap from specified indexed image buffer.
        ///  The currently loaded palette will be used for index to RGB matching.
        /// </summary>
        /// <param name="DFBitmap">Object containing source indexed bitmap data.</param>
        /// <param name="IndexedColour">True to maintain idexed colour, false to return RGB bitmap.</param>
        /// <param name="MakeTransparent">True to make image transparent, otherwise false.</param>
        /// <returns>Bitmap object.</returns>
        internal Bitmap GetManagedBitmap(ref DFBitmap DFBitmap, bool IndexedColour, bool MakeTransparent)
        {
            // Validate
            if (DFBitmap.Data == null || DFBitmap.Format != DFBitmap.Formats.Indexed)
                throw new Exception("Invalid bitmap data or format.");

            // Specify a special colour unused in Daggerfall's palette for transparency check
            Color TransparentRGB = Color.FromArgb(255, 1, 2);

            // Create bitmap
            Size sz = new Size(DFBitmap.Width, DFBitmap.Height);
            Bitmap bitmap = new Bitmap(sz.Width, sz.Height, PixelFormat.Format8bppIndexed);

            // Lock bitmap
            Rectangle rect = new Rectangle(0, 0, sz.Width, sz.Height);
            BitmapData bmd = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            // Copy row data accounting for stride
            UInt32 dst = (UInt32)bmd.Scan0;
            for (int y = 0; y < sz.Height; y++)
            {
                System.Runtime.InteropServices.Marshal.Copy(DFBitmap.Data, y * sz.Width, (IntPtr)(dst + y * bmd.Stride), sz.Width);
            }

            // Unlock bitmap
            bitmap.UnlockBits(bmd);

            // If making transparent set index zero to special colour
            Color OldIndex0 = MyPalette.Get(0);
            if (MakeTransparent && !IndexedColour)
                MyPalette.Set(0, TransparentRGB.R, TransparentRGB.G, TransparentRGB.B);

            // Set bitmap palette
            bitmap.Palette = MyPalette.GetManagedColorPalette();

            // Indexed bitmap completed
            if (IndexedColour)
                return bitmap;

            // Clone image into final pixel format
            Bitmap finalBitmap = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), PixelFormat.Format32bppArgb);

            // Make transparent
            if (MakeTransparent)
                finalBitmap.MakeTransparent(TransparentRGB);

            // Set back index 0
            MyPalette.Set(0, OldIndex0.R, OldIndex0.G, OldIndex0.B);

            return finalBitmap;
        }

        /// <summary>
        /// Reads a standard IMG file header from the source stream into the desination header struct.
        ///  This header is found in multiple image files which is why it's implemented here in the base.
        /// </summary>
        /// <param name="Reader">Source reader positioned at start of header data.</param>
        /// <param name="Header">Destination header structure.</param>
        internal void ReadImgFileHeader(ref BinaryReader Reader, ref ImgFileHeader Header)
        {
            // Read IMG header data
            Header.Position = Reader.BaseStream.Position;
            Header.XOffset = Reader.ReadInt16();
            Header.YOffset = Reader.ReadInt16();
            Header.Width = Reader.ReadInt16();
            Header.Height = Reader.ReadInt16();
            Header.Compression = (CompressionFormats)Reader.ReadUInt16();
            Header.PixelDataLength = Reader.ReadUInt16();
            Header.FrameCount = 1;
            Header.DataPosition = Reader.BaseStream.Position;
        }

        #endregion 
    }
}
