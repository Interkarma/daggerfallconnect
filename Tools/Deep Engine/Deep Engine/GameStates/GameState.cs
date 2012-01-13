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
#endregion

namespace DeepEngine.GameStates
{

    /// <summary>
    /// Interface for a game state.
    /// </summary>
    public interface IGameState
    {
        GameState Value { get; }
    }

    /// <summary>
    /// GameState abstract base class.
    /// </summary>
    public abstract partial class GameState : DrawableGameComponent, IGameState
    {

        #region Fields

        protected DeepCore core;
        protected IGameStateManager gameStateManager;
        
        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="game">Game object.</param>
        public GameState(DeepCore core, Game game)
            : base(game)
        {
            this.core = core;
            this.gameStateManager = (IGameStateManager)game.Services.GetService(typeof(IGameStateManager));
        }

        #endregion

        #region DrawableGameComponent Overrides

        /// <summary>
        /// Loads content for this game state.
        /// </summary>
        protected override void LoadContent()
        {
        }

        #endregion

        #region IGameState Properties

        /// <summary>
        /// Gets game state value.
        /// </summary>
        public GameState Value
        {
            get { return (this); }
        }

        #endregion

        #region Events

        /// <summary>
        /// Called when state changed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">EventArgs.</param>
        internal protected virtual void StateChanged(object sender, EventArgs e)
        {
            if (gameStateManager.State == this.Value)
                Visible = Enabled = true;
            else
                Visible = Enabled = false;
        }

        #endregion

    }

}
