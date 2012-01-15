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
using DeepEngine.Utility;
#endregion

namespace DeepEngine.Utility
{
    
    /// <summary>
    /// Wrapper for commonly used material effects.
    ///  Allows basic material properties to be applied in a single call.
    /// </summary>
    public class BaseMaterialEffect :
        IEquatable<BaseMaterialEffect>
    {

        #region Fields

        const string invalidParameterError = "At least one parameter was not found on the effect.";

        protected uint id;
        protected DeepCore core;
        protected long lastFrameTime;

        protected Texture2D currentDiffuseTexture;

        #endregion

        #region Properties

        /// <summary>Effect to draw with.</summary>
        public Effect Effect { get; set; }

        /// <summary>Technique to draw with.</summary>
        public EffectTechnique Technique { get; set; }

        /// <summary>Diffuse texture effect parameters.</summary>
        public EffectParameter DiffuseTextureParam { get; set; }

        /// <summary>Normal texture effect parameters - not implemented.</summary>
        public EffectParameter NormalTextureParam { get; set; }

        /// <summary>Diffuse texture.</summary>
        public Texture2D DiffuseTexture
        {
            get { return GetDiffuseTexture(); }
            set { currentDiffuseTexture = value; }
        }

        /// <summary>Normal texture - not implemented.</summary>
        public Texture2D NormalTexture { get; set; }

        /// <summary>Sampler state 0 to use when drawing.</summary>
        public SamplerState SamplerState0 { get; set; }

        /// <summary>True if texture is animated.</summary>
        public bool IsAnimated { get; internal set; }

        /// <summary>Time between frames in milliseconds.</summary>
        public long Speed { get; internal set; }

        /// <summary>Current animation frame.</summary>
        private int CurrentFrame { get; set; }

        /// <summary>Texture frames for animated textures.</summary>
        internal List<Texture2D> DiffuseTextureFrames { get; set; }

        /// <summary>
        /// Gets unique ID.
        /// </summary>
        public uint ID
        {
            get { return id; }
        }

        #endregion

        #region Static Properties

        /// <summary>
        /// Provides a unique ID for materials by
        ///  incrementing a static value.
        /// </summary>
        private static uint IDCounter = 0;

        /// <summary>
        /// Gets new ID.
        /// </summary>
        public static uint NewID
        {
            get { return IDCounter++; }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Resets ID counter back to start.
        ///  Only do this after purging any stored materials.
        /// </summary>
        public static void ResetID()
        {
            IDCounter = 0;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="effect">Effect to use.</param>
        /// <param name="technique">Technique to use.</param>
        /// <param name="diffuseTextureParam">Diffuse texture parameter name. Can be null.</param>
        /// <param name="normalTextureParam">Normals texture parameter name. Can be null.</param>
        public BaseMaterialEffect(
            DeepCore core,
            Effect effect,
            EffectTechnique technique,
            string diffuseTextureParam,
            string normalTextureParam)
        {
            try
            {
                // Set effect values
                this.id = NewID;
                this.core = core;
                this.Effect = effect;
                this.Technique = technique;

                // Set diffuse texture parameter
                if (string.IsNullOrEmpty(diffuseTextureParam))
                    this.DiffuseTextureParam = null;
                else
                    this.DiffuseTextureParam = effect.Parameters[diffuseTextureParam];

                // Set normal texture parameter
                if (string.IsNullOrEmpty(normalTextureParam))
                    this.NormalTextureParam = null;
                else
                    this.NormalTextureParam = effect.Parameters[normalTextureParam];

                // Set default sampler state
                SamplerState0 = SamplerState.AnisotropicWrap;

                // Setup default animation timer
                lastFrameTime = core.Stopwatch.ElapsedMilliseconds;
                Speed = 90;
            }
            catch (Exception e)
            {
                throw new Exception(invalidParameterError + " | " + e.Message);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Apply effect sampler states, technique and texture parameters.
        /// </summary>
        public void Apply()
        {
            Effect.GraphicsDevice.SamplerStates[0] = SamplerState0;

            // Apply technique
            Effect.CurrentTechnique = Technique;

            // Apply parameters
            if (DiffuseTextureParam != null) DiffuseTextureParam.SetValue(DiffuseTexture);
            //if (NormalTextureParam != null) NormalTextureParam.SetValue(NormalTexture);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets current diffuse texture with animation support.
        /// </summary>
        private Texture2D GetDiffuseTexture()
        {
            // Just return stored value if not animated
            if (!IsAnimated)
                return currentDiffuseTexture;

            // Check for a frame update
            long elapsed = core.Stopwatch.ElapsedMilliseconds;
            if (elapsed - lastFrameTime > Speed)
            {
                CurrentFrame++;
                lastFrameTime = elapsed;
            }

            // Wrap around frame counter
            if (CurrentFrame >= DiffuseTextureFrames.Count)
                CurrentFrame = 0;

            return DiffuseTextureFrames[CurrentFrame];
        }

        #endregion

        #region IEquatable

        /// <summary>
        /// Compares ID of two materials.
        ///  Override to extend equality test.
        /// </summary>
        /// <param name="other">BaseMaterial.</param>
        /// <returns>True if ID equal.</returns>
        public virtual bool Equals(BaseMaterialEffect other)
        {
            if (this.id == other.id)
                return true;
            else
                return false;
        }

        #endregion

    }

}
