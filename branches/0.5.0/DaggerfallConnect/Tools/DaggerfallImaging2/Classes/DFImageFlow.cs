// Project:         DaggerfallImaging
// Description:     Explore and export bitmaps from Daggerfall.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/using System;

#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;

#endregion

namespace DaggerfallImaging2.Classes
{
    /// <summary>
    /// Fast image flow layout control for Daggerfall Imaging.
    ///  Takes a reference to an instantiated DFImageFile and renders content images based on properties.
    /// </summary>
    class DFImageFlow : ScrollableControl
    {
        #region Class Variables

        private Point MousePos;
        private int PageHeight = 0;
        private DFImageFile MyDFImageFile = null;
        private LayoutItem[] MyLayout = null;
        private LayoutStyles MyLayoutStyle = LayoutStyles.Flow;
        private Padding MyInnerMargin = new Padding(6, 6, 6, 6);
        private float MyZoomAmount = 1.0f;
        private int MyMouseOverIndex = -1;
        private int MySelectedIndex = -1;
        private bool IsAnimatedValue = true;
        private bool IsTransparentValue = true;
        private bool ShowImageBordersValue = true;
        private bool AllowImageMouseOverValue = true;
        private bool AllowImageSelectValue = true;
        private Font CaptionFont;

        #endregion

        #region Class Structures

        public enum LayoutStyles
        {
            Flow,
        }

        private struct LayoutItem
        {
            public bool IsVisible;
            public bool IsAnimated;
            public Bitmap[] ManagedBitmaps;
            public Rectangle OuterBounds;
            public Rectangle InnerBounds;
            public int Record;
            public int Frame;
            public int ImageWidth;
            public int ImageHeight;
            public int FrameCount;
            public int CurrentFrame;
        }

        #endregion

        #region Constructors

        public DFImageFlow()
        {
            // Set value of double-buffering style bits to true
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            // Create font
            CaptionFont = new Font(FontFamily.GenericSansSerif, 7.0f, FontStyle.Regular);

            // Init scroll bar
            ResetScrollBars();
        }

        #endregion

        #region Public Properties

        public DFImageFile DFImageFile
        {
            get { return MyDFImageFile; }
            set { SetDFImageFile(value); }
        }

        public LayoutStyles LayoutStyle
        {
            get { return MyLayoutStyle; }
            set { MyLayoutStyle = value; }
        }

        public float ZoomAmount
        {
            get { return MyZoomAmount; }
            set
            {
                MyZoomAmount = value;
                UpdateFlowLayout();
                ResetScrollBars();
                UpdateScroller();
                this.Refresh();
            }
        }

        public bool AnimateImages
        {
            get { return IsAnimatedValue; }
            set
            {
                IsAnimatedValue = value;
                BuildLayoutArray();
                UpdateFlowLayout();
                ResetScrollBars();
                UpdateScroller();
                this.Refresh();
            }
        }

        public bool Transparency
        {
            get { return IsTransparentValue; }
            set
            {
                IsTransparentValue = value;
                BuildLayoutArray();
                UpdateFlowLayout();
                ResetScrollBars();
                UpdateScroller();
                this.Refresh();
            }
        }

        public Padding InnerMargin
        {
            get { return MyInnerMargin; }
            set
            {
                MyInnerMargin = value;
                UpdateView();
            }
        }

        public int SelectedIndex
        {
            get { return MySelectedIndex; }
        }

        public Bitmap SeletedImage
        {
            get { return GetSelectedImage(); }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Progress all animated images to next frame. When the last frame is reached animation will start again from first frame.
        /// </summary>
        public void Tick()
        {
            // Nothing to do if not animated or no file
            if (!IsAnimatedValue || null == MyDFImageFile)
                return;

            // Cycle animations in all multi-frame items
            for (int i = 0; i < MyLayout.Length; i++)
            {
                // Skip single-frame and non-animated images
                if (MyLayout[i].IsAnimated == false || MyLayout[i].FrameCount < 2)
                    continue;

                // Increment and wrap current frame
                if (++MyLayout[i].CurrentFrame >= MyLayout[i].FrameCount)
                    MyLayout[i].CurrentFrame = 0;
            }

            // Redraw page to show updated frames
            this.Refresh();
        }

        #endregion

        #region SelectedImageChanged Event

        public class SelectedImageEventArgs
        {
            public bool AnimateImages;
            public int Record;
            public int Frame;
            public int FrameCount;
            public int Width;
            public int Height;
        }

        public delegate void ImageSelectedEventHandler(object sender, SelectedImageEventArgs e);
        public event ImageSelectedEventHandler SelectedImageChanged;

        protected virtual void RaiseSelectedImageChangedEvent()
        {
            // Raise event
            if (null != SelectedImageChanged)
            {
                // Populate event args based on selection
                SelectedImageEventArgs e = new SelectedImageEventArgs();
                if (MySelectedIndex >= 0)
                {
                    e.AnimateImages = MyLayout[MySelectedIndex].IsAnimated;
                    e.Record = MyLayout[MySelectedIndex].Record;
                    e.Frame = MyLayout[MySelectedIndex].Frame;
                    e.FrameCount = MyLayout[MySelectedIndex].FrameCount;
                    e.Width = MyLayout[MySelectedIndex].ManagedBitmaps[0].Width;
                    e.Height = MyLayout[MySelectedIndex].ManagedBitmaps[0].Height;
                }
                else
                {
                    e.AnimateImages = false;
                    e.Record = -1;
                    e.Frame = -1;
                    e.FrameCount = -1;
                    e.Width = -1;
                    e.Height = -1;
                }

                // Raise event
                SelectedImageChanged(this, e);
            }
        }

        #endregion

        #region ShowContextMenu Event

        public class ShowContextMenuEventArgs
        {
            public Point MousePos;
            public int SelectedRecord;
            public int SelectedFrame;
        }

        public delegate void ShowContextMenuEventHandler(object sender, ShowContextMenuEventArgs e);
        public event ShowContextMenuEventHandler ShowContextMenu;

        protected virtual void RaiseShowContextMenuEvent()
        {
            // Raise event
            if (null != ShowContextMenu)
            {
                // Populate event args
                ShowContextMenuEventArgs e = new ShowContextMenuEventArgs();
                e.MousePos = MousePos;
                if (MySelectedIndex >= 0)
                {
                    e.SelectedRecord = MyLayout[MySelectedIndex].Record;
                    e.SelectedFrame = MyLayout[MySelectedIndex].Frame;
                }
                else
                {
                    e.SelectedRecord = -1;
                    e.SelectedFrame = -1;
                }

                // Raise event
                ShowContextMenu(this, e);
            }
        }

        #endregion

        #region MouseOverItem Event

        public delegate void MouseOverItemEventHandler(object sender, SelectedImageEventArgs e);
        public event MouseOverItemEventHandler MouseOverItem;

        protected virtual void RaiseMouseOverItemEvent()
        {
            // Raise event
            if (null != MouseOverItem)
            {
                // Populate event args based on selection
                SelectedImageEventArgs e = new SelectedImageEventArgs();
                if (MyMouseOverIndex >= 0)
                {
                    e.AnimateImages = MyLayout[MyMouseOverIndex].IsAnimated;
                    e.Record = MyLayout[MyMouseOverIndex].Record;
                    e.Frame = MyLayout[MyMouseOverIndex].Frame;
                    e.FrameCount = MyLayout[MyMouseOverIndex].FrameCount;
                    e.Width = MyLayout[MyMouseOverIndex].ManagedBitmaps[0].Width;
                    e.Height = MyLayout[MyMouseOverIndex].ManagedBitmaps[0].Height;
                }
                else
                {
                    e.AnimateImages = false;
                    e.Record = -1;
                    e.Frame = -1;
                    e.FrameCount = -1;
                    e.Width = -1;
                    e.Height = -1;
                }

                // Raise event
                MouseOverItem(this, e);
            }
        }

        #endregion

        #region Private Methods

        private void BuildLayoutArray()
        {
            // Exit if image file not set
            if (null == MyDFImageFile)
                return;

            // Count total image items from all records and frames
            int ItemCount = 0;
            for (int record = 0; record < MyDFImageFile.RecordCount; record++)
            {
                if (!IsAnimatedValue)
                {
                    // Count each frame as an item if not animated
                    int frameCount = MyDFImageFile.GetFrameCount(record);
                    for (int frame = 0; frame < frameCount; frame++)
                    {
                        ItemCount++;
                    }
                }
                else
                {
                    // Just increment count
                    ItemCount++;
                }
            }

            // Create layout array
            MyLayout = new LayoutItem[ItemCount];

            // Add bitmaps to layout array
            int curItem = 0;
            for (int record = 0; record < MyDFImageFile.RecordCount; record++)
            {
                // Get frame count and skip record with zero frames (no image)
                int FrameCount = MyDFImageFile.GetFrameCount(record);
                if (0 == FrameCount)
                    continue;

                if (AnimateImages && FrameCount > 1)
                {
                    // Create managed bitmap array
                    MyLayout[curItem].ManagedBitmaps = new Bitmap[FrameCount];

                    // Add all frames
                    for (int frame = 0; frame < FrameCount; frame++)
                    {
                        MyLayout[curItem].ManagedBitmaps[frame] = DFImageFile.GetManagedBitmap(record, frame, false, Transparency);
                    }

                    // Store details
                    MyLayout[curItem].IsAnimated = true;
                    MyLayout[curItem].Record = record;
                    MyLayout[curItem].Frame = -1;
                    MyLayout[curItem].FrameCount = FrameCount;
                    MyLayout[curItem].CurrentFrame = 0;

                    // Increment
                    curItem++;
                }
                else
                {
                    // Create new layout item
                    MyLayout[curItem] = new LayoutItem();

                    // Add each frame as a single item
                    for (int frame = 0; frame < FrameCount; frame++)
                    {
                        // Create managed bitmap array
                        MyLayout[curItem].ManagedBitmaps = new Bitmap[1];

                        // Get single frame
                        Bitmap bitmap = DFImageFile.GetManagedBitmap(record, frame, false, Transparency);

                        // Store details
                        MyLayout[curItem].IsAnimated = false;
                        MyLayout[curItem].ManagedBitmaps[0] = bitmap;
                        MyLayout[curItem].Record = record;
                        MyLayout[curItem].Frame = frame;
                        MyLayout[curItem].FrameCount = FrameCount;
                        MyLayout[curItem].CurrentFrame = 0;

                        // Increment
                        curItem++;
                    }
                }
            }

            // Reset mouse over and selected indices
            MyMouseOverIndex = -1;
            MySelectedIndex = -1;
            RaiseSelectedImageChangedEvent();
        }

        private void SetDFImageFile(DFImageFile ImageFile)
        {
            // Handle null
            if (null == ImageFile)
            {
                MyDFImageFile = null;
                this.Refresh();
                return;
            }

            // Store file object
            MyDFImageFile = ImageFile;

            // Build layout array
            BuildLayoutArray();

            // Reset scroll bar
            ResetScrollBars();
            UpdateScroller();

            // Recalculate layout
            UpdateFlowLayout();

            // Update scrolling
            UpdateScroller();

            // Force view redraw
            this.Refresh();
        }

        #endregion

        #region Layout Methods

        private void UpdateView()
        {
             // Recalculate layout based on new size
            UpdateFlowLayout();

            // Update scrolling
            UpdateScroller();

            // Force view redraw
            this.Refresh();
        }

        private Rectangle CalculateOuterBounds(int ImageWidth, int ImageHeight)
        {
            // This is the total hoverable, selectable area of the image tile
            int Width = MyInnerMargin.Horizontal + (int)(ImageWidth * MyZoomAmount);
            int Height = MyInnerMargin.Vertical + (int)(ImageHeight * MyZoomAmount);
            return new Rectangle(0, 0, Width, Height);
        }

        private Rectangle CalculateInnerBounds(int ImageWidth, int ImageHeight)
        {
            // This is the interior cel containing image only
            return new Rectangle(MyInnerMargin.Left, MyInnerMargin.Right, (int)(ImageWidth * MyZoomAmount), (int)(ImageHeight * MyZoomAmount));
        }

        private void UpdateFlowLayout()
        {
            // Do nothing if file not set
            if (null == DFImageFile)
                return;

            PageHeight = 0;
            int MaxTileHeight = 0;
            Point pos = new Point(Margin.Left, Margin.Top);
            for (int i = 0; i < MyLayout.Length; i++)
            {
                if (MyLayout[i].FrameCount == 0)
                {
                    return;
                }

                // Calculate bounds
                Rectangle OuterBounds = CalculateOuterBounds(MyLayout[i].ManagedBitmaps[0].Width, MyLayout[i].ManagedBitmaps[0].Height);
                Rectangle InnerBounds = CalculateInnerBounds(MyLayout[i].ManagedBitmaps[0].Width, MyLayout[i].ManagedBitmaps[0].Height);

                // Check for right side overflow and wrap flow
                if ((pos.X + OuterBounds.Width + Margin.Horizontal) >= ClientRectangle.Width)
                {
                    // Update page height
                    PageHeight += (MaxTileHeight + Margin.Vertical);

                    // Wrap flow
                    pos.X = Margin.Left;
                    pos.Y += (MaxTileHeight + Margin.Vertical);
                    MaxTileHeight = 0;
                }

                // Update max tile height
                if (OuterBounds.Height > MaxTileHeight)
                    MaxTileHeight = OuterBounds.Height;

                // Store layout information
                MyLayout[i].OuterBounds.X = pos.X;
                MyLayout[i].OuterBounds.Y = pos.Y;
                MyLayout[i].OuterBounds.Width = OuterBounds.Width;
                MyLayout[i].OuterBounds.Height = OuterBounds.Height;
                MyLayout[i].InnerBounds.X = InnerBounds.X + pos.X;
                MyLayout[i].InnerBounds.Y = InnerBounds.X + pos.Y;
                MyLayout[i].InnerBounds.Width = InnerBounds.Width;
                MyLayout[i].InnerBounds.Height = InnerBounds.Height;
                MyLayout[i].ImageWidth = MyLayout[i].ManagedBitmaps[0].Width;
                MyLayout[i].ImageHeight = MyLayout[i].ManagedBitmaps[0].Height;

                // Advance position
                pos.X += (OuterBounds.Width + Margin.Horizontal);
            }

            // Update max page height for last row
            PageHeight += (MaxTileHeight + Margin.Vertical);
        }

        private void ResetScrollBars()
        {
            AutoScroll = false;
            HorizontalScroll.Enabled = false;
            HorizontalScroll.Visible = false;
            VerticalScroll.Enabled = false;
            VerticalScroll.Visible = true;
            VerticalScroll.Value = 0;
            VerticalScroll.Maximum = 100;
            VerticalScroll.LargeChange = 0;
            AutoScrollMinSize = new Size(0, 0);
        }

        private void UpdateScroller()
        {
            // Set vertical scroll bar
            if (PageHeight < ClientSize.Height)
            {
                // Disable
                ResetScrollBars();
            }
            else
            {
                // Enable and set range
                AutoScroll = false;
                HorizontalScroll.Enabled = false;
                HorizontalScroll.Visible = false;
                VerticalScroll.Enabled = true;
                VerticalScroll.Visible = true;
                VerticalScroll.Maximum = PageHeight;
                VerticalScroll.SmallChange = 4;
                VerticalScroll.LargeChange = ClientSize.Height;

                // Scroll value up if items out of visible area
                if (VerticalScroll.Value + ClientSize.Height > PageHeight)
                    VerticalScroll.Value = PageHeight - ClientSize.Height;
            }
        }

        private Bitmap GetSelectedImage()
        {
            // Return small empty bitmap if file not set or nothing selected
            if (null == DFImageFile || -1 == MySelectedIndex)
                return new Bitmap(4,4);

            // Get image from layour array
            return MyLayout[MySelectedIndex].ManagedBitmaps[0];
        }

        #endregion

        #region Event Overides

        protected override void OnScroll(ScrollEventArgs se)
        {
            // Discard horizontal scrolls
            if (se.ScrollOrientation != ScrollOrientation.VerticalScroll)
            {
                base.OnScroll(se);
                return;
            }

            // Adjust scroller
            AutoScrollMinSize = this.ClientSize;
            switch (se.Type)
            {
                case ScrollEventType.ThumbTrack:
                    VerticalScroll.Value = se.NewValue;
                    break;
                case ScrollEventType.LargeIncrement:
                    VerticalScroll.Value = PageHeight - ClientSize.Height;
                    break;
                case ScrollEventType.LargeDecrement:
                    VerticalScroll.Value = 0;
                    break;
                case ScrollEventType.SmallIncrement:
                    VerticalScroll.Value += 4;
                    break;
                case ScrollEventType.SmallDecrement:
                    VerticalScroll.Value -= 4;
                    break;
            }

            // Redraw
            this.Refresh();

            // Call base handler
            base.OnScroll(se);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            // Scroll
            int NewValue = VerticalScroll.Value - e.Delta;
            if (NewValue > (PageHeight - ClientSize.Height)) NewValue = PageHeight - ClientSize.Height;
            if (NewValue < 0) NewValue = 0;
            VerticalScroll.Value = NewValue;
            this.Refresh();

            base.OnMouseWheel(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            // Just work like an empty control if image file not set
            if (null == MyDFImageFile)
            {
                base.OnSizeChanged(e);
                return;
            }

            UpdateView();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (null != MyDFImageFile)
            {
                // Draw gradiant
                Brush brush = new LinearGradientBrush(this.ClientRectangle, Color.FromArgb(190, 190, 190), Color.FromArgb(120, 120, 120), LinearGradientMode.Vertical);
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
            else
            {
                // Handle as normal
                base.OnPaintBackground(e);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Just work like an empty control if image file not set
            if (null == MyDFImageFile)
            {
                base.OnPaint(e);
                return;
            }

            // Redraw items
            for (int i = 0; i < MyLayout.Length; i++)
            {
                // Get containing rects
                Rectangle OuterRect = MyLayout[i].OuterBounds;
                Rectangle InnerRect = MyLayout[i].InnerBounds;

                // Adjust scrolling
                OuterRect.Offset(0, -VerticalScroll.Value);
                InnerRect.Offset(0, -VerticalScroll.Value);

                // Skip if rect not visible
                if (!ClientRectangle.IntersectsWith(OuterRect))
                {
                    MyLayout[i].IsVisible = false;
                    continue;
                }
                else
                {
                    MyLayout[i].IsVisible = true;
                }

                // Draw bitmap cel
                if (i == MyMouseOverIndex && i != MySelectedIndex)
                {
                    Brush brush = new LinearGradientBrush(OuterRect, Color.AliceBlue, Color.LightBlue, LinearGradientMode.Vertical);
                    Manina.Windows.Forms.Utility.FillRoundedRectangle(e.Graphics, brush, OuterRect, 4);
                    if (ShowImageBordersValue) Manina.Windows.Forms.Utility.DrawRoundedRectangle(e.Graphics, new Pen(Color.DarkBlue), OuterRect.Left, OuterRect.Top, OuterRect.Width, OuterRect.Height, 4);
                }
                else if (i == MyMouseOverIndex && i == MySelectedIndex)
                {
                    Brush brush = new LinearGradientBrush(OuterRect, Color.LightBlue, Color.DarkCyan, LinearGradientMode.Vertical);
                    Manina.Windows.Forms.Utility.FillRoundedRectangle(e.Graphics, brush, OuterRect, 4);
                    if (ShowImageBordersValue) Manina.Windows.Forms.Utility.DrawRoundedRectangle(e.Graphics, new Pen(Color.Blue), OuterRect.Left, OuterRect.Top, OuterRect.Width, OuterRect.Height, 4);
                }
                else if (i == MySelectedIndex)
                {
                    if (this.Focused)
                    {
                        Brush brush = new LinearGradientBrush(OuterRect, Color.LightBlue, Color.DarkCyan, LinearGradientMode.Vertical);
                        Manina.Windows.Forms.Utility.FillRoundedRectangle(e.Graphics, brush, OuterRect, 4);
                        if (ShowImageBordersValue) Manina.Windows.Forms.Utility.DrawRoundedRectangle(e.Graphics, new Pen(Color.Black), OuterRect.Left, OuterRect.Top, OuterRect.Width, OuterRect.Height, 4);
                    }
                    else
                    {
                        Brush brush = new LinearGradientBrush(OuterRect, Color.LightGray, Color.DarkSlateGray, LinearGradientMode.Vertical);
                        Manina.Windows.Forms.Utility.FillRoundedRectangle(e.Graphics, brush, OuterRect, 4);
                        if (ShowImageBordersValue) Manina.Windows.Forms.Utility.DrawRoundedRectangle(e.Graphics, new Pen(Color.Black), OuterRect.Left, OuterRect.Top, OuterRect.Width, OuterRect.Height, 4);
                    }
                }
                else
                {
                    if (ShowImageBordersValue) Manina.Windows.Forms.Utility.DrawRoundedRectangle(e.Graphics, new Pen(Color.FromArgb(90, 90, 120)), OuterRect.Left, OuterRect.Top, OuterRect.Width, OuterRect.Height, 4);
                }

                // Get current frame
                int CurrentFrame = 0;
                if (MyLayout[i].IsAnimated)
                    CurrentFrame = MyLayout[i].CurrentFrame;

                // Draw current frame
                Rectangle SrcRect = new Rectangle(0, 0, MyLayout[i].ManagedBitmaps[CurrentFrame].Width, MyLayout[i].ManagedBitmaps[CurrentFrame].Height);
                e.Graphics.DrawImage(MyLayout[i].ManagedBitmaps[CurrentFrame], InnerRect, SrcRect, GraphicsUnit.Pixel);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            // Store location
            MousePos = e.Location;

            // Validate
            if (null == MyLayout || !AllowImageMouseOverValue)
            {
                base.OnMouseMove(e);
                return;
            }

            // Get scroll-aware mouse location
            Point MouseLocation = new Point(e.Location.X, e.Location.Y + VerticalScroll.Value);

            // Check if pointer inside any item
            int NewMouseOverIndex = -1;
            for (int i = 0; i < MyLayout.Length; i++)
            {
                // Skip if not visible
                if (!MyLayout[i].IsVisible)
                    continue;

                // Test for mouse-over image rect in layout
                if (MyLayout[i].OuterBounds.Contains(MouseLocation))
                {
                    // Only change if new index
                    if (i == MyMouseOverIndex)
                    {
                        // Still in same index
                        return;
                    }
                    else
                    {
                        // New index
                        NewMouseOverIndex = i;
                        break;
                    }
                }
            }

            // Handle entering new rect
            if (NewMouseOverIndex >= 0)
            {
                MyMouseOverIndex = NewMouseOverIndex;
                this.Refresh();
                base.OnMouseMove(e);
                RaiseMouseOverItemEvent();
                return;
            }

            // Handle exiting rect
            if (-1 == NewMouseOverIndex)
            {
                MyMouseOverIndex = -1;
                this.Refresh();
                base.OnMouseMove(e);
                RaiseMouseOverItemEvent();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            // Always clear mouse over index on exit
            MyMouseOverIndex = -1;
            this.Refresh();
            base.OnMouseLeave(e);
            RaiseMouseOverItemEvent();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            this.Refresh();
            base.OnLostFocus(e);
        }

        protected override void OnClick(EventArgs e)
        {
            // Set focus to control
            this.Focus();

            // Call base
            base.OnClick(e);

            // Handle disabled mouse over or selection
            if (AllowImageMouseOverValue && AllowImageSelectValue)
            {
                // Select control
                if (MyMouseOverIndex >= 0 && MyMouseOverIndex != MySelectedIndex)
                {
                    // Change selection
                    MySelectedIndex = MyMouseOverIndex;
                    RaiseSelectedImageChangedEvent();
                    this.Refresh();
                }
                else if (MyMouseOverIndex >= 0 && MyMouseOverIndex == MySelectedIndex)
                {
                    // Nothing to do
                }
                else
                {
                    // Clear selection
                    MySelectedIndex = -1;
                    RaiseSelectedImageChangedEvent();
                    this.Refresh();
                }
            }

            // Raise context menu request on right click
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == System.Windows.Forms.MouseButtons.Right)
                RaiseShowContextMenuEvent();
        }
        
        #endregion
    }
}
