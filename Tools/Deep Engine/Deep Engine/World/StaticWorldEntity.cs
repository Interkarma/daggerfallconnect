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
using DeepEngine.Utility;
#endregion

namespace DeepEngine.World
{

    /// <summary>
    /// A static world entity automatically combines supported geometry components
    ///  into static buffers. This entity is ideal for building efficient level geometry
    ///  that does not need to move in the scene. Remember to set a component's transform
    ///  prior to attaching as you will not be able to change it later.
    /// </summary>
    class StaticWorldEntity : BaseEntity
    {
        #region Fields

        StaticBatchBuilder batchBuilder;

        #endregion

        #region Properties
        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="scene">Scene to attach entity.</param>
        public StaticWorldEntity(Scene scene)
            : base(scene)
        {
            // Create batch builder
            batchBuilder = new StaticBatchBuilder(scene.Core.GraphicsDevice);
        }

        #endregion

        #region BaseEntity Overrides

        /// <summary>
        /// Called when entity should update itself.
        /// </summary>
        /// <param name="gameTime">GameTime.</param>
        public override void Update(GameTime gameTime)
        {
            // Do nothing if disabled
            if (!enabled)
                return;

            // Update all components
            foreach (BaseComponent component in components)
            {
                component.Update(gameTime, this);
            }
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
        /// Called when a component is added.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        protected override void ComponentAdded(object sender, ComponentCollection.ComponentAddedEventArgs e)
        {
            base.ComponentAdded(sender, e);
        }

        #endregion
    }

}
