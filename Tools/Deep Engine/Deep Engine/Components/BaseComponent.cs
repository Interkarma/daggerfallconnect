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
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DeepEngine.Core;
using DeepEngine.World;
#endregion

namespace DeepEngine.Components
{

    /// <summary>
    /// Provides features common to all world components.
    /// </summary>
    public abstract class BaseComponent :
        IDisposable
    {

        #region Fields

        // Property values
        protected DeepCore core;
        protected bool enabled;
        protected object tag;

        #endregion

        #region Properties

        /// <summary>
        /// Gets engine core.
        /// </summary>
        public DeepCore Core
        {
            get { return core; }
        }

        /// <summary>
        /// Gets or sets enabled flag.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// Gets or sets custom tag.
        /// </summary>
        public object Tag
        {
            get { return tag; }
            set { tag = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        public BaseComponent(DeepCore core)
        {
            // Store values
            this.core = core;
            this.enabled = true;
            this.tag = null;
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Called when component should update itself.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last update.</param>
        /// <param name="caller">The entity calling the update.</param>
        public virtual void Update(TimeSpan elapsedTime, BaseEntity caller)
        {
            // Do nothing if disabled
            if (!enabled)
                return;
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Called when component is to be disposed.
        ///  Override if special handling needed
        ///  to dispose of component resources.
        /// </summary>
        public virtual void Dispose()
        {
        }

        #endregion

    }

}
