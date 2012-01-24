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

        const string title = "Ruins of Hill Deep Playgrounds";

        // Settings
        ConfigManager ini = new ConfigManager();
        string arena2Path;
        float mouseLookSpeed;
        bool invertMouseVertical;
        bool bloomEnabled;
        bool fxaaEnabled;
        bool windowedMode;

        // XNA
        GraphicsDeviceManager graphics;

        // Deep Engine
        DeepCore core;

        // States
        GameStateManager gameManager;
        TitleScreen titleScreen;
        PlayingGame playingGame;
        ExitMenu gameOptionsMenu;

        // Display
        DisplayMode displayMode;
        DisplayPreferences displayPreference;

        #endregion

        #region Structures

        /// <summary>
        /// Display output preferences.
        /// </summary>
        public enum DisplayPreferences
        {
            Windowed,
            BorderlessWindowed,
            Fullscreen,
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

            // Capture device settings event
            graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(Graphics_PreparingDeviceSettings);

            // Timing
            this.IsFixedTimeStep = true;
            this.Window.Title = title;
            graphics.SynchronizeWithVerticalRetrace = true;

            // Read INI file
            ReadINISettings();

            // Set display mode
            if (windowedMode)
                displayPreference = DisplayPreferences.Windowed;
            else
                displayPreference = DisplayPreferences.Fullscreen;

            // Create engine core
            core = new DeepCore(arena2Path, this.Services);

            // Create game state manager
            gameManager = new GameStateManager(this);
            Components.Add(gameManager);

            // Create game states
            titleScreen = new TitleScreen(core, this);
            playingGame = new PlayingGame(core, this);
            gameOptionsMenu = new ExitMenu(core, this);

            // Set settings
            playingGame.MouseLookSpeed = mouseLookSpeed;

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

            // Assign settings
            core.Input.InvertMouseLook = invertMouseVertical;
            core.Renderer.FXAAEnabled = fxaaEnabled;
            core.Renderer.BloomEnabled = bloomEnabled;

            base.Initialize();

            // Toggle fullscreen
            if (displayPreference == DisplayPreferences.Fullscreen)
                graphics.ToggleFullScreen();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();
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
            // Handle ESC to exit and pop state
            if (core.Input.KeyPressed(Keys.Escape))
            {
                if (gameManager.State == titleScreen)
                    this.Exit();
                else if (gameManager.State == playingGame)
                    gameManager.PushState(gameOptionsMenu);
                else
                    gameManager.PopState();   
            }

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

        #region INI File

        private void ReadINISettings()
        {
            try
            {
                // Open ini file
                string appStartPath = System.Windows.Forms.Application.StartupPath;
                string configName = "config.ini";
                ini.LoadFile(System.IO.Path.Combine(appStartPath, configName));

                arena2Path = ini.GetValue("Daggerfall", "arena2Path");
                mouseLookSpeed = float.Parse(ini.GetValue("Controls", "mouseLookSpeed"));
                invertMouseVertical = bool.Parse(ini.GetValue("Controls", "invertMouseVertical"));
                fxaaEnabled = bool.Parse(ini.GetValue("Renderer", "fxaaEnabled"));
                bloomEnabled = bool.Parse(ini.GetValue("Renderer", "bloomEnabled"));
                windowedMode = bool.Parse(ini.GetValue("Renderer", "windowedMode"));
                string displayResolution = ini.GetValue("Renderer", "displayResolution");

                // Get preferred resolution width and height
                displayResolution.Replace(" ", string.Empty);
                string[] split = displayResolution.Split('x');
                if (split.Length != 2)
                    throw new Exception("Invalid resolution specified.");

                // Get width and height of desired reolution
                int width, height;
                try
                {
                    width = int.Parse(split[0]);
                    height = int.Parse(split[1]);
                }
                catch(Exception e)
                {
                    throw new Exception("Invalid resolution specified. " + e.Message);
                }

                // Validate arena2 path
                DFValidator.ValidationResults results;
                DFValidator.ValidateArena2Folder(arena2Path, out results);
                if (!results.AppearsValid)
                    throw new Exception("The specified Arena2 path is invalid or incomplete.");

                // Get current display mode information
                displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;

                // Attempt to set preferred resolution
                List<DeepCore.DisplayModeDesc> displayModes = DeepCore.EnumerateDisplayModes();
                foreach (var mode in displayModes)
                {
                    if (mode.Mode.Width == width && mode.Mode.Height == height)
                    {
                        displayMode = mode.Mode;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message, "Error Parsing INI File", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                this.Exit();
            }
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
                case DisplayPreferences.Fullscreen:
                    StartFullscreen(e.GraphicsDeviceInformation);
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

        /// <summary>
        /// Starts game in fullscreen at display resolution.
        /// </summary>
        /// <param name="graphicsDeviceInformation"></param>
        private void StartFullscreen(GraphicsDeviceInformation graphicsDeviceInformation)
        {
            // Set borderless window resolution to match display resolution
            graphicsDeviceInformation.PresentationParameters.BackBufferFormat = displayMode.Format;
            graphicsDeviceInformation.PresentationParameters.BackBufferWidth = displayMode.Width;
            graphicsDeviceInformation.PresentationParameters.BackBufferHeight = displayMode.Height;
        }

        #endregion

        #region Title Screen Events

        private void TitleScreen_OnStartClicked(object sender, EventArgs e)
        {
            gameManager.PushState(playingGame);
        }

        private void TitleScreen_OnExitClicked(object sender, EventArgs e)
        {
            this.Exit();
        }

        #endregion

    }

}
