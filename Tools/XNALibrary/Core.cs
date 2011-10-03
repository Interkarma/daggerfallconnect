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
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace XNALibrary
{

    /// <summary>
    /// Engine core.
    /// </summary>
    public class Core
    {
        #region Class Variables

        // Daggerfall
        private string arena2Path;

        // XNA
        IServiceProvider serviceProvider;
        GraphicsDevice graphicsDevice;
        ContentManager contentManager;

        #endregion

        #region Properties

        /// <summary>
        /// Gets Arena2 path set at construction.
        /// </summary>
        public string Arena2Path
        {
            get { return arena2Path; }
        }

        /// <summary>
        /// Gets GraphicsDevice set at construction.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get { return graphicsDevice; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="arena2Folder">Path to Arena2 folder.</param>
        /// <param name="serviceProvider">IServiceProvider .</param>
        public Core(string arena2Path, IServiceProvider serviceProvider)
        {
            // Store values
            this.arena2Path = arena2Path;
            this.serviceProvider = serviceProvider;
        }

        #endregion
    }

}
