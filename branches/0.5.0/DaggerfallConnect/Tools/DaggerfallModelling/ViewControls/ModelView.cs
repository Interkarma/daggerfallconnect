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

        string arena2Folder = string.Empty;

        SpriteBatch spriteBatch;
        VertexDeclaration vertexDeclaration;
        BasicEffect effect;

        private float nearPlaneDistance = 1.0f;
        private float farPlaneDistance = 50000.0f;

        private Matrix projectionMatrix;
        private Matrix viewMatrix;

        private Vector3 cameraPosition = new Vector3(1024, 1000, 3000);
        private Vector3 cameraReference = new Vector3(0, 0, -1);
        private Vector3 cameraUpVector = new Vector3(0, 1, 0);
        //private float cameraYaw = 0.0f;
        //private float cameraPitch = 0.0f;

        #endregion

        #region Class Structures

        public enum ViewMode
        {
            SingleModel,
            ModelThumbs,
            Block,
            Map,
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets Arena2 folder.
        /// </summary>
        string Arena2Folder
        {
            get { return arena2Folder; }
            set { SetArena2Folder(value); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ModelView()
        {
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

            // TEST: Create manager
            TextureManager textureManager = new TextureManager(GraphicsDevice, "C:\\dosgames\\DAGGER\\ARENA2");
            ModelManager modelManager = new ModelManager(textureManager);

            // Create vertex declaration
            vertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionNormalTexture.VertexElements);

            // Setup basic effect
            effect = new BasicEffect(GraphicsDevice, null);
            effect.World = Matrix.Identity;
            effect.TextureEnabled = true;
            effect.PreferPerPixelLighting = true;
            effect.EnableDefaultLighting();
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
            GraphicsDevice.Clear(Color.Gray);

            // Set vertex declaration
            GraphicsDevice.VertexDeclaration = vertexDeclaration;

            // Set view and projection matrices
            //effect.View = viewMatrix;
            //effect.Projection = projectionMatrix;

            // Draw the block
            //Matrix matrix = Matrix.Identity;
            //blockManager.DrawBlock(blockIndex, ref effect, ref matrix);

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

        #region Public Methods

        public void SetBlock(string name)
        {
            // Load content
            //int index = blockManager.BlocksFile.GetBlockIndex(name);
            //if (index != -1)
            //{
            //    blockIndex = index;
            //    blockManager.LoadBlock(blockIndex);
            //    this.Invalidate();
            //}
        }

        #endregion

        #region Private Methods

        void SetArena2Folder(string path)
        {
            // Check folder valid
            if (!Directory.Exists(path))
                throw new Exception("Specified ARENA2 path does not exist.");

            arena2Folder = path;
        }

        #endregion

    }

}
