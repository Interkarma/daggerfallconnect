﻿// Project:         Deep Engine
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
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using DeepEngine.Core;
using DeepEngine.World;
#endregion

namespace DeepEngine.Components
{

    /// <summary>
    /// Provides simplistic integration with physics engine.
    /// </summary>
    public class PhysicsColliderComponent : BaseComponent
    {

        #region Fields

        Scene scene;
        Entity physicsEntity;
        PhysicsPrimitiveType type;

        #endregion

        #region Structures

        /// <summary>
        /// Defines supported physics primtives.
        /// </summary>
        public enum PhysicsPrimitiveType
        {
            Box,
            Sphere,
            Geometry,
            ConvexHull,
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets physics entity for direct control over properties.
        ///  Must reference BEPUphysics.dll to use.
        /// </summary>
        public BEPUphysics.Entities.Entity PhysicsEntity
        {
            get { return physicsEntity; }
        }

        /// <summary>
        /// Gets type of physics primtive.
        /// </summary>
        public PhysicsPrimitiveType Type
        {
            get { return type; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Dynamic Box constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="scene">Scene to add physics properties.</param>
        /// <param name="matrix">Starting transform.</param>
        /// <param name="width">Width of box.</param>
        /// <param name="height">Height of box.</param>
        /// <param name="length">Length of box.</param>
        /// <param name="mass">Mass of box.</param>
        public PhysicsColliderComponent(DeepCore core, Scene scene, Matrix matrix, float width, float height, float length, float mass)
            : base(core)
        {
            // Store references
            this.scene = scene;

            // Add physics entity
            physicsEntity = new Box(Vector3.Zero, width, height, length, mass);
            physicsEntity.WorldTransform = matrix;
            scene.Space.Add(physicsEntity);
            type = PhysicsPrimitiveType.Box;
        }

        /// <summary>
        /// Kinematic Box constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="scene">Scene to add physics properties.</param>
        /// <param name="matrix">Starting transform.</param>
        /// <param name="width">Width of box.</param>
        /// <param name="height">Height of box.</param>
        /// <param name="length">Length of box.</param>
        public PhysicsColliderComponent(DeepCore core, Scene scene, Matrix matrix, float width, float height, float length)
            : base(core)
        {
            // Store references
            this.scene = scene;

            // Add physics entity
            physicsEntity = new Box(Vector3.Zero, width, height, length);
            physicsEntity.WorldTransform = matrix;
            scene.Space.Add(physicsEntity);
            type = PhysicsPrimitiveType.Box;
        }

        /// <summary>
        /// Dynamic Sphere constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="scene">Scene to add physics properties.</param>
        /// <param name="matrix">Starting transform.</param>
        /// <param name="radius">Radius of sphere.</param>
        /// <param name="mass">Mass of sphere.</param>
        public PhysicsColliderComponent(DeepCore core, Scene scene, Matrix matrix, float radius, float mass)
            : base(core)
        {
            // Store references
            this.scene = scene;

            // Add physics entity
            physicsEntity = new Sphere(Vector3.Zero, radius, mass);
            physicsEntity.WorldTransform = matrix;
            scene.Space.Add(physicsEntity);
            type = PhysicsPrimitiveType.Sphere;
        }

        /// <summary>
        /// Kinematic Sphere constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="scene">Scene to add physics properties.</param>
        /// <param name="matrix">Starting transform.</param>
        /// <param name="radius">Radius of sphere.</param>
        public PhysicsColliderComponent(DeepCore core, Scene scene, Matrix matrix, float radius)
            : base(core)
        {
            // Store references
            this.scene = scene;

            // Add physics entity
            physicsEntity = new Sphere(Vector3.Zero, radius);
            physicsEntity.WorldTransform = matrix;
            scene.Space.Add(physicsEntity);
            type = PhysicsPrimitiveType.Sphere;
        }

        /// <summary>
        /// Dynamic convex hull constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="scene">Scene to add physics properties.</param>
        /// <param name="matrix">Starting transform.</param>
        /// <param name="points">Points defining convex hull.</param>
        /// <param name="mass">Mass of object.</param>
        public PhysicsColliderComponent(DeepCore core, Scene scene, Matrix matrix, List<Vector3> points, float mass)
            : base(core)
        {
            // Store references
            this.scene = scene;

            // Add physics entity
            physicsEntity = new ConvexHull(points, mass);
            physicsEntity.WorldTransform = matrix;
            scene.Space.Add(physicsEntity);
            type = PhysicsPrimitiveType.ConvexHull;
        }

        /// <summary>
        /// Kinematic convex hull constructor.
        /// </summary>
        /// <param name="core">Engine core.</param>
        /// <param name="scene">Scene to add physics properties.</param>
        /// <param name="matrix">Starting transform.</param>
        /// <param name="points">Points defining convex hull.</param>
        public PhysicsColliderComponent(DeepCore core, Scene scene, Matrix matrix, List<Vector3> points)
            : base(core)
        {
            // Store references
            this.scene = scene;

            // Add physics entity
            physicsEntity = new ConvexHull(points);
            physicsEntity.WorldTransform = matrix;
            scene.Space.Add(physicsEntity);
            type = PhysicsPrimitiveType.ConvexHull;
        }

        #endregion

        #region BaseComponent Overrides

        /// <summary>
        /// Called when component should update itself.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time since last update.</param>
        /// <param name="caller">Entity calling the draw operation.</param>
        public override void Update(TimeSpan elapsedTime, BaseEntity caller)
        {
            // Do nothing if disabled
            if (!enabled)
                return;

            // Adjust dynamic entity world transform based on physics entity
            if (caller is DynamicEntity)
            {
                (caller as DynamicEntity).Matrix = physicsEntity.WorldTransform;
            }
        }

        /// <summary>
        /// Frees resources used by this object when they are no longer needed.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            // Clear physics entity from space
            scene.Space.Remove(physicsEntity);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Changes to a dynamic physics entity.
        /// </summary>
        /// <param name="mass">Mass of entity.</param>
        public void BecomeDynamic(float mass)
        {
            physicsEntity.BecomeDynamic(mass);
        }

        /// <summary>
        /// Changes to a kinematic physics entity.
        /// </summary>
        public void BecomeKinematic()
        {
            physicsEntity.BecomeKinematic();
        }

        #endregion

    }

}
