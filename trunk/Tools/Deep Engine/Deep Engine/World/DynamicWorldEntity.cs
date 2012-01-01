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
#endregion

namespace DeepEngine.World
{

    /// <summary>
    /// A world entity that can freely move within the scene.
    /// </summary>
    public class DynamicWorldEntity : BaseEntity
    {

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="scene">Scene to attach entity.</param>
        public DynamicWorldEntity(Scene scene)
            :base(scene)
        {
            // Set values
            base.dynamic = true;
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
                component.Update(gameTime);
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
            foreach (var component in components)
            {
                if (component is DrawableComponent)
                {
                    (component as DrawableComponent).Draw();
                }
                else if (component is LightComponent)
                {
                    scene.Core.Renderer.SubmitLight(component as LightComponent);
                }
            }
        }

        #endregion

    }

}
