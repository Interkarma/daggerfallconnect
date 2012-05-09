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
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ionic.Zip;
using DeepEngine.Core;
using DeepEngine.Components;
using DeepEngine.World;
using DeepEngine.Utility;
using ProjectEditor.Proxies;
using ProjectEditor.UserControls;
#endregion

namespace ProjectEditor.Documents
{

    /// <summary>
    /// Interface to a project file. Handles adding, deleting, moving, and renaming files and folders.
    ///  Manages a single zipped archive containing all files.
    /// </summary>
    class ProjectDocument
    {

        #region Fields

        // Constants
        const string projectExtension = ".zip";
        const string projectDescription = "Deep Engine Project File";
        const string projectSchema = "ProjectDirectory";
        const string projectDirectoryFileName = "_ProjectDirectory.xml";
        const string projectElement = "Project";
        const string projectNameAttribute = "Name";

        // Project xml document
        ZipFile projectFile;
        XmlDocument projectDocument;

        // State
        bool projectOpen = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets extension of a project file.
        /// </summary>
        static public string Extension
        {
            get { return projectExtension; }
        }

        /// <summary>
        /// Gets description of a project file.
        /// </summary>
        static public string Description
        {
            get { return projectDescription; }
        }

        /// <summary>
        /// Gets filter for file dialogs.
        /// </summary>
        static public string Filter
        {
            get { return string.Format("{0} (*{1})|*{1}", projectDescription, projectExtension); }
        }

        /// <summary>
        /// Gets flag stating if project is open or closed.
        /// </summary>
        public bool IsProjectOpen
        {
            get { return projectOpen; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public ProjectDocument()
        {
        }
       
        #endregion

        #region Public Methods

        /// <summary>
        /// Creates empty project file at specified path.
        /// </summary>
        /// <param name="filename">Full path to project file.</param>
        public void CreateProjectFile(string filename)
        {
            // Create project zip file
            projectFile = new ZipFile(filename);

            // Create project file inside of zip file
            string name = Path.GetFileNameWithoutExtension(filename);
            CreateProjectDirectoryFile(name, projectFile);

            // Save file
            projectFile.Save();

            // Flag as open
            projectOpen = true;
        }

        /// <summary>
        /// Opens existing project file at specified path.
        /// </summary>
        /// <param name="filename">Full path to project file.</param>
        public void OpenProjectFile(string filename)
        {
            // Open project zip file
            projectFile = ZipFile.Read(filename);

            // Read project file inside of zip file
            OpenProjectDirectoryFile(projectFile);

            // Flag as open
            projectOpen = true;
        }

        /// <summary>
        /// Close an open project file.
        /// </summary>
        public void Close()
        {
            // Close document
            if (projectDocument != null)
            {
                projectDocument = null;
            }

            // Close project file
            if (projectFile != null)
            {
                projectFile.Dispose();
                projectFile = null;
            }

            // Flag as closed
            projectOpen = false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates project directory inside specified project file.
        /// </summary>
        /// <param name="name">Name of directory file.</param>
        /// <param name="file">Project zip file.</param>
        private void CreateProjectDirectoryFile(string name, ZipFile file)
        {
            // Create document
            projectDocument = XmlHelper.CreateXmlDocument(projectSchema);
            XmlElement element = XmlHelper.AppendElement(projectDocument, null, projectElement, null);
            element.SetAttribute(projectNameAttribute, name);

            // Add project document to zip file
            file.UpdateEntry(projectDirectoryFileName, XmlHelper.ToMemoryStream(projectDocument));
        }

        /// <summary>
        /// Opens project directory inside specified project file.
        /// </summary>
        /// <param name="projectFile">Project zip file.</param>
        private void OpenProjectDirectoryFile(ZipFile file)
        {
        }

        /// <summary>
        /// Updates project directory file inside project zip file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="document"></param>
        private void UpdateProjectDirectoryFile(ZipFile file, XmlDocument document)
        {
        }

        #endregion

    }

}
