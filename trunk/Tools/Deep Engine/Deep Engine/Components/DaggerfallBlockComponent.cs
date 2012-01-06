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

        // References
        Scene scene;

        // Variables
        string blockName;

        // Static batch
        StaticGeometryBuilder staticGeometry;

        // Physics
        StaticMesh physicsMesh = null;

        #endregion

        #region Structures

        /// <summary>
        /// A block flat texture used for trees, rocks, animals, and other scenery.
        /// </summary>
        private struct BlockFlat
        {
        }

        /// <summary>
        /// A block light used to illuminate the environment.
        /// </summary>
        private struct BlockLight
        {
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
        /// <param name="scene">Scene to add physics properties.</param>
        public DaggerfallBlockComponent(DeepCore core, Scene scene)
            : base(core)
        {
            // Save references
            this.scene = scene;

            // Create static geometry builder
            staticGeometry = new StaticGeometryBuilder(core.GraphicsDevice);
        }

        #endregion

        #region DrawableComponent Overrides

        /// <summary>
        /// Called when component should update itself.
        /// </summary>
        /// <param name="gameTime">GameTime.</param>
        /// <param name="caller">The entity calling the update.</param>
        public override void Update(GameTime gameTime, BaseEntity caller)
        {
            // Do nothing if disabled
            if (!enabled)
                return;

            // Ensure component is aligned to physics entity
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
            Matrix worldMatrix = caller.Matrix * matrix;

            // Set transforms
            core.MaterialManager.DefaultEffect_World = worldMatrix;
            core.MaterialManager.DefaultEffect_View = core.ActiveScene.DeprecatedCamera.View;
            core.MaterialManager.DefaultEffect_Projection = core.ActiveScene.DeprecatedCamera.Projection;

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

                    // Draw batched indexed primitives
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
        /// Gets static geometry.
        /// </summary>
        /// <returns>Static geometry builder.</returns>
        public override Utility.StaticGeometryBuilder GetStaticGeometry()
        {
            return staticGeometry;
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
        /// <returns>True if successful.</returns>
        public bool LoadBlock(string blockName, DFLocation.ClimateSettings climateSettings)
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
                    BuildRMB(ref blockData);
                    break;
                case DFBlock.BlockTypes.Rdb:
                    BuildRDB(ref blockData);
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
        private void Seal()
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
            scene.Space.Add(physicsMesh);
        }

        #endregion

        #region Block Building

        /// <summary>
        /// Builds an RMB block.
        /// </summary>
        /// <param name="blockData">Block data.</param>
        private void BuildRMB(ref DFBlock blockData)
        {
            // Add RMB data
            AddRMBGroundTiles(ref blockData);
            AddRMBModels(ref blockData);
            AddRMBMiscModels(ref blockData);
            //AddRMBSceneryFlats(ref blockData);

            // Finish batch building
            Seal();

            // Set component bounding sphere
            this.BoundingSphere = new BoundingSphere(
                new Vector3(BlocksFile.RMBDimension / 2, 0, BlocksFile.RMBDimension / 2),
                BlocksFile.RMBDimension);

            //AddRMBMiscFlats(ref block, blockNode);
        }

        /// <summary>
        /// Builds an RDB block.
        /// </summary>
        /// <param name="blockData">Block data.</param>
        private void BuildRDB(ref DFBlock blockData)
        {
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
            const float groundHeight = -2f;

            // Corner positions
            Vector3 topLeftPos, topRightPos, bottomLeftPos, bottomRightPos;
            Vector2 topLeftUV, topRightUV, bottomLeftUV, bottomRightUV;

            // Create vertices. These will be updated for each tile based on position and UV orientation.
            VertexPositionNormalTextureBump[] vertices = new VertexPositionNormalTextureBump[4];

            // Create indices. These are the same for every tile.
            int[] indices = new int[] {0, 1, 2, 1, 3, 2};

            // Loop through tiles
            const int tileCount = 16;
            const float tileDimension = 256.0f;
            for (int y = 0; y < tileCount; y++)
            {
                for (int x = 0; x < tileCount; x++)
                {
                    // Get source tile data
                    DFBlock.RmbGroundTiles tile = blockData.RmbBlock.FldHeader.GroundData.GroundTiles[x, y];

                    // Set random terrain marker back to grass
                    int textureRecord = (tile.TextureRecord > 55) ? 2 : tile.TextureRecord;

                    // Create default material
                    BaseMaterialEffect materialEffect = core.MaterialManager.CreateDefaultEffect(
                        (int)DFLocation.ClimateTextureSet.Exterior_Terrain,
                        textureRecord);
                    materialEffect.SamplerState0 = SamplerState.AnisotropicClamp;

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
                    staticGeometry.AddToBuilder(materialEffect.ID, vertices, indices, Matrix.Identity);
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
                // Create subrecord transform
                Matrix subrecordMatrix = Matrix.Identity;
                degrees = subRecord.YRotation / BlocksFile.RotationDivisor;
                subrecordMatrix *= Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(degrees), 0, 0);
                subrecordMatrix *= Matrix.CreateTranslation(subRecord.XPos, 0, subRecord.ZPos);

                // Iterate through models in this subrecord
                foreach (DFBlock.RmbBlock3dObjectRecord obj in subRecord.Exterior.Block3dObjectRecords)
                {
                    // Load model data
                    ModelManager.ModelData modelData;
                    LoadModel(obj.ModelIdNum, out modelData);

                    // Create model transform
                    Matrix modelMatrix = Matrix.Identity;
                    degrees = obj.YRotation / BlocksFile.RotationDivisor;
                    modelMatrix *= Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(degrees), 0, 0);
                    modelMatrix *= Matrix.CreateTranslation(obj.XPos, -obj.YPos, -obj.ZPos);

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

                // Create model transform
                Matrix modelMatrix = Matrix.Identity;
                degrees = obj.YRotation / BlocksFile.RotationDivisor;
                modelMatrix *= Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(degrees), 0, 0);
                modelMatrix *= Matrix.CreateTranslation(obj.XPos, -obj.YPos, -obj.ZPos);

                // Add model data to batch builder
                staticGeometry.AddToBuilder(ref modelData, modelMatrix);
            }
        }

        #endregion

    }

}
