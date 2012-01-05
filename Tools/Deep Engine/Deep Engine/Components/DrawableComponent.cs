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
        /// Gets or sets bounding sphere for visibility tests.
        ///  Getting will return transformed bounding sphere based on Matrix.
        /// </summary>
        public BoundingSphere BoundingSphere
        {
            get { return boundingSphere.Transform(matrix); }
            set { boundingSphere = value; }
        }

        /// <summary>
        /// Gets or sets local transform relative to entity.
        ///  Cannot change matrix of static components.
        /// </summary>
        public virtual Matrix Matrix
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
        /// Gets static geometry.
        /// </summary>
        /// <returns>Static geometry builder.</returns>
        public abstract StaticGeometryBuilder GetStaticGeometry();

        #endregion
    }

}
