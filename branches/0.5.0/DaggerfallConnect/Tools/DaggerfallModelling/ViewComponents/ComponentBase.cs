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
using System.IO;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using XNALibrary;
using DaggerfallModelling.ViewControls;
#endregion

namespace DaggerfallModelling.ViewComponents
{

    /// <summary>
    /// Base class for drawable view components.
    /// </summary>
    public abstract class ComponentBase
    {
        #region Class Variables

        protected ViewHost host = null;
        protected bool enabled = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets enabled state.
        /// </summary>
        public bool Enabled
        {
            get { return this.enabled; }
            set { this.enabled = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public ComponentBase(ViewHost host)
        {
            this.host = host;
        }

        #endregion

        #region Public Abstract Methods

        /// <summary>
        /// Called when component must initialise.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Called when view component should tick animation.
        /// </summary>
        public abstract void Tick();

        /// <summary>
        /// Called when view component should redraw.
        /// </summary>
        public abstract void Draw();

        #endregion
    }

}
