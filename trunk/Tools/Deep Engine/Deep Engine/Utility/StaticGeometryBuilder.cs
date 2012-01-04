﻿// Project:         Deep Engine
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
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DeepEngine.Core;
using DeepEngine.Daggerfall;
using DeepEngine.Rendering;
using DeepEngine.World;
#endregion

namespace DeepEngine.Utility
{

    /// <summary>
    /// Builds combined vertex and index buffers.
    ///  Groups primitives by material and pre-transforms vertices.
    ///  Useful for building efficient static level geometry from smaller pieces.
    /// </summary>
    public class StaticGeometryBuilder
    {
        #region Fields

        // XNA
        GraphicsDevice graphicsDevice;

        // Static buffers
        public VertexBuffer vertexBuffer;
        public IndexBuffer indexBuffer;

        // Static batch dictionary
        Dictionary<int, StaticBatch> batchDictionary;

        // Builder dictionary used during build process
        Dictionary<int, BatchData> builderDictionary;

        #endregion

        #region Structures

        /// <summary>
        /// Describes a batch of combined geometry grouped by material.
        /// </summary>
        public struct StaticBatch
        {
            public int StartIndex;
            public int PrimitiveCount;
        }

        /// <summary>
        /// Describes a batch of combined geometry during the build process.
        /// </summary>
        public struct BatchData
        {
            public List<VertexPositionNormalTextureBump> Vertices;
            public List<int> Indices;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets grpahics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get { return graphicsDevice; }
        }

        /// <summary>
        /// Gets combined vertex buffer.
        ///  Not valid until data has been added and ApplyBuilder() has been called.
        /// </summary>
        public VertexBuffer VertexBuffer
        {
            get { return vertexBuffer; }
        }

        /// <summary>
        /// Gets combined index buffer.
        ///  Not valid until data has been added and ApplyBuilder() has been called.
        /// </summary>
        public IndexBuffer IndexBuffer
        {
            get { return indexBuffer; }
        }

        /// <summary>
        /// Gets static batches grouped and keyed by material.
        ///  Not valid until data has been added and ApplyBuilder() has been called.
        /// </summary>
        public Dictionary<int, StaticBatch> StaticBatches
        {
            get { return batchDictionary; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="graphicsDevice">Graphics device to use during buffer creation.</param>
        public StaticGeometryBuilder(GraphicsDevice graphicsDevice)
        {
            // Save objects
            this.graphicsDevice = graphicsDevice;

            // Start new builder
            NewBuilder();
        }

        #endregion

        #region Batch Building Methods

        /// <summary>
        /// Starts a new batch builder.
        ///  Current batches will be overwritten next time you call ApplyBuilder().
        /// </summary>
        public void NewBuilder()
        {
            // Create empty dictionaries
            batchDictionary = new Dictionary<int, StaticBatch>();
            builderDictionary = new Dictionary<int, BatchData>();
        }

        /// <summary>
        /// Adds buffer arrays to the batch builder.
        /// </summary>
        /// <param name="textureKey">Textue key for this geometry.</param>
        /// <param name="vertices">Vertex array.</param>
        /// <param name="indices">Index array.</param>
        /// <param name="matrix">Geometry transform to apply before adding.</param>
        public void AddToBuilder(int textureKey, VertexPositionNormalTextureBump[] vertices, int[] indices, Matrix matrix)
        {
            // Start new batch data
            BatchData batchData;
            batchData.Vertices = new List<VertexPositionNormalTextureBump>();
            batchData.Indices = new List<int>();

            // Add data
            batchData.Vertices.AddRange(vertices);
            batchData.Indices.AddRange(indices);
            AddToBuilder(textureKey, batchData, matrix);
        }

        /// <summary>
        /// Adds model data to the batch builder.
        /// </summary>
        /// <param name="modelData">Model data to add.</param>
        /// <param name="matrix">Transform to apply before adding model data.</param>
        /// <param name="matrix">Geometry transform to apply before adding.</param>
        public void AddToBuilder(ref ModelManager.ModelData modelData, Matrix matrix)
        {
            // Iterate submeshes
            BatchData batchData;
            foreach (var sm in modelData.SubMeshes)
            {
                // Start new batch data for this submesh
                batchData.Vertices = new List<VertexPositionNormalTextureBump>();
                batchData.Indices = new List<int>();

                int counter = 0;
                int index = sm.StartIndex;
                for (int tri = 0; tri < sm.PrimitiveCount; tri++)
                {
                    // Get indices
                    int i1 = modelData.Indices[index++];
                    int i2 = modelData.Indices[index++];
                    int i3 = modelData.Indices[index++];

                    // Get vertices
                    VertexPositionNormalTextureBump vert1 = modelData.Vertices[i1];
                    VertexPositionNormalTextureBump vert2 = modelData.Vertices[i2];
                    VertexPositionNormalTextureBump vert3 = modelData.Vertices[i3];

                    // Add vertices
                    batchData.Vertices.Add(vert1);
                    batchData.Vertices.Add(vert2);
                    batchData.Vertices.Add(vert3);

                    // Add indices
                    batchData.Indices.Add(counter++);
                    batchData.Indices.Add(counter++);
                    batchData.Indices.Add(counter++);
                }

                // Add to builder
                AddToBuilder(sm.TextureKey, batchData, matrix);
            }
        }

        /// <summary>
        /// Adds batch data to builder from another builder.
        /// </summary>
        /// <param name="builder">Source builder.</param>
        /// <param name="matrix">Geometry transform to apply before adding.</param>
        public void AddToBuilder(StaticGeometryBuilder builder, Matrix matrix)
        {
            // Add items to this builder.
            foreach (var item in builder.builderDictionary)
            {
                AddToBuilder(item.Key, item.Value, matrix);
            }
        }

        /// <summary>
        /// Adds batch data to the batch builder.
        ///  Geometry data is batched by key.
        /// </summary>
        /// <param name="textureKey">Key to batch against.</param>
        /// <param name="batchData">Data to add.</param>
        /// <param name="matrix">Geometry transform to apply before adding.</param>
        public void AddToBuilder(int textureKey, BatchData batchData, Matrix matrix)
        {
            BatchData builder;
            if (builderDictionary.ContainsKey(textureKey))
            {
                // Get current batch data
                builder = builderDictionary[textureKey];
            }
            else
            {
                // Start a new batch
                builder.Vertices = new List<VertexPositionNormalTextureBump>();
                builder.Indices = new List<int>();
                builderDictionary.Add(textureKey, builder);
            }

            // Transform vertices
            for (int i = 0; i < batchData.Vertices.Count; i++)
            {
                VertexPositionNormalTextureBump vertex = batchData.Vertices[i];
                vertex.Position = Vector3.Transform(vertex.Position, matrix);
                vertex.Normal = Vector3.TransformNormal(vertex.Normal, matrix);
                batchData.Vertices[i] = vertex;
            }

            // Add new vertices to builder
            int currentVertex = builder.Vertices.Count;
            builder.Vertices.AddRange(batchData.Vertices);

            // Update indices to new vertex base
            for (int i = 0; i < batchData.Indices.Count; i++)
            {
                batchData.Indices[i] += currentVertex;
            }

            // Add indices to builder
            builder.Indices.AddRange(batchData.Indices);

            // Update dictionary
            builderDictionary[textureKey] = builder;
        }

        /// <summary>
        /// Creates static vertex and index buffers from builder.
        ///  You can keep adding to the builder and calling this method to apply changes.
        /// </summary>
        public void ApplyBuilder()
        {
            // Create new batch dictionary
            batchDictionary = new Dictionary<int, StaticBatch>();

            // Count total vertices and indices
            int totalVertices = 0;
            int totalIndices = 0;
            foreach (var item in builderDictionary)
            {
                totalVertices += item.Value.Vertices.Count;
                totalIndices += item.Value.Indices.Count;
            }

            // Create static arrays
            VertexPositionNormalTextureBump[] allVertices = new VertexPositionNormalTextureBump[totalVertices];
            int[] allIndices = new int[totalIndices];

            // Populate static arrays
            int currentVertex = 0;
            int currentIndex = 0;
            foreach (var item in builderDictionary)
            {
                // Save current highest vertex and index
                int highestVertex = currentVertex;
                int highestIndex = currentIndex;

                // Copy vertex data
                foreach (var vertex in item.Value.Vertices)
                {
                    allVertices[currentVertex++] = vertex;
                }

                // Copy index data
                foreach (var index in item.Value.Indices)
                {
                    allIndices[currentIndex++] = highestVertex + index;
                }

                // Add batch details
                StaticBatch batch = new StaticBatch
                {
                    StartIndex = highestIndex,
                    PrimitiveCount = item.Value.Indices.Count / 3,
                };
                batchDictionary.Add(item.Key, batch);
            }

            // Create new static buffers
            vertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionNormalTextureBump.VertexDeclaration, allVertices.Length, BufferUsage.WriteOnly);
            indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, allIndices.Length, BufferUsage.WriteOnly);

            // Set buffer data
            vertexBuffer.SetData<VertexPositionNormalTextureBump>(allVertices);
            indexBuffer.SetData<int>(allIndices);
        }

        #endregion

    }

}
