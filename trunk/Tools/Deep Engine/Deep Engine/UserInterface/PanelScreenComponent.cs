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
        protected Texture2D backgroundTexture;

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

        public int TopMargin { get; set; }
        public int BottomMargin { get; set; }
        public int LeftMargin { get; set; }
        public int RightMargin { get; set; }

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
            // Do nothing if disabled
            if (!enabled)
                return;

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

            // Draw background
            if (backgroundColor != Color.Transparent)
            {
                spriteBatch.Draw(backgroundTexture, Rectangle, Color.White);
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
            backgroundTexture = new Texture2D(core.GraphicsDevice, backgroundTextureDimension, backgroundTextureDimension, false, SurfaceFormat.Color);
            backgroundTexture.SetData<Color>(buffer);

            // Store colour
            backgroundColor = color;
        }

        #endregion

    }

}
