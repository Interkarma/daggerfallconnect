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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DeepEngine.Core;
using DeepEngine.World;
#endregion

namespace DeepEngine.Components
{

    /// <summary>
    /// Base for components that creates visible output.
    /// </summary>
    public class DrawableComponent : BaseComponent
    {
        #region Fields

        protected BoundingSphere boundingSphere;
        protected Matrix matrix = Matrix.Identity;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets local bounding sphere for visibility tests.
        /// </summary>
        public BoundingSphere BoundingSphere
        {
            get { return boundingSphere; }
            set { boundingSphere = value; }
        }

        /// <summary>
        /// Gets or sets local transform relative to entity.
        /// </summary>
        public Matrix Matrix
        {
            get { return matrix; }
            set { matrix = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        public DrawableComponent(DeepCore core)
            : base(core)
        {
            // Store values
            this.core = core;
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Called when component should draw itself.
        /// </summary>
        /// <param name="caller">Entity calling the draw operation.</param>
        public virtual void Draw(BaseEntity caller)
        {
            // Do nothing if disabled
            if (!enabled)
                return;
        }

        #endregion
    }

}
