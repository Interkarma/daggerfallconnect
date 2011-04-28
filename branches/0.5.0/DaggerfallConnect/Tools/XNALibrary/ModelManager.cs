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
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace XNALibrary
{
    /// <summary>
    /// Helper class to load and store Daggerfall models for XNA. Does not load textures, but does calc UV coordinates.
    ///  This is to allow developers to apply their own texture logic independant of model loading.
    /// </summary>
    public class ModelManager
    {

        #region Class Variables

        string arena2Path = string.Empty;
        private GraphicsDevice graphicsDevice;
        private Arch3dFile arch3dFile;
        private Dictionary<uint, ModelData> modelDict;
        private bool cacheModels = true;

        #endregion

        #region Class Structures

        /// <summary>
        /// Defines mesh data and bounding box.
        /// </summary>
        public struct ModelData
        {
            /// <summary>Original geometry for picking native data.</summary>
            public DFMesh DFMesh;

            /// <summary>Axis-aligned bounding box of mesh data.</summary>
            public BoundingBox BoundingBox;

            /// <summary>Bounding sphere containing mesh data.</summary>
            public BoundingSphere BoundingSphere;

            /// <summary>Vertex array containing position, normal, and texture coordinates.</summary>
            public VertexPositionNormalTexture[] Vertices;

            /// <summary>Data for each SubMesh, grouped by texture.</summary>
            public SubMeshData[] SubMeshes;

            /// <summary>
            /// Defines submesh data.
            /// </summary>
            public struct SubMeshData
            {
                /// <summary>Texture archive index.</summary>
                public int TextureArchive;

                /// <summary>Texture record index.</summary>
                public int TextureRecord;

                /// <summary>User-defined texture key.</summary>
                public int TextureKey;

                /// <summary>Index array desribing the triangles of this SubMesh.</summary>
                public int[] Indices;
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
        ///  True will cache models when loaded.
        ///  False will not cache models when loaded.
        /// </summary>
        public bool CacheModels
        {
            get { return cacheModels; }
            set { cacheModels = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public ModelManager(GraphicsDevice device, string arena2Path)
        {
            // Setup
            graphicsDevice = device;
            arch3dFile = new Arch3dFile(Path.Combine(arena2Path, "ARCH3D.BSA"), FileUsage.UseDisk, true);
            this.arena2Path = arena2Path;
            modelDict = new Dictionary<uint, ModelData>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves ModelData from cache.
        ///  Will load model if not in cache, or if caching is disabled.
        /// </summary>
        /// <param name="key">ID of model.</param>
        /// <returns>Model object.</returns>
        public ModelData GetModel(uint key)
        {
            // Load model data
            ModelData modelData;
            LoadModelData(key, out modelData);
            return modelData;
        }

        /// <summary>
        /// Retrieves ModelData from cache.
        ///  Will load model if not in cache, or if caching is disabled.
        /// </summary>
        /// <param name="key">ID of model.</param>
        /// <param name="model">ModelData out.</param>
        /// <returns>True if successful.</returns>
        public bool GetModel(uint key, out ModelData model)
        {
            // Load model data
            return LoadModelData(key, out model);
        }

        /// <summary>
        /// Sets a model. Use a unique key to store as a different model
        ///  or existing key to overwrite a model.
        /// </summary>
        /// <param name="key">ID of model.</param>
        /// <param name="model">Model object.</param>
        public void SetModel(uint key, ref ModelData model)
        {
            // Do nothing if not caching models
            if (!cacheModels)
                return;

            // Add or overwrite cache
            if (modelDict.ContainsKey(key))
                modelDict[key] = model;
            else
                modelDict.Add(key, model);
        }

        /// <summary>
        /// Removes model from cache.
        /// </summary>
        /// <param name="key">ID of model.</param>
        public void RemoveModel(uint key)
        {
            if (cacheModels && modelDict.ContainsKey(key))
                modelDict.Remove(key);
        }

        /// <summary>
        /// Clear all models.
        /// </summary>
        public void ClearModels()
        {
            if (cacheModels)
                modelDict.Clear();
        }

        /// <summary>
        /// Transforms vertices and bounding box of model. Source model is unchanged.
        /// </summary>
        /// <param name="model">Source Model.</param>
        /// <param name="matrix">Matrix applied to output model.</param>
        /// <returns>Transformed Model.</returns>
        public ModelData TransformModel(ref ModelData model, Matrix matrix)
        {
            // Transform model
            int vertexPos = 0;
            ModelData transformedModel = model;
            foreach (var vertex in transformedModel.Vertices)
            {
                transformedModel.Vertices[vertexPos].Position = Vector3.Transform(vertex.Position, matrix);
                transformedModel.Vertices[vertexPos].Normal = vertex.Normal;
                transformedModel.Vertices[vertexPos].TextureCoordinate = vertex.TextureCoordinate;
                vertexPos++;
            }

            // Transform bounding box
            transformedModel.BoundingBox.Min = Vector3.Transform(transformedModel.BoundingBox.Min, matrix);
            transformedModel.BoundingBox.Max = Vector3.Transform(transformedModel.BoundingBox.Max, matrix);

            return transformedModel;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads model data from DFMesh.
        /// </summary>
        /// <param name="key">Key of source mesh.</param>
        /// <param name="model">ModelData out.</param>
        /// <returns>True if successful.</returns>
        private bool LoadModelData(uint key, out ModelData model)
        {
            // Return from cache if present
            if (cacheModels && modelDict.ContainsKey(key))
            {
                model = modelDict[key];
                return true;
            }

            // New model object
            model = new ModelData();

            // Find mesh index
            int index = arch3dFile.GetRecordIndex(key);
            if (index == -1)
                return false;

            // Get DFMesh
            DFMesh dfMesh = arch3dFile.GetMesh(index);
            if (dfMesh.TotalVertices == 0)
                return false;

            // Load mesh data
            model.DFMesh = dfMesh;
            LoadVertices(ref dfMesh, ref model);
            LoadIndices(ref dfMesh, ref model);

            // Add to cache
            if (cacheModels)
                modelDict.Add(key, model);

            return true;
        }

        /// <summary>
        /// Loads all vertices for this DFMesh.
        /// </summary>
        /// <param name="dfMesh">DFMesh source object.</param>
        /// <param name="model">Model object.</param>
        private void LoadVertices(ref DFMesh dfMesh, ref ModelData model)
        {
            // Allocate vertex buffer
            model.Vertices = new VertexPositionNormalTexture[dfMesh.TotalVertices];

            // Track min and max vectors for bounding box
            Vector3 min = new Vector3(0, 0, 0);
            Vector3 max = new Vector3(0, 0, 0);

            // Loop through all submeshes
            int vertexCount = 0;
            foreach (DFMesh.DFSubMesh dfSubMesh in dfMesh.SubMeshes)
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
        /// <param name="dfMesh">DFMesh source object.</param>
        /// <param name="model">Model object.</param>
        private void LoadIndices(ref DFMesh dfMesh, ref ModelData model)
        {
            // Allocate local submesh buffer
            model.SubMeshes = new ModelData.SubMeshData[dfMesh.SubMeshes.Length];

            // Loop through all submeshes
            int subMeshCount = 0, vertexCount = 0;
            foreach (DFMesh.DFSubMesh dfSubMesh in dfMesh.SubMeshes)
            {
                // Set texture indices
                model.SubMeshes[subMeshCount].TextureArchive = dfSubMesh.TextureArchive;
                model.SubMeshes[subMeshCount].TextureRecord = dfSubMesh.TextureRecord;
                model.SubMeshes[subMeshCount].TextureKey = -1;

                // Allocate index buffer
                model.SubMeshes[subMeshCount].Indices = new int[dfSubMesh.TotalTriangles * 3];

                // Loop through all planes in this submesh
                int indexCount = 0;
                foreach (DFMesh.DFPlane dfPlane in dfSubMesh.Planes)
                {
                    // Every DFPlane is a triangle fan radiating from point 0
                    int sharedPoint = vertexCount++;

                    // Index remaining points. There are (plane.Points.Length - 2) triangles in every plane
                    for (int tri = 0; tri < dfPlane.Points.Length - 2; tri++)
                    {
                        // Store 3 points of current triangle.
                        // The second two indices are swapped so the winding order is correct after
                        // inverting Y and Z axes in LoadVertices().
                        model.SubMeshes[subMeshCount].Indices[indexCount++] = sharedPoint;
                        model.SubMeshes[subMeshCount].Indices[indexCount++] = vertexCount + 1;
                        model.SubMeshes[subMeshCount].Indices[indexCount++] = vertexCount;

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

        #endregion

    }

}
