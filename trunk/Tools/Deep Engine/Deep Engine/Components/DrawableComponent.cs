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
        /// </summary>
        public virtual Matrix Matrix
        {
            get { return matrix; }
            set { matrix = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        public DrawableComponent(DeepCore core)
            : base(core)
        {
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Packs an identity into a float4 structure.
        /// </summary>
        /// <param name="identity">Identity to pack.</param>
        /// <returns>Packed identity vector.</returns>
        protected Vector4 PackIdentity(uint identity)
        {
            Color packedIdentity = Color.Transparent;
            packedIdentity.A = (byte)(identity & 0xff);
            packedIdentity.B = (byte)((identity >> 8) & 0xff);
            packedIdentity.G = (byte)((identity >> 16) & 0xff);
            packedIdentity.R = (byte)((identity >> 24) & 0xff);

            return packedIdentity.ToVector4();
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Called when component should draw itself.
        /// </summary>
        /// <param name="caller">Entity calling the draw operation.</param>
        /// <param name="identity">Identity to associate with draw operation.</param>
        public abstract void Draw(BaseEntity caller, uint identity);

        #endregion
    }

}
