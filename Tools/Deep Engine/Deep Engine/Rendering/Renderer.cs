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
        Effect finalCombineEffect;
        Effect directionalLightEffect;
        Effect pointLightEffect;
        Effect emissiveLightEffect;
        Effect renderBillboards;

        // Render parameters
        EffectParameter renderBillboards_Texture;
        EffectParameter renderBillboards_Position;
        EffectParameter renderBillboards_Size;

        // Geometry
        private Model pointLightSphereModel;

        // Billboard geometry template
        private VertexBuffer billboardVertexBuffer;
        private IndexBuffer billboardIndexBuffer;

        // Visible lights
        const int maxVisibleLights = 512;
        int visibleLightsCount;
        LightData[] visibleLights;

        // Visible billboards
        const int maxVisibleBillboards = 2048;
        int visibleBillboardsCount;
        BillboardData[] visibleBillboards;

        // Debug buffers
        bool showDebugBuffers = false;

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
            /// <summary>Texture to use when drawing billboard.</summary>
            public Texture2D Texture;

            /// <summary>Position of billboard in world space.</summary>
            public Vector3 Position;

            /// <summary>Dimensions of billboard.</summary>
            public Vector2 Size;
        }

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

        /// <summary>
        /// Gets the number of light submitted to renderer during last lighting operation.
        /// </summary>
        public int VisibleLightsCount
        {
            get { return visibleLightsCount; }
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
        /// Gets or sets flag to show debug buffers after each render.
        /// </summary>
        public bool ShowDebugBuffers
        {
            get { return showDebugBuffers; }
            set { showDebugBuffers = value; }
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

            // Create arraya
            visibleLights = new LightData[maxVisibleLights];
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
            finalCombineEffect = core.ContentManager.Load<Effect>("Effects/CombineFinal");
            directionalLightEffect = core.ContentManager.Load<Effect>("Effects/DirectionalLight");
            pointLightEffect = core.ContentManager.Load<Effect>("Effects/PointLight");
            emissiveLightEffect = core.ContentManager.Load<Effect>("Effects/EmissiveLight");
            renderBillboards = core.ContentManager.Load<Effect>("Effects/RenderBillboards");

            // Get parameters
            renderBillboards_Texture = renderBillboards.Parameters["Texture"];
            renderBillboards_Position = renderBillboards.Parameters["Position"];
            renderBillboards_Size = renderBillboards.Parameters["Size"];

            // Load models
            pointLightSphereModel = core.ContentManager.Load<Model>("Models/PointLightSphere");

            // Create billboard template
            CreateBillboardTemplate();

            // Create rendering classes
            fullScreenQuad = new FullScreenQuad(graphicsDevice);
            gBuffer = new GBuffer(core);

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
            visibleLightsCount = 0;
            visibleBillboardsCount = 0;
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
        /// <param name="lightComponent">Light to render.</param>
        /// <param name="caller">The entity submitting the light.</param>
        public void SubmitLight(LightComponent lightComponent, BaseEntity caller)
        {
            if (visibleLightsCount < maxVisibleLights)
            {
                visibleLights[visibleLightsCount].LightComponent = lightComponent;
                visibleLights[visibleLightsCount].Entity = caller;
                visibleLightsCount++;
            }
        }

        /// <summary>
        /// Submit a billboard to be drawn in GBuffer.
        /// </summary>
        /// <param name="material">Texture used when rendering the billboard.</param>
        /// <param name="position">Position of billboard in world space.</param>
        /// <param name="size">Dimensions of billboard.</param>
        public void SubmitBillboard(Texture2D texture, Vector3 position, Vector2 size)
        {
            if (visibleBillboardsCount < maxVisibleBillboards)
            {
                visibleBillboards[visibleBillboardsCount].Texture = texture;
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

            // Finish deferred rendering
            ComposeFinal();
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

            // Draw debug buffers
            if (showDebugBuffers)
                gBuffer.DrawDebugBuffers(core.SpriteBatch, fullScreenQuad);
        }

        #endregion

        #region Lighting

        /// <summary>
        /// Draws lights into GBuffer.
        /// </summary>
        private void DrawLights()
        {
            gBuffer.ResolveGBuffer();

            // Set render target
            graphicsDevice.SetRenderTarget(gBuffer.LightRT);

            // Set render states
            graphicsDevice.Clear(Color.Transparent);
            graphicsDevice.BlendState = BlendState.AlphaBlend;
            graphicsDevice.DepthStencilState = DepthStencilState.None;

            // Draw visible lights for geometry
            pointLightEffect.CurrentTechnique = pointLightEffect.Techniques["Default"];
            for (int i = 0; i < maxVisibleLights; i++)
            {
                if (i < visibleLightsCount)
                {
                    // Draw light
                    LightComponent light = visibleLights[i].LightComponent;
                    BaseEntity entity = visibleLights[i].Entity;
                    switch (light.Type)
                    {
                        case LightComponent.LightType.Directional:
                            DrawDirectionalLight(light.Direction, light.Color, light.Intensity);
                            break;
                        case LightComponent.LightType.Point:
                            DrawPointLight(Vector3.Transform(light.Position, entity.Matrix), light.Radius, light.Color, light.Intensity);
                            break;
                    }
                }
                else
                {
                    // Clear remaining buffer positions to release any references
                    visibleLights[i].LightComponent = null;
                    visibleLights[i].Entity = null;
                }
            }

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
            directionalLightEffect.CurrentTechnique.Passes[0].Apply();

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
            pointLightEffect.CurrentTechnique.Passes[0].Apply();
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

        /// <summary>
        /// Draws light from emissive textures.
        /// </summary>
        private void DrawEmissiveLight()
        {
            // Set GBuffer
            emissiveLightEffect.Parameters["colorMap"].SetValue(gBuffer.ColorRT);

            // Set parameters
            emissiveLightEffect.Parameters["GBufferTextureSize"].SetValue(gBuffer.Size);

            // Apply changes
            emissiveLightEffect.Techniques[0].Passes[0].Apply();

            // Draw
            fullScreenQuad.Draw(graphicsDevice);
        }

        #endregion

        #region Billboards

        /// <summary>
        /// Draws billboard colors in a forward pass using previously
        ///  accumulated light information.
        /// </summary>
        private void DrawBillboards()
        {
            // Set transforms
            renderBillboards.Parameters["World"].SetValue(Matrix.Identity);
            renderBillboards.Parameters["View"].SetValue(core.ActiveScene.DeprecatedCamera.View);
            renderBillboards.Parameters["Projection"].SetValue(core.ActiveScene.DeprecatedCamera.Projection);

            // Set buffers
            core.GraphicsDevice.SetVertexBuffer(billboardVertexBuffer);
            core.GraphicsDevice.Indices = billboardIndexBuffer;

            // Set render states
            core.GraphicsDevice.BlendState = BlendState.Opaque;
            core.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            core.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            core.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            // Set technique
            //renderBillboards.CurrentTechnique = renderBillboards.Techniques["Default"];

            // First pass renders opaque part of texture.
            // These are depth sorted using depth-stencil buffer.
            renderBillboards.Parameters["AlphaTestDirection"].SetValue(1f);
            for (int i = 0; i < maxVisibleBillboards; i++)
            {
                DrawBillboardPass(i);
            }

            /*
            // Second pass renders unsorted alpha fringe of texture.
            // This yields decent looking results and is much cheaper than a CPU sort.
            core.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            core.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            renderBillboards.Parameters["AlphaTestDirection"].SetValue(-1f);
            for (int i = 0; i < maxVisibleBillboards; i++)
            {
                DrawBillboardPass(i);
            }
            */
        }

        /// <summary>
        /// Draw static batches with current settings.
        /// </summary>
        /// <param name="current">Current billboard index.</param>
        private void DrawBillboardPass(int current)
        {
            // Apply parameters
            renderBillboards_Texture.SetValue(visibleBillboards[current].Texture);
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
                    billboardVertexBuffer.VertexCount,
                    0,
                    2);
            }
        }

        /// <summary>
        /// Creates billboard template.
        /// </summary>
        private void CreateBillboardTemplate()
        {
            // Set dimensions of billboard 
            const float w = 0.5f;
            const float h = 0.5f;

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
            billboardVertexBuffer = new VertexBuffer(core.GraphicsDevice, VertexPositionNormalTextureBump.VertexDeclaration, 4, BufferUsage.WriteOnly);
            billboardIndexBuffer = new IndexBuffer(core.GraphicsDevice, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);

            // Set data
            billboardVertexBuffer.SetData(billboardVertices);
            billboardIndexBuffer.SetData(billboardIndices);
        }

        #endregion

    }

}
