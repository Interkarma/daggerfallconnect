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
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DeepEngine.Core;
using DeepEngine.Utility;
using DeepEngine.Player;
using DeepEngine.World;
#endregion

namespace DeepEngine.Components
{
    
    /// <summary>
    /// Static quad-tree terrain component.
    ///  This component is always centred on origin and cannot be transformed.
    ///  Parent entity transform is ignored.
    /// </summary>
    public class QuadTerrainComponent : DrawableComponent
    {
        #region Fields

        // Effects
        Effect terrainEffect;

        // Quadtree root
        QuadNode rootNode;
        int levels;
        float scale;
        
        // Map data
        float maxHeight;
        float normalStrength = 8f;
        int dimension;
        int leafDimension;
        Vector4[] terrainData;

        // Terrain textures
        Texture2D terrainVertexMap;
        Texture2D terrainBlendMap;

        // Grid
        Grid grid;

        #endregion

        #region Properties

        /// <summary>
        /// Terrain cannot be transformed.
        /// </summary>
        public new Matrix Matrix
        {
            get { return base.matrix; }
            set { base.matrix = base.Matrix; }
        }

        /// <summary>
        /// Gets or sets normal strength.
        /// </summary>
        public float NormalStrength
        {
            get { return normalStrength; }
            set { normalStrength = value; }
        }

        /// <summary>
        /// Gets dimension of data.
        /// </summary>
        public int Dimension
        {
            get { return dimension; }
        }

        /// <summary>
        /// Gets height and normal data as array of Vector4.
        /// </summary>
        public Vector4[] TerrainData
        {
            get { return terrainData; }
        }

        /// <summary>
        /// Gets root terrain node.
        /// </summary>
        public QuadNode Root
        {
            get { return rootNode; }
        }

        /// <summary>
        /// First diffuse texture.
        /// </summary>
        public Texture2D Diffuse1 { get; set; }

        /// <summary>
        /// Second diffuse texture.
        /// </summary>
        public Texture2D Diffuse2 { get; set; }

        /// <summary>
        /// Third diffuse texture.
        /// </summary>
        public Texture2D Diffuse3 { get; set; }

        /// <summary>
        /// Fourth diffuse texture.
        /// </summary>
        public Texture2D Diffuse4 { get; set; }

        /// <summary>
        /// Fifth diffuse texture.
        /// </summary>
        public Texture2D Diffuse5 { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="heightMap">Source heightmap. Must be POW2 grayscale image with equal width and height.</param>
        /// <param name="blendMap">Blend map. Must be POW2 image with equal width and height.</param>
        /// <param name="levels">Number of quad levels to divide heightmap into. Determines how many render calls are made per frame.</param>
        /// <param name="scale">Scale of each vertex. Determines final XZ size of each terrain node.</param>
        /// <param name="maxHeight">Maximum height of terrain.</param>
        public QuadTerrainComponent(DeepCore core, Texture2D heightMap, Texture2D blendMap, int levels, float scale, float maxHeight)
            : base(core)
        {
            // Ensure heightmap image is POW2 with equal dimensions
            if (heightMap.Width != heightMap.Height ||
                !PowerOfTwo.IsPowerOfTwo(heightMap.Width))
            {
                throw new Exception("Heightmap must be POW2 and width equal to height.");
            }

            // Ensure blendmap image is POW2 with equal dimensions
            if (blendMap.Width != blendMap.Height ||
                !PowerOfTwo.IsPowerOfTwo(blendMap.Width))
            {
                throw new Exception("Blendmap must be POW2 and width equal to height.");
            }
            
            // Store values
            this.dimension = heightMap.Width;
            this.levels = levels;
            this.maxHeight = maxHeight / scale;
            this.scale = scale;
            this.terrainBlendMap = blendMap;

            // Create arrays
            terrainData = new Vector4[dimension * dimension];

            // Create grid
            leafDimension = PowerOfTwo.MipMapSize(dimension, levels);
            grid = new Grid(core.GraphicsDevice, leafDimension, 1.0f);

            // Initialise map data
            SetHeightData(heightMap);
            UpdateNormalData();
            UpdateTerrainVertexTexture();

            // Initialise quad tree
            BuildQuadTree();

            // Load effects
            terrainEffect = core.ContentManager.Load<Effect>("Effects/RenderTerrain");
        }

        #endregion

        #region DrawableComponents Overrides

        /// <summary>
        /// Draws component.
        /// </summary>
        /// <param name="caller">Entity calling the draw operation.</param>
        public override void Draw(BaseEntity caller)
        {
            // Do nothing if disabled
            if (!enabled)
                return;

            // Draw quad tree
            DrawNode(rootNode, caller);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets value from height data.
        ///  Coordinates are clamped to within valid range.
        /// </summary>
        /// <param name="x">X coord.</param>
        /// <param name="y">Y coord.</param>
        /// <returns>Height value.</returns>
        public float GetHeight(int x, int y)
        {
            if (terrainData == null)
                return 0;

            // Clamp X & Y
            if (x < 0) x = 0; else if (x >= dimension) x = dimension - 1;
            if (y < 0) y = 0; else if (y >= dimension) y = dimension - 1;

            // Get position
            int pos = y * dimension + x;

            return terrainData[pos].W;
        }

        /// <summary>
        /// Updates normal data based on current height data.
        /// </summary>
        public void UpdateNormalData()
        {
            if (terrainData == null)
                return;

            // Compute normals
            for (int y = 0; y < dimension; y++)
            {
                for (int x = 0; x < dimension; x++)
                {
                    // Look up the heights to either side of this pixel
                    float left = GetHeight(x - 1, y);
                    float right = GetHeight(x + 1, y);
                    float top = GetHeight(x, y - 1);
                    float bottom = GetHeight(x, y + 1);

                    // Compute gradient vectors, then cross them to get the normal
                    Vector3 dx = new Vector3(1, 0, (right - left) * normalStrength);
                    Vector3 dy = new Vector3(0, 1, (bottom - top) * normalStrength);
                    Vector3 normal = Vector3.Cross(dx, dy);
                    normal.Normalize();

                    // Store result
                    int pos = y * dimension + x;
                    terrainData[pos].X = normal.X;
                    terrainData[pos].Y = normal.Z;
                    terrainData[pos].Z = normal.Y;
                }
            }
        }

        /// <summary>
        /// Updates terrain vertex texture based on current height and normal data.
        /// </summary>
        public void UpdateTerrainVertexTexture()
        {
            if (terrainData == null)
                return;

            // Create destination texture
            terrainVertexMap = new Texture2D(
                core.GraphicsDevice,
                dimension,
                dimension,
                false,
                SurfaceFormat.Vector4);
            terrainVertexMap.SetData<Vector4>(terrainData);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Unpacks height texture to local arrays.
        /// </summary>
        /// <param name="heightMap">Source height map.</param>
        private void SetHeightData(Texture2D heightMap)
        {
            // Get color data from heightmap
            Color[] srcData = new Color[heightMap.Width * heightMap.Height];
            heightMap.GetData<Color>(srcData);

            // Compute heights
            for (int y = 0; y < heightMap.Height; y++)
            {
                for (int x = 0; x < heightMap.Width; x++)
                {
                    int pos = y * heightMap.Width + x;
                    terrainData[pos].W = ((srcData[pos].R + srcData[pos].G + srcData[pos].B) / 3) / 255.0f;
                }
            }

            // Set bounding sphere
            float diameter = (maxHeight > dimension * scale) ? maxHeight : dimension * scale;
            this.boundingSphere = new BoundingSphere(
                new Vector3(diameter / 2, 0, diameter / 2),
                diameter * 0.70f);

            // Centre terrain bounds on origin
            this.boundingSphere.Center -= new Vector3(diameter / 2, 0, diameter / 2);
        }

        /// <summary>
        /// Builds quadtree.
        /// </summary>
        private void BuildQuadTree()
        {
            // Create parent node
            rootNode = new QuadNode(dimension, scale, maxHeight);
            AddQuadChildren(rootNode);
        }

        /// <summary>
        /// Adds quad node children to tree.
        /// </summary>
        /// <param name="parent">Parent node.</param>
        private void AddQuadChildren(QuadNode parent)
        {
            // Add children
            if (parent.Level < levels)
            {
                // North-West
                QuadNode nw = new QuadNode(parent, QuadNode.Quadrants.NW);
                AddQuadChildren(nw);

                // North-East
                QuadNode ne = new QuadNode(parent, QuadNode.Quadrants.NE);
                AddQuadChildren(ne);

                // South-West
                QuadNode sw = new QuadNode(parent, QuadNode.Quadrants.SW);
                AddQuadChildren(sw);

                // South-East
                QuadNode se = new QuadNode(parent, QuadNode.Quadrants.SE);
                AddQuadChildren(se);
            }
        }

        #endregion

        #region Drawing Methods

        /// <summary>
        /// Draw this node and all child nodes.
        /// </summary>
        /// <param name="node">Node to draw.</param>
        private void DrawNode(QuadNode node, BaseEntity caller)
        {
            if (node == null)
                return;

            // Test node against frustum
            if (!node.BoundingBox.Intersects(core.ActiveScene.Camera.BoundingFrustum))
                return;

            // Recurse children
            foreach (QuadNode child in node.Children)
                DrawNode(child, caller);

            // Calculate sample scale
            Vector2 sampleScale;
            sampleScale.X = (float)leafDimension / (float)dimension;
            sampleScale.Y = sampleScale.X;

            // Only draw terrain grid for leaf nodes
            if (!node.HasChildren)
            {
                // Initialise effect
                terrainEffect.Parameters["View"].SetValue(core.ActiveScene.Camera.ViewMatrix);
                terrainEffect.Parameters["Projection"].SetValue(core.ActiveScene.Camera.ProjectionMatrix);
                terrainEffect.Parameters["VertexTexture"].SetValue(terrainVertexMap);
                terrainEffect.Parameters["BlendTexture"].SetValue(terrainBlendMap);
                terrainEffect.Parameters["Diffuse1Texture"].SetValue(Diffuse1);
                terrainEffect.Parameters["Diffuse2Texture"].SetValue(Diffuse2);
                terrainEffect.Parameters["Diffuse3Texture"].SetValue(Diffuse3);
                terrainEffect.Parameters["Diffuse4Texture"].SetValue(Diffuse4);
                terrainEffect.Parameters["Diffuse5Texture"].SetValue(Diffuse5);
                terrainEffect.Parameters["MaxHeight"].SetValue(maxHeight);
                terrainEffect.Parameters["SampleScale"].SetValue(sampleScale);

                // Calculate sample offset
                Vector2 sampleOffset;
                sampleOffset.X = node.X / dimension;
                sampleOffset.Y = node.Y / dimension;

                // Set effect textures for this quad
                terrainEffect.Parameters["SampleOffset"].SetValue(sampleOffset);
                terrainEffect.Parameters["World"].SetValue(node.Matrix);

                // Apply effect
                terrainEffect.Techniques[0].Passes[0].Apply();

                // Draw grid
                core.GraphicsDevice.SetVertexBuffer(grid.VertexBuffer);
                core.GraphicsDevice.Indices = grid.IndexBuffer;
                core.GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    0,
                    grid.VertexBuffer.VertexCount,
                    0,
                    grid.IndexBuffer.IndexCount / 3);
            }
        }

        #endregion

    }

}
