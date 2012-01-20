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
using Microsoft.Xna.Framework.Graphics;
using DeepEngine.Core;
#endregion

namespace DeepEngine.UserInterface
{

    /// <summary>
    /// Manages a heirarchy of screen components to create a user interface.
    ///  The manager itself is a special panel component covering the entire viewport.
    /// </summary>
    public class InterfaceManager : PanelScreenComponent
    {

        #region Fields

        RasterizerState rasterizerState;

        #endregion

        #region Properties
        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        public InterfaceManager(DeepCore core)
            : base(core, Vector2.Zero, Vector2.Zero)
        {
            // Overlay title safe area of viewport
            Viewport vp = core.GraphicsDevice.Viewport;
            base.position.X = vp.TitleSafeArea.X;
            base.position.Y = vp.TitleSafeArea.Y;
            base.size.X = vp.TitleSafeArea.Width;
            base.size.Y = vp.TitleSafeArea.Height;

            // Create rasterizer state
            rasterizerState = new RasterizerState
            {
                ScissorTestEnable = true,
                CullMode = CullMode.None,
            };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Called when screen component should update itself.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last update.</param>
        public override void Update(TimeSpan elapsedTime)
        {
            // Update child components
            foreach (BaseScreenComponent component in Components)
            {
                component.Update(elapsedTime);
            }
        }

        /// <summary>
        /// Called when screen component should draw itself.
        /// </summary>
        public void Draw()
        {
            // The interface manager clips against the title safe area for controls
            // but we still want the background to cover the entire viewport.
            Rectangle vprect;
            vprect.X = core.GraphicsDevice.Viewport.X;
            vprect.Y = core.GraphicsDevice.Viewport.Y;
            vprect.Width = core.GraphicsDevice.Viewport.Width;
            vprect.Height = core.GraphicsDevice.Viewport.Height;

            // Begin drawing
            core.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, rasterizerState);

            // Draw background
            if (BackgroundColor != Color.Transparent)
            {
                core.GraphicsDevice.ScissorRectangle = vprect;
                core.SpriteBatch.Draw(backgroundTexture, vprect, Color.White);
            }

            // Scissor to title safe area
            core.GraphicsDevice.ScissorRectangle = Rectangle;

            // Draw child components
            foreach (BaseScreenComponent component in Components)
            {
                component.Draw(core.SpriteBatch);
            }

            // End drawing
            core.SpriteBatch.End();

            // Disable scissoring
            core.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }

        #endregion
    }

}
