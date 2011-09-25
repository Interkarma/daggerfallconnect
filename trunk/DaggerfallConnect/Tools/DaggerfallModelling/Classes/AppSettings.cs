// Project:         DaggerfallModelling
// Description:     Explore and export 3D models from Daggerfall.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
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
    ///  document in path %appdata%.
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
        private const string Setting_InvertMouseY = "InvertMouseY";
        private const string Setting_InvertGamePadY = "InvertGamePadY";
        private const string Setting_ColladaExportPath = "ColladaExportPath";
        private const string Setting_ColladaExportOrientation = "ColladaExportOrientation";
        private const string Setting_ColladaExportImageFormat = "ColladaExportImageFormat";

        // Application defaults (deployed first time "Settings.xml" is created
        private const string Default_Arena2Path = "C:\\dosgames\\DAGGER\\ARENA2";
        private const int Default_IsMaximised = 1;
        private const int Default_InvertMouseY = 0;
        private const int Default_InvertGamePadY = 0;
        private const string Default_ColladaExportPath = "";
        private const int Default_ColladaExportOrientation = 2;
        private const int Default_ColladaExportImageFormat = 1;

        // Application settings states (as read from "Settings.xml")
        private string State_Arena2Path;
        private int State_IsMaximised;
        private int State_InvertMouseY;
        private int State_InvertGamePadY;
        private string State_ColladaExportPath;
        private int State_ColladaExportOrientation;
        private int State_ColladaExportImageFormat;

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

        /// <summary>
        /// Gets or sets InvertMouseY setting.
        /// </summary>
        public int InvertMouseY
        {
            get { return State_InvertMouseY; }
            set { State_InvertMouseY = value; }
        }

        /// <summary>
        /// Gets or sets InvertGamePadY setting.
        /// </summary>
        public int InvertGamePadY
        {
            get { return State_InvertGamePadY; }
            set { State_InvertGamePadY = value; }
        }

        /// <summary>
        /// Gets or sets ColladaExportPath setting.
        /// </summary>
        public string ColladaExportPath
        {
            get { return State_ColladaExportPath; }
            set { State_ColladaExportPath = value; }
        }

        /// <summary>
        /// Gets or sets ColladaExportOrientation setting.
        /// </summary>
        public int ColladaExportOrientation
        {
            get { return State_ColladaExportOrientation; }
            set { State_ColladaExportOrientation = value; }
        }

        /// <summary>
        /// Gets or sets ColladaExportImageFormat setting.
        /// </summary>
        public int ColladaExportImageFormat
        {
            get { return State_ColladaExportImageFormat; }
            set { State_ColladaExportImageFormat = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Saves live settings back to file.
        /// </summary>
        /// <returns>True if successful.</returns>
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
                helper.AppendElement(SettingsXml, null, Setting_InvertMouseY, Default_InvertMouseY.ToString());
                helper.AppendElement(SettingsXml, null, Setting_InvertGamePadY, Default_InvertGamePadY.ToString());
                helper.AppendElement(SettingsXml, null, Setting_ColladaExportPath, Default_ColladaExportPath);
                helper.AppendElement(SettingsXml, null, Setting_ColladaExportOrientation, Default_ColladaExportOrientation.ToString());
                helper.AppendElement(SettingsXml, null, Setting_ColladaExportImageFormat, Default_ColladaExportImageFormat.ToString());

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

                // Read InvertMouseY
                nodes = SettingsXml.GetElementsByTagName(Setting_InvertMouseY);
                State_InvertMouseY = int.Parse(nodes.Item(0).InnerText);

                // Read InvertGamePadY
                nodes = SettingsXml.GetElementsByTagName(Setting_InvertGamePadY);
                State_InvertGamePadY = int.Parse(nodes.Item(0).InnerText);

                // Read ColladaExportPath
                nodes = SettingsXml.GetElementsByTagName(Setting_ColladaExportPath);
                State_ColladaExportPath = nodes.Item(0).InnerText;

                // Read ColladaExportOrientation
                nodes = SettingsXml.GetElementsByTagName(Setting_ColladaExportOrientation);
                State_ColladaExportOrientation = int.Parse(nodes.Item(0).InnerText);

                // Read ColladaExportImageFormat
                nodes = SettingsXml.GetElementsByTagName(Setting_ColladaExportImageFormat);
                State_ColladaExportImageFormat = int.Parse(nodes.Item(0).InnerText);

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

                // Write InvertMouseY
                nodes = SettingsXml.GetElementsByTagName(Setting_InvertMouseY);
                nodes.Item(0).InnerText = State_InvertMouseY.ToString();

                // Write InvertGamePadY
                nodes = SettingsXml.GetElementsByTagName(Setting_InvertGamePadY);
                nodes.Item(0).InnerText = State_InvertGamePadY.ToString();

                // Write ColladaExportPath
                nodes = SettingsXml.GetElementsByTagName(Setting_ColladaExportPath);
                nodes.Item(0).InnerText = State_ColladaExportPath;

                // Write ColladaExportOrientation
                nodes = SettingsXml.GetElementsByTagName(Setting_ColladaExportOrientation);
                nodes.Item(0).InnerText = State_ColladaExportOrientation.ToString();

                // Write ColladaExportImageFormat
                nodes = SettingsXml.GetElementsByTagName(Setting_ColladaExportImageFormat);
                nodes.Item(0).InnerText = State_ColladaExportImageFormat.ToString();

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
