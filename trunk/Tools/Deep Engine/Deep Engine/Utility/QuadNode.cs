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
#endregion

namespace DeepEngine.Utility
{

    /// <summary>
    /// Recursive quad-tree node for terrain rendering.
    /// </summary>
    public class QuadNode
    {

        #region Fields

        int level;
        bool hasChildren;
        Rectangle rectangle;
        float maxHeight;
        BoundingBox boundingBox;
        Matrix matrix;
        QuadNode root;
        QuadNode parent;
        QuadNode[] children;

        #endregion

        #region Structures

        /// <summary>
        /// Parent or child index.
        /// </summary>
        public enum Quadrants
        {
            NW = 0,
            NE = 1,
            SW = 2,
            SE = 3,
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets level of node.
        /// </summary>
        public int Level
        {
            get { return level; }
        }

        /// <summary>
        /// True if node has children, false if a leaf node.
        /// </summary>
        public bool HasChildren
        {
            get { return hasChildren; }
        }

        /// <summary>
        /// Gets maximum height of node.
        /// </summary>
        public float MaxHeight
        {
            get { return maxHeight; }
        }

        /// <summary>
        /// Gets bounding box for this node.
        /// </summary>
        public BoundingBox BoundingBox
        {
            get { return boundingBox; }
        }

        /// <summary>
        /// Gets node matrix.
        /// </summary>
        public Matrix Matrix
        {
            get { return matrix; }
        }

        /// <summary>
        /// Gets or sets a matrix that is the sum transform of the node.
        /// </summary>
        public Matrix WorldMatrix { get; set; }

        /// <summary>
        /// Gets 2D coordinates of quad node in source data.
        /// </summary>
        public Rectangle Rectangle
        {
            get { return rectangle; }
        }

        /// <summary>
        /// Gets parent node.
        /// </summary>
        public QuadNode Parent
        {
            get { return parent; }
        }

        /// <summary>
        /// Gets child nodes.
        /// </summary>
        public QuadNode[] Children
        {
            get { return children; }
        }

        /// <summary>
        /// Gets X position of source data rectangle.
        /// </summary>
        public float X
        {
            get { return rectangle.X; }
        }

        /// <summary>
        /// Gets Y position of source data rectangle.
        /// </summary>
        public float Y
        {
            get { return rectangle.Y; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a root node.
        /// </summary>
        /// <param name="dimension">Dimension of root quad.</param>
        /// <param name="maxHeight">Maximum height of node.</param>
        public QuadNode(int dimension, float maxHeight)
        {
            // Store values
            this.root = this;
            this.level = 0;
            this.hasChildren = false;
            this.maxHeight = maxHeight;
            this.rectangle = new Rectangle(0, 0, dimension, dimension);
            this.parent = null;
            this.children = new QuadNode[4];

            // Calculate bounding box
            this.boundingBox = new BoundingBox(
                new Vector3(0, -maxHeight, 0),
                new Vector3(rectangle.Width, maxHeight, rectangle.Height));

            // Set transform
            Vector3 origin = new Vector3(rectangle.X, 0, rectangle.Y);
            this.matrix = Matrix.CreateTranslation(origin);
        }

        /// <summary>
        /// Constructs a child node.
        /// </summary>
        /// <param name="parent">Parent node.</param>
        /// <param name="quadrant">Position index.</param>
        public QuadNode(QuadNode parent, Quadrants quadrant)
            : this(0, parent.maxHeight)
        {
            // Store values
            this.root = parent.root;
            this.parent = parent;
            this.parent.children[(int)quadrant] = this;
            this.parent.hasChildren = true;
            this.level = parent.level + 1;

            // Determine interior area of child
            if (quadrant == Quadrants.NW)
            {
                rectangle = new Rectangle(
                    parent.Rectangle.X,
                    parent.Rectangle.Y,
                    parent.Rectangle.Width / 2,
                    parent.Rectangle.Height / 2);
            }
            else if (quadrant == Quadrants.NE)
            {
                rectangle = new Rectangle(
                    parent.rectangle.X + parent.Rectangle.Width / 2,
                    parent.Rectangle.Y,
                    parent.Rectangle.Width / 2,
                    parent.Rectangle.Height / 2);
            }
            else if (quadrant == Quadrants.SW)
            {
                rectangle = new Rectangle(
                    parent.Rectangle.X,
                    parent.Rectangle.Y + parent.Rectangle.Height / 2,
                    parent.Rectangle.Width / 2,
                    parent.Rectangle.Height / 2);
            }
            else if (quadrant == Quadrants.SE)
            {
                rectangle = new Rectangle(
                    parent.Rectangle.X + parent.Rectangle.Width / 2,
                    parent.Rectangle.Y + parent.Rectangle.Height / 2,
                    parent.Rectangle.Width / 2,
                    parent.Rectangle.Height / 2);
            }

            // Calculate bounding box
            this.boundingBox = new BoundingBox(
                new Vector3(0, -maxHeight, 0),
                new Vector3(rectangle.Width, maxHeight, rectangle.Height));
            
            // Set transform
            Vector3 origin = new Vector3(rectangle.X, 0, rectangle.Y);
            this.matrix = Matrix.CreateTranslation(origin);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets child node.
        /// </summary>
        /// <param name="quadrant">Index of child node.</param>
        /// <returns>QuadNode or null if child not present.</returns>
        public QuadNode GetChild(Quadrants quadrant)
        {
            return children[(int)quadrant];
        }

        #endregion

    }

}
