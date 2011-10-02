// Project:         DaggerfallConnect
// Description:     Read data from Daggerfall's file formats.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using XNALibrary;

namespace XNASeries_4
{
    /// <summary>
    /// Currently an action record testbed.
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // XNA
        GraphicsDeviceManager graphics;
        Microsoft.Xna.Framework.Input.KeyboardState oldKeyboardState;

        // XNALibrary
        SceneBuilder sceneBuilder;
        Renderer renderer;
        Input input;

        // Daggerfall path
        string arena2Path = @"c:\dosgames\dagger\arena2";

        // Content
        string blockName = "W0000029.RDB";
        //string blockName = "S0000080.RDB";
        //string blockName = "S0000004.RDB";
        //string blockName = "S0000999.RDB";
        //string blockName = "S0000205.RDB";

        // Camera
        Vector3 cameraPos = new Vector3(900, 1860, -440);

        // Options
        bool invertMouseLook = false;
        bool invertGamePadLook = true;
        bool drawBounds = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Create scene builder
            sceneBuilder = new SceneBuilder(GraphicsDevice, arena2Path);

            // Create renderer
            renderer = new Renderer(sceneBuilder.TextureManager);

            // Enable picking
            renderer.Options |= Renderer.RendererOptions.Picking;

            // Create input
            input = new Input();
            input.ActiveDevices = Input.DeviceFlags.All;
            input.InvertMouseLook = invertMouseLook;
            input.InvertGamePadLook = invertGamePadLook;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a block node
            BlockNode node = sceneBuilder.CreateBlockNode(blockName, null);
            renderer.Camera.Position = cameraPos;

            // Add to scene
            renderer.Scene.AddNode(null, node);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Update scene
            renderer.Scene.Update(gameTime.ElapsedGameTime);

            // Update pointer ray with mouse position
            Point mousePos = input.MousePos;
            renderer.UpdatePointerRay(mousePos.X, mousePos.Y);

            // Update input
            input.Update(gameTime.ElapsedGameTime);
            input.Apply(renderer.Camera, true);

            // Space pressed
            if (
                input.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space) &&
                !oldKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space))
            {
                // Test pointer over node
                if (renderer.PointerOverNode != null)
                {
                    // Test node has action
                    if (renderer.PointerOverNode.Action.Enabled)
                        ToggleAction((ModelNode)renderer.PointerOverNode);
                }
            }

            oldKeyboardState = input.KeyboardState;

            // Update camera
            renderer.Camera.Update();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Draw scene
            renderer.Draw();

            // Draw bounds
            if (drawBounds)
                renderer.DrawBounds();
        }

        /// <summary>
        /// Toggles action between start and end states.
        /// </summary>
        /// <param name="node">SceneNode.</param>
        public void ToggleAction(ModelNode node)
        {
            if (node.Action.ActionState == SceneNode.ActionState.Start)
                node.Action.ActionState = SceneNode.ActionState.RunningForwards;
            else if (node.Action.ActionState == SceneNode.ActionState.End)
                node.Action.ActionState = SceneNode.ActionState.RunningBackwards;
        }
    }
}
