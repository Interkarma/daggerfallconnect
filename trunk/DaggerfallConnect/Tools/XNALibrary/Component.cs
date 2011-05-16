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
using XNALibrary;
#endregion

namespace XNALibrary
{

    /// <summary>
    /// Base class for drawable view components.
    /// </summary>
    public abstract class Component
    {
        #region Class Variables

        protected string arena2Path;
        protected GraphicsDevice graphicsDevice = null;
        protected bool enabled = true;
        protected Camera camera = null;

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
        /// Gets or sets camera.
        /// </summary>
        public Camera Camera
        {
            get { return camera; }
            set { camera = value; }
        }

        /// <summary>
        /// Gets or sets Arena2 path.
        /// </summary>
        public string Arena2Path
        {
            get { return arena2Path; }
            set { arena2Path = value; }
        }

        /// <summary>
        /// Gets or sets GraphicsDevice.
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
        /// <param name="device">Graphics Device.</param>
        /// <param name="arena2Path">Path to Arena2 folder.</param>
        public Component(GraphicsDevice graphicsDevice, string arena2Path)
        {
            this.graphicsDevice = graphicsDevice;
            this.arena2Path = arena2Path;
        }

        #endregion

        #region Public Abstract Methods

        /// <summary>
        /// Called when component must initialise.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Called when component should update animation.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last frame.</param>
        public abstract void Update(TimeSpan elapsedTime);

        /// <summary>
        /// Called when component should redraw.
        /// </summary>
        public abstract void Draw();

        #endregion
    }

}
