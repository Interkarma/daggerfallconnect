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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace DeepEngine.Deprecated
{

    /// <summary>
    /// Helper class to enable XNA ContentManager for external WinForms applications.
    ///  XNA Content must be added to the XNALibrary Content project and ContentHelper
    ///  instantiated using the IServiceProvider and content path at runtime.
    ///  ContentHelper.ContentManager.RootDirectory is normally set to Application.StartupPath\Content.
    ///  As long as the application is part of the same solution as XNALibrary,
    ///  the Content directory and compiled XNB files will be copied into the application
    ///  bin folder at build time. Content can then be loaded from the external application using
    ///  ContentHelper.ContentManager.Load<>().
    /// </summary>
    public class ContentHelper
    {

        #region Class Variables

        // XNA content manager
        private ContentManager contentManager = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the XNA ContentManager.
        /// </summary>
        public ContentManager ContentManager
        {
            get { return contentManager; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceProvider">IServiceProvider.</param>
        /// <param name="contentRootDirectory">XNA content root directory.</param>
        public ContentHelper(
            IServiceProvider serviceProvider,
            string contentRootDirectory)
        {
            // Initialise XNA content manager
            contentManager = new ContentManager(serviceProvider);
            contentManager.RootDirectory = contentRootDirectory;
        }

        #endregion

    }

}
