// Project:         XNALibrary
// Description:     Simple XNA game library for DaggerfallConnect.
// Copyright:       Copyright (C) 2011 Gavin Clayton
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
using Microsoft.Xna.Framework.Input;
#endregion

namespace DeepEngine.Deprecated
{

    /// <summary>
    /// Helper class to apply gravity to a camera.
    /// </summary>
    public class Gravity
    {
        #region Class Variables

        // Starting values
        private Vector3 gravityVector = new Vector3(0f, -25f, 0f);
        private Vector3 jumpVector = new Vector3(0f, 500f, 0f);
        private bool jump = false;

        // Velocity
        private Vector3 velocity = Vector3.Zero;
        private bool airborne = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets gravity vector.
        /// </summary>
        public Vector3 GravityVector
        {
            get { return gravityVector; }
            set { gravityVector = value; }
        }

        /// <summary>
        /// Gets or sets jump vector.
        /// </summary>
        public Vector3 JumpVector
        {
            get { return jumpVector; }
            set { jumpVector = value; }
        }

        /// <summary>
        /// Gets airborne flag.
        /// </summary>
        public bool Airborne
        {
            get { return airborne; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Called when gravity should update.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last frame.</param>
        /// /// <param name="camera">Camera.</param>
        /// <param name="collision">Collision.</param>
        public void Update(TimeSpan elapsedTime, Camera camera, Collision collision)
        {
            // Calculate time delta
            float timeDelta = (float)elapsedTime.TotalSeconds;

            // Apply jump
            if (jump)
            {
                velocity += jumpVector;
            }

            // Apply gravity to velocity
            velocity += gravityVector;

            // Apply velocity to camera
            Vector3 position = camera.Position;
            position.X += velocity.X * timeDelta;
            position.Y += velocity.Y * timeDelta;
            position.Z += velocity.Z * timeDelta;
            camera.Position = position;

            // Handle airborne and grounded cases
            if (!jump)
            {
                // Determine if airborne
                if (collision.DistanceToGround <= camera.EyeHeight)
                {
                    airborne = false;
                    velocity = Vector3.Zero;
                }
                else
                {
                    airborne = true;
                }
            }
            else
            {
                airborne = true;
                jump = false;
            }
        }

        /// <summary>
        /// Initiates a jump.
        ///  Does nothing if airborne.
        /// </summary>
        public void Jump()
        {
            if (!airborne)
                jump = true;
        }

        #endregion

    }

}
