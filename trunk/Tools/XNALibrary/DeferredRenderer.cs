// Project:         XNALibrary
// Description:     Simple XNA game library for DaggerfallConnect.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace XNALibrary
{

    /// <summary>
    /// Extends default renderer with deferred rendering.
    ///  This implementation has to make do without the Content Pipeline as we're building all the models at run-time.
    ///  Check links in this comment block for the original versions using the Content Pipeline.
    ///  Many thanks to Catalin Zima for the excellent tutorial! http://www.catalinzima.com
    ///  Also thanks to Roy Triesscheijn for the XNA 4 version! http://roy-t.nl/index.php/2010/12/28/deferred-rendering-in-xna4-0-source-code/
    /// </summary>
    public class DeferredRenderer : DefaultRenderer
    {
        #region QuadRenderer

        protected class QuadRenderer
        {
            // Variables
            VertexBuffer vb;
            IndexBuffer ib;

            // Constructor
            public QuadRenderer(GraphicsDevice graphicsDevice)
            {
                // Vertices
                VertexPositionTexture[] vertices =
            {
                new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 0))
            };

                // Create Vertex Buffer
                vb = new VertexBuffer(
                    graphicsDevice,
                    VertexPositionTexture.VertexDeclaration,
                    vertices.Length,
                    BufferUsage.None);
                vb.SetData<VertexPositionTexture>(vertices);

                // Indices
                ushort[] indices = { 0, 1, 2, 2, 3, 0 };

                // Create Index Buffer
                ib = new IndexBuffer(
                    graphicsDevice,
                    IndexElementSize.SixteenBits,
                    indices.Length,
                    BufferUsage.None);
                ib.SetData<ushort>(indices);
            }

            // Draw and set buffers
            public void Draw(GraphicsDevice graphicsDevice)
            {
                graphicsDevice.SetVertexBuffer(vb);
                graphicsDevice.Indices = ib;
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
            }

            // Just set buffers
            public void JustSetBuffers(GraphicsDevice graphicsDevice)
            {
                graphicsDevice.SetVertexBuffer(vb);
                graphicsDevice.Indices = ib;
            }

            // Just draw without setting buffers
            public void JustDraw(GraphicsDevice graphicsDevice)
            {
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
            }
        }

        #endregion

        #region Class Variables

        // GBuffer render targets
        private RenderTarget2D colorRT;     // Color and specular intensity
        private RenderTarget2D normalRT;    // Normals + specular power
        private RenderTarget2D depthRT;     // Depth
        private RenderTarget2D lightRT;     // Lighting

        // GBuffer effects
        private Effect clearBufferEffect;
        private Effect renderBufferEffect;
        private Effect directionalLightEffect;
        private Effect pointLightEffect;
        private Effect finalCombineEffect;

        // GBuffer textures
        //private Texture2D nullNormalTexture;
        //private Texture2D nullSpecularTexture;

        // Lighting geometry
        private Model sphereModel;          // Point light volume

        // GBuffer size
        Viewport viewport;
        private Vector2 size;
        private Vector2 halfPixel;

        // Quad drawing
        private QuadRenderer quadRenderer;

        // Lights
        private AmbientLight ambientLight;
        private List<DirectionalLight> directionalLights;

        #endregion

        #region Class Structures

        /// <summary>
        /// Defines ambient light.
        /// </summary>
        public struct AmbientLight
        {
            public Color Color;
            public float Intensity;
        }

        /// <summary>
        /// Defines a directional light for the deferred renderer.
        /// </summary>
        public struct DirectionalLight
        {
            public Vector3 Direction;
            public Color Color;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets size of render targets.
        /// </summary>
        public Vector2 Size
        {
            get { return size; }
        }

        /// <summary>
        /// Gets list of directional lights.
        /// </summary>
        public List<DirectionalLight> DirectionalLights
        {
            get { return directionalLights; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="textureManager"></param>
        public DeferredRenderer(TextureManager textureManager)
            : base(textureManager)
        {
            quadRenderer = new QuadRenderer(graphicsDevice);
            directionalLights = new List<DirectionalLight>();
        }

        #endregion

        #region Device Events

        /// <summary>
        /// Called when device is reset and we need to recreate resources.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">EventArgs.</param>
        protected override void GraphicsDevice_DeviceReset(object sender, EventArgs e)
        {
            // Work around .fx bug in XNA 4.0.
            // XNA will error if any sampler state has a SurfaceFormat.Single attached,
            // even if that sampler state is not in use.
            // In this case, it is SamplerState[2] (depth buffer in deferred renderer).
            // Source1: http://forums.create.msdn.com/forums/p/61268/438840.aspx
            // Source2: http://www.gamedev.net/topic/603699-xna-framework-hidef-profile-requires-texturefilter-to-be-point-when-using-texture-format-single/
            graphicsDevice.SamplerStates[2] = SamplerState.LinearClamp;
            graphicsDevice.SamplerStates[2] = SamplerState.PointClamp;

            CreateGBuffer();
        }

        /// <summary>
        /// Called when device is lost.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">EventArgs.</param>
        protected override void GraphicsDevice_DeviceLost(object sender, EventArgs e)
        {
            throw new Exception("Not implemented.");
        }

        #endregion

        #region Public Overrides

        /// <summary>
        /// Called when the renderer should initialise.
        /// </summary>
        public override void Initialise()
        {
            base.Initialise();

            CreateGBuffer();
        }

        /// <summary>
        /// Called when the render should load content.
        /// </summary>
        /// <param name="content">ContentManager.</param>
        public override void LoadContent(ContentManager content)
        {
            // Load effects
            clearBufferEffect = content.Load<Effect>(@"Effects\ClearGBuffer");
            renderBufferEffect = content.Load<Effect>(@"Effects\RenderGBuffer");
            directionalLightEffect = content.Load<Effect>(@"Effects\DirectionalLight");
            pointLightEffect = content.Load<Effect>(@"Effects\PointLight");
            finalCombineEffect = content.Load<Effect>(@"Effects\CombineFinal");

            // Load models
            sphereModel = content.Load<Model>(@"Models\sphere");

            // Set default ambient light
            ambientLight.Color = Color.White;
            ambientLight.Intensity = 0.0f;
            
            // Add default directional lights
            DirectionalLight d0;
            d0.Direction = new Vector3(-0.4f, -0.6f, 0.0f);
            d0.Color = Color.FromNonPremultiplied(64, 64, 64, 255);
            directionalLights.Add(d0);

            // Load textures
            //nullNormalTexture = content.Load<Texture2D>(@"Textures\null_normal");
            //nullSpecularTexture = content.Load<Texture2D>(@"Textures\null_specular");

            // Assign null textures to render effect
            //renderBufferEffect.Parameters["NormalMap"].SetValue(nullNormalTexture);
            //renderBufferEffect.Parameters["SpecularMap"].SetValue(nullSpecularTexture);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Render active scene.
        /// </summary>
        public override void Draw()
        {
#if DEBUG
            // Start timing
            stopwatch.Reset();
            stopwatch.Start();
#endif

            // Store current viewport.
            // This is important for WinForms apps as we could
            // be drawing to a subrect of the total surface.
            // This viewport needs to be reset every time the render
            // target changes.
            viewport = graphicsDevice.Viewport;

            // Batch scene
            ClearBatches();
            BatchNode(scene.Root, true);

            // Setup gbuffer
            SetGBuffer();
            ClearGBuffer();

            // Draw scene
            DrawBatches();

            // Resolve gbuffer
            ResolveGBuffer();

            // Draw lights
            DrawLights();

            // Combine final image
            Finalise();

            // Restore viewport
            graphicsDevice.Viewport = viewport;

            // Draw billboard batches
            if (HasOptionsFlags(RendererOptions.Flats))
            {
                billboardManager.Draw(camera);
            }

            // Draw compass
            if (HasOptionsFlags(RendererOptions.Compass))
            {
                //compass.Draw(camera);
            }

            // Draw debug version
            //DrawDebug();

#if DEBUG
            // End timing
            stopwatch.Stop();
            drawTime = stopwatch.ElapsedMilliseconds;
#endif
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates GBuffer.
        /// </summary>
        private void CreateGBuffer()
        {
            // Get size of back buffer
            viewport = graphicsDevice.Viewport;
            int width = graphicsDevice.PresentationParameters.BackBufferWidth;
            int height = graphicsDevice.PresentationParameters.BackBufferHeight;
            size = new Vector2(width, height);
            halfPixel = new Vector2(0.5f / (float)viewport.Width, 0.5f / (float)viewport.Height);

            // Create render targets
            colorRT = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            normalRT = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);
            depthRT = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Single, DepthFormat.None);
            lightRT = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);
        }

        /// <summary>
        /// Sets GBuffer render targets.
        /// </summary>
        private void SetGBuffer()
        {
            graphicsDevice.SetRenderTargets(colorRT, normalRT, depthRT);
            graphicsDevice.Viewport = viewport;
        }

        /// <summary>
        /// Resets GBuffer render targets.
        /// </summary>
        private void ResolveGBuffer()
        {
            graphicsDevice.SetRenderTargets(null);
            graphicsDevice.Viewport = viewport;
        }

        /// <summary>
        /// Clear GBuffer.
        /// </summary>
        private void ClearGBuffer()
        {
            graphicsDevice.BlendState = BlendState.Opaque;
            clearBufferEffect.Techniques[0].Passes[0].Apply();
            quadRenderer.Draw(graphicsDevice);
        }

        #endregion

        #region Rendering

        /// <summary>
        /// Renders batches of visible geometry to GBuffer.
        /// </summary>
        protected new void DrawBatches()
        {
            // Update view and projection matrices
            renderBufferEffect.Parameters["View"].SetValue(camera.View);
            renderBufferEffect.Parameters["Projection"].SetValue(camera.Projection);

            // Set render states
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            graphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;

            // Iterate batches
            foreach (var batch in batches)
            {
                // Do nothing if batch empty
                if (batch.Value.Count == 0)
                    continue;

                // Set texture
                if (batch.Key != TextureManager.GroundBatchKey)
                    renderBufferEffect.Parameters["Texture"].SetValue(textureManager.GetTexture(batch.Key));

                // Iterate batch items
                foreach (var batchItem in batch.Value)
                {
                    // Handle ground textures
                    if (batch.Key == TextureManager.GroundBatchKey)
                        renderBufferEffect.Parameters["Texture"].SetValue(batchItem.Texture);

                    // Set vertex buffer
                    graphicsDevice.SetVertexBuffer(batchItem.VertexBuffer);

                    // Set world transform
                    renderBufferEffect.Parameters["World"].SetValue(batchItem.Matrix);

                    // Apply changes
                    renderBufferEffect.CurrentTechnique.Passes[0].Apply();

                    // Draw based on indexed flag
                    if (batchItem.Indexed)
                    {
                        // Set index buffer
                        graphicsDevice.Indices = batchItem.IndexBuffer;

                        // Draw indexed primitives
                        graphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            0,
                            0,
                            batchItem.NumVertices,
                            batchItem.StartIndex,
                            batchItem.PrimitiveCount);
                    }
                    else
                    {
                        // Draw primitives
                        graphicsDevice.DrawPrimitives(
                            PrimitiveType.TriangleList,
                            batchItem.StartIndex,
                            batchItem.PrimitiveCount);
                    }
                }
            }
        }

        /// <summary>
        /// Draw a debug version of GBuffer.
        /// </summary>
        protected void DrawDebug()
        {
            // Width + Height
            int width = 320;
            int height = 180;

            // Begin sprite batch
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null);

            //Set up Drawing Rectangle
            Rectangle rect;
            rect.X = 0;
            rect.Y = 0;
            rect.Width = width;
            rect.Height = height;

            // Draw color
            spriteBatch.Draw(colorRT, rect, Color.White);

            // Draw normal
            rect.Y += height;
            spriteBatch.Draw(normalRT, rect, Color.White);

            // Draw depth
            rect.Y += height;
            spriteBatch.Draw(depthRT, rect, Color.White);

            // Draw light
            rect.Y += height;
            spriteBatch.Draw(lightRT, rect, Color.White);

            // End sprite batch
            spriteBatch.End();
        }

        /// <summary>
        /// Combines render targets to back buffer.
        /// </summary>
        protected void Finalise()
        {
            // Set render target
            graphicsDevice.SetRenderTarget(null);

            // Set render states
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Set values
            finalCombineEffect.Parameters["colorMap"].SetValue(colorRT);
            finalCombineEffect.Parameters["lightMap"].SetValue(lightRT);
            finalCombineEffect.Parameters["depthMap"].SetValue(depthRT);
            finalCombineEffect.Parameters["halfPixel"].SetValue(halfPixel);
            finalCombineEffect.Parameters["AmbientColor"].SetValue(ambientLight.Color.ToVector3());
            finalCombineEffect.Parameters["AmbientIntensity"].SetValue(ambientLight.Intensity);

            // Apply changes and draw
            finalCombineEffect.Techniques[0].Passes[0].Apply();
            quadRenderer.Draw(graphicsDevice);
        }

        #endregion

        #region Lighting

        /// <summary>
        /// Draws lights.
        /// </summary>
        protected void DrawLights()
        {
            // Set render states
            graphicsDevice.SetRenderTarget(lightRT);
            graphicsDevice.Clear(Color.Transparent);
            graphicsDevice.BlendState = BlendState.AlphaBlend;
            graphicsDevice.DepthStencilState = DepthStencilState.None;

            // Draw directional lights
            foreach (var dl in directionalLights)
            {
                DrawDirectionalLight(dl.Direction, dl.Color);
            }

            // Draw point lights
            foreach (var pl in pointLightBatch)
            {
                DrawPointLight(pl.TransformedBounds.Center, Color.White, PointLightNode.Radius, 1.0f);
            }

            // Draw personal light
            DrawPointLight(camera.Position, Color.White, PointLightNode.Radius, 1.0f);
        }

        /// <summary>
        /// Draws a directional light.
        /// </summary>
        /// <param name="lightDirection">Light direction.</param>
        /// <param name="lightColor">Light color.</param>
        private void DrawDirectionalLight(Vector3 lightDirection, Color lightColor)
        {
            // Set GBuffer
            directionalLightEffect.Parameters["colorMap"].SetValue(colorRT);
            directionalLightEffect.Parameters["normalMap"].SetValue(normalRT);
            directionalLightEffect.Parameters["depthMap"].SetValue(depthRT);

            // Set light properties
            directionalLightEffect.Parameters["lightDirection"].SetValue(lightDirection);
            directionalLightEffect.Parameters["Color"].SetValue(lightColor.ToVector3());

            // Set camera
            directionalLightEffect.Parameters["cameraPosition"].SetValue(camera.Position);
            directionalLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(camera.View * camera.Projection));

            // Set size
            directionalLightEffect.Parameters["halfPixel"].SetValue(halfPixel);

            // Apply changes
            directionalLightEffect.Techniques[0].Passes[0].Apply();

            // Draw
            quadRenderer.Draw(graphicsDevice);
        }

        /// <summary>
        /// Draws a point light.
        /// </summary>
        /// <param name="lightPosition">Light position.</param>
        /// <param name="color">Light colour.</param>
        /// <param name="lightRadius">Light radius.</param>
        /// <param name="lightIntensity">Light intensity.</param>
        private void DrawPointLight(Vector3 lightPosition, Color color, float lightRadius, float lightIntensity)
        {
            // Set GBuffer
            pointLightEffect.Parameters["colorMap"].SetValue(colorRT);
            pointLightEffect.Parameters["normalMap"].SetValue(normalRT);
            pointLightEffect.Parameters["depthMap"].SetValue(depthRT);

            // Compute the light world matrix.
            // Scale according to light radius, and translate it to light position.
            Matrix sphereWorldMatrix = Matrix.CreateScale(lightRadius) * Matrix.CreateTranslation(lightPosition);
            pointLightEffect.Parameters["World"].SetValue(sphereWorldMatrix);
            pointLightEffect.Parameters["View"].SetValue(camera.View);
            pointLightEffect.Parameters["Projection"].SetValue(camera.Projection);

            // Light position
            pointLightEffect.Parameters["lightPosition"].SetValue(lightPosition);

            // Set the color, radius and intensity
            pointLightEffect.Parameters["Color"].SetValue(color.ToVector3());
            pointLightEffect.Parameters["lightRadius"].SetValue(lightRadius);
            pointLightEffect.Parameters["lightIntensity"].SetValue(lightIntensity);

            // Parameters for specular computations
            pointLightEffect.Parameters["cameraPosition"].SetValue(camera.Position);
            pointLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(camera.View * camera.Projection));

            // Size of a halfpixel, for texture coordinates alignment
            pointLightEffect.Parameters["halfPixel"].SetValue(halfPixel);

            // Calculate the distance between the camera and light center
            float cameraToCenter = Vector3.Distance(camera.Position, lightPosition);

            // If we are inside the light volume, draw the sphere's inside face
            if (cameraToCenter < lightRadius)
                graphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            else
                graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            graphicsDevice.DepthStencilState = DepthStencilState.None;

            pointLightEffect.Techniques[0].Passes[0].Apply();
            foreach (ModelMesh mesh in sphereModel.Meshes)
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
