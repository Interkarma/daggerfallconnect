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
    internal interface IWorldEntityProxy : IEditorProxy { }

    /// <summary>
    /// Encapsulates a world entity for the editor.
    /// </summary>
    internal sealed class WorldEntityProxy : BaseEditorProxy, IWorldEntityProxy, IEditorProxy
    {

        #region Fields

        WorldEntity entity;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets scale.
        /// </summary>
        [Description("Scaling of entity.")]
        public Vector3 Scale { get; set; }

        /// <summary>
        /// Gets or sets rotation.
        /// </summary>
        [Description("Rotation of entity.")]
        public Vector3 Rotation { get; set; }

        /// <summary>
        /// Gets or sets position.
        /// </summary>
        [Description("Position of entity.")]
        public Vector3 Position { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sceneDocument">Scene document.</param>
        public WorldEntityProxy(SceneDocument sceneDocument)
            : base(sceneDocument)
        {
            entity = new WorldEntity(sceneDocument.EditorScene);
        }

        #endregion

    }

}
