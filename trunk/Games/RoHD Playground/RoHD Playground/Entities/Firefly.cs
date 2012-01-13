// Project:         Ruins of Hill Deep - Playground Build
// Description:     Test environment for Ruins of Hill Deep development.
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
using DeepEngine.Primitives;
using DeepEngine.Components;
using DeepEngine.World;
#endregion

namespace RoHD_Playground.Entities
{

    /// <summary>
    /// A firefly that moves along a path before winking out.
    ///  Will randomly repawn and follow a different path based on parameters.
    /// </summary>
    class Firefly : WorldEntity
    {

        #region Fields

        // Components
        GeometricPrimitiveComponent sphere;
        LightComponent light;

        // Lifetime
        bool alive;                    // Firefly is on/off
        long maxTimeToRest = 4000;     // Time to stay off before starting a new path
        long restCounterStart = 0;     // Time rest counter was started
        long timeToRest = 0;           // How long firefly will rest before respawning

        // Animation
        Vector3 startPosition;         // Start position relative to entity
        Vector3 endPosition;           // End position relative to entity
        float speed = 0.003f;          // Movement speed
        float multiplier = 0f;         // Speed multiplier
        float current = 0f;            // Current position

        // Range
        float xzRange = 350;           // Maximum distance firefly can travel in the horizontal dimension
        float yRange = 90;             // Maximum distance firefly can travel in the vertical dimension

        // Appearance
        Color color = Color.YellowGreen;

        // Random
        Random rnd;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets maximum time firefly will rest in the off state before respawning.
        /// </summary>
        public long MaxRestTime
        {
            get { return maxTimeToRest; }
            set { maxTimeToRest = value; }
        }

        /// <summary>
        /// Gets or sets maximum horizontal movement radius.
        /// </summary>
        public float HorizontalRange
        {
            get { return xzRange; }
            set { xzRange = value; }
        }

        /// <summary>
        /// Gets or sets maximum vertical movement radius.
        /// </summary>
        public float VerticalRange
        {
            get { return yRange; }
            set { yRange = value; }
        }

        /// <summary>
        /// Gets or sets firefly colour.
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        /// <summary>
        /// Gets or sets movement speed.
        /// </summary>
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="scene">Scene to attach entity.</param>
        public Firefly(Scene scene)
            : base(scene)
        {
            // Create sphere
            sphere = new GeometricPrimitiveComponent(scene.Core);
            sphere.MakeSphere(1.5f, 4);
            Components.Add(sphere);

            // Create light
            light = new LightComponent(scene.Core, Vector3.Zero, 64f, color, 0.5f);
            Components.Add(light);

            // Start random generator
            rnd = new Random((int)scene.Core.Stopwatch.ElapsedTicks);

            // Random start alive or resting
            if (1 == ((int)(rnd.NextDouble() * 10) & 1))
            {
                alive = true;
            }
            else
            {
                alive = false;
                restCounterStart = scene.Core.Stopwatch.ElapsedMilliseconds;
                timeToRest = (long)(maxTimeToRest * rnd.NextDouble());
            }

            // Start firefly
            StartFirefly();
        }

        #endregion

        #region BaseEntity Overrides

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (alive)
            {
                // Move firefly
                multiplier = current;

                // Update firefly
                UpdateFirefly();

                // Increment current position
                current += speed;
                if (current >= 1f)
                {
                    alive = false;
                    restCounterStart = scene.Core.Stopwatch.ElapsedMilliseconds;
                    timeToRest = (long)(maxTimeToRest * rnd.NextDouble());
                }
            }
            else
            {
                long currentTime = scene.Core.Stopwatch.ElapsedMilliseconds - restCounterStart;
                if (currentTime > timeToRest)
                {
                    alive = true;
                    StartFirefly();
                }
            }
        }

        public override void Draw()
        {
            if (!alive)
                return;

            base.Draw();
        }

        #endregion

        #region Animation

        /// <summary>
        /// Places firefly in start position.
        /// </summary>
        private void StartFirefly()
        {
            // Randomise start position
            startPosition = new Vector3(
                MathHelper.Lerp(-xzRange, xzRange, (float)rnd.NextDouble()),
                MathHelper.Lerp(-yRange, yRange, (float)rnd.NextDouble()),
                MathHelper.Lerp(-xzRange, xzRange, (float)rnd.NextDouble()));

            // Randomise range
            endPosition = startPosition + new Vector3(
                MathHelper.Lerp(-xzRange, xzRange, (float)rnd.NextDouble()),
                MathHelper.Lerp(-yRange, yRange, (float)rnd.NextDouble()),
                MathHelper.Lerp(-xzRange, xzRange, (float)rnd.NextDouble()));

            // Update components
            Matrix matrix = Matrix.CreateTranslation(startPosition);
            sphere.Color = new Vector4(color.ToVector3(), 1f);
            sphere.Matrix = matrix;
            light.Color = color;
            light.Position = startPosition;

            // Make alive
            multiplier = 0;
            current = 0;
        }

        /// <summary>
        /// Updates firefly position.
        /// </summary>
        private void UpdateFirefly()
        {
            if (!alive)
                return;

            // Get current position
            Vector3 position = startPosition + ((endPosition - startPosition) * multiplier);

            // Update components
            Matrix matrix = Matrix.CreateTranslation(position);
            sphere.Matrix = matrix;
            light.Position = position;
        }

        #endregion

    }

}
