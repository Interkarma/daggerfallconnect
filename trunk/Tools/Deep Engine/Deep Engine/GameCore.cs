// Project:         Deep Engine
// Description:     3D game engine for Ruins of Hill Deep and Daggerfall Workshop projects.
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
using DeepEngine.Core;
using DeepEngine.World;
#endregion

namespace DeepEngine
{

    /// <summary>
    /// Core wrapper for XNA Game-class based projects.
    /// </summary>
    public class GameCore : DrawableGameComponent
    {

        #region Fields

        // Engine
        DeepCore deepCore;

        #endregion

        #region Properties

        /// <summary>
        /// Gets central engine core inside wrapper.
        /// </summary>
        public DeepCore DeepCore
        {
            get { return deepCore; }
        }

        /// <summary>
        /// Gets or sets active scene.
        /// </summary>
        public Scene ActiveScene
        {
            get { return deepCore.ActiveScene; }
            set { deepCore.ActiveScene = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="arena2Folder">Path to Arena2 folder.</param>
        /// <param name="renderTargetSize">Size of internal render target.</param>
        /// <param name="game">Game class.</param>
        public GameCore(string arena2Path, Vector2 renderTargetSize, Game game)
            :base(game)
        {
            // Start core
            deepCore = new DeepCore(arena2Path, renderTargetSize, game.Services);
        }

        #endregion

        #region DrawableGameComponent Overrides

        /// <summary>
        /// Initialise core.
        /// </summary>
        public override void Initialize()
        {
            deepCore.Initialize();
            base.Initialize();
        }

        /// <summary>
        /// Load core content.
        /// </summary>
        protected override void LoadContent()
        {
            deepCore.LoadContent();
            base.LoadContent();
        }

        /// <summary>
        /// Update core.
        /// </summary>
        /// <param name="gameTime">Time since last frame.</param>
        public override void Update(GameTime gameTime)
        {
            deepCore.Update(gameTime);
        }

        /// <summary>
        /// Draw core.
        /// </summary>
        /// <param name="gameTime">Time since last frame.</param>
        public override void Draw(GameTime gameTime)
        {
            deepCore.Draw();
        }

        #endregion

    }

}
