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
#endregion

namespace XNALibrary
{

    /// <summary>
    /// Provides static power of two calculations.
    /// </summary>
    public class PowerOfTwo
    {

        /// <summary>
        /// Check if value is a power of 2.
        /// </summary>
        /// <param name="x">Value to check.</param>
        /// <returns>True if power of 2.</returns>
        public static bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }

        /// <summary>
        /// Finds next power of 2 size for value.
        /// </summary>
        /// <param name="x">Value.</param>
        /// <returns>Next power of 2.</returns>
        public static int NextPowerOfTwo(int x)
        {
            int i = 1;
            while (i < x) { i <<= 1; }
            return i;
        }

    }

}
