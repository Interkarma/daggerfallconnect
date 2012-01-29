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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using BEPUphysics;
using DeepEngine.Core;
using DeepEngine.Components;
using DeepEngine.Player;
#endregion

namespace DeepEngine.World
{

    /// <summary>
    /// Defines environmental properties of a scene.
    /// </summary>
    public class SceneEnvironment
    {

        #region Fields
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets clear colour.
        /// </summary>
        public Color ClearColor { get; set; }

        /// <summary>
        /// Gets or sets top colour of sky dome gradient.
        /// </summary>
        public Color SkyGradientTop { get; set; }

        /// <summary>
        /// Gets or sets bottom colour of sky dome gradient.
        /// </summary>
        public Color SkyGradientBottom { get; set; }

        /// <summary>
        /// Enables or disables sky dome.
        /// </summary>
        public bool SkyVisible { get; set; }

        /// <summary>
        /// Enables or disables sky dome clouds.
        /// </summary>
        public bool CloudsVisible { get; set; }

        /// <summary>
        /// Gets or sets sky dome cloud color.
        /// </summary>
        public Color CloudColor { get; set; }

        /// <summary>
        /// Gets or sets sky dome cloud brightness.
        /// </summary>
        public float CloudBrightness { get; set; }

        /// <summary>
        /// Gets or sets cloud animation time.
        /// </summary>
        public float CloudTime { get; set; }

        /// <summary>
        /// Enables or disables sky dome stars.
        /// </summary>
        public bool StarsVisible { get; set; }

        /// <summary>
        /// Gets or sets sky dome star color.
        /// </summary>
        public Color StarColor { get; set; }

        /// <summary>
        /// Gets or sets sky dome star brightness.
        /// </summary>
        public float StarBrightness { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public SceneEnvironment()
        {
            // Set defaults
            ClearColor = Color.CornflowerBlue;
            SkyGradientTop = Color.Blue;
            SkyGradientBottom = Color.LightBlue;
            SkyVisible = false;
            CloudsVisible = true;
            CloudColor = Color.White;
            CloudBrightness = 1.0f;
            CloudTime = 0.0f;
            StarsVisible = false;
            StarColor = Color.White;
            StarBrightness = 1.0f;
        }

        #endregion
    }

}
