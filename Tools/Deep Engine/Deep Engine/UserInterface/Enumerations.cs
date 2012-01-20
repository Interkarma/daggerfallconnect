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
using Microsoft.Xna.Framework.Graphics;
using DeepEngine.Core;
using DeepEngine.Player;
#endregion

namespace DeepEngine.UserInterface
{

    /// <summary>
    /// Defines margins used in panels.
    /// </summary>
    [Flags]
    public enum Margin
    {
        Top = 1,
        Bottom = 2,
        Left = 4,
        Right = 8,
        All = 15,
    }

    /// <summary>
    /// Defines horizontal text alignment options.
    /// </summary>
    public enum HoriztonalTextAlignment
    {
        None,
        Left,
        Center,
        Right,
    }

    /// <summary>
    /// Defines vertical text alignment options.
    /// </summary>
    public enum VerticalTextAlignment
    {
        None,
        Top,
        Middle,
        Bottom,
    }

    /// <summary>
    /// Defines sides for various operations.
    /// </summary>
    public enum Sides
    {
        None,
        Top,
        Bottom,
        Left,
        Right,
    }

}
