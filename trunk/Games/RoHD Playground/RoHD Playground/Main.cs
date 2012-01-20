// Project:         Ruins of Hill Deep - Playground Build
// Description:     Test environment for Ruins of Hill Deep development.
// Copyright:       Copyright (C) 2012 Gavin Clayton
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
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;
using DeepEngine;
using DeepEngine.Core;
using DeepEngine.GameStates;
using DeepEngine.Components;
using DeepEngine.World;
using RoHD_Playground.GameStates;
#endregion

namespace RoHD_Playground
{
    /// <summary>
    /// Game class.
    /// </summary>
    public class Main : Microsoft.Xna.Framework.Game
    {

        #region Fields

        // Set this to your local ARENA2 path
        string arena2Path = @"c:\dosgames\dagger\arena2";           

        // XNA
        GraphicsDeviceManager graphics;

        // Deep Engine
        DeepCore core;

        // States
        GameStateManager gameManager;
        TitleScreen titleScreen;
        PlayingGame playingGame;

        // Display
        DisplayMode displayMode;
        DisplayPreferences displayPreference = DisplayPreferences.BorderlessWindowed;

        #endregion

        #region Structures

        /// <summary>
        /// Display output preferences.
        /// </summary>
        public enum DisplayPreferences
        {
            Windowed,
            BorderlessWindowed,
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public Main()
        {
            // Setup device
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Get display mode information
            displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;

            // Capture device settings event
            graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(Graphics_PreparingDeviceSettings);

            // Timing
            this.IsFixedTimeStep = true;
            graphics.SynchronizeWithVerticalRetrace = true;

            // Create engine core
            core = new DeepCore(arena2Path, this.Services);

            // Create game state manager
            gameManager = new GameStateManager(this);
            Components.Add(gameManager);

            // Create game states
            titleScreen = new TitleScreen(core, this);
            playingGame = new PlayingGame(core, this);

            // Setup title events
            titleScreen.OnStartClicked += new EventHandler(TitleScreen_OnStartClicked);
            titleScreen.OnExitClicked += new EventHandler(TitleScreen_OnExitClicked);

            // Set initial game state
            gameManager.ChangeState(titleScreen);
        }

        #endregion

        #region Game Overrides

        /// <summary>
        /// Initialise game engine.
        /// </summary>
        protected override void Initialize()
        {
            // Initialise core
            core.Initialize();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            // Invert mouse
            core.Input.InvertMouseLook = true;
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
            // Handle ESC to exit
            if (core.Input.KeyboardState.IsKeyDown(Keys.Escape))
                this.Exit();

            // Update core
            core.Update(gameTime.ElapsedGameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        #endregion

        #region Display Setup Methods

        /// <summary>
        /// Called when device is setting up the first time.
        /// </summary>
        private void Graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            // Setup based on display output preference
            switch (displayPreference)
            {
                case DisplayPreferences.Windowed:
                    StartWindowed(e.GraphicsDeviceInformation);
                    break;
                case DisplayPreferences.BorderlessWindowed:
                    StartBorderlessWindowed(e.GraphicsDeviceInformation);
                    break;
            }
        }

        /// <summary>
        /// Start game in a window at render target resolution;
        /// </summary>
        private void StartWindowed(GraphicsDeviceInformation graphicsDeviceInformation)
        {
            // Set back buffer size to match render target size
            graphicsDeviceInformation.PresentationParameters.BackBufferWidth = displayMode.Width;
            graphicsDeviceInformation.PresentationParameters.BackBufferHeight = displayMode.Height;
        }

        /// <summary>
        /// Starts game in a borderless window at display resolution.
        /// </summary>
        private void StartBorderlessWindowed(GraphicsDeviceInformation graphicsDeviceInformation)
        {
            // Set borderless window style
            System.Windows.Forms.Control control = System.Windows.Forms.Form.FromHandle(this.Window.Handle);
            System.Windows.Forms.Form form = control.FindForm();
            form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

            // Set borderless window resolution to match display resolution
            graphicsDeviceInformation.PresentationParameters.BackBufferFormat = displayMode.Format;
            graphicsDeviceInformation.PresentationParameters.BackBufferWidth = displayMode.Width;
            graphicsDeviceInformation.PresentationParameters.BackBufferHeight = displayMode.Height;
        }

        #endregion

        #region Title Screen Events

        private void TitleScreen_OnStartClicked(object sender, EventArgs e)
        {
            gameManager.ChangeState(playingGame);
        }

        private void TitleScreen_OnExitClicked(object sender, EventArgs e)
        {
            this.Exit();
        }

        #endregion

    }

}
