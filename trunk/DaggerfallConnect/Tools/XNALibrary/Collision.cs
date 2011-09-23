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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace XNALibrary
{

    public class Collision
    {

        #region Class Variables

        // Intersections
        private Intersection intersection = new Intersection();
        private const int defaultIntersectionCapacity = 35;
        private List<Intersection.NodeIntersection> sceneCameraIntersections;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Collision()
        {
            sceneCameraIntersections = 
                new List<Intersection.NodeIntersection>(defaultIntersectionCapacity);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Test scene for camera intersections.
        /// </summary>
        /// <param name="scene">Scene.</param>
        /// <param name="camera">Camera.</param>
        public void TestSceneCameraIntersections(Scene scene, Camera camera)
        {
            // Reset scene-camera intersection lists
            sceneCameraIntersections.Clear();
            sceneCameraIntersections.Capacity = defaultIntersectionCapacity;

            // Build list of scene-camera intersections
            TestNodeCameraIntersections(scene.Root, camera);

            // Find model intersections
            TestModelCameraIntersections(camera);
        }

        #endregion

        #region Collision Tests

        /// <summary>
        /// Test scene node against camera.
        /// </summary>
        /// <param name="node">SceneNode.</param>
        /// <param name="camera">Camera.</param>
        private void TestNodeCameraIntersections(SceneNode node, Camera camera)
        {
            // Test node against camera
            if (node.TransformedBounds.Intersects(camera.BoundingSphere))
            {
                // Get distance between camera and model bounds
                float? distance = 
                    Vector3.Distance(camera.BoundingSphere.Center, node.TransformedBounds.Center);

                // Add intersection
                sceneCameraIntersections.Add(
                    new Intersection.NodeIntersection(distance, node));

                // Test child nodes
                foreach (SceneNode child in node.Children)
                {
                    TestNodeCameraIntersections(child, camera);
                }
            }
        }

        /// <summary>
        /// Tests camera against model leaf nodes.
        /// </summary>
        /// <param name="camera">Camera.</param>
        private void TestModelCameraIntersections(Camera camera)
        {
            // Nothing to do if no intersections
            if (sceneCameraIntersections.Count == 0)
                return;

            // Sort intersections by distance
            sceneCameraIntersections.Sort();

            // Work through all intersections
            foreach (var ni in sceneCameraIntersections)
            {
                // Skip if not a leaf model node
                if (false == (ni.Node is ModelNode))
                    continue;

                // Get model
                ModelNode node = (ModelNode)ni.Node;
                ModelManager.ModelData model = node.Model;
            }
        }

        #endregion

    }

}
