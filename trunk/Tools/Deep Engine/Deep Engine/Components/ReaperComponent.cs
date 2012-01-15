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
    ///  This just calls the entity DisposeOnUpdate(), which still leaves the empty
    ///  entity in the scene.
    /// </summary>
    public class ReaperComponent : BaseComponent
    {
        #region Fields

        // References
        BaseEntity entity = null;

        // Timing
        long endTime = 0;

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
            endTime = core.ElapsedMilliseconds + lifetime;
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
            if (core.ElapsedMilliseconds > endTime && entity != null)
            {
                entity.DisposeOnUpdate = true;
                this.Enabled = false;
            }
        }
        
        #endregion
    }

}
