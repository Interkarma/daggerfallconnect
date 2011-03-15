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
        private string[] regionStrings;
        private int[] regionIndices;
        private Bitmap[] regionBitmaps;
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
            return new Size(320, 160);
        }

        protected override void PaintItem(int index, Point position, Graphics gr)
        {
            // Set image file reader
            imageFileReader.LibraryType = LibraryTypes.Img;
            if (imageFileReader.RecordCount == 0)
                return;

            // Get region index
            int regionIndex = regionIndices[index];

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
            Rectangle dst = new Rectangle(position.X, position.Y, bm.Width, bm.Height);
            gr.DrawImage(bm, dst, src, GraphicsUnit.Pixel);
        }

        #endregion

        #region Private Methods

        int GetItemCount()
        {
            if (mapsFile == null || regionIndices == null || regionStrings == null)
                return 0;

            return regionIndices.Length;
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

            // Index regions
            IndexRegions();

            // Update base layout
            base.NewLayout();
        }

        void IndexRegions()
        {
            // Populate sorted region dictionary with valid regions.
            // A valid region has >0 locations.
            SortedDictionary<string, int> regionDict = regionDict = new SortedDictionary<string, int>();
            for (int i = 0; i < mapsFile.RegionCount; i++)
            {
                // Skip "High Rock sea coast" (index=31)
                if (i == 31)
                    continue;

                // Add any other region with >0 locations
                DFRegion dfRegion = mapsFile.GetRegion(i);
                if (dfRegion.MapNames.LocationCount > 0)
                    regionDict.Add(dfRegion.Name, i);
            }

            // Copy region keys and values to simple arrays.
            // This doesn't change for the lifetime of class.
            regionStrings = new string[regionDict.Keys.Count];
            regionIndices = new int[regionDict.Values.Count];
            regionDict.Keys.CopyTo(regionStrings, 0);
            regionDict.Values.CopyTo(regionIndices, 0);
        }

        #endregion
    }
}
