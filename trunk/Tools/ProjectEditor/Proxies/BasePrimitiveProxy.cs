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
    /// Geometric primitive proxy interface.
    /// </summary>
    internal interface IBasePrimitiveProxy : IEditorProxy { }

    /// <summary>
    /// Common items to encapsulate for all geometric primitive proxies.
    /// </summary>
    internal abstract class BasePrimitiveProxy : BaseComponentProxy, IBaseComponentProxy
    {

        #region Fields

        const string categoryName = "Primitive";

        protected GeometricPrimitiveComponent primitive;
        protected Microsoft.Xna.Framework.Color colorRGB;
        protected float colorW;

        #endregion

        #region Editor Properties

        /// <summary>
        /// Gets or sets RGB components of colour.
        /// </summary>
        [Category(categoryName), Description("Primitive colour RGB components.")]
        public System.Drawing.Color ColorRGB
        {
            get { return ColorHelper.FromXNA(colorRGB); }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("ColorRGB"));
                colorRGB = ColorHelper.FromWinForms(value);
                primitive.Color = new Vector4(colorRGB.ToVector3(), colorW);
            }
        }

        /// <summary>
        /// Gets or sets W component of colour.
        /// </summary>
        [Category(categoryName), Description("Primitive colour W component. 0.0 - 0.49 is specular, 0.5 - 1.0 is emissive.")]
        public float ColorW
        {
            get { return colorW; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("ColorW"));
                colorW = value;
                primitive.Color = new Vector4(colorRGB.ToVector3(), colorW);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="document">Scene document.</param>
        /// <param name="entity">Parent entity</param>
        public BasePrimitiveProxy(SceneDocument document, EntityProxy entity)
            : base(document, entity)
        {
            primitive = new GeometricPrimitiveComponent(document.EditorScene.Core);
            this.ColorRGB = System.Drawing.Color.White;
            this.ColorW = 0.0f;
            UpdatePrimitive();

            base.Entity.Components.Add(primitive);
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
                entity.Components.Remove(primitive);
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

            this.primitive.Matrix = base.matrix;
        }

        #endregion

        #region Abstract Methods

        protected abstract void UpdatePrimitive();

        #endregion

    }

}
