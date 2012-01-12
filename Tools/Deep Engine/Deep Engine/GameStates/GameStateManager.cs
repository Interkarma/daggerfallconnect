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
#endregion

namespace DeepEngine.GameStates
{

    /// <summary>
    /// Interface for game state manager.
    /// </summary>
    public interface IGameStateManager
    {
        event EventHandler OnStateChange;
        GameState State { get; }
        void PopState();
        void PushState(GameState state);
        bool ContainsState(GameState state);
        void ChangeState(GameState newState);
    }

    /// <summary>
    /// Game state manager class.
    /// </summary>
    public class GameStateManager : GameComponent, IGameStateManager
    {

        #region Fields

        private Stack<GameState> states = new Stack<GameState>();

        public event EventHandler OnStateChange;

        private int initialDrawOrder = 1000;
        private int drawOrder;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public GameStateManager(Game game)
            : base(game)
        {
            game.Services.AddService(typeof(IGameStateManager), this);
            drawOrder = initialDrawOrder;
        }

        #endregion

        #region IGameStateManager

        /// <summary>
        /// Gets state at top of stack.
        /// </summary>
        public GameState State
        {
            get { return (states.Peek()); }
        }

        /// <summary>
        /// Push a new state onto the stack.
        /// </summary>
        /// <param name="state">New state.</param>
        public void PushState(GameState state)
        {
            // Set draw order
            drawOrder += 100;
            state.DrawOrder = drawOrder;

            // Add state
            AddState(state);

            // Raise event
            if (OnStateChange != null)
                OnStateChange(this, null);
        }

        /// <summary>
        /// Pop a state from the stack.
        /// </summary>
        public void PopState()
        {
            // Remove state
            RemoveState();
            drawOrder -= 100;

            // Raise event
            if (OnStateChange != null)
                OnStateChange(this, null);
        }

        /// <summary>
        /// Checks stack for a specific state.
        /// </summary>
        /// <param name="state">State to look for.</param>
        /// <returns>True if state exists on stack.</returns>
        public bool ContainsState(GameState state)
        {
            return (states.Contains(state));
        }

        /// <summary>
        /// Replace entire stack with a new state.
        /// </summary>
        /// <param name="state">New state.</param>
        public void ChangeState(GameState state)
        {
            // We are changing states so pop everything
            while (states.Count > 0)
                RemoveState();

            // Reset draw order
            state.DrawOrder = drawOrder = initialDrawOrder;
            AddState(state);

            if (OnStateChange != null)
                OnStateChange(this, null);
        }

        #endregion

        #region Private Methods

        private void AddState(GameState state)
        {
            states.Push(state);

            Game.Components.Add(state);

            // Register event for this state
            OnStateChange += state.StateChanged;
        }

        /// <summary>
        /// Removes a state.
        /// </summary>
        private void RemoveState()
        {
            GameState oldState = (GameState)states.Peek();

            // Unregister event for this state
            OnStateChange -= oldState.StateChanged;

            // Remove the state from our game components
            Game.Components.Remove(oldState.Value);

            states.Pop();
        }

        #endregion

    }

}
