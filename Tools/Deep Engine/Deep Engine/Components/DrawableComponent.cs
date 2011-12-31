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

        GraphicsDevice graphicsDevice;
        BoundingSphere boundingSphere;
        Matrix matrix = Matrix.Identity;

        #endregion

        #region Properties

        /// <summary>
        /// Gets GraphicsDevice.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get { return graphicsDevice; }
        }

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
        /// <param name="entity">Entity this component is attached to.</param>
        public DrawableComponent(BaseEntity entity)
            :base(entity)
        {
            // Store values
            this.graphicsDevice = entity.Scene.Core.GraphicsDevice;
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Called when component should draw itself.
        /// </summary>
        public virtual void Draw()
        {
        }

        #endregion
    }

}
