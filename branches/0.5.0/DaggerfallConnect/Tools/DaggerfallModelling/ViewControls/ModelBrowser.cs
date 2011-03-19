#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using XNALibrary;

#endregion

namespace DaggerfallModelling.ViewControls
{
    using GDIColor = System.Drawing.Color;
    using GDIRectangle = System.Drawing.Rectangle;
    using XNAColor = Microsoft.Xna.Framework.Graphics.Color;
    using XNARectangle = Microsoft.Xna.Framework.Rectangle;

    class ModelBrowser : WinFormsGraphicsDevice.GraphicsDeviceControl
    {
        #region Class Variables

        SpriteBatch spriteBatch;

        Arch3dFile arch3dFile;

        float thumbScale = 1.0f;
        const int thumbWidth = 128;
        const int thumbHeight = 128;

        private Font CaptionFont;

        #endregion

        #region Public Properties

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ModelBrowser()
        {
            arch3dFile = new Arch3dFile(Path.Combine("C:\\dosgames\\DAGGER\\ARENA2", "ARCH3D.BSA"), FileUsage.UseDisk, true);
            CaptionFont = new Font(FontFamily.GenericSansSerif, 7.0f, FontStyle.Regular);
        }

        #endregion

        #region Abstract Implementations

        /// <summary>
        /// Initialise the control.
        /// </summary>
        protected override void Initialize()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        /// <summary>
        /// Draw control.
        /// </summary>
        protected override void Draw()
        {
            GraphicsDevice.Clear(XNAColor.Gray);

            //spriteBatch.Begin();
            //spriteBatch.Draw(GetThumbnailTexture(0), new Vector2(0, 0), XNAColor.White);
            //spriteBatch.End();
        }


        #endregion

        #region Overrides

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            this.Invalidate();
        }

        #endregion

        #region Private Methods

        Texture2D GetThumbnailTexture(int index)
        {
            //Bitmap bm = new Bitmap(thumbWidth, thumbHeight);
            //Graphics gr = Graphics.FromImage(bm);
            //gr.DrawRectangle(new Pen(GDIColor.Blue), new GDIRectangle(0, 0, bm.Width, bm.Height));
            //gr.DrawString(index.ToString(), CaptionFont, new Pen(GDIColor.Blue).Brush, 4, 4, StringFormat.GenericDefault);

            return Texture2D.FromFile(GraphicsDevice, "C:\\Test\\bla.png");
        }

        #endregion
    }
}
