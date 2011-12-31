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
using Microsoft.Xna.Framework.Content;
using DeepEngine.Core;
using DeepEngine.World;
using DeepEngine.Primitives;
using DeepEngine.Components;
#endregion

namespace DeepEngine.Rendering
{

    /// <summary>
    /// Deferred renderer.
    /// </summary>
    public class Renderer
    {

        #region Fields

        // Engine
        DeepCore core;

        // Rendering
        Color clearColor = Color.CornflowerBlue;
        GraphicsDevice graphicsDevice;
        FullScreenQuad fullScreenQuad;
        GBuffer gBuffer;

        // Render effects
        Effect clearBufferEffect;
        Effect renderBufferEffect;
        Effect finalCombineEffect;
        Effect directionalLightEffect;
        Effect pointLightEffect;
        //Effect emissiveLightEffect;

        // Geometry
        private Model pointLightSphereModel;

        // Visible lights
        const int maxVisibleLights = 512;
        int visibleLightsCount;
        LightComponent[] visibleLights;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets clear colour.
        /// </summary>
        public Color ClearColor
        {
            get { return clearColor; }
            set { clearColor = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public Renderer(DeepCore core)
        {
            // Store values
            this.core = core;

            // Create visible light array
            visibleLights = new LightComponent[maxVisibleLights];
        }

        #endregion

        #region GraphicsDevice Events

        /// <summary>
        /// Called when device is reset and we need to recreate resources.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">EventArgs.</param>
        private void GraphicsDevice_DeviceReset(object sender, EventArgs e)
        {
            // Work around .fx bug in XNA 4.0.
            // XNA will error if any sampler state has a SurfaceFormat.Single attached,
            // even if that sampler state is not in use.
            // In this case, it is SamplerState[2] (depth buffer in deferred renderer).
            // Source1: http://forums.create.msdn.com/forums/p/61268/438840.aspx
            // Source2: http://www.gamedev.net/topic/603699-xna-framework-hidef-profile-requires-texturefilter-to-be-point-when-using-texture-format-single/
            graphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;
            graphicsDevice.SamplerStates[2] = SamplerState.PointClamp;

            // Reset GBuffer
            gBuffer.CreateGBuffer();
        }

        /// <summary>
        /// Called when device is lost.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">EventArgs.</param>
        private void GraphicsDevice_DeviceLost(object sender, EventArgs e)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Load content.
        /// </summary>
        public void LoadContent()
        {
            // Store graphics device
            this.graphicsDevice = core.GraphicsDevice;

            // Load rendering effects
            clearBufferEffect = core.ContentManager.Load<Effect>("Effects/ClearGBuffer");
            renderBufferEffect = core.ContentManager.Load<Effect>("Effects/RenderGBuffer");
            finalCombineEffect = core.ContentManager.Load<Effect>("Effects/CombineFinal");
            directionalLightEffect = core.ContentManager.Load<Effect>("Effects/DirectionalLight");
            pointLightEffect = core.ContentManager.Load<Effect>("Effects/PointLight");

            // Load models
            pointLightSphereModel = core.ContentManager.Load<Model>("Models/PointLightSphere");

            // Create rendering classes
            fullScreenQuad = new FullScreenQuad(graphicsDevice);
            gBuffer = new GBuffer(graphicsDevice);

            // Wire up GraphicsDevice events
            graphicsDevice.DeviceReset += new EventHandler<EventArgs>(GraphicsDevice_DeviceReset);
            graphicsDevice.DeviceLost += new EventHandler<EventArgs>(GraphicsDevice_DeviceLost);
        }

        /// <summary>
        /// Update renderer before drawing.
        /// </summary>
        public void Update()
        {
            // Reset visible lights count
            visibleLightsCount = 0;
        }

        /// <summary>
        /// Draw visible content.
        /// </summary>
        /// <param name="scene">Scene to render.</param>
        public void Draw(Scene scene)
        {
            BeginDraw();
            DrawScene(scene);
            EndDraw();
        }

        /// <summary>
        /// Submit a light to be drawn in GBuffer.
        ///  In deferred rendering lights are drawn after other buffers have been filled.
        ///  Currently just storing pending light operations until end.
        /// </summary>
        /// <param name="light">Light to render.</param>
        public void SubmitLight(LightComponent light)
        {
            if (visibleLightsCount < maxVisibleLights)
            {
                visibleLights[visibleLightsCount++] = light;
            }
        }

        #endregion

        #region Drawing

        /// <summary>
        /// Prepares and clears GBuffer.
        /// </summary>
        private void BeginDraw()
        {
            // Ensure GBuffer matches viewport size
            if (gBuffer.Size.X != (float)graphicsDevice.Viewport.Width ||
                gBuffer.Size.Y != (float)graphicsDevice.Viewport.Height)
            {
                gBuffer.CreateGBuffer();
            }

            // Prepare GBuffer
            gBuffer.SetGBuffer();
            gBuffer.ClearGBuffer(clearBufferEffect, fullScreenQuad, clearColor);
        }

        /// <summary>
        /// Render active scene into GBuffer.
        /// </summary>
        /// <param name="scene">Scene to render.</param>
        private void DrawScene(Scene scene)
        {
            // Set render states
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            graphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;

            scene.Draw();
        }

        /// <summary>
        /// Ends the drawing process and composits GBuffer.
        /// </summary>
        private void EndDraw()
        {
            // Finish deferred rendering
            DrawLights();
            ComposeFinal();

            // Draw debug buffers
            gBuffer.DrawDebugBuffers(core.SpriteBatch);
        }

        /// <summary>
        /// Draws lights into GBuffer.
        /// </summary>
        private void DrawLights()
        {
            gBuffer.ResolveGBuffer();

            // Set render states
            graphicsDevice.SetRenderTarget(gBuffer.LightRT);
            graphicsDevice.Clear(Color.Transparent);
            graphicsDevice.BlendState = BlendState.AlphaBlend;
            graphicsDevice.DepthStencilState = DepthStencilState.None;

            // Draw visible lights
            LightComponent light;
            for (int i = 0; i < maxVisibleLights; i++)
            {
                if (i < visibleLightsCount)
                {
                    // Draw light
                    light = visibleLights[i];
                    switch (light.Type)
                    {
                        case LightComponent.LightType.Directional:
                            DrawDirectionalLight(light.Direction, light.Color, light.Intensity);
                            break;
                        case LightComponent.LightType.Point:
                            DrawPointLight(Vector3.Transform(light.Position, light.Entity.Matrix), light.Radius, light.Color, light.Intensity);
                            break;
                    }
                }
                else
                {
                    // Clear buffer position to release any references
                    visibleLights[i] = null;
                }
            }
        }

        /// <summary>
        /// Combines all render targets into back buffer for presentation.
        /// </summary>
        private void ComposeFinal()
        {
            gBuffer.ResolveGBuffer();

            // Clear frame buffer
            graphicsDevice.Clear(clearColor);

            // Compose final image
            gBuffer.ComposeFinal(finalCombineEffect, fullScreenQuad);
        }

        #endregion

        #region Lighting

        /// <summary>
        /// Draws a directional light.
        /// </summary>
        /// <param name="lightDirection">Light direction.</param>
        /// <param name="lightColor">Light color.</param>
        /// <param name="lightIntensity">Light intensity.</param>
        private void DrawDirectionalLight(Vector3 lightDirection, Color lightColor, float lightIntensity)
        {
            // Set GBuffer
            directionalLightEffect.Parameters["ColorMap"].SetValue(gBuffer.ColorRT);
            directionalLightEffect.Parameters["NormalMap"].SetValue(gBuffer.NormalRT);
            directionalLightEffect.Parameters["DepthMap"].SetValue(gBuffer.DepthRT);

            // Set light properties
            directionalLightEffect.Parameters["LightDirection"].SetValue(lightDirection);
            directionalLightEffect.Parameters["LightIntensity"].SetValue(lightIntensity);
            directionalLightEffect.Parameters["Color"].SetValue(lightColor.ToVector3());

            // Set camera
            directionalLightEffect.Parameters["CameraPosition"].SetValue(core.ActiveScene.DeprecatedCamera.Position);
            directionalLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(core.ActiveScene.DeprecatedCamera.View * core.ActiveScene.DeprecatedCamera.Projection));

            // Set size
            directionalLightEffect.Parameters["GBufferTextureSize"].SetValue(gBuffer.Size);

            // Apply changes
            directionalLightEffect.Techniques[0].Passes[0].Apply();

            // Draw
            fullScreenQuad.Draw(graphicsDevice);
        }

        /// <summary>
        /// Draws a point light.
        /// </summary>
        /// <param name="lightPosition">Light position.</param>
        /// /// <param name="lightRadius">Light radius.</param>
        /// <param name="color">Light colour.</param>
        /// <param name="lightIntensity">Light intensity.</param>
        private void DrawPointLight(Vector3 lightPosition, float lightRadius, Color color, float lightIntensity)
        {
            // Set GBuffer
            pointLightEffect.Parameters["ColorMap"].SetValue(gBuffer.ColorRT);
            pointLightEffect.Parameters["NormalMap"].SetValue(gBuffer.NormalRT);
            pointLightEffect.Parameters["DepthMap"].SetValue(gBuffer.DepthRT);

            // Compute the light world matrix.
            // Scale according to light radius, and translate it to light position.
            Matrix sphereWorldMatrix = Matrix.CreateScale(lightRadius) * Matrix.CreateTranslation(lightPosition);
            pointLightEffect.Parameters["World"].SetValue(sphereWorldMatrix);
            pointLightEffect.Parameters["View"].SetValue(core.ActiveScene.DeprecatedCamera.View);
            pointLightEffect.Parameters["Projection"].SetValue(core.ActiveScene.DeprecatedCamera.Projection);

            // Light position
            pointLightEffect.Parameters["LightPosition"].SetValue(lightPosition);

            // Set the color, radius and intensity
            pointLightEffect.Parameters["Color"].SetValue(color.ToVector3());
            pointLightEffect.Parameters["LightRadius"].SetValue(lightRadius);
            pointLightEffect.Parameters["LightIntensity"].SetValue(lightIntensity);

            // Parameters for specular computations
            pointLightEffect.Parameters["CameraPosition"].SetValue(core.ActiveScene.DeprecatedCamera.Position);
            pointLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(core.ActiveScene.DeprecatedCamera.View * core.ActiveScene.DeprecatedCamera.Projection));

            // Size of a halfpixel, for texture coordinates alignment
            pointLightEffect.Parameters["HalfPixel"].SetValue(gBuffer.HalfPixel);

            // Calculate the distance between the camera and light center
            float cameraToCenter = Vector3.Distance(core.ActiveScene.DeprecatedCamera.Position, lightPosition);

            // If we are inside the light volume, draw the sphere's inside face
            if (cameraToCenter < lightRadius)
                graphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            else
                graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            graphicsDevice.DepthStencilState = DepthStencilState.None;

            pointLightEffect.Techniques[0].Passes[0].Apply();
            foreach (ModelMesh mesh in pointLightSphereModel.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    graphicsDevice.Indices = meshPart.IndexBuffer;
                    graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);

                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                }
            }

            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        #endregion

    }

}
