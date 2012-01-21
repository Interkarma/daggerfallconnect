// Project:         Deep Engine
// Description:     3D game engine for Ruins of Hill Deep and Daggerfall Workshop projects.
// Copyright:       Copyright (C) 2012 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using DeepEngine.Core;
using DeepEngine.World;
#endregion

namespace DeepEngine.Components
{

    /// <summary>
    /// A time-bomb component to dispose an entity after a fixed amount of time.
    /// </summary>
    public class ReaperComponent : BaseComponent
    {
        #region Fields

        // References
        BaseEntity entity = null;

        // Timing
        long currentCount;
        long endCount;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="entity">Entity to dispose.</param>
        /// <param name="lifetime">Number of milliseconds before disposing.</param>
        public ReaperComponent(DeepCore core, BaseEntity entity, long lifetime)
            : base(core)
        {
            // Save references
            this.entity = entity;

            // Set timer
            currentCount = 0;
            endCount = lifetime;
        }

        #endregion

        #region BaseComponent Overrides

        /// <summary>
        /// Called when component should update itself.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last update.</param>
        /// <param name="caller">The entity calling the update.</param>
        public override void Update(TimeSpan elapsedTime, BaseEntity caller)
        {
            // Do nothing if disabled
            if (!enabled)
                return;

            // Test and expire
            currentCount += elapsedTime.Milliseconds;
            if (currentCount > endCount && entity != null)
            {
                entity.DisposeOnUpdate = true;
                this.Enabled = false;
            }
        }
        
        #endregion
    }

}
