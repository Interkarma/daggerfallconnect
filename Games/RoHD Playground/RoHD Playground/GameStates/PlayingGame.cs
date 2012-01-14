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
    /// Playing game interface.
    /// </summary>
    public interface IPlayingGameState : IGameState { }

    /// <summary>
    /// Playing game.
    /// </summary>
    class PlayingGame : GameState, IPlayingGameState
    {

        #region Fields

        Scene scene;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="game">Game object.</param>
        public PlayingGame(DeepCore core, Game game)
            : base(core, game)
        {
        }

        #endregion

        #region GameState Overrides


        protected override void LoadContent()
        {
            // Create scene
            scene = new Scene(core);
            scene.DeprecatedCamera.Position = new Vector3(1024, 142, 2874);
            scene.DeprecatedCamera.Update();
            core.ActiveScene = scene;

            // Set clear colour
            core.Renderer.ClearColor = Color.Black;

            // Set day/night mode for window textures
            core.MaterialManager.Daytime = false;

            // Create level entity
            WorldEntity level = new WorldEntity(core.ActiveScene);

            // Create block component
            DaggerfallBlockComponent block = new DaggerfallBlockComponent(core, core.ActiveScene);
            block.LoadBlock("N0000000.RDB", MapsFile.DefaultClimateSettings);
            level.Components.Add(block);

            // Create directional lights
            Color lightColor = Color.White;
            WorldEntity directionalLight = new WorldEntity(core.ActiveScene);
            directionalLight.Components.Add(new LightComponent(core, Vector3.Normalize(Vector3.Down + Vector3.Right), lightColor, 1f));
            directionalLight.Components.Add(new LightComponent(core, Vector3.Normalize(Vector3.Forward + Vector3.Left), lightColor, 1f));
        }

        #endregion

    }

}
