// Project:         DaggerfallConnect
// Description:     Read data from Daggerfall's file formats.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
#endregion

namespace DaggerfallConnect.Utility
{

    /// <summary>
    /// Static methods to validate ARENA2 folder.
    /// </summary>
    public class DFValidator
    {

        #region Fields
        #endregion

        #region Structures

        /// <summary>
        /// Packages validation information.
        /// </summary>
        public struct ValidationResults
        {
            /// <summary>The full path that was tested.</summary>
            public string PathTested;

            /// <summary>True if all tests succeeded.</summary>
            public bool AppearsValid;

            /// <summary>True if folder exists.</summary>
            public bool FolderValid;

            /// <summary>True if texture count is correct.</summary>
            public bool TexturesValid;

            /// <summary>True if ARCH3D.BSA exists.</summary>
            public bool ModelsValid;

            /// <summary>True if BLOCKS.BSA exists.</summary>
            public bool BlocksValid;

            /// <summary>True if MAPS.BSA exists.</summary>
            public bool MapsValid;

            /// <summary>True if DAGGER.SND exists.</summary>
            public bool SoundsValid;

            /// <summary>True if WOODS.WLD exists.</summary>
            public bool WoodsValid;
        }

        #endregion

        #region Static Public Methods

        /// <summary>
        /// Validates an ARENA2 folder.
        ///  This currently just checks the right major files exist in the right quantities.
        ///  Does not verify contents so test is quite speedy and can be performed at startup.
        ///  A deep inspection option will be added at a later date.
        /// </summary>
        /// <param name="path">Full path of ARENA2 folder to validate.</param>
        /// <param name="results">Output results.</param>
        public static void ValidateArena2Folder(string path, out ValidationResults results)
        {
            results = new ValidationResults();
            results.PathTested = path;

            // Check folder exists
            if (!Directory.Exists(path))
                return;
            else
                results.FolderValid = true;

            // Get files
            string[] textures = Directory.GetFiles(path, "TEXTURE.???");
            string[] models = Directory.GetFiles(path, "ARCH3D.BSA");
            string[] blocks = Directory.GetFiles(path, "BLOCKS.BSA");
            string[] maps = Directory.GetFiles(path, "MAPS.BSA");
            string[] sounds = Directory.GetFiles(path, "DAGGER.SND");
            string[] woods = Directory.GetFiles(path, "WOODS.WLD");

            // Validate texture count
            if (textures.Length >= 472)
                results.TexturesValid = true;

            // Validate models count
            if (models.Length >= 1)
                results.ModelsValid = true;

            // Validate blocks count
            if (blocks.Length >= 1)
                results.BlocksValid = true;

            // Validate maps count
            if (maps.Length >= 1)
                results.MapsValid = true;

            // Validate sounds count
            if (sounds.Length >= 1)
                results.SoundsValid = true;

            // Validate woods count
            if (woods.Length >= 1)
                results.WoodsValid = true;

            // If everything else is valid then set AppearsValid flag
            if (results.FolderValid &&
                results.TexturesValid &&
                results.ModelsValid &&
                results.BlocksValid &&
                results.MapsValid &&
                results.SoundsValid &&
                results.WoodsValid)
            {
                results.AppearsValid = true;
            }
        }

        #endregion

    }

}
