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
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using XNALibrary;
using DaggerfallModelling.ViewControls;
#endregion

namespace DaggerfallModelling.ViewComponents
{

    /// <summary>
    /// Component to draw a sky background in outsoor scenes.
    /// </summary>
    public class SkyComponent : ComponentBase
    {

        #region Class Variables

        SkyFile skyFile;
        Texture2D westTexture;
        Texture2D eastTexture;
        Color suggestedClearColor;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public SkyComponent(ViewHost host)
            : base(host)
        {
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Called when component mnust initialise.
        /// </summary>
        public override void Initialize()
        {
            // Load a sample sky file
            skyFile = new SkyFile(
                Path.Combine(host.Arena2Path, "SKY09.DAT"),
                FileUsage.UseDisk,
                true);

            // Load a sample sky
            LoadTextures(16);
        }

        /// <summary>
        /// Called when view component should tick animation.
        /// </summary>
        public override void Tick()
        {
            if (Enabled)
            {
            }
        }

        /// <summary>
        /// Called when view component should redraw.
        /// </summary>
        public override void Draw()
        {
            if (host.IsReady && this.Enabled)
            {
                DrawTestSky();
            }
        }

        #endregion

        #region Private Methods

        private void LoadTextures(int index)
        {
            // Get images for both sides of sky
            skyFile.Palette = skyFile.GetDFPalette(index);
            DFBitmap west = skyFile.GetBitmapFormat(0, index, 0, DFBitmap.Formats.ARGB);
            DFBitmap east = skyFile.GetBitmapFormat(1, index, 0, DFBitmap.Formats.ARGB);

            // Create textures
            westTexture = new Texture2D(host.GraphicsDevice, west.Width, west.Height, 1, TextureUsage.None, SurfaceFormat.Color);
            eastTexture = new Texture2D(host.GraphicsDevice, east.Width, east.Height, 1, TextureUsage.None, SurfaceFormat.Color);

            // Set data
            westTexture.SetData<byte>(0, null, west.Data, 0, west.Width * west.Height * 4, SetDataOptions.None);
            eastTexture.SetData<byte>(0, null, east.Data, 0, west.Width * west.Height * 4, SetDataOptions.None);

            // TODO: Get suggested clear colour from bottom of sky image
        }

        private void DrawTestSky()
        {
            // TEST: Just plant a sky in the corner for now
            host.SpriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);

            host.SpriteBatch.Draw(
                eastTexture,
                new Rectangle(0, -440, 2048, 880),
                Color.White);

            host.SpriteBatch.End();
        }

        #endregion

    }

}
