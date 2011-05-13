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
using System.Drawing;
using System.Threading;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace XNALibrary
{

    /// <summary>
    /// Helper class for loading Daggerfall environments.
    ///  Provides a generic scene graph for structuring blocks,
    ///  models, billboards, actions, etc.
    /// </summary>
    public class SceneManager
    {

        #region Class Variables

        private SceneNode scene;

        #endregion

        #region Class Structures
        #endregion

        #region SubClasses
        #endregion

        #region Static Properties

        /// <summary>Creates a unique ID for scene nodes by counting.</summary>
        private static uint IDCounter = 0;

        /// <summary>
        /// Gets next ID.
        /// </summary>
        public static uint NextID
        {
            get { return IDCounter++; }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets root scene node.
        /// </summary>
        public SceneNode Scene
        {
            get { return scene; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SceneManager()
        {
            scene = new SceneNode();
        }

        #endregion

    }

}
