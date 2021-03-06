﻿// Project:         XNALibrary
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
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace XNALibrary
{
    /// <summary>
    /// Helper class to load and store Daggerfall models for XNA. Does not load textures but does calc UV coordinates.
    ///  This is to allow developers to apply their own texture logic independent of model loading.
    ///  Model data is stored as vertex and index arrays and as vertex and index buffers. Use whichever suits
    ///  your application best. The original geometry is also stored if needed.
    /// </summary>
    public class ModelManager
    {

        #region Class Variables

        string arena2Path = string.Empty;
        private GraphicsDevice graphicsDevice;
        private Arch3dFile arch3dFile;
        private Dictionary<uint, ModelData> modelDataDict;
        private bool cacheModelData = true;

        private Random rnd = new Random();

        #endregion

        #region Class Structures

        /// <summary>
        /// Defines mesh data.
        /// </summary>
        public class ModelData
        {
            /// <summary>Native geometry as read from ARCH3D.BSA.</summary>
            public DFMesh DFMesh;

            /// <summary>Axis-aligned bounding box tightly surrounding mesh data.</summary>
            public BoundingBox BoundingBox;

            /// <summary>Bounding sphere tightly surrounding mesh data.</summary>
            public BoundingSphere BoundingSphere;

            /// <summary>Vertex array containing position, normal, texture coordinates, tangent, bitangent.</summary>
            public VertexPosNormalTexTanBitan[] Vertices;

            /// <summary>Index array desribing the triangles of this mesh.</summary>
            public short[] Indices;

            /// <summary>Vertex buffer.</summary>
            public VertexBuffer VertexBuffer;

            /// <summary>Index buffer.</summary>
            public IndexBuffer IndexBuffer;

            /// <summary>Data for each SubMesh, grouped by texture.</summary>
            public SubMeshData[] SubMeshes;

            /// <summary>
            /// Defines submesh data.
            /// </summary>
            public struct SubMeshData
            {
                /// <summary>TextureManager key.</summary>
                public int TextureKey;

                /// <summary>Location in the index array at which to start reading vertices.</summary>
                public int StartIndex;

                /// <summary>Number of primitives in this submesh.</summary>
                public int PrimitiveCount;
            }
        }

        #endregion

        #region Custom Vertex Formats

        /// <summary>
        /// Position, Normal, TextureCoordinate, Tangent, Bitangent (binormal).
        /// </summary>
        public struct VertexPosNormalTexTanBitan : IVertexType
        {
            Vector3 pos;
            Vector3 normal;
            Vector2 tex;
            Vector3 tan;
            Vector3 bitan;

            public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(32, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
                new VertexElement(44, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0));

            VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }

            public VertexPosNormalTexTanBitan(
                Vector3 Position,
                Vector3 Normal,
                Vector2 TextureCoordinate,
                Vector3 Tangent,
                Vector3 Binormal)
            {
                pos = Position;
                this.normal = Normal;
                tex = TextureCoordinate;
                this.tan = Tangent;
                this.bitan = Binormal;
            }

            public static bool operator !=(VertexPosNormalTexTanBitan left, VertexPosNormalTexTanBitan right)
            {
                return left.GetHashCode() != right.GetHashCode();
            }

            public static bool operator ==(VertexPosNormalTexTanBitan left, VertexPosNormalTexTanBitan right)
            {
                return left.GetHashCode() == right.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return this == (VertexPosNormalTexTanBitan)obj;
            }

            public Vector3 Position { get { return pos; } set { pos = value; } }
            public Vector3 Normal { get { return normal; } set { normal = value; } }
            public Vector2 TextureCoordinate { get { return tex; } set { tex = value; } }
            public Vector3 Tangent { get { return tan; } set { tan = value; } }
            public Vector3 Binormal { get { return bitan; } set { bitan = value; } }

            public static int SizeInBytes { get { return 56; } }

            public override int GetHashCode()
            {
                return pos.GetHashCode() | tex.GetHashCode() | normal.GetHashCode() | tan.GetHashCode() | bitan.GetHashCode();
            }

            public override string ToString()
            {
                return string.Format("{0},{1},{2}", pos.X, pos.Y, pos.Z);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets Arena2 path set at construction.
        /// </summary>
        public string Arena2Path
        {
            get { return arena2Path; }
        }

        /// <summary>
        /// Gets underlying Arch3dFile.
        /// </summary>
        public Arch3dFile Arch3dFile
        {
            get { return arch3dFile; }
        }

        /// <summary>
        /// Gets or sets flag controlling cache behaviour.
        ///  True will cache model data when loaded.
        ///  False will not cache model data when loaded.
        /// </summary>
        public bool CacheModelData
        {
            get { return cacheModelData; }
            set { cacheModelData = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="device">Graphics Device.</param>
        /// <param name="arena2Path">Path to Arena2 folder.</param>
        public ModelManager(GraphicsDevice device, string arena2Path)
        {
            // Setup
            graphicsDevice = device;
            arch3dFile = new Arch3dFile(Path.Combine(arena2Path, Arch3dFile.Filename), FileUsage.UseDisk, true);
            arch3dFile.AutoDiscard = true;
            this.arena2Path = arena2Path;
            modelDataDict = new Dictionary<uint, ModelData>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets flag stating if model is present in cache.
        /// </summary>
        /// <param name="id">ID of model</param>
        /// <returns>True if model is present in cache.</returns>
        public bool ContainsModel(uint id)
        {
            // Return from cache if present
            if (cacheModelData && modelDataDict.ContainsKey(id))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Get model data.
        ///  Model UVs will be aligned to power of two. Ensure PowerOfTwo
        ///  flag is set when loading textures with TextureManager.
        /// </summary>
        /// <param name="id">ID of model.</param>
        /// <returns>Model object.</returns>
        public ModelData GetModelData(uint id)
        {
            // Load model data
            ModelData modelData;
            LoadModelData(id, out modelData);
            return modelData;
        }

        /// <summary>
        /// Get model data.
        ///  Model UVs will be aligned to power of two. Ensure PowerOfTwo
        ///  flag is set when loading textures with TextureManager.
        /// </summary>
        /// <param name="id">ID of model.</param>
        /// <param name="model">ModelData out.</param>
        /// <returns>True if successful.</returns>
        public bool GetModelData(uint id, out ModelData modelData)
        {
            // Load model data
            return LoadModelData(id, out modelData);
        }

        /// <summary>
        /// Set model data. Use a unique id to store as different model data
        ///  or existing id to overwrite model data.
        /// </summary>
        /// <param name="id">ID of model.</param>
        /// <param name="model">Model object.</param>
        public void SetModelData(uint id, ref ModelData modelData)
        {
            // Do nothing if not caching models
            if (!cacheModelData)
                return;

            // Add or overwrite cache
            if (modelDataDict.ContainsKey(id))
                modelDataDict[id] = modelData;
            else
                modelDataDict.Add(id, modelData);
        }

        /// <summary>
        /// Removes model data from cache.
        /// </summary>
        /// <param name="id">ID of model.</param>
        public void RemoveModelData(uint id)
        {
            if (cacheModelData && modelDataDict.ContainsKey(id))
                modelDataDict.Remove(id);
        }

        /// <summary>
        /// Clear all cached model data.
        /// </summary>
        public void ClearModelData()
        {
            if (cacheModelData)
                modelDataDict.Clear();
        }

        /// <summary>
        /// Transforms vertices and bounding box of model data.
        ///  Source model data is unchanged.
        /// </summary>
        /// <param name="model">Source Model.</param>
        /// <param name="matrix">Matrix applied to output model.</param>
        /// <returns>Transformed Model.</returns>
        public ModelData TransformModelData(ref ModelData modelData, Matrix matrix)
        {
            // Transform model
            int vertexPos = 0;
            ModelData transformedModelData = modelData;
            foreach (var vertex in transformedModelData.Vertices)
            {
                transformedModelData.Vertices[vertexPos].Position = Vector3.Transform(vertex.Position, matrix);
                transformedModelData.Vertices[vertexPos].Normal = vertex.Normal;
                transformedModelData.Vertices[vertexPos].TextureCoordinate = vertex.TextureCoordinate;
                vertexPos++;
            }

            // Transform bounding box
            transformedModelData.BoundingBox.Min = Vector3.Transform(transformedModelData.BoundingBox.Min, matrix);
            transformedModelData.BoundingBox.Max = Vector3.Transform(transformedModelData.BoundingBox.Max, matrix);

            // Transform bounding sphere
            transformedModelData.BoundingSphere.Center = Vector3.Transform(transformedModelData.BoundingSphere.Center, matrix);

            return transformedModelData;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads model data from DFMesh.
        /// </summary>
        /// <param name="id">Key of source mesh.</param>
        /// <param name="model">ModelData out.</param>
        /// <returns>True if successful.</returns>
        private bool LoadModelData(uint id, out ModelData model)
        {
            // Return from cache if present
            if (cacheModelData && modelDataDict.ContainsKey(id))
            {
                model = modelDataDict[id];
                return true;
            }

            // New model object
            model = new ModelData();

            // Find mesh index
            int index = arch3dFile.GetRecordIndex(id);
            if (index == -1)
                return false;

            // Get DFMesh
            DFMesh dfMesh = arch3dFile.GetMesh(index);
            if (dfMesh.TotalVertices == 0)
                return false;

            // Load mesh data
            model.DFMesh = dfMesh;
            LoadVertices(ref model);
            LoadIndices(ref model);
            AddModelTangents(ref model);
            CreateModelBuffers(ref model);

            // Add to cache
            if (cacheModelData)
                modelDataDict.Add(id, model);

            return true;
        }

        /// <summary>
        /// Loads all vertices for this DFMesh.
        /// </summary>
        /// <param name="model">Model object.</param>
        private void LoadVertices(ref ModelData model)
        {
            // Allocate vertex buffer
            model.Vertices = new VertexPosNormalTexTanBitan[model.DFMesh.TotalVertices];

            // Track min and max vectors for bounding box
            Vector3 min = new Vector3(0, 0, 0);
            Vector3 max = new Vector3(0, 0, 0);

            // Loop through all submeshes
            int vertexCount = 0;
            foreach (DFMesh.DFSubMesh dfSubMesh in model.DFMesh.SubMeshes)
            {
                // Get texture dimensions for this submesh
                string archivePath = Path.Combine(arena2Path, TextureFile.IndexToFileName(dfSubMesh.TextureArchive));
                System.Drawing.Size sz = TextureFile.QuickSize(archivePath, dfSubMesh.TextureRecord);

                // Ensure texture dimensions are POW2 as TextureManager will be emitting POW2 textures
                int width = (PowerOfTwo.IsPowerOfTwo(sz.Width)) ? sz.Width : PowerOfTwo.NextPowerOfTwo(sz.Width);
                int height = (PowerOfTwo.IsPowerOfTwo(sz.Height)) ? sz.Height : PowerOfTwo.NextPowerOfTwo(sz.Height);
                Vector2 scale = new Vector2(
                    (float)sz.Width / (float)width,
                    (float)sz.Height / (float)height);

                // Loop through all planes in this submesh
                foreach (DFMesh.DFPlane dfPlane in dfSubMesh.Planes)
                {
                    // Copy each point in this plane to vertex buffer
                    foreach (DFMesh.DFPoint dfPoint in dfPlane.Points)
                    {
                        // Daggerfall uses a different axis layout than XNA.
                        // The Y and Z axes should be inverted so the model is displayed correctly.
                        // This also requires a change to winding order in LoadIndices().
                        Vector3 position = new Vector3(dfPoint.X, -dfPoint.Y, -dfPoint.Z);
                        Vector3 normal = new Vector3(dfPoint.NX, -dfPoint.NY, -dfPoint.NZ);

                        // Store vertex data
                        model.Vertices[vertexCount].Position = position;
                        model.Vertices[vertexCount].Normal = normal;
                        model.Vertices[vertexCount].TextureCoordinate = new Vector2(
                            (dfPoint.U / sz.Width) * scale.X,
                            (dfPoint.V / sz.Height) * scale.Y);

                        // Inrement count
                        vertexCount++;

                        // Compare min and max vectors
                        if (position.X < min.X) min.X = position.X;
                        if (position.Y < min.Y) min.Y = position.Y;
                        if (position.Z < min.Z) min.Z = position.Z;
                        if (position.X > max.X) max.X = position.X;
                        if (position.Y > max.Y) max.Y = position.Y;
                        if (position.Z > max.Z) max.Z = position.Z;
                    }
                }
            }

            // Create bounding box
            model.BoundingBox = new BoundingBox(min, max);

            // Find model centre
            Vector3 modelCenter;
            modelCenter.X = min.X + (max.X - min.X) / 2;
            modelCenter.Y = min.Y + (max.Y - min.Y) / 2;
            modelCenter.Z = min.Z + (max.Z - min.Z) / 2;

            // Find model radius
            float modelRadius;
            modelRadius = Vector3.Distance(min, max) / 2;

            // Create bounding sphere
            model.BoundingSphere = new BoundingSphere(modelCenter, modelRadius);
        }

        /// <summary>
        /// Build indices for this DFMesh.
        /// </summary>
        /// <param name="model">Model object.</param>
        private void LoadIndices(ref ModelData model)
        {
            // Allocate model data submesh buffer
            model.SubMeshes = new ModelData.SubMeshData[model.DFMesh.SubMeshes.Length];

            // Allocate index buffer
            model.Indices = new short[model.DFMesh.TotalTriangles * 3];

            // Iterate through all submeshes
            short indexCount = 0;
            short subMeshCount = 0, vertexCount = 0;
            foreach (DFMesh.DFSubMesh dfSubMesh in model.DFMesh.SubMeshes)
            {
                // Set start index and primitive count for this submesh
                model.SubMeshes[subMeshCount].StartIndex = indexCount;
                model.SubMeshes[subMeshCount].PrimitiveCount = dfSubMesh.TotalTriangles;

                // Iterate through all planes in this submesh
                foreach (DFMesh.DFPlane dfPlane in dfSubMesh.Planes)
                {
                    // Every DFPlane is a triangle fan radiating from point 0
                    short sharedPoint = vertexCount++;

                    // Index remaining points. There are (plane.Points.Length - 2) triangles in every plane
                    for (int tri = 0; tri < dfPlane.Points.Length - 2; tri++)
                    {
                        // Store 3 points of current triangle.
                        // The second two indices are swapped so the winding order is correct after
                        // inverting Y and Z axes in LoadVertices().
                        model.Indices[indexCount++] = sharedPoint;
                        model.Indices[indexCount++] = (short)(vertexCount + 1);
                        model.Indices[indexCount++] = vertexCount;

                        // Increment vertexCount to next point in fan
                        vertexCount++;
                    }

                    // Increment vertexCount to start of next fan in vertex buffer
                    vertexCount++;
                }

                // Increment submesh count
                subMeshCount++;
            }
        }

        /// <summary>
        /// Adds tangent and bitangent information to vertices.
        /// </summary>
        /// <param name="model">ModelData.</param>
        private void AddModelTangents(ref ModelData model)
        {
            foreach (var subMesh in model.SubMeshes)
            {
                int index = subMesh.StartIndex;
                for (int tri = 0; tri < subMesh.PrimitiveCount; tri++)
                {
                    // Get indices
                    short i1 = model.Indices[index++];
                    short i2 = model.Indices[index++];
                    short i3 = model.Indices[index++];

                    // Get vertices
                    VertexPosNormalTexTanBitan vert1 = model.Vertices[i1];
                    VertexPosNormalTexTanBitan vert2 = model.Vertices[i2];
                    VertexPosNormalTexTanBitan vert3 = model.Vertices[i3];

                    // Make tangent and bitangent
                    MakeTangentBitangent(ref vert1, ref vert2, ref vert3);

                    // Store updated vertices
                    model.Vertices[i1] = vert1;
                    model.Vertices[i2] = vert2;
                    model.Vertices[i3] = vert3;
                }
            }
        }

        /// <summary>
        /// Creates VertexBuffer and IndexBuffer from ModelData.
        /// </summary>
        /// <param name="model">ModelData.</param>
        private void CreateModelBuffers(ref ModelData model)
        {
            // Create VertexBuffer
            model.VertexBuffer = new VertexBuffer(
                graphicsDevice,
                VertexPosNormalTexTanBitan.VertexDeclaration,
                model.DFMesh.TotalVertices,
                BufferUsage.WriteOnly);
            model.VertexBuffer.SetData<VertexPosNormalTexTanBitan>(model.Vertices);

            // Create IndexBuffer
            model.IndexBuffer = new IndexBuffer(
                graphicsDevice,
                IndexElementSize.SixteenBits,
                model.DFMesh.TotalTriangles * 3,
                BufferUsage.WriteOnly);
            model.IndexBuffer.SetData<short>(model.Indices);
        }

        /// <summary>
        /// Calculates tangent and bitangent values.
        /// Source1: Lengyel, Eric. “Computing Tangent Space Basis Vectors for an Arbitrary Mesh”. Terathon Software 3D Graphics Library, 2001. http://www.terathon.com/code/tangent.html
        /// Source2: http://forums.create.msdn.com/forums/p/30443/172057.aspx
        /// </summary>
        private void MakeTangentBitangent(
            ref VertexPosNormalTexTanBitan vert1,
            ref VertexPosNormalTexTanBitan vert2,
            ref VertexPosNormalTexTanBitan vert3)
        {
            Vector3 v1 = vert1.Position;
            Vector3 v2 = vert2.Position;
            Vector3 v3 = vert3.Position;

            Vector2 w1 = vert1.TextureCoordinate;
            Vector2 w2 = vert2.TextureCoordinate;
            Vector2 w3 = vert3.TextureCoordinate;

            // All points in a Daggerfall plane have the same vertex normal.
            // Each vertex normal is equal to the plane normal.
            Vector3 planeNormal = vert1.Normal;

            float x1 = v2.X - v1.X;
            float x2 = v3.X - v1.X;
            float y1 = v2.Y - v1.Y;
            float y2 = v3.Y - v1.Y;
            float z1 = v2.Z - v1.Z;
            float z2 = v3.Z - v1.Z;

            float s1 = w2.X - w1.X;
            float s2 = w3.X - w1.X;
            float t1 = w2.Y - w1.Y;
            float t2 = w3.Y - w1.Y;

            float r = 1.0f / (s1 * t2 - s2 * t1);
            Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

            // Gram-Schmidt orthogonalize
            Vector3 tangent = sdir - planeNormal * Vector3.Dot(planeNormal, sdir);
            tangent.Normalize();

            float tangentdir = (Vector3.Dot(Vector3.Cross(planeNormal, sdir), tdir) >= 0.0f) ? 1.0f : -1.0f;
            Vector3 binormal = Vector3.Cross(planeNormal, tangent) * tangentdir;

            vert1.Tangent = tangent;
            vert1.Binormal = binormal;
            vert2.Tangent = tangent;
            vert2.Binormal = binormal;
            vert3.Tangent = tangent;
            vert3.Binormal = binormal;
        }

        #endregion

    }

}
