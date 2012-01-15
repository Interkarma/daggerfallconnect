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
using DeepEngine.Core;
using DeepEngine.Components;
using DeepEngine.Utility;
#endregion

namespace DeepEngine.World
{

    /// <summary>
    /// Provides features common to all world entities.
    /// </summary>
    public abstract class BaseEntity :
        IComparable<BaseEntity>,
        IEquatable<BaseEntity>,
        IDisposable
    {

        #region Fields

        // Property values
        protected Scene scene;
        protected uint id;
        protected string name;
        protected bool enabled;
        protected object tag;
        protected Matrix matrix;
        protected BoundingSphere bounds;
        protected ComponentCollection components;

        // Variables
        protected bool disposeOnUpdate = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets unique ID of entity.
        ///  ID is generated automatically.
        ///  Only use set when loading a scene from file.
        /// </summary>
        public uint ID
        {
            get { return id; }
            set
            {
                id = value;
                if (id > IDCounter)
                    IDCounter = id + 1;
            }
        }

        /// <summary>
        /// Gets scene this entity is attached to.
        /// </summary>
        public Scene Scene
        {
            get { return scene; }
        }

        /// <summary>
        /// Gets or sets name of entity.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Gets or sets enabled flag.
        ///  A disabled entity, including all of its children, will be ignored by engine.
        ///  Default is true.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// Gets or sets custom tag.
        /// </summary>
        public object Tag
        {
            get { return tag; }
            set { tag = value; }
        }

        /// <summary>
        /// Gets or sets transformation matrix.
        /// </summary>
        public Matrix Matrix
        {
            get { return matrix; }
            set { matrix = value; }
        }

        /// <summary>
        /// Gets bounding sphere of this entity for visibility tests.
        ///  Sphere is merged from drawable components.
        ///  Has no purpose if entity does not have any drawable components.
        /// </summary>
        public BoundingSphere BoundingSphere
        {
            get { return bounds; }
        }

        /// <summary>
        /// Gets component collection.
        /// </summary>
        public ComponentCollection Components
        {
            get { return components; }
        }

        /// <summary>
        /// Gets flag stating if entity should be disposed on next update.
        /// </summary>
        public bool DisposeOnUpdate
        {
            set { disposeOnUpdate = value; }
            get { return disposeOnUpdate; }
        }

        #endregion

        #region Static Properties

        /// <summary>
        /// Provides a unique ID for scene entities by
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

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="scene">Scene to attach entity.</param>
        public BaseEntity(Scene scene)
        {
            // Set values
            this.id = NewID;
            this.scene = scene;
            this.enabled = true;
            this.tag = null;
            this.matrix = Matrix.Identity;
            this.bounds.Center = Vector3.Zero;
            this.bounds.Radius = 0f;

            // Create component collection
            components = new ComponentCollection();
            
            // Subscribe to component added event
            components.ComponentAdded += new ComponentCollection.ComponentAddedEventHandler(ComponentAdded);

            // Add to scene
            this.scene.Entities.Add(this);
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Called when entity should update itself.
        /// </summary>
        /// <param name="gameTime">GameTime.</param>
        public abstract void Update(TimeSpan elapsedTime);

        /// <summary>
        /// Called when entity should draw itself.
        /// </summary>
        public abstract void Draw();

        #endregion

        #region Protected Methods

        /// <summary>
        /// Updates entity bounding sphere based on any drawable components attached.
        /// </summary>
        protected void UpdateBoundingSphere()
        {
            // Update all components
            BoundingSphere sphere;
            sphere.Center = Vector3.Zero;
            sphere.Radius = 0f;
            foreach (var component in components)
            {
                // Merge bounds for all drawable components
                if (component is DrawableComponent)
                {
                    sphere = BoundingSphere.CreateMerged(sphere, ((DrawableComponent)component).BoundingSphere);
                }
            }

            // Store updated bounding sphere
            this.bounds = sphere;
        }

        #endregion

        #region IComparable

        /// <summary>
        /// Compare two entities.
        ///  Override if special handling needed to compare entities.
        /// </summary>
        /// <param name="other">BaseEntity.</param>
        /// <returns>Comparison result.</returns>
        public virtual int CompareTo(BaseEntity other)
        {
            return 0;
        }

        #endregion

        #region IEquatable

        /// <summary>
        /// Compares ID of two entities.
        ///  Override to extend equality test.
        /// </summary>
        /// <param name="other">BaseEntity.</param>
        /// <returns>True if ID equal.</returns>
        public virtual bool Equals(BaseEntity other)
        {
            if (this.id == other.id)
                return true;
            else
                return false;
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Called when entity is to be disposed.
        ///  Override if special handling needed
        ///  to dispose of entity resources.
        /// </summary>
        public virtual void Dispose()
        {
            // Dispose each component
            foreach (BaseComponent component in components)
            {
                component.Dispose();
            }

            // Clear collection
            components.Clear();

            // Disable self
            this.enabled = false;
        }

        #endregion

        #region Events

        /// <summary>
        /// Event when component is added.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        protected virtual void ComponentAdded(object sender, ComponentCollection.ComponentAddedEventArgs e)
        {
            // Update entity bounds every time a drawable component is added
            if (e.Component is DrawableComponent)
            {
                UpdateBoundingSphere();
            }
        }

        #endregion
    }

}
