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
    /// Default world entity that can move freely within the scene.
    /// </summary>
    public class WorldEntity : BaseEntity
    {

        #region Fields

        DeepCore core;

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
                // Update component
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

            // Draw all components
            foreach (BaseComponent component in components)
            {
                if (component is DrawableComponent && !component.IsStatic)
                {
                    // Get bounding sphere of component transformed to entity space
                    BoundingSphere sphere = (component as DrawableComponent).BoundingSphere.Transform(matrix);

                    // Only draw if component is visible by camera
                    if (sphere.Intersects(core.ActiveScene.Camera.BoundingFrustum))
                        (component as DrawableComponent).Draw(this);
                }
                else if (component is LightComponent)
                {
                    // Point lights must be visible to camera
                    if ((component as LightComponent).Type == LightComponent.LightType.Point)
                    {
                        // Get bounding sphere of component transformed to entity space
                        BoundingSphere sphere = (component as LightComponent).BoundingSphere.Transform(matrix);

                        // Only draw if component is visible by camera
                        if (sphere.Intersects(core.ActiveScene.Camera.BoundingFrustum))
                            core.Renderer.SubmitLight(component as LightComponent, this);
                    }
                    else
                    {
                        core.Renderer.SubmitLight(component as LightComponent, this);
                    }
                }
            }
        }

        /// <summary>
        /// Frees resources used by this object when they are no longer needed.
        ///  Destroys all static geometry.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
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
        }

        #endregion

    }

}
