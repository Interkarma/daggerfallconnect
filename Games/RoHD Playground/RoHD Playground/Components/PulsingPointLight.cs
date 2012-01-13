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

namespace RoHD_Playground.Components
{

    /// <summary>
    /// A point light that changes radius based on a sine and some randomness.
    /// </summary>
    public class PulsingPointLight : LightComponent
    {

        #region Fields
        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core"></param>
        public PulsingPointLight(DeepCore core)
            : base(core, Vector3.Zero, 1f, Color.White, 1f)
        {
        }

        #endregion

    }

}
