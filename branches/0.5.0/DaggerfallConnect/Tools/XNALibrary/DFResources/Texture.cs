// Project:         XNALibrary
// Description:     Simple XNA game library for DaggerfallConnect.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/using System;

#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace XNALibrary.DFResources
{

    /// <summary>
    /// Each Texture references part of a texture atlas.
    /// </summary>
    class Texture
    {

        #region Class Variables

        TextureLayout textureLayout;

        #endregion

        #region Class Structures

        /// <summary>
        /// Defines texture layout in atlas.
        /// </summary>
        public struct TextureLayout
        {
            public float top;
            public float bottom;
            public float left;
            public float right;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets texture layout in atlas.
        /// </summary>
        TextureLayout Layout
        {
            get { return textureLayout; }
            set { textureLayout = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Texture()
        {
        }

        #endregion

    }

}
