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
using DeepEngine.Utility;
using DeepEngine.Daggerfall;
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
        Color clearColor = Color.Transparent;
        GraphicsDevice graphicsDevice;
        FullScreenQuad fullScreenQuad;
        GBuffer gBuffer;

        // Render effects
        Effect clearBufferEffect;
        Effect finalCombineEffect;
        Effect directionalLightEffect;
        Effect pointLightEffect;
        Effect emissiveLightEffect;
        Effect renderBillboards;
        Effect fxaaAntialiasing;

        // Render targets
        RenderTarget2D renderTarget;            // Final render target for any renderer output.
        RenderTarget2D fxaaRenderTarget;        // Render target to use as source for FXAA post-process.
        RenderTarget2D bloomRenderTarget;       // Render target ro use as source for Bloom post-process.

        // Effect parameters
        EffectParameter renderBillboards_Texture;
        EffectParameter renderBillboards_Position;
        EffectParameter renderBillboards_Size;
        EffectParameter pointLight_colorMap;
        EffectParameter pointLight_normalMap;
        EffectParameter pointLight_depthMap;
        EffectParameter pointLight_worldMatrix;
        EffectParameter pointLight_viewMatrix;
        EffectParameter pointLight_projectionMatrix;
        EffectParameter pointLight_lightPosition;
        EffectParameter pointLight_lightColor;
        EffectParameter pointLight_lightRadius;
        EffectParameter pointLight_lightIntensity;
        EffectParameter pointLight_cameraPosition;
        EffectParameter pointLight_invertViewProjection;
        EffectParameter pointLight_halfPixel;
        EffectParameter directionalLight_colorMap;
        EffectParameter directionalLight_normalMap;
        EffectParameter directionalLight_depthMap;
        EffectParameter directionalLight_lightDirection;
        EffectParameter directionalLight_lightColor;
        EffectParameter directionalLight_lightIntensity;
        EffectParameter directionalLight_cameraPosition;
        EffectParameter directionalLight_invertViewProjection;
        EffectParameter directionalLight_gBufferTextureSize;
        EffectParameter emissiveLight_ColorMap;
        EffectParameter emissiveLight_gBufferTextureSize;

        // Light geometry
        Model pointLightGeometry;
        Model spotLightGeometry;

        // Light textures
        Texture2D spotLightCookie;

        // Billboard geometry template for Daggerfall flats
        VertexBuffer daggerfallBillboardVertexBuffer;
        IndexBuffer daggerfallBillboardIndexBuffer;

        // Visible lights
        const int maxDirectionalLights = 10;
        const int maxPointLights = 512;
        const int maxSpotLights = 50;
        int directionalLightsCount;
        int pointLightsCount;
        int spotLightsCount;
        LightData[] directionalLights;
        LightData[] pointLights;
        LightData[] spotLights;

        // Visible billboards
        const int maxVisibleBillboards = 2048;
        int visibleBillboardsCount;
        BillboardData[] visibleBillboards;

        // Debug buffers
        bool showDebugBuffers = false;

        // Post processing
        BloomProcessor bloomProcessor;
        bool fxaaEnabled = true;
        bool bloomEnabled = true;

        // Screen rectangles
        Rectangle renderTargetRectangle;
        Rectangle graphicsDeviceRectangle;

        // Ambient light total
        Vector4 sumAmbientLightColor;

        // Sky dome
        Effect skyDomeEffect;
        Model skyDomeModel;
        CloudFactory cloudFactory;
        StarFactory starFactory;

        // Pointer
        private Ray pointerRay = new Ray();

        #endregion

        #region Structures

        /// <summary>
        /// Information about a light being submitted for rendering.
        /// </summary>
        private struct LightData
        {
            /// <summary>The light component to draw.</summary>
            public LightComponent LightComponent;

            /// <summary>The entity that initiated this submission.</summary>
            public BaseEntity Entity;
        }

        /// <summary>
        /// Information about a billboard being submitted for rendering.
        /// </summary>
        private struct BillboardData
        {
            /// <summary>Material to use when drawing billboard.</summary>
            public BaseMaterialEffect Material;

            /// <summary>Position of billboard in world space.</summary>
            public Vector3 Position;

            /// <summary>Dimensions of billboard.</summary>
            public Vector2 Size;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the clear colour to use during frame setup and post processing.
        ///  Use transparent colours to layer over your scene.
        /// </summary>
        public Color ClearColor
        {
            get { return clearColor; }
            set { clearColor = value; }
        }

        /// <summary>
        /// Gets the number of light submitted to renderer during last lighting operation.
        /// </summary>
        public int VisibleLightsCount
        {
            get { return directionalLightsCount + pointLightsCount + spotLightsCount; }
        }

        /// <summary>
        /// Gets the number of billboards submitted to renderer during last draw operation.
        /// </summary>
        public int VisibleBillboardsCount
        {
            get { return visibleBillboardsCount; }
        }

        /// <summary>
        /// Gets current GBuffer.
        /// </summary>
        public GBuffer GBuffer
        {
            get { return gBuffer; }
        }

        /// <summary>
        /// Gets rectangle of internal render target.
        /// </summary>
        public Rectangle RenderTargetRectangle
        {
            get { return renderTargetRectangle; }
        }

        /// <summary>
        /// Gets rectangle of graphics device render target.
        /// </summary>
        public Rectangle GraphicsDeviceRectangle
        {
            get { return graphicsDeviceRectangle; }
        }

        /// <summary>
        /// Gets or sets flag to show debug buffers after each render.
        /// </summary>
        public bool ShowDebugBuffers
        {
            get { return showDebugBuffers; }
            set { showDebugBuffers = value; }
        }

        /// <summary>
        /// Gets full screen quad renderer.
        /// </summary>
        public FullScreenQuad FullScreenQuad
        {
            get { return fullScreenQuad; }
        }

        /// <summary>
        /// Gets contents of last render for screenshots, etc.
        /// </summary>
        public Texture2D RenderTargetTexture
        {
            get { return (renderTarget as Texture2D); }
        }

        /// <summary>
        /// Gets or sets flag to enable/disable FXAA.
        /// </summary>
        public bool FXAAEnabled
        {
            get { return fxaaEnabled; }
            set { fxaaEnabled = value; }
        }

        /// <summary>
        /// Gets or sets flag to enable/disable Bloom.
        /// </summary>
        public bool BloomEnabled
        {
            get { return bloomEnabled; }
            set { bloomEnabled = value; }
        }

        /// <summary>
        /// Gets current pointer ray.
        ///  Must have already called UpdatePointerRay() with current mouse position in viewport.
        /// </summary>
        public Ray PointerRay
        {
            get { return pointerRay; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        public Renderer(DeepCore core)
        {
            // Store values
            this.core = core;

            // Create arrays
            directionalLights = new LightData[maxDirectionalLights];
            pointLights = new LightData[maxPointLights];
            spotLights = new LightData[maxSpotLights];
            visibleBillboards = new BillboardData[maxVisibleBillboards];
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

            // Reset render targets
            CreateRenderTargets();
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
            finalCombineEffect = core.ContentManager.Load<Effect>("Effects/CombineFinal");
            directionalLightEffect = core.ContentManager.Load<Effect>("Effects/DirectionalLight");
            pointLightEffect = core.ContentManager.Load<Effect>("Effects/PointLight");
            emissiveLightEffect = core.ContentManager.Load<Effect>("Effects/EmissiveLight");
            renderBillboards = core.ContentManager.Load<Effect>("Effects/RenderBillboards");
            fxaaAntialiasing = core.ContentManager.Load<Effect>("FXAA/fxaa");

            // Get render billboards parameters
            renderBillboards_Texture = renderBillboards.Parameters["Texture"];
            renderBillboards_Position = renderBillboards.Parameters["Position"];
            renderBillboards_Size = renderBillboards.Parameters["Size"];

            // Get point light parameters
            pointLight_colorMap = pointLightEffect.Parameters["ColorMap"];
            pointLight_normalMap = pointLightEffect.Parameters["NormalMap"];
            pointLight_depthMap = pointLightEffect.Parameters["DepthMap"];
            pointLight_worldMatrix = pointLightEffect.Parameters["World"];
            pointLight_viewMatrix = pointLightEffect.Parameters["View"];
            pointLight_projectionMatrix = pointLightEffect.Parameters["Projection"];
            pointLight_lightPosition = pointLightEffect.Parameters["LightPosition"];
            pointLight_lightColor = pointLightEffect.Parameters["Color"];
            pointLight_lightRadius = pointLightEffect.Parameters["LightRadius"];
            pointLight_lightIntensity = pointLightEffect.Parameters["LightIntensity"];
            pointLight_cameraPosition = pointLightEffect.Parameters["CameraPosition"];
            pointLight_invertViewProjection = pointLightEffect.Parameters["InvertViewProjection"];
            pointLight_halfPixel = pointLightEffect.Parameters["HalfPixel"];

            // Get directional light paramters
            directionalLight_colorMap = directionalLightEffect.Parameters["ColorMap"];
            directionalLight_normalMap = directionalLightEffect.Parameters["NormalMap"];
            directionalLight_depthMap = directionalLightEffect.Parameters["DepthMap"];
            directionalLight_lightDirection = directionalLightEffect.Parameters["LightDirection"];
            directionalLight_lightColor = directionalLightEffect.Parameters["Color"];
            directionalLight_lightIntensity = directionalLightEffect.Parameters["LightIntensity"];
            directionalLight_cameraPosition = directionalLightEffect.Parameters["CameraPosition"];
            directionalLight_invertViewProjection = directionalLightEffect.Parameters["InvertViewProjection"];
            directionalLight_gBufferTextureSize = directionalLightEffect.Parameters["GBufferTextureSize"];

            // Get emissive light parameters
            emissiveLight_ColorMap = emissiveLightEffect.Parameters["colorMap"];
            emissiveLight_gBufferTextureSize = emissiveLightEffect.Parameters["GBufferTextureSize"];

            // Load light geometry
            pointLightGeometry = core.ContentManager.Load<Model>("Models/PointLightGeometry");
            spotLightGeometry = core.ContentManager.Load<Model>("Models/SpotLightGeometry");

            // Load light textures
            spotLightCookie = core.ContentManager.Load<Texture2D>("Textures/SpotLightCookie");

            // Create billboard template
            CreateDaggerfallBillboardTemplate();

            // Create rendering classes
            fullScreenQuad = new FullScreenQuad(graphicsDevice);
            gBuffer = new GBuffer(core);
            bloomProcessor = new BloomProcessor(core);

            // Load content
            skyDomeEffect = core.ContentManager.Load<Effect>("Effects/SkyDomeEffect");
            skyDomeModel = core.ContentManager.Load<Model>("Models/SkyDomeModel");
            skyDomeModel.Meshes[0].MeshParts[0].Effect = skyDomeEffect.Clone();

            // Create factories
            cloudFactory = new CloudFactory(core);
            starFactory = new StarFactory(core);

            // Wire up GraphicsDevice events
            graphicsDevice.DeviceReset += new EventHandler<EventArgs>(GraphicsDevice_DeviceReset);
            graphicsDevice.DeviceLost += new EventHandler<EventArgs>(GraphicsDevice_DeviceLost);
        }

        /// <summary>
        /// Update renderer before drawing.
        /// </summary>
        public void Update()
        {
            // Reset visible lights and billboards count
            directionalLightsCount = 0;
            pointLightsCount = 0;
            spotLightsCount = 0;
            visibleBillboardsCount = 0;
        }

        /// <summary>
        /// Update pointer ray for picking.
        ///  Uses view and projection matrices from current camera.
        /// </summary>
        /// <param name="x">Pointer X in viewport.</param>
        /// <param name="y">Pointer Y in viewport.</param>
        public void UpdatePointerRay(int x, int y)
        {
            // Get matrices
            Matrix view = core.ActiveScene.Camera.ViewMatrix;
            Matrix projection = core.ActiveScene.Camera.ProjectionMatrix;

            // Unproject vectors into view area
            Viewport vp = graphicsDevice.Viewport;
            Vector3 near = vp.Unproject(new Vector3(x, y, 0), projection, view, Matrix.Identity);
            Vector3 far = vp.Unproject(new Vector3(x, y, 1), projection, view, Matrix.Identity);

            // Create ray
            Vector3 direction = far - near;
            direction.Normalize();
            pointerRay.Position = near;
            pointerRay.Direction = direction;
        }

        /// <summary>
        /// Draw visible content and performs post-processing into a final render target.
        ///  Must call Present() to copy render target into frame buffer.
        /// </summary>
        /// <param name="scene">Scene to render.</param>
        public void Draw(Scene scene)
        {
            // Deferred render scene to render target buffer
            BeginDraw();
            DrawScene(scene);
            EndDraw();

            // Clear frame buffer based on environment settings
            if (core.ActiveScene.Environment.SkyVisible)
            {
                // Draw a sky dome
                DrawSkyDome();
            }
            else
            {
                // Just clear to solid colour
                graphicsDevice.Clear(core.ActiveScene.Environment.ClearColor);
            }
        }

        /// <summary>
        /// Presents render target by copying to frame buffer.
        ///  Will be alpha blended over anything already in frame buffer, allowing caller to draw
        ///  what they need before presenting.
        /// </summary>
        public void Present()
        {
            // Copy renderTarget to frame buffer
            core.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            core.SpriteBatch.Draw(renderTarget, graphicsDeviceRectangle, renderTargetRectangle, Color.White);
            core.SpriteBatch.End();

            // Draw debug buffers
            if (showDebugBuffers)
                gBuffer.DrawDebugBuffers(core.SpriteBatch, fullScreenQuad);
        }

        /// <summary>
        /// Submit a light to be drawn in GBuffer.
        ///  In deferred rendering, most lights are drawn after other buffers have been filled.
        ///  One exception being ambient light which is simply accumulated until we're ready to draw.
        /// </summary>
        /// <param name="lightComponent">Light to render.</param>
        /// <param name="caller">The entity submitting the light.</param>
        public void SubmitLight(LightComponent lightComponent, BaseEntity caller)
        {
            // Submit light by type
            if (lightComponent.Type == LightComponent.LightType.Ambient)
            {
                // Ambient light is just accumulated
                sumAmbientLightColor += lightComponent.Color.ToVector4() * lightComponent.Intensity;
                sumAmbientLightColor.W = MathHelper.Clamp(sumAmbientLightColor.W, 0, 1);
            }
            else if (lightComponent.Type == LightComponent.LightType.Directional)
            {
                // Add light to array
                if (directionalLightsCount < maxDirectionalLights)
                {
                    directionalLights[directionalLightsCount].LightComponent = lightComponent;
                    directionalLights[directionalLightsCount].Entity = caller;
                    directionalLightsCount++;
                }
            }
            else if (lightComponent.Type == LightComponent.LightType.Point)
            {
                // Add light to array
                if (pointLightsCount < maxPointLights)
                {
                    pointLights[pointLightsCount].LightComponent = lightComponent;
                    pointLights[pointLightsCount].Entity = caller;
                    pointLightsCount++;
                }
            }
            else if (lightComponent.Type == LightComponent.LightType.Spot)
            {
                // Add light to array
                if (spotLightsCount < maxSpotLights)
                {
                    spotLights[spotLightsCount].LightComponent = lightComponent;
                    spotLights[spotLightsCount].Entity = caller;
                    spotLightsCount++;
                }
            }
        }

        /// <summary>
        /// Submit a billboard to be drawn in GBuffer.
        /// </summary>
        /// <param name="material">Material used when rendering the billboard.</param>
        /// <param name="position">Position of billboard in world space.</param>
        /// <param name="size">Dimensions of billboard.</param>
        public void SubmitBillboard(BaseMaterialEffect material, Vector3 position, Vector2 size)
        {
            if (visibleBillboardsCount < maxVisibleBillboards)
            {
                // Add billboard to array
                visibleBillboards[visibleBillboardsCount].Material = material;
                visibleBillboards[visibleBillboardsCount].Position = position;
                visibleBillboards[visibleBillboardsCount].Size = size;
                visibleBillboardsCount++;
            }
        }

        #endregion

        #region Drawing

        /// <summary>
        /// Prepares and clears GBuffer.
        /// </summary>
        private void BeginDraw()
        {
            // Reset ambient light colour
            sumAmbientLightColor = Vector4.Zero;

            // Ensure render targets match viewport size
            if (gBuffer.Size.X != (float)graphicsDevice.Viewport.Width ||
                gBuffer.Size.Y != (float)graphicsDevice.Viewport.Height)
            {
                // Create render targets
                CreateRenderTargets();
            }
            
            // Prepare GBuffer
            gBuffer.SetGBuffer();
            gBuffer.ClearGBuffer(clearBufferEffect, fullScreenQuad, Color.Transparent);
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

            // Draw scene
            scene.Draw();

            // Draw billboards
            DrawBillboards();
        }

        /// <summary>
        /// Draws post-geometry objects and composits GBuffer.
        /// </summary>
        private void EndDraw()
        {
            // Draw lights
            DrawLights();

            // Update depth debug buffer
            if (showDebugBuffers)
                gBuffer.UpdateDepthDebugBuffer(fullScreenQuad);

            // Set ambient light
            gBuffer.AmbientLightColor = sumAmbientLightColor;

            // Finish deferred rendering
            ComposeFinal();

            // Resolve GBuffer
            gBuffer.ResolveGBuffer();
        }

        /// <summary>
        /// Combines all render targets into back buffer for presentation.
        ///  Runs post-processing until a proper framework is written.
        /// </summary>
        private void ComposeFinal()
        {
            // No post-processing enabled, just compose into render target
            if (!fxaaEnabled && !bloomEnabled)
            {
                // Set render target
                graphicsDevice.SetRenderTarget(renderTarget);

                // Clear target
                graphicsDevice.Clear(clearColor);

                // Compose final image from GBuffer
                gBuffer.ComposeFinal(finalCombineEffect, fullScreenQuad);
            }

            // Only fxaa is enabled
            else if (fxaaEnabled && !bloomEnabled)
            {
                // Set render target
                graphicsDevice.SetRenderTarget(fxaaRenderTarget);

                // Clear target
                graphicsDevice.Clear(clearColor);

                // Compose final image from GBuffer
                gBuffer.ComposeFinal(finalCombineEffect, fullScreenQuad);

                // Set render target
                graphicsDevice.SetRenderTarget(renderTarget);

                // Clear target
                graphicsDevice.Clear(clearColor);

                // Set effect parameters
                fxaaAntialiasing.CurrentTechnique = fxaaAntialiasing.Techniques["ppfxaa"];
                fxaaAntialiasing.Parameters["SCREEN_WIDTH"].SetValue(fxaaRenderTarget.Width);
                fxaaAntialiasing.Parameters["SCREEN_HEIGHT"].SetValue(fxaaRenderTarget.Height);
                fxaaAntialiasing.Parameters["gScreenTexture"].SetValue(fxaaRenderTarget as Texture2D);

                // Set render states
                graphicsDevice.BlendState = BlendState.AlphaBlend;
                graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
                graphicsDevice.DepthStencilState = DepthStencilState.None;

                // Draw FXAA
                fxaaAntialiasing.Techniques[0].Passes[0].Apply();
                fullScreenQuad.Draw(graphicsDevice);
            }

            // Only bloom is enabled
            else if (bloomEnabled && !fxaaEnabled)
            {
                // Set render target
                graphicsDevice.SetRenderTarget(bloomRenderTarget);

                // Clear target
                graphicsDevice.Clear(clearColor);

                // Compose final image from GBuffer
                gBuffer.ComposeFinal(finalCombineEffect, fullScreenQuad);

                // Draw bloom
                bloomProcessor.Draw(bloomRenderTarget, renderTarget);
            }

            // Both fxaa and bloom are enabled
            else if (fxaaEnabled && bloomEnabled)
            {
                // Set render target
                graphicsDevice.SetRenderTarget(fxaaRenderTarget);

                // Clear target
                graphicsDevice.Clear(clearColor);

                // Compose final image from GBuffer
                gBuffer.ComposeFinal(finalCombineEffect, fullScreenQuad);

                // Next render target
                graphicsDevice.SetRenderTarget(bloomRenderTarget);

                // Clear target
                graphicsDevice.Clear(clearColor);

                // Set effect parameters
                fxaaAntialiasing.CurrentTechnique = fxaaAntialiasing.Techniques["ppfxaa"];
                fxaaAntialiasing.Parameters["SCREEN_WIDTH"].SetValue(fxaaRenderTarget.Width);
                fxaaAntialiasing.Parameters["SCREEN_HEIGHT"].SetValue(fxaaRenderTarget.Height);
                fxaaAntialiasing.Parameters["gScreenTexture"].SetValue(fxaaRenderTarget as Texture2D);

                // Set render states
                graphicsDevice.BlendState = BlendState.AlphaBlend;
                graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
                graphicsDevice.DepthStencilState = DepthStencilState.None;

                // Draw FXAA
                fxaaAntialiasing.Techniques[0].Passes[0].Apply();
                fullScreenQuad.Draw(graphicsDevice);

                // Draw bloom
                bloomProcessor.Draw(bloomRenderTarget, renderTarget);
            }
        }

        /// <summary>
        /// Draws a dynamically generated skydome based on scene environment settings.
        /// </summary>
        private void DrawSkyDome()
        {
            // Get environment from scene
            SceneEnvironment environment = core.ActiveScene.Environment;

            // Get current cloud texture
            Texture2D cloudMap = cloudFactory.GetClouds(environment.CloudTime, environment.CloudBrightness);

            // Clear device to match bottom colour of sky dome
            graphicsDevice.Clear(environment.SkyGradientBottom);

            // Set render states
            //core.GraphicsDevice.DepthStencilState = DepthStencilState.None;

            // Draw sky dome mesh
            Matrix worldMatrix = Matrix.CreateTranslation(0, -0.2f, 0) * Matrix.CreateScale(10) * Matrix.CreateTranslation(core.ActiveScene.Camera.Position);
            foreach (ModelMesh mesh in skyDomeModel.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    currentEffect.CurrentTechnique = currentEffect.Techniques["SkyDome"];
                    currentEffect.Parameters["World"].SetValue(worldMatrix);
                    currentEffect.Parameters["View"].SetValue(core.ActiveScene.Camera.ViewMatrix);
                    currentEffect.Parameters["Projection"].SetValue(core.ActiveScene.Camera.ProjectionMatrix);
                    currentEffect.Parameters["CloudTexture"].SetValue(cloudMap);
                    currentEffect.Parameters["TopColor"].SetValue(environment.SkyGradientTop.ToVector4());
                    currentEffect.Parameters["BottomColor"].SetValue(environment.SkyGradientBottom.ToVector4());

                    //if (environment.SkyDomeStarsVisible)
                    //    currentEffect.Parameters["StarTexture"].SetValue(starFactory.StarMap);
                }
                mesh.Draw();
            }

            // Reset render states
            //core.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        #endregion

        #region Lighting

        /// <summary>
        /// Draws lights into GBuffer.
        /// </summary>
        private void DrawLights()
        {
            // Set render target
            graphicsDevice.SetRenderTarget(gBuffer.LightRT);

            // Set render states
            graphicsDevice.Clear(Color.Transparent);
            graphicsDevice.BlendState = BlendState.AlphaBlend;
            graphicsDevice.DepthStencilState = DepthStencilState.None;

            // Draw directional lights
            for (int i = 0; i < directionalLightsCount; i++)
            {
                LightComponent light = directionalLights[i].LightComponent;
                DrawDirectionalLight(light.Direction, light.Color, light.Intensity);
            }

            // Draw point lights
            DrawPointLights();

            // TODO: Draw spot lights

            // Draw emissive light
            DrawEmissiveLight();
        }

        /// <summary>
        /// Draws a directional light.
        /// </summary>
        /// <param name="lightDirection">Light direction.</param>
        /// <param name="lightColor">Light color.</param>
        /// <param name="lightIntensity">Light intensity.</param>
        private void DrawDirectionalLight(Vector3 lightDirection, Color lightColor, float lightIntensity)
        {
            // Set GBuffer
            directionalLight_colorMap.SetValue(gBuffer.ColorRT);
            directionalLight_normalMap.SetValue(gBuffer.NormalRT);
            directionalLight_depthMap.SetValue(gBuffer.DepthRT);

            // Set light properties
            directionalLight_lightDirection.SetValue(lightDirection);
            directionalLight_lightColor.SetValue(lightColor.ToVector3());
            directionalLight_lightIntensity.SetValue(lightIntensity);

            // Set camera
            directionalLight_cameraPosition.SetValue(core.ActiveScene.Camera.Position);
            directionalLight_invertViewProjection.SetValue(Matrix.Invert(core.ActiveScene.Camera.ViewMatrix * core.ActiveScene.Camera.ProjectionMatrix));

            // Set size
            directionalLight_gBufferTextureSize.SetValue(gBuffer.Size);

            // Apply changes
            directionalLightEffect.CurrentTechnique.Passes[0].Apply();

            // Draw
            fullScreenQuad.Draw(graphicsDevice);
        }

        /// <summary>
        /// Draws point lights.
        /// </summary>
        private void DrawPointLights()
        {
            // Set GBuffer
            pointLight_colorMap.SetValue(gBuffer.ColorRT);
            pointLight_normalMap.SetValue(gBuffer.NormalRT);
            pointLight_depthMap.SetValue(gBuffer.DepthRT);

            // Set geometry
            ModelMeshPart meshPart = pointLightGeometry.Meshes[0].MeshParts[0];
            graphicsDevice.Indices = meshPart.IndexBuffer;
            graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);

            // Draw all point lights
            LightComponent light;
            BaseEntity entity;
            for (int i = 0; i < pointLightsCount; i++)
            {
                // Get light data
                light = pointLights[i].LightComponent;
                entity = pointLights[i].Entity;

                // Light position
                Vector3 position = Vector3.Transform(light.Position, entity.Matrix);

                // Compute the light world matrix.
                // Scale according to light radius, and translate it to light position.
                Matrix sphereWorldMatrix = Matrix.CreateScale(light.PointRadius) * Matrix.CreateTranslation(position);
                pointLight_worldMatrix.SetValue(sphereWorldMatrix);
                pointLight_viewMatrix.SetValue(core.ActiveScene.Camera.ViewMatrix);
                pointLight_projectionMatrix.SetValue(core.ActiveScene.Camera.ProjectionMatrix);

                // Light position
                pointLight_lightPosition.SetValue(position);

                // Set the color, radius and intensity
                pointLight_lightColor.SetValue(light.Color.ToVector3());
                pointLight_lightRadius.SetValue(light.PointRadius);
                pointLight_lightIntensity.SetValue(light.Intensity);

                // Parameters for specular computations
                pointLight_cameraPosition.SetValue(core.ActiveScene.Camera.Position);
                pointLight_invertViewProjection.SetValue(Matrix.Invert(core.ActiveScene.Camera.ViewMatrix * core.ActiveScene.Camera.ProjectionMatrix));

                // Size of a halfpixel, for texture coordinates alignment
                pointLight_halfPixel.SetValue(gBuffer.HalfPixel);

                // Calculate the distance between the camera and light center
                float cameraToCenter = Vector3.Distance(core.ActiveScene.Camera.Position, position);

                // If we are inside the light volume, draw the sphere's inside face
                if (cameraToCenter < light.PointRadius)
                    graphicsDevice.RasterizerState = RasterizerState.CullClockwise;
                else
                    graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

                // Draw light sphere
                pointLightEffect.CurrentTechnique.Passes[0].Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
            }
        }

        /// <summary>
        /// Draws light from emissive materials.
        /// </summary>
        private void DrawEmissiveLight()
        {
            // Set states
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            // Set parameters
            emissiveLight_ColorMap.SetValue(gBuffer.ColorRT);
            emissiveLight_gBufferTextureSize.SetValue(gBuffer.Size);

            // Apply changes
            emissiveLightEffect.Techniques[0].Passes[0].Apply();

            // Draw
            fullScreenQuad.Draw(graphicsDevice);
        }

        #endregion

        #region Billboards

        /// <summary>
        /// Draws billboards
        /// </summary>
        private void DrawBillboards()
        {
            // Set transforms
            renderBillboards.Parameters["World"].SetValue(Matrix.Identity);
            renderBillboards.Parameters["View"].SetValue(core.ActiveScene.Camera.ViewMatrix);
            renderBillboards.Parameters["Projection"].SetValue(core.ActiveScene.Camera.ProjectionMatrix);

            // Set buffers
            core.GraphicsDevice.SetVertexBuffer(daggerfallBillboardVertexBuffer);
            core.GraphicsDevice.Indices = daggerfallBillboardIndexBuffer;

            // Set render states
            core.GraphicsDevice.BlendState = BlendState.Opaque;
            core.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            core.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            core.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;

            // Draw billboards.
            renderBillboards.Parameters["AlphaTestDirection"].SetValue(1f);
            for (int i = 0; i < visibleBillboardsCount; i++)
            {
                DrawBillboardPass(i);
            }
        }

        /// <summary>
        /// Draw static batches with current settings.
        /// </summary>
        /// <param name="current">Current billboard index.</param>
        private void DrawBillboardPass(int current)
        {
            // Apply parameters
            renderBillboards_Texture.SetValue(visibleBillboards[current].Material.DiffuseTexture);
            renderBillboards_Position.SetValue(visibleBillboards[current].Position);
            renderBillboards_Size.SetValue(visibleBillboards[current].Size);

            // Render geometry
            foreach (EffectPass pass in renderBillboards.CurrentTechnique.Passes)
            {
                // Apply effect pass
                pass.Apply();

                // Draw primitives
                core.GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    0,
                    daggerfallBillboardVertexBuffer.VertexCount,
                    0,
                    2);
            }
        }

        /// <summary>
        /// Creates billboard template.
        /// </summary>
        private void CreateDaggerfallBillboardTemplate()
        {
            // Set dimensions of billboard 
            float w = 0.5f * ModelManager.GlobalScale;
            float h = 0.5f * ModelManager.GlobalScale;

            // Create vertex array
            VertexPositionNormalTextureBump[] billboardVertices = new VertexPositionNormalTextureBump[4];
            billboardVertices[0] = new VertexPositionNormalTextureBump(
                new Vector3(-w, h, 0),
                Vector3.Up,
                new Vector2(0, 0),
                Vector3.Zero,
                Vector3.Zero);
            billboardVertices[1] = new VertexPositionNormalTextureBump(
                new Vector3(w, h, 0),
                Vector3.Up,
                new Vector2(1, 0),
                Vector3.Zero,
                Vector3.Zero);
            billboardVertices[2] = new VertexPositionNormalTextureBump(
                new Vector3(-w, -h, 0),
                Vector3.Up,
                new Vector2(0, 1),
                Vector3.Zero,
                Vector3.Zero);
            billboardVertices[3] = new VertexPositionNormalTextureBump(
                new Vector3(w, -h, 0),
                Vector3.Up,
                new Vector2(1, 1),
                Vector3.Zero,
                Vector3.Zero);

            // Create index array
            short[] billboardIndices = new short[6]
            {
                0, 1, 2,
                1, 3, 2,
            };

            // Create buffers
            daggerfallBillboardVertexBuffer = new VertexBuffer(core.GraphicsDevice, VertexPositionNormalTextureBump.VertexDeclaration, 4, BufferUsage.WriteOnly);
            daggerfallBillboardIndexBuffer = new IndexBuffer(core.GraphicsDevice, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);

            // Set data
            daggerfallBillboardVertexBuffer.SetData(billboardVertices);
            daggerfallBillboardIndexBuffer.SetData(billboardIndices);
        }

        #endregion

        #region Scene Render Targets

        /// <summary>
        /// Creates new render target for post-processing effects.
        ///  Standard rendering just draws direct into the frame buffer.
        /// </summary>
        private void CreateRenderTargets()
        {
            // Create new targets on other classes
            gBuffer.CreateGBuffer();
            bloomProcessor.CreateTargets();

            // Dispose of previous targets
            if (renderTarget != null) renderTarget.Dispose();
            if (fxaaRenderTarget != null) fxaaRenderTarget.Dispose();
            if (bloomRenderTarget != null) bloomRenderTarget.Dispose();

            // Get viewport size
            int width = graphicsDevice.Viewport.Width;
            int height = graphicsDevice.Viewport.Height;

            // Set rectangles
            this.renderTargetRectangle = new Rectangle(0, 0, width, height);
            this.graphicsDeviceRectangle = new Rectangle(
                graphicsDevice.Viewport.X,
                graphicsDevice.Viewport.Y,
                graphicsDevice.Viewport.Width,
                graphicsDevice.Viewport.Height);

            // Create final render target.
            // Remember to add a depth-stencil buffer if any forward rendering is done in the future.
            renderTarget = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);

            // Create FXAA post-processing target
            if (fxaaEnabled)
                fxaaRenderTarget = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);
            else
                fxaaRenderTarget = null;

            // Create Bloom post-processing target
            if (bloomEnabled)
                bloomRenderTarget = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);
            else
                bloomRenderTarget = null;
        }

        #endregion

    }

}
