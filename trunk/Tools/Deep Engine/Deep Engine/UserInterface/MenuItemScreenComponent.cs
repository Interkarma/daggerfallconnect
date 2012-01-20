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
#endregion

namespace DeepEngine.UserInterface
{

    /// <summary>
    /// A standalone menu item that raises an event when clicked.
    /// </summary>
    public class MenuItemScreenComponent : TextItemScreenComponent
    {

        #region Fields

        bool mouseOverText = false;
        public event EventHandler OnMouseEnter;
        public event EventHandler OnMouseLeave;
        public event EventHandler OnMouseClick;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="text">Menu text.</param>
        /// <param name="position">Menu screen position.</param>
        /// <param name="font">Menu font.</param>
        public MenuItemScreenComponent(DeepCore core, SpriteFont font, string text)
            : base(core, font, text)
        {
        }

        #endregion

        #region BaseScreenComponent Overrides

        /// <summary>
        /// Called when screen component should update itself.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last update.</param>
        public override void Update(TimeSpan elapsedTime)
        {
            // Do nothing if disabled
            if (!enabled)
                return;

            // Update state
            MouseState lastMouseState = core.Input.PreviousMouseState;
            MouseState mouseState = core.Input.MouseState;

            // Check if mouse is inside text rectangle
            if (Rectangle.Contains(mouseState.X, mouseState.Y))
            {
                if (mouseOverText == false)
                {
                    // Raise mouse entered event
                    if (OnMouseEnter != null)
                        OnMouseEnter(this, null);

                    mouseOverText = true;
                }
            }
            else
            {
                if (mouseOverText == true)
                {
                    // Raise mouse leaving event
                    if (OnMouseLeave != null)
                        OnMouseLeave(this, null);

                    mouseOverText = false;
                }
            }

            // Handle mouse click
            if (mouseOverText &&
                lastMouseState.LeftButton == ButtonState.Pressed &&
                mouseState.LeftButton == ButtonState.Released)
            {
                if (OnMouseClick != null)
                    OnMouseClick(this, null);
            }
        }

        #endregion

    }

}
