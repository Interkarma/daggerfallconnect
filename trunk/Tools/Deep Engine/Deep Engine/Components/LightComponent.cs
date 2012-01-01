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
using DeepEngine.Core;
using DeepEngine.World;
#endregion

namespace DeepEngine.Components
{

    /// <summary>
    /// Provides direct and point lighting.
    /// </summary>
    public class LightComponent : BaseComponent
    {
        #region Fields

        // General
        LightType type;
        Color color;
        float intensity;

        // Directional light
        Vector3 direction;
        
        // Point light
        Vector3 position;
        float radius;

        #endregion

        #region Structures

        /// <summary>
        /// Defines supported light types.
        /// </summary>
        public enum LightType
        {
            Directional,
            Point,
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets type of light.
        /// </summary>
        public LightType Type
        {
            get { return type; }
        }

        /// <summary>
        /// Gets or sets colour of light.
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        /// <summary>
        /// Gets or sets light intensity.
        /// </summary>
        public float Intensity
        {
            get { return intensity; }
            set { intensity = value; }
        }

        /// <summary>
        /// Gets or sets directional light direction.
        /// </summary>
        public Vector3 Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        /// <summary>
        /// Gets or sets point light position relative to entity.
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// Gets or sets point light radius.
        /// </summary>
        public float Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Directional light constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="direction">Direction of light.</param>
        /// <param name="color">Color of light.</param>
        /// <param name="intensity">Intensity of light.</param>
        public LightComponent(DeepCore core, Vector3 direction, Color color, float intensity)
            : base(core)
        {
            // Set values
            this.direction = direction;
            this.color = color;
            this.intensity = intensity;
            this.type = LightType.Directional;
        }

        /// <summary>
        /// Point light constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="position">Position of light relative to entity.</param>
        /// <param name="radius">Radius of light.</param>
        /// <param name="color">Color of light.</param>
        /// <param name="intensity">Intensity of light.</param>
        public LightComponent(DeepCore core, Vector3 position, float radius, Color color, float intensity)
            : base(core)
        {
            // Set values
            this.position = position;
            this.radius = radius;
            this.color = color;
            this.intensity = intensity;
            this.type = LightType.Point;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Change light into a directional light.
        /// </summary>
        /// <param name="direction">Direction of light.</param>
        /// <param name="color">Color of light.</param>
        /// <param name="intensity">Intensity of light.</param>
        public void BecomeDirectional(Vector3 direction, Color color, float intensity)
        {
            // Set values
            this.direction = direction;
            this.color = color;
            this.intensity = intensity;
            this.type = LightType.Directional;
        }

        /// <summary>
        /// Change light into a point light.
        /// </summary>
        /// <param name="position">Position of light.</param>
        /// <param name="radius">Radius of light.</param>
        /// <param name="color">Color of light.</param>
        /// <param name="intensity">Intensity of light.</param>
        public void BecomePoint(Vector3 position, float radius, Color color, float intensity)
        {
            // Set values
            this.position = position;
            this.radius = radius;
            this.color = color;
            this.intensity = intensity;
            this.type = LightType.Point;
        }

        #endregion
    }

}
