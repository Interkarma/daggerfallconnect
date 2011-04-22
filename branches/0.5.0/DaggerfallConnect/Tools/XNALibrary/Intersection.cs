// Project:         XNALibrary
// Description:     Simple XNA game library for DaggerfallConnect.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;

#endregion

namespace XNALibrary
{

    /// <summary>
    /// Helper class to provide certain intersection methods.
    /// </summary>
    public class Intersection
    {
        #region Model Intersections

        /// <summary>
        /// Tests if ray intersects DFMesh geometry.
        ///  This method is for testing against native face data only.
        ///  It should not be used for general picking or collision tests.
        ///  RayIntersectsModel() is better used for this purpose.
        /// </summary>
        /// <param name="ray">Ray.</param>
        /// <param name="modelTransform">World space transform.</param>
        /// <param name="model">ModelManager.Model.</param>
        /// <param name="insideBoundingBox">True if ray intersects bounding box.</param>
        /// <param name="subMeshResult">Index of DFSubMesh intersected by ray, or -1 on miss.</param>
        /// <param name="planeResult">Index of DFPlane interested by ray, or -1 on miss.</param>
        public static void RayIntersectsDFMesh(Ray ray,
                                            Matrix modelTransform,
                                            ref ModelManager.Model model,
                                            out bool insideBoundingBox,
                                            out int subMeshResult,
                                            out int planeResult)
        {
            // Reset results
            insideBoundingBox = false;
            subMeshResult = -1;
            planeResult = -1;

            // Transform ray back to object space
            Matrix inverseTransform = Matrix.Invert(modelTransform);
            ray.Position = Vector3.Transform(ray.Position, inverseTransform);
            ray.Direction = Vector3.Transform(ray.Direction, inverseTransform);

            // Test against bounding box
            if (ray.Intersects(model.BoundingBox) == null)
            {
                // No intersection possible
                return;
            }
            else
            {
                // Ray is inside bounding box
                insideBoundingBox = true;
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
                        vertex2.X = plane.Points[p+1].X;
                        vertex2.Y = -plane.Points[p+1].Y;
                        vertex2.Z = -plane.Points[p+1].Z;

                        // Get third point (vertex3)
                        vertex3.X = plane.Points[p+2].X;
                        vertex3.Y = -plane.Points[p+2].Y;
                        vertex3.Z = -plane.Points[p+2].Z;

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

                                // Break out of this plane to continue searching
                                // for a closer match.
                                //break;
                            }
                        }
                    }

                    // Increment faceIndex
                    planeIndex++;
                }

                // Increment subMeshIndex
                subMeshIndex++;
            }
        }

        /// <summary>
        /// Tests if ray intersects ModelManager.Model geometry.
        ///  Use this method for all triangle picking and collision tests in a scene.
        /// </summary>
        /// <param name="ray">Ray.</param>
        /// <param name="modelTransform">World space transform.</param>
        /// <param name="model">ModelManager.Model.</param>
        /// <param name="subMesh">Index of Model.SubMeshes intersected by ray, or NULL on miss.</param>
        /// <param name="face">Index of triangle interested by ray, or NULL on miss.</param>
        public static void RayIntersectsModel(Ray ray,
                                            Matrix modelTransform,
                                            ref ModelManager.Model model,
                                            out int? subMeshResult,
                                            out int? faceResult)
        {
            // Reset results
            subMeshResult = null;
            faceResult = null;
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
