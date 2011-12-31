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
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace DeepEngine.Deprecated
{

    /// <summary>
    /// Helper class to test camera-scene and camera-model collisions.
    /// </summary>
    public class Collision
    {

        #region Class Variables

        // Intersections
        private Camera testCamera = new Camera();
        private Intersection intersection = new Intersection();
        private const int defaultIntersectionCapacity = 50;
        private List<Intersection.NodeIntersection> sceneIntersections;

        // Collision test classes
        private Camera camera;
        private Input input;
        private Scene scene;

        // Distances
        private float distanceToGround = 0f;

#if DEBUG
        // Performance
        Stopwatch stopwatch = new Stopwatch();
        long updateTime = 0;
#endif

        #endregion

        #region Properties

        /// <summary>
        /// Gets distance to ground.
        /// </summary>
        public float DistanceToGround
        {
            get { return distanceToGround; }
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
        /// Default constructor.
        /// </summary>
        public Collision()
        {
            sceneIntersections = 
                new List<Intersection.NodeIntersection>(defaultIntersectionCapacity);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Update collision subsystem.
        ///  Tests camera against scene and will modify
        ///  input to prevent camera from passing through scene.
        ///  Final input will be applied to camera.
        /// </summary>
        /// <param name="camera">Camera.</param>
        /// <param name="input">Input.</param>
        /// <param name="scene">Scene.</param>
        public void Update(Camera camera, Scene scene, Input input)
        {
#if DEBUG
            // Start timing
            stopwatch.Reset();
            stopwatch.Start();
#endif

            // Store classes
            this.camera = camera;
            this.scene = scene;
            this.input = input;

            // Update test camera
            Camera.Copy(camera, testCamera);
            input.Apply(testCamera, true);

            // Set initial distance to ground
            distanceToGround = camera.Position.Y - testCamera.MovementBounds.Min.Y;

            // Test movement against scene
            TestMovementSceneIntersections();

            // Test camera against scene
            TestCameraSceneIntersections();

            // Set live camera
            Camera.Copy(testCamera, camera);

#if DEBUG
            // End timing
            stopwatch.Stop();
            updateTime = stopwatch.ElapsedMilliseconds;
#endif
        }

        #endregion

        #region Movement Collision Tests

        /// <summary>
        /// Test movement vector for scene intersections at a
        ///  course bounding volume level. This will create
        ///  a short list of possible models we might collide with.
        /// </summary>
        private void TestMovementSceneIntersections()
        {
            // Get movement ray
            Ray movementRay;
            Vector3 movement = testCamera.Position - camera.Position;
            movementRay.Position = camera.Position;
            movementRay.Direction = Vector3.Normalize(movement);

            // Nothing to do if no movement
            if (movement == Vector3.Zero)
                return;

            // Reset intersections list
            sceneIntersections.Clear();
            sceneIntersections.Capacity = defaultIntersectionCapacity;

            // Build list of movement node intersections
            TestMovementNodeIntersection(scene.Root, ref movementRay);

            // Find model intersections
            TestMovementModelIntersections(ref movementRay, ref movement);
        }

        /// <summary>
        /// Test node for movement intersections.
        /// </summary>
        /// <param name="node">SceneNode.</param>
        /// <param name="movementRay">Ray.</param>
        private void TestMovementNodeIntersection(SceneNode node, ref Ray movementRay)
        {
            // Test movement against node
            float? distance = movementRay.Intersects(node.TransformedBounds);
            if (distance != null)
            {
                sceneIntersections.Add(
                    new Intersection.NodeIntersection(distance, node));

                // Test child nodes
                foreach (SceneNode child in node.Children)
                {
                    TestMovementNodeIntersection(child, ref movementRay);
                }
            }
        }

        /// <summary>
        /// Tests movement for model leaf node intersections.
        /// </summary>
        /// <param name="movementRay">Ray.</param>
        /// <param name="movement">Total movement.</param>
        private void TestMovementModelIntersections(ref Ray movementRay, ref Vector3 movement)
        {
            // Nothing to do if zero intersections
            if (sceneIntersections.Count == 0)
                return;

            // Sort intersections by distance
            sceneIntersections.Sort();

            // Work through all intersections
            foreach (var si in sceneIntersections)
            {
                // Skip if not a leaf model node
                if (false == (si.Node is ModelNode))
                    continue;

                // Test model
                TestMovementModel((ModelNode)si.Node, ref movementRay, ref movement);
            }
        }

        /// <summary>
        /// Tests movement for model intersection.
        /// </summary>
        /// <param name="node">ModelNode.</param>
        /// <param name="movementRay">Ray.</param>
        /// <param name="movement">Total movement.</param>
        private void TestMovementModel(ModelNode node, ref Ray movementRay, ref Vector3 movement)
        {
            // Ignore open doors
            if (IsOpenDoor(node))
                return;

            // Test movement ray against model
            int subMeshResult, planeResult;
            float? distance = Intersection.RayIntersectsDFMesh(
                movementRay,
                node.Matrix,
                node.Model,
                out subMeshResult,
                out planeResult);

            // Handle ray intersection
            if (distance != null)
            {
                float totalDistance = Vector3.Distance(Vector3.Zero, movement);
                float validDistance = distance.Value - camera.BodyRadius / 2;
                if (totalDistance > validDistance)
                {
                    // Move camera back along movement ray by penetration distance
                    float penetrationDistance = totalDistance - validDistance;
                    Vector3 position = testCamera.Position;
                    Matrix m = Matrix.CreateTranslation(-movementRay.Direction * penetrationDistance);
                    Vector3.Transform(ref position, ref m, out position);
                    testCamera.Position = position;
                }
            }
        }

        #endregion

        #region Local Collision Tests

        /// <summary>
        /// Test local camera volume for scene intersections at a
        ///  course bounding volume level. This will create
        ///  a short list of possible models we might collide with.
        /// </summary>
        private void TestCameraSceneIntersections()
        {
            // Reset intersections list
            sceneIntersections.Clear();
            sceneIntersections.Capacity = defaultIntersectionCapacity;

            // Build list of camera node intersections
            TestCameraNodeIntersection(scene.Root);

            // Find model intersections
            TestCameraModelIntersections();
        }

        /// <summary>
        /// Test node for node intersections.
        /// </summary>
        /// <param name="node">SceneNode.</param>
        private void TestCameraNodeIntersection(SceneNode node)
        {
            // Test camera against node
            if (camera.BoundingSphere.Intersects(node.TransformedBounds))
            {
                sceneIntersections.Add(
                    new Intersection.NodeIntersection(0f, node));

                // Test child nodes
                foreach (SceneNode child in node.Children)
                {
                    TestCameraNodeIntersection(child);
                }
            }
        }

        /// <summary>
        /// Tests camera for model intersection.
        /// </summary>
        private void TestCameraModelIntersections()
        {
            // Nothing to do if zero intersections
            if (sceneIntersections.Count == 0)
                return;

            // Sort intersections by distance
            sceneIntersections.Sort();

            // Work through all intersections
            foreach (var si in sceneIntersections)
            {
                // Skip if not a leaf model node
                if (false == (si.Node is ModelNode))
                    continue;

                // Test model
                TestCameraModel((ModelNode)si.Node);
            }
        }

        /// <summary>
        /// Tests camera against a model node.
        /// </summary>
        /// <param name="node"></param>
        private void TestCameraModel(ModelNode node)
        {
            // Ignore open doors
            if (IsOpenDoor(node))
                return;

            // Test sphere intersection
            Intersection.CollisionResult sphereResult;
            sphereResult = intersection.SphereIntersectDFMesh(
                camera.BoundingSphere.Center,
                camera.BoundingSphere.Radius,
                node.Matrix,
                node.Model);

            // Refine next camera position using sphere intersection
            float? distance;
            if (sphereResult.Hit)
            {
                // Bump camera backwards along plane normal
                Vector3 position = testCamera.Position;
                distance = (float)Math.Sqrt(sphereResult.DistanceSquared);
                float difference = camera.BodyRadius - distance.Value;
                Vector3 normal = Vector3.TransformNormal(sphereResult.Normal, node.Matrix);
                Matrix m = Matrix.CreateTranslation(normal * difference);
                Vector3.Transform(ref position, ref m, out position);
                testCamera.Position = position;
            }

            // Test down ray against model
            Ray downRay;
            downRay.Position = testCamera.Position;
            downRay.Direction = Vector3.Down;
            int subMeshResult, planeResult;
            distance = Intersection.RayIntersectsDFMesh(
                downRay,
                node.Matrix,
                node.Model,
                out subMeshResult,
                out planeResult);

            // Handle ray intersection
            if (distance != null)
            {
                if (distance.Value < testCamera.EyeHeight)
                {
                    // Keep camera at eye height
                    float penetrationDistance = testCamera.EyeHeight - distance.Value;
                    Vector3 position = testCamera.Position;
                    Matrix m = Matrix.CreateTranslation(Vector3.Up * penetrationDistance);
                    Vector3.Transform(ref position, ref m, out position);
                    testCamera.Position = position;
                    distanceToGround = testCamera.EyeHeight;
                }
                else
                {
                    distanceToGround = distance.Value;
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Check if this is an open (or opening) door.
        ///  These are ignored by the collision system.
        /// </summary>
        /// <param name="node">ModelNode.</param>
        /// <returns>True if door open.</returns>
        private bool IsOpenDoor(ModelNode node)
        {
            // Ignore doors that are not closed
            if (node.Action.ModelDescription == "DOR" &&
                node.Action.ActionState != SceneNode.ActionState.Start)
            {
                return true;
            }

            return false;
        }

        #endregion

    }

}
