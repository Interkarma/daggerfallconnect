// Project:         DaggerfallModelling
// Description:     Explore and export 3D models from Daggerfall.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Imports

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
    /// Abstract base class for content views inserted into a ContentViewHost.
    /// </summary>
    public abstract class ViewBase
    {

        #region Class Variables

        protected ViewHost host = null;
        protected CameraModes cameraMode = CameraModes.Normal;

        #endregion

        #region Class Structures

        public enum CameraModes
        {
            /// <summary>No camera mode possible (e.g. content not available).</summary>
            None,
            /// <summary>Normal camera.</summary>
            Normal,
            /// <summary>Free camera.</summary>
            Free,
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets camera mode. Each view will manage the camera
        ///  in a way suited to the content it displays.
        /// </summary>
        public CameraModes CameraMode
        {
            get { return cameraMode; }
            set { cameraMode = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public ViewBase(ViewHost host)
        {
            this.host = host;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Called when view must initialise itself.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Called when view should tick animation.
        /// </summary>
        public abstract void Tick();

        /// <summary>
        /// Called when view should redraw view.
        /// </summary>
        public abstract void Draw();

        /// <summary>
        /// Called when view should resize.
        /// </summary>
        public abstract void Resize();

        /// <summary>
        /// Called when view should track mouse movement.
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

        /// <summary>
        /// Called when mouse enters client area.
        /// </summary>
        /// <param name="e">EventArgs</param>
        public abstract void OnMouseEnter(EventArgs e);

        /// <summary>
        /// Called when mouse leaves client area.
        /// </summary>
        /// <param name="e"></param>
        public abstract void OnMouseLeave(EventArgs e);

        /// <summary>
        /// The filtered list of models in the host has been modified.
        /// </summary>
        public abstract void FilteredModelsChanged();

        /// <summary>
        /// Called when the view is resumed after being inactive.
        ///  Allows view to perform any layout or other requirements before redraw.
        /// </summary>
        public abstract void ResumeView();

        #endregion

    }

}
