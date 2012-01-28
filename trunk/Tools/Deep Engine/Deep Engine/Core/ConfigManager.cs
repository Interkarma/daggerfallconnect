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
using System.IO;
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

        #region Properties

        /// <summary>
        /// Gets a flag stating is file is loaded.
        /// </summary>
        public bool IsLoaded
        {
            get { return (ini == null) ? false : true; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConfigManager()
        {
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
        /// Loads ini file from user's appdata folder.
        ///  Deploys source defaults ini if appdata ini is missing.
        /// </summary>
        /// <param name="appName">Name of application.</param>
        /// <param name="configName">Name of INI file.</param>
        /// <param name="defaultsName">Name of source defaults INI file to be deployed if configName is missing.</param>
        public void LoadFile(string appName, string configName, string defaultsName)
        {
            // Ensure appdata folder exists
            string appDataFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);
            if (!Directory.Exists(appDataFolder))
                Directory.CreateDirectory(appDataFolder);

            // Check file exists
            string filename = System.IO.Path.Combine(appDataFolder, configName);
            string src = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, defaultsName);
            if (!File.Exists(filename))
                File.Copy(src, filename);

            // Load the file
            LoadFile(filename);
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
