#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

#endregion

namespace XNALibrary
{

    /// <summary>
    /// Provides basic content loading for external WinForms projects.
    ///  Content must be added to the XNALibrary Content project and ContentHelper
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

        private ContentManager contentManager;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ContentManager.
        /// </summary>
        public ContentManager ContentManager
        {
            get { return contentManager; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="serviceProvider"></param>
        public ContentHelper(IServiceProvider serviceProvider, string contentRootDirectory)
        {
            // Initialise content manager
            contentManager = new ContentManager(serviceProvider);
            contentManager.RootDirectory = contentRootDirectory;
        }

        #endregion

    }

}
