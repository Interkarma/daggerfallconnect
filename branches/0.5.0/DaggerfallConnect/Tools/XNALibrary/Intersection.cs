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
    class Intersection
    {
        #region Model Intersections

        /// <summary>
        /// Tests if ray intersects DFMesh geometry.
        ///  This method is for testing against native face data only.
        ///  Should not be used for general picking or collision tests.
        ///  RayIntersectsModel() is better optimised for this purpose.
        /// </summary>
        /// <param name="ray">Ray.</param>
        /// <param name="modelTransform">World space transform.</param>
        /// <param name="dfMesh">DFMesh.</param>
        /// <param name="subMesh">Index of DFSubMesh intersected by ray, or NULL on miss.</param>
        /// <param name="face">Index of DFPlane interested by ray, or NULL on miss.</param>
        public static void RayIntersectsDFMesh(Ray ray,
                                            Matrix modelTransform,
                                            ref DFMesh dfMesh,
                                            out int? subMesh,
                                            out int? face)
        {
            subMesh = null;
            face = null;
        }

        /// <summary>
        /// Tests if ray intersects ModelManager.Model geometry.
        ///  Use this method for all general picking and collision tests against
        ///  in a scene.
        /// </summary>
        /// <param name="ray">Ray.</param>
        /// <param name="modelTransform">World space transform.</param>
        /// <param name="model">ModelManager.Model.</param>
        /// <param name="subMesh">Index of Model.SubMeshes intersected by ray, or NULL on miss.</param>
        /// <param name="face">Index of triangle interested by ray, or NULL on miss.</param>
        public static void RayIntersectsModel(Ray ray,
                                            Matrix modelTransform,
                                            ref ModelManager.Model model,
                                            out int? subMesh,
                                            out int? face)
        {
            subMesh = null;
            face = null;
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
