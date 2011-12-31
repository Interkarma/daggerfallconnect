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
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace DeepEngine.Deprecated
{

    /// <summary>
    /// Base class for components that plug into renderer.
    /// </summary>
    public abstract class Component
    {
        #region Class Variables

        private bool enabled = true;
        private GraphicsDevice graphicsDevice;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets enabled state.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// Gets or set graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get { return graphicsDevice; }
            set { graphicsDevice = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="graphicsDevice">Graphics device used in rendering.</param>
        public Component(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        #endregion

        #region Public Abstract Methods

        /// <summary>
        /// Called when component should redraw.
        /// </summary>
        /// <param name="camera">Camera looking into scene.</param>
        public abstract void Draw(Camera camera);

        #endregion
    }

}
