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

namespace DaggerfallConnect
{
    /// <summary>
    /// Used to constructs manual images from DFImage. Provides all the base handling from DFImageFile, and all the benefits
    ///  (e.g. preview, format conversion, palette handling, etc.). However, it only stores a single image.
    /// </summary>
    public class DFManualImage : DFImageFile
    {
        #region Class Variables

        private DFBitmap manualBitmap;

        #endregion

        #region Public Properties

        /// <summary>
        /// Manual images do not use a specific palette. This will always return string.Empty.
        /// </summary>
        public override string PaletteName
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Description of this file. Always "Manual Image".
        /// </summary>
        public override string Description
        {
            get { return "Manual Image"; }
        }

        /// <summary>
        /// Number of image records in this manual image. Always 1.
        /// </summary>
        public override int RecordCount
        {
            get { return 1; }
        }

        /// <summary>
        /// Gets or sets DFBitmap object.
        /// </summary>
        public DFBitmap DFBitmap
        {
            get { return manualBitmap; }
            set { manualBitmap = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DFManualImage()
        {
            manualBitmap = new DFBitmap();
        }

        /// <summary>
        /// Constructor using a DFBitmap.
        /// </summary>
        /// <param name="dfBitmap">Source DFBitmap object.</param>
        public DFManualImage(DFBitmap dfBitmap)
        {
            manualBitmap = dfBitmap;
        }

        /// <summary>
        /// Constructor to create an empty image of size and format.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="format"></param>
        public DFManualImage(int width, int height, DFBitmap.Formats format)
        {
            // Create bitmap data
            manualBitmap = new DFBitmap();
            manualBitmap.Width = width;
            manualBitmap.Height = height;
            manualBitmap.Format = format;
            manualBitmap.Data = new byte[width * height];

            // Set stride (1 byte per pixel for indexed, 4 bytes per pixel for all other formats)
            switch (format)
            {
                case DFBitmap.Formats.Indexed:
                    manualBitmap.Stride = width;
                    break;
                default:
                    manualBitmap.Stride = width*4;
                    break;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Implemented only to satisfy abstract base. Does nothing.
        /// </summary>
        /// <param name="FilePath">N/A</param>
        /// <param name="Usage">N/A</param>
        /// <param name="ReadOnly">N/A</param>
        /// <returns>Always true.</returns>
        public override bool Load(string FilePath, FileUsage Usage, bool ReadOnly)
        {
            return true;
        }

        /// <summary>
        /// Gets width and height of specified record. As manual images only have 1 record,
        ///  this must be 0.
        /// </summary>
        /// <param name="Record">Index of record. Must be 0.</param>
        /// <returns>Size object.</returns>
        public override Size GetSize(int Record)
        {
            // Validate
            if (Record != 0)
                return new Size(0, 0);

            return new Size(manualBitmap.Width, manualBitmap.Height);
        }

        /// <summary>
        /// Gets number of frames in specified record.
        /// </summary>
        /// <param name="Record">Index of record. Must be 0 for manual images.</param>
        /// <returns>Number of frames. Always 1 for manual images.</returns>
        public override int GetFrameCount(int Record)
        {
            // Validate
            if (Record != 0)
                return -1;

            return 1;
        }

        /// <summary>
        /// Get a preview of the image.
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
            Bitmap bmp = GetManagedBitmap(ref manualBitmap, true, false);

            // Copy managed bitmap to preview frame
            Rectangle srcRect = new Rectangle(0, 0, manualBitmap.Width, manualBitmap.Height);
            Rectangle dstRect = new Rectangle(xpos, ypos, manualBitmap.Width, manualBitmap.Height);
            gr.DrawImage(bmp, dstRect, srcRect, GraphicsUnit.Pixel);

            return preview;
        }

        /// <summary>
        /// Gets bitmap data as indexed 8-bit byte array for specified record and frame.
        ///  Manual images only have 1 image, so the Record and Frame indices must always be 0
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

            return manualBitmap;
        }

        /// <summary>
        /// Gets managed bitmap.
        /// </summary>
        /// <param name="Record">Record index. Must be 0 for manual images.</param>
        /// <param name="Frame">Frame index. Must be 0 for manual images.</param>
        /// <param name="IndexedColour">True to maintain idexed colour, false to return RGB bitmap.</param>
        /// <param name="MakeTransparent">True to make colour 0x000000 transparent, otherwise false.</param>
        /// <returns>Bitmap object.</returns>
        public override Bitmap GetManagedBitmap(int Record, int Frame, bool IndexedColour, bool MakeTransparent)
        {
            // Validate
            if (Record != 0 || Frame != 0)
                return new Bitmap(4, 4);

            return base.GetManagedBitmap(ref manualBitmap, IndexedColour, MakeTransparent);
        }

        /// <summary>
        /// Set data byte array to anything.
        ///  Other image properties must also be set manually.
        ///  Byte array Must be equal to stride*height.
        /// </summary>
        /// <param name="Data">Byte array.</param>
        public void SetData(byte[] Data)
        {
            // Check size of incoming array against image dimensions
            //if (Data.Length != manualBitmap.Stride * manualBitmap.Height)
            //    throw new Exception("Invalid buffer length for DFManualImage.SetData().");

            // Store the data
            manualBitmap.Data = Data;
        }

        #endregion
    }
}
