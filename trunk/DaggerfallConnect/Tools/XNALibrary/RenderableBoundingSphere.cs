// Project:         XNALibrary
// Description:     Explore and export 3D models from Daggerfall.
// Copyright:       Copyright (C) 2011 Gavin Clayton
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
#endregion

namespace XNALibrary
{

    /// <summary>
    /// Renders a BoundingSphere as a series of line primitives
    /// </summary>
    public class RenderableBoundingSphere
    {

        #region Class Variables

        // Appearance
        private int sphereResolution;
        private Color boundingSphereColor = Color.White;

        // XNA
        private GraphicsDevice graphicsDevice;
        private BoundingSphere boundingSphere;
        private VertexBuffer vertexBuffer;
        private VertexDeclaration lineVertexDeclaration;
        private BasicEffect lineEffect;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets BoundingSphere colour.
        /// </summary>
        public Color Color
        {
            get { return boundingSphereColor; }
            set { boundingSphereColor = value; }
        }

        /// <summary>
        /// Gets or sets the BoundingSphere to draw.
        /// </summary>
        public BoundingSphere BoundingSphere
        {
            get { return this.boundingSphere; }
            set { SetBoundingSphere(value); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public RenderableBoundingSphere(GraphicsDevice graphicsDevice)
        {
            // Setup line BasicEffect
            lineEffect = new BasicEffect(graphicsDevice, null);
            lineEffect.LightingEnabled = false;
            lineEffect.TextureEnabled = false;
            lineEffect.VertexColorEnabled = true;

            // Create vertex declaration
            lineVertexDeclaration = new VertexDeclaration(graphicsDevice, VertexPositionColor.VertexElements);

            // Store GraphicsDevice
            this.graphicsDevice = graphicsDevice;

            // Initialise sphere
            InitializeSphere(graphicsDevice, 30);
        }

        /// <summary>
        /// BoundingSphere constructor.
        /// </summary>
        /// <param name="boundingSphere">BoundingSphere.</param>
        public RenderableBoundingSphere(GraphicsDevice graphicsDevice, BoundingSphere boundingSphere)
            : this(graphicsDevice)
        {
            SetBoundingSphere(boundingSphere);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Draw the specified BoundingSphere.
        /// </summary>
        /// <param name="boundingSphere">New bounding sphere to draw.</param>
        /// <param name="viewMatrix">View matrix.</param>
        /// <param name="projectionMatrix">Projection matrix.</param>
        /// <param name="worldMatrix">World Matrix.</param>
        public void Draw(BoundingSphere boundingSphere, Matrix viewMatrix, Matrix projectionMatrix, Matrix worldMatrix)
        {
            // Set new bounding sphere and draw as normal
            SetBoundingSphere(boundingSphere);
            Draw(viewMatrix, projectionMatrix, worldMatrix);
        }

        /// <summary>
        /// Draw the current BoundingSphere.
        /// </summary>
        /// <param name="viewMatrix">View matrix.</param>
        /// <param name="projectionMatrix">Projections matrix.</param>
        /// <param name="worldMatrix">World Matrix.</param>
        public void Draw(Matrix viewMatrix, Matrix projectionMatrix, Matrix worldMatrix)
        {
            RenderSphere(this.boundingSphere,
                this.graphicsDevice,
                viewMatrix,
                projectionMatrix,
                worldMatrix,
                this.boundingSphereColor);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes the graphics objects for rendering the sphere.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to use when rendering.</param>
        /// <param name="sphereResolution">The number of line segments 
        ///     to use for each of the three circles.</param>
        private void InitializeSphere(GraphicsDevice graphicsDevice, int sphereResolution)
        {
            this.sphereResolution = sphereResolution;
            VertexPositionColor[] verts = new VertexPositionColor[(sphereResolution + 1) * 3];

            int index = 0;
            float step = MathHelper.TwoPi / (float)sphereResolution;

            // Create the loop on the XY plane first
            for (float a = 0f; a <= MathHelper.TwoPi; a += step)
            {
                verts[index++] = new VertexPositionColor(
                    new Vector3((float)Math.Cos(a), (float)Math.Sin(a), 0f),
                    Color.White);
            }

            // Next on the XZ plane
            for (float a = 0f; a <= MathHelper.TwoPi; a += step)
            {
                verts[index++] = new VertexPositionColor(
                    new Vector3((float)Math.Cos(a), 0f, (float)Math.Sin(a)),
                    Color.White);
            }

            // Finally on the YZ plane
            for (float a = 0f; a <= MathHelper.TwoPi; a += step)
            {
                verts[index++] = new VertexPositionColor(
                    new Vector3(0f, (float)Math.Cos(a), (float)Math.Sin(a)),
                    Color.White);
            }

            vertexBuffer = new VertexBuffer(
                graphicsDevice,
                verts.Length * VertexPositionColor.SizeInBytes,
                BufferUsage.None);
            vertexBuffer.SetData(verts);
        }

        /// <summary>
        /// Renders a bounding sphere using a single color for all three axis.
        /// </summary>
        /// <param name="sphere">The sphere to render.</param>
        /// <param name="graphicsDevice">The graphics device to use when rendering.</param>
        /// <param name="view">The current view matrix.</param>
        /// <param name="projection">The current projection matrix.</param>
        /// <param name="world">The current world matrix.</param>
        /// <param name="color">The color to use for rendering the circles.</param>
        private void RenderSphere(
            BoundingSphere sphere,
            GraphicsDevice graphicsDevice,
            Matrix view,
            Matrix projection,
            Matrix world,
            Color color)
        {
            if (vertexBuffer == null)
                return;

            graphicsDevice.VertexDeclaration = lineVertexDeclaration;
            graphicsDevice.Vertices[0].SetSource(
                  vertexBuffer,
                  0,
                  VertexPositionColor.SizeInBytes);

            world = (Matrix.CreateScale(sphere.Radius) * Matrix.CreateTranslation(sphere.Center)) * world;

            lineEffect.World = world;
            lineEffect.View = view;
            lineEffect.Projection = projection;
            lineEffect.DiffuseColor = color.ToVector3();

            lineEffect.Begin();
            foreach (EffectPass pass in lineEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                //render each circle individually
                graphicsDevice.DrawPrimitives(
                      PrimitiveType.LineStrip,
                      0,
                      sphereResolution);
                graphicsDevice.DrawPrimitives(
                      PrimitiveType.LineStrip,
                      sphereResolution + 1,
                      sphereResolution);
                graphicsDevice.DrawPrimitives(
                      PrimitiveType.LineStrip,
                      (sphereResolution + 1) * 2,
                      sphereResolution);

                pass.End();
            }
            lineEffect.End();
        }

        /// <summary>
        /// Sets the BoundingSphere to draw.
        /// </summary>
        /// <param name="boundingSphere">BoundingSphere.</param>
        private void SetBoundingSphere(BoundingSphere boundingSphere)
        {
            this.boundingSphere = boundingSphere;
        }

        #endregion

    }

}
