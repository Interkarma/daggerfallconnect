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
using DeepEngine.Daggerfall;
#endregion

namespace DeepEngine.Components
{
    
    /// <summary>
    /// Terrain component using nested axis-aligned boxes in a quad-tree for visibility culling.
    ///  Can be translated and scaled using the component matrix, but rotations will be ignored.
    ///  Inherits translations from parent entity, but ignores entity rotation and scale.
    ///  This component is best placed in its own static entity.
    /// </summary>
    public class QuadTerrainComponent : DrawableComponent
    {
        #region Fields

        // Effects
        Effect terrainEffect;

        // Quadtree root
        QuadNode rootNode;
        int levels;
        
        // Map data
        float maxHeight = 256f;
        float normalStrength = 8f;
        int dimension;
        int leafDimension;
        Vector4[] terrainData;

        // Terrain textures
        Texture2D terrainVertexMap;
        Texture2D terrainBlendMap;

        // Grid
        Grid grid;

        // Picking
        bool enablePicking = false;
        const int defaultIntersectionCapacity = 35;
        List<Intersection.ObjectIntersection<QuadNode>> pointerNodeIntersections;
        TerrainIntersectionData pointerIntersection;

        #endregion

        #region Class Structures

        /// <summary>
        /// Information about where an intersection has occurred on the terrain.
        /// </summary>
        public struct TerrainIntersectionData
        {
            /// <summary>Distance to intersection point in world. Null if no collision.</summary>
            public float? Distance;
            /// <summary>Intersection point in world.</summary>
            public Vector3 WorldPosition;
            /// <summary>Intersection point in heightmap.</summary>
            public Vector2 MapPosition;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets matrix for terrain component.
        ///  Rotations will be ignored.
        /// </summary>
        public new Matrix Matrix
        {
            get { return base.matrix; }
            set { SetMatrix(value); }
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
        /// Gets or sets maximum height.
        /// </summary>
        public float MaxHeight
        {
            get { return maxHeight; }
            set { maxHeight = value; }
        }

        /// <summary>
        /// Gets or sets flag to enable or disable mouse picking.
        ///  Application must keep pointer ray updated in renderer.
        /// </summary>
        public bool EnablePicking
        {
            get { return enablePicking; }
            set { enablePicking = value; }
        }

        /// <summary>
        /// Gets current pointer intersection.
        /// </summary>
        public TerrainIntersectionData PointerIntersection
        {
            get { return pointerIntersection; }
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
        /// <param name="levels">Number of quad levels to divide heightmap into. Use as few levels as possible.</param>
        public QuadTerrainComponent(DeepCore core, Texture2D heightMap, Texture2D blendMap, int levels)
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
            this.terrainBlendMap = blendMap;

            // Set default textures
            Diffuse1 = core.MaterialManager.CreateDaggerfallMaterialEffect(302, 1, null, MaterialManager.DefaultTerrainFlags).DiffuseTexture;
            Diffuse2 = core.MaterialManager.CreateDaggerfallMaterialEffect(402, 1, null, MaterialManager.DefaultTerrainFlags).DiffuseTexture;
            Diffuse3 = core.MaterialManager.CreateDaggerfallMaterialEffect(302, 2, null, MaterialManager.DefaultTerrainFlags).DiffuseTexture;
            Diffuse4 = core.MaterialManager.CreateDaggerfallMaterialEffect(402, 2, null, MaterialManager.DefaultTerrainFlags).DiffuseTexture;
            Diffuse5 = core.MaterialManager.CreateDaggerfallMaterialEffect(302, 3, null, MaterialManager.DefaultTerrainFlags).DiffuseTexture;

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

            // Create intersections list
            pointerNodeIntersections = new List<Intersection.ObjectIntersection<QuadNode>>();
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

            if (enablePicking)
            {
                // Reset intersections
                pointerNodeIntersections.Clear();
                pointerNodeIntersections.Capacity = defaultIntersectionCapacity;

                // Draw quad tree
                DrawNode(rootNode, caller);

                // Test intersections
                TestPointerIntersections();
            }
            else
            {
                // Draw quad tree
                DrawNode(rootNode, caller);
            }
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
        /// Unpacks height texture to local arrays and sets course bounding sphere.
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
            float diameter = (maxHeight > dimension) ? maxHeight : dimension;
            this.boundingSphere = new BoundingSphere(
                new Vector3(diameter / 2, 0, diameter / 2),
                diameter * 0.70f);
        }

        /// <summary>
        /// Builds quadtree.
        /// </summary>
        private void BuildQuadTree()
        {
            // Create parent node
            rootNode = new QuadNode(dimension, maxHeight);
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

        /// <summary>
        /// Updates matrix with valid transforms.
        /// </summary>
        /// <param name="matrix">Transformation matrix.</param>
        private void SetMatrix(Matrix matrix)
        {
            // Decompose matrix
            Vector3 scale;
            Quaternion rotation;
            Vector3 translation;
            matrix.Decompose(out scale, out rotation, out translation);

            // Create new matrix ignoring rotation
            this.matrix = Matrix.CreateScale(scale) * Matrix.CreateTranslation(translation);
        }

        #endregion

        #region Drawing Methods

        /// <summary>
        /// Draw this node and all child nodes.
        /// </summary>
        /// <param name="node">Node to draw.</param>
        /// <param name="caller">Entity calling the draw operation.</param>
        private void DrawNode(QuadNode node, BaseEntity caller)
        {
            if (node == null)
                return;

            // Calculate world matrix
            node.WorldMatrix = node.Matrix * this.matrix * Matrix.CreateTranslation(caller.Matrix.Translation);

            // Transform bounds for this node
            BoundingBox bounds;
            bounds.Min = Vector3.Transform(node.BoundingBox.Min, node.WorldMatrix);
            bounds.Max = Vector3.Transform(node.BoundingBox.Max, node.WorldMatrix);

            // Test node against frustum
            if (!bounds.Intersects(core.ActiveScene.Camera.BoundingFrustum))
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
                if (enablePicking)
                {
                    // Test for node-ray intersection
                    float? intersectDistance = core.Renderer.PointerRay.Intersects(bounds);
                    if (intersectDistance != null)
                    {
                        // Add to intersection list
                        Intersection.ObjectIntersection<QuadNode> ni =
                            new Intersection.ObjectIntersection<QuadNode>(intersectDistance, node);
                        pointerNodeIntersections.Add(ni);
                    }
                }

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
                terrainEffect.Parameters["World"].SetValue(node.WorldMatrix);

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

        #region Intersections

        /// <summary>
        /// Tests pointer intersections against elevation data.
        /// </summary>
        private void TestPointerIntersections()
        {
            if (pointerNodeIntersections.Count == 0)
            {
                // Clear previous intersection
                pointerIntersection.Distance = null;
                pointerIntersection.MapPosition = Vector2.Zero;
                pointerIntersection.WorldPosition = Vector3.Zero;
                return;
            }
            else
            {
                // Test for terrain intersections
                pointerNodeIntersections.Sort();
                RayIntersectsTerrain(pointerNodeIntersections[0], out pointerIntersection);
            }
        }

        /// <summary>
        /// Test if terrain intersects a sub-rect of terrain.
        /// </summary>
        private void RayIntersectsTerrain(Intersection.ObjectIntersection<QuadNode> ni, out TerrainIntersectionData terrainCollisionData)
        {
            // Get node
            QuadNode node = ni.Object;

            // Transform ray back to object space
            Ray ray;
            Matrix inverseTransform = Matrix.Invert(node.WorldMatrix);
            ray.Position = Vector3.Transform(core.Renderer.PointerRay.Position, inverseTransform);
            ray.Direction = Vector3.TransformNormal(core.Renderer.PointerRay.Direction, inverseTransform);

            int index;
            float height = 0;
            Vector2 position = Vector2.Zero;
            int loopCounter = 0;
            Vector3 step = ray.Direction * 10.0f;
            while (ray.Position.Y > 0)
            {
                // Step ray along direction
                ray.Position += step;

                // Break loop if running too long
                if (++loopCounter > 1000)
                {
                    terrainCollisionData.Distance = null;
                    terrainCollisionData.WorldPosition = Vector3.Zero;
                    terrainCollisionData.MapPosition = Vector2.Zero;
                    return;
                }

                // Get position
                position.X = ray.Position.X + node.Rectangle.X;
                position.Y = ray.Position.Z + node.Rectangle.Y;
                if (position.X < 0 || position.X > dimension - 1) continue;
                if (position.Y < 0 || position.Y > dimension - 1) continue;

                // Get index into arrays
                index = (int)position.Y * (int)dimension + (int)position.X;

                // Get height of terrain at this position
                height = terrainData[index].W * maxHeight;
                if (ray.Position.Y <= height)
                    break;
            }

            // Store map collision in 0,1 space
            terrainCollisionData.MapPosition.X = position.X / dimension;
            terrainCollisionData.MapPosition.Y = position.Y / dimension;

            // Store world collision in world space
            terrainCollisionData.WorldPosition.X = position.X;
            terrainCollisionData.WorldPosition.Y = height;
            terrainCollisionData.WorldPosition.Z = position.Y;

            // Set distance
            terrainCollisionData.Distance = Vector3.Distance(core.ActiveScene.Camera.Position, terrainCollisionData.WorldPosition);
        }

        #endregion

    }

}
