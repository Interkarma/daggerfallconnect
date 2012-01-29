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
using System.ComponentModel;
using SceneEditor.Documents;
#endregion

namespace SceneEditor.Proxies
{

    /// <summary>
    /// Base proxy interface.
    /// </summary>
    internal interface IBaseProxy : IEditorProxy { }

    /// <summary>
    /// Defines base requirements of editor proxies.
    /// </summary>
    internal abstract class BaseEditorProxy : IBaseProxy, IEditorProxy
    {

        #region Fields

        SceneDocument sceneDocument;

        #endregion

        #region Properties

        /// <summary>
        /// Gets scene document this proxy belongs to.
        /// </summary>
        [Browsable(false)]
        public SceneDocument SceneDocument
        {
            get { return sceneDocument; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sceneDocument">Scene document.</param>
        public BaseEditorProxy(SceneDocument sceneDocument)
        {
            // Save references
            this.sceneDocument = sceneDocument;
        }

        #endregion

    }

}
