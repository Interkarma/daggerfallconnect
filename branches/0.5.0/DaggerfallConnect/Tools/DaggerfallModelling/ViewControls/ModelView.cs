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
    /// View single or aggregate models.
    /// </summary>
    public class ModelView : WinFormsGraphicsDevice.GraphicsDeviceControl
    {

        #region Class Variables

        //
        // Resources
        //
        private bool isReady = false;
        private string arena2Path = string.Empty;

        //
        // Managers
        //
        private TextureManager textureManager;
        private ModelManager modelManager;

        //
        // XNA
        //
        private long startTime = 0;
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
        // Model view
        //
        private Color modelViewBackgroundColor = Color.Gray;

        //
        // Mouse
        //
        private Point mousePos;
        private long mouseTime;
        private Point mousePosDelta;
        private long mouseTimeDelta;
        private bool leftMouseDown = false;
        private bool rightMouseDown = false;

        #endregion

        #region Class Structures
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets Arena2 path.
        /// </summary>
        public string Arena2Path
        {
            get { return arena2Path; }
            set { SetArena2Path(value); }
        }

        /// <summary>
        /// Gets ready flag indicating control is operating.
        ///  Must set Arena2 path before control is ready.
        /// </summary>
        public bool IsReady
        {
            get { return isReady; }
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

        public ModelView(string arena2Path)
            : this()
        {
            SetArena2Path(arena2Path);
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

            // Draw thumbnails
            GraphicsDevice.Clear(modelViewBackgroundColor);

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

        #endregion

        #region Overrides

        /// <summary>
        /// Handles resize events.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

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

            // TODO: Handle model rotation
            if (leftMouseDown)
            {
            }

            // TODO: Handle model translation
            if (rightMouseDown)
            {
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
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
                    break;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Allows view to set up managers for content loading. Must be called post control creation.
        ///  of control.
        /// </summary>
        /// <param name="arena2Path"></param>
        /// <returns></returns>
        private void SetArena2Path(string arena2Path)
        {
            // Exit if graphics device not set
            if (!this.Created)
                return;

            // Create managers
            try
            {
                textureManager = new TextureManager(GraphicsDevice, arena2Path);
                modelManager = new ModelManager(GraphicsDevice, arena2Path);
                this.arena2Path = arena2Path;
            }
            catch
            {
                return;
            }

            // Set ready flag
            isReady = true;

            return;
        }

        #endregion

        #region Drawing Methods

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

        private void DrawBlock()
        {
        }

        #endregion

    }

}
