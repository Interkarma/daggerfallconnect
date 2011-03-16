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
using System.Collections.Generic;

#endregion

namespace DaggerfallConnect
{
    /// <summary>
    /// Stores information about locations in a region.
    /// </summary>
    public struct DFRegion
    {
        #region Structure Variables

        /// <summary>
        /// Name of this region.
        /// </summary>
        public String Name;

        /// <summary>
        /// Number of locations in this region. Not all regions have locations.
        ///  Always check LocationCount before working with MapNames array.
        /// </summary>
        public UInt32 LocationCount;

        /// <summary>
        /// Contains the names of all locations for this region.
        /// </summary>
        public string[] MapNames;

        /// <summary>
        /// Contains extended data about each location in this region.
        /// </summary>
        public RegionMapTable[] MapTable;

        /// <summary>
        /// Dictionary to find map index from MapID.
        /// </summary>
        public Dictionary<int, int> MapIdLookup;

        /// <summary>
        /// Dictionary to find a map index from map name. Note that some map names
        ///  are duplicates. Only the first instance will be stored and subsequent
        ///  duplicates discarded. These locations can still be referenced by MapID or index.
        /// </summary>
        public Dictionary<string, int> MapNameLookup;

        #endregion

        #region Child Structures

        /// <summary>
        /// Describes a single location.
        /// </summary>
        public struct RegionMapTable
        {
            /// <summary>Numeric ID of this location.</summary>
            public Int32 MapId;

            /// <summary>Numeric ID of dungeon map associated with this location.
            ///  Set to 0 if no dungeon for this location.</summary>
            public Int32 DungeonMapId;

            /// <summary>Unknown.</summary>
            public Byte Unknown1;

            /// <summary>Longitude and Type compressed into a bitfield.</summary>
            public UInt32 LongitudeTypeBitfield;

            /// <summary>Longitude value read from bitfield.</summary>
            public UInt32 Longitude;

            /// <summary>Type value read from bitfield.</summary>
            public UInt32 Type;

            /// <summary>Latitude of this location.</summary>
            public UInt16 Latitude;

            /// <summary>Unknown.</summary>
            public UInt16 Unknown2;

            /// <summary>Unknown.</summary>
            public UInt32 Unknown3;
        }

        #endregion
    }
}
