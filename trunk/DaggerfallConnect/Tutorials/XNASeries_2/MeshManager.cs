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

namespace XNASeries_2
{
    /// <summary>
    /// Helper class to load and draw Daggerfall meshes for XNA.
    ///  This class uses a dictionary for converted meshes so the same data is not converted more than once.
    /// </summary>
    public class MeshManager
    {
        #region Class Variables

        private GraphicsDevice graphicsDevice;
        private TextureManager textureManager;
        private Arch3dFile arch3dFile = new Arch3dFile();
        private Dictionary<int, Mesh> meshDictionary;

        #endregion

        #region Class Structures

        /// <summary>Stores mesh data.</summary>
        public struct Mesh
        {
            public VertexPositionNormalTexture[] vertices;
            public SubMesh[] submeshes;
        }

        /// <summary>Stores submesh data for an individual mesh.</summary>
        public struct SubMesh
        {
            public int textureKey;
            public short[] indices;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// TextureManager object.
        /// </summary>
        public TextureManager TextureManager
        {
            get { return textureManager; }
        }

        /// <summary>
        /// Arch3DFile object.
        /// </summary>
        public Arch3dFile Arch3dFile
        {
            get { return arch3dFile; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MeshManager(GraphicsDevice device, string arena2Path)
        {
            textureManager = new TextureManager(device, arena2Path);
            arch3dFile.Load(Path.Combine(arena2Path, "ARCH3D.BSA"), FileUsage.UseDisk, true);
            meshDictionary = new Dictionary<int, Mesh>();
            graphicsDevice = device;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load and convert a Daggerfall mesh.
        /// </summary>
        /// <param name="index">Index of mesh.</param>
        /// <returns>Mesh object.</returns>
        public Mesh LoadMesh(int index)
        {
            if (!meshDictionary.ContainsKey(index))
            {
                // Load mesh if not already loaded
                Mesh mesh = LoadMeshData(index);
                meshDictionary.Add(index, mesh);
                return mesh;
            }
            else
            {
                return meshDictionary[index];
            }
        }

        /// <summary>
        /// Draws the loaded mesh using the BasicEffect specifed.
        /// </summary>
        /// <param name="index">Index of mesh to draw.</param>
        /// <param name="effect">BasicEffect to use for rendering.</param>
        public void DrawMesh(int index, ref BasicEffect effect)
        {
            Mesh mesh = meshDictionary[index];
            foreach (var submesh in mesh.submeshes)
            {
                effect.Texture = textureManager.GetTexture(submesh.textureKey);

                effect.Begin();
                effect.CurrentTechnique.Passes[0].Begin();

                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                    mesh.vertices, 0, mesh.vertices.Length,
                    submesh.indices, 0, submesh.indices.Length / 3);

                effect.CurrentTechnique.Passes[0].End();
                effect.End();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads mesh data from DFMesh.
        /// </summary>
        /// <param name="dfMesh">DFMesh source.</param>
        /// <returns>Mesh object.</returns>
        private Mesh LoadMeshData(int index)
        {
            // Create new mesh
            Mesh mesh = new Mesh();
            DFMesh dfMesh = arch3dFile.GetMesh(index);

            // Load mesh data. These methods could be combined to save on loops over the submeshes and vertices.
            // They have been split out here for clarity and to demonstrate each step in isolation.
            // Whatever buffer scheme you use, the same basic process for converting to XNA formats will be applied.
            LoadTextures(ref dfMesh);
            LoadVertices(ref dfMesh, ref mesh);
            LoadIndices(ref dfMesh, ref mesh);

            return mesh;
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
                textureManager.LoadTexture(dfSubMesh.TextureArchive, dfSubMesh.TextureRecord, 0);
            }
        }

        /// <summary>
        /// Loads all vertices for this DFMesh.
        /// </summary>
        /// <param name="dfMesh">DFMesh source object.</param>
        /// <param name="mesh">Mesh destination object.</param>
        private void LoadVertices(ref DFMesh dfMesh, ref Mesh mesh)
        {
            // Allocate vertex buffer
            mesh.vertices = new VertexPositionNormalTexture[dfMesh.TotalVertices];

            // Loop through all submeshes
            int vertexCount = 0;
            foreach (DFMesh.DFSubMesh dfSubMesh in dfMesh.SubMeshes)
            {
                // Get texture dimensions. This is required to normalise Daggerfall's texture coordinates
                int textureKey = textureManager.GetTextureKey(dfSubMesh.TextureArchive, dfSubMesh.TextureRecord, 0);
                Texture2D texture = textureManager.GetTexture(textureKey);

                // Loop through all planes in this submesh
                foreach (DFMesh.DFPlane dfPlane in dfSubMesh.Planes)
                {
                    // Copy each point in this plane to vertex buffer
                    foreach (DFMesh.DFPoint dfPoint in dfPlane.Points)
                    {
                        // Daggerfall uses a different axis layout than XNA.
                        // The X and Y axes should be inverted so the model is displayed correctly.
                        // This also requires a change to winding order in LoadIndices().
                        mesh.vertices[vertexCount].Position = new Vector3(-dfPoint.X, -dfPoint.Y, dfPoint.Z);
                        mesh.vertices[vertexCount].Normal = new Vector3(-dfPoint.NX, -dfPoint.NY, dfPoint.NZ);
                        mesh.vertices[vertexCount].TextureCoordinate = new Vector2(dfPoint.U / texture.Width, dfPoint.V / texture.Height);
                        vertexCount++;
                    }
                }
            }
        }

        /// <summary>
        /// Build indices for this DFMesh.
        /// </summary>
        /// <param name="dfMesh">DFMesh source object.</param>
        /// <param name="mesh">Mesh destination object.</param>
        private void LoadIndices(ref DFMesh dfMesh, ref Mesh mesh)
        {
            // Allocate local submesh buffer
            mesh.submeshes = new SubMesh[dfMesh.SubMeshes.Length];

            // Loop through all submeshes
            int subMeshCount = 0, vertexCount = 0;
            foreach (DFMesh.DFSubMesh dfSubMesh in dfMesh.SubMeshes)
            {
                // Allocate index buffer
                mesh.submeshes[subMeshCount].indices = new short[dfSubMesh.TotalTriangles * 3];

                // Store texture key for this submesh
                mesh.submeshes[subMeshCount].textureKey = textureManager.GetTextureKey(dfSubMesh.TextureArchive, dfSubMesh.TextureRecord, 0);

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
                        // inverting X and Y axes in LoadVertices().
                        mesh.submeshes[subMeshCount].indices[indexCount++] = sharedPoint;
                        mesh.submeshes[subMeshCount].indices[indexCount++] = (short)(vertexCount + 1);
                        mesh.submeshes[subMeshCount].indices[indexCount++] = (short)(vertexCount);

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
