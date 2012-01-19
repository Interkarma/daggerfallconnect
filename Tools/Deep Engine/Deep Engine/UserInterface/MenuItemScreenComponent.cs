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
    public class MenuItemScreenComponent : BaseScreenComponent
    {

        #region Fields

        string text;
        SpriteFont font;
        Color color = Color.White;

        bool mouseOverText = false;

        public event EventHandler OnMouseEnter;
        public event EventHandler OnMouseLeave;
        public event EventHandler OnMouseClick;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the menu text.
        /// </summary>
        public string Text
        {
            get { return text; }
            set { SetText(value); }
        }

        /// <summary>
        /// Gets size of text.
        /// </summary>
        public override Vector2 Size
        {
            get { return base.Size; }
        }

        /// <summary>
        /// Gets or sets font used for drawing.
        /// </summary>
        public SpriteFont SpriteFont
        {
            get { return font; }
            set { font = value; SetText(text); }
        }

        /// <summary>
        /// Gets or sets colour of menu text.
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }
        
        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="text">Menu text.</param>
        /// <param name="position">Menu screen position.</param>
        /// <param name="font">Menu font.</param>
        public MenuItemScreenComponent(DeepCore core, string text, Vector2 position, SpriteFont font)
            : base(core, position, Vector2.Zero)
        {
            this.font = font;
            this.position = position;
            SetText(text);
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

        /// <summary>
        /// Called when screen component should draw itself.
        ///  Must be called between SpriteBatch Begin() & End() methods.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch to draw with.</param>
        internal override void Draw(SpriteBatch spriteBatch)
        {
            // Do nothing if disabled
            if (!enabled)
                return;

            spriteBatch.DrawString(font, text, position + parent.Position, color);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets text and updates size of string.
        /// </summary>
        /// <param name="text"></param>
        private void SetText(string text)
        {
            // Set text
            this.text = text;

            // Measure string
            size = font.MeasureString(text);
        }

        #endregion

    }

}
