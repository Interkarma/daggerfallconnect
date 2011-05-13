// Project:         DaggerfallModelling
// Description:     Explore and export 3D models from Daggerfall.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using XNALibrary;
using DaggerfallModelling.Engine;
using DaggerfallModelling.ViewControls;
#endregion

namespace DaggerfallModelling.Engine
{

    /// <summary>
    /// Scene intersection tests and collision response.
    /// </summary>
    public class Collision : ComponentBase
    {
        #region Class Variables

        // Intersection and collision
        private Intersection intersection = new Intersection();
        private const int defaultIntersectionCapacity = 35;
        private List<ModelIntersection> pointerModelIntersections;
        private List<ModelIntersection> cameraModelIntersections;

        // Picking
        private ModelIntersection pointerOverModel = null;

        // Scene
        int sceneLayoutCount = 0;
        Scene.BlockPosition[] sceneLayout = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets ID of model selected by pointer ray.
        /// </summary>
        public ModelIntersection PointerOverModel
        {
            get { return pointerOverModel; }
        }

        #endregion

        #region SubClasses

        /// <summary>
        /// Describes a model that has intersected with something.
        ///  Used when sorting intersections for face-accurate picking
        ///  and collision tests.
        /// </summary>
        public class ModelIntersection : IComparable<ModelIntersection>
        {
            // Variables
            private float? distance;
            private uint? modelId;
            private Matrix blockMatrix;
            private Matrix modelMatrix;
            private BlockManager.ModelData? blockModel;

            // Properties
            public float? Distance
            {
                get { return distance; }
                set { distance = value; }
            }
            public uint? ModelID
            {
                get { return modelId; }
                set { modelId = value; }
            }
            public Matrix BlockMatrix
            {
                get { return blockMatrix; }
                set { blockMatrix = value; }
            }
            public Matrix ModelMatrix
            {
                get { return modelMatrix; }
                set { modelMatrix = value; }
            }
            public BlockManager.ModelData? BlockModel
            {
                get { return blockModel; }
                set { blockModel = value; }
            }

            // Constructors
            public ModelIntersection()
            {
                this.blockModel = null;
                this.distance = null;
                this.modelId = null;
                this.blockMatrix = Matrix.Identity;
                this.modelMatrix = Matrix.Identity;
            }
            public ModelIntersection(
                BlockManager.ModelData? blockModel,
                float? distance,
                uint? modelId,
                Matrix blockMatrix,
                Matrix modelMatrix)
            {
                this.blockModel = blockModel;
                this.distance = distance;
                this.modelId = modelId;
                this.blockMatrix = blockMatrix;
                this.modelMatrix = modelMatrix;
            }

            // IComparable
            public int CompareTo(ModelIntersection other)
            {
                int returnValue = -1;
                if (other.Distance < this.Distance)
                    returnValue = 1;
                else if (other.Distance == this.Distance)
                    returnValue = 0;
                return returnValue;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Collision(ViewHost host)
            : base (host)
        {
            // Create model intersections lists
            pointerModelIntersections = new List<ModelIntersection>();
            cameraModelIntersections = new List<ModelIntersection>();
        }

        #endregion

        #region Abstract Overrides

        /// <summary>
        /// Called when component must initialise.
        /// </summary>
        public override void Initialize()
        {
        }

        /// <summary>
        /// Called when component should update.
        /// </summary>
        public override void Update()
        {
            // Test for scene intersections
            TestSceneIntersections();

            // Perform model tests
            PointerModelIntersectionsTest();
            CameraModelCollisionResponse();
        }

        /// <summary>
        /// Called when component should redraw.
        /// </summary>
        public override void Draw()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set scene layout array.
        /// </summary>
        /// <param name="layout">Layout array.</param>
        /// <param name="count">Number of valid elements.</param>
        public void SetLayout(
            Scene.BlockPosition[] layout,
            int count)
        {
            this.sceneLayoutCount = count;
            this.sceneLayout = layout;
        }

        #endregion

        #region Bounds Intersection

        /// <summary>
        /// Test blocks in scene for intersections.
        /// </summary>
        private void TestSceneIntersections()
        {
            // Exit if no layout data
            if (sceneLayout == null || sceneLayoutCount == 0)
                return;

            // Reset pointer-model intersection lists
            pointerModelIntersections.Clear();
            pointerModelIntersections.Capacity = defaultIntersectionCapacity;

            // Reset camera-model intersection lists
            cameraModelIntersections.Clear();
            cameraModelIntersections.Capacity = defaultIntersectionCapacity;

            // Iterate each block in scene
            Matrix blockTransform;
            BoundingBox blockBounds;
            float? intersectDistance;
            bool pointerInBlock, cameraInBlock;
            for (int i = 0; i < sceneLayoutCount; i++)
            {
                // Create transformed block bounding box
                blockTransform = Matrix.CreateTranslation(sceneLayout[i].position);
                blockBounds.Min = Vector3.Transform(sceneLayout[i].block.BoundingBox.Min, blockTransform);
                blockBounds.Max = Vector3.Transform(sceneLayout[i].block.BoundingBox.Max, blockTransform);

                // Test pointer ray against block bounds
                intersectDistance = host.MouseRay.Intersects(blockBounds);
                if (intersectDistance != null)
                    pointerInBlock = true;
                else
                    pointerInBlock = false;

                // Test camera bounds against block bounds
                if (camera.BoundingSphere.Intersects(blockBounds))
                    cameraInBlock = true;
                else
                    cameraInBlock = false;

                // Test models in block
                TestModelIntersections(
                    ref sceneLayout[i].block,
                    ref blockTransform,
                    pointerInBlock,
                    cameraInBlock);
            }
        }

        /// <summary>
        /// Test models in block for intersections.
        /// </summary>
        /// <param name="block">BlockManager.Block</param>
        /// <param name="blockTransform">Block transform.</param>
        /// <param name="pointerInBlock">True if pointer is in block.</param>
        /// <param name="cameraInBlock">True if camera is in block.</param>
        private void TestModelIntersections(
            ref BlockManager.BlockData block,
            ref Matrix blockTransform,
            bool pointerInBlock,
            bool cameraInBlock)
        {
            // Iterate each model in this block
            Matrix modelTransform;
            BoundingSphere modelBounds;
            float? intersectDistance;
            foreach (var modelInfo in block.Models)
            {
                // Create transformed model bounding sphere
                modelTransform = modelInfo.Matrix * blockTransform;
                modelBounds.Center = Vector3.Transform(modelInfo.BoundingSphere.Center, modelTransform);
                modelBounds.Radius = modelInfo.BoundingSphere.Radius;

                // Test pointer intersections
                if (pointerInBlock)
                {
                    // Test if ray intersects with model sphere
                    intersectDistance = host.MouseRay.Intersects(modelBounds);
                    if (intersectDistance != null)
                    {
                        // Add to pointer-model intersection list
                        ModelIntersection mi = new ModelIntersection(
                            modelInfo,
                            intersectDistance,
                            modelInfo.ModelId,
                            blockTransform,
                            modelTransform);
                        pointerModelIntersections.Add(mi);
                    }
                }

                // Test camera intersections
                if (cameraInBlock)
                {
                    // Test if camera collides with model sphere
                    if (modelBounds.Intersects(camera.BoundingSphere))
                    {
                        // Get distance between camera and model spheres
                        intersectDistance = Vector3.Distance(camera.BoundingSphere.Center, modelBounds.Center);

                        // Add to camera-model intersection list
                        ModelIntersection mi = new ModelIntersection(
                            null,
                            intersectDistance,
                            modelInfo.ModelId,
                            blockTransform,
                            modelTransform);
                        cameraModelIntersections.Add(mi);
                    }
                }
            }
        }

        #endregion

        #region Model Intersection

        /// <summary>
        /// Tests pointer against model intersections to
        ///  resolve actual model intersection at face level.
        /// </summary>
        private void PointerModelIntersectionsTest()
        {
            // Nothing to do if no intersections
            if (pointerModelIntersections.Count == 0)
                return;

            // Sort intersections by distance
            pointerModelIntersections.Sort();

            // Iterate intersections
            float? intersection = null;
            float? closestIntersection = null;
            ModelIntersection closestModelIntersection = null;
            foreach (var mi in pointerModelIntersections)
            {
                // Get model
                ModelManager.ModelData model = host.ModelManager.GetModelData(mi.ModelID.Value);

                // Test model
                bool insideBoundingSphere;
                int subMeshResult, planeResult;
                intersection = Intersection.RayIntersectsDFMesh(
                    host.MouseRay,
                    mi.ModelMatrix,
                    ref model,
                    out insideBoundingSphere,
                    out subMeshResult,
                    out planeResult);

                if (intersection != null)
                {
                    if (closestIntersection == null || intersection < closestIntersection)
                    {
                        closestIntersection = intersection;
                        closestModelIntersection = mi;
                    }
                }
            }

            // Store closest intersection
            if (closestModelIntersection != null)
                pointerOverModel = closestModelIntersection;
            else
                pointerOverModel = null;
        }

        /// <summary>
        /// Handle camera-model collision response.
        /// </summary>
        private void CameraModelCollisionResponse()
        {
            // Nothing to do if no intersections
            if (cameraModelIntersections.Count == 0)
                return;

            // Sort intersections by distance
            cameraModelIntersections.Sort();

            // Iterate intersections
            float? distance;
            Vector3 step;
            int subMeshResult, planeResult;
            Intersection.CollisionResult sphereResult;
            Ray downRay, motionRay;
            foreach (var mi in cameraModelIntersections)
            {
                // Get model
                ModelManager.ModelData model = host.ModelManager.GetModelData(mi.ModelID.Value);

                // Get motion ray
                step = camera.NextPosition - camera.Position;
                motionRay.Position = camera.Position;
                motionRay.Direction = Vector3.Normalize(step);
                distance = Intersection.RayIntersectsDFMesh(
                    motionRay,
                    mi.ModelMatrix,
                    ref model,
                    out subMeshResult,
                    out planeResult);

                // Ensure camera does not move beyond obstacle
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
                        camera.NextPosition = Vector3.Transform(camera.Position, m);
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
                        camera.NextPosition = Vector3.Transform(camera.NextPosition, m);
                    }
                }

                // Test down ray
                downRay.Position = camera.NextPosition;
                downRay.Direction = Vector3.Down;
                distance = Intersection.RayIntersectsDFMesh(
                    downRay,
                    mi.ModelMatrix,
                    ref model,
                    out subMeshResult,
                    out planeResult);

                // Ensure camera is always at eye height
                if (distance != null && distance < camera.EyeHeight)
                {
                    Vector3 nextPosition = camera.NextPosition;
                    nextPosition.Y += camera.EyeHeight - distance.Value;
                    camera.NextPosition = nextPosition;

                    camera.ClearGravity();
                }
            }
        }

        #endregion
    }

}
