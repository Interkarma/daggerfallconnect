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
    /// A standalone text item supporting outlines, shadows, scaling, fading, and events.
    /// </summary>
    public class TextItemScreenComponent : BaseScreenComponent
    {

        #region Fields

        SpriteFont font;
        string text;

        Color textColor = Color.White;
        Color outlineColor = Color.Black;
        Color shadowColor = Color.Black;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets font.
        /// </summary>
        public SpriteFont Font
        {
            get { return font; }
            set { SetFont(value); }
        }

        /// <summary>
        /// Gets or sets text.
        /// </summary>
        public string Text
        {
            get { return text; }
            set { SetText(value); }
        }

        /// <summary>
        /// Gets or sets text colour.
        /// </summary>
        public Color TextColor
        {
            get { return textColor; }
            set { textColor = value; }
        }

        /// <summary>
        /// Gets or sets outline colour.
        /// </summary>
        public Color OutlineColor
        {
            get { return outlineColor; }
            set { outlineColor = value; }
        }

        /// <summary>
        /// Gets or sets shadow colour.
        /// </summary>
        public Color ShadowColor
        {
            get { return shadowColor; }
            set { shadowColor = value; }
        }

        /// <summary>
        /// Gets or sets flag to draw outline.
        /// </summary>
        public bool EnableOutline { get; set; }

        /// <summary>
        /// Gets or sets shadow size. Set to Vector2.Zero to disable shadows.
        /// </summary>
        public Vector2 ShadowSize { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        public TextItemScreenComponent(DeepCore core, SpriteFont font, string text)
            : base(core, Vector2.Zero, Vector2.Zero)
        {
            this.font = font;
            this.text = text;
            UpdateSize();
            UpdatePosition();
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

            // Draw outline
            if (EnableOutline)
            {
                Vector2 position = Parent.Position + Position;
                Vector2 tl = new Vector2(position.X - 1, position.Y - 1);
                Vector2 tr = new Vector2(position.X + 1, position.Y - 1);
                Vector2 bl = new Vector2(position.X - 1, position.Y + 1);
                Vector2 br = new Vector2(position.X + 1, position.Y + 1);
                spriteBatch.DrawString(font, text, tl, OutlineColor);
                spriteBatch.DrawString(font, text, tr, OutlineColor);
                spriteBatch.DrawString(font, text, bl, OutlineColor);
                spriteBatch.DrawString(font, text, br, OutlineColor);
            }

            // Draw text
            spriteBatch.DrawString(font, text, Parent.Position + Position, TextColor);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Update size based on text and font.
        /// </summary>
        private void UpdateSize()
        {
            // Measure text using sprite font
            if (font != null && !string.IsNullOrEmpty(text))
                base.Size = font.MeasureString(this.Text);
        }

        /// <summary>
        /// Sets font.
        /// </summary>
        /// <param name="font">Font.</param>
        private void SetFont(SpriteFont font)
        {
            this.font = font;
            UpdateSize();
        }

        /// <summary>
        /// Sets text.
        /// </summary>
        /// <param name="text">Text.</param>
        private void SetText(string text)
        {
            this.text = text;
            UpdateSize();
        }

        #endregion

    }

}
