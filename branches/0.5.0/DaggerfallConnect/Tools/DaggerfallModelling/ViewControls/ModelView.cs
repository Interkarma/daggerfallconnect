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

    class ModelView : WinFormsGraphicsDevice.GraphicsDeviceControl
    {

        #region Class Variables

        //
        // Resources
        //
        private bool isReady = false;
        private string arena2Path = string.Empty;
        private ViewModes viewMode = ViewModes.ModelThumbs;

        //
        // Managers
        //
        private TextureManager textureManager;
        private ModelManager modelManager;

        //
        // XNA
        //
        private long startTime = 0;
        private long timeInSeconds = 0;
        private uint frameCount = 0;
        private SpriteBatch spriteBatch;
        private VertexDeclaration vertexDeclaration;
        private BasicEffect effect;
        private float nearPlaneDistance = 1.0f;
        private float farPlaneDistance = 50000.0f;
        private Matrix projectionMatrix;
        private Matrix viewMatrix;
        private Vector3 cameraPosition = new Vector3(0, 0, 1000);
        private Vector3 cameraReference = new Vector3(0, 0, -1);
        private Vector3 cameraUpVector = new Vector3(0, 1, 0);

        //
        // Model thumbnails view
        //
        private int thumbsPerRow = 5;
        private int thumbsFirstVisibleRow = 1110;
        private int thumbScrollAmount = 0;
        private int thumbSpacing = 16;
        private int thumbWidth;
        private int thumbHeight;
        private Color thumbViewBackgroundColor = Color.White;
        private const string thumbBackgroundFile = "thumbnail_background.png";
        private Texture2D thumbBackgroundTexture;
        private Dictionary<int, Thumbnails> thumbDict = new Dictionary<int, Thumbnails>();
        private float thumbScrollVelocity = 0.0f;

        //
        // Mouse
        //
        private Point mousePos;
        private long mouseTime;
        private Point mousePosDelta;
        private long mouseTimeDelta;
        private bool leftMouseDown = false;
        private bool rightMouseDown = false;

        //
        // Timer
        ///
        private Timer animTimer = new Timer();

        #endregion

        #region Class Structures

        /// <summary>
        /// Different view modes supported by this control.
        /// </summary>
        public enum ViewModes
        {
            /// <summary>Viewing a flow layout of model thumbnails.</summary>
            ModelThumbs,
            /// <summary>Viewing a single model.</summary>
            SingleModel,
            /// <summary>Viewing an exterior block.</summary>
            ExteriorBlock,
            /// <summary>Viewing a dungeon block.</summary>
            DungeonBlock,
            /// <summary>Viewing entire exterior map.</summary>
            ExteriorFull,
            /// <summary>Viewing entire dungeon map.</summary>
            DungeonFull,
        }

        private struct Thumbnails
        {
            public int index;
            public int key;
            public Rectangle rect;
            public ModelManager.Model model;
            public Texture2D texture;
            public float angle;
            public bool update;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets ready state.
        /// </summary>
        public bool IsReady
        {
            get { return isReady; }
        }

        /// <summary>
        /// Gets current ViewMode.
        /// </summary>
        public ViewModes ViewMode
        {
            get { return viewMode; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ModelView()
        {
            // Measure start time of control
            startTime = DateTime.Now.Ticks;
        }

        #endregion

        #region Abstract Implementations

        /// <summary>
        /// Initialise the control.
        /// </summary>
        protected override void Initialize()
        {
            // Handle device reset event
            GraphicsDevice.DeviceReset += new EventHandler(GraphicsDevice_DeviceReset);

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create vertex declaration
            vertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionNormalTexture.VertexElements);

            // Setup basic effect
            effect = new BasicEffect(GraphicsDevice, null);
            effect.World = Matrix.Identity;
            effect.TextureEnabled = true;
            effect.PreferPerPixelLighting = true;
            effect.EnableDefaultLighting();
            effect.AmbientLightColor = new Vector3(0.4f, 0.4f, 0.4f);
            effect.SpecularColor = new Vector3(0.2f, 0.2f, 0.2f);

            // Setup camera
            float aspectRatio = (float)GraphicsDevice.Viewport.Width / (float)GraphicsDevice.Viewport.Height;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, nearPlaneDistance, farPlaneDistance);
            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraPosition + cameraReference, cameraUpVector);
        }

        /// <summary>
        /// Draw control.
        /// </summary>
        protected override void Draw()
        {
            if (!isReady)
            {
                // Just clear the display until ready
                GraphicsDevice.Clear(Color.Gray);
                return;
            }

            // Draw based on view mode
            switch (viewMode)
            {
                case ViewModes.ModelThumbs:
                    GraphicsDevice.Clear(thumbViewBackgroundColor);
                    DrawThumbnails();
                    break;
                case ViewModes.SingleModel:
                    GraphicsDevice.Clear(thumbViewBackgroundColor);
                    break;
                default:
                    break;
            }

            // Increment frame counter
            frameCount++;
        }

        #endregion

        #region Form Events

        /// <summary>
        /// GraphicsDevice reset.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        private void GraphicsDevice_DeviceReset(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Handles animations.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        void AnimTimer_Tick(object sender, EventArgs e)
        {
            // Update time in seconds
            timeInSeconds = (DateTime.Now.Ticks - startTime) / 10000000;

            // Calculate time delta
            float timeDelta = (float)timeInSeconds / (float)frameCount;

            // Thumbnail scrolling
            if (thumbScrollVelocity != 0)
            {
                float adjustedVelocity = (thumbScrollVelocity * timeDelta) * 20.0f;
                ScrollThumbsView((int)adjustedVelocity);
                LayoutThumbnails();
            }

            // Redraw
            this.Refresh();
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Handles resize events.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // Change projection matrix for new size
            if (IsReady)
            {
                float aspectRatio = (float)Size.Width / (float)Size.Height;
                projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, nearPlaneDistance, farPlaneDistance);
            }

            // Update thumbnail layout
            if (viewMode == ViewModes.ModelThumbs)
                LayoutThumbnails();

            this.Refresh();
        }

        /// <summary>
        /// Tracks mouse movement, delta, and time.
        /// </summary>
        /// <param name="e">MouseEventArgs.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Capture deltas from last position and time
            mousePosDelta = new Point(e.Location.X - mousePos.X, e.Location.Y - mousePos.Y);
            mouseTimeDelta = DateTime.Now.Ticks - mouseTime;

            // Ensure mouse time delta is never 0 (possible, and screws with distance/time calcs obviously)
            if (mouseTimeDelta == 0)
                mouseTimeDelta = 1;

            // Update position and time
            mousePos = new Point(e.Location.X, e.Location.Y);
            mouseTime = DateTime.Now.Ticks;

            // Handle thumbnail scrolling
            if (rightMouseDown && viewMode == ViewModes.ModelThumbs)
            {
                ScrollThumbsView(mousePosDelta.Y);
                LayoutThumbnails();
                this.Refresh();
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (viewMode == ViewModes.ModelThumbs)
            {
                // Clear velocity on wheel
                if (viewMode == ViewModes.ModelThumbs)
                    thumbScrollVelocity = 0.0f;

                int amount = (e.Delta / 120) * 60;
                if (amount < -128) amount = -128;
                if (amount > 128) amount = 128;
                ScrollThumbsView(amount);
                LayoutThumbnails();
                this.Refresh();
            }
        }

        /// <summary>
        /// Tracks mouse buttons down.
        /// </summary>
        /// <param name="e">MouseEventArgs.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // Set focus to this control
            this.Focus();

            // Clear velocity for any mouse down event
            if (viewMode == ViewModes.ModelThumbs)
                thumbScrollVelocity = 0.0f;

            // Store button state
            switch (e.Button)
            {
                case MouseButtons.Left:
                    leftMouseDown = true;
                    break;
                case MouseButtons.Right:
                    rightMouseDown = true;
                    break;
            }
        }

        /// <summary>
        /// Tracks mouse buttons up.
        /// </summary>
        /// <param name="e">MouseEventArgs.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            // Store button state
            switch (e.Button)
            {
                case MouseButtons.Left:
                    leftMouseDown = false;
                    break;
                case MouseButtons.Right:
                    rightMouseDown = false;
                    if (viewMode == ViewModes.ModelThumbs)
                        thumbScrollVelocity = CalcMouseVelocity();
                    break;
            }
        }

        private float CalcMouseVelocity()
        {
            // Calc and cap velocity
            float velocity = ((float)mousePosDelta.Y / (float)mouseTimeDelta) * 100000.0f;
            if (velocity <= -50.0f) velocity = -50.0f;
            if (velocity >= 50.0f) velocity = 50.0f;
            if (velocity >= -2.0f && velocity <= 2.0f) velocity = 0.0f;

            return velocity;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Allows view to set up managers for content loading. Must be called post control creation.
        ///  of control.
        /// </summary>
        /// <param name="arena2Path"></param>
        /// <returns></returns>
        public bool InitialiseView(string arena2Path)
        {
            // Exit if graphics device not set
            if (!this.Created)
                return false;

            // Create managers
            try
            {
                textureManager = new TextureManager(GraphicsDevice, arena2Path);
                modelManager = new ModelManager(GraphicsDevice, arena2Path);
                this.arena2Path = arena2Path;
            }
            catch
            {
                return false;
            }

            // Load the thumbnail background texture
            LoadThumbnailBackgroundTexture();

            // Set ready flag
            isReady = true;

            // Perform initial thumbnail layout
            if (viewMode == ViewModes.ModelThumbs)
                LayoutThumbnails();

            // Start anim timer
            animTimer.Interval = 8;
            animTimer.Enabled = true;
            animTimer.Tick += new EventHandler(AnimTimer_Tick);

            return true;
        }

        #endregion

        #region Drawing Methods

        private void DrawThumbnails()
        {
            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Deferred, SaveStateMode.SaveState);

            foreach (var obj in thumbDict)
                spriteBatch.Draw(obj.Value.texture, obj.Value.rect, Color.White);

            spriteBatch.End();
        }

        private void DrawSingleModel(ref ModelManager.Model model)
        {
            // Set vertex declaration
            GraphicsDevice.VertexDeclaration = vertexDeclaration;

            // Set view and projection matrices
            effect.View = viewMatrix;
            effect.Projection = projectionMatrix;

            foreach (var submesh in model.SubMeshes)
            {
                effect.Texture = textureManager.GetMiscTexture(submesh.TextureKey);

                effect.Begin();
                effect.CurrentTechnique.Passes[0].Begin();

                GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                    model.Vertices, 0, model.Vertices.Length,
                    submesh.Indices, 0, submesh.Indices.Length / 3);

                effect.CurrentTechnique.Passes[0].End();
                effect.End();
            }
        }

        #endregion

        #region Private Methods
        #endregion

        #region Thumbnail View

        private void LoadThumbnailBackgroundTexture()
        {
            // Load thumbnail background texture
            Texture2D sourceTexture = Texture2D.FromFile(
                GraphicsDevice,
                Path.Combine(Application.StartupPath, thumbBackgroundFile));
            if (sourceTexture == null)
                throw new Exception("Could not load thumbnail background image.");

            // Create render target.
            // We render loaded texture onto background color so it's blended properly,
            // then store the blended texture to use as a background for every thumbnail.
            RenderTarget2D renderTarget;
            renderTarget = new RenderTarget2D(GraphicsDevice, sourceTexture.Width, sourceTexture.Height, 1, GraphicsDevice.DisplayMode.Format);
            GraphicsDevice.SetRenderTarget(0, renderTarget);

            // Render source texture over background colour
            GraphicsDevice.Clear(thumbViewBackgroundColor);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            spriteBatch.Draw(sourceTexture, new Vector2(0, 0), Color.White);
            spriteBatch.End();

            // Restore default render target
            GraphicsDevice.SetRenderTarget(0, null);

            // A texture created from a render target is coupled to that render target.
            // It can be easily lost when device is reset (commonly on resize) and must be re-created.
            // This is a Windows Forms application so resizing is going to be very common.
            // Rather than recreate this texture on each resize event, just decouple from render target.
            // This is done by GetData to color array, then SetData into a fresh new texture.
            // The new texture will persist for life of GraphicsDevice and through device resets.
            Texture2D renderTargetTexture = renderTarget.GetTexture();
            Color[] colorArray = new Color[renderTargetTexture.Width * renderTargetTexture.Height];
            renderTargetTexture.GetData<Color>(colorArray);
            thumbBackgroundTexture = new Texture2D(GraphicsDevice, renderTargetTexture.Width, renderTargetTexture.Height);
            thumbBackgroundTexture.SetData<Color>(colorArray);
        }

        /// <summary>
        /// Performs layout of thumbnails.
        /// </summary>
        private void LayoutThumbnails()
        {
            if (!isReady)
                return;

            // Calc dimensions
            thumbWidth = ((this.Width - thumbSpacing * thumbsPerRow) / thumbsPerRow) - (thumbSpacing / thumbsPerRow + 1);
            thumbHeight = thumbWidth;

            // Calc ranges
            int visibleRows = this.Height / (thumbHeight + thumbSpacing) + 3;
            int firstIndex = thumbsFirstVisibleRow * thumbsPerRow;
            int lastIndex = firstIndex + visibleRows * thumbsPerRow - 1;

            // Cap first and last indices
            if (firstIndex < 0)
                firstIndex = 0;
            if (lastIndex >= modelManager.Arch3dFile.Count)
                lastIndex = modelManager.Arch3dFile.Count - 1;

            // Calc screen position of first index
            int xpos = thumbSpacing;
            int ypos = -thumbHeight + thumbScrollAmount;

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
            for (int index = firstIndex; index <= lastIndex; index++)
            {
                // Update or create
                int key = (int)modelManager.Arch3dFile.GetRecordId(index);
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
                    //thumb.texture = thumbBackgroundTexture;
                    //thumb.update = false;
                    thumbDict.Add(thumb.key, thumb);
                }

                // Update position
                xpos += thumbWidth + thumbSpacing;
                if (xpos >= (this.Width - thumbSpacing))
                {
                    ypos += thumbHeight + thumbSpacing;
                    xpos = thumbSpacing;
                }
            }
        }

        /// <summary>
        /// Update thumbnail to represent contained model.
        /// </summary>
        private void UpdateThumbnailTexture(ref Thumbnails thumb)
        {
            if (!isReady)
                return;

            // Get dimensions
            int thumbWidth = thumbBackgroundTexture.Width;
            int thumbHeight = thumbBackgroundTexture.Height;

            // Get model
            if (thumb.model.Vertices == null)
            {
                // Load model
                thumb.model = modelManager.GetModel(thumb.key, false);

                // Load texture for each submesh.
                for (int sm = 0; sm < thumb.model.SubMeshes.Length; sm++)
                {
                    // Load texture
                    thumb.model.SubMeshes[sm].TextureKey =
                        textureManager.LoadMiscTexture(
                        thumb.model.SubMeshes[sm].TextureArchive,
                        thumb.model.SubMeshes[sm].TextureRecord, 0);
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
                thumb.model = modelManager.TransformModel(ref thumb.model, matrix);
            }

            // TODO: Store and set camera position to draw thumb
            //Vector3 thumbCameraPosition = new Vector3(0, 0, 1000);
            //Vector3 thumbCameraReference = new Vector3(0, 0, -1);

            // TEST: Turn off backface culling
            GraphicsDevice.RenderState.CullMode = CullMode.None;

            // Create projection matrix
            float aspectRatio = thumb.rect.Width / thumb.rect.Height;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, nearPlaneDistance, farPlaneDistance);

            // Create texture to use as render target
            RenderTarget2D renderTarget;
            renderTarget = new RenderTarget2D(GraphicsDevice, thumbWidth, thumbHeight, 1, GraphicsDevice.DisplayMode.Format);
            GraphicsDevice.SetRenderTarget(0, renderTarget);

            // Render thumbnail components
            GraphicsDevice.Clear(thumbViewBackgroundColor);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            spriteBatch.Draw(thumbBackgroundTexture, new Rectangle(0, 0, thumbWidth, thumbHeight), Color.White);
            spriteBatch.End();
            DrawSingleModel(ref thumb.model);

            // Restore default render target
            GraphicsDevice.SetRenderTarget(0, null);

            // TODO: Restore camera position

            // Decouple from render target
            Texture2D renderTargetTexture = renderTarget.GetTexture();
            Color[] colorArray = new Color[renderTargetTexture.Width * renderTargetTexture.Height];
            renderTargetTexture.GetData<Color>(colorArray);
            Texture2D newTexture = new Texture2D(GraphicsDevice, renderTargetTexture.Width, renderTargetTexture.Height);
            newTexture.SetData<Color>(colorArray);

            // Store updated values
            thumb.texture = newTexture;
            thumb.update = false;
        }

        /// <summary>
        /// Handle thumbnail view scrolling.
        /// </summary>
        /// <param name="amount">Amount in pixels to scroll view.</param>
        private void ScrollThumbsView(int amount)
        {
            // Apply scroll amount
            thumbScrollAmount += amount;

            // Handle scrolling models up with a new row appearing at the bottom
            if (thumbScrollAmount <= -(thumbHeight + thumbSpacing))
            {
                thumbsFirstVisibleRow++;
                thumbScrollAmount += (thumbHeight + thumbSpacing);
            }

            // Handle scrolling models down with a new row appearing at the top
            if (thumbScrollAmount >= (thumbHeight + thumbSpacing))
            {
                thumbsFirstVisibleRow--;
                thumbScrollAmount -= (thumbHeight + thumbSpacing);
            }
        }

        #endregion

    }

}
