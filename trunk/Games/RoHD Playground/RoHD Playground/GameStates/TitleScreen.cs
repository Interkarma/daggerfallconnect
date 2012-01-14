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
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DeepEngine.Core;
using DeepEngine.Components;
using DeepEngine.GameStates;
using DeepEngine.World;
#endregion

namespace RoHD_Playground.GameStates
{

    /// <summary>
    /// Title screen interface.
    /// </summary>
    public interface ITitleScreenState : IGameState { }

    /// <summary>
    /// Title screen class.
    /// </summary>
    public sealed class TitleScreen : GameState, ITitleScreenState
    {

        #region Fields

        const string titleText = "Ruins of Hill Deep";
        const string versionText = "Playground Build 1.0";

        Scene scene;
        Song song;

        SpriteFont titleFont;
        SpriteFont consoleFont;

        Rectangle titleSafeArea;
        Vector2 titlePos;
        Vector2 versionPos;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="game">Game object.</param>
        public TitleScreen(DeepCore core, Game game)
            : base(core, game)
        {
        }

        #endregion

        #region GameState Overrides

        protected override void LoadContent()
        {
            // Create scene
            scene = new Scene(core);
            scene.Camera.Position = new Vector3(1058, 142, 2874);
            scene.Camera.Transform(327, 0, Vector3.Zero);
            scene.Camera.Update();
            core.ActiveScene = scene;

            // Set clear colour
            core.Renderer.ClearColor = Color.Black;

            // Set day/night mode for window textures
            core.MaterialManager.Daytime = false;

            // Set climate
            DFLocation.ClimateSettings climateSettings = MapsFile.DefaultClimateSettings;
            climateSettings.ClimateType = DFLocation.ClimateBaseType.Swamp;
            climateSettings.SceneryArchive = 510;

            // Create level entity
            WorldEntity level = new WorldEntity(core.ActiveScene);

            // Create block component
            DaggerfallBlockComponent block = new DaggerfallBlockComponent(core, core.ActiveScene);
            block.LoadBlock("CASTAA26.RMB", climateSettings);
            //block.LoadBlock("CASTAA05.RMB", climateSettings);
            level.Components.Add(block);

            // Attach block flats
            AddBlockFlats(level, block);

            // Create directional lights
            Color lightColor = new Color(100, 100, 200);
            WorldEntity directionalLight = new WorldEntity(core.ActiveScene);
            directionalLight.Components.Add(new LightComponent(core, Vector3.Normalize(Vector3.Down + Vector3.Right), lightColor, 0.60f));
            directionalLight.Components.Add(new LightComponent(core, Vector3.Normalize(Vector3.Forward + Vector3.Left), lightColor, 0.90f));

            // Create fireflies in courtyard
            for (int i = 0; i < 20; i++)
            {
                Entities.Firefly firefly = new Entities.Firefly(scene);
                firefly.Matrix = Matrix.CreateTranslation(2000, 100, 1400);
            }

            // Create fireflies in left of courtyard
            for (int i = 0; i < 10; i++)
            {
                Entities.Firefly firefly = new Entities.Firefly(scene);
                firefly.Matrix = Matrix.CreateTranslation(1000, 100, 1400);
            }

            // Create fireflies near camera
            for (int i = 0; i < 10; i++)
            {
                Entities.Firefly firefly = new Entities.Firefly(scene);
                firefly.Matrix = Matrix.CreateTranslation(1200, 100, 2400);
            }

            // Load songs
            song = Game.Content.Load<Song>("Songs/DanGoodale_DF-11");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(song);

            // Load fonts
            titleFont = Game.Content.Load<SpriteFont>("Fonts/TitleFont");
            consoleFont = Game.Content.Load<SpriteFont>("Fonts/ConsoleFont");

            // Title area
            titleSafeArea = Game.GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 titleSize = titleFont.MeasureString(titleText);
            Vector2 versionSize = consoleFont.MeasureString(versionText);
            titlePos = new Vector2(titleSafeArea.Right - titleSize.X - 20, titleSafeArea.Top + 20);
            versionPos = new Vector2(titlePos.X + titleSize.X - versionSize.X, titlePos.Y + titleSize.Y);

            base.Game.IsMouseVisible = true;

        }

        protected override void UnloadContent()
        {
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(GameTime gameTime)
        {
            core.Draw();

            // Draw title and version
            scene.Core.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            scene.Core.SpriteBatch.DrawString(titleFont, titleText, titlePos, Color.AliceBlue);
            scene.Core.SpriteBatch.DrawString(consoleFont, versionText, versionPos, Color.Gold);
            scene.Core.SpriteBatch.End();
        }

        #endregion

        #region Scene Building

        /// <summary>
        /// Attach flats component to an entity.
        /// </summary>
        /// <param name="entity">Entity to attach billboards.</param>
        /// <param name="block">Block to get flats from.</param>
        private void AddBlockFlats(BaseEntity entity, DaggerfallBlockComponent block)
        {
            // Exit if no flats
            if (block.BlockFlats.Count == 0)
                return;

            // Add flats to component
            foreach (var flat in block.BlockFlats)
            {
                // Get position
                Vector3 position = new Vector3(flat.Position.X, flat.Position.Y, flat.Position.Z);

                // Add billboard component
                DaggerfallBillboardComponent billboard = new DaggerfallBillboardComponent(core, flat);
                billboard.Matrix = block.Matrix * Matrix.CreateTranslation(position);
                entity.Components.Add(billboard);

                // Add a light commponent for each billboard light source
                if (flat.Archive == 210)
                {
                    position.Y += billboard.Size.Y;
                    LightComponent lightComponent = new LightComponent(core, block.Matrix.Translation + position, 750f, Color.White, 1.1f);
                    entity.Components.Add(lightComponent);
                }
            }
        }

        #endregion

    }

}
