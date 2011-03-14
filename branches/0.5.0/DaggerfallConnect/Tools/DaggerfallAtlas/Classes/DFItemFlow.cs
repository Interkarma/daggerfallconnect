// Project:         DaggerfallAtlas
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

#endregion

namespace DaggerfallAtlas.Classes
{
    /// <summary>
    /// Item flow layout parent control.
    /// </summary>
    public abstract class DFItemFlow : ScrollableControl
    {
        #region Class Variables

        private Point MousePos;
        private int PageHeight = 0;
        private LayoutItem[] MyLayout = null;
        private Padding MyInnerMargin = new Padding(6, 6, 6, 6);
        private float MyZoomAmount = 1.0f;
        private int MyMouseOverIndex = -1;
        private int MySelectedIndex = -1;
        private bool ShowItemBordersValue = true;
        private bool AllowItemMouseOverValue = true;
        private bool AllowItemSelectValue = true;

        #endregion

        #region Class Structures

        private struct LayoutItem
        {
            public bool IsVisible;
            public Size size;
            public Rectangle OuterBounds;
            public Rectangle InnerBounds;
        }

        #endregion

        #region Constructors

        public DFItemFlow()
        {
            // Set value of double-buffering style bits to true
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            // Init scroll bar
            ResetScrollBars();
        }

        #endregion

        #region Public Properties

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

        public abstract int ItemCount
        {
            get;
        }

        #endregion

        #region Abstract Method Definition

        protected abstract Size GetItemSize(int index);

        protected abstract void PaintItem(int index, Point position, Graphics gr);

        #endregion

        #region SelectedItemChanged Event

        public class SelectedItemEventArgs
        {
            public int index;
        }

        public delegate void ItemSelectedEventHandler(object sender, SelectedItemEventArgs e);
        public event ItemSelectedEventHandler SelectedImageChanged;

        protected virtual void RaiseSelectedItemChangedEvent()
        {
            // Raise event
            if (null != SelectedImageChanged)
            {
                // Populate event args based on selection
                SelectedItemEventArgs e = new SelectedItemEventArgs();
                if (MySelectedIndex >= 0)
                {
                    e.index = MySelectedIndex;
                }
                else
                {
                    e.index = -1;
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
            public int selectedIndex;
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
                    e.selectedIndex = MySelectedIndex;
                }
                else
                {
                    e.selectedIndex = -1;
                }

                // Raise event
                ShowContextMenu(this, e);
            }
        }

        #endregion

        #region MouseOverItem Event

        public delegate void MouseOverItemEventHandler(object sender, SelectedItemEventArgs e);
        public event MouseOverItemEventHandler MouseOverItem;

        protected virtual void RaiseMouseOverItemEvent()
        {
            // Raise event
            if (null != MouseOverItem)
            {
                // Populate event args based on selection
                SelectedItemEventArgs e = new SelectedItemEventArgs();
                if (MyMouseOverIndex >= 0)
                {
                    e.index = MyMouseOverIndex;
                }
                else
                {
                    e.index = -1;
                }

                // Raise event
                MouseOverItem(this, e);
            }
        }

        #endregion

        #region Private Methods

        private void BuildLayoutArray()
        {
            // Create layout array
            MyLayout = new LayoutItem[ItemCount];

            // Add items to layout array
            //int curItem = 0;
            for (int item = 0; item < ItemCount; item++)
            {
                // Create new layout item
                MyLayout[item] = new LayoutItem();
                MyLayout[item].size = GetItemSize(item);
            }

            // Reset mouse over and selected indices
            MyMouseOverIndex = -1;
            MySelectedIndex = -1;
            RaiseSelectedItemChangedEvent();
        }

        #endregion

        #region Layout Methods

        protected void NewLayout()
        {
            MyLayout = null;
            UpdateView();
        }

        private void UpdateView()
        {
            // Recalculate layout based on new size
            UpdateFlowLayout();

            // Update scrolling
            UpdateScroller();

            // Force view redraw
            this.Refresh();
        }

        private Rectangle CalculateOuterBounds(Size size)
        {
            // This is the total selectable area of the item tile plus margin
            return new Rectangle(
                0,
                0,
                MyInnerMargin.Horizontal + (int)(size.Width * MyZoomAmount),
                MyInnerMargin.Vertical + (int)(size.Height * MyZoomAmount));
        }

        private Rectangle CalculateInnerBounds(Size size)
        {
            // This is the interior cel containing item tile only
            return new Rectangle(
                MyInnerMargin.Left,
                MyInnerMargin.Right,
                (int)(size.Width * MyZoomAmount),
                (int)(size.Height * MyZoomAmount));
        }

        private void UpdateFlowLayout()
        {
            // Do nothing in design mode
            if (DesignMode)
                return;

            // Build layout if null
            if (MyLayout == null)
                BuildLayoutArray();

            PageHeight = 0;
            int MaxTileHeight = 0;
            Point pos = new Point(Margin.Left, Margin.Top);
            for (int i = 0; i < MyLayout.Length; i++)
            {
                // Calculate bounds
                Rectangle OuterBounds = CalculateOuterBounds(MyLayout[i].size);
                Rectangle InnerBounds = CalculateInnerBounds(MyLayout[i].size);

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
            // Work like an empty control if in design mode
            if (DesignMode)
            {
                base.OnSizeChanged(e);
                return;
            }

            UpdateView();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (!DesignMode)
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
            // Just work like an empty control if in design mode
            if (DesignMode)
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

                // Draw cel containing item
                if (i == MyMouseOverIndex && i != MySelectedIndex)
                {
                    // Paint item background when mouse over item
                    Brush brush = new LinearGradientBrush(OuterRect, Color.AliceBlue, Color.LightBlue, LinearGradientMode.Vertical);
                    e.Graphics.FillRectangle(brush, OuterRect);
                    if (ShowItemBordersValue) e.Graphics.DrawRectangle(new Pen(Color.DarkBlue), OuterRect);
                }
                else if (i == MyMouseOverIndex && i == MySelectedIndex)
                {
                    // Paint item background when mouse over selected item
                    Brush brush = new LinearGradientBrush(OuterRect, Color.LightBlue, Color.DarkCyan, LinearGradientMode.Vertical);
                    e.Graphics.FillRectangle(brush, OuterRect);
                    if (ShowItemBordersValue) e.Graphics.DrawRectangle(new Pen(Color.Blue), OuterRect);
                }
                else if (i == MySelectedIndex)
                {
                    if (this.Focused)
                    {
                        // Paint selected item background when control has focus
                        Brush brush = new LinearGradientBrush(OuterRect, Color.LightBlue, Color.DarkCyan, LinearGradientMode.Vertical);
                        e.Graphics.FillRectangle(brush, OuterRect);
                        if (ShowItemBordersValue) e.Graphics.DrawRectangle(new Pen(Color.Black), OuterRect);
                    }
                    else
                    {
                        // Paint selected item background when control has lost focus
                        Brush brush = new LinearGradientBrush(OuterRect, Color.LightGray, Color.DarkSlateGray, LinearGradientMode.Vertical);
                        e.Graphics.FillRectangle(brush, OuterRect);
                        if (ShowItemBordersValue) e.Graphics.DrawRectangle(new Pen(Color.Black), OuterRect);
                    }
                }
                else
                {
                    // Paint all other item borders
                    if (ShowItemBordersValue) e.Graphics.DrawRectangle(new Pen(Color.FromArgb(90, 90, 120)), OuterRect);
                }

                // Draw item interior
                PaintItem(i, new Point(InnerRect.Left, InnerRect.Top), e.Graphics);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            // Store location
            MousePos = e.Location;

            // Validate
            if (null == MyLayout || !AllowItemMouseOverValue)
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
            if (AllowItemMouseOverValue && AllowItemSelectValue)
            {
                // Select control
                if (MyMouseOverIndex >= 0 && MyMouseOverIndex != MySelectedIndex)
                {
                    // Change selection
                    MySelectedIndex = MyMouseOverIndex;
                    RaiseSelectedItemChangedEvent();
                    this.Refresh();
                }
                else if (MyMouseOverIndex >= 0 && MyMouseOverIndex == MySelectedIndex)
                {
                    // Nothing to do yet
                }
                else
                {
                    // Clear selection
                    MySelectedIndex = -1;
                    RaiseSelectedItemChangedEvent();
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
