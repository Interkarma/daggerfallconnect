#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;

#endregion

namespace DaggerfallModelling.BrowserControls
{
    /// <summary>
    /// Renders a small map representing a location.
    /// </summary>
    public class MapBlockBrowser : Control
    {
        #region Class Variables

        private BlocksFile blocksFile;
        private MapsFile mapsFile;
        private DFLocation dfLocation;

        private int mapSquares = -1;
        private int gridSpacingX;
        private int gridSpacingY;
        private int startX;
        private int startY;
        private Bitmap locationBitmap;

        #endregion

        #region Public Properties

        public MapsFile MapsFile
        {
            get { return mapsFile; }
            set { mapsFile = value; }
        }

        public BlocksFile BlocksFile
        {
            get { return blocksFile; }
            set { blocksFile = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MapBlockBrowser()
        {
            // Set value of double-buffering style bits to true
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        }

        #endregion

        #region Public Methods

        public void SetLocation(int region, int location)
        {
            // Get location
            dfLocation = mapsFile.GetLocation(region, location);
            if (string.IsNullOrEmpty(dfLocation.Name))
                return;

            // Get map square from longest side
            if (dfLocation.Exterior.ExteriorData.Width > dfLocation.Exterior.ExteriorData.Height)
                mapSquares = dfLocation.Exterior.ExteriorData.Width;
            else
                mapSquares = dfLocation.Exterior.ExteriorData.Height;

            // Calculate values using in drawing
            gridSpacingX = this.Width / mapSquares;
            gridSpacingY = this.Height / mapSquares;
            startX = (this.Width - gridSpacingX * mapSquares) / 2;
            startY = (this.Height - gridSpacingY * mapSquares) / 2;

            // Create location image
            CreateLocationBitmap();

            this.Invalidate();
        }

        #endregion

        #region Overrides

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (DesignMode)
            {
                base.OnPaintBackground(e);
                return;
            }

            // Handle location not set
            if (string.IsNullOrEmpty(dfLocation.Name))
            {
                base.OnPaintBackground(e);
                return;
            }

            // Draw gradiant
            Brush brush = new LinearGradientBrush(this.ClientRectangle, Color.FromArgb(240, 240, 240), Color.FromArgb(190, 190, 190), LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(brush, this.ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Handle design mode
            if (DesignMode)
            {
                PaintUsingSystemDrawing(e.Graphics, this.Text + "\n\n" + GetType());
                return;
            }

            // Handle connect objects null
            if (blocksFile == null || mapsFile == null)
                return;

            // Handle location not set
            if (string.IsNullOrEmpty(dfLocation.Name))
                return;

            // Draw location bitmap
            e.Graphics.DrawImageUnscaled(locationBitmap, new Point(startX, startY));

            // Draw grid overlay
            Pen pen = new Pen(Color.SlateGray);
            for (int y = 0; y < mapSquares + 1; y++)
            {
                e.Graphics.DrawLine(pen, startY + y * gridSpacingY, startY, startY + y * gridSpacingY, startY + gridSpacingY * mapSquares);
                for (int x = 0; x < mapSquares + 1; x++)
                {
                    e.Graphics.DrawLine(pen, startX, startX + x * gridSpacingX, startX + gridSpacingX * mapSquares, startX + x * gridSpacingX);
                }
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// If we do not have a valid graphics device (for instance if the device
        /// is lost, or if we are running inside the Form designer), we must use
        /// regular System.Drawing method to display a status message.
        /// </summary>
        protected virtual void PaintUsingSystemDrawing(Graphics graphics, string text)
        {
            graphics.Clear(SystemColors.Control);

            using (Brush brush = new SolidBrush(Color.Black))
            {
                using (StringFormat format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;

                    graphics.DrawString(text, Font, brush, ClientRectangle, format);
                }
            }
        }

        #endregion

        #region Private Methods

        private void CreateLocationBitmap()
        {
            locationBitmap = new Bitmap(gridSpacingX * mapSquares, gridSpacingY * mapSquares);

            int xpos = 0;
            int ypos = locationBitmap.Height - gridSpacingY;
            Graphics gr = Graphics.FromImage(locationBitmap);

            DFManualImage dfManualImage = new DFManualImage();
            dfManualImage.Palette.MakeAutomap();
            for (int y = 0; y < dfLocation.Exterior.ExteriorData.Height; y++)
            {
                for (int x = 0; x < dfLocation.Exterior.ExteriorData.Width; x++)
                {
                    string blockName = mapsFile.GetRmbBlockName(ref dfLocation, x, y);
                    dfManualImage.DFBitmap = blocksFile.GetBlockAutoMap(blockName);

                    Bitmap bm = dfManualImage.GetManagedBitmap(0, 0, false, true);
                    gr.DrawImage(bm, xpos, ypos, gridSpacingY, gridSpacingY);

                    xpos += gridSpacingX;
                }

                ypos -= gridSpacingY;
                xpos = 0;
            }
        }

        #endregion
    }
}