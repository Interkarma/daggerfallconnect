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

        QuadTerrainComponent quadTerrain;

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

    }

}
