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
    /// SceneDocument proxy interface.
    /// </summary>
    internal interface ISceneDocumentProxy : IEditorProxy { }

    /// <summary>
    /// Encapsulates scene document properties for the editor.
    /// </summary>
    internal sealed class SceneDocumentProxy : BaseEditorProxy, ISceneDocumentProxy, IEditorProxy
    {

        #region Fields

        const string defaultName = "New Scene";
        const string categoryName = "Document";

        SceneDocument document;

        #endregion

        #region Editor Properties
        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="document">Scene document.</param>
        public SceneDocumentProxy(SceneDocument document)
            : base(document)
        {
            base.Name = defaultName;
            this.document = document;
        }

        #endregion

    }

}
