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
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DeepEngine.Core;
using DeepEngine.Components;
using DeepEngine.World;
using DeepEngine.Utility;
using SceneEditor.Documents;
#endregion

namespace SceneEditor.Proxies
{

    /// <summary>
    /// Drawable item proxy interface.
    /// </summary>
    internal interface IBaseDrawableProxy : IEditorProxy { }

    /// <summary>
    /// Common items to encapsulate for all drawable proxies.
    /// </summary>
    internal abstract class BaseDrawableProxy : BaseEditorProxy, IBaseDrawableProxy
    {

        #region Fields

        const string transformCategoryName = "Transform";

        protected Matrix matrix = Matrix.Identity;
        protected Vector3 scale = Vector3.One;
        protected Vector3 rotation = Vector3.Zero;
        protected Vector3 position = Vector3.Zero;

        #endregion

        #region Editor Properties

        /// <summary>
        /// Gets or sets scale.
        /// </summary>
        [Category(transformCategoryName), Description("Scaling of entity.")]
        public Vector3 Scale
        {
            get { return scale; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("Scale"));
                scale = value;
                UpdateMatrix();
            }
        }

        /// <summary>
        /// Gets or sets rotation in degrees.
        /// </summary>
        [Category(transformCategoryName), Description("Rotation of entity in degrees.")]
        public Vector3 Rotation
        {
            get { return rotation; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("Rotation"));
                rotation = value;
                UpdateMatrix();
            }
        }

        /// <summary>
        /// Gets or sets position.
        /// </summary>
        [Category(transformCategoryName), Description("Position of entity.")]
        public Vector3 Position
        {
            get { return position; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("Position"));
                position = value;
                UpdateMatrix();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="document">Scene document.</param>
        public BaseDrawableProxy(SceneDocument document)
            : base(document)
        {
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Updates matrix using current scale, rotation, position.
        /// </summary>
        protected virtual void UpdateMatrix()
        {
            // Convert rotation to radians
            float yaw = MathHelper.ToRadians(Rotation.Y);
            float pitch = MathHelper.ToRadians(Rotation.X);
            float roll = MathHelper.ToRadians(Rotation.Z);

            // Create matrices
            Matrix scale = Matrix.CreateScale(Scale);
            Matrix rotation = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
            Matrix translation = Matrix.CreateTranslation(Position);

            // Set matrix
            matrix = scale * rotation * translation;
        }

        #endregion

    }

}
