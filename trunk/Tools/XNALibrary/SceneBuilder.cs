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
using System.Threading;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace XNALibrary
{

    /// <summary>
    /// Helper class to load Daggerfall content and build compound scene nodes.
    /// </summary>
    public class SceneBuilder
    {
        #region Class Variables

        // Content managers
        private TextureManager textureManager = null;
        private ModelManager modelManager = null;
        private BlocksFile blocksFile = null;
        private MapsFile mapsFile = null;

        // Constants
        private const float rotationDivisor = 5.68888888888889f;
        private const float scaleDivisor = 256f;
        private const float rmbSide = 4096f;
        private const float rdbSide = 2048f;
        private const float tileSide = 256f;
        private const int groundHeight = -1;
        private const int defaultWorldClimate = 231;

        // Action link dictionary
        private Dictionary<int, ActionLink> actionLinkDict;

        #endregion

        #region Class Structures

        /// <summary>
        /// Links actions to scene nodes.
        /// </summary>
        private struct ActionLink
        {
            public ModelNode node;
            public int nextKey;
            public int prevKey;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets TextureManager to use when loading textures.
        /// </summary>
        public TextureManager TextureManager
        {
            get { return textureManager; }
            set { textureManager = value; }
        }

        /// <summary>
        /// Gets or sets ModelManager to use when loading models.
        /// </summary>
        public ModelManager ModelManager
        {
            get { return modelManager; }
            set { modelManager = value; }
        }

        /// <summary>
        /// Gets or sets BlocksFile to use when loading blocks.
        /// </summary>
        public BlocksFile BlocksFile
        {
            get { return blocksFile; }
            set { blocksFile = value; }
        }

        /// <summary>
        /// Gets or sets MapsFile to use when loading map layouts.
        /// </summary>
        public MapsFile MapsFile
        {
            get { return mapsFile; }
            set { mapsFile = value; }
        }

        #endregion

        #region Static Properties

        /// <summary>
        /// Gets default world climate.
        /// </summary>
        static public int DefaultWorldClimate
        {
            get { return defaultWorldClimate; }
        }

        /// <summary>
        /// Gets RMB side length.
        /// </summary>
        static public float RMBSide
        {
            get { return rmbSide; }
        }

        /// <summary>
        /// Gets RDB side length.
        /// </summary>
        static public float RDBSide
        {
            get { return rdbSide; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor. Accepts GraphicsDevice and Arena2 path.
        ///  Creates own content manager objects.
        /// </summary>
        /// <param name="graphicsDevice">GraphicsDevice.</param>
        /// <param name="arena2Path">Path to Arena2 folder.</param>
        public SceneBuilder(
            GraphicsDevice graphicsDevice,
            string arena2Path)
        {
            // Create managers
            this.textureManager = new TextureManager(graphicsDevice, arena2Path);
            this.modelManager = new ModelManager(graphicsDevice, arena2Path);
            this.blocksFile = new BlocksFile(
                Path.Combine(arena2Path, BlocksFile.Filename),
                FileUsage.UseDisk,
                true);
            this.mapsFile = new MapsFile(
                Path.Combine(arena2Path, MapsFile.Filename),
                FileUsage.UseDisk,
                true);

            // Create action link dictionary
            actionLinkDict = new Dictionary<int, ActionLink>();
        }

        /// <summary>
        /// Constructor. Accepts pre-created TextureManager, ModelManager,
        ///  BlocksFile, and MapsFile objects.
        ///  These content managers cannot be NULL and must
        ///  be configured ready for use.
        /// </summary>
        /// <param name="textureManager">TextureManager.</param>
        /// <param name="modelManager">ModelManager</param>
        /// <param name="blocksFile">BlocksFile.</param>
        /// <param name="mapsFile">MapsFile.</param>
        public SceneBuilder(
            TextureManager textureManager,
            ModelManager modelManager,
            BlocksFile blocksFile,
            MapsFile mapsFile)
        {
            // Check managers non-null
            if (textureManager == null ||
                modelManager == null ||
                blocksFile == null ||
                mapsFile == null)
            {
                throw new Exception(
                    "One or more content managers are NULL.");
            }

            // Store
            this.textureManager = textureManager;
            this.modelManager = modelManager;
            this.blocksFile = blocksFile;
            this.mapsFile = mapsFile;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a new model node.
        /// </summary>
        /// <param name="id">ModelID</param>
        /// <returns>ModelNode.</returns>
        public ModelNode CreateModelNode(uint id)
        {
            // Load model
            ModelManager.ModelData model;
            if (!LoadDaggerfallModel(id, out model))
                return null;
            else
                return new ModelNode(model);
        }

        /// <summary>
        /// Creates a new block node.
        /// </summary>
        /// <param name="name">Block name.</param>
        /// <param name="climate">Climate settings.</param>
        /// <param name="clearGroundTextures">Clear ground plane texture dictionary.</param>
        /// <returns>BlockNode.</returns>
        public BlockNode CreateBlockNode(string name, DFLocation.ClimateSettings? climate, bool clearGroundTextures)
        {
            // Load block
            DFBlock block;
            if (!LoadDaggerfallBlock(name, out block))
                return null;

            // Set default world climate
            if (climate == null)
                climate = MapsFile.GetWorldClimateSettings(defaultWorldClimate);

            // Reset ground plane texture cache
            if (clearGroundTextures)
                textureManager.ClearGroundTextures();
            
            // Build node
            BlockNode node = null;
            switch (block.Type)
            {
                case DFBlock.BlockTypes.Rmb:
                    textureManager.ClimateType = climate.Value.ClimateType;
                    node = BuildRMBNode(ref block, climate.Value);
                    break;
                case DFBlock.BlockTypes.Rdb:
                    textureManager.ClimateType = DFLocation.ClimateBaseType.None;
                    node = BuildRDBNode(ref block);
                    break;
            }

            return node;
        }

        /// <summary>
        /// Creates a new exterior location node.
        /// </summary>
        /// <param name="regionName">Region name.</param>
        /// <param name="locationName">Location name.</param>
        /// <returns>LocationNode.</returns>
        public LocationNode CreateExteriorLocationNode(string regionName, string locationName)
        {
            // Get location
            DFLocation location;
            if (!LoadDaggerfallLocation(
                regionName,
                locationName,
                out location))
            {
                return null;
            }

            // Reset ground plane texture cache
            textureManager.ClearGroundTextures();

            // Create location node
            LocationNode locationNode = new LocationNode(location);

            // Get dimensions of exterior location array
            int width = location.Exterior.ExteriorData.Width;
            int height = location.Exterior.ExteriorData.Height;

            // Build exterior node from blocks
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Get final block name
                    string name = blocksFile.CheckName(
                        mapsFile.GetRmbBlockName(ref location, x, y));

                    // Create block position data
                    SceneNode blockNode = CreateBlockNode(name, location.Climate, false);
                    blockNode.Position = new Vector3(
                        x * rmbSide,
                        0f,
                        -(y * rmbSide));

                    // Add block to location node
                    locationNode.Add(blockNode);
                }
            }

            return locationNode;
        }

        /// <summary>
        /// Creates a new dungeon location node.
        /// </summary>
        /// <param name="regionName">Region name.</param>
        /// <param name="locationName">Location name.</param>
        /// <returns>LocationNode.</returns>
        public LocationNode CreateDungeonLocationNode(string regionName, string locationName)
        {
            // Get location
            DFLocation location;
            if (!LoadDaggerfallLocation(
                regionName,
                locationName,
                out location))
            {
                return null;
            }

            // Exit if location does not have a dungeon
            if (!location.HasDungeon)
                return null;

            // Create location node
            LocationNode locationNode = new LocationNode(location);

            // Create dungeon layout
            foreach (var block in location.Dungeon.Blocks)
            {
                // TODO: Handle duplicate block coordinates (e.g. Orsinium)

                // Create block position data
                SceneNode blockNode = CreateBlockNode(block.BlockName, null, false);
                blockNode.Position = new Vector3(
                    block.X * rdbSide,
                    0f,
                    -(block.Z * rdbSide));

                // Add block to location node
                locationNode.Add(blockNode);
            }

            return locationNode;
        }

        #endregion

        #region Content Loading

        /// <summary>
        /// Loads a Daggerfall model and any textures required.
        ///  Will use whatever climate is currently set in texture manager.
        /// </summary>
        /// <param name="id">ID of model.</param>
        /// <param name="model">ModelManager.ModelData.</param>
        /// <returns>True if successful.</returns>
        private bool LoadDaggerfallModel(uint id, out ModelManager.ModelData model)
        {
            try
            {
                // Load model and textures
                model = modelManager.GetModelData(id);
                for (int i = 0; i < model.SubMeshes.Length; i++)
                {
                    // Load texture
                    model.SubMeshes[i].TextureKey = textureManager.LoadTexture(
                        model.DFMesh.SubMeshes[i].TextureArchive,
                        model.DFMesh.SubMeshes[i].TextureRecord,
                        TextureManager.TextureCreateFlags.ApplyClimate |
                        TextureManager.TextureCreateFlags.MipMaps |
                        TextureManager.TextureCreateFlags.PowerOfTwo);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                model = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loads a Daggerfall block.
        /// </summary>
        /// <param name="name">Name of block.</param>
        /// <param name="block">DFBlock.</param>
        /// <returns>True if successful.</returns>
        private bool LoadDaggerfallBlock(string name, out DFBlock block)
        {
            try
            {
                // Load block
                block = blocksFile.GetBlock(name);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                block = new DFBlock();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loads a Daggerfall flat (billboard).
        /// </summary>
        /// <param name="textureArchive">Texture archive index.</param>
        /// <param name="textureRecord">Texture record index.</param>
        /// <param name="textureFlags">Texture create flags.</param>
        /// <param name="textureKey">Texture key.</param>
        /// <param name="startSize">Start size before scaling.</param>
        /// <param name="finalSize">Final size after scaling.</param>
        /// <returns>True if successful.</returns>
        private bool LoadDaggerfallFlat(
            int textureArchive,
            int textureRecord,
            TextureManager.TextureCreateFlags textureFlags,
            out int textureKey,
            out Vector2 startSize,
            out Vector2 finalSize)
        {
            try
            {
                // Get path to texture file
                string path = Path.Combine(
                    textureManager.Arena2Path,
                    TextureFile.IndexToFileName(textureArchive));

                // Get size and scale of this texture
                System.Drawing.Size size = TextureFile.QuickSize(path, textureRecord);
                System.Drawing.Size scale = TextureFile.QuickScale(path, textureRecord);

                // Set start size
                startSize.X = size.Width;
                startSize.Y = size.Height;

                // Apply scale
                int xChange = (int)(size.Width * (scale.Width / scaleDivisor));
                int yChange = (int)(size.Height * (scale.Height / scaleDivisor));
                finalSize.X = size.Width + xChange;
                finalSize.Y = size.Height + yChange;

                // Load texture
                textureKey = textureManager.LoadTexture(
                    textureArchive,
                    textureRecord,
                    textureFlags);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                textureKey = -1;
                startSize.X = 0;
                startSize.Y = 0;
                finalSize.X = 0;
                finalSize.Y = 0;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loads a Daggerfall location.
        /// </summary>
        /// <param name="regionName">Region name.</param>
        /// <param name="locationName">Location name.</param>
        /// <param name="location">DFLocation.</param>
        /// <returns>True if successful.</returns>
        private bool LoadDaggerfallLocation(
            string regionName,
            string locationName,
            out DFLocation location)
        {
            try
            {
                // Get location
                location = mapsFile.GetLocation(regionName, locationName);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                location = new DFLocation();
                return false;
            }

            return true;
        }

        #endregion

        #region RMB Block Building

        /// <summary>
        /// Constructs a scene node from the specified RMB block.
        /// </summary>
        /// <param name="block">DFBlock.</param>
        /// <param name="climate">Climate settings.</param>
        /// <returns>BlockNode.</returns>
        private BlockNode BuildRMBNode(ref DFBlock block, DFLocation.ClimateSettings climate)
        {
            // Create parent block node
            BlockNode blockNode = new BlockNode(block);

            // Add child nodes
            AddRMBModels(ref block, blockNode);
            AddRMBMiscModels(ref block, blockNode);
            AddRMBGroundPlane(ref block, blockNode, climate);
            AddRMBMiscFlats(ref block, blockNode);
            AddRMBSceneryFlats(ref block, blockNode, climate.SceneryArchive);

            return blockNode;
        }

        /// <summary>
        /// Adds RMB models to block node.
        /// </summary>
        /// <param name="block">DFBlock.</param>
        /// <param name="blockNode">BlockNode.</param>
        private void AddRMBModels(ref DFBlock block, BlockNode blockNode)
        {
            // Iterate through all subrecords
            float degrees;
            foreach (DFBlock.RmbSubRecord subRecord in block.RmbBlock.SubRecords)
            {
                // Create subrecord node
                SceneNode subrecordNode = new SceneNode();
                degrees = subRecord.YRotation / rotationDivisor;
                subrecordNode.Position = new Vector3(subRecord.XPos, 0f, -rmbSide + subRecord.ZPos);
                subrecordNode.Rotation = new Vector3(0f, MathHelper.ToRadians(degrees), 0f);
                blockNode.Add(subrecordNode);

                // Iterate through models in this subrecord
                foreach (DFBlock.RmbBlock3dObjectRecord obj in subRecord.Exterior.Block3dObjectRecords)
                {
                    // Create model node
                    ModelNode modelNode = CreateModelNode(obj.ModelIdNum);
                    degrees = obj.YRotation / rotationDivisor;
                    modelNode.Position = new Vector3(obj.XPos, -obj.YPos, -obj.ZPos);
                    modelNode.Rotation = new Vector3(0f, MathHelper.ToRadians(degrees), 0f);
                    subrecordNode.Add(modelNode);
                }
            }
        }

        /// <summary>
        /// Adds miscellaneous RMB models to block node.
        /// </summary>
        /// <param name="block">DFBlock</param>
        /// <param name="blockNode">BlockNode.</param>
        private void AddRMBMiscModels(ref DFBlock block, BlockNode blockNode)
        {
            // Iterate through all misc records
            float degrees;
            foreach (DFBlock.RmbBlock3dObjectRecord obj in block.RmbBlock.Misc3dObjectRecords)
            {
                // Create misc record node
                SceneNode miscNode = CreateModelNode(obj.ModelIdNum);
                degrees = obj.YRotation / rotationDivisor;
                miscNode.Position = new Vector3(obj.XPos, -obj.YPos, -rmbSide + -obj.ZPos);
                miscNode.Rotation = new Vector3(0f, MathHelper.ToRadians(degrees), 0f);
                blockNode.Add(miscNode);
            }
        }

        /// <summary>
        /// Adds RMB ground plane to block node.
        /// </summary>
        /// <param name="block">DFBlock</param>
        /// <param name="climate">ClimateSettings.</param>
        private void AddRMBGroundPlane(ref DFBlock block, BlockNode blockNode, DFLocation.ClimateSettings climate)
        {
            // Add ground plane node
            GroundPlaneNode groundNode = new GroundPlaneNode(
                textureManager.GraphicsDevice,
                textureManager.LoadGroundPlaneTexture(ref block, climate));
            groundNode.Position = new Vector3(0f, groundHeight, 0f);
            blockNode.Add(groundNode);
        }

        /// <summary>
        /// Adds miscellaneous RMB flats to block node.
        /// </summary>
        /// <param name="block">DFBlock</param>
        /// <param name="blockNode">BlockNode.</param>
        private void AddRMBMiscFlats(ref DFBlock block, BlockNode blockNode)
        {
            // Iterate through all misc flat records
            foreach (DFBlock.RmbBlockFlatObjectRecord obj in block.RmbBlock.MiscFlatObjectRecords)
            {
                // Get flat type
                BillboardNode.BillboardType billboardType =
                    GetFlatType(obj.TextureArchive);

                // Flags
                TextureManager.TextureCreateFlags flags =
                    TextureManager.TextureCreateFlags.Dilate |
                    TextureManager.TextureCreateFlags.PreMultiplyAlpha;
                if (Core.GraphicsProfile == GraphicsProfile.HiDef)
                    flags |= TextureManager.TextureCreateFlags.MipMaps;

                // Load flat
                int textureKey;
                Vector2 startSize;
                Vector2 finalSize;
                if (true == LoadDaggerfallFlat(
                    obj.TextureArchive,
                    obj.TextureRecord,
                    flags,
                    out textureKey,
                    out startSize,
                    out finalSize))
                {
                    // Calcuate position
                    Vector3 position = new Vector3(
                        obj.XPos,
                        -obj.YPos + (finalSize.Y / 2) - 4,
                        -rmbSide + -obj.ZPos);

                    // Create billboard node
                    BillboardNode billboardNode = new BillboardNode(
                        billboardType,
                        textureKey,
                        finalSize);
                    billboardNode.Position = position;
                    blockNode.Add(billboardNode);

                    // Add point light node
                    if (billboardType == BillboardNode.BillboardType.Light)
                        AddPointLight(position, blockNode);
                }
            }
        }

        /// <summary>
        /// Adds RMB scenery flats to block node.
        /// </summary>
        /// <param name="block">DFBlock</param>
        /// <param name="blockNode">BlockNode.</param>
        /// <param name="sceneryArchive">Scenery texture archive index.</param>
        private void AddRMBSceneryFlats(ref DFBlock block, BlockNode blockNode, int sceneryArchive)
        {
            // Flags
            TextureManager.TextureCreateFlags flags =
                TextureManager.TextureCreateFlags.Dilate |
                TextureManager.TextureCreateFlags.PreMultiplyAlpha;
            if (Core.GraphicsProfile == GraphicsProfile.HiDef)
                flags |= TextureManager.TextureCreateFlags.MipMaps;

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
                        // Load flat
                        int textureKey;
                        Vector2 startSize;
                        Vector2 finalSize;
                        if (true == LoadDaggerfallFlat(
                            sceneryArchive,
                            scenery.TextureRecord,
                            flags,
                            out textureKey,
                            out startSize,
                            out finalSize))
                        {
                            // Calcuate position
                            Vector3 position = new Vector3(
                                x * tileSide,
                                (finalSize.Y / 2) - 4,
                                -rmbSide + y * tileSide);

                            // Create billboard node
                            BillboardNode billboardNode = new BillboardNode(
                                BillboardNode.BillboardType.ClimateScenery,
                                textureKey,
                                finalSize);
                            billboardNode.Position = position;
                            blockNode.Add(billboardNode);
                        }
                    }
                }
            }
        }

        #endregion

        #region RDB Block Building

        /// <summary>
        /// Constructs a scene node from the specified RDB block.
        /// </summary>
        /// <param name="block">DFBlock.</param>
        /// <returns>BlockNode.</returns>
        private BlockNode BuildRDBNode(ref DFBlock block)
        {
            // Create parent block node
            BlockNode blockNode = new BlockNode(block);

            // Clear action link dictionary
            actionLinkDict.Clear();

            // Iterate through object groups
            int groupIndex = 0;
            foreach (DFBlock.RdbObjectRoot group in block.RdbBlock.ObjectRootList)
            {
                // Skip empty object groups
                if (null == group.RdbObjects)
                {
                    groupIndex++;
                    continue;
                }

                // Iterate through objects in this group
                foreach (DFBlock.RdbObject obj in group.RdbObjects)
                {
                    // Filter by type
                    switch (obj.Type)
                    {
                        case DFBlock.RdbResourceTypes.Model:
                            AddRDBModel(ref block, obj, blockNode, groupIndex);
                            break;
                        case DFBlock.RdbResourceTypes.Flat:
                            AddRDBFlat(obj, blockNode);
                            break;
                        default:
                            // Only drawing models and flats for now
                            break;
                    }
                }

                // Increment group index
                groupIndex++;
            }

            // Link action nodes
            LinkActionNodes();

            return blockNode;
        }

        /// <summary>
        /// Adds RDB model to block node.
        /// </summary>
        /// <param name="block">DFBlock.</param>
        /// <param name="obj">RdbObject.</param>
        /// <param name="blockNode">BlockNode.</param>
        /// <param name="groupIndex">Group index.</param>
        private void AddRDBModel(ref DFBlock block, DFBlock.RdbObject obj, BlockNode blockNode, int groupIndex)
        {
            // Get model reference index, desc, and id
            int modelReference = obj.Resources.ModelResource.ModelIndex;
            string modelDescription = block.RdbBlock.ModelReferenceList[modelReference].Description;
            uint modelId = block.RdbBlock.ModelReferenceList[modelReference].ModelIdNum;

            // Get rotation angle for each axis
            float degreesX = obj.Resources.ModelResource.XRotation / rotationDivisor;
            float degreesY = obj.Resources.ModelResource.YRotation / rotationDivisor;
            float degreesZ = -obj.Resources.ModelResource.ZRotation / rotationDivisor;

            // Calcuate position
            Vector3 position = new Vector3(
                obj.XPos,
                -obj.YPos,
                -obj.ZPos);

            // Calculate rotation
            Vector3 rotation = new Vector3(
                MathHelper.ToRadians(degreesX),
                MathHelper.ToRadians(degreesY),
                MathHelper.ToRadians(degreesZ));

            // Create model node
            ModelNode modelNode = CreateModelNode(modelId);
            modelNode.Position = position;
            modelNode.Rotation = rotation;
            blockNode.Add(modelNode);

            // Setup actions for this node
            CreateModelAction(
                obj.Resources.ModelResource.ActionResource,
                modelNode,
                modelDescription,
                groupIndex,
                obj.Index);
        }

        /// <summary>
        /// Adds RDB flat to scene node.
        /// </summary>
        /// <param name="obj">RdbObject.</param>
        /// <param name="blockNode">BlockNode.</param>
        private void AddRDBFlat(DFBlock.RdbObject obj, BlockNode blockNode)
        {
            // Get flat type
            BillboardNode.BillboardType billboardType =
                GetFlatType(obj.Resources.FlatResource.TextureArchive);

            // Add light if needed

            // Load flat
            int textureKey;
            Vector2 startSize;
            Vector2 finalSize;
            if (true == LoadDaggerfallFlat(
                obj.Resources.FlatResource.TextureArchive,
                obj.Resources.FlatResource.TextureRecord,
                TextureManager.TextureCreateFlags.Dilate |
                TextureManager.TextureCreateFlags.PreMultiplyAlpha,
                out textureKey,
                out startSize,
                out finalSize))
            {
                // Foliage (TEXTURE.500 and up) do not seem to use scaling
                // in dungeons. Revert scaling.
                if (obj.Resources.FlatResource.TextureArchive > 499)
                {
                    finalSize = startSize;
                }

                // Calcuate position
                Vector3 position = new Vector3(
                    obj.XPos,
                    -obj.YPos,
                    -obj.ZPos);

                // Create billboard node
                BillboardNode billboardNode = new BillboardNode(
                    billboardType,
                    textureKey,
                    finalSize);
                billboardNode.Position = position;
                blockNode.Add(billboardNode);
            }
        }

        #endregion

        #region Light Building

        /// <summary>
        /// Adds a point light to the scene.
        /// </summary>
        /// <param name="position">Position.</param>
        /// <param name="blockNode">Parent node.</param>
        private void AddPointLight(Vector3 position, BlockNode blockNode)
        {
            // Create light node
            PointLightNode pointLightNode = new PointLightNode();
            pointLightNode.Position = position;
            blockNode.Add(pointLightNode);
        }

        #endregion

        #region Action Building

        /// <summary>
        /// Prepares an action record for use.
        /// </summary>
        /// <param name="action">DFBlock.RdbActionResource</param>
        /// <param name="modelNode">ModelNode</param>
        /// <param name="description">Description of model.</param>
        /// <param name="groupIndex">RDB group index.</param>
        /// <param name="modelIndex">RDB model index.</param>
        private void CreateModelAction(DFBlock.RdbActionResource action, ModelNode modelNode, string description, int groupIndex, int modelIndex)
        {
            // Store description in action node
            modelNode.Action.ModelDescription = description;

            // Handle special case actions. These are models like doors, which
            // do not have an action record, or the coffin lids in Scourg Barrow,
            // which do not use their action data at all.
            switch (description)
            {
                case "DOR":         // Door
                case "DDR":         // Double-door
                    action.ActionType = DFBlock.RdbActionType.Rotation;
                    action.Axis = DFBlock.RdbActionAxes.PositiveY;
                    action.Magnitude = 512;
                    action.Duration = 60;
                    break;
                case "WHE":         // Wheel
                    action.Axis = DFBlock.RdbActionAxes.PositiveY;
                    action.Magnitude = 2000;
                    action.Duration = 67;
                    break;
                case "LID":         // Coffin lid in Scourg Barrow
                    action.Axis = DFBlock.RdbActionAxes.NegativeZ;
                    action.Magnitude = 512;
                    action.Duration = 80;
                    break;
                default:            // Let everything else be handled as per action record
                    break;
            }

            // Create action record for this model from Daggerfall's action record.
            // Only rotation and translation are supported at this time.
            switch (action.ActionType)
            {
                case DFBlock.RdbActionType.None:
                    modelNode.Action.Enabled = false;
                    return;

                case DFBlock.RdbActionType.Rotation:
                    modelNode.Action.Rotation = GetActionVector(ref action);
                    modelNode.Action.Rotation.X =
                        -MathHelper.ToRadians(modelNode.Action.Rotation.X / rotationDivisor);
                    modelNode.Action.Rotation.Y =
                        MathHelper.ToRadians(modelNode.Action.Rotation.Y / rotationDivisor);
                    modelNode.Action.Rotation.Z =
                        MathHelper.ToRadians(modelNode.Action.Rotation.Z / rotationDivisor);
                    break;

                case DFBlock.RdbActionType.Translation:
                    modelNode.Action.Translation = GetActionVector(ref action);
                    break;

                default:
                    // Unsupported action type
                    modelNode.Action.Enabled = false;
                    return;
            }

            // Set duration.
            // Not really sure of the correct unit - definitely not milliseconds.
            // Using n/60ths of a second for now, which seems pretty close.
            modelNode.Action.Duration = (long)(1000f * (action.Duration / 60f));

            // Enable action
            modelNode.Action.Enabled = true;

            // Create action links
            ActionLink link;
            link.node = modelNode;
            link.nextKey = GetActionKey(groupIndex, action.NextObjectIndex);
            link.prevKey = GetActionKey(groupIndex, action.PreviousObjectIndex);
            actionLinkDict.Add(GetActionKey(groupIndex, modelIndex), link);
        }

        /// <summary>
        /// Constructs a Vector3 from magnitude and direction
        ///  in RDB action resource.
        /// </summary>
        /// <param name="resource">DFBlock.RdbActionResource</param>
        /// <returns>Vector3.</returns>
        private Vector3 GetActionVector(ref DFBlock.RdbActionResource resource)
        {
            Vector3 vector = Vector3.Zero;
            float magnitude = resource.Magnitude;
            switch (resource.Axis)
            {
                case DFBlock.RdbActionAxes.NegativeX:
                    vector.X = -magnitude;
                    break;
                case DFBlock.RdbActionAxes.NegativeY:
                    vector.Y = -magnitude;
                    break;
                case DFBlock.RdbActionAxes.NegativeZ:
                    vector.Z = -magnitude;
                    break;

                case DFBlock.RdbActionAxes.PositiveX:
                    vector.X = magnitude;
                    break;
                case DFBlock.RdbActionAxes.PositiveY:
                    vector.Y = magnitude;
                    break;
                case DFBlock.RdbActionAxes.PositiveZ:
                    vector.Z = magnitude;
                    break;

                default:
                    magnitude = 0f;
                    break;
            }

            return vector;
        }

        /// <summary>
        /// Links action chains together.
        /// </summary>
        private void LinkActionNodes()
        {
            // Exit if no actions
            if (actionLinkDict.Count == 0)
                return;

            // Iterate through actions
            foreach (var item in actionLinkDict)
            {
                // Link to next node
                if (actionLinkDict.ContainsKey(item.Value.nextKey))
                    item.Value.node.Action.NextNode = actionLinkDict[item.Value.nextKey].node;

                // Link to previous node
                if (actionLinkDict.ContainsKey(item.Value.prevKey))
                    item.Value.node.Action.PreviousNode = actionLinkDict[item.Value.prevKey].node;
            }
        }

        /// <summary>
        /// Creates action key unique within group.
        /// </summary>
        /// <param name="groupIndex">RDB group index.</param>
        /// <param name="objIndex">RDB object index.</param>
        /// <returns></returns>
        private int GetActionKey(int groupIndex, int objIndex)
        {
            // Create action key for this object
            return groupIndex * 1000 + objIndex;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the type of this flat.
        /// </summary>
        /// <param name="textureArchive">Texture archive index.</param>
        /// <returns>BillboardNode.BillboardType.</returns>
        private BillboardNode.BillboardType GetFlatType(int textureArchive)
        {
            // Determine flat type
            BillboardNode.BillboardType type;
            if (textureArchive == (int)BillboardNode.BillboardType.Editor)
                type = BillboardNode.BillboardType.Editor;
            else if (textureArchive == (int)BillboardNode.BillboardType.Light)
                type = BillboardNode.BillboardType.Light;
            else
                type = BillboardNode.BillboardType.Decorative;

            return type;
        }

        #endregion

    }

}
