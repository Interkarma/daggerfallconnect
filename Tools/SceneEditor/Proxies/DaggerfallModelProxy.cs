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
    /// Daggerfall model proxy interface.
    /// </summary>
    internal interface IDaggerfallModelProxy : IEditorProxy { }

    internal sealed class DaggerfallModelProxy : BaseDrawableProxy, IDaggerfallModelProxy
    {

        #region Fields

        const string defaultName = "Model";
        const string categoryName = "Model";

        uint modelId = 456;

        #endregion

        #region Editor Properties

        /// <summary>
        /// Gets or sets sphere radius.
        /// </summary>
        [Category(categoryName), Description("ModelID of Daggerfall model.")]
        public uint ModelID
        {
            get { return modelId; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("ModelID"));
                modelId = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="document">Scene document.</param>
        /// <param name="entity">Parent entity</param>
        public DaggerfallModelProxy(SceneDocument document, BaseEntity entity)
            : base(document)
        {
            base.name = defaultName;
        }

        #endregion

    }

}
