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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace XNALibrary.DFResources
{

    /// <summary>
    /// Wraps vertex and index data, and provides some helper functions.
    /// </summary>
    class Model
    {

        #region Class Variables

        private MeshData meshData;
        private BoundingBox boundingBox;

        #endregion

        #region Class Structures

        /// <summary>Defines a mesh.</summary>
        public struct MeshData
        {
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
        /// Gets or sets vertex array.
        /// </summary>
        VertexPositionNormalTexture[] Vertices
        {
            get { return meshData.Vertices; }
            set 
            {
                meshData.Vertices = value;
                CreateBoundingBox();
            }
        }

        /// <summary>
        /// Gets or sets submesh array.
        /// </summary>
        SubMeshData[] SubMeshes
        {
            get { return meshData.SubMeshes; }
            set { meshData.SubMeshes = value; }
        }

        /// <summary>
        /// Gets BoundingBox of model.
        /// </summary>
        BoundingBox BoundingBox
        {
            get { return BoundingBox; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Model()
        {
            Clear();
        }

        /// <summary>
        /// Assignment constructor for existing vertex and submesh data.
        /// </summary>
        /// <param name="vertexData">VertexPositionNormalTexture array.</param>
        /// <param name="subMeshData">SubMeshData array.</param>
        public Model(VertexPositionNormalTexture[] vertexData, SubMeshData[] subMeshData)
        {
            // Assign data
            Clear();
            meshData.Vertices = vertexData;
            meshData.SubMeshes = subMeshData;
            CreateBoundingBox();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Clears all mesh data.
        /// </summary>
        public void Clear()
        {
            meshData = new MeshData();
            boundingBox = new BoundingBox();
        }

        public SubMeshData GetSubMeshData(int index)
        {
            // Validate
            if (index < 0 || index > meshData.SubMeshes.Length)
                throw new Exception("Index out of bounds.");

            return meshData.SubMeshes[index];
        }

        #endregion

        #region Private Methods

        private void CreateBoundingBox()
        {
            // Clear bounding box if no vertex data
            if (meshData.Vertices == null)
            {
                boundingBox = new BoundingBox();
                return;
            }

            // Create Vector3 array from vertices
            int index = 0;
            Vector3[] points = new Vector3[meshData.Vertices.Length];
            foreach (var vertex in meshData.Vertices)
                points[index++] = vertex.Position;
            
            // Create bounding box
            boundingBox = BoundingBox.CreateFromPoints(points);
        }

        #endregion

    }

}
