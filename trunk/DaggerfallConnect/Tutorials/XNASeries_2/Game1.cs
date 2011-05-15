// Project:         DaggerfallConnect
// Description:     Read data from Daggerfall's file formats.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using DaggerfallConnect;
using XNALibrary;
#endregion

namespace XNASeries_2
{
    /// <summary>
    /// This tutorial shows how to load a dungeon block scene.
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // XNA
        BasicEffect effect;
        VertexDeclaration vertexDeclaration;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Daggerfall Connect
        string arena2Path = @"C:\dosgames\dagger\arena2";
        TextureManager textureManager;
        ModelManager modelManager;

        // Scene
        //SceneManager sceneManager;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Create vertex declaration
            vertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionNormalTexture.VertexElements);
            GraphicsDevice.VertexDeclaration = vertexDeclaration;

            // Initialise effect and camera
            SetupEffect();
            SetupCamera();

            // Initialise Daggerfall Connect objects
            textureManager = new TextureManager(GraphicsDevice, arena2Path);
            modelManager = new ModelManager(GraphicsDevice, arena2Path);
            //sceneManager = new SceneManager();

            base.Initialize();
        }

        /// <summary>
        /// Setup basic effect.
        /// </summary>
        private void SetupEffect()
        {
            effect = new BasicEffect(GraphicsDevice, null);
            effect.World = Matrix.Identity;
            effect.TextureEnabled = true;
            effect.PreferPerPixelLighting = true;
            effect.EnableDefaultLighting();
            effect.AmbientLightColor = new Vector3(0.4f, 0.4f, 0.4f);
            effect.SpecularColor = new Vector3(0.2f, 0.2f, 0.2f);
        }

        /// <summary>
        /// Setup camera.
        /// </summary>
        private void SetupCamera()
        {
            float aspectRatio = (float)GraphicsDevice.Viewport.Width / (float)GraphicsDevice.Viewport.Height;
            effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1f, 10000f);
            effect.View = Matrix.CreateLookAt(
                new Vector3(0, 350, 1500),
                new Vector3(0, 350, 0),
                Vector3.Up);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create scene
            CreateScene();
        }

        /// <summary>
        /// Loads a Daggerfall model into a new scene.
        /// </summary>
        private void CreateScene()
        {
            /*
            // Load Daggerfall model
            ModelManager.ModelData model = modelManager.GetModelData(456);

            // Load texture for each submesh
            for (int i = 0; i < model.SubMeshes.Length; i++)
            {
                // Load texture.
                // Always set POW2 flag when loading textures for ModelManager.Model.
                int key = textureManager.LoadTexture(
                    model.DFMesh.SubMeshes[i].TextureArchive,
                    model.DFMesh.SubMeshes[i].TextureRecord,
                    TextureManager.TextureCreateFlags.PowerOfTwo);

                // Set key
                model.SubMeshes[i].TextureKey = key;
            }

            // Create scene
            node = new ModelNode(model);
            sceneManager.Scene.Add(node);
            */
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Begin
            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();

            /*
            // Iterate through each submesh
            foreach (var submesh in node.Model.SubMeshes)
            {
                // Set texture for this submesh
                effect.Texture = textureManager.GetTexture(submesh.TextureKey);
                effect.CommitChanges();

                // Draw submesh
                GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    node.Model.Vertices,
                    0,
                    node.Model.Vertices.Length,
                    node.Model.Indices,
                    submesh.StartIndex,
                    submesh.PrimitiveCount);
            }
            */

            // End
            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }
    }
}
