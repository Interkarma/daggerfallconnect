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
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DeepEngine.Daggerfall;
using DeepEngine.World;
#endregion

namespace DeepEngine.Utility
{

    /// <summary>
    /// Helper class to provide certain intersection methods.
    /// </summary>
    public class Intersection
    {
        #region Class Variables

        private Vector3[] verts = new Vector3[3];
        private Vector3[] edgeNormals = new Vector3[3];
        private Vector3 faceNormal = Vector3.Zero;

        #endregion

        #region SubClasses

        /// <summary>
        /// Describes an intersection with an object.
        /// </summary>
        public class ObjectIntersection<T> : IComparable<ObjectIntersection<T>>
        {
            // Variables
            private float? distance;
            private T obj;

            // Properties
            public float? Distance
            {
                get { return distance; }
                set { distance = value; }
            }
            public T Object
            {
                get { return obj; }
                set { obj = value; }
            }

            // Constructors
            public ObjectIntersection()
            {
                this.distance = null;
                this.obj = default(T);
            }
            public ObjectIntersection(
                float? distance,
                T obj)
            {
                this.distance = distance;
                this.obj = obj;
            }

            // IComparable
            public int CompareTo(ObjectIntersection<T> other)
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
        public Intersection()
        {
        }

        #endregion

        #region Sphere-Model Intersections

        /// <summary>
        /// Tests if a sphere intersects DFMesh geometry.
        /// </summary>
        /// <param name="position">Position in world space.</param>
        /// <param name="radius">Radius of sphere.</param>
        /// <param name="modelTransform">World space transform.</param>
        /// <param name="model">ModelManager.ModelData.</param>
        /// <returns>CollisionResult.</returns>
        public CollisionResult SphereIntersectDFMesh(Vector3 position,
                                        float radius,
                                        Matrix modelTransform,
                                        ModelManager.ModelData model)
        {
            // Transform sphere position back to object space
            Matrix inverseTransform = Matrix.Invert(modelTransform);
            position = Vector3.Transform(position, inverseTransform);

            // Test each submesh
            CollisionResult result = CollisionResult.Nothing;
            Vector3 normal, vertex1, vertex2, vertex3;
            int subMeshIndex = 0;
            foreach (var subMesh in model.DFMesh.SubMeshes)
            {
                // Test each plane of this submesh
                int planeIndex = 0;
                foreach (var plane in subMesh.Planes)
                {
                    // Get plane normal (all points in plane have same normal)
                    normal.X = plane.Points[0].NX;
                    normal.Y = -plane.Points[0].NY;
                    normal.Z = -plane.Points[0].NZ;

                    // Get shared point (vertex1)
                    vertex1.X = plane.Points[0].X;
                    vertex1.Y = -plane.Points[0].Y;
                    vertex1.Z = -plane.Points[0].Z;

                    // Walk through triangle fan
                    for (int p = 0; p < plane.Points.Length - 2; p++)
                    {
                        // Get second point (vertex2)
                        vertex2.X = plane.Points[p + 1].X;
                        vertex2.Y = -plane.Points[p + 1].Y;
                        vertex2.Z = -plane.Points[p + 1].Z;

                        // Get third point (vertex3)
                        vertex3.X = plane.Points[p + 2].X;
                        vertex3.Y = -plane.Points[p + 2].Y;
                        vertex3.Z = -plane.Points[p + 2].Z;

                        // Test intersection.
                        // Changing winding order to suit algorithm.
                        SetCollisionTriangle(vertex1, vertex3, vertex2, normal);
                        TestSphereCollision(ref position, radius, ref result);
                    }

                    // Increment planeIndex
                    planeIndex++;
                }

                // Increment subMeshIndex
                subMeshIndex++;
            }

            return result;
        }

        //-----------------------------------------------------------------------------
        // Thanks to Gloei (http://glow.inque.org) for the sphere-triangle tutorial.
        // The following code has been adapated from here:
        // http://glow.inque.org/stuff/polygonsoup/SphereTriangleIntersection-v1.zip
        //-----------------------------------------------------------------------------

        /// <summary>
        /// Stores results of collision tests.
        /// </summary>
        public struct CollisionResult
        {
            /// <summary>True when intersection valid.</summary>
            public bool Hit;

            /// <summary>Distance to intersection point squared.</summary>
            public float DistanceSquared;

            /// <summary>Location of intersection inside triangle.</summary>
            public Vector3 Location;

            /// <summary>Normal of intersection.</summary>
            public Vector3 Normal;

            /// <summary>
            /// No intersection result.
            /// </summary>
            /// <returns>CollisionResult.</returns>
            public static CollisionResult Nothing
            {
                get
                {
                    CollisionResult res;
                    res.Hit = false;
                    res.Location = Vector3.Zero;
                    res.Normal = Vector3.Zero;
                    res.DistanceSquared = 0;
                    return res;
                }
            }

            /// <summary>
            /// Assigns a new result only if intersection is closer than current.
            /// </summary>
            /// <param name="result">CollisionResult.</param>
            public void KeepClosest(CollisionResult result)
            {
                if (result.Hit && (!Hit || (result.DistanceSquared < DistanceSquared)))
                    this = result;
            }
        }

        /// <summary>
        /// Constructs triangle for collision tests.
        ///  Verts must be in correct winding order.
        /// </summary>
        /// <param name="vertex1">Vector3.</param>
        /// <param name="vertex2">Vector3.</param>
        /// <param name="vertex3">Vector3.</param>
        private void SetCollisionTriangle(Vector3 vertex1,
                                        Vector3 vertex2,
                                        Vector3 vertex3,
                                        Vector3 normal)
        {
            // Set vertices of collision triangle
            verts[0] = vertex1;
            verts[1] = vertex2;
            verts[2] = vertex3;
            faceNormal = normal;

            // Calculate normals for edge planes
            for (int i = 0; i < 3; i++)
            {
                edgeNormals[i] = -Vector3.Cross(verts[(i + 1) % 3] - verts[i], faceNormal);
                edgeNormals[i].Normalize();
            }
        }

        /// <summary>
        /// Performs sphere-triangle intersection test.
        ///  Spheres can only collide with front side of  triangles.
        ///  The closest possible intersection position is returned,
        ///  which is always inside the triangle.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="sphereRadius"></param>
        /// <param name="result"></param>
        private void TestSphereCollision(ref Vector3 position,
                                        float sphereRadius,
                                        ref CollisionResult result)
        {
            // Check side of plane
            float dist;
            Vector3 v = position - verts[0];
            Vector3.Dot(ref v, ref faceNormal, out dist);

            // Wrong side of plane
            if (dist < 0)
                return;

            // Too far away for intersection
            if (dist > sphereRadius)
                return;

            // Test for collision point in triangle
            Vector3 collisionPoint = position - (dist * faceNormal);
            if (PointInTriangle(ref collisionPoint))
            {
                CollisionResult t;
                t.Hit = true;
                t.DistanceSquared = (collisionPoint - position).LengthSquared();
                t.Normal = faceNormal;
                t.Location = collisionPoint;

                result.KeepClosest(t);
                return;
            }

            // Test edges
            for (int i = 0; i < 3; i++)
            {
                Vector3 E = verts[(i + 1) % 3] - verts[i];

                // Position relative to edge start
                Vector3 H = collisionPoint - verts[i];

                // Distance of P to edge plane
                float hn = Vector3.Dot(H, edgeNormals[i]);

                // Point is on same side of triangle from the edge plane
                if (hn < 0.0f)
                    continue;

                // Too far away from this edge plane
                if (hn > sphereRadius)
                    return;

                // Test intersection with polygon edge
                Vector3 intersectionPoint = new Vector3();
                if (SpherePartialEdgeCollide(ref position, ref verts[i], ref E, ref intersectionPoint))
                {
                    CollisionResult t;
                    t.Hit = true;
                    t.DistanceSquared = (intersectionPoint - position).LengthSquared();
                    t.Normal = faceNormal;
                    t.Location = intersectionPoint;

                    result.KeepClosest(t);
                }
            }
        }

        /// <summary>
        /// Determines if a given point lies on the triangle's plane) and
        /// if it lies within the triangle's borders.
        /// Implemented by using the triangle's edge planes.
        /// </summary>
        /// <param name="pt">A point on the triangle's plane.</param>
        /// <returns>True when point pt lies within the triangle.</returns>
        private bool PointInTriangle(ref Vector3 pt)
        {
            Vector3 a = pt - verts[0];
            Vector3 b = pt - verts[1];
            Vector3 c = pt - verts[2];

            float v;

            Vector3.Dot(ref a, ref edgeNormals[0], out v);
            if (v >= 0)
                return false;

            Vector3.Dot(ref b, ref edgeNormals[1], out v);
            if (v >= 0)
                return false;

            Vector3.Dot(ref c, ref edgeNormals[2], out v);
            if (v >= 0)
                return false;

            return true;
        }

        /// <summary>
        /// Find the point on line [edgeV0, edgeV0+edgeDir] closest to start.
        /// </summary>
        /// <param name="start">Start vector.</param>
        /// <param name="edgeV0">Edge vector.</param>
        /// <param name="edgeDir">Edge direction.</param>
        /// <param name="intersection">Intersection</param>
        /// <returns></returns>
        private bool SpherePartialEdgeCollide(ref Vector3 start, ref Vector3 edgeV0, ref Vector3 edgeDir, ref Vector3 intersection)
        {
            // From http://softsurfer.com/Archive/algorithm_0102/algorithm_0102.htm
            // Copyright 2001, softSurfer (www.softsurfer.com)
            // This code may be freely used and modified for any purpose
            // providing that this copyright notice is included with it.
            // SoftSurfer makes no warranty for this code, and cannot be held
            // liable for any real or imagined damage resulting from its use.
            // Users of this code must verify correctness for their application.

            Vector3 w = start - edgeV0;

            float c1 = Vector3.Dot(w, edgeDir);
            if (c1 <= 0)
            {
                if ((start - edgeV0).LengthSquared() <= 1)
                {
                    intersection = edgeV0;
                    return true;

                }
                else
                    return false;
            }

            float c2 = Vector3.Dot(edgeDir, edgeDir);
            if (c2 <= c1)
            {
                Vector3 p1 = edgeV0 + edgeDir;
                if ((start - p1).LengthSquared() <= 1)
                {
                    intersection = p1;
                    return true;
                }
                else
                    return false;
            }

            float b = c1 / c2;

            intersection = edgeV0 + b * edgeDir;

            float distance = 0;
            Vector3.DistanceSquared(ref start, ref intersection, out distance);

            return (distance <= 1.0f);
        }

        #endregion

        #region Ray-Model Intersections

        /// <summary>
        /// Tests if ray intersects DFMesh geometry. Does not perform
        ///  bounding sphere test. Use this method for local collisions
        ///  where non-bounding objects have already been eliminated.
        /// </summary>
        /// <param name="ray">Ray in world space.</param>
        /// <param name="modelTransform">World space transform.</param>
        /// <param name="model">ModelManager.Model.</param>
        /// <param name="subMeshResult">Index of DFSubMesh intersected by ray, or -1 on miss.</param>
        /// <param name="planeResult">Index of DFPlane interested by ray, or -1 on miss.</param>
        /// <returns>Distance to intersection, or NULL if miss.</returns>
        public static float? RayIntersectsDFMesh(Ray ray,
                                            Matrix modelTransform,
                                            ModelManager.ModelData model,
                                            out int subMeshResult,
                                            out int planeResult)
        {
            // Reset results
            subMeshResult = -1;
            planeResult = -1;

            // Transform ray back to object space
            Matrix inverseTransform = Matrix.Invert(modelTransform);
            ray.Position = Vector3.Transform(ray.Position, inverseTransform);
            ray.Direction = Vector3.TransformNormal(ray.Direction, inverseTransform);

            // Test each submesh
            float dot;
            float? intersection = null;
            float? closestIntersection = null;
            Vector3 normal, vertex1, vertex2, vertex3;
            int subMeshIndex = 0;
            foreach (var subMesh in model.DFMesh.SubMeshes)
            {
                // Test each plane of this submesh
                int planeIndex = 0;
                foreach (var plane in subMesh.Planes)
                {
                    // Get plane normal (all points in plane have same normal)
                    normal.X = plane.Points[0].NX;
                    normal.Y = -plane.Points[0].NY;
                    normal.Z = -plane.Points[0].NZ;

                    // Cull planes facing away from ray
                    dot = Vector3.Dot(normal, ray.Direction);
                    if (dot > 0f)
                    {
                        planeIndex++;
                        continue;
                    }

                    // Get shared point (vertex1)
                    vertex1.X = plane.Points[0].X;
                    vertex1.Y = -plane.Points[0].Y;
                    vertex1.Z = -plane.Points[0].Z;

                    // Walk through triangle fan
                    for (int p = 0; p < plane.Points.Length - 2; p++)
                    {
                        // Get second point (vertex2)
                        vertex2.X = plane.Points[p + 1].X;
                        vertex2.Y = -plane.Points[p + 1].Y;
                        vertex2.Z = -plane.Points[p + 1].Z;

                        // Get third point (vertex3)
                        vertex3.X = plane.Points[p + 2].X;
                        vertex3.Y = -plane.Points[p + 2].Y;
                        vertex3.Z = -plane.Points[p + 2].Z;

                        // Test intersection
                        RayIntersectsTriangle(ref ray, ref vertex1, ref vertex2, ref vertex3, out intersection);
                        if (intersection != null)
                        {
                            // Test for closest intersection so far
                            if (closestIntersection == null || intersection < closestIntersection)
                            {
                                // Update closest intersection
                                closestIntersection = intersection;
                                subMeshResult = subMeshIndex;
                                planeResult = planeIndex;
                            }
                        }
                    }

                    // Increment planeIndex
                    planeIndex++;
                }

                // Increment subMeshIndex
                subMeshIndex++;
            }

            return closestIntersection;
        }

        /// <summary>
        /// Tests if ray intersects DFMesh geometry. Performs bounding sphere
        ///  test first. Use this method for model picking from an
        ///  unprojected ray.
        /// </summary>
        /// <param name="ray">Ray in world space.</param>
        /// <param name="modelTransform">World space transform.</param>
        /// <param name="model">ModelManager.Model.</param>
        /// <param name="insideBoundingSphere">True if ray intersects model bounding sphere.</param>
        /// <param name="subMeshResult">Index of DFSubMesh intersected by ray, or -1 on miss.</param>
        /// <param name="planeResult">Index of DFPlane interested by ray, or -1 on miss.</param>
        /// <returns>Distance to intersection, or NULL if miss.</returns>
        public static float? RayIntersectsDFMesh(Ray ray,
                                            Matrix modelTransform,
                                            ref ModelManager.ModelData model,
                                            out bool insideBoundingSphere,
                                            out int subMeshResult,
                                            out int planeResult)
        {
            // Reset results
            insideBoundingSphere = false;
            subMeshResult = -1;
            planeResult = -1;

            // Transform ray back to object space
            Matrix inverseTransform = Matrix.Invert(modelTransform);
            ray.Position = Vector3.Transform(ray.Position, inverseTransform);
            ray.Direction = Vector3.TransformNormal(ray.Direction, inverseTransform);

            // Test against bounding sphere
            if (ray.Intersects(model.BoundingSphere) == null)
            {
                // No intersection possible
                return null;
            }
            else
            {
                // Ray is inside model bounding sphere
                insideBoundingSphere = true;
            }

            // Test each submesh
            float dot;
            float? intersection = null;
            float? closestIntersection = null;
            Vector3 normal, vertex1, vertex2, vertex3;
            int subMeshIndex = 0;
            foreach (var subMesh in model.DFMesh.SubMeshes)
            {
                // Test each plane of this submesh
                int planeIndex = 0;
                foreach (var plane in subMesh.Planes)
                {
                    // Get plane normal (all points in plane have same normal)
                    normal.X = plane.Points[0].NX;
                    normal.Y = -plane.Points[0].NY;
                    normal.Z = -plane.Points[0].NZ;

                    // Cull planes facing away from ray
                    dot = Vector3.Dot(normal, ray.Direction);
                    if (dot > 0f)
                    {
                        planeIndex++;
                        continue;
                    }

                    // Get shared point (vertex1)
                    vertex1.X = plane.Points[0].X;
                    vertex1.Y = -plane.Points[0].Y;
                    vertex1.Z = -plane.Points[0].Z;

                    // Walk through triangle fan
                    for (int p = 0; p < plane.Points.Length - 2; p++)
                    {
                        // Get second point (vertex2)
                        vertex2.X = plane.Points[p + 1].X;
                        vertex2.Y = -plane.Points[p + 1].Y;
                        vertex2.Z = -plane.Points[p + 1].Z;

                        // Get third point (vertex3)
                        vertex3.X = plane.Points[p + 2].X;
                        vertex3.Y = -plane.Points[p + 2].Y;
                        vertex3.Z = -plane.Points[p + 2].Z;

                        // Test intersection
                        RayIntersectsTriangle(ref ray, ref vertex1, ref vertex2, ref vertex3, out intersection);
                        if (intersection != null)
                        {
                            // Test for closest intersection so far
                            if (closestIntersection == null || intersection < closestIntersection)
                            {
                                // Update closest intersection
                                closestIntersection = intersection;
                                subMeshResult = subMeshIndex;
                                planeResult = planeIndex;
                            }
                        }
                    }

                    // Increment planeIndex
                    planeIndex++;
                }

                // Increment subMeshIndex
                subMeshIndex++;
            }

            return closestIntersection;
        }

        #endregion

        #region Microsoft Ms-PL

        //-----------------------------------------------------------------------------
        // The following code is bound to the Microsoft Permissive License (Ms-PL).
        // Microsoft XNA Community Game Platform
        // Copyright (C) Microsoft Corporation. All rights reserved.
        //-----------------------------------------------------------------------------

        /// <summary>
        /// Checks whether a ray intersects a triangle. This uses the algorithm
        /// developed by Tomas Moller and Ben Trumbore, which was published in the
        /// Journal of Graphics Tools, volume 2, "Fast, Minimum Storage Ray-Triangle
        /// Intersection".
        /// </summary>
        public static void RayIntersectsTriangle(ref Ray ray,
                                          ref Vector3 vertex1,
                                          ref Vector3 vertex2,
                                          ref Vector3 vertex3, out float? result)
        {
            // Compute vectors along two edges of the triangle.
            Vector3 edge1, edge2;

            Vector3.Subtract(ref vertex2, ref vertex1, out edge1);
            Vector3.Subtract(ref vertex3, ref vertex1, out edge2);

            // Compute the determinant.
            Vector3 directionCrossEdge2;
            Vector3.Cross(ref ray.Direction, ref edge2, out directionCrossEdge2);

            float determinant;
            Vector3.Dot(ref edge1, ref directionCrossEdge2, out determinant);

            // If the ray is parallel to the triangle plane, there is no collision.
            if (determinant > -float.Epsilon && determinant < float.Epsilon)
            {
                result = null;
                return;
            }

            float inverseDeterminant = 1.0f / determinant;

            // Calculate the U parameter of the intersection point.
            Vector3 distanceVector;
            Vector3.Subtract(ref ray.Position, ref vertex1, out distanceVector);

            float triangleU;
            Vector3.Dot(ref distanceVector, ref directionCrossEdge2, out triangleU);
            triangleU *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleU < 0 || triangleU > 1)
            {
                result = null;
                return;
            }

            // Calculate the V parameter of the intersection point.
            Vector3 distanceCrossEdge1;
            Vector3.Cross(ref distanceVector, ref edge1, out distanceCrossEdge1);

            float triangleV;
            Vector3.Dot(ref ray.Direction, ref distanceCrossEdge1, out triangleV);
            triangleV *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleV < 0 || triangleU + triangleV > 1)
            {
                result = null;
                return;
            }

            // Compute the distance along the ray to the triangle.
            float rayDistance;
            Vector3.Dot(ref edge2, ref distanceCrossEdge1, out rayDistance);
            rayDistance *= inverseDeterminant;

            // Is the triangle behind the ray origin?
            if (rayDistance < 0)
            {
                result = null;
                return;
            }

            result = rayDistance;
        }

        #endregion

    }

}
