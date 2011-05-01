// ImageListView - A listview control for image files
// Copyright (C) 2009 Ozgur Ozcitak
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Ozgur Ozcitak (ozcitak@yahoo.com)

// Modified TilesRenderer class for Daggerfall Imaging (www.dfworkshop.net).
// Modifications in DrawItem are for displaying tile information from XML rather than file properties.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Xml;

namespace Manina.Windows.Forms
{
    public static partial class CustomImageListViewRenderers
    {
        /// <summary>
        /// Gets the scaled size of an image required to fit
        /// in to the given size keeping the image aspect ratio.
        /// </summary>
        /// <param name="image">The source image.</param>
        /// <param name="fit">The size to fit in to.</param>
        /// <returns></returns>
        internal static Size GetSizedImageBounds(Image image, Size fit)
        {
            float f = System.Math.Max((float)image.Width / (float)fit.Width, (float)image.Height / (float)fit.Height);
            if (f < 1.0f) f = 1.0f; // Do not upsize small images
            int width = (int)System.Math.Round((float)image.Width / f);
            int height = (int)System.Math.Round((float)image.Height / f);
            return new Size(width, height);
        }
        /// <summary>
        /// Gets the bounding rectangle of an image required to fit
        /// in to the given rectangle keeping the image aspect ratio.
        /// </summary>
        /// <param name="image">The source image.</param>
        /// <param name="fit">The rectangle to fit in to.</param>
        /// <param name="hAlign">Horizontal image aligment in percent.</param>
        /// <param name="vAlign">Vertical image aligment in percent.</param>
        /// <returns></returns>
        internal static Rectangle GetSizedImageBounds(Image image, Rectangle fit, float hAlign, float vAlign)
        {
            Size scaled = GetSizedImageBounds(image, fit.Size);
            int x = fit.Left + (int)(hAlign / 100.0f * (float)(fit.Width - scaled.Width));
            int y = fit.Top + (int)(vAlign / 100.0f * (float)(fit.Height - scaled.Height));

            return new Rectangle(x, y, scaled.Width, scaled.Height);
        }
        /// <summary>
        /// Gets the bounding rectangle of an image required to fit
        /// in to the given rectangle keeping the image aspect ratio.
        /// The image will be centered in the fit box.
        /// </summary>
        /// <param name="image">The source image.</param>
        /// <param name="fit">The rectangle to fit in to.</param>
        /// <returns></returns>
        internal static Rectangle GetSizedImageBounds(Image image, Rectangle fit)
        {
            return GetSizedImageBounds(image, fit, 50.0f, 50.0f);
        }

        #region DFTilesRenderer
        /// <summary> 
        /// Displays items with large tiles. 
        /// </summary> 
        public class DFTilesRenderer : ImageListView.ImageListViewRenderer
        {
            private XmlDocument MyLibraryInfoXmlDocument;
            private Font mCaptionFont;
            private int mTileWidth;
            private int mTextHeight;

            /// <summary> 
            /// Gets or sets the width of the tile. 
            /// </summary> 
            public int TileWidth { get { return mTileWidth; } set { mTileWidth = value; } }

            public XmlDocument LibraryInfoXmlDocument
            {
                get { return MyLibraryInfoXmlDocument; }
                set { MyLibraryInfoXmlDocument = value; }
            }

            private Font CaptionFont
            {
                get
                {
                    if (mCaptionFont == null)
                        mCaptionFont = new Font(ImageListView.Font, FontStyle.Bold);
                    return mCaptionFont;
                }
            }

            /// <summary> 
            /// Initializes a new instance of the TilesRenderer class. 
            /// </summary> 
            public DFTilesRenderer()
                : this(150)
            {
                ;
            }

            /// <summary> 
            /// Initializes a new instance of the TilesRenderer class. 
            /// </summary> 
            /// <param name="tileWidth">Width of tiles in pixels.</param> 
            public DFTilesRenderer(int tileWidth)
            {
                mTileWidth = tileWidth;
            }

            /// <summary> 
            /// Releases managed resources. 
            /// </summary> 
            public override void Dispose()
            {
                if (mCaptionFont != null)
                    mCaptionFont.Dispose();

                base.Dispose();
            }

            /// <summary> 
            /// Returns item size for the given view mode. 
            /// </summary> 
            /// <param name="view">The view mode for which the item measurement should be made.</param> 
            public override Size MeasureItem(Manina.Windows.Forms.View view)
            {
                if (view == Manina.Windows.Forms.View.Thumbnails)
                {
                    Size itemSize = new Size();
                    mTextHeight = (int)(5.8f * (float)CaptionFont.Height);

                    // Calculate item size 
                    Size itemPadding = new Size(4, 4);
                    itemSize.Width = ImageListView.ThumbnailSize.Width + 4 * itemPadding.Width + mTileWidth;
                    itemSize.Height = Math.Max(mTextHeight, ImageListView.ThumbnailSize.Height) + 2 * itemPadding.Height;
                    return itemSize;
                }
                else
                    return base.MeasureItem(view);
            }

            /// <summary> 
            /// Draws the specified item on the given graphics. 
            /// </summary> 
            /// <param name="g">The System.Drawing.Graphics to draw on.</param> 
            /// <param name="item">The ImageListViewItem to draw.</param> 
            /// <param name="state">The current view state of item.</param> 
            /// <param name="bounds">The bounding rectangle of item in client coordinates.</param> 
            public override void DrawItem(Graphics g, ImageListViewItem item, ItemState state, Rectangle bounds)
            {
                if (ImageListView.View == Manina.Windows.Forms.View.Thumbnails)
                {
                    Size itemPadding = new Size(4, 4);

                    // Paint background 
                    using (Brush bItemBack = new SolidBrush(ImageListView.Colors.BackColor))
                    {
                        g.FillRectangle(bItemBack, bounds);
                    }
                    if ((ImageListView.Focused && ((state & ItemState.Selected) != ItemState.None)) ||
                        (!ImageListView.Focused && ((state & ItemState.Selected) != ItemState.None) && ((state & ItemState.Hovered) != ItemState.None)))
                    {
                        using (Brush bSelected = new LinearGradientBrush(bounds, ImageListView.Colors.SelectedColor1, ImageListView.Colors.SelectedColor2, LinearGradientMode.Vertical))
                        {
                            Utility.FillRoundedRectangle(g, bSelected, bounds, 4);
                        }
                    }
                    else if (!ImageListView.Focused && ((state & ItemState.Selected) != ItemState.None))
                    {
                        using (Brush bGray64 = new LinearGradientBrush(bounds, ImageListView.Colors.UnFocusedColor1, ImageListView.Colors.UnFocusedColor2, LinearGradientMode.Vertical))
                        {
                            Utility.FillRoundedRectangle(g, bGray64, bounds, 4);
                        }
                    }
                    if (((state & ItemState.Hovered) != ItemState.None))
                    {
                        using (Brush bHovered = new LinearGradientBrush(bounds, ImageListView.Colors.HoverColor1, ImageListView.Colors.HoverColor2, LinearGradientMode.Vertical))
                        {
                            Utility.FillRoundedRectangle(g, bHovered, bounds, 4);
                        }
                    }

                    // Draw the image 
                    Image img = item.ThumbnailImage;
                    if (img != null)
                    {
                        Rectangle pos = GetSizedImageBounds(img, new Rectangle(bounds.Location + itemPadding, ImageListView.ThumbnailSize), 0.0f, 50.0f);
                        g.DrawImage(img, pos);
                        // Draw image border 
                        if (Math.Min(pos.Width, pos.Height) > 32)
                        {
                            using (Pen pOuterBorder = new Pen(ImageListView.Colors.ImageOuterBorderColor))
                            {
                                g.DrawRectangle(pOuterBorder, pos);
                            }
                            if (System.Math.Min(ImageListView.ThumbnailSize.Width, ImageListView.ThumbnailSize.Height) > 32)
                            {
                                using (Pen pInnerBorder = new Pen(ImageListView.Colors.ImageInnerBorderColor))
                                {
                                    g.DrawRectangle(pInnerBorder, Rectangle.Inflate(pos, -1, -1));
                                }
                            }
                        }

                        // Get DF image details
                        string Description = "File info not found";
                        string RecordCount = "0";
                        if (null != MyLibraryInfoXmlDocument)
                        {
                            XmlNodeList Nodes = MyLibraryInfoXmlDocument.GetElementsByTagName(item.Text);
                            if (Nodes.Count > 0)
                            {
                                Description = Nodes[0].InnerText;
                                RecordCount = Nodes[0].Attributes["Records"].Value;
                            }
                        }

                        // Draw item text 
                        int lineHeight = CaptionFont.Height;
                        RectangleF rt;
                        using (StringFormat sf = new StringFormat())
                        {
                            rt = new RectangleF(bounds.Left + 2 * itemPadding.Width + ImageListView.ThumbnailSize.Width,
                                bounds.Top + itemPadding.Height + (Math.Max(ImageListView.ThumbnailSize.Height, mTextHeight) - mTextHeight) / 2,
                                mTileWidth, lineHeight);
                            sf.Alignment = StringAlignment.Near;
                            sf.FormatFlags = StringFormatFlags.NoWrap;
                            sf.LineAlignment = StringAlignment.Center;
                            sf.Trimming = StringTrimming.EllipsisCharacter;
                            using (Brush bItemFore = new SolidBrush(ImageListView.Colors.ForeColor))
                            {
                                // Draw filename
                                g.DrawString(item.Text, CaptionFont, bItemFore, rt, sf);
                                rt.Offset(0, 1.5f * lineHeight);
                            }
                            using (Brush bItemDetails = new SolidBrush(ImageListView.Colors.PaneLabelColor))
                            {
                                // Draw description
                                g.DrawString(Description, ImageListView.Font, bItemDetails, rt, sf);
                                rt.Offset(0, 1.1f * lineHeight);

                                // Draw record count
                                g.DrawString(RecordCount + " records ", ImageListView.Font, bItemDetails, rt, sf);
                                rt.Offset(0, 1.1f * lineHeight);
                            }
                        }
                    }

                    // Item border 
                    using (Pen pWhite128 = new Pen(Color.FromArgb(128, ImageListView.Colors.ControlBackColor)))
                    {
                        Utility.DrawRoundedRectangle(g, pWhite128, bounds.Left + 1, bounds.Top + 1, bounds.Width - 3, bounds.Height - 3, 4);
                    }
                    if (ImageListView.Focused && ((state & ItemState.Selected) != ItemState.None))
                    {
                        using (Pen pHighlight128 = new Pen(ImageListView.Colors.SelectedBorderColor))
                        {
                            Utility.DrawRoundedRectangle(g, pHighlight128, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1, 4);
                        }
                    }
                    else if (!ImageListView.Focused && ((state & ItemState.Selected) != ItemState.None))
                    {
                        using (Pen pGray128 = new Pen(ImageListView.Colors.UnFocusedBorderColor))
                        {
                            Utility.DrawRoundedRectangle(g, pGray128, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1, 4);
                        }
                    }
                    else if ((state & ItemState.Selected) == ItemState.None)
                    {
                        using (Pen pGray64 = new Pen(ImageListView.Colors.BorderColor))
                        {
                            Utility.DrawRoundedRectangle(g, pGray64, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1, 4);
                        }
                    }

                    if (ImageListView.Focused && ((state & ItemState.Hovered) != ItemState.None))
                    {
                        using (Pen pHighlight64 = new Pen(ImageListView.Colors.HoverBorderColor))
                        {
                            Utility.DrawRoundedRectangle(g, pHighlight64, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1, 4);
                        }
                    }

                    // Focus rectangle 
                    if (ImageListView.Focused && ((state & ItemState.Focused) != ItemState.None))
                    {
                        ControlPaint.DrawFocusRectangle(g, bounds);
                    }
                }
                else
                    base.DrawItem(g, item, state, bounds);
            }
        }
        #endregion
    }
}