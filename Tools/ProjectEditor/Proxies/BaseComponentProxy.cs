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
    /// Drawable item proxy interface.
    /// </summary>
    internal interface IBaseComponentProxy : IEditorProxy { }

    /// <summary>
    /// Common items to encapsulate for all component proxies.
    /// </summary>
    class BaseComponentProxy : BaseTransformableProxy, IBaseTransformableProxy
    {

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="document">Scene document.</param>
        /// <param name="entity">Entity owning this proxy.</param>
        public BaseComponentProxy(SceneDocument document, EntityProxy entity)
            : base(document, entity)
        {
        }

        #endregion

    }

}
