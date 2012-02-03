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
using SceneEditor.Documents;
#endregion

namespace SceneEditor.Proxies
{

    /// <summary>
    /// WorldEntity proxy interface.
    /// </summary>
    internal interface IWorldEntityProxy : IEditorProxy { }

    /// <summary>
    /// Encapsulates a world entity for the editor.
    /// </summary>
    internal sealed class WorldEntityProxy : BaseEditorProxy, IWorldEntityProxy, IEditorProxy
    {

        #region Fields

        const string defaultName = "Entity";
        const string categoryName = "Entity";

        WorldEntity entity;
        Vector3 scale = Vector3.One;
        Vector3 rotation = Vector3.Zero;
        Vector3 position = Vector3.Zero;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets scale.
        /// </summary>
        [Category(categoryName), Description("Scaling of entity.")]
        public Vector3 Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("Scale"));
                UpdateEntityMatrix();
            }
        }

        /// <summary>
        /// Gets or sets rotation in degrees.
        /// </summary>
        [Category(categoryName), Description("Rotation of entity in degrees.")]
        public Vector3 Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("Rotation"));
                UpdateEntityMatrix();
            }
        }

        /// <summary>
        /// Gets or sets position.
        /// </summary>
        [Category(categoryName), Description("Position of entity.")]
        public Vector3 Position
        {
            get { return position; }
            set
            {
                position = value;
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("Position"));
                UpdateEntityMatrix();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="document">Scene document.</param>
        /// <param name="entity">Entity to proxy.</param>
        public WorldEntityProxy(SceneDocument document, WorldEntity entity)
            : base(document)
        {
            base.name = defaultName;
            this.entity = entity;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates entity matrix from scale, rotation, position.
        /// </summary>
        private void UpdateEntityMatrix()
        {
            // Convert rotation to radians
            float yaw = MathHelper.ToRadians(Rotation.Y);
            float pitch = MathHelper.ToRadians(Rotation.X);
            float roll = MathHelper.ToRadians(Rotation.Z);

            // Create matrices
            Matrix scale = Matrix.CreateScale(Scale);
            Matrix rotation = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
            Matrix translation = Matrix.CreateTranslation(Position);

            // Set entity matrix
            entity.Matrix = scale * rotation * translation;
        }

        #endregion

    }

}
