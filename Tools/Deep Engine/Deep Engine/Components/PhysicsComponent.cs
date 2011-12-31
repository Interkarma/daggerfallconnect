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
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using DeepEngine.World;
#endregion

namespace DeepEngine.Components
{

    /// <summary>
    /// Provides integration with physics engine.
    /// </summary>
    public class PhysicsComponent : BaseComponent
    {

        #region Fields

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
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets physics entity. Allows for direct control over properties.
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
        /// <param name="entity">Entity this component is attached to.</param>
        /// <param name="width">Width of box.</param>
        /// <param name="height">Height of box.</param>
        /// <param name="length">Length of box.</param>
        /// <param name="mass">Mass of box.</param>
        public PhysicsComponent(BaseEntity entity, float width, float height, float length, float mass)
            :base(entity)
        {
            // Add physics entity
            physicsEntity = new Box(Vector3.Zero, width, height, length, mass);
            physicsEntity.WorldTransform = entity.Matrix;
            entity.Scene.Space.Add(physicsEntity);
            type = PhysicsPrimitiveType.Box;
        }

        /// <summary>
        /// Kinematic Box constructor.
        /// </summary>
        /// <param name="entity">Entity this component is attached to.</param>
        /// <param name="width">Width of box.</param>
        /// <param name="height">Height of box.</param>
        /// <param name="length">Length of box.</param>
        public PhysicsComponent(BaseEntity entity, float width, float height, float length)
            :base(entity)
        {
            // Add physics entity
            physicsEntity = new Box(Vector3.Zero, width, height, length);
            physicsEntity.WorldTransform = entity.Matrix;
            entity.Scene.Space.Add(physicsEntity);
            type = PhysicsPrimitiveType.Box;
        }

        /// <summary>
        /// Dynamic Sphere constructor.
        /// </summary>
        /// <param name="entity">Entity this component is attached to.</param>
        /// <param name="radius">Radius of sphere.</param>
        /// <param name="mass">Mass of sphere.</param>
        public PhysicsComponent(BaseEntity entity, float radius, float mass)
            : base(entity)
        {
            // Add physics entity
            physicsEntity = new Sphere(Vector3.Zero, radius, mass);
            physicsEntity.WorldTransform = entity.Matrix;
            entity.Scene.Space.Add(physicsEntity);
            type = PhysicsPrimitiveType.Sphere;
        }

        /// <summary>
        /// Kinematic Sphere constructor.
        /// </summary>
        /// <param name="entity">Entity this component is attached to.</param>
        /// <param name="radius">Radius of sphere.</param>
        public PhysicsComponent(BaseEntity entity, float radius)
            : base(entity)
        {
            // Add physics entity
            physicsEntity = new Sphere(Vector3.Zero, radius);
            physicsEntity.WorldTransform = entity.Matrix;
            entity.Scene.Space.Add(physicsEntity);
            type = PhysicsPrimitiveType.Sphere;
        }

        #endregion

        #region BaseComponent Overrides

        /// <summary>
        /// Called when component should update itself.
        /// </summary>
        /// <param name="gameTime">GameTime.</param>
        public override void Update(GameTime gameTime)
        {
            // Adjust scene entity world transform based on physics entity
            entity.Matrix = physicsEntity.WorldTransform;
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
