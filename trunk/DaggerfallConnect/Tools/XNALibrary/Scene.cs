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
    /// Manages a scene graph of SceneNode objects.
    /// </summary>
    public class Scene
    {

        #region Class Variables

        // Scene
        private SceneNode root;

        // Appearance
        private Color defaultRootBoundsColor = Color.Red;

        #endregion

        #region Properties

        /// <summary>
        /// Gets root scene node.
        /// </summary>
        public SceneNode Root
        {
            get { return root; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public Scene()
        {
            // Create default scene
            ResetScene();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reset entire scene back to empty root node.
        /// </summary>
        public void ResetScene()
        {
            root = new SceneNode();
            root.DrawBoundsColor = defaultRootBoundsColor;
        }

        /// <summary>
        /// Prepare scene for rendering.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last frame.</param>
        public void Update(TimeSpan elapsedTime)
        {
            // Update nodes
            UpdateNode(root, Matrix.Identity);
        }

        /// <summary>
        /// Adds a node to the scene.
        /// </summary>
        /// <param name="parent">Parent node, or NULL for root.</param>
        /// <param name="node">SceneNode to add to parent.</param>
        public void AddNode(SceneNode parent, SceneNode node)
        {
            if (parent == null)
                root.Add(node);
            else
                parent.Add(node);
        }

        #endregion

        #region Updating

        /// <summary>
        /// Update node.
        /// </summary>
        /// <param name="node">SceneNode.</param>
        /// <param name="matrix">Cumulative Matrix.</param>
        /// <returns>Transformed and merged BoundingSphere.</returns>
        private BoundingSphere UpdateNode(SceneNode node, Matrix matrix)
        {
            // Create node transforms
            Matrix rotationX = Matrix.CreateRotationX(node.Rotation.X);
            Matrix rotationY = Matrix.CreateRotationY(node.Rotation.Y);
            Matrix rotationZ = Matrix.CreateRotationZ(node.Rotation.Z);
            Matrix translation = Matrix.CreateTranslation(node.Position);

            // Update cumulative matrix with node transforms.
            // Rotation order is Y*X*Z as this seems to be correct in observed cases.
            Matrix cumulativeMatrix = Matrix.Identity;
            Matrix.Multiply(ref cumulativeMatrix, ref rotationY, out cumulativeMatrix);
            Matrix.Multiply(ref cumulativeMatrix, ref rotationX, out cumulativeMatrix);
            Matrix.Multiply(ref cumulativeMatrix, ref rotationZ, out cumulativeMatrix);
            Matrix.Multiply(ref cumulativeMatrix, ref translation, out cumulativeMatrix);
            Matrix.Multiply(ref cumulativeMatrix, ref matrix, out cumulativeMatrix);

            // Transform bounds
            BoundingSphere bounds = node.LocalBounds;
            Vector3.Transform(ref bounds.Center, ref cumulativeMatrix, out bounds.Center);

            // Update child nodes
            foreach (SceneNode child in node.Children)
            {
                bounds = BoundingSphere.CreateMerged(
                    bounds,
                    UpdateNode(child, cumulativeMatrix));
            }

            // Store transformed bounds
            node.TransformedBounds = bounds;

            // Store cumulative matrix
            node.Matrix = cumulativeMatrix;

            // TODO: Get distance to camera

            // TODO: Run actions

            return bounds;
        }

        #endregion

    }

}
