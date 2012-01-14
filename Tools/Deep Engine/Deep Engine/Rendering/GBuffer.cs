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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using DeepEngine.Core;
#endregion

namespace DeepEngine.Rendering
{

    /// <summary>
    /// Class for managing a deferred GBuffer.
    /// </summary>
    public class GBuffer
    {

        #region Fields

        // Engine
        DeepCore core;

        // XNA
        GraphicsDevice graphicsDevice;
        Viewport viewport;

        // Render targets
        RenderTarget2D colorRT;             // Color and specular intensity
        RenderTarget2D normalRT;            // Normals + specular power
        RenderTarget2D depthRT;             // Depth
        RenderTarget2D lightRT;             // Lighting

        // Ambient lighting
        Color ambientColor = Color.White;
        float ambientIntensity = 0.0f;

        // Size
        Vector2 size;
        Vector2 halfPixel;

        // Effects
        Effect drawDebugDepthBufferEffect;

        // Custom debug buffers
        RenderTarget2D depthDebugBuffer = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets size of GBuffer.
        /// </summary>
        public Vector2 Size
        {
            get { return size; }
        }

        /// <summary>
        /// Gets size of half pixel for shaders.
        /// </summary>
        public Vector2 HalfPixel
        {
            get { return halfPixel; }
        }

        /// <summary>
        /// Gets or sets color of basic ambient light.
        ///  Default is Color.White.
        /// </summary>
        public Color AmbientColor
        {
            get { return ambientColor; }
            set { ambientColor = value; }
        }

        /// <summary>
        /// Gets or sets intensity of basic ambient light.
        ///  Default is 0.0f;
        /// </summary>
        public float AmbientIntensity
        {
            get { return ambientIntensity; }
            set { ambientIntensity = value; }
        }

        /// <summary>
        /// Gets diffuse render target.
        /// </summary>
        public RenderTarget2D ColorRT
        {
            get { return colorRT; }
        }

        /// <summary>
        /// Gets normal render target.
        /// </summary>
        public RenderTarget2D NormalRT
        {
            get { return normalRT; }
        }

        /// <summary>
        /// Gets depth render target.
        /// </summary>
        public RenderTarget2D DepthRT
        {
            get { return depthRT; }
        }

        /// <summary>
        /// Gets lighting render target.
        /// </summary>
        public RenderTarget2D LightRT
        {
            get { return lightRT; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        public GBuffer(DeepCore core)
        {
            // Save references
            this.core = core;
            this.graphicsDevice = core.GraphicsDevice;

            // Load content
            drawDebugDepthBufferEffect = core.ContentManager.Load<Effect>("Effects/DrawDebugDepthBuffer");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates GBuffer.
        /// </summary>
        public void CreateGBuffer()
        {
            // Dispose of previous targets
            if (colorRT != null) colorRT.Dispose();
            if (normalRT != null) normalRT.Dispose();
            if (depthRT != null) depthRT.Dispose();
            if (lightRT != null) lightRT.Dispose();

            // Save viewport size as this needs to be restored every time SetRenderTarget(null)
            viewport = graphicsDevice.Viewport;

            // Get width and height
            int width = graphicsDevice.Viewport.Width;
            int height = graphicsDevice.Viewport.Height;
            halfPixel = new Vector2(0.5f / (float)width, 0.5f / (float)height);
            size.X = width;
            size.Y = height;

            // Create render targets
            colorRT = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            normalRT = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);
            depthRT = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Single, DepthFormat.None);
            lightRT = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);
        }

        /// <summary>
        /// Sets GBuffer render targets.
        /// </summary>
        public void SetGBuffer()
        {
            graphicsDevice.SetRenderTargets(colorRT, normalRT, depthRT);
        }

        /// <summary>
        /// Resets render targets back to null and restores viewport.
        ///  Only use this at end of render pipeline.
        /// </summary>
        public void ResolveGBuffer()
        {
            graphicsDevice.SetRenderTargets(null);
            graphicsDevice.Viewport = viewport;
        }

        /// <summary>
        /// Clear GBuffer.
        /// </summary>
        /// <param name="clearBufferEffect">Effect to clear GBuffer with.</param>
        /// <param name="fullScreenQuad">FullScreenQuad class.</param>
        /// <param name="clearColor">Clear colour for diffuse buffer.</param>
        public void ClearGBuffer(Effect clearBufferEffect, FullScreenQuad fullScreenQuad, Color clearColor)
        {
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.None;
            clearBufferEffect.Parameters["ClearColor"].SetValue(clearColor.ToVector3());
            clearBufferEffect.Techniques[0].Passes[0].Apply();
            fullScreenQuad.Draw(graphicsDevice);
        }

        /// <summary>
        /// Compose buffers into one layer.
        /// </summary>
        public void ComposeFinal(Effect finalCombineEffect, FullScreenQuad fullScreenQuad)
        {
            // Set render states
            graphicsDevice.BlendState = BlendState.AlphaBlend;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Set values
            finalCombineEffect.Parameters["ColorMap"].SetValue(colorRT);
            finalCombineEffect.Parameters["LightMap"].SetValue(lightRT);
            finalCombineEffect.Parameters["DepthMap"].SetValue(depthRT);
            finalCombineEffect.Parameters["HalfPixel"].SetValue(halfPixel);
            finalCombineEffect.Parameters["AmbientColor"].SetValue(ambientColor.ToVector3());
            finalCombineEffect.Parameters["AmbientIntensity"].SetValue(ambientIntensity);

            // Apply changes and draw
            finalCombineEffect.Techniques[0].Passes[0].Apply();
            fullScreenQuad.Draw(graphicsDevice);
        }

        #endregion

        #region Debug Buffers

        /// <summary>
        /// Draw a debug version of GBuffer.
        /// </summary>
        /// <param name="fullScreenQuad">FullScreenQuad class.</param>
        public void DrawDebugBuffers(SpriteBatch spriteBatch, FullScreenQuad fullScreenQuad)
        {
            // Width + Height
            int width = (int)size.X / 8;
            int height = (int)size.Y / 8;

            // Begin sprite batch
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null);

            //Set up Drawing Rectangle
            Rectangle rect;
            rect.X = 0;
            rect.Y = 0;
            rect.Width = width;
            rect.Height = height;

            // Draw colour target
            spriteBatch.Draw(colorRT, rect, Color.White);

            // Draw normal target
            rect.X += width;
            spriteBatch.Draw(normalRT, rect, Color.White);

            // Draw depth
            if (depthDebugBuffer != null)
            {
                rect.X += width;
                spriteBatch.Draw(depthDebugBuffer, rect, Color.White);
            }

            // Draw light accumulation target
            rect.X += width;
            spriteBatch.Draw(lightRT, rect, Color.White);

            // End sprite batch
            spriteBatch.End();
        }

        /// <summary>
        /// Renders depth information to grayscale so it is more useful for debug.
        /// </summary>
        /// <param name="fullScreenQuad">FullScreenQuad class.</param>
        public void UpdateDepthDebugBuffer(FullScreenQuad fullScreenQuad)
        {
            // Clear render target
            ResolveGBuffer();

            // Create render target if it doesn't exist or is lost
            if (depthDebugBuffer == null)
                CreateDepthDebugBuffer();
            else if (depthDebugBuffer.IsContentLost)
                CreateDepthDebugBuffer();

            // Set render target
            graphicsDevice.SetRenderTarget(depthDebugBuffer);

            // Set render states
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.None;

            // Set values
            drawDebugDepthBufferEffect.Parameters["DepthMap"].SetValue(depthRT);
            drawDebugDepthBufferEffect.Parameters["HalfPixel"].SetValue(halfPixel);

            // Apply changes and draw
            drawDebugDepthBufferEffect.Techniques[0].Passes[0].Apply();
            fullScreenQuad.Draw(graphicsDevice);
        }

        /// <summary>
        /// Creates depth debug buffer.
        /// </summary>
        private void CreateDepthDebugBuffer()
        {
            depthDebugBuffer = new RenderTarget2D(graphicsDevice, (int)size.X, (int)size.Y, false, SurfaceFormat.Color, DepthFormat.None);
        }

        #endregion

    }

}
