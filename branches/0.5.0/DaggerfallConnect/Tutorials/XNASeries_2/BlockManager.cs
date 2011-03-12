using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;

namespace XNASeries_2
{
    /// <summary>
    /// Helper class to load and draw Daggerfall RMB and RDB blocks for XNA.
    /// </summary>
    public class BlockManager
    {
        #region Class Variables

        private GraphicsDevice graphicsDevice;
        private MeshManager meshManager;
        private BlocksFile blocksFile;
        private Texture2D groundTextureAtlas;
        private Dictionary<string, Block> blockDictionary;
        private MapReader mapReader = new MapReader();
        private TerrainBases terrainBase;

        #endregion

        #region Class Structures

        public struct Block
        {
            public DFBlock dfBlock;
            public VertexPositionNormalTexture[] groundPlaneVertices;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// MeshManager.
        /// </summary>
        public MeshManager MeshManager
        {
            get { return meshManager; }
        }

        /// <summary>
        /// TextureManager.
        /// </summary>
        public TextureManager TextureManager
        {
            get { return meshManager.TextureManager; }
        }

        /// <summary>
        /// BlocksFile.
        /// </summary>
        public BlocksFile BlocksFile
        {
            get { return blocksFile; }
        }

        /// <summary>
        /// Get or set terrain type (i.e. desert, mountain, etc.)
        /// </summary>
        public TerrainBases TerrainBase
        {
            get { return terrainBase; }
            set
            {
                // Store new terrain base and rebuild ground texture atlas
                terrainBase = value;
                BuildGroundTextureAtlas();
            }
        }

        #endregion

        #region Constructors

        public BlockManager(GraphicsDevice device, string arena2Path)
        {
            // Instantiate meshManager
            meshManager = new MeshManager(device, arena2Path);

            // Load BLOCKS.BSA. Setting AutoDiscard to true as we're caching DFBlock data locally
            // and don't need BlocksFile to do it for us.
            blocksFile = new BlocksFile(Path.Combine(arena2Path, "BLOCKS.BSA"), FileUsage.UseDisk, true);
            blocksFile.AutoDiscard = true;

            // Create block dictionary 
            blockDictionary = new Dictionary<string, Block>();

            // Store GraphicsDevice for drawing operations
            graphicsDevice = device;

            // Set default terrain
            TerrainBase = TerrainBases.Temperate;
        }

        #endregion

        #region Public Methods

        public Block LoadBlock(string name)
        {
            if (!blockDictionary.ContainsKey(name))
            {
                // Load block if not already loaded
                int index = blocksFile.GetBlockIndex(name);
                return LoadBlockData(index);
            }
            else
            {
                return blockDictionary[name];
            }
        }

        public Block LoadBlock(int index)
        {
            string name = blocksFile.GetBlockName(index);
            if (!blockDictionary.ContainsKey(name))
            {
                // Load block if not already loaded
                return LoadBlockData(index);
            }
            else
            {
                return blockDictionary[name];
            }
        }

        /// <summary>
        /// Draw block.
        /// </summary>
        /// <param name="name">Name of block.</param>
        /// <param name="effect">BasicEffect to use for rendering.</param>
        public void DrawBlock(string name, ref BasicEffect effect, ref Matrix matrix)
        {
            Block block = blockDictionary[name];
            switch (block.dfBlock.Type)
            {
                case DFBlock.BlockTypes.Rmb:
                    DrawRmbBlock(ref block, ref effect, ref matrix);
                    break;
                case DFBlock.BlockTypes.Rdb:
                    break;
            }
        }

        /// <summary>
        /// Draw block.
        /// </summary>
        /// <param name="index">Index of block.</param>
        /// <param name="effect">BasicEffect to use for rendering.</param>
        public void DrawBlock(int index, ref BasicEffect effect, ref Matrix matrix)
        {
            string name = blocksFile.GetBlockName(index);
            Block block = blockDictionary[name];
            switch (block.dfBlock.Type)
            {
                case DFBlock.BlockTypes.Rmb:
                    DrawRmbBlock(ref block, ref effect, ref matrix);
                    break;
                case DFBlock.BlockTypes.Rdb:
                    break;
            }
        }

        #endregion

        #region Private Methods

        private void DrawRmbBlock(ref Block block, ref BasicEffect effect, ref Matrix matrix)
        {
            Matrix world, rotation, translation;

            // Draw ground plane
            effect.World = matrix;
            DrawRmbGroundPlane(ref block, ref effect);

            // Draw through all subrecords
            foreach (DFBlock.RmbSubRecord record in block.dfBlock.RmbBlock.SubRecords)
            {
                // Transform this record
                float degrees = record.YRotation / 5.68888888888889f;
                translation = Matrix.CreateTranslation(new Vector3(-record.XPos, 0, -record.ZPos));
                rotation = Matrix.CreateRotationY(MathHelper.ToRadians(degrees));
                world = rotation * translation;

                // Draw primary meshes for this record
                foreach (DFBlock.RmbBlock3dObjectRecord obj in record.Exterior.Block3dObjectRecords)
                {
                    // Transform this mesh against record
                    degrees = obj.YRotation / 5.68888888888889f;
                    translation = Matrix.CreateTranslation(new Vector3(-obj.XPos, -obj.YPos, obj.ZPos));
                    rotation = Matrix.CreateRotationY(MathHelper.ToRadians(degrees));
                    effect.World = world * rotation * translation;
                    effect.World *= matrix;

                    // Draw mesh
                    int modelIndex = MeshManager.Arch3dFile.GetRecordIndex(obj.ModelIdNum);
                    MeshManager.DrawMesh(modelIndex, ref effect);
                }
            }
        }

        private void DrawRdbBlock()
        {
        }

        private void DrawRmbGroundPlane(ref Block block, ref BasicEffect effect)
        {
            effect.Texture = groundTextureAtlas;

            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();

            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, block.groundPlaneVertices, 0, 512);

            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }

        private Block LoadBlockData(int index)
        {
            // Get block data
            DFBlock dfBlock = blocksFile.GetBlock(index);

            // Create new local block instance
            Block block = new Block();
            block.dfBlock = dfBlock;

            // Preload content such as models and textures
            PreloadBlockContent(ref block);

            // Store in dictionary
            blockDictionary.Add(block.dfBlock.Name, block);

            return block;
        }

        /// <summary>
        /// Preloads mesh and texture data needed to draw this block.
        /// </summary>
        /// <param name="block">The block object to preload.</param>
        private void PreloadBlockContent(ref Block block)
        {
            if (block.dfBlock.Type == DFBlock.BlockTypes.Rmb)
            {
                // Loop through all subrecords
                foreach (DFBlock.RmbSubRecord record in block.dfBlock.RmbBlock.SubRecords)
                {
                    // Load primary meshes for this record
                    foreach (DFBlock.RmbBlock3dObjectRecord obj in record.Exterior.Block3dObjectRecords)
                    {
                        // Load mesh
                        int index = MeshManager.Arch3dFile.GetRecordIndex(obj.ModelIdNum);
                        MeshManager.LoadMesh(index);
                    }
                }

                // Build ground plane for this RMB block
                BuildRmbGroundPlane(ref block);
            }
            else if (block.dfBlock.Type == DFBlock.BlockTypes.Rdb)
            {
            }
        }

        /// <summary>
        /// Build a texture atlas for each ground texture of the current terrain type.
        /// </summary>
        private void BuildGroundTextureAtlas()
        {
            // Create byte array for texture atlas.
            // The size is 512x512 with 4 bytes per pixel (thus 1048576 bytes total, or exactly 1MB).
            int width = 512, height = 512;
            byte[] atlas = new byte[(width * height) * 4];

            // Load all ground tile textures into atlas array
            int xpos = 0, ypos = 0;
            string filename = mapReader.GetTerrainFileName(terrainBase);
            DFImageFile imageFile = meshManager.TextureManager.ImageFileReader.LoadFile(filename);
            for (int r = 0; r < imageFile.RecordCount; r++)
            {
                DFBitmap dfBitmap = imageFile.GetBitmapFormat(r, 0, 0, DFBitmap.Formats.ARGB);
                int pos = (ypos * (width * dfBitmap.Stride)) + (xpos * dfBitmap.Stride);
                for (int y = 0; y < dfBitmap.Height; y++)
                {
                    Buffer.BlockCopy(dfBitmap.Data,
                        y * dfBitmap.Stride,
                        atlas,
                        pos + (y * (width * 4)),
                        dfBitmap.Stride);
                }

                // Increment position for next texture
                xpos++;
                if (xpos == 8)
                {
                    xpos = 0;
                    ypos++;
                }
            }

            // Create XNA texture from atlas array
            groundTextureAtlas = new Texture2D(graphicsDevice, 512, 512, 0, TextureUsage.AutoGenerateMipMap, SurfaceFormat.Color);
            groundTextureAtlas.SetData<byte>(atlas);
        }

        private void BuildRmbGroundPlane(ref Block block)
        {
            // Create vertex list for ground plane.
            // A ground plane consists of 16x16 tiled squares.
            // There are a full 6 vertices per square (3 per triangle).
            // Doing this so we can just send the whole ground plane to renderer in one call.
            block.groundPlaneVertices = new VertexPositionNormalTexture[(16 * 16) * 6];

            // Add all tiles to the ground plane
            DFBlock.RmbGroundTiles tile;
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    tile = block.dfBlock.RmbBlock.FldHeader.GroundData.GroundTiles[x, y];
                    AddGroundTile(ref block.groundPlaneVertices, x, y, tile.TextureRecord, tile.IsRotated, tile.IsFlipped);
                }
            }
        }

        private void AddGroundTile(ref VertexPositionNormalTexture[] vertices, int x, int y, int record, bool isRotated, bool isFlipped)
        {
            const float side = 256.0f;

            // Calculate atlas texture position from 0.0 to 1.0.
            // Atlas texture is 512x512 pixels with 8x7 rows of tiles.
            // Each tile is 64x64 pixels.
            int row = record / 8;
            int col = record - (row * 8);
            float top = (row * 64) / 512.0f;
            float left = (col * 64) / 512.0f;
            float bottom = top + 0.125f;
            float right = left + 0.125f;

            // Slightly shrink texture area to avoid seams from adjacent textures in atlas
            top += 0.01f;
            left += 0.01f;
            bottom -= 0.01f;
            right -= 0.01f;

            // Set initial UV coordinates to atlas texture
            float tempu, tempv;
            float p0u = left;
            float p0v = top;
            float p1u = left;
            float p1v = bottom;
            float p2u = right;
            float p2v = bottom;
            float p3u = right;
            float p3v = top;

            // Rotated (width becomes height)
            if (isRotated)
            {
                // Move coordinates around 90 degrees
                tempu = p3u;
                tempv = p3v;
                p3u = p2u;
                p3v = p2v;
                p2u = p1u;
                p2v = p1v;
                p1u = p0u;
                p1v = p0v;
                p0u = tempu;
                p0v = tempv;
            }

            // Flipped (first pixel becomes last)
            if (isFlipped)
            {
                // Flip 0 and 2
                tempu = p0u;
                tempv = p0v;
                p0u = p2u;
                p0v = p2v;
                p2u = tempu;
                p2v = tempv;

                // Flip 1 and 3
                tempu = p1u;
                tempv = p1v;
                p1u = p3u;
                p1v = p3v;
                p3u = tempu;
                p3v = tempv;
            }

            // Point 0
            VertexPositionNormalTexture point0 = new VertexPositionNormalTexture();
            point0.Position = new Vector3(-(side * x), 0, -(side * y));
            point0.Normal = new Vector3(0, 1, 0);
            point0.TextureCoordinate = new Vector2(p0u, p0v);

            // Point 1
            VertexPositionNormalTexture point1 = new VertexPositionNormalTexture();
            point1.Position = new Vector3(-(side * x), 0, -(side * (y+1)));
            point1.Normal = new Vector3(0, 1, 0);
            point1.TextureCoordinate = new Vector2(p1u, p1v);

            // Point 2
            VertexPositionNormalTexture point2 = new VertexPositionNormalTexture();
            point2.Position = new Vector3(-(side * (x+1)), 0, -(side * (y+1)));
            point2.Normal = new Vector3(0, 1, 0);
            point2.TextureCoordinate = new Vector2(p2u, p2v);

            // Point 3
            VertexPositionNormalTexture point3 = new VertexPositionNormalTexture();
            point3.Position = new Vector3(-(side * (x+1)), 0, -(side * y));
            point3.Normal = new Vector3(0, 1, 0);
            point3.TextureCoordinate = new Vector2(p3u, p3v);

            // Compute start index
            int index = (y * 16 + x) * 6;

            // Write first triangle of this tile
            vertices[index] = point0;
            vertices[index + 1] = point2;
            vertices[index + 2] = point1;

            // Write second triangle of this tile
            vertices[index + 3] = point0;
            vertices[index + 4] = point3;
            vertices[index + 5] = point2;
        }

        #endregion
    }
}
