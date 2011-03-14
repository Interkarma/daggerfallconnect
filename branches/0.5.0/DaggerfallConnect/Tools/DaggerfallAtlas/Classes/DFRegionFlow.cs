// Project:         DaggerfallAtlas
// Description:     Explore and export bitmaps from Daggerfall.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/using System;

# region Imports

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;

#endregion

namespace DaggerfallAtlas.Classes
{
    /// <summary>
    /// A flow layout control for viewing and selecting regions.
    /// </summary>
    class DFRegionFlow : DFItemFlow
    {
        #region Class Variables

        private string arena2Path;
        string[] regionNames;
        private MapsFile mapsFile = new MapsFile();
        private ImageFileReader imageFileReader = new ImageFileReader();

        #endregion

        #region Public Properties

        public string Arena2Path
        {
            get { return arena2Path; }
            set { SetArena2Path(value); }
        }

        public override int ItemCount
        {
            get { return GetItemCount(); }
        }

        #endregion

        #region Abstract Method Implementation

        protected override Size GetItemSize(int index)
        {
            return new Size(300, 79);
        }

        protected override void PaintItem(int index, Point position, Graphics gr)
        {
            // Set image file reader
            imageFileReader.LibraryType = LibraryTypes.Img;
            if (imageFileReader.RecordCount == 0)
                return;

            // Get region index
            int regionIndex = mapsFile.GetRegionIndex(regionNames[index]);

            // Compose region bitmap filename.
            // Not all regions have a bitmap, and some use more than one bitmap.
            // This will satisfy most cases then we handle special cases.
            string fn = string.Format("FMAP0I{0:00}.IMG", regionIndex);
            
            // Attempt to load region map
            DFImageFile dfImageFile = imageFileReader.LoadFile(fn);
            if (dfImageFile == null)
            {
                // Just create an empty image in "sea" colour for now
                dfImageFile = new DFManualImage(320, 160, DFBitmap.Formats.Indexed);
                dfImageFile.Palette.Set(0, 40, 71, 166);
            }

            // Paint bitmap
            Bitmap bm = dfImageFile.GetManagedBitmap(0, 0, true, false);
            Rectangle src = new Rectangle(0, 0, bm.Width, bm.Height);
            Rectangle dst = new Rectangle(position.X, position.Y, bm.Width / 2, bm.Height / 2);
            gr.DrawImage(bm, dst, src, GraphicsUnit.Pixel);
        }

        #endregion

        #region Private Methods

        int GetItemCount()
        {
            if (mapsFile == null || regionNames == null)
                return 0;

            return regionNames.Length;
        }

        void SetArena2Path(string path)
        {
            // Attach to MAPS.BSA
            if (!mapsFile.Load(Path.Combine(path, "MAPS.BSA"), FileUsage.UseDisk, true))
                return;

            // Set image file reader
            imageFileReader.Arena2Path = path;
            imageFileReader.AutoDiscard = false;

            // Store new value
            arena2Path = path;

            // Populate region names array only with regions that have > 0 locations
            int validRegionCount = 0;
            string[] validRegions = new string[mapsFile.RegionCount];
            for (int i = 0; i < mapsFile.RegionCount; i++)
            {
                DFRegion dfRegion = mapsFile.GetRegion(i);
                if (dfRegion.MapNames.LocationCount > 0)
                {
                    validRegions[validRegionCount] = dfRegion.Name;
                    validRegionCount++;
                }
            }

            // Copy valid regions to stored array
            regionNames = new string[validRegionCount];
            for (int i = 0; i < validRegionCount; i++)
            {
                regionNames[i] = validRegions[i];
            }

            // Sort array of names
            Array.Sort(regionNames);

            // Update base layout
            base.NewLayout();
        }

        #endregion
    }
}
