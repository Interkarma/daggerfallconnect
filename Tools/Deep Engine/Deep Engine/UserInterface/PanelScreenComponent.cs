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
    /// A screen component that can store other screen components.
    /// </summary>
    public class PanelScreenComponent : BaseScreenComponent
    {

        #region Fields

        ScreenComponentCollection components;
        
        const int backgroundTextureDimension = 32;

        Color backgroundColor;
        protected Texture2D backgroundColorTexture;
        protected Texture2D backgroundTexture;
        protected TextureLayout backgroundTextureLayout = TextureLayout.Tile;

        bool bordersSet = false;
        Texture2D topBorder, bottomBorder, leftBorder, rightBorder;
        Texture2D tlBorder, trBorder, blBorder, brBorder;

        #endregion

        #region Properties

        /// <summary>
        /// Gets list of child screen components.
        /// </summary>
        public ScreenComponentCollection Components
        {
            get { return components; }
        }

        /// <summary>
        /// Gets or sets background colour.
        /// </summary>
        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set { SetBackgroundColor(value); }
        }

        /// <summary>
        /// Gets or sets background texture.
        ///  Will replace BackgroundColor if set.
        /// </summary>
        public Texture2D BackgroundTexture
        {
            get { return backgroundTexture; }
            set { backgroundTexture = value; }
        }

        /// <summary>
        /// Gets or sets background texture layout behaviour.
        /// </summary>
        public TextureLayout BackgroundTextureLayout
        {
            get { return backgroundTextureLayout; }
            set { backgroundTextureLayout = value; }
        }

        public int TopMargin { get; set; }
        public int BottomMargin { get; set; }
        public int LeftMargin { get; set; }
        public int RightMargin { get; set; }

        /// <summary>
        /// Gets or sets flag to enable/disable border.
        ///  Must use SetBorderTextures() before enabling border.
        /// </summary>
        public bool EnableBorder { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="position">Position of component relative to parent panel.</param>
        /// <param name="size">Size of component.</param>
        public PanelScreenComponent(DeepCore core, Vector2 position, Vector2 size)
            : base(core, position, size)
        {
            this.position = position;
            this.size = size;
            this.components = new ScreenComponentCollection(this);
            SetBackgroundColor(Color.Transparent);
        }

        #endregion

        #region BaseScreenComponent Overrides

        /// <summary>
        /// Called when the component should update itself.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last update.</param>
        public override void Update(TimeSpan elapsedTime)
        {
            // Update base
            base.Update(elapsedTime);

            // Update child components
            foreach (BaseScreenComponent component in components)
            {
                component.Update(elapsedTime);
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

            // Set scissor rect
            core.GraphicsDevice.ScissorRectangle = Rectangle;

            // Draw background texture
            if (backgroundTexture != null)
            {
                switch (backgroundTextureLayout)
                {
                    case TextureLayout.Stretch:
                        spriteBatch.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
                        spriteBatch.Draw(backgroundTexture, Rectangle, Color.White);
                        break;
                    case TextureLayout.Tile:
                        spriteBatch.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                        spriteBatch.Draw(backgroundTexture, Position, Rectangle, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                        break;
                }
            }
            else if (backgroundColor != Color.Transparent && backgroundColorTexture != null)
            {
                spriteBatch.Draw(backgroundColorTexture, Rectangle, Color.White);
            }

            // Draw border
            if (EnableBorder && bordersSet)
            {
                DrawBorder(spriteBatch);
            }

            // Draw child components
            foreach (BaseScreenComponent component in components)
            {
                component.Draw(spriteBatch);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets value for one or more margins using flags.
        /// </summary>
        /// <param name="margin">Margin flags.</param>
        /// <param name="value">Value to set.</param>
        public void SetMargins(Margins margin, int value)
        {
            if (margin.HasFlag(Margins.Top))
                TopMargin = value;
            if (margin.HasFlag(Margins.Bottom))
                BottomMargin = value;
            if (margin.HasFlag(Margins.Left))
                LeftMargin = value;
            if (margin.HasFlag(Margins.Right))
                RightMargin = value;
        }

        /// <summary>
        /// Sets border textures and enables borders.
        /// </summary>
        /// <param name="topLeft">Top-Left texture.</param>
        /// <param name="top">Top texture.</param>
        /// <param name="topRight">Top-Right texture.</param>
        /// <param name="left">Left texture.</param>
        /// <param name="right">Right texture.</param>
        /// <param name="bottomLeft">Bottom-Left texture.</param>
        /// <param name="bottom">Bottom texture.</param>
        /// <param name="bottomRight">Bottom-Right texture.</param>
        public void SetBorderTextures(
            Texture2D topLeft,
            Texture2D top,
            Texture2D topRight,
            Texture2D left,
            Texture2D right,
            Texture2D bottomLeft,
            Texture2D bottom,
            Texture2D bottomRight)
        {
            // Save texture references
            tlBorder = topLeft;
            topBorder = top;
            trBorder = topRight;
            leftBorder = left;
            rightBorder = right;
            blBorder = bottomLeft;
            bottomBorder = bottom;
            brBorder = bottomRight;

            // Set flag
            bordersSet = true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets background colour and updates texture.
        /// </summary>
        /// <param name="color">Color to use as background colour.</param>
        private void SetBackgroundColor(Color color)
        {
            // Create buffer filled with colour
            int dimensionSquared = backgroundTextureDimension * backgroundTextureDimension;
            Color[] buffer = new Color[dimensionSquared];
            for (int i = 0; i < dimensionSquared; i++)
            {
                buffer[i] = color;
            }

            // Create new background texture
            backgroundColorTexture = new Texture2D(core.GraphicsDevice, backgroundTextureDimension, backgroundTextureDimension, false, SurfaceFormat.Color);
            backgroundColorTexture.SetData<Color>(buffer);

            // Store colour
            backgroundColor = color;
        }

        /// <summary>
        /// Draws border using textures provided.
        /// </summary>
        private void DrawBorder(SpriteBatch spriteBatch)
        {
            // Set linear wrap
            spriteBatch.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            
            // Get draw area
            Rectangle drawRect = spriteBatch.GraphicsDevice.ScissorRectangle;

            // Top-left
            Vector2 tl;
            tl.X = drawRect.X;
            tl.Y = drawRect.Y;

            // Top-right
            Vector2 tr;
            tr.X = drawRect.Right - trBorder.Width;
            tr.Y = drawRect.Y;

            // Bottom-left
            Vector2 bl;
            bl.X = drawRect.X;
            bl.Y = drawRect.Bottom - brBorder.Height;

            // Bottom-right
            Vector2 br;
            br.X = drawRect.Right - brBorder.Width;
            br.Y = drawRect.Bottom - brBorder.Height;

            // Top
            Rectangle top;
            top.X = drawRect.X + tlBorder.Width;
            top.Y = drawRect.Y;
            top.Width = drawRect.Width - tlBorder.Width - trBorder.Width;
            top.Height = topBorder.Height;

            // Left
            Rectangle left;
            left.X = drawRect.X;
            left.Y = drawRect.Y + tlBorder.Height;
            left.Width = leftBorder.Width;
            left.Height = drawRect.Height - tlBorder.Height - blBorder.Height;

            // Right
            Rectangle right;
            right.X = drawRect.Right - rightBorder.Width;
            right.Y = drawRect.Y + trBorder.Height;
            right.Width = rightBorder.Width;
            right.Height = drawRect.Height - trBorder.Height - brBorder.Height;

            // Bottom
            Rectangle bottom;
            bottom.X = drawRect.X + blBorder.Width;
            bottom.Y = drawRect.Bottom - bottomBorder.Height;
            bottom.Width = drawRect.Width - blBorder.Width - brBorder.Width;
            bottom.Height = bottomBorder.Height;

            // Draw corners
            spriteBatch.Draw(tlBorder, tl, Color.White);
            spriteBatch.Draw(trBorder, tr, Color.White);
            spriteBatch.Draw(blBorder, bl, Color.White);
            spriteBatch.Draw(brBorder, br, Color.White);

            // Draw edges
            spriteBatch.Draw(topBorder, top, top, Color.White);
            spriteBatch.Draw(leftBorder, left, left, Color.White);
            spriteBatch.Draw(rightBorder, right, right, Color.White);
            spriteBatch.Draw(bottomBorder, bottom, bottom, Color.White);
        }

        #endregion

    }

}
