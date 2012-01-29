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
#endregion

namespace DeepEngine.Utility
{

    /// <summary>
    /// Provides some static helper functions for changing colour types between engine and editor.
    /// </summary>
    public class ColorHelper
    {

        /// <summary>
        /// Converts a WinForms color to an XNA color.
        /// </summary>
        /// <param name="color">Source WinForms color.</param>
        /// <returns>XNA color.</returns>
        public static Microsoft.Xna.Framework.Color FromWinForms(System.Drawing.Color color)
        {
            Microsoft.Xna.Framework.Color output = new Microsoft.Xna.Framework.Color
            {
                R = color.R,
                G = color.G,
                B = color.B,
                A = color.A,
            };

            return output;
        }

        /// <summary>
        /// Converts an XNA color to a WinForms color.
        /// </summary>
        /// <param name="color">Source XNA color.</param>
        /// <returns>WinForms color.</returns>
        public static System.Drawing.Color FromXNA(Microsoft.Xna.Framework.Color color)
        {
            System.Drawing.Color output = System.Drawing.Color.FromArgb(
                color.A,
                color.R,
                color.G,
                color.B);

            return output;
        }

    }

}
