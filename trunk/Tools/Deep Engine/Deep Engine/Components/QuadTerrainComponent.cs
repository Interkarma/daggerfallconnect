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

        // Strings
        const string errorInvalidDimensions = "Source texture must equal terrain dimensions.";
        const string errorInvalidBlendMapndex = "Invalid blend map index.";

        // Constants
        const int heightMultiplier = 8;
        const int formatWidth = 4;

        // Quadtree root
        QuadNode rootNode;
        
        // Map data
        float normalStrength = 0.25f;
        float textureRepeat = 20f;
        int mapDimension;
        int leafDimension;
        int levelCount;
        float mapHeight;
        Vector4[] terrainData;

        // Terrain textures
        bool flipFlopHeightMaps = false;
        Texture2D terrainVertexMap;
        Texture2D terrainVertexMapUpdated;
        Texture2D terrainBlendMap0;
        Texture2D terrainBlendMap1;

        // Grid
        Grid grid;

        // Picking
        bool enablePicking = false;
        const int defaultIntersectionCapacity = 35;
        List<Intersection.ObjectIntersection<QuadNode>> pointerNodeIntersections;
        TerrainIntersectionData pointerIntersection;

        // Effects
        Effect terrainEffect;

        // Effect parameters
        EffectParameter terrainEffect_View;
        EffectParameter terrainEffect_Projection;
        EffectParameter terrainEffect_World;
        EffectParameter terrainEffect_VertexTexture;
        EffectParameter terrainEffect_BlendTexture0;
        EffectParameter terrainEffect_BlendTexture1;
        EffectParameter terrainEffect_Texture0;
        EffectParameter terrainEffect_Texture1;
        EffectParameter terrainEffect_Texture2;
        EffectParameter terrainEffect_Texture3;
        EffectParameter terrainEffect_Texture4;
        EffectParameter terrainEffect_Texture5;
        EffectParameter terrainEffect_Texture6;
        EffectParameter terrainEffect_Texture7;
        EffectParameter terrainEffect_Texture8;
        EffectParameter terrainEffect_MaxHeight;
        EffectParameter terrainEffect_SampleScale;
        EffectParameter terrainEffect_SampleOffset;
        EffectParameter terrainEffect_TextureRepeat;

        #endregion

        #region Class Structures

        /// <summary>
        /// Size of terrain.
        /// </summary>
        public enum TerrainSize
        {
            Small = 512,
            Medium = 1024,
            Large = 2048,
        }

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
        /// Gets or sets texture repeat value.
        /// </summary>
        public float TextureRepeat
        {
            get { return textureRepeat; }
            set { textureRepeat = value; }
        }

        /// <summary>
        /// Gets subdivision level count.
        /// </summary>
        public int LevelCount
        {
            get { return levelCount; }
        }

        /// <summary>
        /// Gets dimension of maps along each side.
        /// </summary>
        public int MapDimension
        {
            get { return mapDimension; }
        }

        /// <summary>
        /// Gets dimension of map leaf nodes along each side.
        /// </summary>
        public int LeafDimension
        {
            get { return leafDimension; }
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
        /// Gets map height.
        /// </summary>
        public float MapHeight
        {
            get { return mapHeight; }
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
        /// Diffuse texture used for clearing (zero value in blend map).
        /// </summary>
        public Texture2D Texture0 { get; private set; }

        /// <summary>
        /// Diffuse texture.
        /// </summary>
        public Texture2D Texture1 { get; private set; }

        /// <summary>
        /// Diffuse texture.
        /// </summary>
        public Texture2D Texture2 { get; private set; }

        /// <summary>
        /// Diffuse texture.
        /// </summary>
        public Texture2D Texture3 { get; private set; }

        /// <summary>
        /// Diffuse texture.
        /// </summary>
        public Texture2D Texture4 { get; private set; }

        /// <summary>
        /// Diffuse texture.
        /// </summary>
        public Texture2D Texture5 { get; private set; }

        /// <summary>
        /// Diffuse texture.
        /// </summary>
        public Texture2D Texture6 { get; private set; }

        /// <summary>
        /// Diffuse texture.
        /// </summary>
        public Texture2D Texture7 { get; private set; }

        /// <summary>
        /// Diffuse texture.
        /// </summary>
        public Texture2D Texture8 { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="size">Size of terrain.</param>
        public QuadTerrainComponent(DeepCore core, TerrainSize size)
            : base(core)
        {
            // Get dimensions
            this.mapDimension = (int)size;

            // Height maps must always subdivide into 128x128 or smaller leaf nodes.
            // This is to ensure each leaf tile fits within a single vertex buffer.
            leafDimension = mapDimension;
            int levelCount = 0;
            while (leafDimension > 128)
            {
                levelCount++;
                leafDimension /= 2;
            }

            // Store values
            this.levelCount = levelCount;
            this.terrainBlendMap0 = new Texture2D(core.GraphicsDevice, mapDimension, mapDimension, false, SurfaceFormat.Color);
            this.terrainBlendMap1 = new Texture2D(core.GraphicsDevice, mapDimension, mapDimension, false, SurfaceFormat.Color);
            this.mapHeight = mapDimension * heightMultiplier;

            // Clear blend maps
            ClearBlendMaps();

            // Create vertex texture
            terrainVertexMap = new Texture2D(
                core.GraphicsDevice,
                mapDimension,
                mapDimension,
                false,
                SurfaceFormat.Vector4);

            // Set default textures
            Texture0 = core.MaterialManager.CreateDaggerfallMaterialEffect(302, 1, null, MaterialManager.DefaultTerrainFlags).DiffuseTexture;
            Texture1 = core.MaterialManager.CreateDaggerfallMaterialEffect(102, 1, null, MaterialManager.DefaultTerrainFlags).DiffuseTexture;
            Texture2 = core.MaterialManager.CreateDaggerfallMaterialEffect(302, 2, null, MaterialManager.DefaultTerrainFlags).DiffuseTexture;
            Texture3 = core.MaterialManager.CreateDaggerfallMaterialEffect(302, 3, null, MaterialManager.DefaultTerrainFlags).DiffuseTexture;
            Texture4 = core.MaterialManager.CreateDaggerfallMaterialEffect(303, 3, null, MaterialManager.DefaultTerrainFlags).DiffuseTexture;

            //Texture5 = core.MaterialManager.CreateDaggerfallMaterialEffect(002, 0, null, MaterialManager.DefaultTerrainFlags).DiffuseTexture;
            //Texture6 = core.MaterialManager.CreateDaggerfallMaterialEffect(002, 0, null, MaterialManager.DefaultTerrainFlags).DiffuseTexture;
            //Texture7 = core.MaterialManager.CreateDaggerfallMaterialEffect(002, 0, null, MaterialManager.DefaultTerrainFlags).DiffuseTexture;
            Texture8 = core.MaterialManager.CreateDaggerfallMaterialEffect(002, 0, null, MaterialManager.DefaultTerrainFlags).DiffuseTexture;

            // Create arrays
            terrainData = new Vector4[mapDimension * mapDimension];

            // Create grid
            grid = new Grid(core.GraphicsDevice, leafDimension, 1.0f);

            // Initialise map data
            SetHeight(0);

            // Initialise quad tree
            BuildQuadTree();

            // Create intersections list
            pointerNodeIntersections = new List<Intersection.ObjectIntersection<QuadNode>>();

            // Load effects
            terrainEffect = core.ContentManager.Load<Effect>("Effects/RenderTerrain");

            // Get effect parameters
            terrainEffect_View = terrainEffect.Parameters["View"];
            terrainEffect_Projection = terrainEffect.Parameters["Projection"];
            terrainEffect_World = terrainEffect.Parameters["World"];
            terrainEffect_VertexTexture = terrainEffect.Parameters["VertexTexture"];
            terrainEffect_BlendTexture0 = terrainEffect.Parameters["BlendTexture0"];
            terrainEffect_BlendTexture1 = terrainEffect.Parameters["BlendTexture1"];
            terrainEffect_Texture0 = terrainEffect.Parameters["Texture0"];
            terrainEffect_Texture1 = terrainEffect.Parameters["Texture1"];
            terrainEffect_Texture2 = terrainEffect.Parameters["Texture2"];
            terrainEffect_Texture3 = terrainEffect.Parameters["Texture3"];
            terrainEffect_Texture4 = terrainEffect.Parameters["Texture4"];
            terrainEffect_Texture5 = terrainEffect.Parameters["Texture5"];
            terrainEffect_Texture6 = terrainEffect.Parameters["Texture6"];
            terrainEffect_Texture7 = terrainEffect.Parameters["Texture7"];
            terrainEffect_Texture8 = terrainEffect.Parameters["Texture8"];
            terrainEffect_MaxHeight = terrainEffect.Parameters["MaxHeight"];
            terrainEffect_SampleScale = terrainEffect.Parameters["SampleScale"];
            terrainEffect_SampleOffset = terrainEffect.Parameters["SampleOffset"];
            terrainEffect_TextureRepeat = terrainEffect.Parameters["TextureRepeat"];
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

            // Flip-flop height maps
            if (flipFlopHeightMaps)
            {
                Texture2D temp = terrainVertexMap;
                terrainVertexMap = terrainVertexMapUpdated;
                terrainVertexMapUpdated = temp;
                flipFlopHeightMaps = false;
            }

            // Set effect paramaters
            terrainEffect_View.SetValue(core.ActiveScene.Camera.ViewMatrix);
            terrainEffect_Projection.SetValue(core.ActiveScene.Camera.ProjectionMatrix);
            terrainEffect_VertexTexture.SetValue(terrainVertexMap);
            terrainEffect_BlendTexture0.SetValue(terrainBlendMap0);
            terrainEffect_BlendTexture1.SetValue(terrainBlendMap1);
            terrainEffect_Texture0.SetValue(Texture0);
            terrainEffect_Texture1.SetValue(Texture1);
            terrainEffect_Texture2.SetValue(Texture2);
            terrainEffect_Texture3.SetValue(Texture3);
            terrainEffect_Texture4.SetValue(Texture4);
            terrainEffect_Texture5.SetValue(Texture5);
            terrainEffect_Texture6.SetValue(Texture6);
            terrainEffect_Texture7.SetValue(Texture7);
            terrainEffect_Texture8.SetValue(Texture8);
            terrainEffect_MaxHeight.SetValue(mapHeight);
            terrainEffect_TextureRepeat.SetValue(textureRepeat);

            // Draw
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
        /// Gets height data as an array of floats.
        /// </summary>
        /// <returns>Height data array.</returns>
        public float[] GetHeight()
        {
            // Create data buffer
            float[] buffer = new float[terrainData.Length];
            for (int i = 0; i < terrainData.Length; i++)
            {
                buffer[i] = terrainData[i].W;
            }

            return buffer;
        }

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
            if (x < 0) x = 0; else if (x >= mapDimension) x = mapDimension - 1;
            if (y < 0) y = 0; else if (y >= mapDimension) y = mapDimension - 1;

            // Get position
            int pos = y * mapDimension + x;

            return terrainData[pos].W;
        }

        /// <summary>
        /// Sets height map from a Texture2D.
        /// </summary>
        /// <param name="heightMap">Height map texture.</param>
        public void SetHeight(Texture2D heightMap)
        {
            // Esnure source dimensions are equal to terrain dimensions
            if (heightMap.Width != mapDimension ||
                heightMap.Height != mapDimension)
            {
                throw new Exception(errorInvalidDimensions);
            }

            // Get color data from heightmap
            Color[] heightData = new Color[heightMap.Width * heightMap.Height];
            heightMap.GetData<Color>(heightData);

            // Compute heights
            for (int y = 0; y < mapDimension; y++)
            {
                for (int x = 0; x < mapDimension; x++)
                {
                    // Set height as average of colour image
                    int pos = y * mapDimension + x;
                    terrainData[pos].W = ((heightData[pos].R + heightData[pos].G + heightData[pos].B) / 3) / 255.0f;
                }
            }

            // Update other data
            UpdateNormalData();
            UpdateTerrainVertexTexture();
            UpdateBoundingSphere();
        }

        /// <summary>
        /// Sets height map from a float array.
        /// </summary>
        /// <param name="heightData">Array of height values.</param>
        public void SetHeight(float[] heightData)
        {
            // Esnure source dimensions are equal to terrain dimensions
            if (heightData.Length != mapDimension * mapDimension)
            {
                throw new Exception(errorInvalidDimensions);
            }

            // Assign heights
            for (int pos = 0; pos < mapDimension * mapDimension; pos++)
            {
                terrainData[pos].W = heightData[pos];
            }

            // Update other data
            UpdateNormalData();
            UpdateTerrainVertexTexture();
            UpdateBoundingSphere();
        }

        /// <summary>
        /// Sets entire height map to a single float value;
        /// </summary>
        /// <param name="height">Height of terrain.</param>
        public void SetHeight(float height)
        {
            // Clamp to valid range
            height = MathHelper.Clamp(height, -1.0f, 1.0f);

            // Assign height
            for (int pos = 0; pos < mapDimension * mapDimension; pos++)
            {
                terrainData[pos].W = height;
            }

            // Update other data
            UpdateNormalData();
            UpdateTerrainVertexTexture();
            UpdateBoundingSphere();
        }

        /// <summary>
        /// Gets blend map as an array of RGBA bytes.
        /// </summary>
        /// <param name="index">Index of blend map.</param>
        /// <returns>RGBA byte array.</returns>
        public byte[] GetBlend(int index)
        {
            byte[] buffer = new byte[mapDimension * mapDimension * formatWidth];
            GetBlendMapFromIndex(index).GetData<byte>(buffer);

            return buffer;
        }

        /// <summary>
        /// Sets blend map from an RGBA byte[] array.
        /// </summary>
        /// <param name="index">Index of blend map.</param>
        /// <param name="blendData">Array of blend values.</param>
        public void SetBlend(int index, byte[] blendData)
        {
            // Ensure source dimensions are equal to terrain dimensions
            if (blendData.Length != (mapDimension * mapDimension * formatWidth))
            {
                throw new Exception(errorInvalidDimensions);
            }
            
            // Set blend data
            GetBlendMapFromIndex(index).SetData<byte>(blendData);
        }

        /// <summary>
        /// Clears blend maps.
        /// </summary>
        public void ClearBlendMaps()
        {
            // Create array of transparent colours
            Color[] colors = new Color[mapDimension * mapDimension];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.Transparent;
            }

            // Apply to blend maps
            terrainBlendMap0.SetData<Color>(colors);
            terrainBlendMap1.SetData<Color>(colors);
        }

        /// <summary>
        /// Updates normal data based on current height data.
        /// </summary>
        public void UpdateNormalData()
        {
            if (terrainData == null)
                return;

            // Compute normals
            for (int y = 0; y < mapDimension; y++)
            {
                for (int x = 0; x < mapDimension; x++)
                {
                    // Look up the heights to either side of this pixel
                    float left = GetHeight(x - 1, y);
                    float right = GetHeight(x + 1, y);
                    float top = GetHeight(x, y - 1);
                    float bottom = GetHeight(x, y + 1);

                    // Compute gradient vectors, then cross them to get the normal
                    Vector3 dx = new Vector3(1, 0, (right - left) * (mapHeight * normalStrength));
                    Vector3 dy = new Vector3(0, 1, (bottom - top) * (mapHeight * normalStrength));
                    Vector3 normal = Vector3.Cross(dx, dy);
                    normal.Normalize();

                    // Store result
                    int pos = y * mapDimension + x;
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

            // Create update texture if it doesn't exist
            if (terrainVertexMapUpdated == null)
            {
                terrainVertexMapUpdated = new Texture2D(
                    core.GraphicsDevice,
                    mapDimension,
                    mapDimension,
                    false,
                    SurfaceFormat.Vector4);
            }

            // Update destination texture
            terrainVertexMapUpdated.SetData<Vector4>(terrainData);
            flipFlopHeightMaps = true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates component bounding sphere from current map dimension and height.
        /// </summary>
        private void UpdateBoundingSphere()
        {
            // Set bounding sphere
            float diameter = (mapHeight > mapDimension) ? mapHeight : mapDimension;
            this.boundingSphere = new BoundingSphere(
                new Vector3(mapDimension / 2, 0, mapDimension / 2),
                diameter * 0.70f);
        }

        /// <summary>
        /// Builds quadtree.
        /// </summary>
        private void BuildQuadTree()
        {
            // Create parent node
            rootNode = new QuadNode(mapDimension, mapHeight);
            AddQuadChildren(rootNode);
        }

        /// <summary>
        /// Adds quad node children to tree.
        /// </summary>
        /// <param name="parent">Parent node.</param>
        private void AddQuadChildren(QuadNode parent)
        {
            // Add children
            if (parent.Level < levelCount)
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

        /// <summary>
        /// Gets blend map from index.
        /// </summary>
        /// <param name="index">Index of blend map</param>
        /// <returns>Blend map texture.</returns>
        private Texture2D GetBlendMapFromIndex(int index)
        {
            // Ensure blend map index in range
            if (index < 0 || index > 1)
            {
                throw new Exception(errorInvalidBlendMapndex);
            }

            // Return texture
            switch (index)
            {
                case 0:
                    return terrainBlendMap0;
                case 1:
                    return terrainBlendMap1;
                default:
                    return null;
            }
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
            sampleScale.X = (float)leafDimension / (float)mapDimension;
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

                // Calculate sample offset
                Vector2 sampleOffset;
                sampleOffset.X = node.X / mapDimension;
                sampleOffset.Y = node.Y / mapDimension;

                // Set effect parameters for this quad
                terrainEffect_SampleScale.SetValue(sampleScale);
                terrainEffect_SampleOffset.SetValue(sampleOffset);
                terrainEffect_World.SetValue(node.WorldMatrix);

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
            Vector3 step = ray.Direction * 1f;
            bool found = false;
            while (true)
            {
                // Step ray along direction
                ray.Position += step;

                // Break loop if running too long
                if (++loopCounter > 5000)
                {
                    found = false;
                    break;
                }

                // Get position
                position.X = ray.Position.X + node.Rectangle.X;
                position.Y = ray.Position.Z + node.Rectangle.Y;
                if (position.X < 0 || position.X > mapDimension - 1 ||
                    position.Y < 0 || position.Y > mapDimension - 1)
                {
                    found = false;
                    continue;
                }

                // If we've come this far then an intersection is likely
                found = true;

                // Get index into arrays
                index = (int)position.Y * (int)mapDimension + (int)position.X;

                // Get height of terrain at this position
                height = terrainData[index].W * mapHeight;
                if (ray.Position.Y <= height)
                {
                    ray.Position -= step;
                    break;
                }
            }

            // Exit if no intersection found
            if (!found)
            {
                terrainCollisionData.Distance = null;
                terrainCollisionData.WorldPosition = Vector3.Zero;
                terrainCollisionData.MapPosition = Vector2.Zero;
                return;
            }

            // Store map collision in 0,1 space
            terrainCollisionData.MapPosition.X = position.X / mapDimension;
            terrainCollisionData.MapPosition.Y = position.Y / mapDimension;

            // Store world collision in world space
            Vector3 worldPosition = new Vector3
            {
                X = position.X,
                Y = height,
                Z = position.Y,
            };
            terrainCollisionData.WorldPosition = Vector3.Transform(worldPosition, this.Matrix);

            // Set distance
            terrainCollisionData.Distance = Vector3.Distance(core.ActiveScene.Camera.Position, terrainCollisionData.WorldPosition);
        }

        #endregion

    }

}
