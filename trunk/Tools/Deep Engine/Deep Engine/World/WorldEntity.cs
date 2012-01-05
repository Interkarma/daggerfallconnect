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

        // Constant strings
        const string entitySealedError = "Cannot add a static component to a sealed entity.";

        DeepCore core;
        StaticGeometryBuilder staticGeometry = null;
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

            // Handle dispose on update
            if (base.disposeOnUpdate)
            {
                Dispose();
                return;
            }

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

            // Draw static geometry
            if (staticGeometry != null)
            {
                DrawStaticGeometry();
            }

            // Draw all components
            foreach (BaseComponent component in components)
            {
                if (component is DrawableComponent && !component.IsStatic)
                {
                    // Get bounding sphere of component transformed to entity space
                    BoundingSphere sphere = (component as DrawableComponent).BoundingSphere.Transform(matrix);

                    // Only draw if component is visible by camera
                    if (sphere.Intersects(core.ActiveScene.DeprecatedCamera.BoundingFrustum))
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
                        if (sphere.Intersects(core.ActiveScene.DeprecatedCamera.BoundingFrustum))
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

            // Dispose static geometry
            if (staticGeometry != null)
                staticGeometry.Dispose();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Seals the static geometry builder, finalising build and making
        ///  static buffers available for draw operations. Always seal after
        ///  you have finished adding static components.
        ///  Cannot be unsealed, but dynamic components can still be added as normal.
        /// </summary>
        public void SealStaticGeometry()
        {
            // Seal static geometry
            if (staticGeometry != null)
            {
                staticGeometry.ApplyBuilder();
                staticGeometry.Seal();
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
            // Do nothing if no batch dictionary
            if (staticGeometry.StaticBatches == null)
                return;

            // Update effect
            renderGeometryEffect.Parameters["World"].SetValue(this.Matrix);
            renderGeometryEffect.Parameters["View"].SetValue(core.ActiveScene.DeprecatedCamera.View);
            renderGeometryEffect.Parameters["Projection"].SetValue(core.ActiveScene.DeprecatedCamera.Projection);

            // Set buffers
            core.GraphicsDevice.SetVertexBuffer(staticGeometry.VertexBuffer);
            core.GraphicsDevice.Indices = staticGeometry.IndexBuffer;

            // Draw batches
            Texture2D diffuseTexture = null;
            foreach (var item in staticGeometry.StaticBatches)
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
                        staticGeometry.VertexBuffer.VertexCount,
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
                // Create static geometry builder first time a static component is added
                if (this.staticGeometry == null)
                    staticGeometry = new StaticGeometryBuilder(core.GraphicsDevice);

                // Check seal
                if (staticGeometry.IsSealed)
                    throw new Exception(entitySealedError);

                // Get static geometry from component
                StaticGeometryBuilder builder = (e.Component as DrawableComponent).GetStaticGeometry();

                // Add to entity static geometry
                this.staticGeometry.AddToBuilder(builder, (e.Component as DrawableComponent).Matrix);
            }
        }

        #endregion

    }

}
