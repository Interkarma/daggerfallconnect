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
    /// Parent class of a scene node. Provides hierarchy, visibility,
    ///  containment, distance sorting, actions, and a transformation matrix.
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
        private BoundingSphere bounds;
        private Matrix matrix;
        private ActionData action;
        private SceneNode parent;
        private List<SceneNode> children;

        #endregion

        #region Class Structures

        /// <summary>
        /// Describes an action that can be performed on a node.
        ///  Only rotations and translations are supported at this time.
        /// </summary>
        public struct ActionData
        {
            /// <summary>
            /// Rotation to perform around each axis in degrees.
            /// </summary>
            public Vector3 Rotation;

            /// <summary>
            /// Amount of rotation that has been performed so far, in degrees.
            /// </summary>
            public Vector3 CurrentRotation;

            /// <summary>
            /// Translation to perform on each axis.
            /// </summary>
            public Vector3 Translation;

            /// <summary>
            /// Amount of translation that has been performed so far.
            /// </summary>
            public Vector3 CurrentTranslation;

            /// <summary>
            /// Matrix representing the current state of this action.
            /// </summary>
            public Matrix Matrix;

            /// <summary>
            /// Start time for the action in milliseconds.
            /// </summary>
            public long StartTime;

            /// <summary>
            /// Elapsed time in milliseconds for object to reach final state.
            ///  TotalTime = StartTime+Duration.
            /// </summary>
            public long Duration;

            /// <summary>
            /// State this action is currently in.
            /// </summary>
            public ActionState ActionState;

            /// <summary>
            /// Next node for chained action records.
            /// </summary>
            public SceneNode NextNode;
        }

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
        /// Gets or sets bounding volume.
        /// </summary>
        public BoundingSphere Bounds
        {
            get { return bounds; }
            set { bounds = value; }
        }

        /// <summary>
        /// Gets or sets transformation matrix.
        /// </summary>
        public Matrix Matrix
        {
            get { return matrix; }
            set { matrix = value; }
        }

        /// <summary>
        /// Gets or sets action.
        /// </summary>
        public ActionData Action
        {
            get { return action; }
            set { action = value; }
        }

        /// <summary>
        /// Gets parent node.
        /// </summary>
        public SceneNode Parent
        {
            get { return parent; }
        }

        /// <summary>
        /// Gets children nodes.
        /// </summary>
        public List<SceneNode> Children
        {
            get { return children; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SceneNode()
        {
            this.id = SceneManager.NextID;
            this.visible = false;
            this.distance = null;
            this.matrix = Matrix.Identity;
            this.parent = null;
            this.children = new List<SceneNode>();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Adds a new child node.
        ///  If child node already has a parent it will
        ///  be detached from current parent first.
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

        #region Private methods

        /// <summary>
        /// Pop bounding volume back up hierarchy.
        /// </summary>
        /// <param name="node">Start node.</param>
        private void PopBounds(SceneNode node)
        {
            this.bounds = BoundingSphere.CreateMerged(
                this.bounds, node.bounds);
            if (node.parent != null)
                PopBounds(node.parent);
        }

        #endregion

        #region IComparable

        /// <summary>
        /// Compare distance of two nodes from camera.
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
        public ModelNode(ModelManager.ModelData model)
            : base()
        {
            this.model = model;
            this.Bounds = model.BoundingSphere;
        }
    }

    #endregion

}
