// Project:         DaggerfallModelling
// Description:     Explore and export 3D models from Daggerfall.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Imports

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

#endregion

namespace DaggerfallModelling.ViewControls
{

    /// <summary>
    /// Explore a list of Daggerfall models as thumbnails.
    /// </summary>
    public class ThumbnailView : ViewBase
    {
        #region Class Variables

        // Layout
        private int thumbsPerRow = 5;
        private int thumbsFirstVisibleRow = 0;
        private int thumbSpacing = 16;
        private int thumbWidth;
        private int thumbHeight;
        private Dictionary<int, Thumbnails> thumbDict = new Dictionary<int, Thumbnails>();

        // Scrolling
        private int thumbScrollAmount = 0;
        private int thumbScrollVelocity = 0;
        
        // Appearance
        private Color thumbViewBackgroundColor = Color.White;
        private Color thumbTextColor = Color.BlanchedAlmond;
        private int mouseOverThumbGrow = 16;

        // Resources
        private const string thumbBackgroundFile = "thumbnail_background.png";
        private Texture2D thumbBackgroundTexture;

        // XNA
        private VertexDeclaration vertexDeclaration;
        private BasicEffect effect;
        private float nearPlaneDistance = 1.0f;
        private float farPlaneDistance = 10000.0f;
        private Matrix projectionMatrix;
        private Matrix viewMatrix;
        private Vector3 cameraPosition = new Vector3(0, 0, 1000);
        private Vector3 cameraReference = new Vector3(0, 0, -1);
        private Vector3 cameraUpVector = new Vector3(0, 1, 0);

        // Mouse
        private bool mouseInClientArea = false;
        private int mouseOverThumb = -1;

        // Models list
        private bool useFilteredModels = false;

        #endregion

        #region Class Structures

        /// <summary>
        /// Defines a single thumbnail item.
        /// </summary>
        private struct Thumbnails
        {
            public int index;
            public int key;
            public Rectangle rect;
            public ModelManager.Model model;
            public Texture2D texture;
            public Matrix matrix;
            public float rotation;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets model key of thumbnail under the mouse.
        ///  Returns -1 if no thumbnail under mouse.
        /// </summary>
        public int MouseOverThumbnail
        {
            get { return mouseOverThumb; } 
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public ThumbnailView(ViewHost host)
            : base(host)
        {
            // Camera modes not used in this view
            CameraMode = CameraModes.None;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Called by host when view should initialise.
        /// </summary>
        public override void Initialize()
        {
            // Create vertex declaration
            vertexDeclaration = new VertexDeclaration(host.GraphicsDevice, VertexPositionNormalTexture.VertexElements);

            // Setup basic effect
            effect = new BasicEffect(host.GraphicsDevice, null);
            effect.World = Matrix.Identity;
            effect.TextureEnabled = true;
            effect.PreferPerPixelLighting = true;
            effect.EnableDefaultLighting();
            effect.AmbientLightColor = new Vector3(0.4f, 0.4f, 0.4f);
            effect.SpecularColor = new Vector3(0.2f, 0.2f, 0.2f);

            // Setup camera
            float aspectRatio = (float)host.GraphicsDevice.Viewport.Width / (float)host.GraphicsDevice.Viewport.Height;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, nearPlaneDistance, farPlaneDistance);
            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraPosition + cameraReference, cameraUpVector);

            // Load the thumbnail background texture
            LoadThumbnailBackgroundTexture();

            // Show initial status
            UpdateStatusMessage();
        }

        /// <summary>
        /// Called by host when view should update animation.
        /// </summary>
        public override void Tick()
        {
            // Update
            ScrollThumbsView(thumbScrollVelocity);
            TrackMouseOver();
            LayoutThumbnails();
        }

        /// <summary>
        /// Called by host when view should redraw.
        /// </summary>
        public override void Draw()
        {
            // Clear display
            host.GraphicsDevice.Clear(thumbViewBackgroundColor);

            // Draw thumbnails
            DrawThumbnails();
        }

        /// <summary>
        /// Called by host when view should redraw.
        /// </summary>
        public override void Resize()
        {
            // Update thumbnail layout
            LayoutThumbnails();
            UpdateStatusMessage();
            host.Refresh();
        }

        /// <summary>
        /// Called when view should track mouse movement.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public override void OnMouseMove(MouseEventArgs e)
        {
            // Handle thumbnail scrolling
            if (host.RightMouseDown)
            {
                // Update thumbs for scrolling
                ScrollThumbsView(host.MousePosDelta.Y);
                LayoutThumbnails();

                // Request redraw now for smoother mouse scrolling
                host.Refresh();
            }
        }

        /// <summary>
        /// Called when the mouse wheel is scrolled.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public override void OnMouseWheel(MouseEventArgs e)
        {
            // Clear velocity on wheel
            thumbScrollVelocity = 0;

            // Calculate scroll amount
            int amount = (e.Delta / 120) * 60;
            if (amount < -thumbHeight / 2) amount = -thumbHeight / 2;
            if (amount > thumbHeight / 2) amount = thumbHeight / 2;
            ScrollThumbsView(amount);
            LayoutThumbnails();

            // Request redraw now
            host.Refresh();
        }

        /// <summary>
        /// Called when a mouse button is pressed.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public override void OnMouseDown(MouseEventArgs e)
        {
            // Clear velocity for any mouse down event
            thumbScrollVelocity = 0;
        }

        /// <summary>
        /// Called when a mouse button is released.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public override void OnMouseUp(MouseEventArgs e)
        {
            // Set scroll velocity on right mouse up
            if (e.Button == MouseButtons.Right)
            {
                thumbScrollVelocity = (int)host.MouseVelocity.Y;
                if (thumbScrollVelocity <= -thumbHeight / 2) thumbScrollVelocity = -thumbHeight / 2;
                if (thumbScrollVelocity >= thumbHeight / 2) thumbScrollVelocity = thumbHeight / 2;
            }
        }

        /// <summary>
        /// Called when mouse enters client area.
        /// </summary>
        /// <param name="e">EventArgs</param>
        public override void OnMouseEnter(EventArgs e)
        {
            mouseInClientArea = true;
        }

        /// <summary>
        /// Called when mouse leaves client area.
        /// </summary>
        /// <param name="e">EventArgs</param>
        public override void OnMouseLeave(EventArgs e)
        {
            mouseInClientArea = false;
        }

        /// <summary>
        /// Called when filtered models array has been changed.
        /// </summary>
        public override void FilteredModelsChanged()
        {
            // Reset view
            if (host.FilteredModelsArray == null)
            {
                useFilteredModels = false;
                thumbsFirstVisibleRow = 0;
                thumbScrollAmount = 0;
            }
            else
            {
                useFilteredModels = true;
                thumbsFirstVisibleRow = 0;
                thumbScrollAmount = 0;
            }

            // Clear thumbnails.
            // This forces a full rebuild on next redraw.
            thumbDict.Clear();
        }

        /// <summary>
        /// Called when the view is resumed after being inactive.
        ///  Allows view to perform any layout or other requirements before redraw.
        /// </summary>
        public override void ResumeView()
        {
            // Check filtered models list is active.
            // A little bit of handling here to keep list at same position
            // unless a reset is specfically needed due to array gone, overflow, etc.
            // This will resume the same view whenever possible.
            if (useFilteredModels)
            {
                if (host.FilteredModelsArray == null)
                {
                    // Filter array no longer active. Reset view.
                    FilteredModelsChanged();
                }
                else
                {
                    // Check still within bounds of array. Reset view if overflow.
                    if (thumbsFirstVisibleRow * thumbsPerRow > host.FilteredModelsArray.Length)
                    {
                        FilteredModelsChanged();
                    }
                }
            }
            else
            {
                // If a filter array is now active, switch to it
                if (host.FilteredModelsArray != null)
                {
                    FilteredModelsChanged();
                }
            }

            // Reset layout, status message, and refresh
            thumbScrollVelocity = 0;
            LayoutThumbnails();
            UpdateStatusMessage();
            host.Refresh();
        }

        #endregion

        #region Drawing Methods

        private void DrawThumbnails()
        {
            // Draw thumbnail sprites
            int drawTextKey = -1;
            host.SpriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);
            foreach (var item in thumbDict)
            {
                // Draw thumbnail
                host.SpriteBatch.Draw(item.Value.texture, item.Value.rect, Color.White);

                // Set key to draw text over thumb
                if (mouseOverThumb == item.Key)
                    drawTextKey = item.Key;
            }
            host.SpriteBatch.End();

            // Draw thumbnail id text. This is done in a second batch to avoid alpha problems
            // with sprite render on some cards.
            host.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            if (drawTextKey != -1)
            {
                Thumbnails thumb = thumbDict[drawTextKey];
                string thumbText = string.Format("{0}", thumb.key.ToString());
                Vector2 thumbTextSize = host.SmallFont.MeasureString(thumbText);
                int textX = (int)thumb.rect.X + (int)(thumb.rect.Width - thumbTextSize.X) / 2;
                Vector2 textPos = new Vector2(textX, (float)thumb.rect.Bottom - thumbTextSize.Y - 4);
                host.SpriteBatch.DrawString(host.SmallFont, thumbText, textPos, thumbTextColor);
            }
            host.SpriteBatch.End();
        }

        private void DrawSingleModel(ref ModelManager.Model model)
        {
            // Set render states
            host.GraphicsDevice.RenderState.DepthBufferEnable = true;
            host.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            host.GraphicsDevice.RenderState.AlphaTestEnable = false;
            host.GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            host.GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
            host.GraphicsDevice.RenderState.CullMode = CullMode.None;
            host.GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Linear;
            host.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Linear;
            host.GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Linear;
            host.GraphicsDevice.SamplerStates[0].MaxAnisotropy = 0;

            // Set vertex declaration
            host.GraphicsDevice.VertexDeclaration = vertexDeclaration;

            // Set view and projection matrices
            effect.View = viewMatrix;
            effect.Projection = projectionMatrix;

            // Draw submeshes
            foreach (var submesh in model.SubMeshes)
            {
                effect.Texture = host.TextureManager.GetTexture(submesh.TextureKey);

                effect.Begin();
                effect.CurrentTechnique.Passes[0].Begin();

                host.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                    model.Vertices, 0, model.Vertices.Length,
                    submesh.Indices, 0, submesh.Indices.Length / 3);

                effect.CurrentTechnique.Passes[0].End();
                effect.End();
            }
        }

        #endregion

        #region Thumbnail Management

        private void LoadThumbnailBackgroundTexture()
        {
            // Load thumbnail background texture
            thumbBackgroundTexture = Texture2D.FromFile(
                host.GraphicsDevice,
                Path.Combine(Application.StartupPath, thumbBackgroundFile));

            // Throw if load failed
            if (thumbBackgroundTexture == null)
                throw new Exception("Could not load thumbnail background image.");
        }

        /// <summary>
        /// Performs layout of thumbnails.
        /// </summary>
        private void LayoutThumbnails()
        {
            // Host must be ready as layout depends on host control dimensions
            if (!host.IsReady)
                return;

            // If this is a full layout, set flag to update status at end
            bool updateStatus = false;
            if (thumbDict.Count == 0)
                updateStatus = true;

            // Calc dimensions. Leaving a little gap on the right for a scrollbar.
            thumbWidth = ((host.Width - thumbSpacing * thumbsPerRow) / thumbsPerRow) - (thumbSpacing / thumbsPerRow + 1);
            thumbHeight = thumbWidth;

            // Calc ranges.
            // Top row is started one row up so no pop-in when scrolling up.
            // Bottom row is also extended so no pop-in when scrolling down.
            int visibleRows = host.Height / (thumbHeight + thumbSpacing) + 3;
            int firstIndex = (thumbsFirstVisibleRow - 1) * thumbsPerRow;
            int lastIndex = firstIndex + visibleRows * thumbsPerRow - 1;

            // Cap last index
            int maxIndex;
            if (!useFilteredModels)
            {
                // Use full model database
                maxIndex = host.ModelManager.Arch3dFile.Count - 1;
                if (lastIndex > maxIndex)
                    lastIndex = maxIndex;
            }
            else
            {
                // Use filtered model array
                maxIndex = host.FilteredModelsArray.Length - 1;
                if (lastIndex > maxIndex)
                    lastIndex = maxIndex;
            }

            // Calc screen position of first index
            int xpos = thumbSpacing;
            int ypos = -thumbHeight + thumbScrollAmount;

            // Stop scrolling up when at the top row
            if (firstIndex < 0 && thumbScrollAmount > 0)
            {
                ypos = -thumbHeight;
                thumbScrollAmount = 0;
            }

            // Stop scrolling down when bottom row in view
            if (thumbsFirstVisibleRow > (maxIndex / thumbsPerRow - visibleRows) + 3 && thumbScrollAmount < 0)
            {
                ypos = -thumbHeight;
                thumbScrollAmount = 0;
            }

            // Remove out of range thumbnails
            List<int> keysToRemove = new List<int>();
            foreach (var item in thumbDict)
            {
                // Queue for removal if out of range
                if (item.Value.index < firstIndex || item.Value.index > lastIndex)
                    keysToRemove.Add(item.Key);
            }

            // Remove thumbnails queued for deletion
            foreach (int key in keysToRemove)
                thumbDict.Remove(key);

            // Arrange thumbnails, updating existing and inserting new
            int colCount = 0;
            for (int index = firstIndex; index <= lastIndex; index++)
            {
                // Only render indices zero or higher. It's possible to have a negative index
                // based on view starting one row higher for scrolling purposes.
                if (index >= 0)
                {
                    // Get key based on model source
                    int key;
                    if (!useFilteredModels)
                    {
                        // Use full model database
                        key = (int)host.ModelManager.Arch3dFile.GetRecordId(index);
                    }
                    else
                    {
                        // Use filtered model array
                        key = host.FilteredModelsArray[index];
                    }

                    // Update or create
                    if (thumbDict.ContainsKey(key))
                    {
                        // Update position and size
                        Thumbnails thumb = thumbDict[key];
                        thumb.rect = new Rectangle(xpos, ypos, thumbWidth, thumbHeight);
                        thumbDict[key] = thumb;
                    }
                    else
                    {
                        // Create thumbnail
                        Thumbnails thumb = new Thumbnails();
                        thumb.index = index;
                        thumb.key = key;
                        thumb.rect = new Rectangle(xpos, ypos, thumbWidth, thumbHeight);
                        UpdateThumbnailTexture(ref thumb);
                        thumbDict.Add(thumb.key, thumb);
                    }

                    // Animate thumb under mouse
                    if (key == mouseOverThumb)
                        AnimateThumb(key);
                }

                // Update position for next thumbnail
                colCount++;
                xpos += thumbWidth + thumbSpacing;
                if (colCount >= thumbsPerRow)
                {
                    colCount = 0;
                    ypos += thumbHeight + thumbSpacing;
                    xpos = thumbSpacing;
                }
            }

            // Update status
            if (updateStatus)
                UpdateStatusMessage();
        }

        /// <summary>
        /// Update thumbnail to represent contained model.
        /// </summary>
        private void UpdateThumbnailTexture(ref Thumbnails thumb)
        {
            // Get dimensions
            int thumbWidth = thumbBackgroundTexture.Width;
            int thumbHeight = thumbBackgroundTexture.Height;

            // Get model
            if (thumb.model.Vertices == null)
            {
                // Load model
                thumb.model = host.ModelManager.GetModel(thumb.key, false);

                // Load texture for each submesh.
                for (int sm = 0; sm < thumb.model.SubMeshes.Length; sm++)
                {
                    // Load textures
                    thumb.model.SubMeshes[sm].TextureKey =
                        host.TextureManager.LoadTexture(
                        thumb.model.SubMeshes[sm].TextureArchive,
                        thumb.model.SubMeshes[sm].TextureRecord);
                }

                // Centre model
                Vector3 Min = thumb.model.BoundingBox.Min;
                Vector3 Max = thumb.model.BoundingBox.Max;
                float transX = (float)(Min.X + ((Max.X - Min.X) / 2));
                float transY = (float)(Min.Y + ((Max.Y - Min.Y) / 2));
                float transZ = (float)(Min.Z + ((Max.Z - Min.Z) / 2));
                Matrix matrix = Matrix.CreateTranslation(-transX, -transY, -transZ);

                // Rotate model
                matrix *= Matrix.CreateRotationY(MathHelper.ToRadians(45));
                matrix *= Matrix.CreateRotationX(MathHelper.ToRadians(10));

                // Scale model
                Vector3 size = new Vector3(Max.X - Min.X, Max.Y - Min.Y, Max.Z - Min.Z);
                float scale = 400.0f / (float)((size.X + size.Y + size.Z) / 3);
                matrix *= Matrix.CreateScale(scale);

                // Apply matrix to model
                thumb.model = host.ModelManager.TransformModel(ref thumb.model, matrix);

                // Store matrix
                thumb.matrix = matrix;
            }

            // Create projection matrix
            float aspectRatio = (float)thumb.rect.Width / (float)thumb.rect.Height;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, nearPlaneDistance, farPlaneDistance);

            // Set world matrix
            effect.World = Matrix.CreateRotationY(MathHelper.ToRadians(thumb.rotation));

            // Create texture to use as render target
            RenderTarget2D renderTarget;
            renderTarget = new RenderTarget2D(
                host.GraphicsDevice,
                thumbWidth,
                thumbHeight,
                1,
                host.GraphicsDevice.DisplayMode.Format,
                RenderTargetUsage.PreserveContents);
            host.GraphicsDevice.SetRenderTarget(0, renderTarget);

            // Render thumbnail components
            host.GraphicsDevice.Clear(thumbViewBackgroundColor);
            host.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            host.SpriteBatch.Draw(thumbBackgroundTexture, new Rectangle(0, 0, thumbWidth, thumbHeight), Color.White);
            host.SpriteBatch.End();
            DrawSingleModel(ref thumb.model);

            // Restore default render target
            host.GraphicsDevice.SetRenderTarget(0, null);

            // A texture created from a render target is coupled to that render target.
            // It can be easily lost when device is reset (commonly on resize) and must be re-created.
            // This is a Windows Forms application so resizing is going to be very common.
            // Rather than rebuild this texture on each resize event, just decouple from render target.
            // This is done by GetData to color array, then SetData into a fresh new texture.
            // The new texture will persist for life of GraphicsDevice and through device resets.
            Texture2D renderTargetTexture = renderTarget.GetTexture();
            Color[] colorArray = new Color[renderTargetTexture.Width * renderTargetTexture.Height];
            renderTargetTexture.GetData<Color>(colorArray);
            Texture2D newTexture = new Texture2D(host.GraphicsDevice, renderTargetTexture.Width, renderTargetTexture.Height);
            newTexture.SetData<Color>(colorArray);

            // Store updated values
            thumb.texture = newTexture;
        }

        /// <summary>
        /// Handle thumbnail view scrolling.
        /// </summary>
        /// <param name="amount">Amount in pixels to scroll view.</param>
        private void ScrollThumbsView(int amount)
        {
            // Get total rows
            int totalRows = host.ModelManager.Arch3dFile.Count / thumbsPerRow;

            // Apply scroll amount
            thumbScrollAmount += amount;

            // Handle scrolling models up with a new row appearing at the bottom
            if (thumbScrollAmount <= -(thumbHeight + thumbSpacing))
            {
                if (thumbsFirstVisibleRow < totalRows)
                {
                    thumbsFirstVisibleRow++;
                    thumbScrollAmount += (thumbHeight + thumbSpacing);
                    UpdateStatusMessage();
                }
            }

            // Handle scrolling models down with a new row appearing at the top
            if (thumbScrollAmount >= (thumbHeight + thumbSpacing))
            {
                if (thumbsFirstVisibleRow > 0)
                {
                    thumbsFirstVisibleRow--;
                    thumbScrollAmount -= (thumbHeight + thumbSpacing);
                    UpdateStatusMessage();
                }
            }
        }

        /// <summary>
        /// Tracks mouse over thumbnails.
        /// </summary>
        /// <param name="x">Mouse X.</param>
        /// <param name="y">Mouse Y.</param>
        private void TrackMouseOver()
        {
            // Clear previous mouse over thumb
            mouseOverThumb = -1;

            // Do nothing further when mouse not in client area
            if (!mouseInClientArea)
                return;

            // Scan for current mouse over thumb
            foreach (var item in thumbDict)
            {
                if (item.Value.rect.Contains(host.MousePos.X, host.MousePos.Y))
                {
                    // Set mouse over thumb
                    mouseOverThumb = item.Value.key;
                    break;
                }
            }
        }

        /// <summary>
        /// Animate thumbnail during mouse-over state.
        /// </summary>
        /// <param name="key"></param>
        private void AnimateThumb(int key)
        {
            if (!thumbDict.ContainsKey(key))
                return;

            Thumbnails thumb = thumbDict[key];

            // Step rotation in degrees
            thumb.rotation += 1;
            if (thumb.rotation > 360.0f) thumb.rotation -= 360.0f;
            UpdateThumbnailTexture(ref thumb);

            // Enlarge rect size
            thumb.rect.X -= mouseOverThumbGrow / 2;
            thumb.rect.Y -= mouseOverThumbGrow / 2;
            thumb.rect.Width += mouseOverThumbGrow;
            thumb.rect.Height += mouseOverThumbGrow;

            thumbDict[key] = thumb;
        }

        #endregion

        #region Private Methods

        private void UpdateStatusMessage()
        {
            int count;
            string message;

            // State how many models we are viewing
            if (!useFilteredModels)
            {
                count = host.ModelManager.Arch3dFile.Count;
                message = string.Format("Exploring all {0} models in ARCH3D.BSA.", count);
            }
            else
            {
                count = host.FilteredModelsArray.Length;
                message = string.Format("Exploring filtered list of {0} models.", count);
            }

            // Determine which thumbnails are actually visible in the client area.
            // Discounts non-visible thumbnails above and below client used for scrolling.
            int first = 99999, last = -99999;
            foreach (var item in thumbDict)
            {
                // Get System.Drawing.Rectangle for thumbnail
                Thumbnails thumb = thumbDict[item.Key];
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(
                    thumb.rect.Left, thumb.rect.Top, thumb.rect.Width, thumb.rect.Height);

                // Only consider if completely or partially inside client rectangle
                if (host.ClientRectangle.Contains(rect))
                {
                    if (thumb.index < first)
                        first = thumb.index;
                    if (thumb.index > last)
                        last = thumb.index;
                }
            }

            // Add position in list
            message += string.Format(" You are viewing models {0}-{1}.", first, last);

            // Set the message
            host.StatusMessage = message;
        }

        #endregion

    }

}
