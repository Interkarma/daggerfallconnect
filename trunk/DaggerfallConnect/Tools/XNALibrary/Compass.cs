// Project:         DaggerfallModelling
// Description:     Explore and export 3D models from Daggerfall.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using XNALibrary;
#endregion

namespace XNALibrary
{

    /// <summary>
    /// Component to draw a compass.
    /// </summary>
    public class Compass : Component
    {

        #region Class Variables

        // Daggerfall
        private string arena2Path = string.Empty;

        // Textures
        Texture2D compassTexture = null;
        Texture2D compassBoxTexture = null;

        // XNA
        private SpriteBatch spriteBatch;

        #endregion

        #region Properties

        /// <summary>
        /// Gets Arena2 path set at construction.
        /// </summary>
        public string Arena2Path
        {
            get { return arena2Path; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="graphicsDevice">Graphics device used in rendering.</param>
        /// <param name="arena2Path">Arena2 path to locate compass image files.</param>
        public Compass(GraphicsDevice graphicsDevice, string arena2Path)
            : base(graphicsDevice)
        {
            // Store path
            this.arena2Path = arena2Path;

            // Create spritebatch
            spriteBatch = new SpriteBatch(graphicsDevice);

            // Build compass
            BuildCompass();
        }

        #endregion

        #region Abstract Overrides

        /// <summary>
        /// Called when view component should redraw.
        /// </summary>
        public override void Draw(Camera camera)
        {
            if (compassTexture == null ||
                compassBoxTexture == null)
                return;

            if (this.Enabled && camera != null)
            {
                DrawCompass(camera);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Draws compass.
        /// </summary>
        private void DrawCompass(Camera camera)
        {
            const int scale = 2;

            // Calc displacement
            float percent = (360f - camera.Yaw) / 360f;
            int scroll = (int)((float)258 * percent);

            // Compass box
            Rectangle compassBoxRect;
            compassBoxRect.Width = compassBoxTexture.Width * scale;
            compassBoxRect.Height = compassBoxTexture.Height * scale;
            compassBoxRect.X = GraphicsDevice.Viewport.Width - compassBoxRect.Width - 14;
            compassBoxRect.Y = GraphicsDevice.Viewport.Height - compassBoxRect.Height - 4;

            // Compass strip source
            Rectangle compassSrcRect;
            compassSrcRect.Width = 64;
            compassSrcRect.Height = compassTexture.Height;
            compassSrcRect.X = scroll;
            compassSrcRect.Y = 0;

            // Compass strip destination
            Rectangle compassDstRect;
            compassDstRect.X = compassBoxRect.X + 5;
            compassDstRect.Y = compassBoxRect.Y + 4;
            compassDstRect.Width = compassSrcRect.Width * scale;
            compassDstRect.Height = compassSrcRect.Height * scale - 2;

            // Begin drawing
            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);

            // Draw compass box
            spriteBatch.Draw(compassBoxTexture, compassBoxRect, Color.White);

            // Draw compass strip
            spriteBatch.Draw(compassTexture, compassDstRect, compassSrcRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1);

            // End drawing
            spriteBatch.End();
        }

        /// <summary>
        /// Builds compass textures.
        /// </summary>
        private void BuildCompass()
        {
            // Load compass images
            ImageFileReader imageReader = new ImageFileReader(arena2Path);
            imageReader.LibraryType = LibraryTypes.Img;
            DFImageFile compass = imageReader.GetImageFile("COMPASS.IMG");
            DFImageFile compassBox = imageReader.GetImageFile("COMPBOX.IMG");

            // Get DFBitmaps
            DFBitmap compassBitmap = compass.GetBitmapFormat(0, 0, 0, DFBitmap.Formats.ARGB);
            DFBitmap compassBoxBitmap = compassBox.GetBitmapFormat(0, 0, 0, DFBitmap.Formats.ARGB);

            // Create textures
            compassTexture = new Texture2D(
                GraphicsDevice,
                compassBitmap.Width,
                compassBitmap.Height,
                1,
                TextureUsage.None,
                SurfaceFormat.Color);
            compassBoxTexture = new Texture2D(
                GraphicsDevice,
                compassBoxBitmap.Width,
                compassBoxBitmap.Height,
                1,
                TextureUsage.None,
                SurfaceFormat.Color);

            // Set data
            compassTexture.SetData<byte>(
                0,
                null,
                compassBitmap.Data,
                0,
                compassBitmap.Width * compassBitmap.Height * 4,
                SetDataOptions.None);
            compassBoxTexture.SetData<byte>(
                0,
                null,
                compassBoxBitmap.Data,
                0,
                compassBoxBitmap.Width * compassBoxBitmap.Height * 4,
                SetDataOptions.None);
        }

        #endregion

    }

}
