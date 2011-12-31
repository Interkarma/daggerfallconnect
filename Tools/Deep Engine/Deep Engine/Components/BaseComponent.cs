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
using DeepEngine.World;
#endregion

namespace DeepEngine.Components
{

    /// <summary>
    /// Provides features common to all world components.
    /// </summary>
    public class BaseComponent :
        IDisposable
    {

        #region Fields

        // Property values
        protected BaseEntity entity;
        protected bool enabled;
        protected object tag;

        #endregion

        #region Properties

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

        /// <summary>
        /// Gets entity this component is attached to.
        /// </summary>
        public BaseEntity Entity
        {
            get { return entity; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="entity">Entity this component is being attached to.</param>
        public BaseComponent(BaseEntity entity)
        {
            // Store values
            this.entity = entity;
            this.enabled = true;
            this.tag = null;

            // Attach to entity
            this.entity.Components.Add(this);
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Called when component should update itself.
        /// </summary>
        /// <param name="gameTime">GameTime.</param>
        public virtual void Update(GameTime gameTime)
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
