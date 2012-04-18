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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics.Collidables;
using BEPUphysics.MathExtensions;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DeepEngine.Core;
using DeepEngine.Daggerfall;
using DeepEngine.Rendering;
using DeepEngine.Utility;
using DeepEngine.World;
#endregion

namespace DeepEngine.Components
{

    /// <summary>
    /// Component for creating scenes from Daggerfall blocks.
    ///  Supports RMB blocks, RDB blocks, and building interiors.
    ///  Adds static mesh properties to physics scene so environment is collidable.
    /// </summary>
    public class DaggerfallBlockComponent : DrawableComponent
    {
        #region Fields

        // Constant strings
        const string unknownBlockError = "Error loading an unknown or unsupported block.";

        // Variables
        string blockName;

        // Static batch
        StaticGeometryBuilder staticGeometry;

        // Physics
        StaticMesh physicsMesh = null;

        // Additional block objects
        List<BlockFlat> blockFlats = new List<BlockFlat>();
        List<BlockLight> blockLights = new List<BlockLight>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets list of stationary block flats.
        /// </summary>
        public List<BlockFlat> BlockFlats
        {
            get { return blockFlats; }
        }

        /// <summary>
        /// Gets list of stationary block lights.
        /// </summary>
        public List<BlockLight> BlockLights
        {
            get { return blockLights; }
        }

        #endregion

        #region Structures

        /// <summary>
        /// A static billboard used for trees, rocks, animals, etc.
        /// </summary>
        public struct BlockFlat
        {
            public bool Dungeon;
            public int Archive;
            public int Record;
            public Vector3 Position;
            public FlatTypes Type;
        }

        /// <summary>
        /// A block light used to illuminate the environment.
        /// </summary>
        public struct BlockLight
        {
            public bool Dungeon;
            public Vector3 Position;
            public float Radius;
            public uint Unknown1;
            public uint Unknown2;
        }

        /// <summary>
        /// A monster living in this block.
        /// </summary>
        private struct BlockMonster
        {
        }

        /// <summary>
        /// An NPC living in this block.
        /// </summary>
        private struct BlockNPC
        {
        }

        /// <summary>
        /// A door the player can open and close.
        /// </summary>
        private struct BlockDoor
        {
        }

        /// <summary>
        /// A small area submerged by water.
        /// </summary>
        private struct BlockWater
        {
        }

        #endregion

        #region Enumerations

        /// <summary>
        /// Types of billboard flats in Daggerfall blocks.
        /// </summary>
        public enum FlatTypes
        {
            /// <summary>Decorative flats.</summary>
            Decorative,
            /// <summary>Non-player characters, such as quest givers and shop keepers.</summary>
            NPC,
            /// <summary>Climate-specific scenery in exterior blocks, such as trees and rocks.</summary>
            ClimateScenery,
            /// <summary>Editor flats, such as markers for quests, random monters, and treasure.</summary>
            Editor = 199,
            /// <summary>Flat is also light-source.</summary>
            Light = 210,
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets names of currently loaded block.
        /// </summary>
        public string BlockName
        {
            get { return blockName; }
        }

        /// <summary>
        /// Gets or sets local transform relative to entity.
        ///  Cannot change matrix of static components.
        /// </summary>
        public override Matrix Matrix
        {
            get { return base.Matrix; }
            set
            {
                base.Matrix = value;
                if (physicsMesh != null)
                {
                    physicsMesh.WorldTransform = new AffineTransform(this.matrix.Translation);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        public DaggerfallBlockComponent(DeepCore core)
            : base(core)
        {
            // Create static geometry builder
            staticGeometry = new StaticGeometryBuilder(core.GraphicsDevice);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets static geometry.
        /// </summary>
        /// <returns>Static geometry builder.</returns>
        public StaticGeometryBuilder GetStaticGeometry()
        {
            return staticGeometry;
        }

        #endregion

        #region DrawableComponent Overrides

        /// <summary>
        /// Called when component should update itself.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last update.</param>
        /// <param name="caller">The entity calling the update.</param>
        public override void Update(TimeSpan elapsedTime, BaseEntity caller)
        {
            // Do nothing if disabled
            if (!enabled)
                return;

            // Ensure component is aligned to physics entity
            if (physicsMesh != null)
                this.matrix = physicsMesh.WorldTransform.Matrix;
        }

        /// <summary>
        /// Draws the component.
        /// </summary>
        /// <param name="caller">Entity calling the draw operation.</param>
        public override void Draw(BaseEntity caller)
        {
            // Do nothing if no static geometry
            if (staticGeometry == null)
                return;

            // Do nothing if no batches or component is disabled
            if (staticGeometry.StaticBatches == null || !enabled)
                return;

            // Calculate world matrix
            Matrix worldMatrix = matrix * caller.Matrix;

            // Set transforms
            core.ModelManager.ModelEffect_World = worldMatrix;
            core.ModelManager.ModelEffect_View = core.ActiveScene.Camera.ViewMatrix;
            core.ModelManager.ModelEffect_Projection = core.ActiveScene.Camera.ProjectionMatrix;

            // Set buffers
            core.GraphicsDevice.SetVertexBuffer(staticGeometry.VertexBuffer);
            core.GraphicsDevice.Indices = staticGeometry.IndexBuffer;

            // Draw batches
            foreach (var item in staticGeometry.StaticBatches)
            {
                // Apply material
                BaseMaterialEffect materialEffect = core.MaterialManager.GetMaterialEffect(item.Key);
                materialEffect.Apply();

                // Render geometry
                foreach (EffectPass pass in materialEffect.Effect.CurrentTechnique.Passes)
                {
                    // Apply effect pass
                    pass.Apply();

                    // Draw primitives
                    core.GraphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        0,
                        0,
                        staticGeometry.VertexBuffer.VertexCount,
                        item.Value.StartIndex,
                        item.Value.PrimitiveCount);
                }
            }
        }

        /// <summary>
        /// Frees resources used by this object when they are no longer needed.
        /// </summary>
        public override void Dispose()
        {
            staticGeometry.Dispose();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads a RMB or RDB block.
        ///  Replaces any previously loaded data.
        /// </summary>
        /// <param name="blockName">Name of block to load.</param>
        /// <param name="climateSettings">Desired climate settings for block.</param>
        /// <param name="scene">Scene to add physics properties.</param>
        /// <param name="groundTiles">True to add ground plane tiles.</param>
        /// <returns>True if successful.</returns>
        public bool LoadBlock(string blockName, DFLocation.ClimateSettings climateSettings, Scene scene, bool groundTiles)
        {
            // Set climate
            core.MaterialManager.ClimateType = climateSettings.ClimateType;

            // Start new builder
            staticGeometry.UnSeal();

            // Load block
            DFBlock blockData = core.BlockManager.GetBlock(blockName);
            switch (blockData.Type)
            {
                case DFBlock.BlockTypes.Rmb:
                    BuildRMB(ref blockData, climateSettings, scene, groundTiles);
                    break;
                case DFBlock.BlockTypes.Rdb:
                    BuildRDB(ref blockData, scene);
                    break;
                default:
                    throw new Exception(unknownBlockError);
            }

            // Store name
            this.blockName = blockName;

            return true;
        }

        /// <summary>
        /// Loads a interior location (such as inside a house or tavern).
        ///  Automatically uses correct climate settings.
        ///  Replaces any previously loaded data.
        /// </summary>
        /// <param name="blockName">Blocks name.</param>
        /// <param name="rmbRecordIndex">Index of interior inside block.</param>
        /// <param name="climateSettings">Desired climate settings for block.</param>
        /// <returns>True if successful.</returns>
        public bool LoadInterior(string blockName, int rmbRecordIndex, DFLocation.ClimateSettings climateSettings)
        {
            return false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Completes static geometry build process.
        ///  Submits static geometry to physics scene.
        /// </summary>
        private void Seal(Scene scene)
        {
            // Seal static geometry
            staticGeometry.ApplyBuilder();
            staticGeometry.Seal();

            // Create static geometry for physics engine
            physicsMesh = new StaticMesh(
                staticGeometry.PhysicsVertices,
                staticGeometry.PhysicsIndices,
                new AffineTransform(this.matrix.Translation));

            // Add static mesh to physics space
            if (scene != null)
                scene.Space.Add(physicsMesh);
        }

        #endregion

        #region Block Building

        /// <summary>
        /// Builds an RMB block.
        /// </summary>
        /// <param name="climateSettings">Desired climate settings for block.</param>
        /// <param name="blockData">Block data.</param>
        /// <param name="scene">Scene to add physics properties.</param>
        /// <param name="groundTiles">True to add ground plane tiles to scene.</param>
        private void BuildRMB(ref DFBlock blockData, DFLocation.ClimateSettings climateSettings, Scene scene, bool groundTiles)
        {
            // Add RMB data
            if (groundTiles) AddRMBGroundTiles(ref blockData);
            AddRMBModels(ref blockData);
            AddRMBMiscModels(ref blockData);
            AddRMBMiscFlats(ref blockData);
            AddRMBSceneryFlats(ref blockData, climateSettings);

            // Finish batch building
            Seal(scene);

            // Set component bounding sphere
            this.BoundingSphere = new BoundingSphere(
                new Vector3(
                    (BlocksFile.RMBDimension / 2) * ModelManager.GlobalScale,
                    0,
                    (BlocksFile.RMBDimension / 2) * ModelManager.GlobalScale),
                 BlocksFile.RMBDimension * ModelManager.GlobalScale);
        }

        /// <summary>
        /// Builds an RDB block.
        /// </summary>
        /// <param name="blockData">Block data.</param>
        /// <param name="scene">Scene to add physics properties.</param>
        private void BuildRDB(ref DFBlock blockData, Scene scene)
        {
            // Add RDB data
            AddRDBObjects(ref blockData);

            // Finish batch building
            Seal(scene);

            // Set component bounding sphere
            this.BoundingSphere = new BoundingSphere(
                new Vector3(
                    (BlocksFile.RDBDimension / 2) * ModelManager.GlobalScale,
                    0,
                    (-(BlocksFile.RDBDimension / 2)) * ModelManager.GlobalScale),
                BlocksFile.RDBDimension * ModelManager.GlobalScale);
        }

        /// <summary>
        /// Loads model data.
        /// </summary>
        /// <param name="id">ID of model to load.</param>
        /// <param name="modelData">Model data out.</param>
        private void LoadModel(uint id, out ModelManager.ModelData modelData)
        {
            // Load model and textures
            modelData = core.ModelManager.GetModelData(id);
        }

        #endregion

        #region RMB Block Building

        /// <summary>
        /// Adds exterior ground tiles to the batch.
        /// </summary>
        /// <param name="blockData">Block data.</param>
        private void AddRMBGroundTiles(ref DFBlock blockData)
        {
            // Make ground slightly lower to minimise depth-fighting on ground aligned polygons
            const float groundHeight = 0f;

            // Corner positions
            Vector3 topLeftPos, topRightPos, bottomLeftPos, bottomRightPos;
            Vector2 topLeftUV, topRightUV, bottomLeftUV, bottomRightUV;

            // Create vertices. These will be updated for each tile based on position and UV orientation.
            VertexPositionNormalTextureBump[] vertices = new VertexPositionNormalTextureBump[4];

            // Create indices. These are the same for every tile.
            int[] indices = new int[] {0, 1, 2, 1, 3, 2};

            // Loop through tiles
            int tileCount = 16;
            float tileDimension = 256.0f * ModelManager.GlobalScale;
            for (int y = 0; y < tileCount; y++)
            {
                for (int x = 0; x < tileCount; x++)
                {
                    // Get source tile data
                    DFBlock.RmbGroundTiles tile = blockData.RmbBlock.FldHeader.GroundData.GroundTiles[x, y];

                    // Set random terrain marker back to grass
                    int textureRecord = (tile.TextureRecord > 55) ? 2 : tile.TextureRecord;

                    // Create material
                    BaseMaterialEffect material = core.ModelManager.CreateModelMaterial(
                        (int)DFLocation.ClimateTextureSet.Exterior_Terrain,
                        textureRecord);
                    material.SamplerState0 = SamplerState.AnisotropicClamp;

                    // Create vertices for this quad
                    topLeftPos = new Vector3(x * tileDimension, groundHeight, y * tileDimension);
                    topRightPos = new Vector3(topLeftPos.X + tileDimension, groundHeight, topLeftPos.Z);
                    bottomLeftPos = new Vector3(topLeftPos.X, groundHeight, topLeftPos.Z + tileDimension);
                    bottomRightPos = new Vector3(topLeftPos.X + tileDimension, groundHeight, topLeftPos.Z + tileDimension);

                    // Set UVs
                    if (tile.IsRotated && !tile.IsFlipped)
                    {
                        // Rotate only
                        topLeftUV = new Vector2(1, 0);
                        topRightUV = new Vector2(1, 1);
                        bottomLeftUV = new Vector2(0, 0);
                        bottomRightUV = new Vector2(0, 1);
                    }
                    else if (tile.IsFlipped && !tile.IsRotated)
                    {
                        // Flip only
                        topLeftUV = new Vector2(1, 1);
                        topRightUV = new Vector2(0, 1);
                        bottomLeftUV = new Vector2(1, 0);
                        bottomRightUV = new Vector2(0, 0);
                    }
                    else if (tile.IsRotated && tile.IsRotated)
                    {
                        // Rotate and flip
                        topLeftUV = new Vector2(0, 1);
                        topRightUV = new Vector2(0, 0);
                        bottomLeftUV = new Vector2(1, 1);
                        bottomRightUV = new Vector2(1, 0);
                    }
                    else
                    {
                        // No rotate or flip
                        topLeftUV = new Vector2(0, 0);
                        topRightUV = new Vector2(1, 0);
                        bottomLeftUV = new Vector2(0, 1);
                        bottomRightUV = new Vector2(1, 1);
                    }

                    // Set vertices
                    vertices[0] = new VertexPositionNormalTextureBump(topLeftPos, Vector3.Up, topLeftUV, Vector3.Zero, Vector3.Zero);
                    vertices[1] = new VertexPositionNormalTextureBump(topRightPos, Vector3.Up, topRightUV, Vector3.Zero, Vector3.Zero);
                    vertices[2] = new VertexPositionNormalTextureBump(bottomLeftPos, Vector3.Up, bottomLeftUV, Vector3.Zero, Vector3.Zero);
                    vertices[3] = new VertexPositionNormalTextureBump(bottomRightPos, Vector3.Up, bottomRightUV, Vector3.Zero, Vector3.Zero);

                    // Add to builder
                    staticGeometry.AddToBuilder(material.ID, vertices, indices, Matrix.Identity);
                }
            }
        }

        /// <summary>
        /// Adds models to the batch.
        /// </summary>
        /// <param name="blockData">Block data.</param>
        private void AddRMBModels(ref DFBlock blockData)
        {
            // Iterate through all subrecords
            float degrees;
            foreach (DFBlock.RmbSubRecord subRecord in blockData.RmbBlock.SubRecords)
            {
                // Get position
                Vector3 position = new Vector3(subRecord.XPos, 0, subRecord.ZPos) * ModelManager.GlobalScale;

                // Create subrecord transform
                Matrix subrecordMatrix = Matrix.Identity;
                degrees = subRecord.YRotation / BlocksFile.RotationDivisor;
                subrecordMatrix *= Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(degrees), 0, 0);
                subrecordMatrix *= Matrix.CreateTranslation(position);

                // Iterate through models in this subrecord
                foreach (DFBlock.RmbBlock3dObjectRecord obj in subRecord.Exterior.Block3dObjectRecords)
                {
                    // Load model data
                    ModelManager.ModelData modelData;
                    LoadModel(obj.ModelIdNum, out modelData);

                    // Get position
                    position = new Vector3(obj.XPos, -obj.YPos, -obj.ZPos) * ModelManager.GlobalScale;

                    // Create model transform
                    Matrix modelMatrix = Matrix.Identity;
                    degrees = obj.YRotation / BlocksFile.RotationDivisor;
                    modelMatrix *= Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(degrees), 0, 0);
                    modelMatrix *= Matrix.CreateTranslation(position);

                    // Add model data to batch builder
                    staticGeometry.AddToBuilder(ref modelData, modelMatrix * subrecordMatrix);
                }
            }
        }

        /// <summary>
        /// Adds miscellaneous RMB models to block node.
        /// </summary>
        /// <param name="block">Block data.</param>
        private void AddRMBMiscModels(ref DFBlock block)
        {
            // Iterate through all misc records
            float degrees;
            foreach (DFBlock.RmbBlock3dObjectRecord obj in block.RmbBlock.Misc3dObjectRecords)
            {
                // Load model data
                ModelManager.ModelData modelData;
                LoadModel(obj.ModelIdNum, out modelData);

                // Get position
                Vector3 position = new Vector3(obj.XPos, -obj.YPos, -obj.ZPos) * ModelManager.GlobalScale;

                // Create model transform
                Matrix modelMatrix = Matrix.Identity;
                degrees = obj.YRotation / BlocksFile.RotationDivisor;
                modelMatrix *= Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(degrees), 0, 0);
                modelMatrix *= Matrix.CreateTranslation(position);

                // Add model data to batch builder
                staticGeometry.AddToBuilder(ref modelData, modelMatrix);
            }
        }

        /// <summary>
        /// Stores miscellaneous flats.
        /// </summary>
        /// <param name="block">Block data.</param>
        private void AddRMBMiscFlats(ref DFBlock block)
        {
            // Add block flats
            foreach (DFBlock.RmbBlockFlatObjectRecord obj in block.RmbBlock.MiscFlatObjectRecords)
            {
                BlockFlat flat = new BlockFlat
                {
                    Dungeon = false,
                    Archive = obj.TextureArchive,
                    Record = obj.TextureRecord,
                    Position = new Vector3(obj.XPos, -obj.YPos, -obj.ZPos) * ModelManager.GlobalScale,
                    Type = GetFlatType(obj.TextureArchive),
                };
                blockFlats.Add(flat);
            }
        }

        /// <summary>
        /// Stores scenery flats.
        /// </summary>
        /// <param name="block">Block data.</param>
        /// <param name="climateSettings">Desired climate settings for block.</param>
        private void AddRMBSceneryFlats(ref DFBlock block, DFLocation.ClimateSettings climateSettings)
        {
            // Add block scenery
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    // Get scenery item
                    DFBlock.RmbGroundScenery scenery =
                        block.RmbBlock.FldHeader.GroundData.GroundScenery[x, y];

                    // Ignore 0 as this appears to be a marker/waypoint of some kind
                    if (scenery.TextureRecord > 0)
                    {
                        BlockFlat flat = new BlockFlat
                        {
                            Dungeon = false,
                            Archive = climateSettings.SceneryArchive,
                            Record = scenery.TextureRecord,
                            Position = new Vector3(x * BlocksFile.TileDimension, 0, y * BlocksFile.TileDimension) * ModelManager.GlobalScale,
                            Type = FlatTypes.ClimateScenery,
                        };
                        blockFlats.Add(flat);
                    }
                }
            }
        }

        #endregion

        #region RDB Block Building

        /// <summary>
        /// Adds RDB block objects.
        /// </summary>
        /// <param name="block">Block data.</param>
        private void AddRDBObjects(ref DFBlock block)
        {
            // Iterate object groups
            foreach (DFBlock.RdbObjectRoot group in block.RdbBlock.ObjectRootList)
            {
                // Skip empty object groups
                if (null == group.RdbObjects)
                    continue;

                // Iterate objects in this group
                foreach (DFBlock.RdbObject obj in group.RdbObjects)
                {
                    // Add by type
                    switch (obj.Type)
                    {
                        case DFBlock.RdbResourceTypes.Model:
                            AddRDBModel(ref block, obj);
                            break;
                        case DFBlock.RdbResourceTypes.Flat:
                            AddRDBFlat(obj);
                            break;
                        case DFBlock.RdbResourceTypes.Light:
                            AddRDBLight(obj);
                            break;
                        default:
                            // Unknown type encounter
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Adds RDB model to the batch.
        /// </summary>
        /// <param name="block">DFBlock.</param>
        /// <param name="obj">RdbObject.</param>
        private void AddRDBModel(ref DFBlock block, DFBlock.RdbObject obj)
        {
            // Get model reference index, desc, and id
            int modelReference = obj.Resources.ModelResource.ModelIndex;
            string modelDescription = block.RdbBlock.ModelReferenceList[modelReference].Description;
            uint modelId = block.RdbBlock.ModelReferenceList[modelReference].ModelIdNum;

            // Get rotation angle for each axis
            float degreesX = obj.Resources.ModelResource.XRotation / BlocksFile.RotationDivisor;
            float degreesY = obj.Resources.ModelResource.YRotation / BlocksFile.RotationDivisor;
            float degreesZ = -obj.Resources.ModelResource.ZRotation / BlocksFile.RotationDivisor;

            // Calcuate position
            Vector3 position = new Vector3(obj.XPos, -obj.YPos, -obj.ZPos) * ModelManager.GlobalScale;

            // Calculate rotation
            Vector3 rotation = new Vector3(
                MathHelper.ToRadians(degreesX),
                MathHelper.ToRadians(degreesY),
                MathHelper.ToRadians(degreesZ));

            // Create transforms
            Matrix rotationX = Matrix.CreateRotationX(rotation.X);
            Matrix rotationY = Matrix.CreateRotationY(rotation.Y);
            Matrix rotationZ = Matrix.CreateRotationZ(rotation.Z);
            Matrix translation = Matrix.CreateTranslation(position);

            // Create model transform
            Matrix modelMatrix = Matrix.Identity;
            Matrix.Multiply(ref modelMatrix, ref rotationY, out modelMatrix);
            Matrix.Multiply(ref modelMatrix, ref rotationX, out modelMatrix);
            Matrix.Multiply(ref modelMatrix, ref rotationZ, out modelMatrix);
            Matrix.Multiply(ref modelMatrix, ref translation, out modelMatrix);

            // Load model data
            ModelManager.ModelData modelData;
            LoadModel(modelId, out modelData);

            // Add model data to batch builder
            staticGeometry.AddToBuilder(ref modelData, modelMatrix);
        }

        /// <summary>
        /// Adds RDB flat.
        /// </summary>
        /// <param name="obj">RdbObject.</param>
        private void AddRDBFlat(DFBlock.RdbObject obj)
        {
            BlockFlat flat = new BlockFlat
            {
                Dungeon = true,
                Archive = obj.Resources.FlatResource.TextureArchive,
                Record = obj.Resources.FlatResource.TextureRecord,
                Position = new Vector3(obj.XPos, -obj.YPos, -obj.ZPos) * ModelManager.GlobalScale,
                Type = GetFlatType(obj.Resources.FlatResource.TextureArchive),
            };
            blockFlats.Add(flat);
        }

        /// <summary>
        /// Adds RDB light source.
        /// </summary>
        /// <param name="obj">RdbObject.</param>
        private void AddRDBLight(DFBlock.RdbObject obj)
        {
            BlockLight light = new BlockLight
            {
                Dungeon = true,
                Position = new Vector3(obj.XPos, -obj.YPos, -obj.ZPos) * ModelManager.GlobalScale,
                Radius = (obj.Resources.LightResource.Radius) * ModelManager.GlobalScale,
                Unknown1 = obj.Resources.LightResource.Unknown1,
                Unknown2 = obj.Resources.LightResource.Unknown2,
            };
            blockLights.Add(light);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets the type of this flat.
        /// </summary>
        /// <param name="textureArchive">Texture archive index.</param>
        /// <returns>Type of flat based on texture archive.</returns>
        private FlatTypes GetFlatType(int textureArchive)
        {
            // Determine flat type
            FlatTypes type;
            if (textureArchive == (int)FlatTypes.Editor)
                type = FlatTypes.Editor;
            else if (textureArchive == (int)FlatTypes.Light)
                type = FlatTypes.Light;
            else
                type = FlatTypes.Decorative;

            return type;
        }

        #endregion

    }

}
