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
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace DeepEngine.Deprecated
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

        // Random numbers
        private Random rnd = new Random();

#if DEBUG
        // Performance
        Stopwatch stopwatch = new Stopwatch();
        long updateTime = 0;
#endif

        #endregion

        #region Properties

        /// <summary>
        /// Gets root scene node.
        /// </summary>
        public SceneNode Root
        {
            get { return root; }
        }

#if DEBUG
        /// <summary>
        /// Gets last update time in milliseconds.
        /// </summary>
        public long UpdateTime
        {
            get { return updateTime; }
        }
#endif

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
#if DEBUG
            // Start timing
            stopwatch.Reset();
            stopwatch.Start();
#endif

            // Update nodes
            UpdateNode(root, Matrix.Identity, elapsedTime);

#if DEBUG
            // End timing
            stopwatch.Stop();
            updateTime = stopwatch.ElapsedMilliseconds;
#endif
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
        ///  This method requires some clean-up and optimisation.
        ///  Currently rebuilding matrices on every update whether node
        ///  has changed or not. This is done for simplicity and will
        ///  be improved later.
        /// </summary>
        /// <param name="node">SceneNode.</param>
        /// <param name="matrix">Cumulative Matrix.</param>
        /// <returns>Transformed and merged BoundingSphere.</returns>
        /// <param name="elapsedTime">Elapsed time since last frame.</param>
        private BoundingSphere UpdateNode(SceneNode node, Matrix matrix, TimeSpan elapsedTime)
        {
            Matrix cumulativeMatrix = Matrix.Identity;

            // Create node transforms
            Matrix rotationX = Matrix.CreateRotationX(node.Rotation.X);
            Matrix rotationY = Matrix.CreateRotationY(node.Rotation.Y);
            Matrix rotationZ = Matrix.CreateRotationZ(node.Rotation.Z);
            Matrix translation = Matrix.CreateTranslation(node.Position);

            // Create action transforms
            Matrix actionTranslation = Matrix.Identity;
            if (node.Action.Enabled)
            {
                // Progress actions
                if (node.Action.ActionState == SceneNode.ActionState.RunningForwards)
                {
                    // Progress action
                    node.Action.RunTime += elapsedTime.Milliseconds;
                    if (node.Action.RunTime >= node.Action.Duration)
                    {
                        node.Action.RunTime = node.Action.Duration;
                        node.Action.ActionState = SceneNode.ActionState.End;
                    }
                }
                else if (node.Action.ActionState == SceneNode.ActionState.RunningBackwards)
                {
                    // Progress action
                    node.Action.RunTime -= elapsedTime.Milliseconds;
                    if (node.Action.RunTime <= 0)
                    {
                        node.Action.RunTime = 0;
                        node.Action.ActionState = SceneNode.ActionState.Start;
                    }
                }

                float scale = (float)node.Action.RunTime / (float)node.Action.Duration;
                float xrot = node.Action.Rotation.X * scale;
                float yrot = node.Action.Rotation.Y * scale;
                float zrot = node.Action.Rotation.Z * scale;
                float xtrn = node.Action.Translation.X * scale;
                float ytrn = node.Action.Translation.Y * scale;
                float ztrn = node.Action.Translation.Z * scale;

                // Create action transforms
                Matrix actionRotationX = Matrix.CreateRotationX(xrot);
                Matrix actionRotationY = Matrix.CreateRotationY(yrot);
                Matrix actionRotationZ = Matrix.CreateRotationZ(zrot);
                actionTranslation = Matrix.CreateTranslation(xtrn, ytrn, ztrn);

                // Apply action transforms
                Matrix.Multiply(ref cumulativeMatrix, ref actionRotationY, out cumulativeMatrix);
                Matrix.Multiply(ref cumulativeMatrix, ref actionRotationX, out cumulativeMatrix);
                Matrix.Multiply(ref cumulativeMatrix, ref actionRotationZ, out cumulativeMatrix);
            }

            // Apply node transforms.
            // Rotation order is Y*X*Z which seems to be correct in all observed cases.            
            Matrix.Multiply(ref cumulativeMatrix, ref rotationY, out cumulativeMatrix);
            Matrix.Multiply(ref cumulativeMatrix, ref rotationX, out cumulativeMatrix);
            Matrix.Multiply(ref cumulativeMatrix, ref rotationZ, out cumulativeMatrix);
            Matrix.Multiply(ref cumulativeMatrix, ref actionTranslation, out cumulativeMatrix);
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
                    UpdateNode(child, cumulativeMatrix, elapsedTime));
            }

            // Animate a light node
            if (node is PointLightNode)
            {
                PointLightNode pointLightNode = (PointLightNode)node;
                pointLightNode.AnimTimer += elapsedTime.Milliseconds;
                if (pointLightNode.AnimTimer > 60)
                {
                    pointLightNode.AnimTimer = 0;
                    pointLightNode.AnimScale = MathHelper.Clamp(
                         pointLightNode.AnimScale + (float)(rnd.NextDouble() - 0.5f) * 0.15f,
                         0.70f,
                         1.0f);
                }
            }

            // Calculate point lighting on a billboard node.
            // The light and billboard must have the same parent or
            // they will not be compared.
            // TODO: Find a better way to do this.
            if (node is BillboardNode)
            {
                BillboardNode billboardNode = (BillboardNode)node;
                billboardNode.LightIntensity = 0f;
                foreach (SceneNode child in node.Parent.Children)
                {
                    if (child is PointLightNode)
                    {
                        PointLightNode pointLightNode = (PointLightNode)child;
                        if (pointLightNode.TransformedBounds.Intersects(billboardNode.TransformedBounds))
                        {
                            float distance = Vector3.Distance(
                                pointLightNode.TransformedBounds.Center,
                                billboardNode.TransformedBounds.Center);

                            float attenuation = MathHelper.Clamp(
                                1.0f - distance / pointLightNode.Radius,
                                0,
                                1);

                            billboardNode.LightIntensity = MathHelper.Clamp(
                                billboardNode.LightIntensity + attenuation,
                                0,
                                1);
                        }
                    }
                }
            }

            // Store transformed bounds
            node.TransformedBounds = bounds;

            // Store cumulative matrix
            node.Matrix = cumulativeMatrix;

            return bounds;
        }

        #endregion

    }

}
