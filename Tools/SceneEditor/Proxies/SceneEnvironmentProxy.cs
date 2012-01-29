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
    /// SceneEnvironment proxy interface.
    /// </summary>
    internal interface ISceneEnvironmentProxy : IEditorProxy { }

    /// <summary>
    /// Encapsulates scene environment properties for the editor.
    /// </summary>
    internal sealed class SceneEnvironmentProxy : BaseEditorProxy, ISceneEnvironmentProxy, IEditorProxy
    {

        #region Fields

        const string defaultName = "Environment";
        const string categoryName = "Environment";

        SceneEnvironment environment;

        #endregion

        #region Editor Properties

        /// <summary>
        /// Gets or sets clear colour.
        /// </summary>
        [Category(categoryName), Description("Colour to clear render target. Ignored when skydome is visible.")]
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
        /// Gets or sets top colour of sky dome gradient.
        /// </summary>
        [Category(categoryName), Description("Top colour of skydome gradient.")]
        public System.Drawing.Color SkyGradientTop
        {
            get { return ColorHelper.FromXNA(environment.SkyGradientTop); }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("SkyGradientTop"));
                environment.SkyGradientTop = ColorHelper.FromWinForms(value);
            }
        }

        /// <summary>
        /// Gets or sets bottom colour of sky dome gradient.
        /// </summary>
        [Category(categoryName), Description("Bottom colour of skydome gradient.")]
        public System.Drawing.Color SkyGradientBottom
        {
            get { return ColorHelper.FromXNA(environment.SkyGradientBottom); }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("SkyGradientBottom"));
                environment.SkyGradientBottom = ColorHelper.FromWinForms(value);
            }
        }

        /// <summary>
        /// Enable or disable sky dome.
        /// </summary>
        [Category(categoryName), Description("Show or hide skydome.")]
        public bool SkyVisible
        {
            get { return environment.SkyVisible; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("SkyVisible"));
                environment.SkyVisible = value;
            }
        }

        /// <summary>
        /// Enable or disable clouds.
        /// </summary>
        [Category(categoryName), Description("Show or hide clouds.")]
        public bool CloudsVisible
        {
            get { return environment.CloudsVisible; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("CloudsVisible"));
                environment.CloudsVisible = value;
            }
        }

        /// <summary>
        /// Gets or sets cloud colour.
        /// </summary>
        [Category(categoryName), Description("Cloud colour.")]
        public System.Drawing.Color CloudColor
        {
            get { return ColorHelper.FromXNA(environment.CloudColor); }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("CloudColor"));
                environment.CloudColor = ColorHelper.FromWinForms(value);
            }
        }

        /// <summary>
        /// Gets or sets brightness of clouds.
        /// </summary>
        [Category(categoryName), Description("Cloud brightness.")]
        public float CloudBrightness
        {
            get { return environment.CloudBrightness; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("CloudBrightness"));
                environment.CloudBrightness = value;
            }
        }

        /// <summary>
        /// Gets or sets cloud animation time.
        /// </summary>
        [Category(categoryName), Description("Cloud animation time.")]
        public float CloudTime
        {
            get { return environment.CloudTime; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("CloudTime"));
                environment.CloudTime = value;
            }
        }

        /// <summary>
        /// Enable or disable stars.
        /// </summary>
        [Category(categoryName), Description("Show or hide stars.")]
        public bool StarsVisible
        {
            get { return environment.StarsVisible; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("StarsVisible"));
                environment.StarsVisible = value;
            }
        }

        /// <summary>
        /// Gets or sets star colour.
        /// </summary>
        [Category(categoryName), Description("Star light intensity.")]
        public System.Drawing.Color StarColor
        {
            get { return ColorHelper.FromXNA(environment.StarColor); }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("StarColor"));
                environment.StarColor = ColorHelper.FromWinForms(value);
            }
        }

        /// <summary>
        /// Gets or sets brightness of stars.
        /// </summary>
        [Category(categoryName), Description("Star brightness.")]
        public float StarBrightness
        {
            get { return environment.StarBrightness; }
            set
            {
                base.SceneDocument.PushUndo(this, this.GetType().GetProperty("StarBrightness"));
                environment.StarBrightness = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="document">Scene document.</param>
        /// <param name="environment">Scene environment to proxy.</param>
        public SceneEnvironmentProxy(SceneDocument document, SceneEnvironment environment)
            : base(document)
        {
            // Get environment from scene document
            base.Name = defaultName;
            this.environment = environment;
        }

        #endregion
    }

}
