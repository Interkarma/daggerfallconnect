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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DeepEngine.Rendering;
#endregion

namespace DeepEngine.Utility
{

    /// <summary>
    /// Creates a grid mesh.
    /// </summary>
    public class Grid
    {

        #region Fields

        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;

        #endregion

        #region Properties

        /// <summary>
        /// Gets vertex buffer.
        /// </summary>
        public VertexBuffer VertexBuffer
        {
            get { return vertexBuffer; }
        }

        /// <summary>
        /// Gets index buffer.
        /// </summary>
        public IndexBuffer IndexBuffer
        {
            get { return indexBuffer; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public Grid()
        {
        }

        /// <summary>
        /// Create constructor.
        /// </summary>
        /// <param name="graphicsDevice">GraphicsDevice.</param>
        /// <param name="dimension">Number of vertices wide and high.</param>
        /// <param name="scale">Size of each vertex in world units.</param>
        public Grid(GraphicsDevice graphicsDevice, int dimension, float scale)
        {
            CreateGrid(graphicsDevice, dimension, scale);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a new grid.
        /// </summary>
        /// <param name="graphicsDevice">GraphicsDevice.</param>
        /// <param name="dimension">Number of vertices wide and high.</param>
        /// <param name="scale">Size of each vertex in world units.</param>
        public void CreateGrid(GraphicsDevice graphicsDevice, int dimension, float scale)
        {
            // Create grid
            VertexPositionNormalTextureBump[] vertices = CreateVertices(dimension, scale);
            short[] indices = CreateIndices(dimension, scale);
            CreateBuffers(graphicsDevice, vertices, indices, out vertexBuffer, out indexBuffer);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Create vertices for a terrain tile.
        /// </summary>
        /// <param name="dimension">Number of vertices wide and high.</param>
        /// <param name="scale">Size of each vertex in world units.</param>
        /// <remarks>Vertex array.</remarks>
        private VertexPositionNormalTextureBump[] CreateVertices(int dimension, float scale)
        {
            // Create vertex array
            int max = dimension + 1;
            VertexPositionNormalTextureBump[] vertices =
                new VertexPositionNormalTextureBump[max * max];

            // Set vertices
            for (int x = 0; x < max; x++)
            {
                for (int y = 0; y < max; y++)
                {
                    int pos = x + y * max;
                    vertices[pos].Position = new Vector3(x * scale, 0f, y * scale);
                    vertices[pos].Normal = Vector3.Up;
                    vertices[pos].TextureCoordinate = new Vector2((float)x / (float)dimension, (float)y / (float)dimension);
                }
            }

            return vertices;
        }

        /// <summary>
        /// Create indices for a terrain tile.
        /// </summary>
        /// <returns>Index array.</returns>
        /// <param name="dimension">Number of vertices wide and high.</param>
        /// <param name="scale">Size of each vertex in world units.</param>
        private short[] CreateIndices(int dimension, float scale)
        {
            // Create index array
            int max = dimension;
            short[] indices = new short[max * max * 6];

            // Set indices
            int counter = 0;
            for (int y = 0; y < max; y++)
            {
                for (int x = 0; x < max; x++)
                {
                    short lowerLeft = (short)(x + y * (max + 1));
                    short lowerRight = (short)((x + 1) + y * (max + 1));
                    short topLeft = (short)(x + (y + 1) * (max + 1));
                    short topRight = (short)((x + 1) + (y + 1) * (max + 1));

                    indices[counter++] = lowerRight;
                    indices[counter++] = topLeft;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topRight;
                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                }
            }

            return indices;
        }

        /// <summary>
        /// Creates VertexBuffer and IndexBuffer from arrays.
        /// </summary>
        /// <param name="graphicsDevice">GraphicsDevice.</param>
        /// <param name="vertices">Vertex array.</param>
        /// <param name="indices">Index array.</param>
        /// <param name="vertexBuffer">VertexBuffer out.</param>
        /// <param name="indexBuffer">Index buffer out.</param>
        private void CreateBuffers(
            GraphicsDevice graphicsDevice,
            VertexPositionNormalTextureBump[] vertices,
            short[] indices,
            out VertexBuffer vertexBuffer,
            out IndexBuffer indexBuffer)
        {
            // Create VertexBuffer
            vertexBuffer = new VertexBuffer(
                graphicsDevice,
                VertexPositionNormalTextureBump.VertexDeclaration,
                vertices.Length,
                BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionNormalTextureBump>(vertices);

            // Create IndexBuffer
            indexBuffer = new IndexBuffer(
                graphicsDevice,
                IndexElementSize.SixteenBits,
                indices.Length,
                BufferUsage.WriteOnly);
            indexBuffer.SetData<short>(indices);
        }

        #endregion

    }

}


