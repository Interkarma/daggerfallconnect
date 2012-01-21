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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using DeepEngine.Core;
using DeepEngine.Utility;
#endregion

namespace DeepEngine.UserInterface
{

    /// <summary>
    /// An empty component to use in stack panels when creating variable space between items.
    /// </summary>
    public class StackSpacerScreenComponent : BaseScreenComponent
    {

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="size">Size of spacer.</param>
        public StackSpacerScreenComponent(DeepCore core, float size)
            : base(core)
        {
            Size = new Vector2(1, size);
        }

        #endregion

        #region BaseScreenComponent Overrides

        /// <summary>
        /// Called when component should draw itself.
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to draw with.</param>
        internal override void Draw(SpriteBatch spriteBatch)
        {
            // Nothing to draw
        }

        #endregion

    }

}
