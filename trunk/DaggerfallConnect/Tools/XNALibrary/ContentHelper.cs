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
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace XNALibrary
{

    /// <summary>
    /// Groups Daggerfall content managers and provides
    ///  basic XNA content loading for external WinForms projects.
    /// </summary>
    public class ContentHelper
    {

        #region Class Variables

        // XNA content manager
        private ContentManager contentManager = null;

        // Daggerfall Connect content managers
        private TextureManager textureManager = null;
        private ModelManager modelManager = null;
        private BlockManager blockManager = null;
        private MapManager mapManager = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the XNA ContentManager.
        /// </summary>
        public ContentManager ContentManager
        {
            get { return contentManager; }
        }

        /// <summary>
        /// Gets or sets TextureManager.
        /// </summary>
        public TextureManager TextureManager
        {
            get { return textureManager; }
            set { textureManager = value; }
        }

        /// <summary>
        /// Gets or sets ModelManager.
        /// </summary>
        public ModelManager ModelManager
        {
            get { return modelManager; }
            set { modelManager = value; }
        }

        /// <summary>
        /// Gets or sets BlockManager.
        /// </summary>
        public BlockManager BlockManager
        {
            get { return blockManager; }
            set { blockManager = value; }
        }

        /// <summary>
        /// Gets or sets MapManager.
        /// </summary>
        public MapManager MapManager
        {
            get { return mapManager; }
            set { mapManager = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor to enable Daggerfall content loading.
        /// </summary>
        /// <param name="graphicsDevice">GraphicsDevice.</param>
        /// <param name="arena2Directory">Arena2 content root directory.</param>
        public ContentHelper(
            GraphicsDevice graphicsDevice,
            string arena2Directory)
        {
            // Initialise DaggerfallConnect content managers
            try
            {
                this.textureManager = new TextureManager(graphicsDevice, arena2Directory);
                this.modelManager = new ModelManager(graphicsDevice, arena2Directory);
                this.blockManager = new BlockManager(graphicsDevice, arena2Directory);
                this.mapManager = new MapManager(arena2Directory);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }

        /// <summary>
        /// Constructor to enable XNA ContentManager for external WinForms applications.
        ///  XNA Content must be added to the XNALibrary Content project and ContentHelper
        ///  instantiated using the IServiceProvider and content path at runtime.
        ///  ContentHelper.ContentManager.RootDirectory is normally set to Application.StartupPath\Content.
        ///  As long as the application is part of the same solution as XNALibrary,
        ///  the Content directory and compiled XNB files will be copied into the application
        ///  bin folder at build time. Content can then be loaded from the external application using
        ///  ContentHelper.ContentManager.Load<>().
        /// </summary>
        /// <param name="graphicsDevice">GraphicsDevice.</param>
        /// <param name="arena2Directory">Arena2 content root directory.</param>
        /// <param name="serviceProvider">IServiceProvider.</param>
        /// <param name="contentRootDirectory">XNA content root directory.</param>
        public ContentHelper(
            GraphicsDevice graphicsDevice,
            string arena2Directory,
            IServiceProvider serviceProvider,
            string contentRootDirectory)
            : this(graphicsDevice, arena2Directory)
        {
            // Initialise XNA content manager
            contentManager = new ContentManager(serviceProvider);
            contentManager.RootDirectory = contentRootDirectory;
        }

        #endregion

    }

}
