// Project:         XNALibrary
// Description:     Simple XNA game library for DaggerfallConnect.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace XNALibrary
{

    /// <summary>
    /// Helper class to load and store Daggerfall maps for XNA.
    /// </summary>
    public class MapManager
    {

        #region Class Variables

        // Map management
        private string arena2Path;
        private MapsFile mapsFile;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets Arena2 path set at construction.
        /// </summary>
        public string Arena2Path
        {
            get { return arena2Path; }
        }

        /// <summary>
        /// Gets MapsFile.
        /// </summary>
        public MapsFile MapsFile
        {
            get { return mapsFile; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="arena2Path">Path to Arena2 folder.</param>
        public MapManager(string arena2Path)
        {
            // Setup
            this.arena2Path = arena2Path;

            // Load MAPS.BSA
            mapsFile = new MapsFile(Path.Combine(arena2Path, "MAPS.BSA"), FileUsage.UseDisk, true);
            mapsFile.AutoDiscard = true;
        }

        #endregion

    }

}
