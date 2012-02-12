﻿// Project:         Deep Engine
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
    /// Quad terrain proxy interface.
    /// </summary>
    internal interface IQuadTerrainProxy : IEditorProxy { }

    /// <summary>
    /// Encapsulates a quad terrain component for the editor.
    /// </summary>
    internal sealed class QuadTerrainProxy : BaseEditorProxy, IQuadTerrainProxy
    {

        #region Fields

        const string defaultName = "Terrain";
        const string categoryName = "Terrain";
        const string transformCategoryName = "Transform";

        QuadTerrainComponent quadTerrain;

        Matrix matrix = Matrix.Identity;
        Vector3 scale = Vector3.One;
        Vector3 position = Vector3.Zero;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the QuadTerrainComponent being proxied.
        /// </summary>
        [Browsable(false)]
        public QuadTerrainComponent Component
        {
            get { return quadTerrain; }
        }

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
        /// <param name="quadTerrain">Quad terrain to proxy.</param>
        public QuadTerrainProxy(SceneDocument document, QuadTerrainComponent quadTerrain)
            : base(document)
        {
            base.name = defaultName;
            this.quadTerrain = quadTerrain;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Updates matrix using current scale and position.
        /// </summary>
        private void UpdateMatrix()
        {
            // Create matrices
            Matrix scale = Matrix.CreateScale(Scale);
            Matrix translation = Matrix.CreateTranslation(Position);

            // Set matrix
            matrix = scale * translation;
            quadTerrain.Matrix = matrix;
        }

        #endregion

    }

}