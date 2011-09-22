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
        /// <param name="worldClimate">
        /// World climate value. Valid range is 223-232 or NULL for default.
        ///  This is currently ignored for RDB blocks.
        /// </param>
        /// <returns>BlockNode.</returns>
        public BlockNode CreateBlockNode(string name, int? worldClimate)
        {
            // Load block
            DFBlock block;
            if (!LoadDaggerfallBlock(name, out block))
                return null;

            // Set default world climate
            if (worldClimate == null)
                worldClimate = defaultWorldClimate;

            // Get world climate settings
            DFLocation.ClimateType climateType;
            int rmbSceneryArchive;
            int skyArchive;
            MapsFile.GetWorldClimateSettings(
                worldClimate.Value,
                out climateType,
                out rmbSceneryArchive,
                out skyArchive);
            
            // Build node
            BlockNode node = null;
            switch (block.Type)
            {
                case DFBlock.BlockTypes.Rmb:
                    textureManager.ClimateType = climateType;
                    node = BuildRMBNode(ref block, rmbSceneryArchive);
                    break;
                case DFBlock.BlockTypes.Rdb:
                    textureManager.ClimateType = DFLocation.ClimateType.None;
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
                    SceneNode blockNode = CreateBlockNode(name, location.Climate);
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
                SceneNode blockNode = CreateBlockNode(block.BlockName, null);
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
        /// <param name="sceneryArchive">Scenery texture archive index.</param>
        /// <returns>BlockNode.</returns>
        private BlockNode BuildRMBNode(ref DFBlock block, int sceneryArchive)
        {
            // Create parent block node
            BlockNode blockNode = new BlockNode(block);

            // Add child nodes
            AddRMBModels(ref block, blockNode);
            AddRMBMiscModels(ref block, blockNode);
            AddRMBGroundPlane(ref block, blockNode);
            AddRMBMiscFlats(ref block, blockNode);
            AddRMBSceneryFlats(ref block, blockNode, sceneryArchive);

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
        /// <param name="blockNode">BlockNode.</param>
        private void AddRMBGroundPlane(ref DFBlock block, BlockNode blockNode)
        {
            // Add ground plane node
            VertexBuffer vertexBuffer;
            int vertexCount;
            BuildRmbGroundPlaneVertexBuffer(ref block, out vertexBuffer, out vertexCount);
            GroundPlaneNode groundNode = new GroundPlaneNode(
                vertexBuffer,
                vertexCount / 3);
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
                // Load flat
                int textureKey;
                Vector2 startSize;
                Vector2 finalSize;
                if (true == LoadDaggerfallFlat(
                    obj.TextureArchive,
                    obj.TextureRecord,
                    TextureManager.TextureCreateFlags.Dilate |
                    TextureManager.TextureCreateFlags.PreMultiplyAlpha,
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
                        BillboardNode.BillboardType.Decorative,
                        textureKey,
                        finalSize);
                    billboardNode.Position = position;
                    blockNode.Add(billboardNode);
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
                            TextureManager.TextureCreateFlags.Dilate |
                            TextureManager.TextureCreateFlags.PreMultiplyAlpha,
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
                    BillboardNode.BillboardType.Decorative,
                    textureKey,
                    finalSize);
                billboardNode.Position = position;
                blockNode.Add(billboardNode);
            }
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
            // Handle special case actions. These are models like doors, which
            // do not have an action record, or the coffin lids in Scourg Barrow,
            // which do not use their action data at all.
            switch (description)
            {
                case "DOR":         // Doors
                    action.ActionType = DFBlock.RdbActionType.Rotation;
                    action.Axis = DFBlock.RdbActionAxes.PositiveY;
                    action.Magnitude = 512;
                    action.Duration = 40;
                    break;
                case "LID":         // Coffin lids in Scourg Barrow
                    action.Axis = DFBlock.RdbActionAxes.NegativeZ;
                    action.Magnitude = 512;
                    action.Duration = 40;
                    break;
                default:            // Let everything else be handled as per action record
                    break;
            }

            // Create action record for this model from Daggerfall's action record.
            // Only rotation and translation are supported at this time.
            switch (action.ActionType)
            {
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
                    // Unsupported action record
                    modelNode.Action.Enabled = false;
                    return;
            }

            // Set duration
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

        #region Ground Plane Building

        /// <summary>
        /// Builds vertex buffer for RMB ground plane.
        /// </summary>
        /// <param name="block">DFBlock.</param>
        /// <param name="groundPlaneVertexBuffer">VertexBuffer.</param>
        private void BuildRmbGroundPlaneVertexBuffer(
            ref DFBlock block,
            out VertexBuffer groundPlaneVertexBuffer,
            out int vertexCount)
        {
            // Create vertex list for ground plane.
            // A ground plane consists of 16x16 tiled squares.
            // There are a full 6 vertices per square (3 per triangle).
            // We're doing this so we can send the whole ground plane to renderer in one call.
            VertexPositionNormalTexture[] groundPlaneVertices =
                new VertexPositionNormalTexture[(16 * 16) * 6];

            // Add tiles to the ground plane. Source tiles are stored from bottom to top, then right to left.
            // This must be accounted for when laying out tiles.
            const int tileCount = 16;
            DFBlock.RmbGroundTiles tile;
            for (int x = 0; x < tileCount; x++)
            {
                for (int y = tileCount - 1; y >= 0; y--)
                {
                    tile = block.RmbBlock.FldHeader.GroundData.GroundTiles[x, y];
                    AddGroundTile(ref groundPlaneVertices, x, y, tile.TextureRecord, tile.IsRotated, tile.IsFlipped);
                }
            }

            // Create VertexBuffer
            groundPlaneVertexBuffer = new VertexBuffer(
                textureManager.GraphicsDevice,
                VertexPositionNormalTexture.SizeInBytes * groundPlaneVertices.Length,
                BufferUsage.WriteOnly);
            groundPlaneVertexBuffer.SetData<VertexPositionNormalTexture>(groundPlaneVertices);

            // Set count
            vertexCount = groundPlaneVertices.Length;
        }

        /// <summary>
        /// Adds a single tile to the ground plane.
        /// </summary>
        /// <param name="vertices">Vertex array to be populated with data.</param>
        /// <param name="x">X position in grid from 0-15.</param>
        /// <param name="y">Y position in grid from 0-15.</param>
        /// <param name="record">Record index.</param>
        /// <param name="isRotated">True if rotated 90 degrees right.</param>
        /// <param name="isFlipped">True if flipped horizontally and vertically.</param>
        private void AddGroundTile(ref VertexPositionNormalTexture[] vertices, int x, int y, int record, bool isRotated, bool isFlipped)
        {
            // Each block ground plane is made of 16x16 tiles.
            // Each tile is 256x256 world units.
            const float side = 256.0f;

            // Handle record > 55. This indicates that random terrain should be used outside
            //  of city walls. Here, we just set this back to 2 (grass/earth).
            if (record > 55)
                record = 2;

            // Get subtexture rect
            System.Drawing.RectangleF rect = textureManager.GetTerrainSubTextureRect(record);
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

            // All Z points are offset from -rmbSide to ensure bottom-left corner (index 0 of block array)
            // aligns with local origin. Daggerfall stores block arrays from bottom to top, then right to left.
            // This basically means that the first tile (as seen from above) is at 0,0 in the array.

            // Point 0
            VertexPositionNormalTexture point0 = new VertexPositionNormalTexture();
            point0.Position = new Vector3((side * x), 0, -rmbSide + (side * y));
            point0.Normal = new Vector3(0, 1, 0);
            point0.TextureCoordinate = new Vector2(p0u, p0v);

            // Point 1
            VertexPositionNormalTexture point1 = new VertexPositionNormalTexture();
            point1.Position = new Vector3((side * x), 0, -rmbSide + (side * (y + 1)));
            point1.Normal = new Vector3(0, 1, 0);
            point1.TextureCoordinate = new Vector2(p1u, p1v);

            // Point 2
            VertexPositionNormalTexture point2 = new VertexPositionNormalTexture();
            point2.Position = new Vector3((side * (x + 1)), 0, -rmbSide + (side * (y + 1)));
            point2.Normal = new Vector3(0, 1, 0);
            point2.TextureCoordinate = new Vector2(p2u, p2v);

            // Point 3
            VertexPositionNormalTexture point3 = new VertexPositionNormalTexture();
            point3.Position = new Vector3((side * (x + 1)), 0, -rmbSide + (side * y));
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
