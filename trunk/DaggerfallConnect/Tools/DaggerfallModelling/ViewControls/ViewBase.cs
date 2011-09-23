// Project:         DaggerfallModelling
// Description:     Explore and export 3D models from Daggerfall.
// Copyright:       Copyright (C) 2011 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
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
        private CameraModes cameraMode = CameraModes.None;
        private DFLocation.ClimateBaseType climateType = DFLocation.ClimateBaseType.None;

        #endregion

        #region Class Structures

        public enum CameraModes
        {
            /// <summary>No camera mode change possible.</summary>
            None,
            /// <summary>Top-down camera.</summary>
            TopDown,
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
            set { OnChangeCameraMode(value); }
        }

        /// <summary>
        /// Gets or sets climate type. Each view will maintain a
        /// preferred climate type.
        /// </summary>
        public DFLocation.ClimateBaseType Climate
        {
            get { return climateType; }
            set { climateType = value; }
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

        #region Public Virtual Methods

        /// <summary>
        /// Called when the view is resumed after being inactive.
        ///  Allows view to perform any layout or other requirements before redraw.
        /// </summary>
        public virtual void ResumeView()
        {
            // Set preferred climate for this view
            host.TextureManager.ClimateType = this.climateType;
        }

        /// <summary>
        /// Called when view should reset, release resources, destroy layouts, etc.
        /// </summary>
        public virtual void ResetView()
        {
        }

        /// <summary>
        /// Called when view should track mouse movement.
        /// </summary>
        /// <param name="e">MouseEventArgs.</param>
        public virtual void OnMouseMove(MouseEventArgs e)
        {
        }

        /// <summary>
        /// Called when the mouse wheel is scrolled.
        /// </summary>
        /// <param name="e">MouseEventArgs.</param>
        public virtual void OnMouseWheel(MouseEventArgs e)
        {
        }

        /// <summary>
        /// Called when mouse is clicked.
        /// </summary>
        /// <param name="e">MouseEventArgs.</param>
        public virtual void OnMouseClick(MouseEventArgs e)
        {
        }

        /// <summary>
        /// Called when user double-clicks mouse.
        /// </summary>
        /// <param name="e">MouseEventArgs.</param>
        public virtual void OnMouseDoubleClick(MouseEventArgs e)
        {
        }

        /// <summary>
        /// Called when a mouse button is pressed.
        /// </summary>
        /// <param name="e">MouseEventArgs.</param>
        public virtual void OnMouseDown(MouseEventArgs e)
        {
        }

        /// <summary>
        /// Called when a mouse button is released.
        /// </summary>
        /// <param name="e">MouseEventArgs.</param>
        public virtual void OnMouseUp(MouseEventArgs e)
        {
        }

        /// <summary>
        /// Called when a key is pressed.
        /// </summary>
        /// <param name="e">KeyEventArgs.</param>
        public virtual void OnKeyDown(KeyEventArgs e)
        {
        }

        /// <summary>
        /// The filtered list of models in the host has been modified.
        /// </summary>
        public virtual void FilteredModelsChanged()
        {
        }

        #endregion

        #region Public Abstract Methods

        /// <summary>
        /// Called when view must initialise itself.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Called when view should update animation.
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Called when view should redraw view.
        /// </summary>
        public abstract void Draw();

        /// <summary>
        /// Called when view should resize.
        /// </summary>
        public abstract void Resize();

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// Called to change camera mode.
        /// </summary>
        /// <param name="mode">New camera mode.</param>
        protected virtual void OnChangeCameraMode(CameraModes cameraMode)
        {
            this.cameraMode = cameraMode;
        }

        #endregion

    }

}
