// Project:         XNALibrary
// Description:     Simple XNA game library for DaggerfallConnect.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Imports

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
        private Dictionary<int, Model> modelDict;

        #endregion

        #region Class Structures

        /// <summary>
        /// Defines mesh data and bounding box.
        /// </summary>
        public struct Model
        {
            /// <summary>Original geometry for picking and collision tests.</summary>
            public DFMesh DFMesh;

            /// <summary>Axis-aligned bounding box of mesh data.</summary>
            public BoundingBox BoundingBox;

            /// <summary>Vertex array containing position, normal, and texture coordinates.</summary>
            public VertexPositionNormalTexture[] Vertices;

            /// <summary>Data for each SubMesh, grouped by texture.</summary>
            public SubMeshData[] SubMeshes;
        }

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
            modelDict = new Dictionary<int, Model>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load and convert Daggerfall mesh data.
        /// </summary>
        /// <param name="key">ID of mesh.</param>
        /// <returns>True if successful.</returns>
        public bool LoadModel(int key)
        {
            // Return if already loaded
            if (modelDict.ContainsKey(key))
                return true;

            // Load mesh data
            int index = arch3dFile.GetRecordIndex((uint)key);
            if (index == -1)
                return false;

            // Convert model and store in dictionary
            Model model = LoadMeshData(index);
            modelDict.Add(key, model);

            return true;
        }

        /// <summary>
        /// Load and convert Daggerfall mesh data.
        /// </summary>
        /// <param name="key">ID of mesh.</param>
        /// <param name="modelOut">Model out.</param>
        /// <returns>True if successful.</returns>
        public bool LoadModel(int key, out Model modelOut)
        {
            // Return if already loaded
            if (modelDict.ContainsKey(key))
            {
                modelOut = modelDict[key];
                return true;
            }

            // Load mesh data
            if (LoadModel(key))
            {
                modelOut = modelDict[key];
                return true;
            }

            // Load failed
            modelOut = new Model();
            return false;
        }

        /// <summary>
        /// Gets a Daggerfall model. Will load model if not already loaded.
        /// </summary>
        /// <param name="key">ID of model.</param>
        /// <returns>Model object.</returns>
        public Model GetModel(int key)
        {
            // Return model if already loaded
            if (modelDict.ContainsKey(key))
                return modelDict[key];

            // Load model
            if (LoadModel(key))
                return modelDict[key];

            return new Model();
        }

        /// <summary>
        /// Gets a Daggerfall model. Will load model if not already loaded.
        ///  In this overload storing in ModelManager dictionary is optional.
        ///  Use this method to acquire models for an external caching scheme.
        /// </summary>
        /// <param name="key">ID of model.</param>
        /// <param name="addDictionary">True to store in ModelManager's dictionary. False to just return model.</param>
        /// <returns></returns>
        public Model GetModel(int key, bool addDictionary)
        {
            // Work as normal
            if (addDictionary)
                return GetModel(key);

            // Load mesh data
            int index = arch3dFile.GetRecordIndex((uint)key);
            if (index == -1)
                return new Model(); ;

            // Convert model and store in dictionary
            return LoadMeshData(index);
        }

        /// <summary>
        /// Sets a model. Use a unique key to store as a different model
        ///  or existing key to overwrite a model.
        /// </summary>
        /// <param name="key">ID of model.</param>
        /// <param name="model">Model object.</param>
        public void SetModel(int key, ref Model model)
        {
            if (modelDict.ContainsKey(key))
                modelDict[key] = model;
            else
                modelDict.Add(key, model);
        }

        /// <summary>
        /// Removes model.
        /// </summary>
        /// <param name="key">ID of model.</param>
        public void RemoveModel(int key)
        {
            if (modelDict.ContainsKey(key))
                modelDict.Remove(key);
        }

        /// <summary>
        /// Clear all models.
        /// </summary>
        public void ClearModels()
        {
            modelDict.Clear();
        }

        /// <summary>
        /// Transforms vertices and bounding box of model. Source model is unchanged.
        /// </summary>
        /// <param name="model">Source Model.</param>
        /// <param name="matrix">Matrix applied to output model.</param>
        /// <returns>Transformed Model.</returns>
        public Model TransformModel(ref Model model, Matrix matrix)
        {
            // Transform model
            int vertexPos = 0;
            Model transformedModel = model;
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
        /// Loads mesh data from DFMesh.
        /// </summary>
        /// <param name="dfMesh">DFMesh source.</param>
        /// <returns>Mesh object.</returns>
        private Model LoadMeshData(int index)
        {
            // Create new mesh
            Model model = new Model();
            DFMesh dfMesh = arch3dFile.GetMesh(index);

            // Load mesh data
            model.DFMesh = dfMesh;
            LoadVertices(ref dfMesh, ref model);
            LoadIndices(ref dfMesh, ref model);

            return model;
        }

        /// <summary>
        /// Loads all vertices for this DFMesh.
        /// </summary>
        /// <param name="dfMesh">DFMesh source object.</param>
        /// <param name="model">Model object.</param>
        private void LoadVertices(ref DFMesh dfMesh, ref Model model)
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
                int width = (IsPowerOfTwo(sz.Width)) ? sz.Width : NextPowerOfTwo(sz.Width);
                int height = (IsPowerOfTwo(sz.Height)) ? sz.Height : NextPowerOfTwo(sz.Height);
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
        }

        /// <summary>
        /// Build indices for this DFMesh.
        /// </summary>
        /// <param name="dfMesh">DFMesh source object.</param>
        /// <param name="model">Model object.</param>
        private void LoadIndices(ref DFMesh dfMesh, ref Model model)
        {
            // Allocate local submesh buffer
            model.SubMeshes = new SubMeshData[dfMesh.SubMeshes.Length];

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

        /// <summary>
        /// Check if value is a power of 2.
        /// </summary>
        /// <param name="x">Value to check.</param>
        /// <returns>True if power of 2.</returns>
        private bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }

        /// <summary>
        /// Finds next power of 2 size for value.
        /// </summary>
        /// <param name="x">Value.</param>
        /// <returns>Next power of 2.</returns>
        private int NextPowerOfTwo(int x)
        {
            int i = 1;
            while (i < x) { i <<= 1; }
            return i;
        }

        #endregion

    }

}
