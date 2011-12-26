// Project:         DaggerfallPipeline
// Description:     Load Daggerfall content using the XNA Content Pipeline
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace DaggerfallPipeline
{
    [ContentImporter(".dfm", DisplayName = "Daggerfall - Model Importer", DefaultProcessor = "ModelProcessor")]
    public class DFMeshImporter : ContentImporter<NodeContent>
    {
        #region Variables

        private const string Arena2PathTxt = "Arena2Path.txt";
        private const string textureSubPath = "Textures";

        private string arena2Path;
        private string texturesPath;

        #endregion

        #region Variables

        // Source model data read from Daggerfall
        ModelData model;

        // The root NodeContent of our model
        private NodeContent rootNode;

        // The current mesh being constructed
        private MeshBuilder meshBuilder;

        // Indices of vertex channels for the current mesh
        private int textureCoordinateDataIndex;
        //private int normalDataIndex;

        #endregion

        public override NodeContent Import(string filename, ContentImporterContext context)
        {
            // Load Arena2Path.txt
            arena2Path = File.ReadAllText(
                Path.Combine(Path.GetDirectoryName(filename), Arena2PathTxt));

            // Reset all importer state
            rootNode = new NodeContent();
            meshBuilder = null;

            // Model identity is tied to the file it is loaded from
            rootNode.Identity = new ContentIdentity(filename);

            // Read input text
            string input = File.ReadAllText(filename);

            // Remove new lines
            input = input.Replace('\n', ' ').Trim();
            input = input.Replace('\r', ' ').Trim();

            // Get source information from file
            string[] lines = input.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            uint id = Convert.ToUInt32(lines[0].Trim());
            //string terrain = lines[1].Trim();
            //string weather = lines[2].Trim();
            Double scale = Convert.ToDouble(lines[1].Trim());

            // Load source model
            LoadModelData(id, (float)scale);

            // Export texture definitions
            texturesPath = Path.Combine(Path.GetDirectoryName(filename), textureSubPath);
            SaveTextures(model, Path.GetDirectoryName(filename), texturesPath);

            // Build each submesh group
            for (int sm = 0; sm < model.SubMeshes.Length; sm++)
            {
                StartMesh(sm);
                AddMaterial(sm, context);
                AddVerticesInformation(sm);
                FinishMesh();
            }

            return rootNode;
        }

        #region NodeContent Building

        /// <summary>
        /// Starts a new mesh and fills it with mesh mapped positions.
        /// </summary>
        /// <param name="subMesh">Index of submesh in source.</param>
        private void StartMesh(int subMesh)
        {
            string meshName = string.Format("submesh_{0}", subMesh);
            meshBuilder = MeshBuilder.StartMesh(meshName);

            // Add additional vertex channels for texture coordinates and normals
            textureCoordinateDataIndex = meshBuilder.CreateVertexChannel<Vector2>(
                VertexChannelNames.TextureCoordinate(0));
            //normalDataIndex =
            //    meshBuilder.CreateVertexChannel<Vector3>(VertexChannelNames.Normal());

            // Add each position to this mesh with CreatePosition
            int start = model.SubMeshes[subMesh].StartIndex;
            for (int i = 0; i < model.SubMeshes[subMesh].PrimitiveCount * 3; i++)
            {
                int index = model.Indices[start + i];
                int pos = meshBuilder.CreatePosition(model.Vertices[index].Position);
            }
        }

        /// <summary>
        /// Add vertex information for this mesh.
        /// </summary>
        /// <param name="subMesh">Index of submesh in source.</param>
        private void AddVerticesInformation(int subMesh)
        {
            int start = model.SubMeshes[subMesh].StartIndex;
            for (int i = 0; i < model.SubMeshes[subMesh].PrimitiveCount * 3; i++)
            {
                int index = model.Indices[start + i];
                //meshBuilder.SetVertexChannelData(normalDataIndex, model.Vertices[index].Normal);
                meshBuilder.SetVertexChannelData(textureCoordinateDataIndex, model.Vertices[index].TextureCoordinate);
                meshBuilder.AddTriangleVertex(i);
            }
        }

        /// <summary>
        /// Finishes building a mesh and adds the resulting MeshContent or
        /// NodeContent to the root model's NodeContent.
        /// </summary>
        private void FinishMesh()
        {
            MeshContent meshContent = meshBuilder.FinishMesh();

            // Add the mesh to the model
            rootNode.Children.Add(meshContent);

            meshBuilder = null;
        }

        #endregion

        #region Material Building

        /// <summary>
        /// Adds material to mesh being built.
        /// </summary>
        /// <param name="subMesh">Index of submesh in source.</param>
        private void AddMaterial(int subMesh, ContentImporterContext context)
        {
            // Get submesh
            ModelData.SubMeshData sm = model.SubMeshes[subMesh];

            // Get relative path to texture
            string relativePath = Path.Combine(textureSubPath, sm.MaterialKey) + ".dfb";

            // Create material
            BasicMaterialContent material = new BasicMaterialContent();
            material.Name = sm.MaterialKey;
            material.Texture = new ExternalReference<TextureContent>(relativePath, rootNode.Identity);

            // Add dependency to texture
            //context.AddDependency(material.Texture.Filename);

            // Assign material
            meshBuilder.SetMaterial(material);
        }

        /// <summary>
        /// Gets a unique key for each texture.
        /// </summary>
        /// <param name="archive">Texture archive index.</param>
        /// <param name="record">Texture record index.</param>
        private string GetTextureKey(int archive, int record)
        {
            return string.Format("texture_{0}-{1}", archive, record);
        }

        /// <summary>
        /// Saves texture definitions.
        /// </summary>
        /// <param name="model">Model to save textures from.</param>
        /// <param name="path">Output path.</param>
        private void SaveTextures(ModelData model, string modelPath, string texturePath)
        {
            // Ensure folder exists
            if (!Directory.Exists(texturePath))
                Directory.CreateDirectory(texturePath);

            // Copy Arena2Path.txt
            File.Copy(
                Path.Combine(modelPath, Arena2PathTxt),
                Path.Combine(texturePath, Arena2PathTxt),
                true);

            // Export all textures from model
            foreach (var submesh in model.SubMeshes)
            {
                // Get filename
                string filename = Path.Combine(texturePath, submesh.MaterialKey) + ".dfb";
                
                // Build texture definition
                string texture = string.Format(
                    "TEXTURE.{0:000},{1},0",
                    submesh.TextureArchive,
                    submesh.TextureRecord);

                // Export texture
                if (!File.Exists(filename))
                {
                    File.WriteAllText(filename, texture);
                }
            }
        }

        #endregion

        #region Mesh Loading

        /// <summary>
        /// Loads model data for conversion.
        /// </summary>
        /// <param name="id">Model ID.</param>
        /// <param name="scale">Amount to scale model.</param>
        private void LoadModelData(uint id, float scale)
        {
            // Open ARCH3D.BSA
            Arch3dFile arch3dFile = new Arch3dFile(
                Path.Combine(arena2Path, "ARCH3D.BSA"),
                FileUsage.UseDisk,
                true);

            // Get DFMesh
            int record = arch3dFile.GetRecordIndex(id);
            DFMesh dfMesh = arch3dFile.GetMesh(record);

            // Scale DFMesh
            if (scale != 1.0f)
            {
                foreach (var mesh in dfMesh.SubMeshes)
                {
                    foreach (var plane in mesh.Planes)
                    {
                        for (int point = 0; point < plane.Points.Length; point++)
                        {
                            plane.Points[point].X *= scale;
                            plane.Points[point].Y *= scale;
                            plane.Points[point].Z *= scale;
                        }
                    }
                }
            }

            // Load model data
            model = new ModelData();
            model.DFMesh = dfMesh;
            LoadVertices(ref model);
            LoadIndices(ref model);
        }

        /// <summary>
        /// Loads all vertices for this DFMesh.
        /// </summary>
        /// <param name="model">Model object.</param>
        private void LoadVertices(ref ModelData model)
        {
            // Allocate vertex buffer
            model.Vertices = new VertexPositionNormalTexture[model.DFMesh.TotalVertices];

            // Loop through all submeshes
            int vertexCount = 0;
            foreach (DFMesh.DFSubMesh dfSubMesh in model.DFMesh.SubMeshes)
            {
                // Get texture dimensions for this submesh
                string archivePath = Path.Combine(arena2Path, TextureFile.IndexToFileName(dfSubMesh.TextureArchive));
                System.Drawing.Size sz = TextureFile.QuickSize(archivePath, dfSubMesh.TextureRecord);

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
                            (dfPoint.U / sz.Width),
                            (dfPoint.V / sz.Height));

                        // Inrement vertex count
                        vertexCount++;
                    }
                }
            }
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
                // Set texture indices for this submesh
                model.SubMeshes[subMeshCount].TextureArchive = dfSubMesh.TextureArchive;
                model.SubMeshes[subMeshCount].TextureRecord = dfSubMesh.TextureRecord;

                // Set material key for this submesh
                model.SubMeshes[subMeshCount].MaterialKey = GetTextureKey(
                    dfSubMesh.TextureArchive,
                    dfSubMesh.TextureRecord);

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

        #endregion

        #region ModelData

        /// <summary>
        /// Defines model data.
        /// </summary>
        private class ModelData
        {
            /// <summary>Native geometry as read from ARCH3D.BSA.</summary>
            public DFMesh DFMesh;

            /// <summary>Vertex array containing position, normal, texture coordinates, tangent, bitangent.</summary>
            public VertexPositionNormalTexture[] Vertices;

            /// <summary>Index array desribing the triangles of this mesh.</summary>
            public short[] Indices;

            /// <summary>Data for each SubMesh, grouped by texture.</summary>
            public SubMeshData[] SubMeshes;

            public struct SubMeshData
            {
                /// <summary>Texture archive index.</summary>
                public int TextureArchive;

                /// <summary>Texture record index.</summary>
                public int TextureRecord;

                /// <summary>Filename of material for this submesh.</summary>
                public string MaterialKey;

                /// <summary>Location in the index array at which to start reading vertices.</summary>
                public int StartIndex;

                /// <summary>Number of primitives in this submesh.</summary>
                public int PrimitiveCount;
            }
        }

        #endregion
    }
}
