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
    /// Entity proxy interface.
    /// </summary>
    internal interface IEntityProxy : IEditorProxy { }

    /// <summary>
    /// Encapsulates an entity for the editor.
    /// </summary>
    internal sealed class EntityProxy : BaseDrawableProxy, IEntityProxy
    {

        #region Fields

        const string defaultName = "Entity";
        const string behaviourCategoryName = "Behaviour";

        bool makeStatic = false;

        #endregion

        #region Editor Properties

        /// <summary>
        /// Gets or sets make static flag.
        /// </summary>
        [Category(behaviourCategoryName), Description("Make entity static.")]
        public bool MakeStatic
        {
            get { return makeStatic; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("MakeStatic"));
                makeStatic = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="document">Scene document.</param>
        /// <param name="entity">Entity to proxy.</param>
        public EntityProxy(SceneDocument document, DynamicEntity entity)
            : base(document, entity)
        {
            base.name = defaultName;
            this.entity = entity;
        }

        #endregion

        #region BaseDrawableProxy Overrides

        /// <summary>
        /// Updates matrix.
        /// </summary>
        protected override void UpdateMatrix()
        {
            base.UpdateMatrix();

            this.entity.Matrix = base.matrix;
        }

        #endregion

    }

}
