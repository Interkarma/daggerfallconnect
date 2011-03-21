// Project:         XNALibrary
// Description:     Simple XNA game library for DaggerfallConnect.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/using System;

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
    /// Helper class to load and store Daggerfall models for XNA.
    /// </summary>
    public class ModelManager
    {

        #region Class Variables

        private TextureManager textureManager;
        private GraphicsDevice graphicsDevice;
        private Arch3dFile arch3dFile;
        private Dictionary<int, Model> modelDict;

        #endregion

        #region Class Structures

        /// <summary>Defines mesh data and bounding box.</summary>
        public struct Model
        {
            public BoundingBox BoundingBox;
            public VertexPositionNormalTexture[] Vertices;
            public SubMeshData[] SubMeshes;
        }

        /// <summary>Defines submesh data.</summary>
        public struct SubMeshData
        {
            public int TextureKey;
            public short[] Indices;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the texture manager set at construction.
        /// </summary>
        TextureManager TextureManager
        {
            get { return textureManager; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public ModelManager(TextureManager textureManager)
        {
            // Setup
            graphicsDevice = textureManager.GraphicsDevice;
            this.textureManager = textureManager;
            string path = Path.Combine(textureManager.Arena2Path, "ARCH3D.BSA");
            arch3dFile = new Arch3dFile(path, FileUsage.UseDisk, true);
            modelDict = new Dictionary<int, Model>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load and convert a Daggerfall mesh.
        /// </summary>
        /// <param name="index">Index of mesh.</param>
        /// <returns>Model object.</returns>
        public Model LoadMesh(int index)
        {
            // Return model if already loaded
            if (modelDict.ContainsKey(index))
                return modelDict[index];

            // Load mesh if not already loaded
            Model model = LoadMeshData(index);
            modelDict.Add(index, model);
            return model;
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

            // Load mesh data. These methods could be combined to save on loops over the submeshes and vertices.
            // They have been split out here for clarity and to demonstrate each step in isolation.
            // Whatever buffer scheme you use, the same basic process for converting to XNA formats will be applied.
            LoadTextures(ref dfMesh);
            LoadVertices(ref dfMesh, ref model);
            LoadIndices(ref dfMesh, ref model);

            return model;
        }

        /// <summary>
        /// Loads all textures for this DFMesh.
        /// </summary>
        /// <param name="dfMesh">DFMesh object.</param>
        private void LoadTextures(ref DFMesh dfMesh)
        {
            // Loop through all submeshes
            foreach (DFMesh.DFSubMesh dfSubMesh in dfMesh.SubMeshes)
            {
                // TODO: Handle textures
                //textureManager.LoadTexture(dfSubMesh.TextureArchive, dfSubMesh.TextureRecord, 0);
            }
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
                // TODO: Get texture dimensions. This is required to normalise Daggerfall's texture coordinates
                //int textureKey = textureManager.GetTextureKey(dfSubMesh.TextureArchive, dfSubMesh.TextureRecord, 0);
                //Texture2D texture = textureManager.GetTexture(textureKey);

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
                        //mesh.Vertices[vertexCount].TextureCoordinate = new Vector2(dfPoint.U / texture.Width, dfPoint.V / texture.Height);
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
        /// <param name="model">Mesh object.</param>
        private void LoadIndices(ref DFMesh dfMesh, ref Model model)
        {
            // Allocate local submesh buffer
            model.SubMeshes = new SubMeshData[dfMesh.SubMeshes.Length];

            // Loop through all submeshes
            int subMeshCount = 0, vertexCount = 0;
            foreach (DFMesh.DFSubMesh dfSubMesh in dfMesh.SubMeshes)
            {
                // Allocate index buffer
                model.SubMeshes[subMeshCount].Indices = new short[dfSubMesh.TotalTriangles * 3];

                // TODO: Store texture key for this submesh
                //mesh.SubMeshes[subMeshCount].textureKey = textureManager.GetTextureKey(dfSubMesh.TextureArchive, dfSubMesh.TextureRecord, 0);

                // Loop through all planes in this submesh
                int indexCount = 0;
                foreach (DFMesh.DFPlane dfPlane in dfSubMesh.Planes)
                {
                    // Every DFPlane is a triangle fan radiating from point 0
                    short sharedPoint = (short)vertexCount++;

                    // Index remaining points. There are (plane.Points.Length - 2) triangles in every plane
                    for (int tri = 0; tri < dfPlane.Points.Length - 2; tri++)
                    {
                        // Store 3 points of current triangle.
                        // The second two indices are swapped so the winding order is correct after
                        // inverting Y and Z axes in LoadVertices().
                        model.SubMeshes[subMeshCount].Indices[indexCount++] = sharedPoint;
                        model.SubMeshes[subMeshCount].Indices[indexCount++] = (short)(vertexCount + 1);
                        model.SubMeshes[subMeshCount].Indices[indexCount++] = (short)(vertexCount);

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
