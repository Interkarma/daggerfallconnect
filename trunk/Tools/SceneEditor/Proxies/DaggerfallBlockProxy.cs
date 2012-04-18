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
    /// Daggerfall block proxy interface.
    /// </summary>
    internal interface IDaggerfallBlockProxy : IEditorProxy { }

    internal sealed class DaggerfallBlockProxy : BaseComponentProxy, IBaseComponentProxy
    {

        #region Fields

        const string defaultName = "Block";
        const string categoryName = "Block";

        DaggerfallBlockComponent block;

        #endregion

        #region Editor Properties

        /// <summary>
        /// Gets or sets block name.
        /// </summary>
        [Category(categoryName), Description("Name of Daggerfall block.")]
        public string BlockName
        {
            get { return block.BlockName; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("BlockName"));
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="document">Scene document.</param>
        /// <param name="entity">Entity owning this proxy.</param>
        /// <param name="block">Block to proxy.</param>
        public DaggerfallBlockProxy(SceneDocument document, EntityProxy entity, DaggerfallBlockComponent block)
            : base(document, entity)
        {
            base.name = defaultName;
            this.block = block;

            // Add to parent entity
            base.Entity.Components.Add(block);
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
                entity.Components.Remove(block);
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

            this.block.Matrix = base.matrix;
        }

        #endregion

    }

}
