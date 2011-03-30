﻿#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using XNALibrary;

#endregion

namespace DaggerfallModelling.ViewControls
{

    /// <summary>
    /// Abstract base class for modelling views inserted into a ModelViewContainer.
    /// </summary>
    public abstract class ContentViewClient
    {

        #region Class Variables

        protected ContentViewHost host = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public ContentViewClient(ContentViewHost host)
        {
            this.host = host;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Called when client must initialise itself.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Called when client should tick animation.
        /// </summary>
        public abstract void Tick();

        /// <summary>
        /// Called when client should redraw view.
        /// </summary>
        public abstract void Draw();

        /// <summary>
        /// Called when client should resize.
        /// </summary>
        public abstract void Resize();

        /// <summary>
        /// Called when client should track mouse movement.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public abstract void OnMouseMove(MouseEventArgs e);

        /// <summary>
        /// Called when the mouse wheel is scrolled.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public abstract void OnMouseWheel(MouseEventArgs e);

        /// <summary>
        /// Called when a mouse button is pressed.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public abstract void OnMouseDown(MouseEventArgs e);

        /// <summary>
        /// Called when a mouse button is released.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        public abstract void OnMouseUp(MouseEventArgs e);

        #endregion

    }

}
