// Project:         DaggerfallConnect
// Description:     Read data from Daggerfall's file formats.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Imports

using System;
using System.Text;

#endregion

namespace DaggerfallConnect.Utility
{
    /// <summary>
    /// This class is based on Dave Humphrey's DF_3DSTex.CPP.
    ///  Modified for Daggerfall Connect Library by Gavin Clayton "Interkarma" (www.dfworkshop.net).
    ///  For original version download DFTo3DS at www.uesp.net.
    /// </summary>
    internal class FaceUVTool
    {
        #region Class Structures

        /// <summary>
        /// Describes a single Daggerfall-native vertex.
        /// </summary>
        internal struct DFPurePoint
        {
            public Int32 x;
            public Int32 y;
            public Int32 z;
            public Int32 nx;
            public Int32 ny;
            public Int32 nz;
            public Int32 u;
            public Int32 v;
        }

        /// <summary>
        /// Used to store a 2D point.
        /// </summary>
        private struct DF2DPoint
        {
            public Int32 x;
            public Int32 y;
        }

        /// <summary>
        /// Local type for matrix conversion parameters.
        /// </summary>
        private struct df3duvparams_lt
        {
            public float[] X;
            public float[] Y;
            public float[] Z;
            public float[] U;
            public float[] V;
        }

        /// <summary>
        /// Used to convert XYZ point coordinates to DF UV coordinates.
        /// </summary>
        private struct df3duvmatrix_t
        {
            public float UA;
            public float UB;
            public float UC;
            public float UD;
            public float VA;
            public float VB;
            public float VC;
            public float VD;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Calculates absolute UV values for all points of a face.
        /// </summary>
        /// <param name="FaceVertsIn">Source array of native point values.</param>
        /// <param name="FaceVertsOut">Destination array for calculated UV values.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool ComputeFaceUVCoordinates(ref DFPurePoint[] FaceVertsIn, ref DFPurePoint[] FaceVertsOut)
        {
            // Get first three vertices of ngon (these three vertices are always non-collinear)
            Vector3 P0 = new Vector3(FaceVertsIn[0].x, FaceVertsIn[0].y, FaceVertsIn[0].z);
            Vector3 P1 = new Vector3(FaceVertsIn[1].x, FaceVertsIn[1].y, FaceVertsIn[1].z);
            Vector3 P2 = new Vector3(FaceVertsIn[2].x, FaceVertsIn[2].y, FaceVertsIn[2].z);

            // Create coplanar vectors from p1->p0 and p2->p0
            Vector3 V0 = P1 - P0;
            Vector3 V1 = P2 - P0;

            // Orthogonalize V1
            V1 = V1 - (V0 * (V1.DotProduct(V0) / (V0.DotProduct(V0))));

            // Normalize both vectors
            V0.Normalize();
            V1.Normalize();

            // Compute first three vertices in 2D space
            DF2DPoint p0, p1, p2;
            p0.x = (Int32)P0.DotProduct(V0);
            p0.y = (Int32)P0.DotProduct(V1);
            p1.x = (Int32)P1.DotProduct(V0);
            p1.y = (Int32)P1.DotProduct(V1);
            p2.x = (Int32)P2.DotProduct(V0);
            p2.y = (Int32)P2.DotProduct(V1);

            // Initialise the params struct
            df3duvparams_lt Params = new df3duvparams_lt();
            Params.X = new float[4];
            Params.Y = new float[4];
            Params.Z = new float[4];
            Params.U = new float[4];
            Params.V = new float[4];

            // Initialise the conversion matrix
            df3duvmatrix_t Matrix = new df3duvmatrix_t();
            Matrix.UA = 1.0f;
            Matrix.UB = 0.0f;
            Matrix.UC = 0.0f;
            Matrix.UD = 0.0f;
            Matrix.VA = 0.0f;
            Matrix.VB = 1.0f;
            Matrix.VC = 0.0f;
            Matrix.UD = 0.0f;

            // Store the first 3 points of texture coordinates
            Params.U[0] = FaceVertsIn[0].u;
            Params.U[1] = FaceVertsIn[1].u + Params.U[0];
            Params.U[2] = FaceVertsIn[2].u + Params.U[1];
            Params.V[0] = FaceVertsIn[0].v;
            Params.V[1] = FaceVertsIn[1].v + Params.V[0];
            Params.V[2] = FaceVertsIn[2].v + Params.V[1];

            // Get and store the 1st point coordinates in face
            Params.X[0] = p0.x;
            Params.Y[0] = p0.y;
            Params.Z[0] = 0;

            // Get and store the 2nd point coordinates in face
            Params.X[1] = p1.x;
            Params.Y[1] = p1.y;
            Params.Z[1] = 0;

            // Get and store the 3rd point coordinates in face
            Params.X[2] = p2.x;
            Params.Y[2] = p2.y;
            Params.Z[2] = 0;

            // Compute the solution using an XY linear equation
            if (!l_ComputeDFUVMatrixXY(ref Matrix, ref Params))
                return false;

            // Assign matrix to all points if successful
            Int32 u = 0, v = 0;
            for (int point = 0; point < FaceVertsIn.Length; point++)
            {
                if (point > 2)
                {
                    // Use generated matrix to calculate UV value from 2D point
                    DF2DPoint pn;
                    Vector3 PN = new Vector3(FaceVertsIn[point].x, FaceVertsIn[point].y, FaceVertsIn[point].z);
                    pn.x = (Int32)PN.DotProduct(V0);
                    pn.y = (Int32)PN.DotProduct(V1);
                    u = (Int32)((pn.x * Matrix.UA) + (pn.y * Matrix.UB) + Matrix.UD);
                    v = (Int32)((pn.x * Matrix.VA) + (pn.y * Matrix.VB) + Matrix.VD);
                }
                else if (point == 0)
                {
                    // UV[0] is absolute
                    u = FaceVertsIn[0].u;
                    v = FaceVertsIn[0].v;
                }
                else if (point == 1)
                {
                    // UV[1] is a delta from UV[0]
                    u = FaceVertsIn[0].u + FaceVertsIn[1].u;
                    v = FaceVertsIn[0].v + FaceVertsIn[1].v;
                }
                else if (point == 2)
                {
                    // UV[2] is a delta from UV[1] + UV[0]
                    u = FaceVertsIn[0].u + FaceVertsIn[1].u + FaceVertsIn[2].u;
                    v = FaceVertsIn[0].v + FaceVertsIn[1].v + FaceVertsIn[2].v;
                }

                // Write outgoing point
                FaceVertsOut[point].x = FaceVertsIn[point].x;
                FaceVertsOut[point].y = FaceVertsIn[point].y;
                FaceVertsOut[point].z = FaceVertsIn[point].z;
                FaceVertsOut[point].nx = FaceVertsIn[point].nx;
                FaceVertsOut[point].ny = FaceVertsIn[point].ny;
                FaceVertsOut[point].nz = FaceVertsIn[point].nz;
                FaceVertsOut[point].u = u;
                FaceVertsOut[point].v = v;
            }

            return true;
        }

        #endregion

        #region Private Methods

        /*===========================================================================
        *
        * Local Function - boolean l_ComputeDFUVMatrixXY (Matrix, Params);
        *
        * Computes the UV conversion parameters from the given input based on
        * the formula:
        *			U = AX + BY + D
        *
        * Returns FALSE on any error.  For use on faces with 0 Z-coordinates.
        *
        *=========================================================================*/
        private bool l_ComputeDFUVMatrixXY(ref df3duvmatrix_t Matrix, ref df3duvparams_lt Params)
        {
            float Determinant;
            float[] Xi = new float[3];
            float[] Yi = new float[3];
            float[] Zi = new float[3];

            /* Compute the determinant of the coefficient matrix */
            Determinant = Params.X[0] * Params.Y[1] + Params.Y[0] * Params.X[2] +
            Params.X[1] * Params.Y[2] - Params.Y[1] * Params.X[2] -
            Params.Y[0] * Params.X[1] - Params.X[0] * Params.Y[2];

            /* Check for a singular matrix indicating no valid solution */
            if (Determinant == 0)
            {
                return false;
            }

            /* Compute parameters of the the inverted XYZ matrix */
            Xi[0] = (Params.Y[1] - Params.Y[2]) / Determinant;
            Xi[1] = (-Params.X[1] + Params.X[2]) / Determinant;
            Xi[2] = (Params.X[1] * Params.Y[2] - Params.X[2] * Params.Y[1]) / Determinant;

            Yi[0] = (-Params.Y[0] + Params.Y[2]) / Determinant;
            Yi[1] = (Params.X[0] - Params.X[2]) / Determinant;
            Yi[2] = (-Params.X[0] * Params.Y[2] + Params.X[2] * Params.Y[0]) / Determinant;

            Zi[0] = (Params.Y[0] - Params.Y[1]) / Determinant;
            Zi[1] = (-Params.X[0] + Params.X[1]) / Determinant;
            Zi[2] = (Params.X[0] * Params.Y[1] - Params.X[1] * Params.Y[0]) / Determinant;

            /* Compute the UV conversion parameters */
            Matrix.UA = (Params.U[0] * Xi[0] + Params.U[1] * Yi[0] + Params.U[2] * Zi[0]);
            Matrix.UB = (Params.U[0] * Xi[1] + Params.U[1] * Yi[1] + Params.U[2] * Zi[1]);
            Matrix.UC = (float)0.0;
            Matrix.UD = (Params.U[0] * Xi[2] + Params.U[1] * Yi[2] + Params.U[2] * Zi[2]); ;

            Matrix.VA = (Params.V[0] * Xi[0] + Params.V[1] * Yi[0] + Params.V[2] * Zi[0]);
            Matrix.VB = (Params.V[0] * Xi[1] + Params.V[1] * Yi[1] + Params.V[2] * Zi[1]);
            Matrix.VC = (float)0.0;
            Matrix.VD = (Params.V[0] * Xi[2] + Params.V[1] * Yi[2] + Params.V[2] * Zi[2]);

            return true;
        }
        /*===========================================================================
         *		End of Function l_ComputeDFUVMatrixXY()
         *=========================================================================*/

        #endregion
    }
}
