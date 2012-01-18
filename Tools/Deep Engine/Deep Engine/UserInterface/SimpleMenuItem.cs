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
    public class SimpleMenuItem : BaseScreenComponent
    {

        #region Fields

        string text;
        Vector2 position;
        Vector2 size;
        Rectangle rectangle;
        MouseState mouseState, lastMouseState;
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
        /// Gets or sets position on screen.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; SetText(text); }
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

        /// <summary>
        /// Gets screen area occupied by the menu item.
        /// </summary>
        public Rectangle Rectangle
        {
            get { return rectangle; }
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
        public SimpleMenuItem(DeepCore core, string text, Vector2 position, SpriteFont font)
            : base(core)
        {
            this.font = font;
            this.position = position;
            SetText(text);
            lastMouseState = mouseState = Mouse.GetState();
        }

        #endregion

        /// <summary>
        /// Draws menu at current position.
        ///  Must be called between SpriteBatch Begin() & End() methods.
        ///  This allows the caller to group simple menu items as needed
        ///  and apply their own drawing logic.
        /// </summary>
        public void DrawMenu(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(font, text, position, color);
        }

        #region BaseScreenComponent Overrides

        /// <summary>
        /// Called when the component should update itself.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last update.</param>
        public override void Update(TimeSpan elapsedTime)
        {
            // Do nothing if disabled
            if (!enabled)
                return;

            // Update state
            lastMouseState = mouseState;
            mouseState = Mouse.GetState();

            // Check if mouse is inside text rectangle
            if (rectangle.Contains(mouseState.X, mouseState.Y))
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

            // Update rectangle
            rectangle.X = (int)position.X;
            rectangle.Y = (int)position.Y;
            rectangle.Width = (int)size.X;
            rectangle.Height = (int)size.Y;
        }

        #endregion

    }

}
