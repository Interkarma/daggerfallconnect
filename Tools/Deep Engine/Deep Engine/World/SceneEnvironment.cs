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
        public Color SkyDomeTopColor { get; set; }

        /// <summary>
        /// Gets or sets bottom colour of sky dome gradient.
        /// </summary>
        public Color SkyDomeBottomColor { get; set; }

        /// <summary>
        /// Enables or disables sky dome.
        /// </summary>
        public bool SkyDomeVisible { get; set; }

        /// <summary>
        /// Enables or disables sky dome clouds.
        /// </summary>
        public bool SkyDomeCloudsVisible { get; set; }

        /// <summary>
        /// Gets or sets sky dome cloud color.
        /// </summary>
        public Color SkyDomeCloudColor { get; set; }

        /// <summary>
        /// Gets or sets sky dome cloud brightness.
        /// </summary>
        public float SkyDomeCloudIntensity { get; set; }

        /// <summary>
        /// Gets or sets cloud animation time.
        /// </summary>
        public float SkyDomeCloudTime { get; set; }

        /// <summary>
        /// Enables or disables sky dome stars.
        /// </summary>
        public bool SkyDomeStarsVisible { get; set; }

        /// <summary>
        /// Gets or sets sky dome star color.
        /// </summary>
        public Color SkyDomeStarColor { get; set; }

        /// <summary>
        /// Gets or sets sky dome star brightness.
        /// </summary>
        public float SkyDomeStarIntensity { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public SceneEnvironment()
        {
            // Set defaults
            ClearColor = Color.CornflowerBlue;
            SkyDomeTopColor = Color.Blue;
            SkyDomeBottomColor = Color.LightBlue;
            SkyDomeVisible = true;
            SkyDomeCloudsVisible = true;
            SkyDomeCloudColor = Color.White;
            SkyDomeCloudIntensity = 1.0f;
            SkyDomeCloudTime = 1.0f;
            SkyDomeStarsVisible = false;
            SkyDomeStarColor = Color.White;
            SkyDomeStarIntensity = 1.0f;
        }

        #endregion
    }

}
