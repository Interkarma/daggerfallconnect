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
using ProjectEditor.Documents;
#endregion

namespace ProjectEditor.Proxies
{

    /// <summary>
    /// Daggerfall model proxy interface.
    /// </summary>
    internal interface IDaggerfallModelProxy : IEditorProxy { }

    internal sealed class DaggerfallModelProxy : BaseComponentProxy, IBaseComponentProxy
    {

        #region Fields

        const string defaultName = "Model";
        const string categoryName = "Model";

        DaggerfallModelComponent model;

        #endregion

        #region Editor Properties

        /// <summary>
        /// Gets or sets model id.
        /// </summary>
        [Category(categoryName), Description("ModelID of Daggerfall model.")]
        public uint ModelID
        {
            get { return model.ModelID; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("ModelID"));
                model.LoadModel(value);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="document">Scene document.</param>
        /// <param name="entity">Entity owning this proxy.</param>
        /// <param name="model">Model to proxy.</param>
        public DaggerfallModelProxy(SceneDocument document, EntityProxy entity, DaggerfallModelComponent model)
            : base(document, entity)
        {
            base.name = defaultName;
            this.model = model;

            // Add to parent entity
            base.Entity.Components.Add(model);
        }

        #endregion

        #region BaseEditorProxy Overrides

        /// <summary>
        /// Removes this proxy from editor.
        /// </summary>
        public override void Remove()
        {
            // Remove from entity
            if (entity != null)
            {
                entity.Components.Remove(model);
            }
        }

        #endregion

        #region BaseDrawableProxy Overrides

        /// <summary>
        /// Updates matrix.
        /// </summary>
        protected override void UpdateMatrix()
        {
            base.UpdateMatrix();

            this.model.Matrix = base.matrix;
        }

        #endregion

    }

}
