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
using DeepEngine.World;
#endregion

namespace DeepEngine.Components
{

    /// <summary>
    /// A light component providing control over multiple types of light.
    /// </summary>
    public class LightComponent : BaseComponent
    {
        
        #region Structures

        /// <summary>
        /// Defines supported light types.
        /// </summary>
        public enum LightType
        {
            Ambient,
            Directional,
            Point,
            Spot,
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets current light type.
        /// </summary>
        public LightType Type { get; set; }

        /// <summary>
        /// Gets or sets light colour.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Gets or sets light intensity.
        /// </summary>
        public float Intensity { get; set; }

        /// <summary>
        /// Gets or sets light position for positional lights (e.g. point, spot).
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets light directon for directional lights (e.g. directional, light);
        /// </summary>
        public Vector3 Direction { get; set; }

        /// <summary>
        /// Gets or sets start distance of spot lights.
        /// </summary>
        public float SpotNearPlane { get; set; }

        /// <summary>
        /// Gets or sets end distance of spot lights.
        /// </summary>
        public float SpotFarPlane { get; set; }

        /// <summary>
        /// Gets or sets field of view of spot lights.
        /// </summary>
        public float SpotFOV { get; set; }

        /// <summary>
        /// Gets or sets spot light attenuation texture.
        /// </summary>
        public Texture2D SpotAttenuationTexture { get; set; }

        /// <summary>
        /// Gets or sets radius of point lights.
        /// </summary>
        public float PointRadius { get; set; }

        /// <summary>
        /// Gets or sets flag stating if light casts shadows.
        ///  Currently only valid for point and spot lights.
        /// </summary>
        public bool CastsShadows { get; set; }

        /// <summary>
        /// Gets translation matrix of positional lights (e.g. point, spot).
        /// </summary>
        public Matrix TranslationMatrix
        {
            get { return Matrix.CreateTranslation(Position); }
        }

        /// <summary>
        /// Gets bounding sphere of point lights.
        ///  Getting will return transformed bounding sphere based on Matrix.
        /// </summary>
        public BoundingSphere PointBoundingSphere
        {
            get
            {
                BoundingSphere sphere;
                sphere.Center = Vector3.Zero;
                sphere.Radius = PointRadius;
                return sphere.Transform(TranslationMatrix);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        public LightComponent(DeepCore core)
            : base(core)
        {
            MakeAmbient(Color.White, 1.0f);
        }

        /// <summary>
        /// Ambient light constructor
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="color">Color of light.</param>
        /// <param name="intensity">Intensity of light.</param>
        public LightComponent(DeepCore core, Color color, float intensity)
            : base(core)
        {
            MakeAmbient(color, intensity);
        }

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
            MakeDirectional(direction, color, intensity);
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
            MakePoint(position, radius, color, intensity);
        }

        /// <summary>
        /// Spot light constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="position">Position of light relative to entity.</param>
        /// <param name="direction">Direction spot light is facing.</param>
        /// <param name="nearPlane">Distance at which light begins.</param>
        /// <param name="farPlane">Distance at which light ends.</param>
        /// <param name="fov">Field of view</param>
        /// <param name="color">Color of light.</param>
        /// <param name="intensity">Intensity of light.</param>
        public LightComponent(DeepCore core, Vector3 position, Vector3 direction, float nearPlane, float farPlane, float fov, Color color, float intensity)
            : base(core)
        {
            MakeSpot(position, direction, nearPlane, farPlane, fov, color, intensity);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Change light into an ambient light.
        /// </summary>
        /// <param name="color">Color of light.</param>
        /// <param name="intensity">Intensity of light.</param>
        public void MakeAmbient(Color color, float intensity)
        {
            // Set values
            this.Color = color;
            this.Intensity = intensity;
            this.Type = LightType.Ambient;
        }

        /// <summary>
        /// Change light into a directional light.
        /// </summary>
        /// <param name="direction">Direction of light.</param>
        /// <param name="color">Color of light.</param>
        /// <param name="intensity">Intensity of light.</param>
        public void MakeDirectional(Vector3 direction, Color color, float intensity)
        {
            // Set values
            this.Direction = direction;
            this.Color = color;
            this.Intensity = intensity;
            this.Type = LightType.Directional;
        }

        /// <summary>
        /// Change light into a point light.
        /// </summary>
        /// <param name="position">Position of light.</param>
        /// <param name="radius">Radius of light.</param>
        /// <param name="color">Color of light.</param>
        /// <param name="intensity">Intensity of light.</param>
        public void MakePoint(Vector3 position, float radius, Color color, float intensity)
        {
            // Set values
            this.Position = position;
            this.PointRadius = radius;
            this.Color = color;
            this.Intensity = intensity;
            this.Type = LightType.Point;
        }

        /// <summary>
        /// Change light to a spot light.
        /// </summary>
        /// <param name="position">Position of light relative to entity.</param>
        /// <param name="direction">Direction spot light is facing.</param>
        /// <param name="nearPlane">Distance at which light begins.</param>
        /// <param name="farPlane">Distance at which light ends.</param>
        /// <param name="fov">Field of view</param>
        /// <param name="color">Color of light.</param>
        /// <param name="intensity">Intensity of light.</param>
        public void MakeSpot(Vector3 position, Vector3 direction, float nearPlane, float farPlane, float fov, Color color, float intensity)
        {
            this.Position = position;
            this.Direction = direction;
            this.SpotNearPlane = nearPlane;
            this.SpotFarPlane = farPlane;
            this.SpotFOV = fov;
            this.Color = color;
            this.Intensity = intensity;
            this.Type = LightType.Spot;
        }

        #endregion

    }

}
