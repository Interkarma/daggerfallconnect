﻿// Project:         DaggerfallModelling
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
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
#endregion

namespace DaggerfallModelling.ViewControls
{

    /// <summary>
    /// Renders a small map representing a location exterior or dungeon.
    /// </summary>
    public class AutoMapView : Control
    {

        #region Class Variables

        // Connect objects
        private BlocksFile blocksFile;
        private MapsFile mapsFile;
        private DFLocation dfLocation;

        // Layouts
        private BlockLayout[] exteriorLayout;
        private BlockLayout[] dungeonLayout;
        private Bitmap exteriorLayoutBitmap;
        private Bitmap dungeonLayoutBitmap;
        private Point mapOffset;

        // Modes
        private bool exteriorModeAllowed = false;
        private bool dungeonModeAllowed = false;
        private ViewModes viewMode = ViewModes.None;
        private ViewModes userPreferredViewMode = ViewModes.None;

        // Mouse
        Point mousePos;
        int mouseOverBlock = -1;
        int selectedExteriorBlock = -1;
        int selectedDungeonBlock = -1;

        #endregion

        #region Drawing Colours

        private Color gridColour = Color.SlateGray;
        private Color mouseOverBlockColour = Color.Blue;
        private Color dungeonStartColour = Color.FromArgb(85, 154, 154);
        private Color dungeonNormalColour = Color.FromArgb(243, 239, 44);
        private Color dungeonBorderColour = Color.FromArgb(213, 209, 14);
        private Color dungeonSelectedColour = Color.FromArgb(190, 85, 24);

        #endregion

        #region Class Structures

        /// <summary>
        /// View modes supported by control.
        /// </summary>
        public enum ViewModes
        {
            /// <summary>No view mode set.</summary>
            None,
            /// <summary>Viewing exterior layout.</summary>
            Exterior,
            /// <summary>Viewing dungeon layout.</summary>
            Dungeon,
        }

        /// <summary>
        /// Block layout of location.
        /// </summary>
        private struct BlockLayout
        {
            public int x;
            public int y;
            public Rectangle rect;
            public string name;
            public DFBlock.BlockTypes blocktype;
            public DFBlock.RdbTypes rdbType;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the MapsFile object for map operations.
        /// </summary>
        public MapsFile MapsFile
        {
            get { return mapsFile; }
            set { mapsFile = value; }
        }

        /// <summary>
        /// Gets or sets the BlocksFile object for block operations.
        /// </summary>
        public BlocksFile BlocksFile
        {
            get { return blocksFile; }
            set { blocksFile = value; }
        }

        /// <summary>
        /// Gets the current view mode.
        /// </summary>
        public ViewModes ViewMode
        {
            get { return viewMode; }
        }

        /// <summary>
        /// Gets the current DFLocation loaded in the view.
        /// </summary>
        public DFLocation DFLocation
        {
            get { return dfLocation; }
        }

        /// <summary>
        /// Gets
        /// </summary>
        public string SelectedBlockName
        {
            get { return GetSelectedBlockName(); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AutoMapView()
        {
            // Set value of double-buffering style bits to true
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        }

        #endregion

        #region Event Arguments

        public class BlockEventArgs
        {
            public int X;
            public int Y;
            public string Name;
            public ViewModes ViewMode;
            public DFBlock.BlockTypes BlockType;
            public DFBlock.RdbTypes RdbType;
        }

        public class ModeChangedEventArgs
        {
            public bool ExteriorModeAllowed;
            public bool DungeonModeAllowed;
            public ViewModes ViewMode;
        }

        #endregion

        #region SelectedBlockChanged Event

        public delegate void SelectedBlockChangedEventHandler(object sender, BlockEventArgs e);
        public event SelectedBlockChangedEventHandler SelectedBlockChanged;

        protected virtual void RaiseSelectedBlockChangedEvent()
        {
            BlockEventArgs e = new BlockEventArgs();

            // Get selected block based on view mode
            int selectedBlock = -1;
            switch (viewMode)
            {
                case ViewModes.Exterior:
                    selectedBlock = selectedExteriorBlock;
                    break;
                case ViewModes.Dungeon:
                    selectedBlock = selectedDungeonBlock;
                    break;
            }

            if (selectedBlock == -1)
            {
                // Dummy args
                e.X = -1;
                e.Y = -1;
                e.Name = string.Empty;
                e.ViewMode = viewMode;
                e.BlockType = DFBlock.BlockTypes.Unknown;
                e.RdbType = DFBlock.RdbTypes.Unknown;
            }
            else
            {
                // Populate args
                e.ViewMode = viewMode;
                switch (viewMode)
                {
                    case ViewModes.Exterior:
                        e.X = exteriorLayout[mouseOverBlock].x;
                        e.Y = exteriorLayout[mouseOverBlock].y;
                        e.Name = exteriorLayout[mouseOverBlock].name;
                        e.BlockType = DFBlock.BlockTypes.Rmb;
                        e.RdbType = DFBlock.RdbTypes.Unknown;
                        break;
                    case ViewModes.Dungeon:
                        e.X = dungeonLayout[mouseOverBlock].x;
                        e.Y = dungeonLayout[mouseOverBlock].y;
                        e.Name = dungeonLayout[mouseOverBlock].name;
                        e.BlockType = DFBlock.BlockTypes.Rdb;
                        e.RdbType = dungeonLayout[mouseOverBlock].rdbType;
                        break;
                }
            }

            // Raise event
            SelectedBlockChanged(this, e);
        }

        #endregion

        #region MouseOverBlockChanged Event

        public delegate void MouseOverBlockChangedEventHandler(object sender, BlockEventArgs e);
        public event MouseOverBlockChangedEventHandler MouseOverBlockChanged;

        protected virtual void RaiseMouseOverBlockChangedEvent()
        {
            BlockEventArgs e = new BlockEventArgs();

            if (mouseOverBlock == -1)
            {
                // Dummy args
                e.X = -1;
                e.Y = -1;
                e.Name = string.Empty;
                e.ViewMode = viewMode;
                e.BlockType = DFBlock.BlockTypes.Unknown;
                e.RdbType = DFBlock.RdbTypes.Unknown;
            }
            else
            {
                // Populate args
                e.ViewMode = viewMode;
                switch (viewMode)
                {
                    case ViewModes.Exterior:
                        e.X = exteriorLayout[mouseOverBlock].x;
                        e.Y = exteriorLayout[mouseOverBlock].y;
                        e.Name = exteriorLayout[mouseOverBlock].name;
                        e.BlockType = DFBlock.BlockTypes.Rmb;
                        e.RdbType = DFBlock.RdbTypes.Unknown;
                        break;
                    case ViewModes.Dungeon:
                        e.X = dungeonLayout[mouseOverBlock].x;
                        e.Y = dungeonLayout[mouseOverBlock].y;
                        e.Name = dungeonLayout[mouseOverBlock].name;
                        e.BlockType = DFBlock.BlockTypes.Rdb;
                        e.RdbType = dungeonLayout[mouseOverBlock].rdbType;
                        break;
                }
            }

            // Raise event
            MouseOverBlockChanged(this, e);
        }

        #endregion

        #region ModeChanged Event

        public delegate void ModeChangedEventHandler(object sender, ModeChangedEventArgs e);
        public event ModeChangedEventHandler ModeChanged;

        protected virtual void RaiseModeChangedEvent()
        {
            // Populate event args based on modes
            ModeChangedEventArgs e = new ModeChangedEventArgs();
            e.ExteriorModeAllowed = exteriorModeAllowed;
            e.DungeonModeAllowed = dungeonModeAllowed;
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

            // Reset modes and redraw
            ConfigureModes();
            this.Invalidate();
        }

        public void ShowLocation(int region, int location)
        {
            // Get location
            dfLocation = mapsFile.GetLocation(region, location);
            if (string.IsNullOrEmpty(dfLocation.Name))
                return;

            // Create exterior map
            CreateExteriorMap();

            // Conditionally create dungeon map
            if (dfLocation.HasDungeon)
                CreateDungeonMap();
            else
                dungeonLayoutBitmap = null;

            // Configure modes for new location
            ConfigureModes();

            // Clear block mouse over and selection
            selectedExteriorBlock = -1;
            selectedDungeonBlock = -1;
            mouseOverBlock = -1;
            RaiseMouseOverBlockChangedEvent();

            // Redraw
            this.Invalidate();
        }

        public void SetViewMode(ViewModes mode)
        {
            // Can always allow exterior mode
            if (mode == ViewModes.Exterior)
                viewMode = mode;

            // Only allow dungeon when dungeon present
            if (mode == ViewModes.Dungeon && dungeonLayoutBitmap != null)
                viewMode = mode;

            // This is now the user preferred mode and will be
            // automatically selected later when possible
            userPreferredViewMode = mode;

            // Clear mouse over block
            mouseOverBlock = -1;

            // Raise events
            RaiseModeChangedEvent();
            RaiseMouseOverBlockChangedEvent();

            // Redraw
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

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            // Clear mouse-over value and redraw
            mouseOverBlock = -1;
            RaiseMouseOverBlockChangedEvent();
            this.Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Record mouse position and test for mouse over block
            mousePos.X = e.X;
            mousePos.Y = e.Y;
            TestMouse();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            // Select block under mouse
            if (mouseOverBlock != -1)
            {
                switch (viewMode)
                {
                    case ViewModes.Exterior:
                        selectedExteriorBlock = mouseOverBlock;
                        break;
                    case ViewModes.Dungeon:
                        selectedDungeonBlock = mouseOverBlock;
                        break;
                }

                // Raise select event and redraw
                RaiseSelectedBlockChangedEvent();
                this.Invalidate();
            }
        }

        #endregion

        #region Mouse

        private void TestMouse()
        {
            int newMouseOverBlock = -1;

            // Test if mouse point is inside an exterior block rect
            if (viewMode == ViewModes.Exterior)
            {
                int index = 0;
                foreach (var layout in exteriorLayout)
                {
                    Rectangle rect = layout.rect;
                    rect.Offset(mapOffset);
                    if (rect.Contains(mousePos))
                    {
                        newMouseOverBlock = index;
                        break;
                    }
                    index++;
                }
            }

            // Test if mouse point is inside a dungeon block rect
            if (viewMode == ViewModes.Dungeon)
            {
                int index = 0;
                foreach (var layout in dungeonLayout)
                {
                    Rectangle rect = layout.rect;
                    rect.Offset(mapOffset);
                    if (rect.Contains(mousePos))
                    {
                        newMouseOverBlock = index;
                        break;
                    }
                    index++;
                }
            }

            // Redraw if new mouse over block different to current
            if (newMouseOverBlock != mouseOverBlock)
            {
                mouseOverBlock = newMouseOverBlock;
                RaiseMouseOverBlockChangedEvent();
                this.Invalidate();
            }
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
            Brush brush = new LinearGradientBrush(
                this.ClientRectangle,
                Color.FromArgb(190, 190, 190),
                Color.FromArgb(240, 240, 240),
                LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(brush, this.ClientRectangle);

            // Calculate offset of location bitmap inside client area
            switch (viewMode)
            {
                case ViewModes.Exterior:
                    mapOffset = new Point(
                        this.Width / 2 - exteriorLayoutBitmap.Width / 2,
                        this.Height / 2 - exteriorLayoutBitmap.Height / 2);
                    break;
                case ViewModes.Dungeon:
                    mapOffset = new Point(
                        this.Width / 2 - dungeonLayoutBitmap.Width / 2,
                        this.Height / 2 - dungeonLayoutBitmap.Height / 2);
                    break;
            }

            // Highlight exterior blocks
            if (viewMode == ViewModes.Exterior)
            {
                // Highlight block under mouse (unless over selected block)
                if (mouseOverBlock != -1 && mouseOverBlock != selectedExteriorBlock)
                {
                    Rectangle rect = exteriorLayout[mouseOverBlock].rect;
                    rect.Offset(mapOffset);
                    Brush b = new LinearGradientBrush(
                        rect,
                        Color.AliceBlue,
                        Color.CornflowerBlue,
                        LinearGradientMode.Vertical);
                    e.Graphics.FillRectangle(b, rect);
                }

                // Highlight selected block
                if (selectedExteriorBlock != -1)
                {
                    Rectangle rect = exteriorLayout[selectedExteriorBlock].rect;
                    rect.Offset(mapOffset);
                    Brush b = new LinearGradientBrush(
                        rect,
                        Color.White,
                        Color.Gold,
                        LinearGradientMode.Vertical);
                    e.Graphics.FillRectangle(b, rect);
                }
            }
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
                case ViewModes.Exterior:
                    e.Graphics.DrawImageUnscaled(exteriorLayoutBitmap, mapOffset);
                    break;
                case ViewModes.Dungeon:
                    e.Graphics.DrawImageUnscaled(dungeonLayoutBitmap, mapOffset);
                    break;
            }

            // Highlight dungeon blocks
            if (viewMode == ViewModes.Dungeon)
            {
                // Highlight block under mouse
                if (mouseOverBlock != -1)
                {
                    Rectangle rect = dungeonLayout[mouseOverBlock].rect;
                    rect.Offset(mapOffset);
                    Pen p = new Pen(Color.Red);
                    e.Graphics.DrawRectangle(p, rect);
                }

                // Highlight selected block
                if (selectedDungeonBlock != -1)
                {
                    Rectangle rect = dungeonLayout[selectedDungeonBlock].rect;
                    rect.Offset(mapOffset);
                    Pen p = new Pen(dungeonSelectedColour);
                    int width = rect.Width / 4;
                    int height = rect.Height / 4;
                    rect.X += width;
                    rect.Y += height;
                    rect.Width -= width*2;
                    rect.Height -= height*2;
                    e.Graphics.FillRectangle(p.Brush, rect);
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

        private void ConfigureModes()
        {
            if (!IsReady())
            {
                // Everything off
                exteriorModeAllowed = false;
                dungeonModeAllowed = false;
                viewMode = ViewModes.None;
                RaiseModeChangedEvent();
                return;
            }

            // Always enable exterior mode mode
            exteriorModeAllowed = true;

            // Only enable dungeon mode when the location has a dungeon
            if (dfLocation.HasDungeon)
                dungeonModeAllowed = true;
            else
                dungeonModeAllowed = false;

            // Test if location type is a dungeon type
            bool isDungeonMapType = false;
            if (dfLocation.MapTableData.Type == DFRegion.LocationTypes.DungeonKeep ||
                dfLocation.MapTableData.Type == DFRegion.LocationTypes.DungeonLabyrinth ||
                dfLocation.MapTableData.Type == DFRegion.LocationTypes.DungeonRuin)
            {
                    isDungeonMapType = true;
            }

            // Try to keep user preferred mode if possible
            if (userPreferredViewMode != ViewModes.None)
            {
                if (userPreferredViewMode == ViewModes.Dungeon && dfLocation.HasDungeon)
                    viewMode = ViewModes.Dungeon;
                else
                    viewMode = ViewModes.Exterior;
            }
            else
            {
                // Work out view mode based on location
                // Start in dungeon mode when a dungeon is present and this is a dedicated dungeon type.
                // Otherwise start in exterior mode.
                if (dfLocation.HasDungeon && isDungeonMapType)
                    viewMode = ViewModes.Dungeon;
                else
                    viewMode = ViewModes.Exterior;
            }

            // Raise event to update form
            RaiseModeChangedEvent();
        }

        /// <summary>
        /// Gets name of selected block for current mode.
        /// </summary>
        /// <returns>Name of selected block, or String.Empty if no selection.</returns>
        private string GetSelectedBlockName()
        {
            if (viewMode == ViewModes.Exterior)
            {
                if (selectedExteriorBlock == -1)
                    return string.Empty;
                else
                    return exteriorLayout[selectedExteriorBlock].name;
            }

            if (viewMode == ViewModes.Dungeon)
            {
                if (selectedDungeonBlock == -1)
                    return string.Empty;
                else
                    return dungeonLayout[selectedDungeonBlock].name;
            }

            return string.Empty;
        }

        #endregion

        #region AutoMap Layout

        private void CreateExteriorMap()
        {
            // Get dimensions of exterior location
            int width = dfLocation.Exterior.ExteriorData.Width;
            int height = dfLocation.Exterior.ExteriorData.Height;

            // Get longest side
            int longestSide;
            if (width >= height)
                longestSide = width;
            else
                longestSide = height;

            // Calc block layout size based on longest side.
            // Note: If control dimensions are not square then blocks will look squashed.
            Size blockSize = new Size();
            if (longestSide <= 4)
            {
                blockSize.Width = this.Width / 4;
                blockSize.Height = this.Height / 4;
            }
            else
            {
                blockSize.Width = this.Width / 8;
                blockSize.Height = this.Height / 8;
            }

            // Create starting offsets.
            // Daggerfall's blocks are laid out bottom to top, then left to right.
            int xpos = 0;
            int ypos = (height * blockSize.Height) - blockSize.Height;

            // Create map layout
            exteriorLayout = new BlockLayout[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    exteriorLayout[index].x = x;
                    exteriorLayout[index].y = y;
                    exteriorLayout[index].rect = new Rectangle(xpos, ypos, blockSize.Width, blockSize.Height);
                    exteriorLayout[index].name = blocksFile.CheckName(mapsFile.GetRmbBlockName(ref dfLocation, x, y));
                    exteriorLayout[index].blocktype = DFBlock.BlockTypes.Rmb;
                    exteriorLayout[index].rdbType = DFBlock.RdbTypes.Unknown;
                    xpos += blockSize.Width;
                }
                ypos -= blockSize.Height;
                xpos = 0;
            }

            // Create layout image
            int layoutWidth = width * blockSize.Width + 1;
            int layoutHeight = height * blockSize.Height + 1;
            exteriorLayoutBitmap = new Bitmap(layoutWidth, layoutHeight);
            Graphics gr = Graphics.FromImage(exteriorLayoutBitmap);

            // Render map layout
            DFManualImage dfManualImage = new DFManualImage();
            dfManualImage.Palette.MakeAutomap();
            foreach (var layout in exteriorLayout)
            {
                // Get block automap image
                dfManualImage.DFBitmap = blocksFile.GetBlockAutoMap(layout.name, true);
                Bitmap bm = dfManualImage.GetManagedBitmap(0, 0, false, true);

                // Render automap block
                gr.DrawImage(bm, layout.rect);

                // Render grid overlay
                gr.DrawRectangle(new Pen(gridColour), layout.rect);
            }
        }

        private void CreateDungeonMap()
        {
            // Quick pass over blocks to find dimensions of dungeon relative to 0,0
            // This is done because the 0,0 block is not typically at origin.
            // We use these dimensions to calc layout surface and offsets to centre.
            int left = 0, right = 0;
            int top = 0, bottom = 0;
            foreach (var block in dfLocation.Dungeon.Blocks)
            {
                if (block.X < left) left = block.X;
                if (block.X > right) right = block.X;
                if (block.Z < top) top = block.Z;
                if (block.Z > bottom) bottom = block.Z;
            }

            // Always use the same block size
            Size blockSize = new Size();
            blockSize.Width = this.Width / 8;
            blockSize.Height = this.Height / 8;

            // Calc dimensions of layout image
            int layoutWidth = blockSize.Width * ((right - left) + 1);
            int layoutHeight = blockSize.Height * ((bottom - top) + 1);

            // Offset rects right and down to fit snugly inside layout surface.
            // Daggerfall's blocks are laid out bottom to top, then left to right.
            int offx = blockSize.Width * Math.Abs(left);
            int offy = blockSize.Height * Math.Abs(bottom);

            // Create map layout
            int index = 0;
            dungeonLayout = new BlockLayout[dfLocation.Dungeon.Blocks.Length];
            foreach (var block in dfLocation.Dungeon.Blocks)
            {
                // Store coordinates
                dungeonLayout[index].x = block.X;
                dungeonLayout[index].y = block.Z;

                // Calc layout rect.
                // Invert block.Z as blocks are laid out bottom to top.
                dungeonLayout[index].rect = new Rectangle(
                    block.X * blockSize.Width + offx,
                    -block.Z * blockSize.Height + offy,
                    blockSize.Width, blockSize.Height);

                // Store block name
                dungeonLayout[index].name = block.BlockName;

                // Set block type
                dungeonLayout[index].blocktype = DFBlock.BlockTypes.Rdb;

                // Set RDB block type
                dungeonLayout[index].rdbType = blocksFile.GetRdbType(block.BlockName);

                // Detect starting block
                if (block.IsStartingBlock)
                    dungeonLayout[index].rdbType = DFBlock.RdbTypes.Start;

                // Increment index
                index++;
            }

            // Create layout image
            dungeonLayoutBitmap = new Bitmap(layoutWidth + 1, layoutHeight + 1);
            Graphics gr = Graphics.FromImage(dungeonLayoutBitmap);

            // Render map layout
            foreach (var layout in dungeonLayout)
            {
                // Get colour based on type. Only using the same two
                // colours as Daggerfall here so the layout is a reasonable
                // facsimile of what you see in the dungeon automap.
                Pen pen;
                switch (layout.rdbType)
                {
                    case DFBlock.RdbTypes.Start:
                        pen = new Pen(dungeonStartColour);
                        break;
                    case DFBlock.RdbTypes.Border:
                        pen = new Pen(dungeonBorderColour);
                        break;
                    default:
                        pen = new Pen(dungeonNormalColour);
                        break;
                }

                // Render dungeon block
                gr.FillRectangle(pen.Brush, layout.rect);

                // Render grid overlay
                gr.DrawRectangle(new Pen(gridColour), layout.rect);
            }
        }

        #endregion

    }

}