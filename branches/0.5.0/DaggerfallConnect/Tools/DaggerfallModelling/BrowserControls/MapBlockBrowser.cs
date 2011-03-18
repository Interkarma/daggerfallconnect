﻿#region Imports

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
        private DFBlock dfBlock;
        private DFLocation dfLocation;

        private int mapSquares = -1;
        private int gridSpacingX;
        private int gridSpacingY;
        private int startX;
        private int startY;

        private Rectangle[] locationLayoutRects;
        private DungeonBlock[] dungeonLayout;

        private Bitmap locationLayoutBitmap;
        private Bitmap dungeonLayoutBitmap;

        private bool cityModeAllowed = false;
        private bool dungeonModeAllowed = false;
        private bool blockModeAllowed = false;
        private ViewModes viewMode = ViewModes.None;

        #endregion

        #region Drawing Colours

        private Color gridColour = Color.SlateGray;
        private Color dungeonStartColour = Color.FromArgb(85, 154, 154);
        private Color dungeonNormalColour = Color.FromArgb(243, 239, 44);

        #endregion

        #region Class Structures

        public enum ViewModes
        {
            None,
            City,
            Dungeon,
            Block,
        }

        private struct DungeonBlock
        {
            public Rectangle layoutRect;
            public DFBlock.RdbTypes blockType;
        }

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

        #region ModeChanged Event

        public class ModeChangedEventArgs
        {
            public bool CityModeAllowed;
            public bool DungeonModeAllowed;
            public bool BlockModeAllowed;
            public ViewModes ViewMode;
        }

        public delegate void ModeChangedEventHandler(object sender, ModeChangedEventArgs e);
        public event ModeChangedEventHandler ModeChanged;

        protected virtual void RaiseModeChangedEvent()
        {
            // Populate event args based on modes
            ModeChangedEventArgs e = new ModeChangedEventArgs();
            e.CityModeAllowed = cityModeAllowed;
            e.DungeonModeAllowed = dungeonModeAllowed;
            e.BlockModeAllowed = blockModeAllowed;
            e.ViewMode = viewMode;

            // Raise event
            ModeChanged(this, e);
        }

        #endregion

        #region Public Methods

        public void Clear()
        {
            // Clear connect resources
            dfLocation = new DFLocation();
            dfBlock = new DFBlock();

            // Reset modes and redraw
            SetModes();
            this.Invalidate();
        }

        public void ShowLocation(int region, int location)
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

            // Create location map
            CreateLocationBitmap();

            // Conditionally create dungeon map
            if (dfLocation.HasDungeon)
                CreateDungeonMap();
            else
                dungeonLayoutBitmap = null;

            // Enable modes
            SetModes();

            this.Invalidate();
        }

        public void ShowBlock(int block)
        {
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

        #endregion

        #region Painting Methods

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Handle design mode
            if (DesignMode)
            {
                base.OnPaintBackground(e);
                return;
            }

            // Handle not ready
            if (!IsReady())
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

            // Handle not ready
            if (!IsReady())
                return;

            // Render map area
            switch (viewMode)
            {
                case ViewModes.City:
                    e.Graphics.DrawImageUnscaled(locationLayoutBitmap, new Point(startX, startY));
                    DrawGridOverlay(e.Graphics);
                    break;
                case ViewModes.Dungeon:
                    int offx = this.Width / 2 - dungeonLayoutBitmap.Width / 2;
                    int offy = this.Height / 2 - dungeonLayoutBitmap.Height / 2;
                    e.Graphics.DrawImageUnscaled(dungeonLayoutBitmap, new Point(offx, offy));
                    break;
            }
        }

        private void DrawGridOverlay(Graphics gr)
        {
            // Draw grid overlay
            Pen pen = new Pen(Color.SlateGray);
            for (int y = 0; y < mapSquares + 1; y++)
            {
                gr.DrawLine(pen, startY + y * gridSpacingY, startY, startY + y * gridSpacingY, startY + gridSpacingY * mapSquares);
                for (int x = 0; x < mapSquares + 1; x++)
                {
                    gr.DrawLine(pen, startX, startX + x * gridSpacingX, startX + gridSpacingX * mapSquares, startX + x * gridSpacingX);
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

        private bool IsReady()
        {
            // Handle connect objects null
            if (blocksFile == null || mapsFile == null)
                return false;

            // Handle location not set
            if (string.IsNullOrEmpty(dfLocation.Name))
                return false;

            return true;
        }

        private void SetModes()
        {
            if (!IsReady())
            {
                // Everything off
                cityModeAllowed = false;
                blockModeAllowed = false;
                dungeonModeAllowed = false;
                viewMode = ViewModes.None;
                RaiseModeChangedEvent();
                return;
            }

            // Always enable city mode and block mode
            cityModeAllowed = true;
            blockModeAllowed = true;

            // Only enable dungeon mode when the location has a dungeon
            if (dfLocation.HasDungeon)
                dungeonModeAllowed = true;
            else
                dungeonModeAllowed = false;

            // TEMP: Start in dungeon mode when dungeon present
            if (dfLocation.HasDungeon)
                viewMode = ViewModes.Dungeon;
            else
                viewMode = ViewModes.City;

            // Always start in city mode
            //viewMode = ViewModes.City;

            // Raise event to update form
            RaiseModeChangedEvent();
        }

        private void CreateLocationBitmap()
        {
            locationLayoutBitmap = new Bitmap(gridSpacingX * mapSquares, gridSpacingY * mapSquares);

            int xpos = 0;
            int ypos = locationLayoutBitmap.Height - gridSpacingY;
            Graphics gr = Graphics.FromImage(locationLayoutBitmap);

            // Draw automap background
            //for (int y = 0; y < mapSquares; y++)
            //{
            //    for (int x = 0; x < mapSquares; x++)
            //    {
            //        gr.DrawImage(Properties.Resources.MapBackground, x * gridSpacingX, y * gridSpacingY, gridSpacingX, gridSpacingY);
            //    }
            //}

            // Draw automap tiles
            DFManualImage dfManualImage = new DFManualImage();
            dfManualImage.Palette.MakeAutomap();
            for (int y = 0; y < dfLocation.Exterior.ExteriorData.Height; y++)
            {
                for (int x = 0; x < dfLocation.Exterior.ExteriorData.Width; x++)
                {
                    string blockName = mapsFile.GetRmbBlockName(ref dfLocation, x, y);
                    dfManualImage.DFBitmap = blocksFile.GetBlockAutoMap(blockName, true);

                    Bitmap bm = dfManualImage.GetManagedBitmap(0, 0, false, true);
                    gr.DrawImage(bm, xpos, ypos, gridSpacingY, gridSpacingY);

                    xpos += gridSpacingX;
                }

                ypos -= gridSpacingY;
                xpos = 0;
            }
        }

        private void CreateDungeonMap()
        {
            // Always use same layout size for dungeon blocks
            const int blockSide = 32;

            // Quick pass over blocks to find dimensions of dungeon relative to 0,0
            // This is done because the 0,0 block is not typically at origin.
            // We use these dimensions to calc layout surface and offsets to centre
            int left = 0, right = 0;
            int top = 0, bottom = 0;
            foreach (var block in dfLocation.Dungeon.Blocks)
            {
                if (block.X < left) left = block.X;
                if (block.X > right) right = block.X;
                if (block.Z < top) top = block.Z;
                if (block.Z > bottom) bottom = block.Z;
            }

            // Calc dimensions of layout image
            int layoutWidth = blockSide * ((right - left) + 1);
            int layoutHeight = blockSide * ((bottom - top) + 1);

            // Create layout image
            dungeonLayoutBitmap = new Bitmap(layoutWidth + 1, layoutHeight + 1);

            // Offset rects right and down to fit snugly inside layout surface
            int offx = blockSide * Math.Abs(left);
            int offy = blockSide * Math.Abs(top);

            // Layout blocks
            int index = 0;
            dungeonLayout = new DungeonBlock[dfLocation.Dungeon.Blocks.Length];
            foreach (var block in dfLocation.Dungeon.Blocks)
            {
                // Calc layout rect
                dungeonLayout[index].layoutRect = new Rectangle(
                    block.X * blockSide + offx,
                    block.Z * blockSide + offy,
                    blockSide, blockSide);

                // Get block type
                dungeonLayout[index].blockType = blocksFile.GetRdbType(block.BlockName);

                // Override to start type
                if (block.IsStartingBlock)
                    dungeonLayout[index].blockType = DFBlock.RdbTypes.Start;

                // Increment index
                index++;
            }

            // Render blocks to layout image
            Graphics gr = Graphics.FromImage(dungeonLayoutBitmap);
            foreach (var layout in dungeonLayout)
            {
                // Get colour based on type. Only using the same two
                // colours as Daggerfall here so the layout is a reasonable
                // facsimile of what you see in the dungeon automap.
                Pen pen;
                switch (layout.blockType)
                {
                    case DFBlock.RdbTypes.Start:
                        pen = new Pen(dungeonStartColour);
                        break;
                    default:
                        pen = new Pen(dungeonNormalColour);
                        break;
                }

                // Render dungeon block
                gr.FillRectangle(pen.Brush, layout.layoutRect);

                // Render overlay
                gr.DrawRectangle(new Pen(gridColour), layout.layoutRect);
            }
        }

        #endregion
    }
}