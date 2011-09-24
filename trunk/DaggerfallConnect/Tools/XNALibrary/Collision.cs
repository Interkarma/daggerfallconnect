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

    /// <summary>
    /// Helper class to test camera-scene and camera-model collisions.
    /// </summary>
    public class Collision
    {

        #region Class Variables

        // Intersections
        private Intersection intersection = new Intersection();
        private const int defaultIntersectionCapacity = 35;
        private List<Intersection.NodeIntersection> cameraSceneIntersections;

        // Collision test classes
        private Camera camera;
        private Input input;
        private Scene scene;

        #endregion

        #region Properties
        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Collision()
        {
            cameraSceneIntersections = 
                new List<Intersection.NodeIntersection>(defaultIntersectionCapacity);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Update collision subsystem.
        /// </summary>
        /// <param name="camera">Camera.</param>
        /// <param name="input">Input.</param>
        /// <param name="scene">Scene.</param>
        public void Update(Camera camera, Input input, Scene scene)
        {
            // Store test classes
            this.camera = camera;
            this.input = input;
            this.scene = scene;

            // Test camera against scene
            TestCameraSceneIntersections();
        }

        #endregion

        #region Private Methods
        #endregion

        #region Collision Tests

        /// <summary>
        /// Test camera for scene intersections at a
        ///  course bounding volume level.
        /// </summary>
        private void TestCameraSceneIntersections()
        {
            // Reset scene-camera intersection lists
            cameraSceneIntersections.Clear();
            cameraSceneIntersections.Capacity = defaultIntersectionCapacity;

            // Build list of scene-camera intersections
            TestCameraNodeIntersections(scene.Root);

            // Find model intersections
            TestCameraModelIntersections();
        }

        /// <summary>
        /// Test camera for node intersections.
        /// </summary>
        private void TestCameraNodeIntersections(SceneNode node)
        {
            // Test node against camera
            if (node.TransformedBounds.Intersects(camera.BoundingSphere))
            {
                // Get distance between camera and model bounds
                float? distance = 
                    Vector3.Distance(camera.BoundingSphere.Center, node.TransformedBounds.Center);

                // Add intersection
                cameraSceneIntersections.Add(
                    new Intersection.NodeIntersection(distance, node));

                // Test child nodes
                foreach (SceneNode child in node.Children)
                {
                    TestCameraNodeIntersections(child);
                }
            }
        }

        /// <summary>
        /// Tests camera for model leaf node intersections.
        /// </summary>
        private void TestCameraModelIntersections()
        {
            // Nothing to do if zero scene intersections
            if (cameraSceneIntersections.Count == 0)
                return;

            // Sort intersections by distance
            cameraSceneIntersections.Sort();

            // Work through all intersections
            foreach (var ni in cameraSceneIntersections)
            {
                // Skip if not a leaf model node
                if (false == (ni.Node is ModelNode))
                    continue;

                // Test model
                TestModelNode((ModelNode)ni.Node);
            }
        }

        private void TestModelNode(ModelNode node)
        {
            // Get model
            ModelManager.ModelData model = node.Model;

            // Get motion ray
            //Vector3 step = input.MovementDelta;
            //step = camera.NextPosition - camera.Position;
            //Ray motionRay;
            //motionRay.Position = camera.Position;
            //motionRay.Direction = Vector3.Normalize(step);
            //distance = Intersection.RayIntersectsDFMesh(
            //    motionRay,
            //    mi.ModelMatrix,
            //    ref model,
            //    out subMeshResult,
            //    out planeResult);

            // Ensure camera does not move beyond obstacle
            /*
            if (distance != null)
            {
                // If distance to move is greater than distance to collision
                // then something is in the way.
                float totalDistance = Vector3.Distance(Vector3.Zero, step);
                float validDistance = distance.Value - camera.BodyRadius / 2;
                if (totalDistance > validDistance)
                {
                    // Move valid amount along movement vector
                    Matrix m = Matrix.CreateTranslation(motionRay.Direction * validDistance);
                    //camera.NextPosition = Vector3.Transform(camera.Position, m);
                }

                // Test sphere intersection
                sphereResult = intersection.SphereIntersectDFMesh(
                    camera.BoundingSphere.Center,
                    camera.BoundingSphere.Radius,
                    mi.ModelMatrix,
                    ref model);

                // Refine next camera position using sphere intersection
                if (sphereResult.Hit)
                {
                    distance = (float)Math.Sqrt(sphereResult.DistanceSquared);
                    float difference = camera.BodyRadius - distance.Value;
                    Vector3 normal = Vector3.TransformNormal(sphereResult.Normal, mi.ModelMatrix);
                    Matrix m = Matrix.CreateTranslation(normal * difference);
                    //camera.NextPosition = Vector3.Transform(camera.NextPosition, m);
                }
            }
            */
        }

        /*
        /// <summary>
        /// Simple down ray test to keep camera above models.
        /// </summary>
        private void TestDownRay(ModelNode node)
        {
            Ray downRay;
            downRay.Position = camera.Position + input.MovementDelta;
            downRay.Direction = Vector3.Down;
            
            ModelManager.ModelData model = node.Model;
            int subMeshResult, planeResult;
            float? distance = Intersection.RayIntersectsDFMesh(
                downRay,
                node.Matrix,
                ref model,
                out subMeshResult,
                out planeResult);
            if (distance != null && distance < camera.EyeHeight)
            {
                Vector3 nextPosition = downRay.Position;
                nextPosition.Y += camera.EyeHeight - distance.Value;
                input.MovementDelta = downRay.Position + nextPosition;
                //camera.NextPosition = nextPosition;
            }
        }
        */

        #endregion

    }

}
