// Project:         DaggerfallModelling
// Description:     Explore and export 3D models from Daggerfall.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

#endregion

namespace DaggerfallModelling.Classes
{

    /// <summary>
    /// Manges application settings for Daggerfall Modelling. Settings are stored in an XML
    ///  document in path %appdat%.
    /// </summary>
    class AppSettings
    {

        #region Variables

        // Application constants
        private const string AppName = "DaggerfallModelling";
        private const string MySettingsFileName = "Settings.xml";

        // User application data folders
        private static string AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
        private string SettingsFilePath = Path.Combine(AppDataFolder, MySettingsFileName);

        // Application settings (as saved and loaded in "Settings.xml"
        private const string Setting_Arena2Path = "Arena2Path";
        private const string Setting_IsMaximised = "IsMaximised";

        // Application defaults (deployed first time "Settings.xml" is created
        private const string Default_Arena2Path = "C:\\dosgames\\DAGGER\\ARENA2";
        private const int Default_IsMaximised = 0;

        // Application settings states (as read from "Settings.xml")
        private string State_Arena2Path = string.Empty;
        private int State_IsMaximised = 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AppSettings()
        {
            // Read settings
            ReadSettingsXml();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets Arena2 path setting.
        /// </summary>
        public string Arena2Path
        {
            get { return State_Arena2Path; }
            set { State_Arena2Path = value; }
        }

        /// <summary>
        /// Gets or sets IsMaximised setting.
        /// </summary>
        public int IsMaximised
        {
            get { return State_IsMaximised; }
            set { State_IsMaximised = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Saves live settings back to file.
        /// </summary>
        /// <returns></returns>
        public bool SaveSettings()
        {
            return WriteSettingsXml();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates settings XML file for the first time.
        /// </summary>
        /// <returns>True if settings file created, otherwise false.</returns>
        private bool CreateSettingsXml()
        {
            try
            {
                // Create applications settings folder if it does not exist
                if (!Directory.Exists(AppDataFolder))
                    Directory.CreateDirectory(AppDataFolder);

                // Create new settings file
                XmlHelper helper = new XmlHelper();
                XmlDocument SettingsXml = helper.CreateXmlDocument(AppName, SettingsFilePath);

                // Append default settings
                helper.AppendElement(SettingsXml, null, Setting_Arena2Path, Default_Arena2Path);
                helper.AppendElement(SettingsXml, null, Setting_IsMaximised, Default_IsMaximised.ToString());

                // Save file
                SettingsXml.Save(SettingsFilePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Reads settings from file.
        /// </summary>
        /// <returns></returns>
        private bool ReadSettingsXml()
        {
            // Test settings file exists
            if (!File.Exists(SettingsFilePath))
            {
                if (!CreateSettingsXml())
                {
                    string error = string.Format("Failed to create settings file in '{0}'.", SettingsFilePath);
                    Console.WriteLine(error);
                    return false;
                }
            }

            // Read settings
            try
            {
                // Open settings xml document
                XmlDocument SettingsXml = new XmlDocument();
                SettingsXml.Load(SettingsFilePath);

                // Read Arena2Path
                XmlNodeList nodes = SettingsXml.GetElementsByTagName(Setting_Arena2Path);
                State_Arena2Path = nodes.Item(0).InnerText;

                // Read IsMaximised
                nodes = SettingsXml.GetElementsByTagName(Setting_IsMaximised);
                State_IsMaximised = int.Parse(nodes.Item(0).InnerText);

                // Ensure Arena2Path exists
                if (!Directory.Exists(State_Arena2Path))
                    throw new Exception(string.Format("'{0}' does not exist.", State_Arena2Path));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Writes settings to file.
        /// </summary>
        /// <returns></returns>
        private bool WriteSettingsXml()
        {
            try
            {
                // Open settings xml document
                XmlDocument SettingsXml = new XmlDocument();
                SettingsXml.Load(SettingsFilePath);

                // Write Arena2Path
                XmlNodeList nodes = SettingsXml.GetElementsByTagName(Setting_Arena2Path);
                nodes.Item(0).InnerText = State_Arena2Path;

                // Write IsMaximised
                nodes = SettingsXml.GetElementsByTagName(Setting_IsMaximised);
                nodes.Item(0).InnerText = State_IsMaximised.ToString();

                // Save file
                SettingsXml.Save(SettingsFilePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        #endregion

    }

}
