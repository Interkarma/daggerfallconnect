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

    #region SceneNode

    /// <summary>
    /// Parent class of a scene node.
    /// </summary>
    public class SceneNode :
        IComparable<SceneNode>,
        IEquatable<SceneNode>,
        IDisposable
    {
        #region Class Variables

        private uint id;
        private bool visible;
        private float? distance;
        private BoundingSphere localBounds;
        private BoundingSphere transformedBounds;
        private Color drawBoundsColor;
        private Vector3 rotation;
        private Vector3 position;
        private Matrix matrix;
        private ActionRecord action;
        private object tag;
        private SceneNode parent;
        private List<SceneNode> children;

        #endregion

        #region Sub Classes

        /// <summary>
        /// Describes an action that can be performed on a node.
        ///  Only rotations and translations are supported at this time.
        /// </summary>
        public class ActionRecord
        {
            #region Action Settings

            /// <summary>
            /// RDB record description for the model associated with this action.
            /// </summary>
            public string ModelDescription = null;

            /// <summary>
            /// True if action is enabled.
            /// </summary>
            public bool Enabled = false;

            /// <summary>
            /// Rotation to perform.
            /// </summary>
            public Vector3 Rotation = Vector3.Zero;

            /// <summary>
            /// Translation to perform.
            /// </summary>
            public Vector3 Translation = Vector3.Zero;

            /// <summary>
            /// Time for object to reach final state.
            /// </summary>
            public long Duration = 0;

            /// <summary>
            /// Previous node for chained action records.
            /// </summary>
            public SceneNode PreviousNode = null;

            /// <summary>
            /// Next node for chained action records.
            /// </summary>
            public SceneNode NextNode = null;

            #endregion

            #region Action State

            /// <summary>
            /// State this action is currently in.
            /// </summary>
            public ActionState ActionState = ActionState.Start;

            /// <summary>
            /// Time action has been in progress.
            /// </summary>
            public long RunTime = 0;

            #endregion
        }

        #endregion

        #region Class Structures

        /// <summary>
        /// State of an action.
        /// </summary>
        public enum ActionState
        {
            /// <summary>Action is in the start position.</summary>
            Start,

            /// <summary>Action is running forwards (start to end).</summary>
            RunningForwards,

            /// <summary>Action is running backwards (end to start).</summary>
            RunningBackwards,

            /// <summary>Action is in the end position.</summary>
            End,
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets unique ID of node.
        /// </summary>
        public uint ID
        {
            get { return id; }
        }

        /// <summary>
        /// Gets or sets visible flag.
        /// </summary>
        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        /// <summary>
        /// Gets or sets distance from camera.
        /// </summary>
        public float? Distance
        {
            get { return distance; }
            set { distance = value; }
        }

        /// <summary>
        /// Gets or sets bounding volume in model space.
        /// </summary>
        public BoundingSphere LocalBounds
        {
            get { return localBounds; }
            set { localBounds = value; }
        }

        /// <summary>
        /// Gets or sets transformed and merged bounding volume.
        ///  Call Update() in SceneManager to transform bounds.
        /// </summary>
        public BoundingSphere TransformedBounds
        {
            get { return transformedBounds; }
            set { transformedBounds = value; }
        }

        /// <summary>
        /// Gets or sets colour used to draw bounding volume.
        ///  Call DrawBounds() in SceneManager to draw bounds.
        /// </summary>
        public Color DrawBoundsColor
        {
            get { return drawBoundsColor; }
            set { drawBoundsColor = value; }
        }

        /// <summary>
        /// Gets or sets node rotation.
        /// </summary>
        public Vector3 Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        /// <summary>
        /// Gets or sets node position.
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// Gets or sets transformation matrix.
        /// </summary>
        internal Matrix Matrix
        {
            get { return matrix; }
            set { matrix = value; }
        }

        /// <summary>
        /// Gets or sets action data.
        /// </summary>
        public ActionRecord Action
        {
            get { return action; }
            set { action = value; }
        }

        /// <summary>
        /// Gets or sets custom tag.
        /// </summary>
        public object Tag
        {
            get { return tag; }
            set { tag = value; }
        }

        /// <summary>
        /// Gets parent node.
        /// </summary>
        public SceneNode Parent
        {
            get { return parent; }
        }

        /// <summary>
        /// Gets child nodes.
        /// </summary>
        public List<SceneNode> Children
        {
            get { return children; }
        }

        #endregion

        #region Static Properties

        /// <summary>
        /// Provides a unique ID for scene nodes by
        ///  incrementing a static value.
        /// </summary>
        private static uint IDCounter = 0;

        /// <summary>
        /// Gets new ID.
        /// </summary>
        public static uint NewID
        {
            get { return IDCounter++; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SceneNode()
        {
            this.id = SceneNode.NewID;
            this.visible = true;
            this.distance = null;
            this.drawBoundsColor = Color.White;
            this.matrix = Matrix.Identity;
            this.action = new ActionRecord();
            this.parent = null;
            this.children = new List<SceneNode>();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Adds a new child node.
        ///  If child node already has a parent it will
        ///  be detached from current parent first.
        ///  Cannot make node a parent of itself.
        /// </summary>
        /// <param name="node">SceneNode.</param>
        public void Add(SceneNode node)
        {
            if (node == null || node.Children.Contains(this))
                return;

            if (node.parent != null)
                node.Parent.Remove(node);

            node.parent = this;
            this.children.Add(node);
        }

        /// <summary>
        /// Removes a child node.
        /// </summary>
        /// <param name="node"></param>
        public void Remove(SceneNode node)
        {
            if (node != null &&
                this.Children.Contains(node))
            {
                this.Children.Remove(node);
            }
        }

        #endregion

        #region IComparable

        /// <summary>
        /// Compare distance of two nodes from each other.
        ///  Distance must be set before sorting.
        /// </summary>
        /// <param name="other">SceneNode.</param>
        /// <returns>Comparison result.</returns>
        public int CompareTo(SceneNode other)
        {
            if (other.distance == null)
                return 0;

            int returnValue = 1;
            if (other.Distance < this.Distance)
                returnValue = -1;
            else if (other.Distance == this.Distance)
                returnValue = 0;
            return returnValue;
        }

        #endregion

        #region IEquatable

        /// <summary>
        /// Compares ID of two nodes.
        /// </summary>
        /// <param name="other">SceneNode.</param>
        /// <returns>True if ID equal.</returns>
        public bool Equals(SceneNode other)
        {
            if (this.id == other.id)
                return true;
            else
                return false;
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Called when node is to be disposed.
        ///  Override if special handling needed
        ///  to dispose of node resources.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion
    }

    #endregion

    #region ModelNode

    /// <summary>
    /// Scene node for a model.
    /// </summary>
    public class ModelNode : SceneNode
    {
        // Variables
        private ModelManager.ModelData model;

        // Properties
        public ModelManager.ModelData Model
        {
            get { return model; }
            set { model = value; }
        }

        // Constructors
        public ModelNode()
            : base()
        {
            this.model = null;
        }
        public ModelNode(
            ModelManager.ModelData model)
            : base()
        {
            this.model = model;
            this.LocalBounds = model.BoundingSphere;
        }
    }

    #endregion

    #region BlockNode

    /// <summary>
    /// Scene node for a block.
    /// </summary>
    public class BlockNode : SceneNode
    {
        // Variables
        private DFBlock block;

        // Properties
        public DFBlock Block
        {
            get { return block; }
            set { block = value; }
        }

        // Constructors
        public BlockNode()
            : base()
        {
        }
        public BlockNode(
            DFBlock block)
            : base()
        {
            this.block = block;
        }
    }

    #endregion

    #region LocationNode

    /// <summary>
    /// Scene node for a location.
    /// </summary>
    public class LocationNode : SceneNode
    {
        // Variables
        private DFLocation location;

        // Properties
        public DFLocation Location
        {
            get { return location; }
            set { location = value; }
        }

        // Constructors
        public LocationNode()
            : base()
        {
        }
        public LocationNode(
            DFLocation location)
            : base()
        {
            this.location = location;
        }
    }

    #endregion

    #region GroundPlaneNode

    /// <summary>
    /// Scene node for ground plane under an RMB block.
    /// </summary>
    public class GroundPlaneNode : SceneNode
    {
        // Constants
        private const int primitiveCount = 2;
        private const float side = 4096f;
        private const float radius = 2920f;

        // Variables
        private RenderTarget2D texture;
        private VertexBuffer vertexBuffer;
        private int groundArchive;

        // Properties
        public RenderTarget2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }
        public int PrimitiveCount
        {
            get { return primitiveCount; }
        }
        public VertexBuffer VertexBuffer
        {
            get { return vertexBuffer; }
        }
        public int GroundArchive
        {
            get { return groundArchive; }
        }

        // Constructors
        public GroundPlaneNode(
            GraphicsDevice graphicsDevice,
            int groundArchive)
            : base()
        {
            this.texture = null;
            this.vertexBuffer = null;
            this.groundArchive = groundArchive;

            // Build plane rectangle
            VertexPositionNormalTexture p0, p1, p2, p3;
            p0.Position = new Vector3(0, 0, 0);
            p0.Normal = Vector3.Up;
            p0.TextureCoordinate = new Vector2(0, 1);
            p1.Position = new Vector3(0, 0, -side);
            p1.Normal = Vector3.Up;
            p1.TextureCoordinate = new Vector2(0, 0);
            p2.Position = new Vector3(side, 0, -side);
            p2.Normal = Vector3.Up;
            p2.TextureCoordinate = new Vector2(1, 0);
            p3.Position = new Vector3(side, 0, 0);
            p3.Normal = Vector3.Up;
            p3.TextureCoordinate = new Vector2(1, 1);
            VertexPositionNormalTexture[] groundPlaneVertices = 
            {
                p0, p1, p2,
                p0, p2, p3,
            };

            // Create VertexBuffer
            this.vertexBuffer = new VertexBuffer(
                graphicsDevice,
                typeof(VertexPositionNormalTexture),
                groundPlaneVertices.Length,
                BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionNormalTexture>(groundPlaneVertices);

            // Set bounds
            base.LocalBounds = new BoundingSphere(
                new Vector3(side/2, 0f, -side/2),
                radius);
        }
        public GroundPlaneNode(
            GraphicsDevice graphicsDevice,
            int groundArchive,
            RenderTarget2D texture)
            : this(graphicsDevice, groundArchive)
        {
            this.texture = texture;
        }
    }

    #endregion

    #region BillboardNode

    /// <summary>
    /// Scene node for a billboard.
    /// </summary>
    public class BillboardNode : SceneNode
    {
        // Variables
        private BillboardType type;
        private int textureKey;
        private Vector2 size;

        /// <summary>
        /// Billboard types enumeration.
        /// </summary>
        public enum BillboardType
        {
            /// <summary>Decorative flats.</summary>
            Decorative,
            /// <summary>Non-player characters, such as quest givers and shop keepers.</summary>
            NPC,
            /// <summary>Flat is also light-source.</summary>
            Light,
            /// <summary>Editor flats, such as markers for quests, random monters, and treasure.</summary>
            Editor,
            /// <summary>Climate-specific scenery in exterior blocks, such as trees and rocks.</summary>
            ClimateScenery,
        }

        // Properties
        public BillboardType Type
        {
            get { return type; }
            set { type = value; }
        }
        public int TextureKey
        {
            get { return textureKey; }
            set { textureKey = value; }
        }
        public Vector2 Size
        {
            get { return size; }
            set { size = value; }
        }

        // Constructors
        public BillboardNode()
            : base()
        {
            this.type = BillboardType.Decorative;
            this.textureKey = -1;
        }
        public BillboardNode(
            BillboardType type,
            int textureKey,
            Vector2 size)
            : base()
        {
            this.type = type;
            this.textureKey = textureKey;
            this.size = size;
            this.LocalBounds = new BoundingSphere(
                Vector3.Zero,
                (size.X > size.Y) ? size.X / 2 : size.Y / 2);
        }
    }

    #endregion

}
