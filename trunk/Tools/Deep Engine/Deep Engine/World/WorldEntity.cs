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
    ///  Supports adding static geometry from DrawableComponent-based components.
    ///  Note that null-textured geometry, such as primitives, will be drawn white
    ///  regardless of colour set in component.
    /// </summary>
    public class WorldEntity : BaseEntity
    {

        #region Fields

        DeepCore core;
        StaticGeometryBuilder staticGeometryBuilder;
        Effect renderGeometryEffect;

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

            // Load effect for rendering static batches
            renderGeometryEffect = core.ContentManager.Load<Effect>("Effects/RenderGeometry");

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

            // Draw static geometry
            DrawStaticGeometry();

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

        #region Private Methods

        /// <summary>
        /// Draws static geometry contained in this entity.
        ///  All static geometry is relative to the entities world transform.
        /// </summary>
        private void DrawStaticGeometry()
        {
            // Do nothing if no buffers or batches
            if (staticGeometryBuilder.VertexBuffer == null || 
                staticGeometryBuilder.IndexBuffer == null ||
                staticGeometryBuilder.StaticBatches == null)
                return;

            // Update effect
            renderGeometryEffect.Parameters["World"].SetValue(this.Matrix);
            renderGeometryEffect.Parameters["View"].SetValue(core.ActiveScene.DeprecatedCamera.View);
            renderGeometryEffect.Parameters["Projection"].SetValue(core.ActiveScene.DeprecatedCamera.Projection);

            // Set buffers
            core.GraphicsDevice.SetVertexBuffer(staticGeometryBuilder.VertexBuffer);
            core.GraphicsDevice.Indices = staticGeometryBuilder.IndexBuffer;

            // Draw batches
            Texture2D diffuseTexture = null;
            foreach (var item in staticGeometryBuilder.StaticBatches)
            {
                int textureKey = item.Key;
                if (textureKey == MaterialManager.NullTextureKey)
                {
                    // Set diffuse effect
                    renderGeometryEffect.Parameters["DiffuseColor"].SetValue(Vector3.One);
                    renderGeometryEffect.CurrentTechnique = renderGeometryEffect.Techniques["Diffuse"];
                }
                else
                {
                    // Set texture effect
                    diffuseTexture = core.MaterialManager.GetTexture(item.Key);
                    renderGeometryEffect.Parameters["Texture"].SetValue(diffuseTexture);
                    renderGeometryEffect.CurrentTechnique = renderGeometryEffect.Techniques["Default"];
                }

                // Render geometry
                foreach (EffectPass pass in renderGeometryEffect.CurrentTechnique.Passes)
                {
                    // Apply effect pass
                    pass.Apply();

                    // Draw batched indexed primitives
                    core.GraphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        0,
                        0,
                        staticGeometryBuilder.VertexBuffer.VertexCount,
                        item.Value.StartIndex,
                        item.Value.PrimitiveCount);
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
                StaticGeometryBuilder builder = (e.Component as DrawableComponent).GetStaticGeometry(false, false);

                // Create new matrix for static component
                this.staticGeometryBuilder.AddToBuilder(builder, (e.Component as DrawableComponent).Matrix);
                this.staticGeometryBuilder.ApplyBuilder();
            }
        }

        #endregion

    }

}
