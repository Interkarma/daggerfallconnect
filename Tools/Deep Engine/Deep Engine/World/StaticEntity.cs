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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DeepEngine.Core;
using DeepEngine.Components;
using DeepEngine.Daggerfall;
using DeepEngine.Utility;
#endregion

namespace DeepEngine.World
{

    /// <summary>
    /// Static world entity that cannot move within the scene.
    /// </summary>
    public sealed class StaticEntity : BaseEntity
    {

        #region Fields

        DeepCore core;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="scene">Scene to attach entity.</param>
        /// <param name="matrix">Initial transformation matrix.</param>
        public StaticEntity(Scene scene, Matrix matrix)
            : base(scene)
        {
            // Save references
            core = scene.Core;

            // Subscribe events
            base.Components.ComponentAdded += new ComponentCollection.ComponentAddedEventHandler(Components_ComponentAdded);
        }

        #endregion

        #region BaseEntity Overrides

        /// <summary>
        /// Called when entity should update itself.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last update.</param>
        public override void Update(TimeSpan elapsedTime)
        {
            // Do nothing if disabled
            if (!enabled)
                return;
        }

        /// <summary>
        /// Called when entity should draw itself.
        /// </summary>
        public override void Draw()
        {
            // Do nothing if disabled
            if (!enabled)
                return;
        }

        /// <summary>
        /// Frees resources used by this object when they are no longer needed.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        #endregion

        #region Events

        /// <summary>
        /// Called when a new component is added.
        /// </summary>
        private void Components_ComponentAdded(object sender, ComponentCollection.ComponentAddedEventArgs e)
        {
        }

        #endregion

    }

}
