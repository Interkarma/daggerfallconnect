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
    /// Default world entity that can move freely within the scene.
    ///  Supports adding static geometry from DrawableComponent-based components.
    /// </summary>
    public class WorldEntity : BaseEntity
    {

        #region Fields

        DeepCore core;
        StaticGeometryBuilder staticGeometryBuilder;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="scene">Scene to attach entity.</param>
        public WorldEntity(Scene scene)
            :base(scene)
        {
            // Save references
            core = scene.Core;

            // Create static geometry builder
            staticGeometryBuilder = new StaticGeometryBuilder(core.GraphicsDevice);
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

            // TODO: Draw static geometry

            // Draw all components
            foreach (BaseComponent component in components)
            {
                if (component is DrawableComponent)
                {
                    if (!component.IsStatic)
                        (component as DrawableComponent).Draw(this);
                }
                else if (component is LightComponent)
                {
                    core.Renderer.SubmitLight(component as LightComponent, this);
                }
            }
        }

        #endregion

        #region Event Overrides

        /// <summary>
        /// Event when component is added.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        protected override void ComponentAdded(object sender, ComponentCollection.ComponentAddedEventArgs e)
        {
            // Call base
            base.ComponentAdded(sender, e);

            // Handle static geometry components
            if (e.IsStatic && e.Component is DrawableComponent)
            {
                // Get static geometry from component
                StaticGeometryBuilder builder = (e.Component as DrawableComponent).GetStaticGeometry(false, true);
            }
        }

        #endregion

    }

}
