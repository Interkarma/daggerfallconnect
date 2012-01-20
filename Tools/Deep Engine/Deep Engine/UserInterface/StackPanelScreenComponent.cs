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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using DeepEngine.Core;
using DeepEngine.Utility;
#endregion

namespace DeepEngine.UserInterface
{

    /// <summary>
    /// A panel that automatically stacks components vertically.
    /// </summary>
    public class StackPanelScreenComponent : PanelScreenComponent
    {

        #region Properties

        /// <summary>
        /// Gets or sets spacing between each component in stack.
        /// </summary>
        float Spacing { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Enine core.</param>
        /// <param name="position">Position of component relative to parent panel.</param>
        /// <param name="size">Size of component.</param>
        /// <param name="spacing">Spacing between items in stack.</param>
        public StackPanelScreenComponent(DeepCore core, Vector2 position, Vector2 size, float spacing)
            : base(core, position, size)
        {
            // Subscribe to component added event
            Components.ComponentAdded += new ScreenComponentCollection.ComponentAddedEventHandler(Components_ComponentAdded);

            // Set spacing
            this.Spacing = spacing;
        }
        
        #endregion

        #region Events

        /// <summary>
        /// Called when component added.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">EventArgs.</param>
        private void Components_ComponentAdded(object sender, EventArgs e)
        {
            UpdateStackPositions();
        }

        #endregion

        #region Private Methods

        private void UpdateStackPositions()
        {
            // Update stack positions
            float ypos = TopMargin;
            foreach (BaseScreenComponent component in Components)
            {
                // Update general position based on settings
                component.UpdatePosition();

                // Set vertical position
                Vector2 position = component.Position;
                position.Y = ypos;
                component.Position = position;

                // Set next vertical position
                ypos += component.Size.Y + Spacing;
            }
        }

        #endregion

    }

}
