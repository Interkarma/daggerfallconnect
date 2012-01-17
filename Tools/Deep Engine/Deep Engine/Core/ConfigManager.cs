// Project:         Deep Engine
// Description:     3D game engine for Ruins of Hill Deep and Daggerfall Workshop projects.
// Copyright:       Copyright (C) 2012 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using IniParser;
#endregion

namespace DeepEngine.Core
{

    /// <summary>
    /// INI file configuration manager.
    /// </summary>
    public class ConfigManager
    {

        #region Fields

        const string fileNotLoadedError = "File not loaded.";

        string filename;
        FileIniDataParser parser = new FileIniDataParser();
        IniData ini = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConfigManager()
        {
        }

        /// <summary>
        /// Load constructor.
        /// </summary>
        /// <param name="filename">Full path to ini file.</param>
        public ConfigManager(string filename)
        {
            LoadFile(filename);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the specified ini file.
        /// </summary>
        /// <param name="filename">Full path to ini file.</param>
        /// <returns>True if successful.</returns>
        public void LoadFile(string filename)
        {
            ini = parser.LoadFile(filename);
            this.filename = filename;
        }

        /// <summary>
        /// Saves any changes to ini file.
        /// </summary>
        public void SaveFile()
        {
            // No file loaded
            if (ini == null)
                throw new Exception(fileNotLoadedError);

            parser.SaveFile(filename, ini);
        }

        /// <summary>
        /// Gets a key value from ini file.
        /// </summary>
        /// <param name="section">Section containing value.</param>
        /// <param name="key">Key to obtain value for.</param>
        /// <returns>Value, or null if section/key not found.</returns>
        public string GetValue(string section, string key)
        {
            // No file loaded
            if (ini == null)
                return null;

            string value = ini[section][key];

            return value;
        }

        /// <summary>
        /// Sets a key value in ini file.
        /// </summary>
        /// <param name="section">Section containing value.</param>
        /// <param name="key">Key to set value for.</param>
        /// <param name="value">Value to set.</param>
        public void SetValue(string section, string key, string value)
        {
            // No file loaded
            if (ini == null)
                throw new Exception(fileNotLoadedError);

            ini[section][key] = value;
        }

        #endregion

    }

}
