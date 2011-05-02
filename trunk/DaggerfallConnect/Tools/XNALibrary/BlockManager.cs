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
using System.Drawing;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace XNALibrary
{

    /// <summary>
    /// Helper class to load and store Daggerfall RMB and RDB blocks. This class will stub model id
    ///  and bounding boxes, but does not load model data or textures. All blocks are cached to
    ///  minimise reloading data.
    /// </summary>
    public class BlockManager
    {
        #region Class Variables

        private string arena2Path;
        private GraphicsDevice graphicsDevice;
        private BlocksFile blocksFile;
        private const float rotationDivisor = 5.68888888888889f;
        private Dictionary<string, BlockData> blockDictionary;

        #endregion

        #region Class Structures

        /// <summary>
        /// Describes block layout as a ground plane and list of models. Models are represented by bounding
        ///  boxes and ID. These bounding boxes are just stubs that will need to be properly sized once model
        ///  data has been loaded. Use these bounding boxes for frustum culling, collision tests, etc.
        /// </summary>
        public struct BlockData
        {
            /// <summary>Original DFBlock object from Daggerfall data.</summary>
            public DFBlock DFBlock;

            /// <summary>Flag set when block needs an update.</summary>
            public bool UpdateRequired;

            /// <summary>Bounding volume of this block in local space.</summary>
            public BoundingBox BoundingBox;

            /// <summary>Vertices of ground plane.</summary>
            public VertexPositionNormalTexture[] GroundPlaneVertices;

            /// <summary>VertexBuffer of ground plane.</summary>
            public VertexBuffer GroundPlaneVertexBuffer;

            /// <summary>Array of models used to build this block.</summary>
            public List<BlockModel> Models;

            /// <summary>Array of flats (billboards) populating block.</summary>
            public List<BlockFlat> Flats;
        }

        /// <summary>
        /// Describes a model contained within a block.
        /// </summary>
        public struct BlockModel
        {
            /// <summary>Unique ID of model.</summary>
            public uint ModelId;

            /// <summary>Local transform to model and bounding volume.</summary>
            public Matrix Matrix;

            /// <summary>Bounding volume of this model in local space.</summary>
            public BoundingBox BoundingBox;

            /// <summary>Bounding sphere containing mesh data.</summary>
            public BoundingSphere BoundingSphere;
        }

        /// <summary>
        /// Describes a flat object (billboard) contained within the block.
        /// </summary>
        public struct BlockFlat
        {
            /// <summary>Type of block this flat is inside.</summary>
            public DFBlock.BlockTypes BlockType;

            /// <summary>Index of texture archive.</summary>
            public int TextureArchive;

            /// <summary>Index of texture record.</summary>
            public int TextureRecord;

            /// <summary>Texture key.</summary>
            public int TextureKey;

            /// <summary>Flat origin.</summary>
            public Vector3 Origin;

            /// <summary>Position in block space.</summary>
            public Vector3 Position;

            /// <summary>Dimensions of flat.</summary>
            public Vector2 Size;

            /// <summary>Bounding sphere containing flat.</summary>
            public BoundingSphere BoundingSphere;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets Arena2 path set at construction.
        /// </summary>
        public string Arena2Path
        {
            get { return arena2Path; }
        }

        /// <summary>
        /// BlocksFile.
        /// </summary>
        public BlocksFile BlocksFile
        {
            get { return blocksFile; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="device">Graphics Device.</param>
        /// <param name="arena2Path">Path to Arena2 folder.</param>
        public BlockManager(GraphicsDevice device, string arena2Path)
        {
            // Setup
            graphicsDevice = device;
            this.arena2Path = arena2Path;

            // Load BLOCKS.BSA
            blocksFile = new BlocksFile(Path.Combine(arena2Path, "BLOCKS.BSA"), FileUsage.UseDisk, true);

            // Create block dictionary 
            blockDictionary = new Dictionary<string, BlockData>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load a block by name.
        /// </summary>
        /// <param name="name">Name of block.</param>
        /// <returns>Block.</returns>
        public BlockData LoadBlock(string name)
        {
            if (!blockDictionary.ContainsKey(name))
            {
                // Load block if not already loaded
                return LoadBlockData(name);
            }
            else
            {
                return blockDictionary[name];
            }
        }

        /// <summary>
        /// Load a block by index.
        /// </summary>
        /// <param name="index">Index of block.</param>
        /// <returns>Block.</returns>
        public BlockData LoadBlock(int index)
        {
            string name = blocksFile.GetBlockName(index);
            if (!blockDictionary.ContainsKey(name))
            {
                // Load block if not already loaded
                return LoadBlockData(name);
            }
            else
            {
                return blockDictionary[name];
            }
        }

        /// <summary>
        /// Builds an RMB ground plane. References terrain atlas from the
        ///  specified texture manager. You can swap climate and weather in the
        ///  texture manager without needing to rebuild the ground plane.
        /// </summary>
        /// <param name="textureManager">TextureManager.</param>
        /// <param name="block">Block.</param>
        public void BuildRmbGroundPlane(TextureManager textureManager, ref BlockData block)
        {
            // Create vertex list for ground plane.
            // A ground plane consists of 16x16 tiled squares.
            // There are a full 6 vertices per square (3 per triangle).
            // We're doing this so we can send the whole ground plane to renderer in one call.
            block.GroundPlaneVertices = new VertexPositionNormalTexture[(16 * 16) * 6];

            // Add tiles to the ground plane. Source tiles are stored from bottom to top, then right to left.
            // This must be accounted for when laying out tiles.
            const int tileCount = 16;
            DFBlock.RmbGroundTiles tile;
            for (int x = 0; x < tileCount; x++)
            {
                for (int y = tileCount - 1; y >= 0; y--)
                {
                    tile = block.DFBlock.RmbBlock.FldHeader.GroundData.GroundTiles[x, y];
                    AddGroundTile(ref textureManager, ref block.GroundPlaneVertices, x, y, tile.TextureRecord, tile.IsRotated, tile.IsFlipped);
                }
            }

            // Create VertexBuffer
            block.GroundPlaneVertexBuffer = new VertexBuffer(
                graphicsDevice,
                VertexPositionNormalTexture.SizeInBytes * block.GroundPlaneVertices.Length,
                BufferUsage.WriteOnly);
            block.GroundPlaneVertexBuffer.SetData<VertexPositionNormalTexture>(block.GroundPlaneVertices);
        }

        #endregion

        #region Content Loading Methods

        private BlockData LoadBlockData(string name)
        {
            // Get block data
            DFBlock dfBlock = blocksFile.GetBlock(name);

            // Create new local block instance
            BlockData block = new BlockData();
            block.DFBlock = dfBlock;
            block.UpdateRequired = true;

            // Build a model from this block
            switch (block.DFBlock.Type)
            {
                case DFBlock.BlockTypes.Rmb:
                    StubRmbBlockLayout(ref block);
                    break;
                case DFBlock.BlockTypes.Rdb:
                    StubRdbBlockLayout(ref block);
                    break;
                default:
                    break;
            }

            // Store in dictionary
            if (!string.IsNullOrEmpty(block.DFBlock.Name))
                blockDictionary.Add(block.DFBlock.Name, block);

            return block;
        }

        private void StubRmbBlockLayout(ref BlockData block)
        {
            // Create bounding box for this block.
            // All outdoor blocks are initialised to 4096x512x4096.
            // Blocks are laid out on a 2D grid in X-Z space of 4096x4096 per block,
            // so only height is arbitrary. You should adjust bounding box later
            // to properly contain block.
            Vector3 blockMin = new Vector3(0, 0, -4096);
            Vector3 blockMax = new Vector3(4096, 512, 0);
            block.BoundingBox = new BoundingBox(blockMin, blockMax);

            // Create model info list with a starting capacity equal to subrecord count.
            // Many subrecords have only 1 model per subrecord, but may have more.
            // The List will grow if needed.
            block.Models = new List<BlockModel>(block.DFBlock.RmbBlock.SubRecords.Length);
            block.Flats = new List<BlockFlat>(block.DFBlock.RmbBlock.MiscFlatObjectRecords.Length);

            // Iterate through all subrecords
            float degrees;
            Matrix rotation, translation;
            foreach (DFBlock.RmbSubRecord subRecord in block.DFBlock.RmbBlock.SubRecords)
            {
                // Get matrix for this subrecord
                Matrix subRecordMatrix = Matrix.Identity;
                degrees = subRecord.YRotation / rotationDivisor;
                translation = Matrix.CreateTranslation(new Vector3(subRecord.XPos, 0, -4096 + subRecord.ZPos));
                rotation = Matrix.CreateRotationY(MathHelper.ToRadians(degrees));
                subRecordMatrix *= rotation;
                subRecordMatrix *= translation;

                // Iterate through models in this subrecord
                foreach (DFBlock.RmbBlock3dObjectRecord obj in subRecord.Exterior.Block3dObjectRecords)
                {
                    // Get matrix for this model
                    Matrix objMatrix = Matrix.Identity;
                    degrees = obj.YRotation / rotationDivisor;
                    translation = Matrix.CreateTranslation(new Vector3(obj.XPos, -obj.YPos, -obj.ZPos));
                    rotation = Matrix.CreateRotationY(MathHelper.ToRadians(degrees));
                    objMatrix *= rotation;
                    objMatrix *= translation;

                    // Create stub of model info
                    BlockModel modelInfo = new BlockModel();
                    modelInfo.ModelId = obj.ModelIdNum;
                    modelInfo.Matrix = objMatrix * subRecordMatrix;
                    block.Models.Add(modelInfo);
                }
            }

            // Iterate through all misc records
            foreach (DFBlock.RmbBlock3dObjectRecord obj in block.DFBlock.RmbBlock.Misc3dObjectRecords)
            {
                // Get matrix for this record
                Matrix recordMatrix = Matrix.Identity;
                degrees = obj.YRotation / rotationDivisor;
                translation = Matrix.CreateTranslation(new Vector3(obj.XPos, -obj.YPos, -4096 + -obj.ZPos));
                rotation = Matrix.CreateRotationY(MathHelper.ToRadians(degrees));
                recordMatrix *= rotation;
                recordMatrix *= translation;

                // Create stub of model info
                BlockModel modelInfo = new BlockModel();
                modelInfo.ModelId = obj.ModelIdNum;
                modelInfo.Matrix = recordMatrix;
                block.Models.Add(modelInfo);
            }

            // Iterate through all misc flat records
            foreach (DFBlock.RmbBlockFlatObjectRecord obj in block.DFBlock.RmbBlock.MiscFlatObjectRecords)
            {
                // Create stub of billboard info
                BlockFlat flatInfo = new BlockFlat();
                flatInfo.BlockType = DFBlock.BlockTypes.Rmb;
                flatInfo.TextureArchive = obj.TextureArchive;
                flatInfo.TextureRecord = obj.TextureRecord;
                flatInfo.TextureKey = -1;
                flatInfo.Position.X = obj.XPos;
                flatInfo.Position.Y = -obj.YPos;
                flatInfo.Position.Z = -4096 + -obj.ZPos;
                block.Flats.Add(flatInfo);
            }
        }

        private void StubRdbBlockLayout(ref BlockData block)
        {
            float degreesX, degreesY, degreesZ;
            Matrix rotationX, rotationY, rotationZ;
            Matrix rotation, translation;

            // Create bounding box for this block.
            // All dungeon blocks are initialised to 2048x2048x2048.
            // Blocks are laid out on a 2D grid in X-Z space of 2048x2048 per block,
            // so only height is arbitrary. You should adjust bounding box later
            // to properly contain block.
            Vector3 blockMin = new Vector3(0, 0, -2048);
            Vector3 blockMax = new Vector3(2048, 2048, 0);
            block.BoundingBox = new BoundingBox(blockMin, blockMax);

            // Create empty model and flat info lists. These will grow as needed.
            block.Models = new List<BlockModel>();
            block.Flats = new List<BlockFlat>();

            // Iterate through object groups
            foreach (DFBlock.RdbObjectRoot group in block.DFBlock.RdbBlock.ObjectRootList)
            {
                // Skip empty object groups
                if (null == group.RdbObjects)
                    continue;

                // Iterate through objects in this group
                foreach (DFBlock.RdbObject obj in group.RdbObjects)
                {
                    // Create translation matrix
                    translation = Matrix.CreateTranslation(obj.XPos, -obj.YPos, -obj.ZPos);

                    // Filter by type
                    switch (obj.Type)
                    {
                        case DFBlock.RdbResourceTypes.Model:
                            // Get model reference index, then id, and finally index
                            int modelReference = obj.Resources.ModelResource.ModelIndex;
                            uint modelId = block.DFBlock.RdbBlock.ModelReferenceList[modelReference].ModelIdNum;

                            // Get rotation matrix for each axis
                            degreesX = obj.Resources.ModelResource.XRotation / rotationDivisor;
                            degreesY = obj.Resources.ModelResource.YRotation / rotationDivisor;
                            degreesZ = -obj.Resources.ModelResource.ZRotation / rotationDivisor;
                            rotationX = Matrix.CreateRotationX(MathHelper.ToRadians(degreesX));
                            rotationY = Matrix.CreateRotationY(MathHelper.ToRadians(degreesY));
                            rotationZ = Matrix.CreateRotationZ(MathHelper.ToRadians(degreesZ));

                            // Create final rotation matrix
                            rotation = Matrix.Identity;
                            rotation *= rotationY;
                            rotation *= rotationX;
                            rotation *= rotationZ;

                            // Create stub of model info
                            BlockModel modelInfo = new BlockModel();
                            modelInfo.ModelId = modelId;
                            modelInfo.Matrix = rotation * translation;
                            block.Models.Add(modelInfo);
                            break;

                        case DFBlock.RdbResourceTypes.Flat:
                            // Create stub of billboard info
                            BlockFlat flatInfo = new BlockFlat();
                            flatInfo.BlockType = DFBlock.BlockTypes.Rdb;
                            flatInfo.TextureArchive = obj.Resources.FlatResource.TextureArchive;
                            flatInfo.TextureRecord = obj.Resources.FlatResource.TextureRecord;
                            flatInfo.TextureKey = -1;
                            flatInfo.Origin = Vector3.Zero;
                            flatInfo.Position.X = obj.XPos;
                            flatInfo.Position.Y = -obj.YPos;
                            flatInfo.Position.Z = -obj.ZPos;
                            block.Flats.Add(flatInfo);
                            break;

                        default:
                            // Only drawing models and flats for now
                            break;
                    }
                }
            }
        }

        #endregion

        #region Ground Plane Methods

        /// <summary>
        /// Adds a single tile to the ground plane in.
        /// </summary>
        /// <param name="textureManager">Texture manager for texture lookups.</param>
        /// <param name="vertices">Vertex array to be populated with data.</param>
        /// <param name="x">X position in grid from 0-15.</param>
        /// <param name="y">Y position in grid from 0-15.</param>
        /// <param name="record">Record index.</param>
        /// <param name="isRotated">True if rotated 90 degrees right.</param>
        /// <param name="isFlipped">True if flipped horizontally and vertically.</param>
        private void AddGroundTile(ref TextureManager textureManager, ref VertexPositionNormalTexture[] vertices, int x, int y, int record, bool isRotated, bool isFlipped)
        {
            // Each block ground plane is made of 16x16 tiles.
            // Each tile is 256x256 world units.
            const float side = 256.0f;

            // Handle record > 55. This indicates that random terrain should be used outside
            //  of city walls. Here, we just set this back to 2 (grass/earth).
            if (record > 55)
                record = 2;

            // Get subtexture rect
            RectangleF rect = textureManager.GetTerrainSubTextureRect(record);
            float top = rect.Top;
            float left = rect.Left;
            float bottom = rect.Bottom;
            float right = rect.Right;

            // Slightly shrink texture area to avoid filter overlap with adjacent pixels in atlas
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

            // All Z points are offset from -4096 to ensure bottom-left corner (index 0 of block array)
            // aligns with local origin. Daggerfall stores block arrays from bottom to top, then right to left.
            // This basically means that the first tile (as seen from above) is at 0,0 in the array.

            // Point 0
            VertexPositionNormalTexture point0 = new VertexPositionNormalTexture();
            point0.Position = new Vector3((side * x), 0, -4096 + (side * y));
            point0.Normal = new Vector3(0, 1, 0);
            point0.TextureCoordinate = new Vector2(p0u, p0v);

            // Point 1
            VertexPositionNormalTexture point1 = new VertexPositionNormalTexture();
            point1.Position = new Vector3((side * x), 0, -4096 + (side * (y + 1)));
            point1.Normal = new Vector3(0, 1, 0);
            point1.TextureCoordinate = new Vector2(p1u, p1v);

            // Point 2
            VertexPositionNormalTexture point2 = new VertexPositionNormalTexture();
            point2.Position = new Vector3((side * (x + 1)), 0, -4096 + (side * (y + 1)));
            point2.Normal = new Vector3(0, 1, 0);
            point2.TextureCoordinate = new Vector2(p2u, p2v);

            // Point 3
            VertexPositionNormalTexture point3 = new VertexPositionNormalTexture();
            point3.Position = new Vector3((side * (x + 1)), 0, -4096 + (side * y));
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