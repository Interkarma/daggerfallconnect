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
    /// Sphere proxy interface.
    /// </summary>
    internal interface ISphereProxy : IEditorProxy { }

    /// <summary>
    /// Encapsulates a sphere primitive for the editor.
    /// </summary>
    internal sealed class SphereProxy : BasePrimitiveProxy, ISphereProxy
    {

        #region Fields

        const string defaultName = "Sphere";
        const string categoryName = "Sphere";

        float radius = 0.5f;
        int tessellation = 8;

        #endregion

        #region Editor Properties

        /// <summary>
        /// Gets or sets sphere radius.
        /// </summary>
        [Category(categoryName), Description("Radius of sphere.")]
        public float Radius
        {
            get { return radius; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("Radius"));
                radius = value;
                UpdatePrimitive();
            }
        }

        /// <summary>
        /// Gets or sets sphere tessellation.
        /// </summary>
        [Category(categoryName), Description("Tessellation of sphere.")]
        public int Tessellation
        {
            get { return tessellation; }
            set
            {
                if (value < 3) value = 3;
                if (value > 64) value = 64;
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("Tessellation"));
                tessellation = value;
                UpdatePrimitive();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="document">Scene document.</param>
        /// <param name="entity">Parent entity</param>
        public SphereProxy(SceneDocument document, EntityProxy entity)
            : base(document, entity)
        {
            base.name = defaultName;
        }

        #endregion

        #region BasePrimitiveProxy Overrides

        /// <summary>
        /// Updates primitive using current properties.
        /// </summary>
        protected override void UpdatePrimitive()
        {
            primitive.MakeSphere(radius, tessellation);
        }

        #endregion

    }

}
