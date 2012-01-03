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
    /// A drawable Daggerfall block. Can be attached at runtime independently of content pipeline.
    ///  Supports exterior (RMB) and dungeon (RDB) blocks.
    /// </summary>
    public class NativeBlockComponent : DrawableComponent
    {
        #region Fields

        // Constant strings
        const string unknownBlockError = "Error loading an unknown or unsupported block.";

        // Variables
        string blockName;

        // Static batch
        StaticGeometryBuilder batches;

        // Effects
        Effect renderGeometryEffect;

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

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="blockName">Name of block to load.</param>
        public NativeBlockComponent(DeepCore core, string blockName)
            : base(core)
        {
            // Load effect
            renderGeometryEffect = core.ContentManager.Load<Effect>("Effects/RenderGeometry");

            // Create builder
            batches = new StaticGeometryBuilder(core.GraphicsDevice);

            // Load block
            LoadBlock(blockName);
        }

        #endregion

        #region DrawableComponent Overrides

        /// <summary>
        /// Draws the component.
        /// </summary>
        /// <param name="caller">Entity calling the draw operation.</param>
        public override void Draw(BaseEntity caller)
        {
            // Do nothing if component is disabled
            if (!enabled)
                return;

            // Calculate world matrix
            Matrix worldMatrix = caller.Matrix * matrix;

            // Setup effect
            renderGeometryEffect.Parameters["World"].SetValue(worldMatrix);
            renderGeometryEffect.Parameters["View"].SetValue(core.ActiveScene.DeprecatedCamera.View);
            renderGeometryEffect.Parameters["Projection"].SetValue(core.ActiveScene.DeprecatedCamera.Projection);

            // Draw batches
            //batches.Draw(core.MaterialManager, renderGeometryEffect);
        }

        /// <summary>
        /// Gets static geometry.
        /// </summary>
        /// <param name="applyBuilder">Request to apply builder before completion. Caller may only require geometry temporarily, so this optional.</param>
        /// <param name="cleanUpLocalContent">Request to clean up local copies of drawable content after being made static.</param>
        /// <returns>Static geometry builder.</returns>
        public override Utility.StaticGeometryBuilder GetStaticGeometry(bool applyBuilder, bool cleanUpLocalContent)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads a new block to display with this component.
        ///  Replaces any previously loaded block.
        /// </summary>
        /// <param name="blockName">Name of block to load.</param>
        /// <returns>True if successful.</returns>
        public bool LoadBlock(string blockName)
        {
            try
            {
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            // Store name
            this.blockName = blockName;

            return true;
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
            AddRMBGroundTile(ref blockData);
            AddRMBModels(ref blockData);

            // Finish batch building
            batches.ApplyBuilder();

            //AddRMBModels(ref block, blockNode);
            //AddRMBMiscModels(ref block, blockNode);
            //AddRMBGroundPlane(ref block, blockNode, climate);
            //AddRMBMiscFlats(ref block, blockNode);
            //AddRMBSceneryFlats(ref block, blockNode, climate.SceneryArchive);
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
            for (int i = 0; i < modelData.SubMeshes.Length; i++)
            {
                // Set flags
                MaterialManager.TextureCreateFlags flags =
                    MaterialManager.TextureCreateFlags.ApplyClimate |
                    MaterialManager.TextureCreateFlags.MipMaps |
                    MaterialManager.TextureCreateFlags.PowerOfTwo;

                // Set extended alpha flags
                flags |= MaterialManager.TextureCreateFlags.ExtendedAlpha;

                // Load texture
                modelData.SubMeshes[i].TextureKey = core.MaterialManager.LoadTexture(
                    modelData.DFMesh.SubMeshes[i].TextureArchive,
                    modelData.DFMesh.SubMeshes[i].TextureRecord,
                    flags);
            }
        }

        #endregion

        #region RMB Block Building

        /// <summary>
        /// Adds exterior ground tile to the batch.
        /// </summary>
        /// <param name="blockData">Block data.</param>
        private void AddRMBGroundTile(ref DFBlock blockData)
        {
            StaticGeometryBuilder.BatchData batchData;

            // Just build a simple grass tile for now

            int textureKey = core.MaterialManager.LoadTexture(302, 2, MaterialManager.TextureCreateFlags.MipMaps | MaterialManager.TextureCreateFlags.ExtendedAlpha);
            batchData.Vertices = new List<VertexPositionNormalTextureBump>(1024);
            batchData.Indices = new List<int>(1536);

            int currentVertex;
            Vector3 topLeft, topRight, bottomLeft, bottomRight;

            Vector2 topLeftUV = new Vector2(0, 0);
            Vector2 topRightUV = new Vector2(1, 0);
            Vector2 bottomLeftUV = new Vector2(0, 1);
            Vector2 bottomRightUV = new Vector2(1, 1);

            const int tileCount = 16;
            const float tileDimension = 256.0f;
            for (int y = 0; y < tileCount; y++)
            {
                for (int x = 0; x < tileCount; x++)
                {
                    // Create vertices for this quad
                    topLeft = new Vector3(x * tileDimension, 0, y * tileDimension);
                    topRight = new Vector3(topLeft.X + tileDimension, 0, topLeft.Z);
                    bottomLeft = new Vector3(topLeft.X, 0, topLeft.Z + tileDimension);
                    bottomRight = new Vector3(topLeft.X + tileDimension, 0, topLeft.Z + tileDimension);

                    // Get current vertex
                    currentVertex = batchData.Vertices.Count;

                    // Add vertices for quad
                    batchData.Vertices.Add(new VertexPositionNormalTextureBump(topLeft, Vector3.Up, topLeftUV, Vector3.Zero, Vector3.Zero));
                    batchData.Vertices.Add(new VertexPositionNormalTextureBump(topRight, Vector3.Up, topRightUV, Vector3.Zero, Vector3.Zero));
                    batchData.Vertices.Add(new VertexPositionNormalTextureBump(bottomLeft, Vector3.Up, bottomLeftUV, Vector3.Zero, Vector3.Zero));
                    batchData.Vertices.Add(new VertexPositionNormalTextureBump(bottomRight, Vector3.Up, bottomRightUV, Vector3.Zero, Vector3.Zero));

                    // Add first face of quad
                    batchData.Indices.Add(currentVertex + 0);
                    batchData.Indices.Add(currentVertex + 1);
                    batchData.Indices.Add(currentVertex + 2);

                    // Add second face of quad
                    batchData.Indices.Add(currentVertex + 1);
                    batchData.Indices.Add(currentVertex + 3);
                    batchData.Indices.Add(currentVertex + 2);
                }
            }

            // Add to builder
            batches.AddToBuilder(textureKey, batchData);
        }

        /// <summary>
        /// Adds models to the batch.
        /// </summary>
        /// <param name="blockData">Block data.</param>
        private void AddRMBModels(ref DFBlock blockData)
        {
            // Constants
            const float rotationDivisor = 5.68888888888889f;

            // Iterate through all subrecords
            float degrees;
            foreach (DFBlock.RmbSubRecord subRecord in blockData.RmbBlock.SubRecords)
            {
                // Create subrecord transform
                Matrix subrecordMatrix = Matrix.Identity;
                degrees = subRecord.YRotation / rotationDivisor;
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
                    degrees = obj.YRotation / rotationDivisor;
                    modelMatrix *= Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(degrees), 0, 0);
                    modelMatrix *= Matrix.CreateTranslation(obj.XPos, -obj.YPos, -obj.ZPos);

                    // Add model data to batch builder
                    batches.AddToBuilder(ref modelData, subrecordMatrix * modelMatrix);
                }
            }
        }

        #endregion

    }

}
