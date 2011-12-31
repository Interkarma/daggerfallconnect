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
        Core core;

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
            // Create engine
            core = new Core(arena2Path, this.Services);

            // Set camera position
            core.Camera.Position = cameraPos;

            // Set input preferences
            core.Input.InvertMouseLook = invertMouseLook;
            core.Input.InvertGamePadLook = invertGamePadLook;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a block node
            BlockNode node = core.SceneBuilder.CreateBlockNode(blockName, null, false);

            // Add to scene
            core.Scene.AddNode(null, node);
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
            core.Scene.Update(gameTime.ElapsedGameTime);

            // Update pointer ray with mouse position
            Point mousePos = core.Input.MousePos;
            core.Renderer.UpdatePointerRay(mousePos.X, mousePos.Y);

            // Update input
            core.Input.Update(gameTime.ElapsedGameTime);
            core.Input.Apply(core.Camera, true);

            // Space pressed
            if (
                core.Input.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space) &&
                !oldKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space))
            {
                // Test pointer over node
                if (core.Renderer.PointerOverNode != null)
                {
                    // Test node has action
                    if (core.Renderer.PointerOverNode.Action.Enabled)
                        ToggleAction((ModelNode)core.Renderer.PointerOverNode);
                }
            }

            oldKeyboardState = core.Input.KeyboardState;

            // Update camera
            core.Camera.Update();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Draw scene
            core.Draw();

            // Draw bounds
            if (drawBounds)
                core.Renderer.DrawBounds();
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
