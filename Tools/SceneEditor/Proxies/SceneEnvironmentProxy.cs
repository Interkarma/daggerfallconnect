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
using System.IO;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DeepEngine.Core;
using DeepEngine.Components;
using DeepEngine.World;
using DeepEngine.Utility;
using SceneEditor.Documents;
#endregion

namespace SceneEditor.Proxies
{

    /// <summary>
    /// Environment proxy interface.
    /// </summary>
    internal interface IEnvironmentProxy : IEditorProxy { }

    /// <summary>
    /// Encapsulates scene environment for the editor.
    /// </summary>
    internal sealed class SceneEnvironmentProxy : BaseEditorProxy, IEnvironmentProxy, IEditorProxy
    {

        #region Fields

        SceneEnvironment environment;

        #endregion

        #region Editor Properties

        /// <summary>
        /// Gets or sets clear colour.
        /// </summary>
        [Category("Render Target"), DisplayName("Color"), Description("Colour to clear render target.")]
        public System.Drawing.Color ClearColor
        {
            get { return ColorHelper.FromXNA(environment.ClearColor); }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("ClearColor"));
                environment.ClearColor = ColorHelper.FromWinForms(value);
            }
        }

        /// <summary>
        /// Enable or disable sky dome.
        /// </summary>
        [Category("Sky Dome"), DisplayName("Visible"), Description("Show sky dome.")]
        public bool SkyDomeVisible
        {
            get { return environment.SkyDomeVisible; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("SkyDomeVisible"));
                environment.SkyDomeVisible = value;
            }
        }

        /// <summary>
        /// Gets or sets top colour of sky dome gradient.
        /// </summary>
        [Category("Sky Dome"), DisplayName("TopColor"), Description("Top colour of gradient.")]
        public System.Drawing.Color SkyDomeTopColor
        {
            get { return ColorHelper.FromXNA(environment.SkyDomeTopColor); }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("SkyDomeTopColor"));
                environment.SkyDomeTopColor = ColorHelper.FromWinForms(value);
            }
        }

        /// <summary>
        /// Gets or sets bottom colour of sky dome gradient.
        /// </summary>
        [Category("Sky Dome"), DisplayName("BottomColor"), Description("Bottom colour of gradient.")]
        public System.Drawing.Color SkyDomeBottomColor
        {
            get { return ColorHelper.FromXNA(environment.SkyDomeBottomColor); }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("SkyDomeBottomColor"));
                environment.SkyDomeBottomColor = ColorHelper.FromWinForms(value);
            }
        }

        /// <summary>
        /// Enable or disable clouds.
        /// </summary>
        [Category("Clouds"), DisplayName("Visible"), Description("Show clouds.")]
        public bool SkyDomeCloudsVisible
        {
            get { return environment.SkyDomeCloudsVisible; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("SkyDomeCloudsVisible"));
                environment.SkyDomeCloudsVisible = value;
            }
        }

        /// <summary>
        /// Enable or disable stars.
        /// </summary>
        [Category("Stars"), DisplayName("Visible"), Description("Show stars.")]
        public bool SkyDomeStarsVisible
        {
            get { return environment.SkyDomeStarsVisible; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("SkyDomeStarsVisible"));
                environment.SkyDomeStarsVisible = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sceneDocument">Scene document.</param>
        public SceneEnvironmentProxy(SceneDocument sceneDocument)
            : base(sceneDocument)
        {
            // Get environment from scene document
            this.environment = base.SceneDocument.EditorScene.Environment;
        }

        #endregion
    }

}
