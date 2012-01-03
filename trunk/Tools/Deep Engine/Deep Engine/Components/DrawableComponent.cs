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
using DeepEngine.Utility;
using DeepEngine.World;
#endregion

namespace DeepEngine.Components
{

    /// <summary>
    /// Base for components that creates visible output.
    /// </summary>
    public abstract class DrawableComponent : BaseComponent
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
        ///  Cannot change matrix of static components.
        /// </summary>
        public Matrix Matrix
        {
            get { return matrix; }
            set { if (!base.isStatic) matrix = value; }
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

        #region Abstract Methods

        /// <summary>
        /// Called when component should draw itself.
        /// </summary>
        /// <param name="caller">Entity calling the draw operation.</param>
        public abstract void Draw(BaseEntity caller);

        /// <summary>
        /// Provides static geometry.
        /// </summary>
        /// <param name="applyBuilder">Request to apply builder before completion. Caller may only require geometry temporarily, so this optional.</param>
        /// <param name="cleanUpLocalContent">Request to clean up local copies of drawable content after being made static.</param>
        /// <returns>Static geometry builder.</returns>
        public abstract StaticGeometryBuilder GetStaticGeometry(bool applyBuilder, bool cleanUpLocalContent);

        #endregion
    }

}
