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
    /// Manages hierarchy of entities for rendering, lighting, and physical simulation.
    /// </summary>
    public class Scene
    {

        #region Fields

        // Engine
        DeepCore core;

        // Simulation
        Space space;

        // Entity list
        List<BaseEntity> entities;
        List<int> entitiesToDispose;

        // Temporary
        Camera camera;

        #endregion

        #region Properties

        /// <summary>
        /// Gets engine core.
        /// </summary>
        public DeepCore Core
        {
            get { return core; }
        }

        /// <summary>
        /// Gets physics simulation.
        /// </summary>
        public Space Space
        {
            get { return space; }
        }

        /// <summary>
        /// Gets entity list.
        /// </summary>
        public List<BaseEntity> Entities
        {
            get { return entities; }
        }

        /// <summary>
        /// Gets scene camera.
        /// </summary>
        public Camera Camera
        {
            get { return camera; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// DeepCore Constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        public Scene(DeepCore core)
        {
            // Store values
            this.core = core;

            // Create simulation space
            space = new Space();
            space.ForceUpdater.Gravity = new Vector3(0, -90f, 0);

            // Create camera
            camera = new Camera();

            // Create entity lists
            entities = new List<BaseEntity>();
            entitiesToDispose = new List<int>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Called when scene should update itself.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            // Update simulation
            space.Update((float)gameTime.ElapsedGameTime.TotalMilliseconds);

            // Update camera
            float aspectRatio = (float)core.GraphicsDevice.Viewport.Width / (float)core.GraphicsDevice.Viewport.Height;
            camera.SetAspectRatio(aspectRatio);

            // Update entities
            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i].DisposeOnUpdate)
                    entitiesToDispose.Add(i);
                else
                    entities[i].Update(gameTime);
            }

            // Dispose of any flagged entities
            if (entitiesToDispose.Count > 0)
            {
                // Dispose of entity
                foreach (var index in entitiesToDispose)
                {
                    entities[index].Dispose();
                    entities.RemoveAt(index);
                }

                // Reset list
                entitiesToDispose.Clear();
            }
        }

        /// <summary>
        /// Called when scene should draw itself.
        /// </summary>
        public void Draw()
        {
            // Draw entities
            foreach (var entity in entities)
            {
                entity.Draw();
            }
        }

        #endregion

    }

}
