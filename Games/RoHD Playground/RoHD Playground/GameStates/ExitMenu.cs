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
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DeepEngine.Core;
using DeepEngine.Components;
using DeepEngine.GameStates;
using DeepEngine.World;
using DeepEngine.Player;
using DeepEngine.UserInterface;
#endregion

namespace RoHD_Playground.GameStates
{

    /// <summary>
    /// Exit menu interface.
    /// </summary>
    public interface IGameOptionsState : IGameState { }

    /// <summary>
    /// Exit menu class.
    /// </summary>
    class ExitMenu : GameState, IGameOptionsState
    {

        #region Fields

        InterfaceManager gui;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="game">Game object.</param>
        public ExitMenu(DeepCore core, Game game)
            : base(core, game)
        {
        }

        #endregion

        #region GameState Overrides

        protected override void LoadContent()
        {
            // Load textures
            Texture2D backgroundTexture = Game.Content.Load<Texture2D>("Textures/solid225-tiling");

            // Load fonts
            SpriteFont menuFont2 = Game.Content.Load<SpriteFont>("Fonts/MenuFont2");

            // Create gui manager
            gui = new InterfaceManager(core);

            // Add a stack panel for menu items
            PanelScreenComponent panel = new PanelScreenComponent(core, Vector2.Zero, new Vector2(400, 200));
            panel.HorizontalAlignment = HorizontalAlignment.Center;
            panel.VerticalAlignment = VerticalAlignment.Middle;
            panel.BackgroundTexture = backgroundTexture;
            panel.BackgroundTextureLayout = TextureLayout.Tile;
            panel.TopMargin = 20;
            panel.LeftMargin = 20;
            panel.RightMargin = 20;
            panel.BottomMargin = 10;

            // Set panel borders
            panel.SetBorderTextures(
                Game.Content.Load<Texture2D>("Textures/top-left"),
                Game.Content.Load<Texture2D>("Textures/top"),
                Game.Content.Load<Texture2D>("Textures/top-right"),
                Game.Content.Load<Texture2D>("Textures/left"),
                Game.Content.Load<Texture2D>("Textures/right"),
                Game.Content.Load<Texture2D>("Textures/bottom-left"),
                Game.Content.Load<Texture2D>("Textures/bottom"),
                Game.Content.Load<Texture2D>("Textures/bottom-right"));
            panel.EnableBorder = true;

            // Add stack panel to gui
            gui.Components.Add(panel);

            // Exit game text
            TextItemScreenComponent exitGameText = new TextItemScreenComponent(core, menuFont2, "Exit Game?");
            exitGameText.HorizontalAlignment = HorizontalAlignment.Center;
            exitGameText.VerticalAlignment = VerticalAlignment.Top;
            exitGameText.EnableOutline = true;
            exitGameText.OutlineColor = Color.Black;
            exitGameText.ShadowColor = Color.Black;
            exitGameText.ShadowVector = new Vector2(3, 3);

            // Continue button
            TextItemScreenComponent continueButton = new TextItemScreenComponent(core, menuFont2, "Continue");
            continueButton.HorizontalAlignment = HorizontalAlignment.Left;
            continueButton.VerticalAlignment = VerticalAlignment.Bottom;
            continueButton.EnableOutline = false;
            continueButton.OutlineColor = Color.Goldenrod;

            // Exit button
            TextItemScreenComponent exitButton = new TextItemScreenComponent(core, menuFont2, "Exit");
            exitButton.HorizontalAlignment = HorizontalAlignment.Right;
            exitButton.VerticalAlignment = VerticalAlignment.Bottom;
            exitButton.EnableOutline = false;
            exitButton.OutlineColor = Color.Goldenrod;

            // Wire up events
            continueButton.OnMouseEnter += new EventHandler(OnMouseEnter);
            continueButton.OnMouseLeave += new EventHandler(OnMouseLeave);
            continueButton.OnMouseClick += new EventHandler(ContinueButton_OnMouseClick);
            exitButton.OnMouseEnter += new EventHandler(OnMouseEnter);
            exitButton.OnMouseLeave += new EventHandler(OnMouseLeave);
            exitButton.OnMouseClick += new EventHandler(ExitButton_OnMouseClick);

            // Add items to stack panel
            panel.Components.Add(exitGameText);
            panel.Components.Add(exitButton);
            panel.Components.Add(continueButton);
        }

        public override void Update(GameTime gameTime)
        {
            // Update gui components
            gui.Update(gameTime.ElapsedGameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            // Draw background from last render
            core.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            core.SpriteBatch.Draw(core.Renderer.RenderTargetTexture, core.Renderer.RenderTargetRectangle, Color.White);
            core.SpriteBatch.End();

            // Draw gui components
            gui.Draw();
        }

        #endregion

        #region State Events

        protected override void StateChanged(object sender, EventArgs e)
        {
            base.StateChanged(sender, e);

            if (Enabled && Visible)
            {
                core.ActiveScene = null;
                Game.IsMouseVisible = true;
            }
        }

        private void OnMouseEnter(object sender, EventArgs e)
        {
            (sender as TextItemScreenComponent).EnableOutline = true;
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            (sender as TextItemScreenComponent).EnableOutline = false;
        }

        private void ContinueButton_OnMouseClick(object sender, EventArgs e)
        {
            gameStateManager.PopState();
        }

        private void ExitButton_OnMouseClick(object sender, EventArgs e)
        {
            Game.Exit();
        }

        #endregion

    }

}
